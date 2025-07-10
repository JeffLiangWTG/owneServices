using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusMAWB))]
	sealed class CusMAWBWorkflowProviderTest : WorkflowProviderTest<CusMAWB, ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>>
	{
		[StressTest]
		public void TestResetCustomBusinessObject()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template1.P0_Name = "TEST 1";
			template1.P0_Description = "TEST 1 DESC";
			template1.P0_SubType1 = "QF";
			template1.P0_LoadPortCountry = "AUSYD";
			template1.P0_DischargePortCountry = "NZAKL";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = template1.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template2.P0_Name = "TEST 2";
			template2.P0_Description = "TEST 2 DESC";
			template2.P0_SubType1 = "NZ";
			template2.P0_LoadPortCountry = "AUSYD";
			template2.P0_DischargePortCountry = "NZAKL";
			var customField3 = template2.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "decimalField";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;

			var template3 = Factory.New<ProcessTaskTemplate>();
			template3.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template3.P0_Name = "TEST 3";
			template3.P0_Description = "TEST 3 DESC";
			template3.P0_SubType1 = "NZ";
			template3.P0_LoadPortCountry = "AUMEL";
			template3.P0_DischargePortCountry = "NZAKL";
			var customField4 = template3.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "shortField";
			customField4.XC_Type = AddOnColumnDataType.Codes.Short;

			var template4 = Factory.New<ProcessTaskTemplate>();
			template4.P0_ProcessType = JobInvoicingConsumerTypes.CusMAWBCode;
			template4.P0_Name = "TEST 4";
			template4.P0_Description = "TEST 4 DESC";
			template4.P0_SubType1 = "NZ";
			template4.P0_LoadPortCountry = "AUMEL";
			template4.P0_DischargePortCountry = "NZCHC";
			var customField5 = template4.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "datetimeField";
			customField5.XC_Type = AddOnColumnDataType.Codes.Datetime;
			Factory.Save();

			var mawb = Factory.New<CusMAWB>();
			mawb.CM_FlightNo = "QF123";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			var resetCount = 0;
			mawb.OnResetCustomBusinessObject = () => resetCount++;
			ICustomFieldProvider customFieldProvider = mawb;
			ICustomPropertyContainer customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			var properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			mawb.CM_FlightNo = "QF456";
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			AssertEquals("resetCount", 0, resetCount);
			mawb.CM_FlightNo = "NZ456";
			AssertEquals("resetCount", 1, resetCount);
			var oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("DECIMALFIELD", properties[0]);
			AssertEquals("resetCount", 1, resetCount);
			mawb.CM_RL_NKLoadPort = "AUMEL";
			AssertEquals("resetCount", 2, resetCount);
			oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("SHORTFIELD", properties[0]);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("SHORTFIELD", properties[0]);
			AssertEquals("resetCount", 2, resetCount);
			mawb.CM_RL_NKDischargePort = "NZCHC";
			AssertEquals("resetCount", 3, resetCount);
			oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("DATETIMEFIELD", properties[0]);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("DATETIMEFIELD", properties[0]);
		}

		public void TestWorkflowDoNotGetAddedFromTemplateToConsolCusMAWB()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ACR";

			var workflowItem = template.WorkflowItems.AddNew();
			workflowItem.P9_Description = "TEST";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_JK = consol.PK;
			Factory.Save();

			AssertEquals("Should not add to Consol CusMAWB", 0, ((IWorkflowProvider)mawb).WorkflowItems.Count);
		}

		public void TestIDocManagerSupportIncudingRelatedObjectsMembers()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var underbond = mawb.AllUnderbonds.AddNew();
			var supporter = mawb as IDocManagerSupportIncudingRelatedObjects;

			AssertEquals("Self reference", mawb, supporter.SelfReference);
			AssertEquals(2, supporter.GetRelatedBusinessObjects().Count());
			Assert(supporter.GetRelatedBusinessObjects().Contains(hawb));
			Assert(supporter.GetRelatedBusinessObjects().Contains(underbond));
			AssertType(typeof(DocManagerIncludingRelatedObjectsInfo), ((IDocManagerSupport)mawb).DocManagerInfo);
		}

		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var mawb = Factory.New<CusMAWB>();
			var fileNameProvider = mawb as IDataExportCSVFileNameProvider;
			mawb.CM_MAWB = "123456";
			AssertEquals("File name suffix", "123456", fileNameProvider.FileNameSuffix);
		}

		public void TestGetTemplateSelectionCriteria()
		{
			AssertHasJobRelatedTemplateFilterCriteria((CusMAWB workflowProvider, OrgHeader consignee, OrgHeader consignor, string originCode, string destinationCode) =>
			{
				workflowProvider.CM_RL_NKLoadPort = originCode;
				workflowProvider.CM_RL_NKDischargePort = destinationCode;
			}, false);

			BusinessObject.CM_FlightNo = "QF123";
			ColumnValueRanker ranker = (ColumnValueRanker)((IWorkflowProvider)BusinessObject).GetTemplateSelectionCriteria();
			AssertArrayEqualsByElements(new object[] { "QF", "" }, ranker.GetValues(ProcessTaskTemplateSchema.P0_SubType1));
		}

		protected override ZString ExpectedWorkflowType => JobInvoicingConsumerTypes.CusMAWB.Code;
	}
}
