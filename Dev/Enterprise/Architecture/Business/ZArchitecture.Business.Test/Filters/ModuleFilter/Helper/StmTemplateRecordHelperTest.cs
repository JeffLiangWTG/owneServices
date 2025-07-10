using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(ShipmentXQueryPaths))]

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmTemplateRecordHelperTest : TestCaseWithFactory
	{
		public void TestGenerateXQuery()
		{
			using (XQueryFilterHelper.SetNamespaceTemp(UniversalXmlInfo.Namespace_2011_11))
			{
				var query = XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, "AIR"), new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength));
				AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2011/11\"; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') = 'AIR'",
					query.LiteralTextADO);
			}

			using (XQueryFilterHelper.SetNamespaceTemp(UniversalXmlInfo.Namespace_2012_11))
			{
				var query = XQueryFilterHelper.GenerateXQuery((column) => new ZQuery(column, "AIR"), new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, JobShipmentSchema.JS_TransportMode.MaxLength));
				AssertEquals("STR_Data.value('declare default element namespace \"http://www.cargowise.com/Schemas/Universal/2012/11\"; (UniversalShipment/Shipment/TransportMode)[1]', 'varchar(3)') = 'AIR'",
					query.LiteralTextADO);
			}
		}

		public void TestGenerateXQuery_2()
		{
			using (XQueryFilterHelper.SetNamespaceTemp(UniversalXmlInfo.Namespace_2011_11))
			{
				var xQueryFilterInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, ShipmentXQueryPaths.ShipmentType, JobShipmentSchema.JS_TransportMode.MaxLength, JobShipmentSchema.JS_ShipmentType.MaxLength);
				var query = XQueryFilterHelper.GenerateXQuery((column1, column2) => new ZQuery(column1, "AIR").AddToFilter(new ZQuery(column2, "STD")), xQueryFilterInfo);

				var expectedQueryText = @"STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/TransportMode/Code)[1]', 'varchar(3)') = 'AIR' and STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2011/11""; (UniversalShipment/Shipment/ShipmentType/Code)[1]', 'varchar(3)') = 'STD'";
				AssertEquals(expectedQueryText, query.LiteralTextADO);
			}

			using (XQueryFilterHelper.SetNamespaceTemp(UniversalXmlInfo.Namespace_2012_11))
			{
				var xQueryFilterInfo = new XQueryFilterInfo(ShipmentXQueryPaths.TransportMode, ShipmentXQueryPaths.ShipmentType, JobShipmentSchema.JS_TransportMode.MaxLength, JobShipmentSchema.JS_ShipmentType.MaxLength);
				var query = XQueryFilterHelper.GenerateXQuery((column1, column2) => new ZQuery(column1, "AIR").AddToFilter(new ZQuery(column2, "STD")), xQueryFilterInfo);

				var expectedQueryText = @"STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2012/11""; (UniversalShipment/Shipment/TransportMode)[1]', 'varchar(3)') = 'AIR' and STR_Data.value('declare default element namespace ""http://www.cargowise.com/Schemas/Universal/2012/11""; (UniversalShipment/Shipment/ShipmentType)[1]', 'varchar(3)') = 'STD'";
				AssertEquals(expectedQueryText, query.LiteralTextADO);
			}
		}
	}
}
