using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.WCB.Testing
{
	public class DaimlerDataConverterTestClass : DaimlerDataConverter
	{
		public DaimlerDataConverterTestClass(BusinessObjectFactory factory) : base(new NotificationBuffer(), factory)
		{
		}

		public new void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			base.MapImport(valueObject, fileLines);
		}
	}
}
