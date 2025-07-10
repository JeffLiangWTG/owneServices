using System.IO;
using CargoWise.IO;
using Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	[TestedType(typeof(PayrollsDataAdapter))]
	sealed class PayrollsDataAdapterTest : BaseAccountingDataAdapterTest<BusinessObjectThatDoesntSave, XSDs.员工薪酬>
	{
		public void TestExportPayrolls()
		{
			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
			using (StringWriter writer = new StringWriter())
			{
				serialiser.WriteToXml(writer, Adapter, new BusinessObjectThatDoesntSave(Factory), context);
				string expectedText = Retriever.GetString("Payrolls.xml");
				this.AssertXMLEqualsByDiff("Payrolls XML does not match", expectedText, writer.ToString());
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

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, BusinessObjectThatDoesntSave bizObjToImportTo, XSDs.员工薪酬 exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
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

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSave, XSDs.员工薪酬> GetNewBizObjXmlDataAdapter()
		{
			return new PayrollsDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyPayrolls.xml"), ValidationKind.None, "Fully Populated Payrolls Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyPayrolls.xml"), ValidationKind.None, "Semi Populated Payrolls Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyPayrolls.xml"), ValidationKind.None, "Empty Payrolls Files");
		}

		protected override BusinessObjectThatDoesntSave NewBusinessObject()
		{
			return new BusinessObjectThatDoesntSave(Factory);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return Adapter.RootCollectionElementName; }
		}

		protected override string ExpectedRootElementName
		{
			get { return Adapter.RootElementName; }
		}

		PayrollsDataAdapter fAdapter;
		PayrollsDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new PayrollsDataAdapter()); }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				//Fully populated DirectDebitBatchHeader export tested explicitly in its own test
				return new[] { "薪酬期间/薪酬年度/locID",
"薪酬期间/薪酬年度/Value",
"薪酬期间/薪酬期间号/locID",
"薪酬期间/薪酬期间号/Value",
"薪酬期间/薪酬期间起始日期/locID",
"薪酬期间/薪酬期间起始日期/Value",
"薪酬期间/薪酬期间结束日期/locID",
"薪酬期间/薪酬期间结束日期/Value",
"薪酬期间/locID",
"薪酬项目/薪酬类别名称/locID",
"薪酬项目/薪酬类别名称/Value",
"薪酬项目/薪酬项目编码/locID",
"薪酬项目/薪酬项目编码/Value",
"薪酬项目/薪酬项目名称/locID",
"薪酬项目/薪酬项目名称/Value",
"薪酬项目/locID",
"员工薪酬记录/员工编码/locID",
"员工薪酬记录/员工编码/Value",
"员工薪酬记录/员工类别/locID",
"员工薪酬记录/员工类别/Value",
"员工薪酬记录/部门编码/locID",
"员工薪酬记录/部门编码/Value",
"员工薪酬记录/薪酬类别名称/locID",
"员工薪酬记录/薪酬类别名称/Value",
"员工薪酬记录/薪酬年度/locID",
"员工薪酬记录/薪酬年度/Value",
"员工薪酬记录/薪酬期间号/locID",
"员工薪酬记录/薪酬期间号/Value",
"员工薪酬记录/会计年度/locID",
"员工薪酬记录/会计年度/Value",
"员工薪酬记录/会计期间号/locID",
"员工薪酬记录/会计期间号/Value",
"员工薪酬记录/币种编码/locID",
"员工薪酬记录/币种编码/Value",
"员工薪酬记录/locID",
"员工薪酬记录明细/员工编码/locID",
"员工薪酬记录明细/员工编码/Value",
"员工薪酬记录明细/薪酬类别名称/locID",
"员工薪酬记录明细/薪酬类别名称/Value",
"员工薪酬记录明细/薪酬年度/locID",
"员工薪酬记录明细/薪酬年度/Value",
"员工薪酬记录明细/薪酬期间号/locID",
"员工薪酬记录明细/薪酬期间号/Value",
"员工薪酬记录明细/薪酬项目编码/locID",
"员工薪酬记录明细/薪酬项目编码/Value",
"员工薪酬记录明细/薪酬金额/locID",
"员工薪酬记录明细/locID",
"locID"
									};
			}
		}

		#endregion
	}
}
