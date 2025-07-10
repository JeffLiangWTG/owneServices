using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class SDVRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Overrides

		BusinessContext fBusinessContext;
		public override BusinessContext BusinessContext
		{
			get { return fBusinessContext; }
		}

		BusinessObject BusinessObjectForTest;
		public override BusinessObject GetBusinessObject
		{
			get { return BusinessObjectForTest; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		protected override ZString ClientName
		{
			get { return "SDV"; }
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestTSLBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			fBusinessContext = BusinessContext.Shipment;
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("TSL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestPrintInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice");
			fBusinessContext = BusinessContext.ARInvoice;
			BusinessObjectForTest = Factory.New<ARInvoice>();
			RunDocument();
		}
	}
}
