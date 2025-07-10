using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	class CusStatement_CusStatementHeader_Test : TestCaseWithFactory
	{
		public void TestImportXml()
		{
			var manager = new ImportServiceManagerForTesting();
			var statement = GetSourceStatement();
			var importer = statement.Importer;
			var element = SerialiseToXml(statement);
			var rawXml = element.ToString();
			ClearData(statement);
			Factory.Save();
			var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(rawXml));
			manager.ImportService.Import(memoryStream);

			var logs = manager.GetLogs();

			string expectedLog = "Statements are generated when system receives statement messages from Customs. System will not generate statements from Native Xml messages.";
			AssertContains("NoImportReason is shown to users", expectedLog, logs);

			var queryStatementNumber = new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, statementNumberFromXml);
			var newFactory = new BusinessObjectFactory();
			var header = newFactory.LoadTop1<CusStatementHeader>(queryStatementNumber);

			AssertEquals("Import has not proceeded and nothing is imported from xml", "", header.B2_AccountNo);

			var lineGroups = Factory.Load<CusStatementLineGroup>(new ZQuery(CusStatementLineGroupSchema.B10_B2, header.PK));
			AssertEquals("Should import the line group.", 1, lineGroups.Length);
			AssertEquals("Should import the line group.", "18121933RM0001", lineGroups[0].B10_ImporterCustomsID);

			var financialDetails = Factory.Load<CusStatementLineGroupFinancialDetail>(new ZQuery(CusStatementLineGroupFinancialDetailSchema.B11_B10, lineGroups[0].PK));
			AssertEquals("Should import the line group financial detail.", 1, financialDetails.Length);
			AssertEquals("Should import the line group financial detail.", "AAA", financialDetails[0].B11_Type);
			AssertEquals("Should import the line group financial detail.", 101.03m, financialDetails[0].B11_Amount);
		}

		public void TestExportToXml()
		{
			var actualMessage = string.Empty;
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			var statement = GetSourceStatement();

			using (var dataStream = xmlSerializer.SerializeToStream(statement))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}

			var element = XElement.Load(new StringReader(actualMessage));

			CombineAssertions(delegate
			{
				AssertEquals("Should have DailyStatementHeaderCollection.", 1, element.Descendants().Count(e => e.Name.LocalName == "DailyStatementHeaderCollection"));
				AssertEquals("Should have 2 DailyStatement.", 2, element.Descendants().Count(e => e.Name.LocalName == "DailyStatementHeader"));

				AssertEquals("Should have CusStatementLineCollection", 2, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLineCollection"));
				AssertEquals("Should have 3 statement line", 3, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLine"));

				AssertEquals("Should have 3 CusStatementLineGroupCollection", 3, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLineGroupCollection"));
				AssertEquals("Should have 3 statement line group", 3, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLineGroup"));

				AssertEquals("Should have 3 CusStatementLineGroupFinancialDetailCollection", 3, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLineGroupFinancialDetailCollection"));
				AssertEquals("Should have 3 statement line group financial detail", 3, element.Descendants().Count(e => e.Name.LocalName == "CusStatementLineGroupFinancialDetail"));
			});
		}

		public void TestProductSchemaValidity()
		{
			var statement = GetSourceStatement();
			SchemaValidationTest.CheckSchemaValidity(statement, "CusStatement");
		}

		void ClearData(CusStatementHeader header)
		{
			header.B2_AccountNo = "";
			header.B2_BranchDesignation = "";
			header.B2_CheckNo = "";
			header.B2_DueDate = ZDateTime.Empty;
			header.B2_EntryFilerCode = "";
			header.B2_ImporterCustomsID = "";
			header.B2_OH_Importer = ZGuid.Empty;
			header.B2_PaymentAuthorizationDate = ZDateTime.Empty;
			header.B2_PaymentParty = "";
			header.B2_PaymentStatus = "";
			header.B2_PaymentType = "";
			header.B2_PreparerDistrictPort = "";
			header.B2_PrintDate = ZDateTime.Empty;
			header.B2_ProcessDate = ZDateTime.Empty;
			header.B2_ProcessPort = "";
			header.B2_StatementAmount = 0m;
			header.B2_Status = "";
		}

		CusStatementHeader GetSourceStatement()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP23423";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.MainAddress.OA_Address1 = "BOB'S ADDRESS 1";
			importer.OH_IsConsignee = ZBool.True;
			var header = Factory.New<CusStatementHeader>();
			header.B2_AccountNo = "ACN123";
			header.B2_BranchDesignation = "BD1";
			header.B2_CheckNo = "CHECKNO1234567890";
			header.B2_DueDate = new ZDateTime(2014, 8, 1);
			header.B2_EntryFilerCode = "EFC1234567890";
			header.B2_ImporterCustomsID = "IMPCUSID123456";
			header.B2_OH_Importer = importer.PK;
			header.B2_PaymentAuthorizationDate = new ZDateTime(2014, 8, 30);
			header.B2_PaymentParty = "PP1";
			header.B2_PaymentStatus = "PS1";
			header.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			header.B2_PreparerDistrictPort = "PDP1";
			header.B2_PrintDate = new ZDateTime(2014, 8, 4);
			header.B2_ProcessDate = new ZDateTime(2014, 8, 5);
			header.B2_ProcessPort = "PROP1";
			header.B2_StatementAmount = 15045.45m;
			header.B2_Status = "ST1";
			header.B2_StatementType = "I";
			var agentsInstructionDoc = header.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.AgentsInstruction);
			agentsInstructionDoc.EQ_ValidToDate = new ZDateTime(2014, 8, 12);
			var dangerousGoodsFormDoc = header.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.DangerousGoodsForm);
			dangerousGoodsFormDoc.EQ_ValidToDate = new ZDateTime(2014, 10, 12);

			var dailyHeader = header.DailyStatements.AddNew();
			dailyHeader.B2_StatementNumber = statementNumberFromXml + "D";
			dailyHeader.B2_AccountNo = "ACN456";
			dailyHeader.B2_BranchDesignation = "BD2";
			dailyHeader.B2_CheckNo = "CHECKNO0987654321";
			dailyHeader.B2_DueDate = new ZDateTime(2014, 8, 10);
			dailyHeader.B2_EntryFilerCode = "EFC0987654321";
			dailyHeader.B2_ImporterCustomsID = "IMPCUSID789456";
			dailyHeader.B2_IsMonthlyStatement = ZBool.False;
			dailyHeader.B2_OH_Importer = importer.PK;
			dailyHeader.B2_PaymentAuthorizationDate = new ZDateTime(2014, 9, 30);
			dailyHeader.B2_PaymentParty = "PP2";
			dailyHeader.B2_PaymentStatus = "PS2";
			dailyHeader.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			dailyHeader.B2_PreparerDistrictPort = "PDP2";
			dailyHeader.B2_PrintDate = new ZDateTime(2014, 9, 4);
			dailyHeader.B2_ProcessDate = new ZDateTime(2014, 9, 5);
			dailyHeader.B2_ProcessPort = "PROP2";
			dailyHeader.B2_StatementAmount = 5621.89m;
			dailyHeader.B2_Status = "ST2";
			dailyHeader.B2_StatementType = "I";
			var arrivalNoticeDoc = dailyHeader.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.ArrivalNotice);
			arrivalNoticeDoc.EQ_ValidToDate = new ZDateTime(2014, 9, 12);
			var dailyHeader2 = header.DailyStatements.AddNew();
			dailyHeader2.B2_StatementNumber = statementNumberFromXml + "D2";
			dailyHeader2.B2_AccountNo = "ACN789";
			dailyHeader2.B2_BranchDesignation = "BD3";
			dailyHeader2.B2_CheckNo = "CHECKNO5678945";
			dailyHeader2.B2_DueDate = new ZDateTime(2014, 9, 10);
			dailyHeader2.B2_EntryFilerCode = "EFC567894";
			dailyHeader2.B2_ImporterCustomsID = "IMPCUSID1584";
			dailyHeader2.B2_IsMonthlyStatement = ZBool.False;
			dailyHeader2.B2_OH_Importer = importer.PK;
			dailyHeader2.B2_PaymentAuthorizationDate = new ZDateTime(2014, 10, 30);
			dailyHeader2.B2_PaymentParty = "PP3";
			dailyHeader2.B2_PaymentStatus = "PS3";
			dailyHeader2.B2_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			dailyHeader2.B2_PreparerDistrictPort = "PDP3";
			dailyHeader2.B2_PrintDate = new ZDateTime(2014, 10, 4);
			dailyHeader2.B2_ProcessDate = new ZDateTime(2014, 10, 5);
			dailyHeader2.B2_ProcessPort = "PROP3";
			dailyHeader2.B2_StatementAmount = 5342.89m;
			dailyHeader2.B2_Status = "ST3";
			dailyHeader2.B2_StatementType = "I";
			var authorityToDealDoc = dailyHeader2.RequiredDocuments.AddNew(Enterprise.Core.Constants.RefDocTypes.AuthorityToDeal);
			authorityToDealDoc.EQ_ValidToDate = new ZDateTime(2014, 10, 12);
			var line1 = header.StatementLines.AddNew();
			line1.B3_BrokerReference = "BK452389023";
			line1.B3_CustomsFeesTotal = 4586.57m;
			line1.B3_DeletedByParty = "DP1";
			line1.B3_EIIndicator = "EI1";
			line1.B3_EntryFilerCode = "EF1";
			line1.B3_EntryNum = "ENTNO123456";
			line1.B3_EntryProcessPort = "EPP12";
			line1.B3_EntryStatus = "ES1";
			line1.B3_EntryType = "ET1";
			line1.B3_Status = "ST1";
			line1.B3_Team = "TM1";
			var charge1 = line1.Charges.AddNew();
			charge1.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;
			charge1.B4_ChargeAmount = 25m;
			var charge2 = line1.Charges.AddNew();
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Cotton;
			charge2.B4_ChargeAmount = 150.15m;

			var line2 = header.StatementLines.AddNew();
			line2.B3_BrokerReference = "BK96355630";
			line2.B3_CustomsFeesTotal = 86546.45m;
			line2.B3_DeletedByParty = "DP2";
			line2.B3_EIIndicator = "EI2";
			line2.B3_EntryFilerCode = "EF2";
			line2.B3_EntryNum = "ENTNO7895564";
			line2.B3_EntryProcessPort = "EPP34";
			line2.B3_EntryStatus = "ES2";
			line2.B3_EntryType = "ET2";
			line2.B3_Status = "ST2";
			line2.B3_Team = "TM2";
			var charge3 = line2.Charges.AddNew();
			charge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Avocado;
			charge3.B4_ChargeAmount = 896.50m;

			var line3 = dailyHeader2.StatementLines.AddNew();
			line3.B3_BrokerReference = "BK865211";
			line3.B3_CustomsFeesTotal = 863215.45m;
			line3.B3_DeletedByParty = "DP3";
			line3.B3_EIIndicator = "EI3";
			line3.B3_EntryFilerCode = "EF3";
			line3.B3_EntryNum = "ENTNO362584";
			line3.B3_EntryProcessPort = "EPP98";
			line3.B3_EntryStatus = "ES3";
			line3.B3_EntryType = "ET3";
			line3.B3_Status = "ST3";
			line3.B3_Team = "T32";
			var charge4 = line3.Charges.AddNew();
			charge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.Pork;
			charge4.B4_ChargeAmount = 1532m;

			header.B2_StatementNumber = statementNumberFromXml;
			header.B2_IsMonthlyStatement = ZBool.True;

			CreateLineGroupAndFinancialDetail(header, importer.PK, "18121933RM0001", "AAA", 101.03m);
			CreateLineGroupAndFinancialDetail(dailyHeader, importer.PK, "18123333RM0002", "BBB", 760.50m);
			CreateLineGroupAndFinancialDetail(dailyHeader2, importer.PK, "18184233RM0003", "CCC", 697.20m);

			Factory.Save();

			return header;
		}

		void CreateLineGroupAndFinancialDetail(CusStatementHeader header, ZGuid importerPk, string customsId, string chargeType, decimal amount)
		{
			var lineGroup = header.Factory.New<CusStatementLineGroup>();
			lineGroup.B10_B2 = header.PK;
			lineGroup.B10_OH_Importer = importerPk;
			lineGroup.B10_ImporterCustomsID = customsId;

			var lineGroupFinancialDetail = header.Factory.New<CusStatementLineGroupFinancialDetail>();
			lineGroupFinancialDetail.B11_B10 = lineGroup.PK;
			lineGroupFinancialDetail.B11_Amount = amount;
			lineGroupFinancialDetail.B11_Type = chargeType;
		}

		sealed class CusStatementLineGroup : AutoCusStatementLineGroup
		{
			public CusStatementLineGroup(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		sealed class CusStatementLineGroupFinancialDetail : AutoCusStatementLineGroupFinancialDetail
		{
			public CusStatementLineGroupFinancialDetail(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		static XElement SerialiseToXml(CusStatementHeader header)
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };
			var actualMessage = "";
			using (var dataStream = xmlSerializer.SerializeToStream(header))
			using (var reader = new StreamReader(dataStream))
			{
				actualMessage = reader.ReadToEnd();
			}
			var element = XElement.Load(new StringReader(actualMessage));
			return element;
		}

		const string statementNumberFromXml = "ST213218956";
	}
}
