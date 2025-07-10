using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class IJSRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Implementation

		public IJSRunDocsTest()
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
				return "IJS";
			}
		}

		BusinessObject BusinessObjectForTest;
		public override BusinessObject GetBusinessObject
		{
			get
			{
				return BusinessObjectForTest;
			}
		}

		#endregion

		[ExpectNoExceptions()]
		public void TestBillOfLading()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("IJ5");
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("IJ6");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingPrePrinted()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("IJ6");
			RunDocument();
		}
	}
}
