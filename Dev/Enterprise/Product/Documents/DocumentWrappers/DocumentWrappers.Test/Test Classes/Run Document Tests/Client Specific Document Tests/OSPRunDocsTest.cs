using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class OSPRunDocsTest : ClientSpecificRunDocsTest
	{
		public OSPRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "OSP"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestOSPBill()
		{
			SetShipmentHBLType("OSP");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestOBXBill()
		{
			SetShipmentHBLType("OBX");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestCHLBill()
		{
			SetShipmentHBLType("CHL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestHIPBill()
		{
			SetShipmentHBLType("HIP");
			RunDocument();
		}
	}
}
