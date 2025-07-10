using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ABLEntryNumValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCE_EntryNum()
		{
			EntryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			EntryNum.CE_EntryNum = "1234";
			var duplicateEntryNum = EntryNum.Bill.CustomsEntryNumbers.AddNew();
			duplicateEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			duplicateEntryNum.CE_EntryNum = "1234";
			AssertHasMessageError(duplicateEntryNum.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(CusEntryNumberTypes.Standard.LocalReferenceNumber, true));

			duplicateEntryNum.Delete();
			var newBill = EntryNum.Bill.Header.Bills.AddNew();
			duplicateEntryNum = newBill.CustomsEntryNumbers.AddNew();
			duplicateEntryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			duplicateEntryNum.CE_EntryNum = "1234";
			AssertHasMessageError(duplicateEntryNum.CE_EntryNumInfo, ABLEntryNumValidation.LRNNumbersCanOnlyBeEnteredOnce(CusEntryNumberTypes.Standard.LocalReferenceNumber, false));
		}

		public void TestValidateAssociatedPacks()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			entryNum = bill.CustomsEntryNumbers.AddNew();

			entryNum.Validation.ValidateAll();
			AssertHasRowMessageError(entryNum, ABLEntryNumValidation.AssociatedPacksRequired("Customs Number"));
			entryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNum.Validation.ValidateAll();
			AssertHasRowMessageError(entryNum, ABLEntryNumValidation.AssociatedPacksRequired(CusEntryNumberTypes.Standard.LocalReferenceNumber));
			var pack = bill.Packs.AddNew();
			entryNum.PackPivots.AddPivotFor(pack);
			entryNum.Validation.ValidateAll();
			AssertNoRowMessageError(entryNum, ABLEntryNumValidation.AssociatedPacksRequired(CusEntryNumberTypes.Standard.LocalReferenceNumber));
		}

		public void TestCheckCE_EntryType()
		{
			EntryNum.CE_EntryNum = "1234";
			EntryNum.Validation.ValidateCE_EntryType();
			AssertHasMessageErrorContaining(EntryNum.CE_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestNumberUsedMoreThanOnceNoBills()
		{
			var entryNum = Factory.New<ABLEntryNum>();
			var validation = new ABLEntryNumValidationForTest(entryNum);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("checkOtherBills: True", () => validation.AddErrorIfNumberUsedMoreThanOnce_Exposed(true));
				AssertNoExceptionThrown("checkOtherBills: False", () => validation.AddErrorIfNumberUsedMoreThanOnce_Exposed(false));
			});
		}

		ABLEntryNum EntryNum
		{
			get
			{
				if (entryNum == null)
				{
					var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
					var bill = header.Bills.AddNew();
					entryNum = bill.CustomsEntryNumbers.AddNew();
				}
				return entryNum;
			}
		}
		ABLEntryNum entryNum;
	}

	sealed class ABLEntryNumValidationForTest : ABLEntryNumValidation
	{
		public ABLEntryNumValidationForTest(ABLEntryNum entryNumber) : base(entryNumber)
		{
		}

		public void AddErrorIfNumberUsedMoreThanOnce_Exposed(bool checkOtherBills) => AddErrorIfNumberUsedMoreThanOnce(checkOtherBills);
	}
}
