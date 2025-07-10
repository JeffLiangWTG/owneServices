using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class FBVRunDocsTest : ClientSpecificRunDocsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted"); }
			set { }
		}

		[ExpectNoExceptions()]
		public void TestFBVBill()
		{
			SetShipmentHBLType("FBV");
			RunDocument();
		}

		protected override ZString ClientName
		{
			get { return "FBV"; }
		}
	}
}
