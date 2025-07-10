using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class ASIRunDocsTest : ClientSpecificRunDocsTest
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
			get { return "ASI"; }
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestASHBill()
		{
			SetShipmentHBLType("ASH");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestASSBill()
		{
			SetShipmentHBLType("ASS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestSUNBill()
		{
			SetShipmentHBLType("SUN");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestASHPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("AHP");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestASSPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("ASP");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestSUNPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("SUP");
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
