using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ABLEntryNumCollection))]
	sealed class ABCEntryNumCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "Customs Entry Number Types");
			helper.CreateNewOrGetExistingCusCodeList("ER", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes, "PRV", "Customs Entry Number Types", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNumber = bill.CustomsEntryNumbers.AddNew();

			AssertEquals(true, entryNumber.Lookups.CustomsEntryNumberTypes.ContainsCode("PRV"));
			AssertEquals(entryNumber.CE_EntryType, entryNumber.Lookups.CustomsEntryNumberTypes.GetAllCodes()[0]);
		}

		public void TestSetDefaultsForNewChild_WhenSupportMultipleCustomsNumbers()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.IEH7.IAsycudaManifestHeader>();
			Assert("Precondition: header supports multiple customs numbers", header.SupportMultipleCustomsNumbers);
			var bill = header.Bills.AddNew();
			AssertEquals("Precondition: Bill has non-empty FilterForSingleEntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, bill.FilterForSingleEntryType);
			var entryNumber = bill.CustomsEntryNumbers.AddNew();

			AssertEquals("CE_EntryType should be set from FilterForSingleEntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryNumber.CE_EntryType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			return new ABLEntryNumCollection(bill);
		}
	}
}
