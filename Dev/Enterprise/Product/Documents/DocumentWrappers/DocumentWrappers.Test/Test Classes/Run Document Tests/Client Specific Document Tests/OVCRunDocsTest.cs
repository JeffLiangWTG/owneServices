using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class OVCRunDocsTest : ClientSpecificRunDocsTest
	{
		public OVCRunDocsTest()
			: base()
		{
		}

		public override BusinessContext BusinessContext
		{
			get
			{
				return BusinessContext.Shipment;
			}
		}

		protected override ZString ClientName
		{
			get
			{
				return "OVC";
			}
		}

		public override ZQuery FilterForMenuItem
		{
			get
			{
				return new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			}
			set
			{
			}
		}

		[ExpectNoExceptions()]
		public void TestOVCBill()
		{
			SetShipmentHBLType("OVC");
			RunDocument();
		}
	}
}
