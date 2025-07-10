using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	sealed class BDPRunDocsTest : ClientSpecificRunDocsTest
	{
		#region Implementation

		public BDPRunDocsTest()
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
				return "BDP";
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
			SetShipmentHBLType("SUB");
			RunDocument();

			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading");
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("SUP");
			RunDocument();
		}

		[ExpectNoExceptions()]
		public void TestBillOfLadingPrePrinted()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Bill Of Lading To Preprinted");
			BusinessObjectForTest = Factory.New<ForwardingShipment>();
			SetShipmentHBLType("SUP");
			RunDocument();
		}
	}
}
