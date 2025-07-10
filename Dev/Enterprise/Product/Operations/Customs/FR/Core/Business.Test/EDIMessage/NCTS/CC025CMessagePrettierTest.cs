using CargoWise.Customs.FR.MessageDefinitions.TP5.CC025C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.EdiMessages.Testing
{
	sealed class CC025CMessagePrettierTest : NCTSMessagePrettierTest<Cc025CType, CC025CMessageDataObject>
	{
		protected override ZString GetExpectedMessageInterpretation() => @"<style> body, p, span { font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px; } ul, #tree { list-style-type: none; line-height: 1.4; } #tree { margin: 0; padding: 0; }</style><p style=""font-size: 120%""><strong>Status: </strong>Goods Release Notification<br><strong>MRN: </strong>MRN1<br><strong>Release date: </strong>01/08/2024<br><strong>Release type: </strong>1 - Full release of goods (as per declaration) - Movement closed</p><ul id=""tree""><li><span><b>Consignment</b></span><ul><li><span><b>House Consignment</b></span><ul><li><strong>Sequence number</strong>: sequenceNumber1</li><li><strong>Release type</strong>: 1</li><li><strong>Release description</strong>: Partial release</li><li><span><b>Consignment Item</b></span><ul><li><strong>Goods item number</strong>: goodsItemNumber1</li><li><strong>Declaration Goods item number</strong>: declarationGoodsItemNumber1</li><li><strong>Release type</strong>: 1</li><li><strong>Release description</strong>: Partial release</li><li><span><b>Commodity</b></span><ul><li><strong>Harmonised system sub-heading code</strong>: subheadingCode123</li><li><strong>Combined nomenclature code</strong>: combinedNomenclatureCode123</li><li><strong>Description of goods</strong>: descriptionOfGoods1</li></ul></li><li><span><b>Packaging</b></span><ul><li><strong>Sequence number</strong>: sequenceNumber1</li><li><strong>Number of packages</strong>: 3</li><li><strong>Type of packages</strong>: typeOfPackages1</li><li><strong>Shipping marks</strong>: shippingMarks1</li></ul></li></ul></li><li><span><b>Consignment Item</b></span><ul><li><strong>Goods item number</strong>: goodsItemNumber1</li><li><strong>Declaration Goods item number</strong>: declarationGoodsItemNumber1</li><li><strong>Release type</strong>: 1</li><li><strong>Release description</strong>: Partial release</li><li><span><b>Commodity</b></span><ul><li><strong>Harmonised system sub-heading code</strong>: subheadingCode123</li><li><strong>Combined nomenclature code</strong>: combinedNomenclatureCode123</li><li><strong>Description of goods</strong>: descriptionOfGoods1</li></ul></li><li><span><b>Packaging</b></span><ul><li><strong>Sequence number</strong>: sequenceNumber1</li><li><strong>Number of packages</strong>: 3</li><li><strong>Type of packages</strong>: typeOfPackages1</li><li><strong>Shipping marks</strong>: shippingMarks1</li></ul></li></ul></li></ul></li></ul></li></ul>";

		protected override ZString GetMessageText() => resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_CC025C_RI1_ResponseMessage.xml");

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL163, "CL163 codes list", Core.Constants.CountryCodes.France);
			helper.CreateCusCodeType(EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL164, "CL164 codes list", Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL163, "1", "Partial release", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(+1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL164, "1", "Full release of goods (as per declaration) - Movement closed", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(+1));
			Factory.Save();
		}
	}
}
