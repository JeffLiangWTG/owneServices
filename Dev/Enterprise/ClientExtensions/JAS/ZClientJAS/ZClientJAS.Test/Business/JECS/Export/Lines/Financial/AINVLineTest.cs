using CargoWise.Types;
using Enterprise.Client.JAS.Business.Invoicing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class AINVLineTest : AirINVCDTLineTestCase
	{
		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.AINV;
			}
		}

		protected override IJASInvoicingBase GetNewJASInvoicingBase()
		{
			return Factory.New<JASARInvoice>();
		}
	}
}
