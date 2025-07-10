using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class BALRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Implementation

		public BALRunDocsTest() : base() { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "BAL"; }
		}

		#endregion

		#region Bill Of Lading

		[ExpectNoExceptions()]
		public void TestBillOfLadingBCL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("BCL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingBAL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("BAL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingDCL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("DCL");
			RunDocument();
		}

		#endregion

		#region Bill Of Lading To Preprinted

		[ExpectNoExceptions()]
		public void TestBillOfLadingPrePrintedBCP()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("BCP");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingPrePrintedBAP()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("BAP");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingPrePrintedDCP()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("DCP");
			RunDocument();
		}

		#endregion
	}
}
