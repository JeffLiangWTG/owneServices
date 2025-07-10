using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.CustomsFiles.NZ.DataImporters.Excel
{
	public class ClassificationDataImporter : ExcelDataImporter
	{
		public ClassificationDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool excludeExistingRecords) : base(logger, dataSourcePath, excludeExistingRecords)
		{
		}

		protected override string DataTypeDescription
		{
			get { return "NZ Classifications"; }
		}

		protected override string FirstLineExclusionString
		{
			get { return "LOOKUP"; }
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			var line = GetNextCSVLine();

			ClassificationWriter result = new ClassificationWriter(factory);

			result.LookupCode = GetZStringValueIfExists(line.FieldValues, 0, 35);
			result.Description = GetZStringValueIfExists(line.FieldValues, 1, 80);
			result.TariffCode = GetZStringValueIfExists(line.FieldValues, 2, 14);
			result.ClassificationType = Customs.Common.ClassificationType.Both;
			result.ConcessionCode = GetZStringValueIfExists(line.FieldValues, 3, 9);
			result.PartsOfTariffCode = GetZStringValueIfExists(line.FieldValues, 4, 14);

			result.PermitCode1 = GetZStringValueIfExists(line.FieldValues, 5, 3);
			result.PermitNumber1 = GetZStringValueIfExists(line.FieldValues, 6, 12);
			result.PermitCode2 = GetZStringValueIfExists(line.FieldValues, 7, 3);
			result.PermitNumber2 = GetZStringValueIfExists(line.FieldValues, 8, 12);
			result.PermitCode3 = GetZStringValueIfExists(line.FieldValues, 9, 3);
			result.PermitNumber3 = GetZStringValueIfExists(line.FieldValues, 10, 12);

			result.ProhibitedCode1 = GetZStringValueIfExists(line.FieldValues, 11, 3);
			result.ProhibitedCode2 = GetZStringValueIfExists(line.FieldValues, 12, 3);

			return result;
		}
	}
}
