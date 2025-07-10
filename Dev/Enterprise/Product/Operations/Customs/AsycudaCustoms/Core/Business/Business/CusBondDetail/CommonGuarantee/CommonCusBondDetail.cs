using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[SingleObjectAroundARow]
	public abstract class CommonCusBondDetail : MasterFiles.Business.CusBondDetail
	{
		protected CommonCusBondDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly CusBondDetailTypeDecider TypeDecider = new CusBondDetailTypeDecider();

		[List(nameof(Lookups) + "." + nameof(CommonCusBondDetailLookups.BondTypeList))]
		public override ZString PW_BondType
		{
			get => base.PW_BondType;
			set => base.PW_BondType = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonCusBondDetailLookups.GuaranteeCollection))]
		public override ZGuid PW_CPH_Guarantee
		{
			get => base.PW_CPH_Guarantee;
			set => base.PW_CPH_Guarantee = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonCusBondDetailLookups.ActivityCodeList))]
		public override ZString PW_ActivityCode
		{
			get => base.PW_ActivityCode;
			set => base.PW_ActivityCode = value;
		}

		[List(nameof(Lookups) + "." + nameof(CommonCusBondDetailLookups.StatusList))]
		[ReadOnly(true)]
		public override ZString PW_Status
		{
			get => base.PW_Status;
			set => base.PW_Status = value;
		}

		public override ZGuid PW_ParentID
		{
			get => base.PW_ParentID;
			set
			{
				var oldValue = PW_ParentID;
				base.PW_ParentID = value;
				if (oldValue != PW_ParentID)
				{
					entryInstruction = null;
				}
			}
		}

		[ResourceStringData("8fa2c8bf-718c-4968-8f93-7eea58796372|PW_BondNumber2", Caption = "Reference Number")]
		public override ZString PW_BondNumber2
		{
			get => base.PW_BondNumber2;
			set => base.PW_BondNumber2 = value;
		}

		[ResourceStringData("df9ace2e-1395-44a1-9f58-b8cde3c42b63|PW_BondEffectiveDate", Caption = "Issue Date")]
		public override ZDateTime PW_BondEffectiveDate
		{
			get => base.PW_BondEffectiveDate;
			set => base.PW_BondEffectiveDate = value;
		}

		[ResourceStringData("d72c072a-add3-4c5f-83b9-c8820935e071|PW_BondAmount", Caption = "Amount")]
		[DecimalPlaces(2)]
		public override ZDecimal PW_BondAmount
		{
			get => base.PW_BondAmount;
			set => base.PW_BondAmount = value;
		}

		public ZBool IsLinked => PW_Status == GuaranteeStatusList.Codes.Linked;

		public CusEntryInstruction Instruction
		{
			get
			{
				if (entryInstruction == null || entryInstruction.IsDeleted)
				{
					entryInstruction = Factory.Load<CusEntryInstruction>(PW_ParentID);
				}
				return entryInstruction;
			}
		}
		CusEntryInstruction entryInstruction;

		public CusGuaranteeHeader CusGuarantee
		{
			get
			{
				if (cusGuarantee == null || cusGuarantee.IsDeleted || cusGuarantee.PK != PW_CPH_Guarantee)
				{
					cusGuarantee = Factory.Load<CusGuaranteeHeader>(PW_CPH_Guarantee);
				}
				return cusGuarantee;
			}
		}
		CusGuaranteeHeader cusGuarantee;

		protected void SetValueIfCanAcquireGuaranteeManagementMutexLock<T>(ZPropertyInfo info, T value, Action<T, T> setValue)
			where T : IZType
		{
			var oldValue = (T)info.Value;
			if (!oldValue.Equals(value))
			{
				if (((IBusinessObjectInternals)this).IsUnCommittedRow || LockGuaranteeManagementMutex())
				{
					setValue(oldValue, value);
				}
				else
				{
					info.RefreshBinding();
				}
			}
		}

		protected bool LockGuaranteeManagementMutex(bool warnMutexText = true) => Instruction?.LockGuaranteeManagementMutex(warnMutexText) ?? true;

		public override bool CanDelete => LockGuaranteeManagementMutex(false);

		public override MultilingualString ReasonForNotAbleToDelete => !LockGuaranteeManagementMutex(false)
			? ResString.GetMultilingualString("f178ae0b-9f67-43b2-832d-a9b7b1105eac",
				"{0} cannot be deleted. {1}",
				HumanReadableName,
				Instruction?.GuaranteeManagementMutexText
			) : base.ReasonForNotAbleToDelete;

		public new CommonCusBondDetailLookups Lookups => (CommonCusBondDetailLookups)base.Lookups;

		public new CommonCusBondDetailValidation Validation => (CommonCusBondDetailValidation)base.Validation;

		protected override CusBondDetailLookups GetNewLookups() => new CommonCusBondDetailLookups(this);

		protected override MasterFiles.Business.CusBondDetailValidation GetNewValidation() => new CommonCusBondDetailValidation(this);

		public bool IsConsumed => PW_ActivityCode == GuaranteeActivityCodeList.Codes.ConsumesGuarantee;

		protected override TypeLoaderCollection ParentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PW_Status = GuaranteeStatusList.Codes.NotLinked;
		}

		protected SharedCusPermitLineTransaction AddTransaction(ZDecimal amount, bool checkBursting, Action<ZString, ZDecimal> notifier, bool reversing = false)
		{
			SharedCusPermitLineTransaction result = null;

			if (!amount.IsEmpty)
			{
				result = GetTransactionHeader()?.AddTransaction(GetTransactionReference()
					, GetTransactionComment()
					, GetTransactionAppId()
					, GetTransactionProcedure()
					, !reversing ? GetTransactionAmount(amount) : (ZDecimal)(GetTransactionAmount(amount) * -1)
					, ZDecimal.Zero
					, GetTransactionStatus()
					, transactionType: GetTransactionType()
					, transactionDate: GetTransactionDate()
					, notifier: notifier
					, checkBursting: checkBursting
					);
			}

			return result;
		}

		protected static void ThrowBurstException(ZString message, ZDecimal value)
		{
			throw new ZCannotSaveException(message, Res.GetString("f8cb44b0-74e3-4c19-8231-a985c4ac920b", "Can burst..."));
		}

		protected virtual CusGuaranteeHeader GetTransactionHeader() => CusGuarantee;

		protected ZString GetTransactionReference() => PW_BondNumber2;

		protected ZString GetTransactionComment() => Instruction?.CEI_Description ?? ZString.Empty;

		protected ZString GetTransactionAppId() => Instruction?.JobDeclaration?.JE_DeclarationReference ?? ZString.Empty;

		protected ZString GetTransactionProcedure() => Instruction?.CEI_Style ?? ZString.Empty;

		protected ZDateTime GetTransactionDate() => ZDateTime.Now;

		ZDecimal GetTransactionAmount(ZDecimal amount) => IsConsumed ? (ZDecimal)(-1 * amount) : amount;

		protected ZString GetTransactionStatus() => PermitTransactionStatusList.Codes.Confirmed;

		protected ZString GetTransactionType() => PermitTransactionTypeList.Codes.CUS;
	}
}
