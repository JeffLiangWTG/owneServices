using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	internal class GLHeaderAndChargeCodesCSVFlatFileConverter : FlatFileConverter
	{
		public const string GLAccountRowType = "GLACCOUNT";
		public const string ChargeCodeRowType = "CHARGECODE";
		public const string GLMappingRowType = "GLMAPPING";
		public const string ReportMappingRowType = "REPSETUP";

		public GLHeaderAndChargeCodesCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{ }

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.GLHeadersAndChargeCodes gLHeadersAndChargeCodes = (Xsd.GLHeadersAndChargeCodes)valueObject;

			FlatFileDataRowCollection gLHeaderFileLines = new FlatFileDataRowCollection();
			FlatFileDataRowCollection chargeCodeFileLines = new FlatFileDataRowCollection();

			FlatFileDataRowCollection gLMappingFileLines = new FlatFileDataRowCollection();
			FlatFileDataRowCollection gLMappingReportSetupLines = new FlatFileDataRowCollection();

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				if (lineInFile[0] == GLAccountRowType)
				{
					gLHeaderFileLines.Add(lineInFile);
				}
				else if (lineInFile[0] == ChargeCodeRowType)
				{
					chargeCodeFileLines.Add(lineInFile);
				}
				else if (lineInFile[0] == GLMappingRowType)
				{
					gLMappingFileLines.Add(lineInFile);
				}
				else if (lineInFile[0] == ReportMappingRowType)
				{
					gLMappingReportSetupLines.Add(lineInFile);
				}
				else
				{
					ProcessUnRecognisedRow(lineInFile);
				}
			}

			if (gLHeaderFileLines.Count > 0)
			{
				Xsd.GLHeadersGLHeaderCollection headers = gLHeadersAndChargeCodes.SingleGLHeadersAndChargeCodesElement.GLHeaders;
				GLHeaderCSVConverter.ImportValueObjectFromFlatFileLines(headers, gLHeaderFileLines);
			}

			if (chargeCodeFileLines.Count > 0)
			{
				Xsd.ChargeCodesChargeCodeCollection chargeCodes = gLHeadersAndChargeCodes.SingleGLHeadersAndChargeCodesElement.ChargeCodes;
				ChargeCodeCSVConverter.ImportValueObjectFromFlatFileLines(chargeCodes, chargeCodeFileLines);
			}

			if (gLMappingFileLines.Count > 0)
			{
				Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection gLMappings = gLHeadersAndChargeCodes.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageMappings;
				GLAccDescriptorCSVFlatFileConverter.ImportValueObjectFromFlatFileLines(gLMappings, gLMappingFileLines);
			}

			if (gLMappingReportSetupLines.Count > 0)
			{
				Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetupCollection gLMappingReportSetups = gLHeadersAndChargeCodes.SingleGLHeadersAndChargeCodesElement.GLHeaderMultiLanguageReportSetups;
				GLMappingReportSetupCSVFlatFileConverter.ImportValueObjectFromFlatFileLines(gLMappingReportSetups, gLMappingReportSetupLines);
			}
		}

		ChargeCodeCSVFlatFileConverter ChargeCodeCSVConverter
		{
			get
			{
				return fChargeCodeCSVConverter ??
						(fChargeCodeCSVConverter = new ChargeCodeCSVFlatFileConverter(Notification, Factory));
			}
		}

		ChargeCodeCSVFlatFileConverter fChargeCodeCSVConverter;

		GLHeaderCSVFlatFileConverter GLHeaderCSVConverter
		{
			get { return fGLHeaderCSVConverter ?? (fGLHeaderCSVConverter = new GLHeaderCSVFlatFileConverter(Notification, Factory)); }
		}

		GLHeaderCSVFlatFileConverter fGLHeaderCSVConverter;

		GLMappingReportSetupCSVFlatFileConverter GLMappingReportSetupCSVFlatFileConverter
		{
			get
			{
				return fGLMappingReportSetupCSVFlatFileConverter ??
						(fGLMappingReportSetupCSVFlatFileConverter =
						new GLMappingReportSetupCSVFlatFileConverter(Notification, Factory));
			}
		}

		GLMappingReportSetupCSVFlatFileConverter fGLMappingReportSetupCSVFlatFileConverter;

		GLAccountDescriptorCSVFlatFileConverter GLAccDescriptorCSVFlatFileConverter
		{
			get
			{
				return fGLAccDescriptorCSVFlatFileConverter ??
						(fGLAccDescriptorCSVFlatFileConverter = new GLAccountDescriptorCSVFlatFileConverter(Notification, Factory));
			}
		}

		GLAccountDescriptorCSVFlatFileConverter fGLAccDescriptorCSVFlatFileConverter;

		void ProcessUnRecognisedRow(FlatFileDataRow row)
		{
			string message = Res.GetString("3A804CA3-19B5-4508-8D9D-5A29A213EF5F",
				"Unrecognized row type detected. Only '{0}', '{1}', '{2}' and '{3}' row types are used for import. The following row will be ignored: '{4}'.",
				GLAccountRowType, ChargeCodeRowType, GLMappingRowType, ReportMappingRowType, row.GetField(0)) + System.Environment.NewLine;

			if ((row.FieldCount < 4) || (row.GetField(0) == GLAccountRowType && row.FieldCount < 13)
				|| (row.GetField(0) == GLMappingRowType && row.FieldCount < 14) || (row.GetField(0) == ChargeCodeRowType && row.FieldCount < 20)
			)
			{
				message = Res.GetString("9A1A3783-1ADA-4D8A-95F3-D561CB1E741B", "The CSV file format is incorrect! Only accepts a comma-delimited CSV file. Please fix and re-import.");
			}

			if (Notification != null)
			{
				Notification.Notify(new WarningNotification(WarningType.Warning, message));
			}
		}
	}
}
