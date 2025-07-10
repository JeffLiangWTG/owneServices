using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class DFDSRunDocsTest : ClientSpecificRunDocsTest
	{
		public DFDSRunDocsTest()
			: base()
		{
		}

		#region Overrides
		protected override ZString ClientName
		{
			get { return "DFD"; }
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

		#endregion

		ZQuery fFilterForMenuItem;

		[ExpectNoExceptions()]
		public void TestBIMCOBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading");
			SetShipmentHBLType("BIM");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBIMCOBillToPrePrinted()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading To PrePrinted");
			SetShipmentHBLType("BIM");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestFCRBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("FCR");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestSeawayBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("SWB");
			RunDocument();
		}
	}
}
