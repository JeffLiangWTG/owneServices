using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AdditionalIdentification))]
	class AdditionalIdentificationTest : Customs.Business.Testing.CusCodeDataTest<AdditionalIdentification>
	{
		public void TestSetDefaultValues()
		{
			var additionalIdentification = Factory.New<AdditionalIdentification>();
			AssertEquals(OrgHeaderSchema.Constants.Prefix, additionalIdentification.CY_ParentTableCode);
			AssertEquals("CY_Type", Common.BR.CusCodeDataTypeList.Codes.AdditionalIdentification, additionalIdentification.CY_Type);
		}

		public void TestLookups()
		{
			var additionalIdentification = Factory.New<AdditionalIdentification>();
			AssertEquals("Lookups", typeof(AdditionalIdentificationLookups), additionalIdentification.Lookups.GetType());
		}

		public void TestValidation()
		{
			var additionalIdentification = Factory.New<AdditionalIdentification>();
			AssertType<AdditionalIdentificationValidation>(additionalIdentification.Validation);
		}

		public void TestSupportsNotes()
		{
			var additionalIdentification = Factory.New<AdditionalIdentification>();
			Assert("SupportsNotes should be false", !additionalIdentification.SupportsNotes);
		}

		protected override IEnumerable<AdditionalIdentification> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();
			var orgWrapper = OrgHeaderWrapper.New(orgHeader);

			yield return orgWrapper.AdditionalIdentification.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<AdditionalIdentification>();
	}
}
