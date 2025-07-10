using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	internal class GLAccountDescriptorCSVFlatFileConverter : FlatFileConverter
	{
		public GLAccountDescriptorCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		public class GLAccDescriptorConstants
		{
			public const int RecordType = 0;
			public const int Language = 1;
			public const int LocalAccountNumber = 2;
			public const int DebitCredit = 3;
			public const int Description = 4;
			public const int ReportCategory = 5;
			public const int PercentNum = 6;
			public const int ConsolidationNum = 7;
			public const int AlternativeNum = 8;
			public const int HeaderDependsOnTotal = 9;
			public const int CarriedForwardAccount = 10;
			public const int TotalLevel = 11;
			public const int PrintSequence = 12;
			public const int ParentAccount = 13;
			public const int Country = 14;
		}

		public void ImportValueObjectFromFlatFileLines(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			MapImport(valueObject, fileLines);
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection gLAccDescriptors = (Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMappingCollection)valueObject;

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping newGLAccDescriptor = gLAccDescriptors.AddNew();
				ProcessGLAccDescriptorRow(newGLAccDescriptor, lineInFile);
			}
		}

		void ProcessGLAccDescriptorRow(Xsd.GLHeaderMultiLanguageMappingsGLHeaderMultiLanguageMapping header, FlatFileDataRow flatFileRow)
		{
			header.CarriedForwardAccount = flatFileRow[GLAccDescriptorConstants.CarriedForwardAccount];
			header.ParentAccount = flatFileRow[GLAccDescriptorConstants.ParentAccount];
			header.Language = flatFileRow[GLAccDescriptorConstants.Language];
			header.CountryOfCompliance = flatFileRow[GLAccDescriptorConstants.Country];
			header.LocalAccountNumber = flatFileRow[GLAccDescriptorConstants.LocalAccountNumber];
			header.Description = flatFileRow[GLAccDescriptorConstants.Description];
			header.ReportCategory = flatFileRow[GLAccDescriptorConstants.ReportCategory];
			header.PercentNum = flatFileRow[GLAccDescriptorConstants.PercentNum];
			header.ConsolidationNum = flatFileRow[GLAccDescriptorConstants.ConsolidationNum];
			header.AlternativeNum = flatFileRow[GLAccDescriptorConstants.AlternativeNum];
			header.HeaderDependsOnTotal = flatFileRow[GLAccDescriptorConstants.HeaderDependsOnTotal];
			header.TotalLevel = flatFileRow.GetFieldAsZInt(GLAccDescriptorConstants.TotalLevel);
			header.PrintSequence = flatFileRow.GetFieldAsZInt(GLAccDescriptorConstants.PrintSequence);
			header.DebitCredit = flatFileRow[GLAccDescriptorConstants.DebitCredit];
		}
	}
}
