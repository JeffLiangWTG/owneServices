using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class JASRunDocsTest : ClientSpecificRunDocsTest
	{
		public JASRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "JAS"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestJASBill()
		{
			SetShipmentHBLType("JAS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestJAKBill()
		{
			SetShipmentHBLType("JAK");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestJABBill()
		{
			SetShipmentHBLType("JAB");
			RunDocument();
		}
	}
}
