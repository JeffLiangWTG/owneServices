using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class MFIRunDocsTest : ClientSpecificRunDocsTest
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
		public void TestMFIBill()
		{
			SetShipmentHBLType("MFI");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestMNZBill()
		{
			SetShipmentHBLType("MNZ");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestMEFBill()
		{
			SetShipmentHBLType("MEF");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestMUSBill()
		{
			SetShipmentHBLType("MUS");
			RunDocument();
		}

		protected override ZString ClientName
		{
			get { return "MFI"; }
		}
	}
}
