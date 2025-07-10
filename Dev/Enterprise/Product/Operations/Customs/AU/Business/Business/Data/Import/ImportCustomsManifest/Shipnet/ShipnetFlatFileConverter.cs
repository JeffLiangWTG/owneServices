using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ShipnetFlatFileConverter : FlatFileConverter
	{
		public ShipnetFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			new FileHeaderMapper().Map(fileLines, valueObject);
		}
	}
}
