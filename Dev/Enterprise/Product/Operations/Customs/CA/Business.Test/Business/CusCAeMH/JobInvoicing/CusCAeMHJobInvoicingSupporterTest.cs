using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHJobInvoicingSupporter))]
	sealed class CusCAeMHJobInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			return Factory.New<CusCAeMHMaster>();
		}

		protected override ZString TestingCountry
		{
			get { return Core.Constants.CountryCodes.Canada; }
		}
	}
}
