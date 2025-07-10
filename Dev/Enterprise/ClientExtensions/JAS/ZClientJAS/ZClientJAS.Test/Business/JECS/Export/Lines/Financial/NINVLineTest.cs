using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class NINVLineTest : NonShipmentINVCDTLineTestCase
	{
		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.NINV;
			}
		}

		protected override IJASInvoicingBase GetNewJASInvoicingBase()
		{
			return Factory.New<JASARInvoice>();
		}
	}
}
