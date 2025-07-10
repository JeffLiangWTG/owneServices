using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class MockFlatFileConverterReadTwentyBytes : MockFlatFileConverter
	{
		public MockFlatFileConverterReadTwentyBytes(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
		{
		}

		#region Import

		public override void ImportFlatFile(IValueObject valueObject, IFlatFileFormat fileFormat, System.IO.TextReader flatFileReader)
		{
			base.ImportFlatFile(valueObject, fileFormat, flatFileReader, 20);
		}

		#endregion
	}
}
