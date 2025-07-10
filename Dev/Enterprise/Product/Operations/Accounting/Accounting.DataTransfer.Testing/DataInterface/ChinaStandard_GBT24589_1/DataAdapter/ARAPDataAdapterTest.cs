using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(ARAPDataAdapter))]
	sealed class ARAPDataAdapterTest : BaseAccountingDataAdapterTest<BusinessObjectThatDoesntSaveForCN, XSDs.应收应付>
	{
		[TestDate(2006, 03, 29, 15, 39, 42)]
		public void TestExportARAPData()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				PeriodTestHelper.SetupSinglePeriod(200603, new ZDateTime(2006, 3, 1), new ZDateTime(2006, 3, 31, 23, 59, 00));

				string exportFileName = Path.Combine(EnvProxy.Instance.TempPath, "GBT245891ChinaStandard应收应付20060329153942.xml");

				try
				{
					AssertEquals("Expected export file should not exist", false, File.Exists(exportFileName));
					ChinaStandard2010DataInterfaceWrapper bizObj = new ChinaStandard2010DataInterfaceWrapper(Factory) { Period = 200603, ExportDirectory = EnvProxy.Instance.TempPath };
					ChinaStandard2010DataInterfaceExporter exporter = new ChinaStandard2010DataInterfaceExporter(bizObj, new NotificationBuffer());
					Assert(exporter.ExportData(new ARAPDataAdapter()));
					AssertEquals("Expected export file should exist", true, File.Exists(exportFileName));

					string expectedExportData = Retriever.GetString("ARAPData.xml", Encoding.GetEncoding("GB18030"));

					string actualExportData = File.ReadAllText(exportFileName);

					this.AssertXMLEqualsByDiff("Exporter is not exporting what is expected", expectedExportData, actualExportData);
				}
				finally
				{
					DeleteIfExists(exportFileName);
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

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, BusinessObjectThatDoesntSaveForCN bizObjToImportTo, XSDs.应收应付 exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
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

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSaveForCN, XSDs.应收应付> GetNewBizObjXmlDataAdapter()
		{
			return new ARAPDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyARAPData.xml"), ValidationKind.None, "Fully Populated AR AP Data Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyARAPData.xml"), ValidationKind.None, "Semi Populated AR AP Data Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSaveForCN(Factory), Retriever.SaveResourceToFile("EmptyARAPData.xml"), ValidationKind.None, "Empty AR AP Data Files");
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

		ARAPDataAdapter fAdapter;
		ARAPDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new ARAPDataAdapter()); }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				//Fully populated DirectDebitBatchHeader export tested explicitly in its own test
				return new string[] { "单据类型/单据类型编码/locID",
"单据类型/单据类型编码/Value",
"单据类型/单据类型名称/locID",
"单据类型/单据类型名称/Value",
"单据类型/locID",
"交易类型/交易类型编码/locID",
"交易类型/交易类型编码/Value",
"交易类型/交易类型名称/locID",
"交易类型/交易类型名称/Value",
"交易类型/locID",
"应收明细表/客户编码/locID",
"应收明细表/客户编码/Value",
"应收明细表/科目编号/locID",
"应收明细表/科目编号/Value",
"应收明细表/记账凭证日期/locID",
"应收明细表/记账凭证日期/Value",
"应收明细表/记账日期/locID",
"应收明细表/记账日期/Value",
"应收明细表/会计年度/locID",
"应收明细表/会计年度/Value",
"应收明细表/会计期间号/locID",
"应收明细表/会计期间号/Value",
"应收明细表/记账凭证类型编号/locID",
"应收明细表/记账凭证类型编号/Value",
"应收明细表/记账凭证编号/locID",
"应收明细表/记账凭证编号/Value",
"应收明细表/本位币/locID",
"应收明细表/本位币/Value",
"应收明细表/汇率/locID",
"应收明细表/余额方向/locID",
"应收明细表/余额方向/Value",
"应收明细表/本币余额/locID",
"应收明细表/原币余额/locID",
"应收明细表/本币发生金额/locID",
"应收明细表/原币币种/locID",
"应收明细表/原币币种/Value",
"应收明细表/原币发生金额/locID",
"应收明细表/摘要/locID",
"应收明细表/摘要/Value",
"应收明细表/到期日/locID",
"应收明细表/到期日/Value",
"应收明细表/核销凭证编号/locID",
"应收明细表/核销凭证编号/Value",
"应收明细表/核销日期/locID",
"应收明细表/核销日期/Value",
"应收明细表/单据类型编码/locID",
"应收明细表/单据类型编码/Value",
"应收明细表/交易类型编码/locID",
"应收明细表/交易类型编码/Value",
"应收明细表/单据编号/locID",
"应收明细表/单据编号/Value",
"应收明细表/发票号/locID",
"应收明细表/发票号/Value",
"应收明细表/合同号/locID",
"应收明细表/合同号/Value",
"应收明细表/项目编码/locID",
"应收明细表/项目编码/Value",
"应收明细表/结算方式编码/locID",
"应收明细表/结算方式编码/Value",
"应收明细表/付款日期/locID",
"应收明细表/付款日期/Value",
"应收明细表/核销标志/locID",
"应收明细表/核销标志/Value",
"应收明细表/汇票编号/locID",
"应收明细表/汇票编号/Value",
"应收明细表/locID",
"应付明细表/供应商编码/locID",
"应付明细表/供应商编码/Value",
"应付明细表/科目编号/locID",
"应付明细表/科目编号/Value",
"应付明细表/记账凭证日期/locID",
"应付明细表/记账凭证日期/Value",
"应付明细表/记账日期/locID",
"应付明细表/记账日期/Value",
"应付明细表/会计年度/locID",
"应付明细表/会计年度/Value",
"应付明细表/会计期间号/locID",
"应付明细表/会计期间号/Value",
"应付明细表/记账凭证类型编号/locID",
"应付明细表/记账凭证类型编号/Value",
"应付明细表/记账凭证编号/locID",
"应付明细表/记账凭证编号/Value",
"应付明细表/本位币/locID",
"应付明细表/本位币/Value",
"应付明细表/汇率/locID",
"应付明细表/余额方向/locID",
"应付明细表/余额方向/Value",
"应付明细表/本币余额/locID",
"应付明细表/原币余额/locID",
"应付明细表/本币发生金额/locID",
"应付明细表/原币币种/locID",
"应付明细表/原币币种/Value",
"应付明细表/原币发生金额/locID",
"应付明细表/摘要/locID",
"应付明细表/摘要/Value",
"应付明细表/到期日/locID",
"应付明细表/到期日/Value",
"应付明细表/核销凭证编号/locID",
"应付明细表/核销凭证编号/Value",
"应付明细表/核销日期/locID",
"应付明细表/核销日期/Value",
"应付明细表/单据类型编码/locID",
"应付明细表/单据类型编码/Value",
"应付明细表/交易类型编码/locID",
"应付明细表/交易类型编码/Value",
"应付明细表/单据编号/locID",
"应付明细表/单据编号/Value",
"应付明细表/发票号/locID",
"应付明细表/发票号/Value",
"应付明细表/合同号/locID",
"应付明细表/合同号/Value",
"应付明细表/项目编码/locID",
"应付明细表/项目编码/Value",
"应付明细表/结算方式编码/locID",
"应付明细表/结算方式编码/Value",
"应付明细表/付款日期/locID",
"应付明细表/付款日期/Value",
"应付明细表/核销标志/locID",
"应付明细表/核销标志/Value",
"应付明细表/汇票编号/locID",
"应付明细表/汇票编号/Value",
"应付明细表/locID",
"locID"
};
			}
		}

		#endregion

	}
}
