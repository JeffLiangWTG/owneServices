using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocProfitShareShipmentDetailCollection))]
	sealed class DocProfitShareShipmentDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocProfitShareShipmentDetailCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			return DocProfitShareShipmentDetail.New(new ProfitShareShipmentDetail(shipment, Factory.New<OrgHeader>(), OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, Factory), Factory);
		}

		protected override DocProfitShareShipmentDetailCollection GetCollectionToTest()
		{
			return new DocProfitShareShipmentDetailCollection(Factory);
		}
	}
}
