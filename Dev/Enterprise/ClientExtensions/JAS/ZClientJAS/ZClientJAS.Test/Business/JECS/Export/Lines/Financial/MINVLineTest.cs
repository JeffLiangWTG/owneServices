using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class MINVLineTest : MaritimeINVCDTLineTestCase
	{
		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.MINV;
			}
		}

		protected override IJASInvoicingBase GetNewJASInvoicingBase()
		{
			return Factory.New<JASARInvoice>();
		}
	}
}
