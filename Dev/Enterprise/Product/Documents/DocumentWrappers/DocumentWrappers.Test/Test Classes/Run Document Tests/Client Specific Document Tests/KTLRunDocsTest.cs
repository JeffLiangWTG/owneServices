using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class KTLRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Construction & SetUp

		public KTLRunDocsTest()
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return businessContext; }
		}

		public override BusinessObject GetBusinessObject
		{
			get { return businessObjectForTest; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return filterForMenuItem; }
			set { filterForMenuItem = value; }
		}

		protected override ZString ClientName
		{
			get { return "KTL"; }
		}

		BusinessContext businessContext;
		BusinessObject businessObjectForTest;
		ZQuery filterForMenuItem;

		#endregion

		[ExpectNoExceptions]
		public void TestKTLBill()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill of Lading");
			businessContext = BusinessContext.Shipment;
			businessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("KTL");
			RunDocument();
		}
	}
}
