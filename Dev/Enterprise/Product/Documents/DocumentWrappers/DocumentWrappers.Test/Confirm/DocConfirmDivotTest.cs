using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocConfirmDivot))]
	sealed class DocConfirmDivotTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot = confirm.GetDivot(packLine);

			DocConfirmDivot divotWrapper = DocConfirmDivot.New(divot, Factory);
			return new DocumentWrapper[] { divotWrapper };
		}
	}
}
