using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class RHLRunDocsTest : ClientSpecificRunDocsTest
	{
		public RHLRunDocsTest()
		{
		}

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
			get { return "RHL"; }
		}

		[ExpectNoExceptions]
		public void TestPrintInvoice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Invoice");
			fBusinessContext = BusinessContext.ARInvoice;
			BusinessObjectForTest = Factory.New<ARInvoice>();
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestRHLBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading");
			fBusinessContext = BusinessContext.Shipment;
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("RHL");
			RunDocument();
		}
	}
}
