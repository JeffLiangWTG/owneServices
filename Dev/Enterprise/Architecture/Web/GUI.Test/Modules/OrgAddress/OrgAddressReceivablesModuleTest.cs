using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(OrgAddressReceivablesModule))]
	sealed class OrgAddressReceivablesModuleTest : OrgAddressModuleTest
	{
		#region Setup

		protected override BusinessObject CreateNewElementForExcelExport()
		{
			var address = (OrgAddress)base.CreateNewElementForExcelExport();
			var receivable = address.CapabilitiesCollection.AddNew();
			receivable.PZ_AddressType = OrgAddressType.Receivables.Code;

			return address;
		}

		protected override void TestFilterBusinessObjectTypeCore()
		{
			base.TestFilterBusinessObjectTypeCore();

			AssertEquals(typeof(OrgAddressReceivablesFilterBusinessObject), FilterGridModule.FilterBusinessObjectType);
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.OrgAddressReceivablesTracking; }
		}

		#endregion
	}
}
