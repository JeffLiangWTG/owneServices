using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class NYKRunDocsTest : ClientSpecificRunDocsTest
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}

		#region FilterForMenuItem

		public override ZQuery FilterForMenuItem
		{
			get
			{
				if (fFilterForMenuItem == null)
				{
					fFilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
				}
				return fFilterForMenuItem;
			}
			set
			{
				fFilterForMenuItem = value;
			}
		}

		ZQuery fFilterForMenuItem;

		#endregion

		[ExpectNoExceptions()]
		public void TestDWPBill()
		{
			SetShipmentHBLType("DWP");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestDWBBill()
		{
			SetShipmentHBLType("DWB");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestDWABill()
		{
			SetShipmentHBLType("DWA");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestDWUBill()
		{
			SetShipmentHBLType("DWU");
			RunDocument();
		}

		protected override ZString ClientName
		{
			get { return "NYK"; }
		}
	}
}
