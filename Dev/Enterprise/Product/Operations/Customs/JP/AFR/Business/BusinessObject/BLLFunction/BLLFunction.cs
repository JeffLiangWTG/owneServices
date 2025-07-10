using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business
{
	public abstract partial class BLLFunction : AutoBLLFunction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected BLLFunction(JPAFRHeader header, BLLFunctionCode functionCode, JPAFRBills bill = null) : base(new BusinessObjectFactory())
		{
			Header = Argument.NotNull(header, nameof(header));
			FunctionCode = functionCode;
			SelectedAFRBill = bill;

			Factory.Saving -= Factory_Saving;
			Factory.Saving += Factory_Saving;

			Header.Bills
				.Where(x => x.IsBillAlreadyRegistered)
				.OrderBy(x => x.JPB_BillNumber)
				.ForEach(x =>
				{
					RegisteredBills.Add(new BLLFunctionBill(x));
				});
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			var masterBill = MasterAFRBill;
			if (masterBill != null)
			{
				var masterBillNumber = JPM_BillOfLadingNumber;
				var changeResonCode = JPM_ChangeReasonCode;
				var functionCode = (int)FunctionCode;
				var selectBillNumbers = SelectedBills.Cast<BLLFunctionBill>().Select(x => x.JPM_BillOfLadingNumber).ToArray();

				CreateOrUpdateFunctionInfo(factory, masterBill, masterBillNumber, changeResonCode, functionCode, selectBillNumbers);
				SelectedBills.Cast<BLLFunctionBill>().ForEach(x =>
				{
					CreateOrUpdateFunctionInfo(factory, x.AFRBill, masterBillNumber, changeResonCode, functionCode, selectBillNumbers);
				});
			}
		}

		static void CreateOrUpdateFunctionInfo(BusinessObjectFactory factory, JPAFRBills bill, ZString masterBillNumber, ZString changeReasonCode, int functionCode, ZString[] selectedBillNumbers)
		{
			var bllFunctionInfo = bill.BLLFunctionInfo ?? factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill.PK;
			bllFunctionInfo.B7_ParentTableCode = JPAFRBillsSchema.Constants.Prefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
			bllFunctionInfo.JP_ChangeReasonCode = changeReasonCode;
			bllFunctionInfo.JP_MasterBillNumber = masterBillNumber;
			bllFunctionInfo.JP_FunctionCode = functionCode;
			bllFunctionInfo.LinkedBills = selectedBillNumbers;
		}

		[List(nameof(Lookups) + "." + nameof(BLLFunctionLookups.RegisteredBillList))]
		[ReadOnlyMember(nameof(JPM_BillOfLadingNumberReadOnly))]
		public override ZString JPM_BillOfLadingNumber
		{
			get => base.JPM_BillOfLadingNumber;
			set => base.JPM_BillOfLadingNumber = value;
		}

		public bool JPM_BillOfLadingNumberReadOnly => SelectedAFRBill != null;

		public JPAFRBills MasterAFRBill => SelectedAFRBill ?? Header.Bills.FirstOrDefault(x => x.JPB_BillNumber == JPM_BillOfLadingNumber);

		[List(nameof(Lookups) + "." + nameof(BLLFunctionLookups.ChangeReasonCodeList))]
		public override ZString JPM_ChangeReasonCode
		{
			get => base.JPM_ChangeReasonCode;
			set => base.JPM_ChangeReasonCode = value;
		}

		public void SelectBill(BLLFunctionBill bill)
		{
			AvailableBills.Remove(bill);
			SelectedBills.Add(bill);
		}

		public void UnselectBill(BLLFunctionBill bill)
		{
			SelectedBills.Remove(bill);
			AvailableBills.Add(bill);
		}

		public abstract ZBool SelectEnabled { get; }

		public abstract ZBool UnselectEnabled { get; }

		public abstract ZBool SendEnabled { get; }

		public BLLFunctionBillCollection SelectedBills
		{
			get
			{
				if (selectedBills == null)
				{
					selectedBills = new BLLFunctionBillCollection(Factory);
					RegisterEditableChildObject(selectedBills);
				}
				return selectedBills;
			}
		}
		BLLFunctionBillCollection selectedBills;

		public BLLFunctionBillCollection AvailableBills
		{
			get
			{
				if (availableBills == null)
				{
					availableBills = new BLLFunctionBillCollection(Factory);
					RegisterEditableChildObject(availableBills);
				}
				return availableBills;
			}
		}
		BLLFunctionBillCollection availableBills;

		public BLLFunctionBillCollection RegisteredBills => registeredBills ?? (registeredBills = new BLLFunctionBillCollection(Factory));
		BLLFunctionBillCollection registeredBills;

		public BLLFunctionLookups Lookups => lookups ?? (lookups = CreateLookups());
		BLLFunctionLookups lookups;

		protected virtual BLLFunctionLookups CreateLookups() => new BLLFunctionLookups(this);

		public readonly BLLFunctionCode FunctionCode;
		protected readonly JPAFRBills SelectedAFRBill;
		protected readonly JPAFRHeader Header;
	}

	public abstract partial class BLLFunction
	{
		public static BLLFunction New(JPAFRHeader header, BLLFunctionCode functionCode, JPAFRBills bill = null)
		{
			BLLFunction res;
			switch (functionCode)
			{
				case BLLFunctionCode.RegisterSplit:
				case BLLFunctionCode.RegisterSwitch:
				case BLLFunctionCode.RegisterMerge:
					res = new BLLRegistration(header, functionCode, bill);
					break;
				case BLLFunctionCode.CancelSplit:
				case BLLFunctionCode.CancelSwitch:
				case BLLFunctionCode.CancelMerge:
					res = new BLLCancellation(header, functionCode, bill);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(functionCode), functionCode, null);
			}
			return res;
		}
	}
}
