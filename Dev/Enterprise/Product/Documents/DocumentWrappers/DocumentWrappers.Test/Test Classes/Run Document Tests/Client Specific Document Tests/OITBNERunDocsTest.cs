using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class OITBNERunDocsTest : ClientSpecificRunDocsTest
	{
		public OITBNERunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		protected override ZString ClientName
		{
			get { return "OIT"; }
		}

		ZQuery fFilterForMenuItem;

		[ExpectNoExceptions()]
		public void TestALSBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			SetShipmentHBLType("ALS");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestALSPrePrintedBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			SetShipmentHBLType("ALS");
			RunDocument();
		}
	}
}
