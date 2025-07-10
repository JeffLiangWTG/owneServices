using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class WFNRunDocsTest : ClientSpecificRunDocsTest
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
		public void TestWFNBill()
		{
			SetShipmentHBLType("WFN");
			RunDocument();
		}

		protected override ZString ClientName
		{
			get { return "WFN"; }
		}
	}
}
