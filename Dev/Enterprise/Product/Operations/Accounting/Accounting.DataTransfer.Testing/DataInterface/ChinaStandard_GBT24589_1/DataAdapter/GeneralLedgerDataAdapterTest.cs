using System;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(GeneralLedgerDataAdapter))]
	sealed class GeneralLedgerDataAdapterTest : BaseAccountingDataAdapterTest<BusinessObjectThatDoesntSaveForCN, XSDs.总账>
	{
		[TestDate(2012, 7, 22)]
		public void TestExportGeneralLedger()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);

			testHelper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			testHelper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			testHelper.SetupSinglePeriod(201207, new ZDateTime(2012, 7, 1), new ZDateTime(2012, 7, 31, 23, 59, 59));

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2PNL, "P&L", "D11");

			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test PNL 3", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test PNL 4", "P&L", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "63000.97", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3PNL, "P&L", "D12");

			AccGLAccountDescriptor testAccGLAccountDescriptor4PNL = testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "53000.98", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4PNL, glHeader4PNL, "P&L", "D13");

			testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1101m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(101m, 201001, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(201m, 201001, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-301m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1, "BSH", "D01");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2, "BSH", "D01");

			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 3", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 4", "BSH", Constants.DebitCredit.Credit);

			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3, "53000.97", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3, "BSH", "D02");
			AccGLAccountDescriptor testAccGLAccountDescriptor4 = testObjectCreator.CreateAccountDescriptor(glHeader4, "53000.98", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4, glHeader4, "BSH", "D03");

			testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1100m, 201001, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(100m, 201001, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(200m, 201001, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-300m, 200912, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");

			var accrual = testObjectCreator.CreateAccrual();
			accrual.AL_PostDate = new ZDateTime(2012, 07, 31, 23, 59, 59);
			testObjectCreator.CreateAccountDescriptor(accrual.AL_AG, "1000000.1", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "Desc", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
			testObjectCreator.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, "88.88.888.8", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "CostControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);

			Factory.Save();

			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
			using (StringWriter writer = new StringWriter())
			{
				BusinessObjectThatDoesntSaveForCN bizObj = new BusinessObjectThatDoesntSaveForCN(Factory);
				bizObj.Period = 201207;
				bizObj.FromDate = new ZDateTime(2012, 7, 1);
				bizObj.ToDate = new ZDateTime(2012, 8, 1);
				serialiser.WriteToXml(writer, Adapter, bizObj, context);
				string expectedText = Retriever.GetString("GeneralLedger.xml");
				this.AssertXMLEqualsByDiff("General Ledger XML does not match", expectedText, writer.ToString());
			}
			GlbCompany.CurrentCompany.SetCountry(countryCode);
		}

		[TestDate(2012, 7, 22)]
		public override void TestExportToValueObject_ForEmptyBizO()
		{
			base.TestExportToValueObject_ForEmptyBizO();
		}

		[TestDate(2012, 7, 22)]
		public override void TestExportToValueObject_ForPopulatedBizObjWithEmptyFields()
		{
			base.TestExportToValueObject_ForPopulatedBizObjWithEmptyFields();
		}

		[TestDate(2012, 7, 22)]
		public override void TestExportToValueObject_ForFullyPopulatedBizO()
		{
			base.TestExportToValueObject_ForFullyPopulatedBizO();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			GLLocalNumberFormatCollection list = new GLLocalNumberFormatCollection();
			GLLocalNumberFormat gNF = list.AddNew();
			gNF.NumberFormat = "4-2-2";
			gNF.CountryCode = Constants.CountryCodes.China;
			gNF.Language = SharedConstants.Languages.ChineseSimplified;
			gNF.IsFixedLength = true;
			AccountingMasterFilesRegistry.Instance.LocalNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			retriever?.Dispose();
			retriever = null;
		}

		EmbeddedResourceRetriever Retriever => retriever ??= new ();
		EmbeddedResourceRetriever retriever;

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, BusinessObjectThatDoesntSaveForCN bizObjToImportTo, XSDs.总账 exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
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

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSaveForCN, XSDs.总账> GetNewBizObjXmlDataAdapter()
		{
			return new GeneralLedgerDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyGeneralLedger.xml"), ValidationKind.None, "Fully Populated General Ledger Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyGeneralLedger.xml"), ValidationKind.None, "Semi Populated General Ledger Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyGeneralLedger.xml"), ValidationKind.None, "Empty General Ledger Files");
		}

		protected override BusinessObjectThatDoesntSaveForCN NewBusinessObject()
		{
			return new BusinessObjectThatDoesntSaveForCN(Factory);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		GeneralLedgerDataAdapter fAdapter;
		GeneralLedgerDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new GeneralLedgerDataAdapter()); }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				//Fully populated DirectDebitBatchHeader export tested explicitly in its own test
				return new[] { "总账基础信息/结构分隔符/locID",
"总账基础信息/结构分隔符/Value",
"总账基础信息/会计科目编号规则/locID",
"总账基础信息/会计科目编号规则/Value",
"总账基础信息/现金流量项目编码规则/locID",
"总账基础信息/现金流量项目编码规则/Value",
"总账基础信息/凭证头可扩展字段结构/locID",
"总账基础信息/凭证头可扩展字段结构/Value",
"总账基础信息/凭证头可扩展结构对应档案/locID",
"总账基础信息/凭证头可扩展结构对应档案/Value",
"总账基础信息/分录行可扩展字段结构/locID",
"总账基础信息/分录行可扩展字段结构/Value",
"总账基础信息/分录行可扩展字段对应档案/locID",
"总账基础信息/分录行可扩展字段对应档案/Value",
"记账凭证/分录行可扩展字段结构值/locID",
"记账凭证/分录行可扩展字段结构值/Value",
"总账基础信息/locID",
"会计科目/科目编号/locID",
"会计科目/科目编号/Value",
"会计科目/科目名称/locID",
"会计科目/科目名称/Value",
"会计科目/科目级次/locID",
"会计科目/科目类型/locID",
"会计科目/科目类型/Value",
"会计科目/余额方向/locID",
"会计科目/余额方向/Value",
"会计科目/locID",
"科目辅助核算/科目编号/locID",
"科目辅助核算/科目编号/Value",
"科目辅助核算/辅助项编号/locID",
"科目辅助核算/辅助项编号/Value",
"科目辅助核算/辅助项名称/locID",
"科目辅助核算/辅助项名称/Value",
"科目辅助核算/对应档案/locID",
"科目辅助核算/对应档案/Value",
"科目辅助核算/辅助项描述/locID",
"科目辅助核算/辅助项描述/Value",
"科目辅助核算/locID",
"现金流量项目/现金流量项目编码/locID",
"现金流量项目/现金流量项目编码/Value",
"现金流量项目/现金流量项目名称/locID",
"现金流量项目/现金流量项目名称/Value",
"现金流量项目/现金流量项目描述/locID",
"现金流量项目/现金流量项目描述/Value",
"现金流量项目/是否末级/locID",
"现金流量项目/是否末级/Value",
"现金流量项目/现金流量项目级次/locID",
"现金流量项目/现金流量项目级次/Value",
"现金流量项目/现金流量项目父节点/locID",
"现金流量项目/现金流量项目父节点/Value",
"现金流量项目/现金流量数据来源/locID",
"现金流量项目/现金流量数据来源/Value",
"现金流量项目/现金流量项目属性/locID",
"现金流量项目/现金流量项目属性/Value",
"现金流量项目/locID",
"科目余额及发生额/科目编号/locID",
"科目余额及发生额/科目编号/Value",
"科目余额及发生额/辅助项1编号/locID",
"科目余额及发生额/辅助项1编号/Value",
"科目余额及发生额/辅助项2编号/locID",
"科目余额及发生额/辅助项2编号/Value",
"科目余额及发生额/辅助项3编号/locID",
"科目余额及发生额/辅助项3编号/Value",
"科目余额及发生额/辅助项4编号/locID",
"科目余额及发生额/辅助项4编号/Value",
"科目余额及发生额/辅助项5编号/locID",
"科目余额及发生额/辅助项5编号/Value",
"科目余额及发生额/辅助项6编号/locID",
"科目余额及发生额/辅助项6编号/Value",
"科目余额及发生额/辅助项7编号/locID",
"科目余额及发生额/辅助项7编号/Value",
"科目余额及发生额/辅助项8编号/locID",
"科目余额及发生额/辅助项8编号/Value",
"科目余额及发生额/辅助项9编号/locID",
"科目余额及发生额/辅助项9编号/Value",
"科目余额及发生额/辅助项10编号/locID",
"科目余额及发生额/辅助项10编号/Value",
"科目余额及发生额/辅助项11编号/locID",
"科目余额及发生额/辅助项11编号/Value",
"科目余额及发生额/辅助项12编号/locID",
"科目余额及发生额/辅助项12编号/Value",
"科目余额及发生额/辅助项13编号/locID",
"科目余额及发生额/辅助项13编号/Value",
"科目余额及发生额/辅助项14编号/locID",
"科目余额及发生额/辅助项14编号/Value",
"科目余额及发生额/辅助项15编号/locID",
"科目余额及发生额/辅助项15编号/Value",
"科目余额及发生额/辅助项16编号/locID",
"科目余额及发生额/辅助项16编号/Value",
"科目余额及发生额/辅助项17编号/locID",
"科目余额及发生额/辅助项17编号/Value",
"科目余额及发生额/辅助项18编号/locID",
"科目余额及发生额/辅助项18编号/Value",
"科目余额及发生额/辅助项19编号/locID",
"科目余额及发生额/辅助项19编号/Value",
"科目余额及发生额/辅助项20编号/locID",
"科目余额及发生额/辅助项20编号/Value",
"科目余额及发生额/辅助项21编号/locID",
"科目余额及发生额/辅助项21编号/Value",
"科目余额及发生额/辅助项22编号/locID",
"科目余额及发生额/辅助项22编号/Value",
"科目余额及发生额/辅助项23编号/locID",
"科目余额及发生额/辅助项23编号/Value",
"科目余额及发生额/辅助项24编号/locID",
"科目余额及发生额/辅助项24编号/Value",
"科目余额及发生额/辅助项25编号/locID",
"科目余额及发生额/辅助项25编号/Value",
"科目余额及发生额/辅助项26编号/locID",
"科目余额及发生额/辅助项26编号/Value",
"科目余额及发生额/辅助项27编号/locID",
"科目余额及发生额/辅助项27编号/Value",
"科目余额及发生额/辅助项28编号/locID",
"科目余额及发生额/辅助项28编号/Value",
"科目余额及发生额/辅助项29编号/locID",
"科目余额及发生额/辅助项29编号/Value",
"科目余额及发生额/辅助项30编号/locID",
"科目余额及发生额/辅助项30编号/Value",
"科目余额及发生额/期初余额方向/locID",
"科目余额及发生额/期初余额方向/Value",
"科目余额及发生额/期末余额方向/locID",
"科目余额及发生额/期末余额方向/Value",
"科目余额及发生额/币种编码/locID",
"科目余额及发生额/币种编码/Value",
"科目余额及发生额/计量单位/locID",
"科目余额及发生额/计量单位/Value",
"科目余额及发生额/会计年度/locID",
"科目余额及发生额/会计年度/Value",
"科目余额及发生额/会计期间号/locID",
"科目余额及发生额/会计期间号/Value",
"科目余额及发生额/期初数量/locID",
"科目余额及发生额/期初原币余额/locID",
"科目余额及发生额/期初本币余额/locID",
"科目余额及发生额/借方数量/locID",
"科目余额及发生额/借方原币金额/locID",
"科目余额及发生额/借方本币金额/locID",
"科目余额及发生额/贷方数量/locID",
"科目余额及发生额/贷方原币金额/locID",
"科目余额及发生额/贷方本币金额/locID",
"科目余额及发生额/期末数量/locID",
"科目余额及发生额/期末原币余额/locID",
"科目余额及发生额/期末本币余额/locID",
"科目余额及发生额/locID",
"记账凭证/记账凭证日期/locID",
"记账凭证/记账凭证日期/Value",
"记账凭证/会计年度/locID",
"记账凭证/会计年度/Value",
"记账凭证/会计期间号/locID",
"记账凭证/会计期间号/Value",
"记账凭证/记账凭证类型编号/locID",
"记账凭证/记账凭证类型编号/Value",
"记账凭证/记账凭证编号/locID",
"记账凭证/记账凭证编号/Value",
"记账凭证/记账凭证行号/locID",
"记账凭证/记账凭证行号/Value",
"记账凭证/记账凭证摘要/locID",
"记账凭证/记账凭证摘要/Value",
"记账凭证/科目编号/locID",
"记账凭证/科目编号/Value",
"记账凭证/辅助项1编号/locID",
"记账凭证/辅助项1编号/Value",
"记账凭证/辅助项2编号/locID",
"记账凭证/辅助项2编号/Value",
"记账凭证/辅助项3编号/locID",
"记账凭证/辅助项3编号/Value",
"记账凭证/辅助项4编号/locID",
"记账凭证/辅助项4编号/Value",
"记账凭证/辅助项5编号/locID",
"记账凭证/辅助项5编号/Value",
"记账凭证/辅助项6编号/locID",
"记账凭证/辅助项6编号/Value",
"记账凭证/辅助项7编号/locID",
"记账凭证/辅助项7编号/Value",
"记账凭证/辅助项8编号/locID",
"记账凭证/辅助项8编号/Value",
"记账凭证/辅助项9编号/locID",
"记账凭证/辅助项9编号/Value",
"记账凭证/辅助项10编号/locID",
"记账凭证/辅助项10编号/Value",
"记账凭证/辅助项11编号/locID",
"记账凭证/辅助项11编号/Value",
"记账凭证/辅助项12编号/locID",
"记账凭证/辅助项12编号/Value",
"记账凭证/辅助项13编号/locID",
"记账凭证/辅助项13编号/Value",
"记账凭证/辅助项14编号/locID",
"记账凭证/辅助项14编号/Value",
"记账凭证/辅助项15编号/locID",
"记账凭证/辅助项15编号/Value",
"记账凭证/辅助项16编号/locID",
"记账凭证/辅助项16编号/Value",
"记账凭证/辅助项17编号/locID",
"记账凭证/辅助项17编号/Value",
"记账凭证/辅助项18编号/locID",
"记账凭证/辅助项18编号/Value",
"记账凭证/辅助项19编号/locID",
"记账凭证/辅助项19编号/Value",
"记账凭证/辅助项20编号/locID",
"记账凭证/辅助项20编号/Value",
"记账凭证/辅助项21编号/locID",
"记账凭证/辅助项21编号/Value",
"记账凭证/辅助项22编号/locID",
"记账凭证/辅助项22编号/Value",
"记账凭证/辅助项23编号/locID",
"记账凭证/辅助项23编号/Value",
"记账凭证/辅助项24编号/locID",
"记账凭证/辅助项24编号/Value",
"记账凭证/辅助项25编号/locID",
"记账凭证/辅助项25编号/Value",
"记账凭证/辅助项26编号/locID",
"记账凭证/辅助项26编号/Value",
"记账凭证/辅助项27编号/locID",
"记账凭证/辅助项27编号/Value",
"记账凭证/辅助项28编号/locID",
"记账凭证/辅助项28编号/Value",
"记账凭证/辅助项29编号/locID",
"记账凭证/辅助项29编号/Value",
"记账凭证/辅助项30编号/locID",
"记账凭证/辅助项30编号/Value",
"记账凭证/币种编码/locID",
"记账凭证/币种编码/Value",
"记账凭证/计量单位/locID",
"记账凭证/计量单位/Value",
"记账凭证/借方数量/locID",
"记账凭证/借方原币金额/locID",
"记账凭证/借方本币金额/locID",
"记账凭证/贷方数量/locID",
"记账凭证/贷方原币金额/locID",
"记账凭证/贷方本币金额/locID",
"记账凭证/汇率类型编号/locID",
"记账凭证/汇率类型编号/Value",
"记账凭证/汇率/locID",
"记账凭证/单价/locID",
"记账凭证/凭证头可扩展字段结构值/locID",
"记账凭证/凭证头可扩展字段结构值/Value",
"记账凭证/分录行可扩展字段结构/locID",
"记账凭证/分录行可扩展字段结构/Value",
"记账凭证/结算方式编码/locID",
"记账凭证/结算方式编码/Value",
"记账凭证/票据类型/locID",
"记账凭证/票据类型/Value",
"记账凭证/票据号/locID",
"记账凭证/票据号/Value",
"记账凭证/票据日期/locID",
"记账凭证/票据日期/Value",
"记账凭证/附件数/locID",
"记账凭证/制单人/locID",
"记账凭证/制单人/Value",
"记账凭证/审核人/locID",
"记账凭证/审核人/Value",
"记账凭证/记账人/locID",
"记账凭证/记账人/Value",
"记账凭证/记账标志/locID",
"记账凭证/记账标志/Value",
"记账凭证/作废标志/locID",
"记账凭证/作废标志/Value",
"记账凭证/凭证来源系统/locID",
"记账凭证/凭证来源系统/Value",
"记账凭证/locID",
"现金流量凭证项目数据/记账凭证类型编号/locID",
"现金流量凭证项目数据/记账凭证类型编号/Value",
"现金流量凭证项目数据/记账凭证编号/locID",
"现金流量凭证项目数据/记账凭证编号/Value",
"现金流量凭证项目数据/币种编码/locID",
"现金流量凭证项目数据/币种编码/Value",
"现金流量凭证项目数据/现金流量行号/locID",
"现金流量凭证项目数据/现金流量行号/Value",
"现金流量凭证项目数据/现金流量摘要/locID",
"现金流量凭证项目数据/现金流量摘要/Value",
"现金流量凭证项目数据/现金流量项目编码/locID",
"现金流量凭证项目数据/现金流量项目编码/Value",
"现金流量凭证项目数据/现金流量项目属性/locID",
"现金流量凭证项目数据/现金流量项目属性/Value",
"现金流量凭证项目数据/现金流量原币金额/locID",
"现金流量凭证项目数据/现金流量本币金额/locID",
"现金流量凭证项目数据/locID",
"报表集/报表编号/locID",
"报表集/报表编号/Value",
"报表集/报表名称/locID",
"报表集/报表名称/Value",
"报表集/报表报告日/locID",
"报表集/报表报告日/Value",
"报表集/报表报告期/locID",
"报表集/报表报告期/Value",
"报表集/编制单位/locID",
"报表集/编制单位/Value",
"报表集/货币单位/locID",
"报表集/货币单位/Value",
"报表集/locID",
"报表项数据/报表编号/locID",
"报表项数据/报表编号/Value",
"报表项数据/报表项编号/locID",
"报表项数据/报表项编号/Value",
"报表项数据/报表项名称/locID",
"报表项数据/报表项名称/Value",
"报表项数据/报表项公式/locID",
"报表项数据/报表项公式/Value",
"报表项数据/报表项数值/locID",
"报表项数据/locID",
"locID"
									};
			}
		}

		#endregion
	}
}
