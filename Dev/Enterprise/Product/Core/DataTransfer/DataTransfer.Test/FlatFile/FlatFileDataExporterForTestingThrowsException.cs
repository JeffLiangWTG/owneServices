using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileDataExporterForTestingThrowsException : FlatFileDataExporterForTesting
	{
		public FlatFileDataExporterForTestingThrowsException(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { throw new Exception(); }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CustomFlatFileFormatForTesting(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			throw new NotImplementedException("Test");
		}
	}
}
