using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class CNPRunDocsTest : ClientSpecificRunDocsTest
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
			get { return "CNP"; }
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestCNSBill()
		{
			SetShipmentHBLType("CNS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestEPLBill()
		{
			SetShipmentHBLType("EPL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestMELBill()
		{
			SetShipmentHBLType("MEL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestOCLBill()
		{
			SetShipmentHBLType("OCL");
			RunDocument();
		}

		protected override void SetUp()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			fBusinessContext = BusinessContext.Shipment;
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			base.SetUp();
		}
	}
}
