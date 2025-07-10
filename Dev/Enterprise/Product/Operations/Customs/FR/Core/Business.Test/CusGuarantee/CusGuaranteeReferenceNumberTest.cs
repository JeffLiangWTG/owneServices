using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	[TestedType(typeof(CusGuaranteeReferenceNumber))]
	public class CusGuaranteeReferenceNumberTest : CusCodeDataTest<CusGuaranteeReferenceNumber>
	{
		public void TestValidation()
		{
			var cusGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var cusGuaranteeRefNumber = cusGuarantee.AdditionalGuaranteeReferences.AddNew();
			AssertType<CusGuaranteeReferenceNumberValidation>(cusGuaranteeRefNumber.Validation);
		}

		protected override IEnumerable<CusGuaranteeReferenceNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetNewCusGuaranteeReferenceNumber(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewCusGuaranteeReferenceNumber(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewCusGuaranteeReferenceNumber();
		}

		CusGuaranteeReferenceNumber GetNewCusGuaranteeReferenceNumber(BusinessObjectFactory factory = null)
		{
			var currentFactory = factory ?? Factory;
			var cusGuarantee = currentFactory.NewWithValidTestData<CusGuaranteeHeaderForTest>();
			var cusGuaranteeRefNumber = cusGuarantee.AdditionalGuaranteeReferences.AddNew();
			return cusGuaranteeRefNumber;
		}
	}

	public class CusGuaranteeHeaderForTest : CusGuaranteeHeader
	{
		public CusGuaranteeHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool SupportsAdditionalCustomsReferencesCore => true;
	}
}
