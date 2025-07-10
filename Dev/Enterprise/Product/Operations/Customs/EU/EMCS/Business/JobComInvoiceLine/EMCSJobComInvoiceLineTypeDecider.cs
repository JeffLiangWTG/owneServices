using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSJobComInvoiceLineTypeDecider : BaseJobComInvoiceLineTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory) => typeof(EMCSJobComInvoiceLine);

		public override Type GetTypeForBinding() => typeof(EMCSJobComInvoiceLine);

		protected override Type GetTypeForNewCore(ITypeDeciderContext context) => typeof(EMCSJobComInvoiceLine);

		protected override Type DefaultTypeForUnsupportedCountry => typeof(EMCSJobComInvoiceLine);
	}
}
