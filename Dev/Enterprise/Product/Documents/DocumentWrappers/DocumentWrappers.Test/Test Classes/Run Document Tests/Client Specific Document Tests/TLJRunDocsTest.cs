using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class TLJRunDocsTest : ClientSpecificRunDocsTest
	{
		public TLJRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "TLJ"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading To PrePrinted"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestTLJBill()
		{
			SetShipmentHBLType("TRP");
			RunDocument();
		}
	}
}
