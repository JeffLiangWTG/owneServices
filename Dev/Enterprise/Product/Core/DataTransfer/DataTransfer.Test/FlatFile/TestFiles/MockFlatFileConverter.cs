using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business.Testing
{
	class MockFlatFileConverter : FlatFileConverter
	{
		public MockFlatFileConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory)
		{
		}

		#region Export

		protected override string FormatLine(FlatFileDataRow row, IFlatFileFormat fileFormat)
		{
			row.SetField(0, "ROFLROFL");
			return base.FormatLine(row, fileFormat);
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			SimpleIValueObject simpleValueObject = (SimpleIValueObject)valueObject;
			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();
			FlatFileDataRow flatFileRow = new FlatFileDataRow(2);

			flatFileRow.SetField(0, simpleValueObject.Value1);
			flatFileRow.SetField(1, simpleValueObject.Value2);

			collection.Add(flatFileRow);

			return collection;
		}

		protected override void CreateHeader(FlatFileDataRowCollection document)
		{
			FlatFileDataRow row = new FlatFileDataRow(2);
			row.SetField(1, "HEADER");
			document.Add(row);
		}

		protected override void CreateFooter(FlatFileDataRowCollection document)
		{
			FlatFileDataRow row = new FlatFileDataRow(2);
			row.SetField(1, "FOOTER");
			document.Add(row);
		}

		#endregion

		#region Import

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			SimpleIValueObject simpleValueObject = (SimpleIValueObject)valueObject;

			foreach (FlatFileDataRow row in fileLines)
			{
				simpleValueObject.ValueCollection1.Add(row.GetField(0));
			}
		}

		#endregion
	}
}
