using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.AUS.Testing
{
	public class AUSDataConverterTestClass : DataConverter
	{
		public AUSDataConverterTestClass(BusinessObjectFactory factory) : base(new NotificationBuffer(), factory, ZGuid.Empty, ZGuid.Empty)
		{
		}

		public new void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			base.MapImport(valueObject, fileLines);
		}
	}
}
