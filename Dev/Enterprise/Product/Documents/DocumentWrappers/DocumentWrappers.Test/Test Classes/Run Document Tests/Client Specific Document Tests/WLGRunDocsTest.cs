using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class WLGRunDocsTest : ClientSpecificRunDocsTest
	{
		public WLGRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "WLG"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestKOLDESBill()
		{
			SetShipmentHBLType("KOL");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestKOEDESBill()
		{
			SetShipmentHBLType("KOE");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBermudaBill()
		{
			SetShipmentHBLType("BEL");
			RunDocument();
		}
	}
}
