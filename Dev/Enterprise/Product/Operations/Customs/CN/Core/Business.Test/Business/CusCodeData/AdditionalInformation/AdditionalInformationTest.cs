using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AdditionalInformation))]
	class AdditionalInformationTest : Customs.Business.Testing.CusCodeDataTest<AdditionalInformation>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((AdditionalInformation)BusinessObject).SupportsNotes);
		}

		public void TestDefaultValues()
		{
			var addInfo = (AdditionalInformation)GetNewBusinessObject();
			AssertEquals(Constants.CusCodeDataTypes.Codes.AdditionalInformation, addInfo.CY_Type);
		}

		protected override IEnumerable<AdditionalInformation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "DK32342";
			var pivot = part.PivotsForBinding.AddNew();
			yield return pivot.AdditionalInformationCodes.AddNew("00000", "111");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AdditionalInformation>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var additionalInfo = factory.New<AdditionalInformation>();
			additionalInfo.CY_ParentID = ZGuid.NewZGuid();
			additionalInfo.CY_ParentTableCode = "JI";
			additionalInfo.CY_Code = "00000";
			additionalInfo.CY_Data = "XXX";
			return additionalInfo;
		}
	}
}
