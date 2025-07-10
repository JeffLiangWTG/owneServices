using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class KFARunDocsTest : ClientSpecificRunDocsTest
	{
		#region Construction & SetUp

		public KFARunDocsTest()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return fBusinessContext; }
		}

		public override BusinessObject GetBusinessObject
		{
			get { return BusinessObjectForTest; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		protected override ZString ClientName
		{
			get { return "KFA"; }
		}

		BusinessContext fBusinessContext;
		BusinessObject BusinessObjectForTest;
		ZQuery fFilterForMenuItem;

		#endregion

		[ExpectNoExceptions]
		public void TestKFABill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading");
			fBusinessContext = BusinessContext.Shipment;
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("KFA");
			RunDocument();
		}
	}
}
