using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	internal class GLMappingReportSetupCSVFlatFileConverter : FlatFileConverter
	{
		public GLMappingReportSetupCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{ }

		public class GLMappingReportSetupConstants
		{
			public const int RecordType = 0;
			public const int Language = 1;
			public const int Country = 5;
			public const int LocalAccountNumber = 2;
			public const int ReportType = 3;
			public const int ReportCategory = 4;
		}

		public void ImportValueObjectFromFlatFileLines(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			MapImport(valueObject, fileLines);
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection gLAccDescriptors = (Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection)valueObject;

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup newGLAccDescriptor = gLAccDescriptors.AddNew();
				ProcessGLReportSetupRow(newGLAccDescriptor, lineInFile);
			}
		}

		void ProcessGLReportSetupRow(Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup header, FlatFileDataRow flatFileRow)
		{
			header.Language = flatFileRow[GLMappingReportSetupConstants.Language];
			header.Country = flatFileRow[GLMappingReportSetupConstants.Country];
			header.LocalAccountNumber = flatFileRow[GLMappingReportSetupConstants.LocalAccountNumber];
			header.ReportType = flatFileRow[GLMappingReportSetupConstants.ReportType];
			header.ReportCategory = flatFileRow[GLMappingReportSetupConstants.ReportCategory];
		}
	}
}
