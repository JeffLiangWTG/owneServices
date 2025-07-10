using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004.Testing
{
	[TestedType(typeof(ChinaStandard2004DataAdapter))]
	sealed class ChinaStandard2004DataAdapterTest : BaseAccountingDataAdapterTest<BizObjThatDoesntSaveForCN2004, XSDs.会计核算软件数据>
	{
		[SuspendGLAccountAndChargeCodeCriticalValidation]
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportFile()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.China))
			{
				var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, Constants.CurrencyCodes.China));
				var helper = new RefCurrencyTestHelper(Factory);
				helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_Desc, currency.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "中国人民币元");
				helper.CreateRefLanguageText(RefCurrencySchema.Constants.RX_UnitName, currency.PK, Constants.Languages.ChineseSimplified, RefCurrencySchema.Constants.Prefix, "人民币");
				Factory.Save();
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.PostPeriodsForEntireYear(2005);
				testHelper.PostPeriodsForEntireYear(2006);

				ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
				AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

				AccGLHeader apControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;
				AccGLHeader arControl = Factory.NewWithValidTestData(typeof(AccGLHeader)) as AccGLHeader;

				apControl.AG_DebitCredit = Constants.DebitCredit.Debit;
				arControl.AG_DebitCredit = Constants.DebitCredit.Debit;

				AccGLAccountDescriptor arControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
				arControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
				arControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
				arControlLocal.ParentGLHeaderPK = arControl.PK;
				arControlLocal.AJ_LocalAccountNumber = "ARControlAccount";
				arControlLocal.AJ_AccountDescription = "ARControlDescription";
				arControlLocal.AJ_DebitCredit = arControl.AG_DebitCredit;

				AccGLAccountDescriptor apControlLocal = Factory.NewWithValidTestData(typeof(AccGLAccountDescriptor)) as AccGLAccountDescriptor;
				apControlLocal.AJ_Language = DataInterfaceUtils.GetLocalLanguage();
				apControlLocal.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
				apControlLocal.ParentGLHeaderPK = apControl.PK;
				apControlLocal.AJ_LocalAccountNumber = "APControlAccount";
				apControlLocal.AJ_AccountDescription = "APControlDescription";
				apControlLocal.AJ_DebitCredit = apControl.AG_DebitCredit;
				AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControl.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControl.PK.ToGuid());

				ARInvoice testArInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
				testArInvoice.AH_TransactionNum = "100111";
				testArInvoice.AH_OH = new TestObjectCreator(Factory).AALSHI.PK;
				//
				testArInvoice.AH_PostDate = new ZDateTime(2006, 3, 13);
				testArInvoice.AH_DueDate = new ZDateTime(2006, 3, 18);
				testArInvoice.AH_TransactionReference = "Invoice No";
				testArInvoice.AH_ExchangeRate = 1m;
				testArInvoice.AH_InvoiceAmount = 111m;
				testArInvoice.AH_OutstandingAmount = 111m;
				testArInvoice.AH_Desc = "Invoice Desc";
				testArInvoice.AH_NumberOfSupportingDocuments = 0;

				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
				AccGLHeader glHeader0PNL = testObjectCreator.CreateAccGLHeader("1100.90.00", "TS", "Test PNL 0", "P&L", Constants.DebitCredit.Debit);
				AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.93.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);

				AccGLAccountDescriptor accountDesriptor = testObjectCreator.CreateAccountDescriptor(glHeader0PNL, "6100.000", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
				testObjectCreator.CreateGLDescriptorPivot(accountDesriptor, glHeader0PNL, "PLA", "A03");
				testObjectCreator.CreateGLDescriptorPivot(accountDesriptor, glHeader0PNL, "SSE", "B03");
				testObjectCreator.CreateAccGLAggregate(10m, 200603, glHeader0PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "6100.095", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1PNL, "SPA", "P02");

				testObjectCreator.CreateAccGLAggregate(1001m, 200603, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
				AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

				testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1, "BSH", "D01");
				testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2, "BSH", "D01");

				testObjectCreator.CreateAccGLAggregate(1000m, 200603, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
				testObjectCreator.CreateAccGLAggregate(2000m, 200603, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

				var accrual = testObjectCreator.CreateAccrual();
				accrual.AL_PostDate = new ZDateTime(2006, 03, 30, 23, 59, 59);
				testObjectCreator.CreateAccountDescriptor(accrual.AL_AG, "1000000.1", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "Desc", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				testObjectCreator.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, "88.88.888.8", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "CostControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);

				Factory.Save();

				ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
				using (StringWriter writer = new StringWriter())
				{
					BizObjToImportTo.ExportFiles.Add("AccountBook");
					BizObjToImportTo.ExportFiles.Add("ChartOfAccounts");
					BizObjToImportTo.ExportFiles.Add("TrialBalance");
					BizObjToImportTo.ExportFiles.Add("SupplementaryAccounts");
					BizObjToImportTo.ExportFiles.Add("AccountingVouchers");
					BizObjToImportTo.ExportFiles.Add("BalanceSheet");
					BizObjToImportTo.ExportFiles.Add("ProfitAndLoss");
					BizObjToImportTo.ExportFiles.Add("VATDetailed");
					BizObjToImportTo.ExportFiles.Add("AssetProvision");
					BizObjToImportTo.ExportFiles.Add("PNLAppropriation");
					BizObjToImportTo.ExportFiles.Add("EquityMovement");
					serialiser.WriteToXml(writer, Adapter, BizObjToImportTo, context);
					string expectedText = Retriever.GetString("ChinaStandard2004.xml");
					string data = writer.ToString();
					data = data.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
					data = data.Replace("xmlns=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"",
												"xmlns:gssm=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"\r\n xmlns:user=\"http://schemas.accounting.org.cn/2004/datainterface/user\"\r\n xmlns=\"http://schemas.accounting.org.cn/2004/datainterface/gssm\"\r\n xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n xsi:schemaLocation=\"http://schemas.accounting.org.cn/2004/datainterface/gssmgssm.xsd\" gssm:locID=\"t000\"");

					this.AssertXMLEqualsByDiff("China Standard 2004 XML does not match", expectedText, data);
				}
			}
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		AccountingPeriodTestHelper PeriodTestHelper
		{
			get { return periodTestHelper ?? (periodTestHelper = new AccountingPeriodTestHelper()); }
		}
		AccountingPeriodTestHelper periodTestHelper;

		BizObjThatDoesntSaveForCN2004 BizObjToImportTo
		{
			get
			{
				if (bObjCN == null)
				{
					TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
					PeriodTestHelper.PostPeriodsForEntireYear(2005);
					PeriodTestHelper.PostPeriodsForEntireYear(2006);
					bObjCN = new BizObjThatDoesntSaveForCN2004(Factory) { Period = 200603, FromDate = new ZDateTime(2006, 3, 1), ToDate = new ZDateTime(2006, 4, 1) };
					bObjCN.ExportFilesType = BizObjThatDoesntSaveForCN2004.FilesType.XML;
				}
				return bObjCN;
			}
		}
		BizObjThatDoesntSaveForCN2004 bObjCN;

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, BizObjThatDoesntSaveForCN2004 bizObjToImportTo, XSDs.会计核算软件数据 exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
		{
			Assert(true);   //Export Only Data Adapter
		}

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert(true);   //Export Only Data Adapter
		}

		protected override bool IsImportFromValueObjectSupported
		{
			get { return false; }
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override ValueObjectDataAdapter<BizObjThatDoesntSaveForCN2004, XSDs.会计核算软件数据> GetNewBizObjXmlDataAdapter()
		{
			return new ChinaStandard2004DataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(BizObjToImportTo, Retriever.SaveResourceToFile("EmptyChinaStandard2004.xml"), ValidationKind.None, "Fully Populated China Standard 2004 Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(BizObjToImportTo, Retriever.SaveResourceToFile("EmptyChinaStandard2004.xml"), ValidationKind.None, "Semi Populated China Standard 2004 Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(BizObjToImportTo, Retriever.SaveResourceToFile("EmptyChinaStandard2004.xml"), ValidationKind.None, "Empty China Standard 2004 Files");
		}

		protected override BizObjThatDoesntSaveForCN2004 NewBusinessObject()
		{
			return new BizObjThatDoesntSaveForCN2004(Factory);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.China; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = Constants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			Factory.Save();
		}

		ChinaStandard2004DataAdapter fAdapter;
		ChinaStandard2004DataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new ChinaStandard2004DataAdapter()); }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[] { "电子账簿/电子账簿编号/locID",
"电子账簿/电子账簿编号/Value",
"电子账簿/电子账簿名称/locID",
"电子账簿/电子账簿名称/Value",
"电子账簿/会计核算单位/locID",
"电子账簿/会计核算单位/Value",
"电子账簿/组织机构代码/locID",
"电子账簿/组织机构代码/Value",
"电子账簿/单位性质/locID",
"电子账簿/单位性质/Value",
"电子账簿/行业/locID",
"电子账簿/行业/Value",
"电子账簿/开发单位/locID",
"电子账簿/开发单位/Value",
"电子账簿/版本号/locID",
"电子账簿/版本号/Value",
"电子账簿/年度/locID",
"电子账簿/年度/Value",
"电子账簿/本位币/locID",
"电子账簿/本位币/Value",
"电子账簿/科目结构/locID",
"电子账簿/科目结构/Value",
"电子账簿/locID",
"会计科目/科目编号/locID",
"会计科目/科目编号/Value",
"会计科目/科目名称/locID",
"会计科目/科目名称/Value",
"会计科目/科目级次/locID",
"会计科目/辅助核算标志/locID",
"会计科目/辅助核算项/locID",
"会计科目/辅助核算项/Value",
"会计科目/科目类型/locID",
"会计科目/科目类型/Value",
"会计科目/计量单位/locID",
"会计科目/计量单位/Value",
"会计科目/余额方向/locID",
"会计科目/locID",
"辅助核算项档案/单位/单位编号/locID",
"辅助核算项档案/单位/单位编号/Value",
"辅助核算项档案/单位/单位名称/locID",
"辅助核算项档案/单位/单位名称/Value",
"辅助核算项档案/单位/类别编号/locID",
"辅助核算项档案/单位/类别编号/Value",
"辅助核算项档案/单位/地区/locID",
"辅助核算项档案/单位/地区/Value",
"辅助核算项档案/单位/电话/locID",
"辅助核算项档案/单位/电话/Value",
"辅助核算项档案/单位/地址/locID",
"辅助核算项档案/单位/地址/Value",
"辅助核算项档案/单位/信用等级/locID",
"辅助核算项档案/单位/信用等级/Value",
"辅助核算项档案/单位/locID",
"辅助核算项档案/部门/部门编号/locID",
"辅助核算项档案/部门/部门编号/Value",
"辅助核算项档案/部门/部门名称/locID",
"辅助核算项档案/部门/部门名称/Value",
"辅助核算项档案/部门/上级部门编号/locID",
"辅助核算项档案/部门/上级部门编号/Value",
"辅助核算项档案/部门/locID",
"辅助核算项档案/人员/人员编号/locID",
"辅助核算项档案/人员/人员编号/Value",
"辅助核算项档案/人员/人员姓名/locID",
"辅助核算项档案/人员/人员姓名/Value",
"辅助核算项档案/人员/证件类别/locID",
"辅助核算项档案/人员/证件类别/Value",
"辅助核算项档案/人员/证件号码/locID",
"辅助核算项档案/人员/证件号码/Value",
"辅助核算项档案/人员/性别/locID",
"辅助核算项档案/人员/性别/Value",
"辅助核算项档案/人员/出生日期/locID",
"辅助核算项档案/人员/出生日期/Value",
"辅助核算项档案/人员/部门编号/locID",
"辅助核算项档案/人员/部门编号/Value",
"辅助核算项档案/人员/入职日期/locID",
"辅助核算项档案/人员/入职日期/Value",
"辅助核算项档案/人员/离职日期/locID",
"辅助核算项档案/人员/离职日期/Value",
"辅助核算项档案/人员/locID",
"辅助核算项档案/locID",
"科目余额及发生额/科目编号/locID",
"科目余额及发生额/科目编号/Value",
"科目余额及发生额/币种/locID",
"科目余额及发生额/币种/Value",
"科目余额及发生额/辅助核算组/locID",
"科目余额及发生额/辅助核算组/Value",
"科目余额及发生额/期初余额/locID",
"科目余额及发生额/期初数量/locID",
"科目余额及发生额/期初外币余额/locID",
"科目余额及发生额/借方发生额/locID",
"科目余额及发生额/借方发生数量/locID",
"科目余额及发生额/借方外币发生额/locID",
"科目余额及发生额/贷方发生额/locID",
"科目余额及发生额/贷方发生数量/locID",
"科目余额及发生额/贷方外币发生额/locID",
"科目余额及发生额/期末余额/locID",
"科目余额及发生额/期末数量/locID",
"科目余额及发生额/期末外币余额/locID",
"科目余额及发生额/会计月度/locID",
"科目余额及发生额/会计月度/Value",
"科目余额及发生额/locID",
"记账凭证/凭证日期/locID",
"记账凭证/凭证日期/Value",
"记账凭证/凭证种类/locID",
"记账凭证/凭证种类/Value",
"记账凭证/凭证编号/locID",
"记账凭证/凭证编号/Value",
"记账凭证/行号/locID",
"记账凭证/摘要/locID",
"记账凭证/摘要/Value",
"记账凭证/科目编号/locID",
"记账凭证/科目编号/Value",
"记账凭证/借方金额/locID",
"记账凭证/贷方金额/locID",
"记账凭证/币种/locID",
"记账凭证/币种/Value",
"记账凭证/借方外币金额/locID",
"记账凭证/贷方外币金额/locID",
"记账凭证/汇率/locID",
"记账凭证/数量/locID",
"记账凭证/单价/locID",
"记账凭证/辅助核算组/locID",
"记账凭证/辅助核算组/Value",
"记账凭证/结算方式/locID",
"记账凭证/结算方式/Value",
"记账凭证/票据类型/locID",
"记账凭证/票据类型/Value",
"记账凭证/票据号/locID",
"记账凭证/票据号/Value",
"记账凭证/票据日期/locID",
"记账凭证/票据日期/Value",
"记账凭证/附件数/locID",
"记账凭证/制单人员/locID",
"记账凭证/制单人员/Value",
"记账凭证/审核人员/locID",
"记账凭证/审核人员/Value",
"记账凭证/记账人员/locID",
"记账凭证/记账人员/Value",
"记账凭证/出纳人员/locID",
"记账凭证/出纳人员/Value",
"记账凭证/结账标志/locID",
"记账凭证/作废标志/locID",
"记账凭证/locID",
"企业资产负债表/报表编号/locID",
"企业资产负债表/报表编号/Value",
"企业资产负债表/编制单位/locID",
"企业资产负债表/编制单位/Value",
"企业资产负债表/报告日/locID",
"企业资产负债表/报告日/Value",
"企业资产负债表/货币单位/locID",
"企业资产负债表/货币单位/Value",
"企业资产负债表/项目/locID",
"企业资产负债表/项目/Value",
"企业资产负债表/行次/locID",
"企业资产负债表/行次/Value",
"企业资产负债表/年初数/locID",
"企业资产负债表/期末数/locID",
"企业资产负债表/locID",
"企业利润表/报表编号/locID",
"企业利润表/报表编号/Value",
"企业利润表/编制单位/locID",
"企业利润表/编制单位/Value",
"企业利润表/报告期/locID",
"企业利润表/报告期/Value",
"企业利润表/货币单位/locID",
"企业利润表/货币单位/Value",
"企业利润表/项目/locID",
"企业利润表/项目/Value",
"企业利润表/行次/locID",
"企业利润表/行次/Value",
"企业利润表/本月数/locID",
"企业利润表/本年累计数/locID",
"企业利润表/locID",
"企业现金流量表/报表编号/locID",
"企业现金流量表/报表编号/Value",
"企业现金流量表/编制单位/locID",
"企业现金流量表/编制单位/Value",
"企业现金流量表/报告期/locID",
"企业现金流量表/报告期/Value",
"企业现金流量表/货币单位/locID",
"企业现金流量表/货币单位/Value",
"企业现金流量表/项目/locID",
"企业现金流量表/项目/Value",
"企业现金流量表/行次/locID",
"企业现金流量表/行次/Value",
"企业现金流量表/金额/locID",
"企业现金流量表/locID",
"企业应交增值税明细表/报表编号/locID",
"企业应交增值税明细表/报表编号/Value",
"企业应交增值税明细表/编制单位/locID",
"企业应交增值税明细表/编制单位/Value",
"企业应交增值税明细表/报告期/locID",
"企业应交增值税明细表/报告期/Value",
"企业应交增值税明细表/货币单位/locID",
"企业应交增值税明细表/货币单位/Value",
"企业应交增值税明细表/项目/locID",
"企业应交增值税明细表/项目/Value",
"企业应交增值税明细表/行次/locID",
"企业应交增值税明细表/行次/Value",
"企业应交增值税明细表/本月数/locID",
"企业应交增值税明细表/本年累计数/locID",
"企业应交增值税明细表/locID",
"企业资产减值准备明细表/报表编号/locID",
"企业资产减值准备明细表/报表编号/Value",
"企业资产减值准备明细表/编制单位/locID",
"企业资产减值准备明细表/编制单位/Value",
"企业资产减值准备明细表/报告期/locID",
"企业资产减值准备明细表/报告期/Value",
"企业资产减值准备明细表/货币单位/locID",
"企业资产减值准备明细表/货币单位/Value",
"企业资产减值准备明细表/项目/locID",
"企业资产减值准备明细表/项目/Value",
"企业资产减值准备明细表/年初余额/locID",
"企业资产减值准备明细表/本年增加数/locID",
"企业资产减值准备明细表/本年转回数/locID",
"企业资产减值准备明细表/年末金额/locID",
"企业资产减值准备明细表/locID",
"企业股东权益增减变动表/报表编号/locID",
"企业股东权益增减变动表/报表编号/Value",
"企业股东权益增减变动表/编制单位/locID",
"企业股东权益增减变动表/编制单位/Value",
"企业股东权益增减变动表/报告期/locID",
"企业股东权益增减变动表/报告期/Value",
"企业股东权益增减变动表/货币单位/locID",
"企业股东权益增减变动表/货币单位/Value",
"企业股东权益增减变动表/项目/locID",
"企业股东权益增减变动表/项目/Value",
"企业股东权益增减变动表/行次/locID",
"企业股东权益增减变动表/行次/Value",
"企业股东权益增减变动表/本年数/locID",
"企业股东权益增减变动表/上年数/locID",
"企业股东权益增减变动表/locID",
"企业利润分配表/报表编号/locID",
"企业利润分配表/报表编号/Value",
"企业利润分配表/编制单位/locID",
"企业利润分配表/编制单位/Value",
"企业利润分配表/报告期/locID",
"企业利润分配表/报告期/Value",
"企业利润分配表/货币单位/locID",
"企业利润分配表/货币单位/Value",
"企业利润分配表/项目/locID",
"企业利润分配表/项目/Value",
"企业利润分配表/行次/locID",
"企业利润分配表/行次/Value",
"企业利润分配表/本年实际/locID",
"企业利润分配表/上年实际/locID",
"企业利润分配表/locID",
"企业会计报表附注/locID",
"企业会计报表附注/Value","locID" };
			}
		}

		#endregion
	}
}
