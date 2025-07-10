using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestSADOfficeCode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(bill, Constants.CustomsEntryType.SAD, Core.Constants.CountryCodes.Bangladesh);
			registrationNumber.CE_EntryLineReference = "JAS";
			var registrationNumber2 = CusEntryNumber.New<ABLEntryNum>(bill, Constants.CustomsEntryType.SAD, Core.Constants.CountryCodes.Namibia);
			registrationNumber2.CE_EntryLineReference = "NBA";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Default country", Core.Constants.CountryCodes.Eritrea, header.AMA_RN_NKCountry);
				AssertEquals("No SADOfficeCode if the country has no SAD registration number", ZString.Empty, bill.SADOfficeCode);

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Namibia;
				AssertEquals("SADOfficeCode valid for any manifest country", "NBA", bill.SADOfficeCode);

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals("SADOfficeCode", "JAS", bill.SADOfficeCode);
				bill.SADOfficeCode = "NEW";
				AssertEquals("SADOfficeCode can be set", "NEW", bill.SADOfficeCode);
				Factory.Save();
				AssertEquals("succeed to save", "NEW", bill.SADOfficeCode);
			});
		}

		public void TestSADRegistrationSerial()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(bill, Constants.CustomsEntryType.SAD, Core.Constants.CountryCodes.Bangladesh);
			registrationNumber.CE_Category = "C";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No SADRegistrationSerial for country other than Bangladesh", ZString.Empty, bill.SADRegistrationSerial);

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals("SADRegistrationSerial", "C", bill.SADRegistrationSerial);
				bill.SADRegistrationSerial = "NEW";
				AssertEquals("SADRegistrationSerial can be set", "NEW", bill.SADRegistrationSerial);
				Factory.Save();
				AssertEquals("succeed to save", "NEW", bill.SADRegistrationSerial);
			});
		}

		public void TestDefaultSADRegistrationSerial()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;

			CombineAssertions(() =>
			{
				var bill = header.Bills.AddNew();
				bill.SADOfficeCode = "100";
				AssertEquals("SADRegistrationSerial default to C when set SADOfficeCode", "C", bill.SADRegistrationSerial);
				bill.SADRegistrationSerial = "NEW";
				AssertEquals("SADRegistrationSerial can be set", "NEW", bill.SADRegistrationSerial);

				var bill2 = header.Bills.AddNew();
				bill2.SADRegistrationNumber = "nnn";
				AssertEquals("SADRegistrationSerial default to C when set SADRegistrationNumber", "C", bill2.SADRegistrationSerial);

				var bill3 = header.Bills.AddNew();
				bill3.SADRegistrationDate = new ZDateTime(2023, 5, 1);
				AssertEquals("SADRegistrationSerial default to C when set SADRegistrationDate", "C", bill3.SADRegistrationSerial);
			});
		}

		public void TestSADRegistrationNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(bill, Constants.CustomsEntryType.SAD, Core.Constants.CountryCodes.Bangladesh);
			registrationNumber.CE_EntryNum = "1234";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No SADRegistrationNumber for country other than Bangladesh", ZString.Empty, bill.SADRegistrationNumber);

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals("SADRegistrationNumber", "1234", bill.SADRegistrationNumber);
				bill.SADRegistrationNumber = "1234NEW";
				AssertEquals("SADRegistrationNumber can be set", "1234NEW", bill.SADRegistrationNumber);
				Factory.Save();
				AssertEquals("succeed to save", "1234NEW", bill.SADRegistrationNumber);
			});
		}

		public void TestSADRegistrationDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var registrationNumber = CusEntryNumber.New<ABLEntryNum>(bill, Constants.CustomsEntryType.SAD, Core.Constants.CountryCodes.Bangladesh);
			registrationNumber.CE_IssueDate = new ZDateTime(2023, 03, 09);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No SADRegistrationDate for country other than Bangladesh", ZDateTime.Empty, bill.SADRegistrationDate);

				header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Bangladesh;
				AssertEquals("SADRegistrationDate", new ZDateTime(2023, 03, 09), bill.SADRegistrationDate);
				bill.SADRegistrationDate = new ZDateTime(2023, 03, 11);
				AssertEquals("SADRegistrationDate can be set", new ZDateTime(2023, 03, 11), bill.SADRegistrationDate);
				Factory.Save();
				AssertEquals("succeed to save", new ZDateTime(2023, 03, 11), bill.SADRegistrationDate);
			});
		}

		public void TestPacks()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("should be generic type", true, bill.Packs.GetType().IsGenericType);
			var genericParams = bill.Packs.GetType().GetGenericArguments();
			AssertEquals("should be 2 generic arguments", 2, genericParams.Length);
			AssertEquals("first argument should be AsycudaPack", true, typeof(AsycudaPack).IsAssignableFrom(genericParams[0]));
			AssertEquals("first argument should be AsycudaBill", true, typeof(AsycudaBill).IsAssignableFrom(genericParams[1]));
		}

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestCanDeleteASYManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals("ASY Manifest Type bills return true here regardless of if they have been sent or not, as user is given a option to delete with warning in GetWarningBeforeBeingDeleted method", true, bill.CanDelete);
		}

		public void TestGetWarningBeforeBeingDeletedForSentBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";
			bill1.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill1.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			AssertEquals("Pre-condition", 1, header.Bills.Count);
			AssertEquals("Pre-condition: Bill1 is in a non deletable state for Bill having been sent already", "This Bill is already registered with Customs.\r\nYou need to delete it from Customs file before deleting it here.", bill1.ReasonForNotAbleToDelete);
			AssertEquals("Bill1 deletion can be overridden as the Manifest Type (ASY) allows for deletion", "Bill number HB1 is already registered with Customs.\r\nAre you sure you want to delete it from the manifest?", bill1.GetWarningBeforeBeingDeleted());

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "HB2";
			bill2.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill2.ABL_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			AssertEquals("Pre-condition", 2, header.Bills.Count);
			AssertEquals("Pre-condition: Bill2 is in a non deletable state for Bill having been sent already", "This Bill is already registered with Customs.\r\nYou need to delete it from Customs file before deleting it here.", bill2.ReasonForNotAbleToDelete);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddYesAnswer();
			AssertEquals("Bill2 deletion can be overridden", "Bill number HB2 is already registered with Customs.\r\nAre you sure you want to delete it from the manifest?", bill2.GetWarningBeforeBeingDeleted());
			AssertEquals("User has decided to override the fact of this bill being in non-deletion state - bill2 can be deleted", true, bill2.CanDelete);
			bill2.Delete();
			AssertEquals("Bill2 has been deleted", 1, header.Bills.Count);

			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillNumber = "HB3";
			bill3.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			bill3.ABL_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertEquals("2 bills on manifest again", 2, header.Bills.Count);
			AssertEquals("Bill3 allows for deletion regardless, has not been sent to customs yet - no warning required", "", bill3.GetWarningBeforeBeingDeleted());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}
	}
}
