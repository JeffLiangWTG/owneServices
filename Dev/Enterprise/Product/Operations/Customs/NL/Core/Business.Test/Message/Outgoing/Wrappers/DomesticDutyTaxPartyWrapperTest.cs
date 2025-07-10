using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class DomesticDutyTaxPartyWrapperTest : DataProviderTestCase<DomesticDutyTaxPartyWrapper>
{
	public void TestConstructor()
	{
		CusReference testCusReference = null;
		AssertExceptionThrown<ArgumentNullException>(() => new DomesticDutyTaxPartyWrapper(testCusReference, 1));

		CusSupportingInfo testCusSupportingInfo = null;
		AssertExceptionThrown<ArgumentNullException>(() => new DomesticDutyTaxPartyWrapper(testCusSupportingInfo, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapperAddInfo.SequenceNumeric);
	}

	public void TestId()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Additional Info", "INF-123", wrapperAddInfo.Id);
			AssertEquals("Previous Document", "PRV-357", wrapperPrevDoc.Id);
			AssertEquals("Supply Chain Actor Reference", "SupplyChainActorReference", wrapperReference.Id);
			AssertEquals("Supporting Info", "REFREF1", wrapperAddInfoGoodsItem.Id);
		});
	}

	public void TestRoleCode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Additional Info", "INF", wrapperAddInfo.RoleCode);
			AssertEquals("Previous Document", "IMH", wrapperPrevDoc.RoleCode);
			AssertEquals("Supply Chain Actor Reference", "FW", wrapperReference.RoleCode);
			AssertEquals("Supporting Info", "REFCOD1", wrapperAddInfoGoodsItem.RoleCode);
		});
	}

	protected override DomesticDutyTaxPartyWrapper GetProvider() => wrapperAddInfo;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<Declaration.JobDeclaration>();
		var previousDocument = declaration.PreviousDocuments.AddNew();
		previousDocument.CSI_ReferenceNumber = "PRV-357";
		previousDocument.CSI_Procedure = "IMH";

		var additionalInfo = Factory.New<AdditionalInfo>();

		additionalInfo.CSI_ReferenceNumber = "INF-123";
		additionalInfo.CSI_Code = "INF";

		previousDocument.CSI_Procedure = "IMH";
		var cusReference = Factory.New<CusReference>();
		cusReference.CFR_Reference = "SupplyChainActorReference";
		cusReference.CFR_Code = "FW";

		var addInfoGoodsItem = Factory.New<AdditionalInfo>();
		addInfoGoodsItem.CSI_ReferenceNumber = "REFREF1";
		addInfoGoodsItem.CSI_Code = "REFCOD1";

		wrapperAddInfo = new DomesticDutyTaxPartyWrapper(additionalInfo, 1);
		wrapperPrevDoc = new DomesticDutyTaxPartyWrapper(previousDocument, 1);
		wrapperReference = new DomesticDutyTaxPartyWrapper(cusReference, 1);
		wrapperAddInfoGoodsItem = new DomesticDutyTaxPartyWrapper(addInfoGoodsItem, 1);
	}
	DomesticDutyTaxPartyWrapper wrapperAddInfo;
	DomesticDutyTaxPartyWrapper wrapperPrevDoc;
	DomesticDutyTaxPartyWrapper wrapperReference;
	DomesticDutyTaxPartyWrapper wrapperAddInfoGoodsItem;
}
