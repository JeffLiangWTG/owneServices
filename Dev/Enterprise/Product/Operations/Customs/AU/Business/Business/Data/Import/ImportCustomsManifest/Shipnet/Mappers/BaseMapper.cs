using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseMapper
	{
		public abstract void Map(FlatFileDataRowCollection rows, IValueObject value);

		internal bool IsRowType(FlatFileDataRow row, ZString recordID) => row[ShipnetConstants.Common.RecordID] == recordID;
	}
}
