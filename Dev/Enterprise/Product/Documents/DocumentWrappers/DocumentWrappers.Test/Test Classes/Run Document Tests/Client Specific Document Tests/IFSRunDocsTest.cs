using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class IFSRunDocsTest : ClientSpecificRunDocsTest
	{
		public IFSRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		protected override ZString ClientName
		{
			get { return "IFS"; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestVisionBill()
		{
			SetShipmentHBLType("VIZ");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestVisionPrePrintedHBL()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("VIZ");
			RunDocument();
		}
	}
}
