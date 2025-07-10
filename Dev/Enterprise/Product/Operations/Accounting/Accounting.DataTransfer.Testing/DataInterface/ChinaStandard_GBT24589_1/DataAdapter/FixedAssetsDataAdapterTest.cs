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
	[TestedType(typeof(FixedAssetsDataAdapter))]
	sealed class FixedAssetsDataAdapterTest : BaseAccountingDataAdapterTest<BusinessObjectThatDoesntSave, XSDs.固定资产>
	{
		public void TestExportFixedAssets()
		{
			ValueObjectExportContext context = new ValueObjectExportContext(new NotificationBuffer());
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(Adapter.ValueObjectType);
			using (StringWriter writer = new StringWriter())
			{
				serialiser.WriteToXml(writer, Adapter, new BusinessObjectThatDoesntSave(Factory), context);
				string expectedText = File.ReadAllText(Retriever.SaveResourceToFile("FixedAssets.xml"));
				this.AssertXMLEqualsByDiff("Fixed Assets XML does not match", expectedText, writer.ToString());
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

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, BusinessObjectThatDoesntSave bizObjToImportTo, XSDs.固定资产 exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
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

		protected override ValueObjectDataAdapter<BusinessObjectThatDoesntSave, XSDs.固定资产> GetNewBizObjXmlDataAdapter()
		{
			return new FixedAssetsDataAdapter();
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyFixedAssets.xml"), ValidationKind.None, "Fully Populated Fixed Assets Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyFixedAssets.xml"), ValidationKind.None, "Semi Populated Fixed Assets Files");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return new BusinessObjectAndExpectedOutputFileName(new BusinessObjectThatDoesntSave(Factory), Retriever.SaveResourceToFile("EmptyFixedAssets.xml"), ValidationKind.None, "Empty Fixed Assets Files");
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

		FixedAssetsDataAdapter fAdapter;
		FixedAssetsDataAdapter Adapter
		{
			get { return fAdapter ?? (fAdapter = new FixedAssetsDataAdapter()); }
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				//Fully populated DirectDebitBatchHeader export tested explicitly in its own test
				return new[] { "固定资产基础信息/固定资产对账科目/locID",
"固定资产基础信息/固定资产对账科目/Value",
"固定资产基础信息/减值准备对账科目/locID",
"固定资产基础信息/减值准备对账科目/Value",
"固定资产基础信息/累计折旧对账科目/locID",
"固定资产基础信息/累计折旧对账科目/Value",
"固定资产基础信息/locID",
"固定资产类别设置/固定资产类别编码规则/locID",
"固定资产类别设置/固定资产类别编码规则/Value",
"固定资产类别设置/固定资产类别编码/locID",
"固定资产类别设置/固定资产类别编码/Value",
"固定资产类别设置/固定资产类别名称/locID",
"固定资产类别设置/固定资产类别名称/Value",
"固定资产类别设置/locID",
"固定资产变动方式/变动方式编码/locID",
"固定资产变动方式/变动方式编码/Value",
"固定资产变动方式/变动方式名称/locID",
"固定资产变动方式/变动方式名称/Value",
"固定资产变动方式/locID",
"固定资产折旧方法/折旧方法编码/locID",
"固定资产折旧方法/折旧方法编码/Value",
"固定资产折旧方法/折旧方法名称/locID",
"固定资产折旧方法/折旧方法名称/Value",
"固定资产折旧方法/折旧公式/locID",
"固定资产折旧方法/折旧公式/Value",
"固定资产折旧方法/locID",
"固定资产使用状况/使用状况编码/locID",
"固定资产使用状况/使用状况编码/Value",
"固定资产使用状况/使用状况名称/locID",
"固定资产使用状况/使用状况名称/Value",
"固定资产使用状况/locID",
"固定资产卡片/固定资产卡片编号/locID",
"固定资产卡片/固定资产卡片编号/Value",
"固定资产卡片/固定资产类别编码/locID",
"固定资产卡片/固定资产类别编码/Value",
"固定资产卡片/固定资产编码/locID",
"固定资产卡片/固定资产编码/Value",
"固定资产卡片/固定资产名称/locID",
"固定资产卡片/固定资产名称/Value",
"固定资产卡片/固定资产入账日期/locID",
"固定资产卡片/固定资产入账日期/Value",
"固定资产卡片/会计期间号/locID",
"固定资产卡片/会计期间号/Value",
"固定资产卡片/固定资产计量单位/locID",
"固定资产卡片/固定资产计量单位/Value",
"固定资产卡片/固定资产数量/locID",
"固定资产卡片/变动方式编码/locID",
"固定资产卡片/变动方式编码/Value",
"固定资产卡片/折旧方法编码/locID",
"固定资产卡片/折旧方法编码/Value",
"固定资产卡片/使用状况编码/locID",
"固定资产卡片/使用状况编码/Value",
"固定资产卡片/预计使用月份/locID",
"固定资产卡片/已计提月份/locID",
"固定资产卡片/本位币/locID",
"固定资产卡片/本位币/Value",
"固定资产卡片/固定资产原值/locID",
"固定资产卡片/固定资产累计折旧/locID",
"固定资产卡片/固定资产净值/locID",
"固定资产卡片/固定资产累计减值准备/locID",
"固定资产卡片/固定资产净残值率/locID",
"固定资产卡片/固定资产净残值/locID",
"固定资产卡片/固定资产月折旧率/locID",
"固定资产卡片/固定资产月折旧额/locID",
"固定资产卡片/固定资产工作量单位/locID",
"固定资产卡片/固定资产工作量单位/Value",
"固定资产卡片/固定资产工作总量/locID",
"固定资产卡片/累计工作总量/locID",
"固定资产卡片/固定资产对账科目/locID",
"固定资产卡片/固定资产对账科目/Value",
"固定资产卡片/减值准备对账科目/locID",
"固定资产卡片/减值准备对账科目/Value",
"固定资产卡片/累计折旧对账科目/locID",
"固定资产卡片/累计折旧对账科目/Value",
"固定资产卡片/locID",
"固定资产卡片实物信息/固定资产卡片编号/locID",
"固定资产卡片实物信息/固定资产卡片编号/Value",
"固定资产卡片实物信息/会计期间号/locID",
"固定资产卡片实物信息/会计期间号/Value",
"固定资产卡片实物信息/固定资产标签号/locID",
"固定资产卡片实物信息/固定资产标签号/Value",
"固定资产卡片实物信息/固定资产位置/locID",
"固定资产卡片实物信息/固定资产位置/Value",
"固定资产卡片实物信息/固定资产规格型号/locID",
"固定资产卡片实物信息/固定资产规格型号/Value",
"固定资产卡片实物信息/locID",
"固定资产卡片使用信息/固定资产卡片编号/locID",
"固定资产卡片使用信息/固定资产卡片编号/Value",
"固定资产卡片使用信息/固定资产标签号/locID",
"固定资产卡片使用信息/固定资产标签号/Value",
"固定资产卡片使用信息/会计期间号/locID",
"固定资产卡片使用信息/会计期间号/Value",
"固定资产卡片使用信息/部门编码/locID",
"固定资产卡片使用信息/部门编码/Value",
"固定资产卡片使用信息/折旧分配比例/locID",
"固定资产卡片使用信息/locID",
"固定资产减少情况/固定资产减少流水号/locID",
"固定资产减少情况/固定资产减少流水号/Value",
"固定资产减少情况/减少发生日期/locID",
"固定资产减少情况/减少发生日期/Value",
"固定资产减少情况/会计期间号/locID",
"固定资产减少情况/会计期间号/Value",
"固定资产减少情况/变动方式编码/locID",
"固定资产减少情况/变动方式编码/Value",
"固定资产减少情况/固定资产卡片编号/locID",
"固定资产减少情况/固定资产卡片编号/Value",
"固定资产减少情况/固定资产名称/locID",
"固定资产减少情况/固定资产名称/Value",
"固定资产减少情况/固定资产编码/locID",
"固定资产减少情况/固定资产编码/Value",
"固定资产减少情况/固定资产减少数量/locID",
"固定资产减少情况/固定资产减少原值/locID",
"固定资产减少情况/固定资产减少累计折旧/locID",
"固定资产减少情况/固定资产减少减值准备/locID",
"固定资产减少情况/固定资产减少残值/locID",
"固定资产减少情况/清理收入/locID",
"固定资产减少情况/清理费用/locID",
"固定资产减少情况/固定资产减少原因/locID",
"固定资产减少情况/固定资产减少原因/Value",
"固定资产减少情况/locID",
"固定资产减少实物信息/固定资产减少流水号/locID",
"固定资产减少实物信息/固定资产减少流水号/Value",
"固定资产减少实物信息/固定资产卡片编号/locID",
"固定资产减少实物信息/固定资产卡片编号/Value",
"固定资产减少实物信息/固定资产标签号/locID",
"固定资产减少实物信息/固定资产标签号/Value",
"固定资产减少实物信息/会计期间号/locID",
"固定资产减少实物信息/会计期间号/Value",
"固定资产减少实物信息/locID",
"固定资产变动情况/固定资产变动流水号/locID",
"固定资产变动情况/固定资产变动流水号/Value",
"固定资产变动情况/固定资产变动日期/locID",
"固定资产变动情况/固定资产变动日期/Value",
"固定资产变动情况/会计期间号/locID",
"固定资产变动情况/会计期间号/Value",
"固定资产变动情况/固定资产卡片编号/locID",
"固定资产变动情况/固定资产卡片编号/Value",
"固定资产变动情况/固定资产编码/locID",
"固定资产变动情况/固定资产编码/Value",
"固定资产变动情况/固定资产名称/locID",
"固定资产变动情况/固定资产名称/Value",
"固定资产变动情况/变动方式编码/locID",
"固定资产变动情况/变动方式编码/Value",
"固定资产变动情况/固定资产标签号/locID",
"固定资产变动情况/固定资产标签号/Value",
"固定资产变动情况/变动前内容及数值/locID",
"固定资产变动情况/变动前内容及数值/Value",
"固定资产变动情况/变动后内容及数值/locID",
"固定资产变动情况/变动后内容及数值/Value",
"固定资产变动情况/固定资产变动原因/locID",
"固定资产变动情况/固定资产变动原因/Value",
"固定资产变动情况/locID",
"locID"
									};
			}
		}

		#endregion

	}
}
