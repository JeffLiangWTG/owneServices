using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class CFSShipmentReceivalRunDocsTest : BaseRunDocumentsTest
	{
		public CFSShipmentReceivalRunDocsTest() { }

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var shipment = Factory.New<CFSShipment>();
				CFSPackLine packLine = shipment.OuterPackLines.AddNew();
				//Shipment.PickupLegs.AddNew();

				return shipment;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CFSShipmentReceival; }
		}

		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		[ExpectNoExceptions]
		public void TestCoverSheet()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cover Sheet");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportCargoReceipt()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Cargo Receipt");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestExportLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Export Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestImportLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Import Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestOnForwardingLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "On Forwarding Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestTranshipmentLabel()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Transhipment Label");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestRequestForService()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Request for Service");
			RunDocument();
		}

		#region Implementation

		ZQuery fFilterForMenuItem;

		#endregion
	}
}
