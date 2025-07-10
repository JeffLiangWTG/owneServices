using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	internal class ChargeCodeCSVFlatFileConverter : FlatFileConverter
	{
		public ChargeCodeCSVFlatFileConverter(INotifications notify, BusinessObjectFactory factory)
			: base(notify, factory)
		{
		}

		public static class ChargeCodeConstants
		{
			public const int RecordType = 0;
			public const int Code = 1;
			public const int Description = 2;
			public const int DepartmentFilterList = 3;
			public const int ChargeType = 4;
			public const int MarginPercentage = 5;
			public const int GSTRate = 6;
			public const int WithHoldingTaxRate = 7;
			public const int SalesGroup = 8;
			public const int ExpenseGroup = 9;
			public const int RevenueAccount = 10;
			public const int WIPAccount = 11;
			public const int CostAccount = 12;
			public const int AccrualAccount = 13;
			public const int ChargeGroup = 14;
			public const int IsGroupageCharge = 15;
			public const int SubGroup = 16;
			public const int RateCalculator = 17;
			public const int ShowOnQuotation = 18;
			public const int SuppressOnQuoteIfZero = 19;
			public const int IATA_ChargeCodeMap = 20;
			public const int IsGlobal = 21;
			public const int GovtChargeCode = 22;
		}

		public void ImportValueObjectFromFlatFileLines(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			MapImport(valueObject, fileLines);
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.ChargeCodesChargeCodeCollection chargeCodes = (Xsd.ChargeCodesChargeCodeCollection)valueObject;

			foreach (FlatFileDataRow lineInFile in fileLines)
			{
				Xsd.ChargeCodesChargeCode newChargeCode = chargeCodes.AddNew();
				ProcessChargeHeaderRow(newChargeCode, lineInFile);
			}
		}

		void ProcessChargeHeaderRow(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			SetCode(chargeCode, flatFileRow);
			SetDescription(chargeCode, flatFileRow);
			SetDepartmentFilterList(chargeCode, flatFileRow);
			SetChargeType(chargeCode, flatFileRow);
			SetMarginPercentage(chargeCode, flatFileRow);
			SetGSTRate(chargeCode, flatFileRow);
			SetWithHoldingTaxRate(chargeCode, flatFileRow);
			SetSalesGroup(chargeCode, flatFileRow);
			SetExpenseGroup(chargeCode, flatFileRow);
			SetRevenueAccount(chargeCode, flatFileRow);
			SetWIPAccount(chargeCode, flatFileRow);
			SetCostAccount(chargeCode, flatFileRow);
			SetAccrualAccount(chargeCode, flatFileRow);
			SetChargeGroup(chargeCode, flatFileRow);
			SetIsGroupageCharge(chargeCode, flatFileRow);
			SetSubGroup(chargeCode, flatFileRow);
			SetRateCalculator(chargeCode, flatFileRow);
			SetShowOnQuotation(chargeCode, flatFileRow);
			SetSuppressOnQuoteIfZero(chargeCode, flatFileRow);
			SetIATA_ChargeCodeMap(chargeCode, flatFileRow);
			SetIsGlobalMap(chargeCode, flatFileRow);
			SetGovtChargeCode(chargeCode, flatFileRow);
		}

		void SetCode(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.Code = flatFileRow[ChargeCodeConstants.Code];
		}

		void SetDescription(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.Description = flatFileRow[ChargeCodeConstants.Description];
		}

		void SetDepartmentFilterList(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.DepartmentFilterList = flatFileRow[ChargeCodeConstants.DepartmentFilterList];
		}

		void SetChargeType(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.ChargeType = flatFileRow[ChargeCodeConstants.ChargeType];
		}

		void SetMarginPercentage(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.MarginPercentage = flatFileRow[ChargeCodeConstants.MarginPercentage];
		}

		void SetGSTRate(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.GSTRate = flatFileRow[ChargeCodeConstants.GSTRate];
		}

		void SetWithHoldingTaxRate(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.WithholdingTaxRate = flatFileRow[ChargeCodeConstants.WithHoldingTaxRate];
		}

		void SetSalesGroup(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.SalesGroup = flatFileRow[ChargeCodeConstants.SalesGroup];
		}

		void SetExpenseGroup(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.ExpenseGroup = flatFileRow[ChargeCodeConstants.ExpenseGroup];
		}

		void SetRevenueAccount(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.RevenueAccount = flatFileRow[ChargeCodeConstants.RevenueAccount];
		}

		void SetWIPAccount(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.WIPAccount = flatFileRow[ChargeCodeConstants.WIPAccount];
		}

		void SetCostAccount(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.CostAccount = flatFileRow[ChargeCodeConstants.CostAccount];
		}

		void SetAccrualAccount(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.AccrualAccount = flatFileRow[ChargeCodeConstants.AccrualAccount];
		}

		void SetChargeGroup(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.ChargeGroup = flatFileRow[ChargeCodeConstants.ChargeGroup];
		}

		void SetIsGroupageCharge(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.IsGroupageCharge = flatFileRow[ChargeCodeConstants.IsGroupageCharge];
		}

		void SetSubGroup(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.SubGroup = flatFileRow[ChargeCodeConstants.SubGroup];
		}

		void SetRateCalculator(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.RateCalculator = flatFileRow[ChargeCodeConstants.RateCalculator];
		}

		void SetShowOnQuotation(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.ShowOnQuotation = flatFileRow[ChargeCodeConstants.ShowOnQuotation];
		}

		void SetSuppressOnQuoteIfZero(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.SuppressOnQuoteIfZero = flatFileRow[ChargeCodeConstants.SuppressOnQuoteIfZero];
		}

		void SetIATA_ChargeCodeMap(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.IATA_ChargeCodeMap = flatFileRow[ChargeCodeConstants.IATA_ChargeCodeMap];
		}

		void SetIsGlobalMap(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.IsGlobal = flatFileRow[ChargeCodeConstants.IsGlobal];
		}

		void SetGovtChargeCode(Xsd.ChargeCodesChargeCode chargeCode, FlatFileDataRow flatFileRow)
		{
			chargeCode.GovtChargeCode = AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value ? flatFileRow[ChargeCodeConstants.GovtChargeCode] : ZString.Empty;
		}
	}
}
