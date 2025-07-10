using System.Linq;
using CargoWise.Schema;
using Enterprise.Core.Forms;
using Enterprise.Customs.Forwarding.GUI;
using Enterprise.Customs.Forwarding.GUI.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Forwarding.Module.Testing
{
	abstract class ForwardingShipmentModuleCustomsColumnsProviderAbstractTest : GridCustomColumnsProviderAbstractTest<ForwardingShipment>
	{
		protected ZGridColumnInfo FindColumnByName(DumyZGrid grid, string columnName)
		{
			return (from columnStyleInfo in grid.ColumnStyles.Cast<ZGridColumnInfo>()
					where columnStyleInfo.ColumnName == columnName
					select columnStyleInfo).FirstOrDefault();
		}

		protected override SchemaPKColumn PkColumn => JobShipmentSchema.PK;

		protected override GridCustomColumnsProvider GetGridCustomColumnsProvider()
		{
			return new ForwardingShipmentModuleCustomsColumnsProvider();
		}
	}
}
