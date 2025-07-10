using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.ReportingBook.AlternateGLAccountWithAttribute.Testing
{
	public class AlternateGLAccountWithAttributeFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestCreateConverter()
		{
			AssertEquals(typeof(AlternateGLAccountWithAttributeCSVFlatFileConverter), fImporter.CreateConverter_ForTestOnly().GetType());
		}

		public void TestCreateXsd()
		{
			AssertEquals(typeof(Xsd.AlternateGLAccounts), fImporter.CreateXsd_ForTestOnly().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals(typeof(CsvFlatFileFormat), fImporter.FlatFileFormat_ForTestOnly().GetType());
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
			Assert("Should return false so it doesn't save", !fImporter.ExtractToDataAdapter(null, new NotificationBuffer()));
			Assert("Should return false so it doesn't save", fImporter.ExtractToDataAdapter(Value, new NotificationBuffer()));
		}

		public void TestImportedAlternateGLAccountFromExtractToAdapter()
		{
			var buffer = new NotificationBuffer();
			fImporter.ExtractToDataAdapter(Value, buffer);

			var query = new ZQuery(AccAlternateGLAccountSchema.AGA_AccountNum, "31101010");

			var importedAccAlternateGLAccount = fImporter.FactoryProviderForTest.Current.LoadTop1<AccAlternateGLAccount>(query);
			AssertNotNull("AccAlternateGLAccount should be exist.", importedAccAlternateGLAccount);
			AssertEquals(importedAccAlternateGLAccount.AGA_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
			AssertEquals(importedAccAlternateGLAccount.AGA_Description, "test");

			query = new ZQuery(AccAlternateGLAccountAttributeSchema.AAA_Value, "LOC");

			var importedAttribute = fImporter.FactoryProviderForTest.Current.LoadTop1<AccAlternateGLAccountAttribute>(query);
			AssertNotNull("Attribute should be exist.", importedAttribute);
			AssertEquals(importedAttribute.AAA_Attribute, "LFO");
		}

		#region Implementation

		AlternateGLAccountWithAttributeFlatFileDataImporterTestClass fImporter;
		Xsd.AlternateGLAccounts Value;

		void SetupXmlData()
		{
			var chart = Creator.CreateAlternateChart("111", "Description", true, true, BalanceSheetStyleCode.EAL);
			Creator.CreateAccAlternateChartFormat(chart, 1, "9999", "desc", "-");
			Creator.CreateAccAlternateChartFormat(chart, 2, "99", "desc", "");
			GLHeader = Creator.CreateAccGLHeader("3410.13.21", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Creator.CreateAlternateGlAccountForCSVImport(Value.AlternateGLAccount, "3410.13.21", "31101010", Core.Constants.AccountType.BalanceSheetAccount, chart.AAC_Code, "test", Core.Constants.DebitCredit.Debit, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0, 5, lfoAttrValue: "LOC");
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			fImporter = new AlternateGLAccountWithAttributeFlatFileDataImporterTestClass();

			Value = new Xsd.AlternateGLAccounts();
			SetupXmlData();
		}

		AccGLHeader GLHeader;
		TestObjectCreator Creator;

		class AlternateGLAccountWithAttributeFlatFileDataImporterTestClass : AlternateGLAccountWithAttributeFlatFileDataImporter
		{
			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}

			public BusinessObjectFactoryProvider FactoryProviderForTest
			{
				get { return base.FactoryProvider; }
			}

			public IFlatFileConverter CreateConverter_ForTestOnly()
			{
				return CreateConverter(null);
			}

			public IValueObject CreateXsd_ForTestOnly()
			{
				return CreateXsd();
			}

			public IFlatFileFormat FlatFileFormat_ForTestOnly()
			{
				return FlatFileFormat;
			}
		}
		#endregion
	}
}
