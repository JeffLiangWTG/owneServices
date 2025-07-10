using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSupportingDocumentPhase5LookupsTest : TestCaseWithFactory
{
	public void TestTypeCodeList()
	{
		var levelAttribute = (Universal.RefCusCodeListAttributeTypes.Codes.Level, UniversalReferenceConstants.RefCusCodeListLevelTypes.Header);
		_ = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
			dataGrouping: Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
			codes: ["CODE1", "CODE3"],
			levelAttribute).ToArray();
		_ = CusSupportingInfoTestHelper.CreateRefCusCodeListsForTesting(Factory,
			dataGrouping: Core.Constants.CountryCodes.Italy,
			codeType: EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.SupportingDocumentOfNCTS,
			codes: ["IT_CODE1", "IT_CODE2"]).ToArray();
		Factory.Save();

		var list = supportingDocument.Lookups.TypeCodeList;
		list.Load();

		var expectedCodes = new ZString[] { "IT_CODE1", "IT_CODE2" };
		AssertContainsExactElementsInAnyOrder("Should only include IT codes", expectedCodes, list.Select(c => c.ZZD_Code));
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = "NC5";
		supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
	}

	NctsHeader nctsHeader;
	NctsSupportingDocument supportingDocument;
}
