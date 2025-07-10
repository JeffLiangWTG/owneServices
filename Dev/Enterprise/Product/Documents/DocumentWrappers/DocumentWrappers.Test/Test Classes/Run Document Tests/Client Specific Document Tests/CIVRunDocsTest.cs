using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class CIVRunDocsTest : ClientSpecificRunDocsTest
	{
		public CIVRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "CIV"; }
		}

		[ExpectNoExceptions()]
		public void TestMTIBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("ATH");
			RunDocument();
		}
	}
}
