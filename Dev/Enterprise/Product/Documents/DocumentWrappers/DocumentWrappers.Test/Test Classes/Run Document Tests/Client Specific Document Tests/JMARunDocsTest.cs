using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class JMARunDocsTest : ClientSpecificRunDocsTest
	{
		public JMARunDocsTest() : base() { }

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "JMA"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
		}

		[ExpectNoExceptions()]
		public void TestVisionBill()
		{
			SetShipmentHBLType("JMP");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestVisionPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("JMB");
			RunDocument();
		}
	}
}
