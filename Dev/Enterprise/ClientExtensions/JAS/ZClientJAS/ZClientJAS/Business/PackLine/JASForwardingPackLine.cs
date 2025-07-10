using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business
{
	public class JASForwardingPackLine : ForwardingPackLine
	{
		public JASForwardingPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LinePriceCurrency
		{
			get { return JL_CustomAttrib1; }
			set { JL_CustomAttrib1 = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LinePriceCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		}
	}
}
