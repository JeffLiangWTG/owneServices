using Enterprise.DocumentEngine.Business.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentCommandDeliveryRestriction))]
	class DocumentCommandDeliveryRestrictionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeliveryRestrictionTypeList()
		{
			var restriction = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			AssertEquals(2, restriction.DeliveryRestrictionTypeList.Count);
			AssertEquals("NON", restriction.DeliveryRestrictionTypeList[0].Code);
			AssertEquals("None", restriction.DeliveryRestrictionTypeList[0].Description);
			AssertEquals("UDF", restriction.DeliveryRestrictionTypeList[1].Code);
			AssertEquals("User Defined", restriction.DeliveryRestrictionTypeList[1].Description);
		}

		public void TestDeliveryRestrictionType()
		{
			var restriction = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			restriction.SDR_DeliveryRestrictionMacro = "Test Macro";
			restriction.SDR_DeliveryRestrictionDescription = "Test Description";
			AssertEquals("Test Macro", restriction.SDR_DeliveryRestrictionMacro);

			restriction.SDR_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
			AssertNullOrEmpty("SDR_DeliveryRestrictionMacro", restriction.SDR_DeliveryRestrictionMacro);
			AssertEquals("Test Description", restriction.SDR_DeliveryRestrictionDescription);
		}

		public void TestDeliveryRestrictionCountryCodeReadOnly()
		{
			var restriction = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.Parent = Factory.NewWithValidTestData<DummyBODocSupportable>();
			restriction.SDR_SU = documentCommand.PK;

			Assert(restriction.Command == documentCommand);

			AssertEquals(true, restriction.SDR_RN_NKOriginCountryCode_ReadOnly);
			AssertEquals(true, restriction.SDR_RN_NKDestinationCountryCode_ReadOnly);

			documentCommand.Parent = Factory.NewWithValidTestData<DummyDeliveryRestrictionBODocSupportable>();
			AssertEquals(false, restriction.SDR_RN_NKOriginCountryCode_ReadOnly);
			AssertEquals(false, restriction.SDR_RN_NKDestinationCountryCode_ReadOnly);
		}
	}
}
