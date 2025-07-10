using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	public class GLHeaderAndChargeCodeFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestCreateConverter()
		{
			AssertEquals(typeof(GLHeaderAndChargeCodesCSVFlatFileConverter), fImporter.CreateConverter_ForTestOnly(null).GetType());
		}

		public void TestCreateXsd()
		{
			AssertEquals(typeof(Xsd.GLHeadersAndChargeCodes), fImporter.CreateXsd_ForTestOnly().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals(typeof(CsvFlatFileFormat), fImporter.FlatFileFormat_ForTestOnly.GetType());
		}
		public void TestPassingNullAsXSD()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			try
			{
				bool importedResult = fImporter.ExtractToDataAdapter(null, buffer);
				Assert(true);
			}
			catch
			{
				Fail("The null as XML file should be handled. Should be no errors.");
			}
		}

		public void TestReturnValueFromExtractToDataAdapter()
		{
			AssertEquals("Should return false so it doesn't save", false, fImporter.ExtractToDataAdapter(null, new NotificationBuffer()));
		}

		public void TestImportedInvoiceFromExtractToAdapter()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			bool importedResult = fImporter.ExtractToDataAdapter(Value, buffer);

			AssertEquals("Should return true", true, importedResult);

			ZQuery query = new ZQuery(AccGLHeaderSchema.AG_AccountNum, "6666.66.66");

			AccGLHeader importedGLHeader = fImporter.FactoryProviderForTest.Current.LoadTop1<AccGLHeader>(query);
			AssertNotNull("GLHeader should be exist.", importedGLHeader);
			AssertEquals(importedGLHeader.AG_AccountType, Core.Constants.AccountType.Total);
			AssertEquals(importedGLHeader.AG_Description, "Test Account Name");

			query = new ZQuery(AccChargeCodeSchema.AC_Code, "TESTCODE");

			AccChargeCode importedChargeCode = fImporter.FactoryProviderForTest.Current.LoadTop1<AccChargeCode>(query);
			AssertNotNull("ChargeCode should be exist.", importedChargeCode);
			AssertEquals(importedChargeCode.AC_Desc, "Test Description!");
			AssertEquals(importedChargeCode.AC_ChargeType, Core.Constants.ChargeType.Margin);
		}

		#region Implementation

		GLHeaderAndChargeCodeFlatFileDataImporterTestClass fImporter;
		Xsd.GLHeadersAndChargeCodes Value;

		void SetupXmlGLheaderWithCorrectData()
		{
			Xsd.GLHeadersGLHeader gLHeader = Value.SingleGLHeadersAndChargeCodesElement.GLHeaders.AddNew();

			gLHeader.AccNumber = "6666.66.66";
			gLHeader.DebitCredit = Core.Constants.DebitCredit.Debit;
			gLHeader.Description = "Test Account Name";
			gLHeader.AccountType = Core.Constants.AccountType.Total;
			gLHeader.ConsolidationNum = "8888.88.88";
			gLHeader.PercentNum = "8888.88.88";
			gLHeader.AlternateNum = "3333.33.33";
			gLHeader.HeaderDependsOnTotal = "9999.99.99";
			gLHeader.TotalLevel = 15;
			gLHeader.ControlAccount = Core.Constants.BooleanTrueString;
			gLHeader.DisallowDirectPosting = Core.Constants.BooleanFalseString;
			gLHeader.PrintSequence = 5;

			Factory.Save();
		}

		void SetupXmlChargeCodeWithCorrectData()
		{
			Xsd.ChargeCodesChargeCode chargeCode = Value.SingleGLHeadersAndChargeCodesElement.ChargeCodes.AddNew();
			chargeCode.Code = "TESTCODE";
			chargeCode.Description = "Test Description!";
			chargeCode.DepartmentFilterList = "FEA, FIA, FES, FIS, CEA, CIA, CES";
			chargeCode.ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode.MarginPercentage = "50";
			chargeCode.GSTRate = "TESTCODE";
			chargeCode.WithholdingTaxRate = "TESTWHCODE";
			chargeCode.SalesGroup = "SALESTEST";
			chargeCode.ExpenseGroup = "EXPTEST";
			chargeCode.RevenueAccount = "9999.99.99";
			chargeCode.WIPAccount = "1111.11.11";
			chargeCode.CostAccount = "2222.22.22";
			chargeCode.AccrualAccount = "3333.33.33";
			chargeCode.ChargeGroup = "CSH";
			chargeCode.IsGroupageCharge = Core.Constants.BooleanTrueString;
			chargeCode.SubGroup = "STG";
			chargeCode.RateCalculator = "AGY";
			chargeCode.ShowOnQuotation = Core.Constants.BooleanTrueString;
			chargeCode.SuppressOnQuoteIfZero = Core.Constants.BooleanFalseString;
			chargeCode.IATA_ChargeCodeMap = "AT";
		}

		protected override void SetUp()
		{
			base.SetUp();
			fImporter = new GLHeaderAndChargeCodeFlatFileDataImporterTestClass();

			Value = new Xsd.GLHeadersAndChargeCodes();
			SetupXmlGLheaderWithCorrectData();
			SetupXmlChargeCodeWithCorrectData();
		}

		class GLHeaderAndChargeCodeFlatFileDataImporterTestClass : GLHeaderAndChargeCodeFlatFileDataImporter
		{
			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}

			public BusinessObjectFactoryProvider FactoryProviderForTest
			{
				get { return base.FactoryProvider; }
			}
		}
		#endregion
	}
}
