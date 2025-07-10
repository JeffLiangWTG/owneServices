using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.GUI
{
	public class DpsMarkJobClearConfirmationModel : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DpsMarkJobClearConfirmationModel()
		{
			using (SuspendSettingHasChanges())
			{
				if (JobClearingReasonList.Count > 0)
				{
					Code = JobClearingReasonList[0].Code;
					SetClearedReason();
				}
			}
		}

		ZString reason = DefaultReason;

		[MaxLength(200)]
		public ZString Reason
		{
			get => reason;
			set
			{
				if (reason != value)
				{
					SetNonPersistentPropertyValue(ReasonInfo, ref reason, value);
					RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					ValidateReason();
				}
			}
		}
		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		public bool Reason_ReadOnly
		{
			get
			{
				var result = false;

				if (!RequireReasonForJobCLR)
				{
					result = true;
				}
				else if (RequireReasonForJobCLRItemForCurrentCode != null)
				{
					result = !RequireReasonForJobCLRItemForCurrentCode.IsMandatory;
				}
				else if (Code != OtherCode)
				{
					result = true;
				}

				return result;
			}
		}

		ZString code;

		[List("JobClearingReasonList")]
		[MaxLength(3)]
		public ZString Code
		{
			get => code;
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref code, value.Trim());
				RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}

				SetClearedReason();
			}
		}

		internal bool Code_ReadOnly => !RequireReasonForJobCLR;

		void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			if (!JobClearingReasonList.ContainsCode(Code))
			{
				CodeInfo.AddError(Res.GetString("1E34D8F0-3BD0-46EF-9D2F-5CE8723E8384", "The Code is invalid."));
			}
		}

		void SetClearedReason()
		{
			if (!RequireReasonForJobCLR)
			{
				Reason = DefaultReason;
			}
			else if (RequireReasonForJobCLRItemForCurrentCode != null)
			{
				if (RequireReasonForJobCLRItemForCurrentCode.IsMandatory)
				{
					Reason = string.Empty;
				}
				else
				{
					Reason = RequireReasonForJobCLRItemForCurrentCode.ClearingReason.IsEmpty ? DefaultReason : RequireReasonForJobCLRItemForCurrentCode.ClearingReason.ToString();
				}
			}
			else if (code == OtherCode)
			{
				Reason = string.Empty;
			}
		}

		RequireReasonForCLRItem RequireReasonForJobCLRItemForCurrentCode => RegistryValue.ItemCollection.Cast<RequireReasonForCLRItem>().FirstOrDefault(i => i.Code == Code);

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#region Clearing Reason List

		CodeDescriptionPairList jobClearingReasonList;
		public CodeDescriptionPairList JobClearingReasonList
		{
			get
			{
				if (jobClearingReasonList == null)
				{
					jobClearingReasonList = new CodeDescriptionPairList();

					if (RequireReasonForJobCLR)
					{
						var itemCollection = RegistryValue.ItemCollection;

						foreach (RequireReasonForCLRItem item in itemCollection)
						{
							jobClearingReasonList.AddPair(item.Code, item.Title);
						}

						if (Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed)
						{
							jobClearingReasonList.AddPair(OtherCode, Constants.RequireReasonForCLRRegistryConstants.Description.Other);
						}
					}
				}

				return jobClearingReasonList;
			}
		}

		#endregion

		protected virtual RequireReasonForCLRWrapper RegistryValue => OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.Value;

		bool RequireReasonForJobCLR => RegistryValue.RequireReasonForCLR;

		static string DefaultReason => StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString();

		const string OtherCode = Constants.RequireReasonForCLRRegistryConstants.Code.Other;

		public ZString JobID { get; set; }

		public IEnumerable<string> RelatedJobsIDNotJCLOrCLR { get; set; }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReason();
		}

		void ValidateReason()
		{
			ReasonInfo.ClearAllNotifications();

			if (string.IsNullOrEmpty(Reason) && RequireReasonForJobCLR)
			{
				ReasonInfo.AddError(Res.GetString("5D0CF2C6-176E-41DC-AA71-96C95E305118", "Your organization requires you to enter a reason for clearing this record. Your reason will be recorded for audit purposes."));
			}
			else if (!string.IsNullOrEmpty(Reason))
			{
				var clearedReasonSplitByWhiteSpace = Reason.Split(System.Array.Empty<char>());
				var amountOfCharacters = ZString.Join(string.Empty, clearedReasonSplitByWhiteSpace).Length;
				var amountOfWords = clearedReasonSplitByWhiteSpace.Length;

				if (amountOfCharacters < 6 || amountOfWords < 2)
				{
					ReasonInfo.AddError(Res.GetString("EE6BF733-598F-484B-A576-69A136494A6A", "Clearing reason must be minimum two words and six characters long."));
				}

				if (Reason.Contains('|') || Reason.Contains('='))
				{
					ReasonInfo.AddError(Res.GetString("F9C5ED79-5734-49BA-8DDB-AE3F73941DAE", "Clearing reason should not contain characters of '|' or '='."));
				}
			}
		}
	}
}
