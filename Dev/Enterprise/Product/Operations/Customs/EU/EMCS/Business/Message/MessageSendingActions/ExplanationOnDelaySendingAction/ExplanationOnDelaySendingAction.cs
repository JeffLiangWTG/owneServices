using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class ExplanationOnDelaySendingAction : EMCSMessageSendingAction, IExplanationOnDelay, IObsoleteValidation
	{
		public ExplanationOnDelaySendingAction(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		[ResourceStringData("9F8BFB95-DC8B-464C-845D-5694929097A5", Caption = "Explanation Code")]
		[List(nameof(Lookups) + "." + nameof(ExplanationOnDelaySendingActionLookups.ExplanationCodeList))]
		[MaxLength(1)]
		public ZString ExplanationCode
		{
			get => explanationCode;
			set
			{
				SetNonPersistentPropertyValue(ExplanationCodeInfo, ref explanationCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateExplanationCode();
				}
			}
		}
		ZString explanationCode;

		public ZPropertyInfo ExplanationCodeInfo => GetZPropertyInfo(nameof(ExplanationCode));

		[ResourceStringData("F5DBA7E3-B849-4B75-9612-85B32D5501A0", Caption = "Information")]
		[MaxLength(350)]
		public ZString Information
		{
			get => information;
			set
			{
				SetNonPersistentPropertyValue(InformationInfo, ref information, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateInformation();
				}
			}
		}
		ZString information;

		public ZPropertyInfo InformationInfo => GetZPropertyInfo(nameof(Information));

		[ResourceStringData("00917BD0-215E-410D-94AE-67359F5CA49B", Caption = "Message Role")]
		[List(nameof(Lookups) + "." + nameof(ExplanationOnDelaySendingActionLookups.MessageRoleList))]
		[MaxLength(1)]
		public ZString MessageRole
		{
			get => messageRole;
			set
			{
				SetNonPersistentPropertyValue(MessageRoleInfo, ref messageRole, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageRole();
				}
			}
		}
		ZString messageRole;

		public ZPropertyInfo MessageRoleInfo => GetZPropertyInfo(nameof(MessageRole));

		#region Lookups

		public ExplanationOnDelaySendingActionLookups Lookups => lookups ?? (lookups = new ExplanationOnDelaySendingActionLookups(this));
		ExplanationOnDelaySendingActionLookups lookups;

		#endregion

		#region Validation

		public ExplanationOnDelaySendingActionValidation Validation => new ExplanationOnDelaySendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("A1DEC9CD-0CF1-4264-A395-7359443D5795", "Explanation On Delay");
	}
}
