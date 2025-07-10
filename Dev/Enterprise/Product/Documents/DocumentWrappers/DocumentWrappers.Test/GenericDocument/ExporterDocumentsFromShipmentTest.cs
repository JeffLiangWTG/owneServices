using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ExporterDocumentsFromShipmentTest : ExporterDocumentsFromDeclarationTest
	{
		public override void TestShippersLetterOfInstruction()
		{
			var filter = new ZQuery(StmMenuItemSchema.SU_MenuName, "Shippers Letter Of Instruction");
			filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Contains, "Departure");
			RunDocument(filter);
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_JS = shipment.PK;
				return shipment;
			}
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Shipment; }
		}
	}
}
