using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class ECURunDocsTest : ClientSpecificRunDocsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestECUBill()
		{
			SetShipmentHBLType("ECU");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestECPBill()
		{
			SetShipmentHBLType("ECP");
			RunDocument();
		}

		protected override ZString ClientName
		{
			get { return "ECU"; }
		}
	}
}
