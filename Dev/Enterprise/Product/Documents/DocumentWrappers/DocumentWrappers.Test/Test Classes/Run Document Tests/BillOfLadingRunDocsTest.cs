using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class BillOfLadingRunDocsTest : BaseRunDocumentsTest
	{
		public BillOfLadingRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.Consols.AddNew();
				shipment.JS_HouseBillOfLadingType = fHBLType;
				shipment.JS_NoCopyBills = 1;
				shipment.JS_NoOriginalBills = 1;
				return shipment;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		public ZString HBLType
		{
			get { return fHBLType; }
			set { fHBLType = value; }
		}

		#region HBL - FIATA

		[ExpectNoExceptions]
		public void TestFIATAHBL()
		{
			HBLType = "FIA";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestFIATAPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			HBLType = "FIP";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestFIATAHBLRail()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading (Rail)");
			HBLType = "FIA";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestFIATAHBLRoad()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading (Road)");
			HBLType = "FIA";
			RunDocument();
		}
		#endregion

		#region HBL - Eagle

		[ExpectNoExceptions]
		public void TestEagleHBL()
		{
			HBLType = "EAG";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEaglePrePrinted()
		{
			HBLType = "EAP";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEAGHBLRail()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading (Rail)");
			HBLType = "EAG";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEAGHBLRoad()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading (Road)");
			HBLType = "EAG";
			RunDocument();
		}
		#endregion

		#region HBL - TAN

		[ExpectNoExceptions]
		public void TestTANHBL()
		{
			HBLType = "TAN";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTANPrePrinted()
		{
			HBLType = "TAP";
			RunDocument();
		}
		#endregion

		#region HBL - Datahawk

		[ExpectNoExceptions]
		public void TestDatahawkHBL()
		{
			HBLType = "DHK";
			RunDocument();
		}

		#endregion

		#region HBL - ITC

		[ExpectNoExceptions]
		public void TestITCAUHBL()
		{
			HBLType = "IAU";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCNZHBL()
		{
			HBLType = "INZ";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCNZPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			HBLType = "INP";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCSingHBL()
		{
			HBLType = "ISI";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCFijiHBL()
		{
			HBLType = "IFJ";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCNoTermsNoLawHBL()
		{
			HBLType = "INN";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestITCPreprintedHBL()
		{
			HBLType = "ITP";
			RunDocument();
		}

		#endregion

		#region HBL - TTC

		[ExpectNoExceptions]
		public void TestTTCHBL()
		{
			HBLType = "TNZ";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTTCNoTermsHBL()
		{
			HBLType = "TTN";
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTTCPreprintedHBL()
		{
			HBLType = "TTP";
			RunDocument();
		}

		#endregion

		#region Implementation

		ZQuery fFilterForMenuItem;
		ZString fHBLType;
		protected override void SetUp()
		{
			base.SetUp();
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
		}
		#endregion
	}
}
