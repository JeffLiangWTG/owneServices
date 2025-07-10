using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class VLSRunDocsTest : ClientSpecificRunDocsTest
	{
		public VLSRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "VLS"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestAFSBill()
		{
			SetShipmentHBLType("AFS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestDCLBill()
		{
			SetShipmentHBLType("DCL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestEFSBill()
		{
			SetShipmentHBLType("EFS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestGCSBill()
		{
			SetShipmentHBLType("GCS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestVLSBill()
		{
			SetShipmentHBLType("VLS");
			RunDocument();
		}
	}
}
