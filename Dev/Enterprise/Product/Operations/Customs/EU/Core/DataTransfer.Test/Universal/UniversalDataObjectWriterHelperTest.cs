using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class UniversalDataObjectWriterHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetReferencedEntityDescription()
		{
			var factory = Factory.BOFactory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				"123", "123 test only",
				ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.SaveForTesting();

			var dec = Factory.New<JobDeclaration>();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Data = "123";
			AssertEquals("123 test only", officeCode.CY_OfficeDescription);
			Factory.SaveForTesting();

			dec = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			officeCode = dec.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Data == "123");
			AssertEquals("123 test only", officeCode.CY_OfficeDescription);

			var writerHelper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedKingdom);
			var referenceEntityDescription = writerHelper.GetReferencedEntityDescriptionForCusCodeData(officeCode);
			AssertEquals("123 test only", referenceEntityDescription);

			referenceEntityDescription = writerHelper.GetReferencedEntityDescriptionForCusCodeData(Factory.New<EuOfficeCode>());
			AssertEquals(string.Empty, referenceEntityDescription);

			referenceEntityDescription = writerHelper.GetReferencedEntityDescriptionForCusCodeData(null);
			AssertEquals(null, referenceEntityDescription);
		}

		public void TestCusCodeDataMappings_ReferencedEntityDescription()
		{
			var factory = Factory.BOFactory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				"123", "123 test only",
				ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.SaveForTesting();
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsOffices.RemoveAndDeleteAll();
			var officeCode = dec.CustomsOffices.AddNew();
			officeCode.CY_Data = "123";
			AssertEquals("123 test only", officeCode.CY_OfficeDescription);
			Factory.SaveForTesting();

			var writerhelper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedKingdom);
			var collection = CustomsReferenceCollectionCreator.CreateCollection(writerhelper, dec, null);
			AssertEquals(1, collection.Count);
			AssertEquals("123 test only", collection[0].ReferencedEntityDescription);
		}

		public void TestAllocateAndGetDv1DetailsLink()
		{
			CombineAssertions(() =>
			{
				var factory = Factory.BOFactory;
				var writerHelper = new UniversalDataObjectWriterHelper(factory, Core.Constants.CountryCodes.UnitedKingdom);

				var testID1 = ZGuid.NewZGuid();
				var testID2 = ZGuid.Empty;
				var testID3 = ZGuid.Invalid;
				var testID4 = ZGuid.NewZGuid();

				AssertNull("Getting From Blank 1", writerHelper.GetAllocatedDv1DetailsLink(testID1));
				AssertNull("Getting From Blank 2", writerHelper.GetAllocatedDv1DetailsLink(testID2));
				AssertNull("Getting From Blank 3", writerHelper.GetAllocatedDv1DetailsLink(testID3));
				AssertNull("Getting From Blakn 4", writerHelper.GetAllocatedDv1DetailsLink(testID4));

				AssertEquals("Setting to Blank 1", 1, writerHelper.AllocateDv1DetailsLink(testID1));
				AssertNull("Setting to Blank 2", writerHelper.AllocateDv1DetailsLink(testID2));
				AssertNull("Setting to Blank 3", writerHelper.AllocateDv1DetailsLink(testID3));
				AssertEquals("Setting to Blank 4", 2, writerHelper.AllocateDv1DetailsLink(testID4));

				AssertEquals("Getting from allocated 1", 1, writerHelper.GetAllocatedDv1DetailsLink(testID1));
				AssertNull("Getting from allocated 2", writerHelper.GetAllocatedDv1DetailsLink(testID2));
				AssertNull("Getting from allocated 3", writerHelper.GetAllocatedDv1DetailsLink(testID3));
				AssertEquals("Getting from allocated 4", 2, writerHelper.GetAllocatedDv1DetailsLink(testID4));
			});
		}
	}
}
