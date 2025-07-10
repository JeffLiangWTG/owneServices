using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	internal class GLHeaderCSVFlatFileConverter : FlatFileConverter
	{
		public GLHeaderCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		public class GLHeaderConstants
		{
			public const int RecordType = 0;
			public const int AccountNumber = 1;
			public const int DebitCredit = 2;
			public const int Description = 3;
			public const int AccountType = 4;
			public const int PercentNum = 5;
			public const int ConsolidationNum = 6;
			public const int AlternateNum = 7;
			public const int HeaderDependsOnTotal = 8;
			public const int TotalLevel = 9;
			public const int ControlAccount = 10;
			public const int DisallowDirectPosting = 11;
			public const int PrintSequence = 12;
			public const int Section = 13;
			public const int SubAccountType = 14;
			public const int IsSubAccountMandatory = 15;
			public const int CashFlowType = 16;
			public const int CompanyFilterList = 17;
			public const int StatisticalUnits = 18;
		}

		public void ImportValueObjectFromFlatFileLines(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			MapImport(valueObject, fileLines);
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.GLHeadersGLHeaderCollection headers = (Xsd.GLHeadersGLHeaderCollection)valueObject;

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				Xsd.GLHeadersGLHeader newHeader = headers.AddNew();
				ProcessGLHeaderRow(newHeader, lineInFile);
			}
		}

		void ProcessGLHeaderRow(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			SetAccountNumber(header, flatFileRow);
			SetDebitCredit(header, flatFileRow);
			SetDescription(header, flatFileRow);
			SetAccountType(header, flatFileRow);
			SetPercentNum(header, flatFileRow);
			SetConsolidationNum(header, flatFileRow);
			SetAlternateNum(header, flatFileRow);
			SetHeaderDependsOnTotal(header, flatFileRow);
			SetTotalLevel(header, flatFileRow);
			SetControlAccount(header, flatFileRow);
			SetDisallowDirectPosting(header, flatFileRow);
			SetPrintSequence(header, flatFileRow);
			SetDebitCredit(header, flatFileRow);
			SetSection(header, flatFileRow);
			SetSubAccountTypeDetails(header, flatFileRow);
			SetCashFlowType(header, flatFileRow);
			SetCompanyFilterList(header, flatFileRow);
			SetStatisticalUnits(header, flatFileRow);
		}

		void SetStatisticalUnits(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.StatisticalUnits = flatFileRow[GLHeaderConstants.StatisticalUnits];
		}

		void SetCompanyFilterList(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.CompanyFilterList = flatFileRow[GLHeaderConstants.CompanyFilterList];
		}

		void SetCashFlowType(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.CashFlowType = flatFileRow[GLHeaderConstants.CashFlowType];
		}

		void SetSubAccountTypeDetails(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.SubAccountType = flatFileRow[GLHeaderConstants.SubAccountType];
			header.IsSubAccountMandatory = flatFileRow[GLHeaderConstants.IsSubAccountMandatory];
		}

		void SetSection(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.Section = flatFileRow[GLHeaderConstants.Section];
		}

		void SetAccountNumber(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.AccNumber = flatFileRow[GLHeaderConstants.AccountNumber];
		}

		void SetDescription(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.Description = flatFileRow[GLHeaderConstants.Description];
		}

		void SetAccountType(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.AccountType = flatFileRow[GLHeaderConstants.AccountType];
		}

		void SetPercentNum(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.PercentNum = flatFileRow[GLHeaderConstants.PercentNum];
		}

		void SetConsolidationNum(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.ConsolidationNum = flatFileRow[GLHeaderConstants.ConsolidationNum];
		}

		void SetAlternateNum(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.AlternateNum = flatFileRow[GLHeaderConstants.AlternateNum];
		}

		void SetHeaderDependsOnTotal(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.HeaderDependsOnTotal = flatFileRow[GLHeaderConstants.HeaderDependsOnTotal];
		}

		void SetTotalLevel(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.TotalLevel = flatFileRow.GetFieldAsZInt(GLHeaderConstants.TotalLevel);
		}

		void SetControlAccount(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.ControlAccount = flatFileRow[GLHeaderConstants.ControlAccount];
		}

		void SetDisallowDirectPosting(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.DisallowDirectPosting = flatFileRow[GLHeaderConstants.DisallowDirectPosting];
		}

		void SetPrintSequence(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.PrintSequence = flatFileRow.GetFieldAsZInt(GLHeaderConstants.PrintSequence);
		}

		void SetDebitCredit(Xsd.GLHeadersGLHeader header, FlatFileDataRow flatFileRow)
		{
			header.DebitCredit = flatFileRow[GLHeaderConstants.DebitCredit];
		}
	}
}
