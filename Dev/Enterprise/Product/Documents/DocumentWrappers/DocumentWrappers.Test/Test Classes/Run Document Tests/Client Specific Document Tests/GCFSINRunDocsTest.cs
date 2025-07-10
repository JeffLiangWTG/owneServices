using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class GCFSINRunDocsTest : ClientSpecificRunDocsTest
	{
		public GCFSINRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading"); }
			set { }
		}

		protected override ZString ClientName
		{
			get { return "GCF"; }
		}

		[ExpectNoExceptions()]
		public void TestGCFBill()
		{
			SetShipmentHBLType("GCF");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestGLFBill()
		{
			SetShipmentHBLType("GFL");
			RunDocument();
		}
	}
}
