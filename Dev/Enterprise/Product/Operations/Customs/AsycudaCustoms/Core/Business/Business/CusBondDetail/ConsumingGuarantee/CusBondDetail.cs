using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusBondDetail : CommonCusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("2570be90-f84b-41e4-9002-3a7389dba875|PW_BondType", Caption = "Type")]
		[ReadOnlyMember(nameof(IsLinked))]
		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondTypeInfo, value, (oldValue, newValue) => base.PW_BondType = newValue);
		}

		public ZBool IsContinuous => PW_BondType == GuaranteeBondTypeList.Codes.Continuous;

		[ResourceStringData("69e5d2a5-7bd8-400c-be75-4746f77794e3|PW_CPH_Guarantee", Caption = "Guarantee")]
		[ReadOnlyMember(nameof(IsLinked))]
		public override ZGuid PW_CPH_Guarantee
		{
			get => base.PW_CPH_Guarantee;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_CPH_GuaranteeInfo, value, (oldValue, newValue) =>
			{
				base.PW_CPH_Guarantee = newValue;
				if (!IsCopying && oldValue != PW_CPH_Guarantee)
				{
					UpdateReleaseGuaranteesWithCusGuarantee();
					Instruction?.ReleaseGuarantees.RefreshBinding();
				}
			});
		}

		void UpdateReleaseGuaranteesWithCusGuarantee()
		{
			if (!PW_CPH_Guarantee.IsEmpty && Instruction is CusEntryInstruction instruction)
			{
				foreach (SecondCusBondDetail releaseGuarantee in instruction.ReleaseGuarantees)
				{
					releaseGuarantee.PW_CPH_Guarantee = PW_CPH_Guarantee;
				}
			}
		}

		[ResourceStringData("17863396-454a-4fe1-a132-4eff0cf0c3b8|PW_Status", Caption = "Status")]
		public override ZString PW_Status
		{
			get => base.PW_Status;
			set
			{
				var oldValue = PW_Status;
				base.PW_Status = value;
				if (oldValue != PW_Status && !IsCopying)
				{
					LinkButtonLabelInfo.RefreshBinding();
				}
			}
		}

		[ReadOnlyMember(nameof(HasReleaseGuarantees))]
		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondAmountInfo, value, (oldValue, newValue) =>
			{
				base.PW_BondAmount = newValue;
				if (!IsCopying && oldValue != PW_BondAmount)
				{
					ClearReleaseGuaranteesIfNeeded();
					if (Instruction is CusEntryInstruction instruction)
					{
						instruction.ReleaseGuarantees.RefreshBinding();
						instruction.MarkNeedAddGuaranteeTransactions();
					}
					RemainingInfo.RefreshBinding();
				}
			});
		}

		[ReadOnlyMember(nameof(IsLinked))]
		public override ZString PW_BondNumber2
		{
			get => base.PW_BondNumber2;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondNumber2Info, value, (oldValue, newValue) =>
			{
				base.PW_BondNumber2 = newValue;
				if (!IsCopying && oldValue != PW_BondNumber2)
				{
					ClearReleaseGuaranteesIfNeeded();
					Instruction?.ReleaseGuarantees.RefreshBinding();
				}
			});
		}

		[ReadOnlyMember(nameof(IsLinked))]
		public override ZDateTime PW_BondEffectiveDate
		{
			get => base.PW_BondEffectiveDate;
			set => SetValueIfCanAcquireGuaranteeManagementMutexLock(PW_BondEffectiveDateInfo, value, (oldValue, newValue) =>
			{
				base.PW_BondEffectiveDate = newValue;
				if (!IsCopying && oldValue != PW_BondEffectiveDate)
				{
					ClearReleaseGuaranteesIfNeeded();
					Instruction?.ReleaseGuarantees.RefreshBinding();
				}
			});
		}

		[ResourceStringData("baddebd7-e794-4dec-883a-e54d2d216ea1", Caption = "Currency")]
		public ZString CustomsCurrency => Factory.GetValue(ref customsCurrencyCached, () => Instruction?.InvoiceLines.FirstOrDefault()?.JI_RX_NKLinePriceCurr ?? ZString.Empty);
		CachedProperty<ZString> customsCurrencyCached;

		public ZPropertyInfo CustomsCurrencyInfo => GetZPropertyInfo(nameof(CustomsCurrency));

		public bool CanAddReleaseGuarantees => Factory.GetValue(ref canAddReleaseGuarantees, () => !PW_BondAmount.IsEmpty && !PW_BondNumber2.IsEmpty && !PW_BondEffectiveDate.IsEmpty);
		CachedProperty<bool> canAddReleaseGuarantees;

		void ClearReleaseGuaranteesIfNeeded()
		{
			if (!CanAddReleaseGuarantees)
			{
				Instruction?.ReleaseGuarantees.DeleteAll();
			}
		}

		[ResourceStringData("baddebd7-e794-4dec-883a-e54d2d216ea1|Remaining", Caption = "Remaining")]
		public ZDecimal Remaining => Factory.GetValue(ref remainingCached, () => PW_BondAmount - (Instruction?.ReleaseGuarantees.Sum(x => x.PW_BondAmount) ?? ZDecimal.Zero));
		CachedProperty<ZDecimal> remainingCached;

		public ZPropertyInfo RemainingInfo => GetZPropertyInfo(nameof(Remaining));

		public ZString LinkButtonLabel => IsLinked
			? Res.GetString("edbf8dd8-3771-4ca1-a25c-e21ff514ced6", "Unlink")
			: Res.GetString("2c7122fc-805b-49dc-83d4-69619e369bf6", "Link");

		public ZPropertyInfo LinkButtonLabelInfo => GetZPropertyInfo(nameof(LinkButtonLabel));

		public bool IsLinkButtonEnabled => Factory.GetValue(ref isLinkButtonEnabled
			, () => IsContinuous
					&& !PW_CPH_Guarantee.IsEmpty
					&& !PW_BondNumber2.IsEmpty
					&& !PW_BondEffectiveDate.IsEmpty
					&& (!PW_BondAmount.IsEmpty || IsLinked)
					&& !HasReleaseGuarantees);
		CachedProperty<bool> isLinkButtonEnabled;

		public bool HasReleaseGuarantees => Instruction?.ReleaseGuarantees.Any() ?? false;

		public void LinkOrUnlink()
		{
			if (IsLinkButtonEnabled)
			{
				if (IsLinked)
				{
					Unlink();
				}
				else
				{
					Link();
				}
			}
		}

		public event EventHandler<CancelEventArgs> OnLinkOrUnlinkAskingSaveJobFirst;

		public static CusBondDetail LoadOrCreate(CusEntryInstruction parent) => Load(parent) ?? Create(parent);

		public static CusBondDetail Load(CusEntryInstruction parent)
		{
			CusBondDetail result = null;
			if (parent != null)
			{
				var filter = new ZQuery(CusBondDetailSchema.PW_ParentID, parent.PK)
				{
					FetchOnlyFromLocalCache = !parent.IsInDatabase,
					OrderBy = CusBondDetailSchema.Constants.PK + OrderByClause.Ascending
				};
				filter.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
				result = parent.Factory.LoadTop1<CusBondDetail>(filter);
			}
			return result;
		}

		public static CusBondDetail Create(BusinessObject parent)
		{
			CusBondDetail result = null;
			if (parent != null)
			{
				result = parent.Factory.New<CusBondDetail>();
				using (result.SuspendSettingHasChanges())
				{
					result.Parent = parent;
				}
			}
			return result;
		}

		public new CusBondDetailValidation Validation => (CusBondDetailValidation)base.Validation;

		protected override ZString HumanReadableNameCore => Res.GetString("5c9cb38a-7a22-4b3c-9727-00bad8029310"
			, "Guarantee ({0} '{1}', {2} '{3}')"
			, PW_BondNumber2Info.HumanReadableName
			, PW_BondNumber2
			, PW_BondEffectiveDateInfo.HumanReadableName
			, PW_BondEffectiveDate.ToBestReadableDateTimeString());

		protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation() => new CusBondDetailValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_ActivityCode = GuaranteeActivityCodeList.Codes.ConsumesGuarantee;
		}

		void Link()
		{
			if (IsLinkButtonEnabled && !IsLinked && NotifyUserIfUnableToLink() && AskUserToSaveJob() && CusGuarantee is CusGuaranteeHeader cusGuarantee)
			{
				cusGuarantee.DoActionWithMutexLock(() =>
				{
					SharedCusPermitLineTransaction transaction = null;
					try
					{
						transaction = AddTransaction();
						var newStatus = transaction == null ? GuaranteeStatusList.Codes.NotLinked : GuaranteeStatusList.Codes.Linked;
						PW_Status = newStatus;
						Factory.Save();
					}
					catch (ZSaveException exception)
					{
						PW_Status = GuaranteeStatusList.Codes.NotLinked;
						transaction?.Delete();
						ZExceptionReporting.HandleSaveException(exception);
					}
				}, (message) => Instruction?.WarnConcurrency(message));
			}
		}

		void Unlink()
		{
			if (IsLinkButtonEnabled && IsLinked && AskUserToSaveJob() && CusGuarantee is CusGuaranteeHeader cusGuarantee)
			{
				cusGuarantee.DoActionWithMutexLock(() =>
				{
					SharedCusPermitLineTransaction transaction = null;
					try
					{
						transaction = AddReversingTransaction();
						var newStatus = transaction == null ? GuaranteeStatusList.Codes.Linked : GuaranteeStatusList.Codes.NotLinked;
						PW_Status = newStatus;
						Factory.Save();
					}
					catch (ZSaveException ex)
					{
						PW_Status = GuaranteeStatusList.Codes.Linked;
						transaction?.Delete();
						ZExceptionReporting.HandleSaveException(ex);
					}
				}, (message) => Instruction?.WarnConcurrency(message));
			}
		}

		bool NotifyUserIfUnableToLink()
		{
			var message = new ZStringBuilder();
			if (!IsLinked)
			{
				if (CusGuarantee == null)
				{
					message.AppendLine(Res.GetString("0967e392-39cd-4d94-9e4e-6c5d9597a8ad", "A valid guarantee should be entered."));
				}

				if (PW_BondNumber2.IsEmpty)
				{
					message.AppendLine(Res.GetString("84d887d7-d3c4-4e52-92e8-27242a47aa18", "Reference Number should be entered."));
				}

				if (PW_BondEffectiveDate.IsEmpty)
				{
					message.AppendLine(Res.GetString("87090d80-c3da-49a2-8aac-e24bdf5fc136", "Issue Date should be entered."));
				}

				var bondAcquittedDate = Instruction?.EntryHeader?.CH_BondAcquittedDate ?? ZDate.Empty;
				if (!bondAcquittedDate.IsEmpty)
				{
					message.AppendLine(Res.GetString("f88993a5-feea-4cc9-9b96-8503824178fe", "Entry Acquitted Date should be empty."));
				}

				if (!message.IsEmpty)
				{
					Instruction?.Warn(message.ToString(), Res.GetString("34281908-7d9f-46bc-95ca-faee3d81fc70", "Unable to {0}", LinkButtonLabel));
				}
			}
			return message.IsEmpty;
		}

		bool AskUserToSaveJob()
		{
			var cancel = new CancelEventArgs(false);
			OnLinkOrUnlinkAskingSaveJobFirst?.Invoke(null, cancel);
			return !cancel.Cancel && !IsDeleted;
		}

		internal SharedCusPermitLineTransaction AddTransaction()
		{
			return AddTransaction(PW_BondAmount, true, AddNotification);
		}

		void AddNotification(ZString message, ZDecimal value)
		{
			PW_BondAmountInfo.AddWarningWithoutValidationCheck(message);
			Instruction?.Warn(message);
		}

		internal SharedCusPermitLineTransaction AddDifferenceTransaction()
		{
			var diff = (ZDecimal)PW_BondAmountInfo.Value - (ZDecimal)PW_BondAmountInfo.OriginalValue;
			return AddTransaction(diff, diff > 0, ThrowBurstException);
		}

		SharedCusPermitLineTransaction AddReversingTransaction()
		{
			return AddTransaction(PW_BondAmount, false, null, true);
		}
	}
}
