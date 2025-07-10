using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocMiscServ))]
	public class DocMiscServTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var header = Factory.New<OrgHeader>();

			return new DocumentWrapper[]
			{
				DocMiscServ.New(header.MiscServ, Factory)
			};
		}

		OrgMiscServ MiscServ;
		DocMiscServ MiscServWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<OrgHeader>();
			MiscServ = header.MiscServ;
			MiscServWrapper = DocMiscServ.New(MiscServ, Factory);
		}

		public void TestCreditTerms()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			MiscServ = header.MiscServ;
			DocMiscServ miscServWrapper = DocMiscServ.New(MiscServ, factory);

			CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "MIC";
			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 2;
			expected = "2 MONTHS " + InvoiceTermsList.MonthsFromInvoiceCycleDate.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;
			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 5;
			expected = "5 " + InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 12;
			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			OrgHeader settlementHeader = factory.New<OrgHeader>();
			header.ARSettlementGroupPK = settlementHeader.PK;
			AssertNotNull("ARSettlementGroup", header.ARSettlementGroup);
			header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementHeader.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			settlementHeader.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 12;
			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.CreditTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "FIN";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "Multiple – As per Invoice";
				AssertEquals(expected, miscServWrapper.CreditTerms);
			}
		}

		public void TestDisbursementCreditTerms()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			MiscServ = header.MiscServ;
			DocMiscServ miscServWrapper = DocMiscServ.New(MiscServ, factory);

			CodeDescriptionPairList termList = new ARInvoiceTermsList();
			ZString expected = termList.GetDescriptionFromCode("COD").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "MIC";
			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 2;
			expected = "2 MONTHS " + InvoiceTermsList.MonthsFromInvoiceCycleDate.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Code;
			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 5;
			expected = "5 " + InvoiceTermsList.TermDaysAndDebtorPaymentCycle.Description.Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 12;
			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			OrgHeader settlementHeader = factory.New<OrgHeader>();
			header.ARSettlementGroupPK = settlementHeader.PK;
			AssertNotNull("ARSettlementGroup", header.ARSettlementGroup);
			header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;
			settlementHeader.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			settlementHeader.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 12;
			expected = "12 DAYS " + termList.GetDescriptionFromCode("INV").Trim().ToUpper();
			AssertEquals("Invoice terms", expected, miscServWrapper.DisbursementCreditTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "DBT";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "Multiple – As per Invoice";
				AssertEquals(expected, miscServWrapper.DisbursementCreditTerms);
			}
		}

		public void TestShortenedCreditTerms()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			MiscServ = header.MiscServ;
			DocMiscServ miscServWrapper = DocMiscServ.New(MiscServ, factory);

			CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
			ZString expected = termList.GetMultilingualDescriptionFromCode("COD").ToString().Trim();
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedCreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedCreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "MIC";
			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 2;
			expected = string.Format(termList.GetMultilingualDescriptionFromCode("MIC").ToString().Trim(), MiscServ.Header.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedCreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "INV";
			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 12;
			expected = string.Format(termList.GetMultilingualDescriptionFromCode("INV").ToString().Trim(), MiscServ.Header.CompanyData.GetARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedCreditTerms);

			MiscServ.Header.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = "XXX";
			expected = "12 DAYS ";
			AssertEquals("Credit Terms", expected, miscServWrapper.ShortenedCreditTerms);

			ExceptionReporterTestListener.Instance.Clear();

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "FIN";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "Multiple – As per Invoice";
				AssertEquals(expected, miscServWrapper.ShortenedCreditTerms);
			}
		}

		public void TestShortenedDisbursementCreditTerms()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			MiscServ = header.MiscServ;
			DocMiscServ miscServWrapper = DocMiscServ.New(MiscServ, factory);

			CodeDescriptionPairList termList = new InvoiceTermsListWithShortDescription();
			ZString expected = termList.GetMultilingualDescriptionFromCode("COD").ToString().Trim();
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedDisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "COD";
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedDisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "MIC";
			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 2;
			expected = string.Format(termList.GetMultilingualDescriptionFromCode("MIC").ToString().Trim(), MiscServ.Header.CompanyData.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedDisbursementCreditTerms);

			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceTerm = "INV";
			MiscServ.Header.CompanyData.CreateOrLoadDisbursementARTerm().PY_InvoiceDays = 12;
			expected = string.Format(termList.GetMultilingualDescriptionFromCode("INV").ToString().Trim(), MiscServ.Header.CompanyData.GetDisbursementARTerm(null, ZString.Empty, ZString.Empty, ZGuid.Empty, ZGuid.Empty).Days);
			AssertEquals("Invoice terms", expected, miscServWrapper.ShortenedDisbursementCreditTerms);

			var term = header.CompanyData.ARTerms.AddNew();
			using (term.GetValidationSuspender())
			{
				term.PY_JobType = "SHP";
				term.PY_GB_Branch = Guid.Empty;
				term.PY_GE_Department = Guid.Empty;
				term.PY_Direction = "ALL";
				term.PY_TransportMode = "AIR";
				term.PY_InvoiceClass = "DBT";
				term.PY_InvoiceTerm = "INV";
				term.PY_InvoiceDays = 25;

				expected = "Multiple – As per Invoice";
				AssertEquals(expected, miscServWrapper.ShortenedDisbursementCreditTerms);
			}
		}

		[ExpectNoExceptions]
		public void TestEmptyTrademarkLogo()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			DocMiscServ miscServWrapper = DocMiscServ.New(header.MiscServ, factory);
			AssertNull(miscServWrapper.TrademarkLogo);
		}

		public void TestGlobalCreditGroupCurrency()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var miscServWrapper = DocMiscServ.New(header.MiscServ, Factory);
			AssertEquals(string.Empty, miscServWrapper.ARGlobalCreditCurrency);

			header.MiscServ.OM_RX_NKARGlobalCreditCurrency = "USD";
			header.MiscServ.OM_ARGlobalCreditApproved = true;
			miscServWrapper = DocMiscServ.New(header.MiscServ, Factory);
			AssertEquals("AR Global Credit Currency should equal to its own USD", "USD", miscServWrapper.ARGlobalCreditCurrency);

			var groupHeader = Factory.NewWithValidTestData<OrgHeader>();
			groupHeader.MiscServ.OM_ARGlobalCreditLimit = 200M;
			groupHeader.OH_Code = "Global Group";
			groupHeader.OH_FullName = "Full Name of Global Group";
			groupHeader.MiscServ.OM_RX_NKARGlobalCreditCurrency = "AUD";

			header.MiscServ.OM_OH_ARGlobalCreditGroup = groupHeader.PK;
			AssertEquals("AR Global Credit Currency should equal to its Group's Currency", "AUD", miscServWrapper.ARGlobalCreditCurrency);
		}

		public void TestGlobalCreditGroupAndLimit()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var miscServWrapper = DocMiscServ.New(header.MiscServ, Factory);

			AssertEquals(ZString.Empty, miscServWrapper.ARGlobalCreditGroup);
			AssertEquals(0M, miscServWrapper.ARGlobalCreditLimit);

			var miscServ = header.MiscServ;
			miscServ.OM_ARGlobalCreditApproved = true;
			miscServ.OM_ARGlobalCreditLimit = 100M;
			header.OH_Code = "ABC";
			header.OH_FullName = "ABilled Group";
			miscServWrapper = DocMiscServ.New(miscServ, Factory);
			AssertEquals("ABC ABilled Group", miscServWrapper.ARGlobalCreditGroup);
			AssertEquals(100M, miscServWrapper.ARGlobalCreditLimit);

			var groupHeader = Factory.NewWithValidTestData<OrgHeader>();
			groupHeader.MiscServ.OM_ARGlobalCreditLimit = 200M;
			groupHeader.OH_Code = "Global Group";
			groupHeader.OH_FullName = "Full Name of Global Group";
			groupHeader.MiscServ.OM_ARGlobalCreditApproved = true;

			header = Factory.NewWithValidTestData<OrgHeader>();
			header.MiscServ.OM_OH_ARGlobalCreditGroup = groupHeader.PK;

			miscServWrapper = DocMiscServ.New(header.MiscServ, Factory);
			AssertEquals("Global Group Full Name of Global Group", miscServWrapper.ARGlobalCreditGroup);
			AssertEquals(200M, miscServWrapper.ARGlobalCreditLimit);

			groupHeader.MiscServ.OM_ARGlobalCreditApproved = false;
			miscServWrapper = DocMiscServ.New(header.MiscServ, Factory);
			AssertEquals(0m, miscServWrapper.ARGlobalCreditLimit);
		}

		public void TestGlobalCreditGroupAndLimitInheritesFromParentGroup()
		{
			var groupOrg = Factory.NewWithValidTestData<OrgHeader>();
			groupOrg.MiscServ.OM_ARGlobalCreditApproved = true;
			groupOrg.MiscServ.OM_ARGlobalCreditLimit = 100M;
			groupOrg.OH_Code = "GlobalGroup";
			groupOrg.OH_FullName = "Foo Global Group";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_OH_ARGlobalCreditGroup = groupOrg.PK;

			var miscServWrapper = DocMiscServ.New(org.MiscServ, Factory);
			AssertEquals("The organisation should return its Global Group Name", groupOrg.OH_Code + " " + groupOrg.OH_FullName, miscServWrapper.ARGlobalCreditGroup);
			AssertEquals(100M, miscServWrapper.ARGlobalCreditLimit);
		}

		public void TestTrademarkLogo()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var header = factory.New<OrgHeader>();
			header.OH_FullName = "Test Org";
			header.MainAddress.OA_Address1 = "Test Address";
			header.OH_RL_NKClosestPort = "AUSYD";
			OrgMiscServ miscServ = header.MiscServ;

			DocMiscServ miscServWrapper = DocMiscServ.New(miscServ, factory);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var memStream = resourceRetriever.GetBytes("Enterprise.DocumentWrappers.Testing.AWBMasterTitle.png");

			var note = factory.New<StmNote>();
			note.ST_NoteData = memStream;
			note.ST_ParentID = miscServ.PK;
			note.ST_Table = miscServ.TableName;
			note.ST_NoteType = nameof(StmNoteVisibility.DOC);
			note.ST_Description = "ClientDocumentLogo";
			factory.Save();

			AssertNotNull(miscServWrapper.TrademarkLogo);
		}

		public void TestAttributes()
		{
			MiscServ.OM_IMPartAttrib1Name = "PartAttrib1";
			MiscServ.OM_IMPartAttrib1Name = "PartAttrib2";
			MiscServ.OM_IMPartAttrib1Name = "PartAttrib3";
			MiscServ.OM_CustomAttrib1 = "CustomAttrib1";
			MiscServ.OM_CustomAttrib2 = "CustomAttrib2";
			MiscServ.OM_CustomAttrib3 = "CustomAttrib3";

			AssertEquals("IMPartAttrib1Name", MiscServ.OM_IMPartAttrib1Name, MiscServWrapper.IMPartAttrib1Name);
			AssertEquals("IMPartAttrib2Name", MiscServ.OM_IMPartAttrib2Name, MiscServWrapper.IMPartAttrib2Name);
			AssertEquals("IMPartAttrib3Name", MiscServ.OM_IMPartAttrib3Name, MiscServWrapper.IMPartAttrib3Name);
			AssertEquals("CustomAttrib1", MiscServ.OM_CustomAttrib1, MiscServWrapper.CustomAttrib1);
			AssertEquals("CustomAttrib2", MiscServ.OM_CustomAttrib2, MiscServWrapper.CustomAttrib2);
			AssertEquals("CustomAttrib3", MiscServ.OM_CustomAttrib3, MiscServWrapper.CustomAttrib3);
		}

		public void TestIMPartAttribNames_Translatable()
		{
			MiscServ.OM_IMPartAttrib1Name = "PartAttrib1";
			MiscServ.OM_IMPartAttrib2Name = "PartAttrib2";
			MiscServ.OM_IMPartAttrib3Name = "PartAttrib3";

			AssertEquals("PartAttrib1", "PartAttrib1", MiscServWrapper.IMPartAttrib1Name);
			AssertEquals("PartAttrib2", "PartAttrib2", MiscServWrapper.IMPartAttrib2Name);
			AssertEquals("PartAttrib3", "PartAttrib3", MiscServWrapper.IMPartAttrib3Name);

			var partAttrib1MultiLingualString = MiscServ.OM_IMPartAttrib1NameMultilingual;
			var resKey1 = MiscServ.OM_IMPartAttrib1NameInfo.CustomizableDataResourceStrings.GetMultilingualString(MiscServ, "PartAttrib1").ResourceKey;

			var partAttrib2MultiLingualString = MiscServ.OM_IMPartAttrib2NameMultilingual;
			var resKey2 = MiscServ.OM_IMPartAttrib2NameInfo.CustomizableDataResourceStrings.GetMultilingualString(MiscServ, "PartAttrib2").ResourceKey;

			var partAttrib3MultiLingualString = MiscServ.OM_IMPartAttrib3NameMultilingual;
			var resKey3 = MiscServ.OM_IMPartAttrib3NameInfo.CustomizableDataResourceStrings.GetMultilingualString(MiscServ, "PartAttrib3").ResourceKey;

			AssertEquals("PartAttrib1MultiLingualString in English", "PartAttrib1", partAttrib1MultiLingualString);
			AssertEquals("PartAttrib2MultiLingualString in English", "PartAttrib2", partAttrib2MultiLingualString);
			AssertEquals("PartAttrib3MultiLingualString in English", "PartAttrib3", partAttrib3MultiLingualString);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey1, new ResourceStringData(resKey1, "属性1"));
				mockRes.Put(resKey2, new ResourceStringData(resKey2, "属性2"));
				mockRes.Put(resKey3, new ResourceStringData(resKey3, "属性3"));

				AssertEquals("PartAttrib1MultiLingualString in Chinese", "属性1", partAttrib1MultiLingualString);
				AssertEquals("PartAttrib2MultiLingualString in Chinese", "属性2", partAttrib2MultiLingualString);
				AssertEquals("PartAttrib3MultiLingualString in Chinese", "属性3", partAttrib3MultiLingualString);
			}
		}

		public void TestAPPaymentTermsAndDays()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			OrgHeader header = factory.New<OrgHeader>();
			OrgMiscServ miscServ = header.MiscServ;
			DocMiscServ miscServWrapper = DocMiscServ.New(miscServ, factory);

			OrgHeader settlementHeader = factory.New<OrgHeader>();
			header.APSettlementGroupPK = settlementHeader.PK;
			AssertNotNull("APSettlementGroup", header.APSettlementGroup);
			header.CompanyData.OB_APPaymentTerms = OrgCompanyDataLookups.DefaultInvoiceTerm.Code;

			settlementHeader.CompanyData.OB_APPaymentTerms = InvoiceTermsList.FromInvoiceDate.Code;
			settlementHeader.CompanyData.OB_APPaymentTermDays = 12;
			AssertEquals("Invoice terms", InvoiceTermsList.FromInvoiceDate.Code, miscServWrapper.APPaymentTerms);
			AssertEquals("Invoice days", (ZByte)12, miscServWrapper.APPaymentTermDays);
		}
	}
}
