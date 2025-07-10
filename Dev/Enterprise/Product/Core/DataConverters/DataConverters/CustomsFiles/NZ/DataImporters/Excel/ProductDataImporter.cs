using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel
{
	public class ProductDataImporter : ExcelDataImporter
	{
		public ProductDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords) : base(logger, dataSourcePath, excludeExistingRecords)
		{
		}

		protected override string DataTypeDescription
		{
			get { return "NZ Products"; }
		}

		protected override string FirstLineExclusionString
		{
			get { return "PART"; }
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			var line = GetNextCSVLine();

			PartWriter result = new PartWriter(factory);

			result.PartNumber = GetZStringValueIfExists(line.FieldValues, 0, 30);
			result.Description = GetZStringValueIfExists(line.FieldValues, 1, 80);
			result.LookupCode = GetZStringValueIfExists(line.FieldValues, 2, 35);
			result.SupplierCode = GetZStringValueIfExists(line.FieldValues, 3, 12);
			result.ImporterCode = GetZStringValueIfExists(line.FieldValues, 4, 12);
			result.DefaultStockUnit = GetZStringValueIfExists(line.FieldValues, 5, 3);
			result.Weight = GetZDecimalValueIfExists(line.FieldValues, 6);
			result.WeightUQ = GetZStringValueIfExists(line.FieldValues, 7, 3);
			result.Volume = GetZDecimalValueIfExists(line.FieldValues, 8);
			result.VolumeUQ = GetZStringValueIfExists(line.FieldValues, 9, 3);

			return result;
		}
	}
}
