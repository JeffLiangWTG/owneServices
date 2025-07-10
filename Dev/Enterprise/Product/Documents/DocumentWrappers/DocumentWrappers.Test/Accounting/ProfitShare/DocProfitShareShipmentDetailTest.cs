using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocProfitShareShipmentDetail))]
	sealed class DocProfitShareShipmentDetailTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			Shipment = CommonShipment.New(Factory);
			return new DocumentWrapper[] { DocProfitShareShipmentDetail.New(new ProfitShareShipmentDetail(Shipment, Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory), Factory) };
		}

		CommonShipment Shipment;

		public void TestCalculateProfitShare()
		{
			var shipment = CommonShipment.New(Factory);
			var profitShareShipmentDetail = new ProfitShareShipmentDetail(shipment, Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory);

			AssertNull(profitShareShipmentDetail.ProfitSharePartyDetails);

			var detail = DocProfitShareShipmentDetail.New(profitShareShipmentDetail, Factory);
			var charge = Factory.NewWithValidTestData<JobCharge>();

			AssertEquals(ZDecimal.Zero, detail.CalculateProfitShare(charge));
		}
	}
}
