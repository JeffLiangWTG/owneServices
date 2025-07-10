using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWB))]
	sealed class CusHAWBWorkflowProviderTest : WorkflowProviderTest<CusHAWB, ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>>
	{
		public void TestResetCustomBusinessObject()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OK!@#1";
			org1.OH_FullName = "ORG 1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OK!@#2";
			org2.OH_FullName = "ORG 2";

			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template1.P0_Name = "TEST 1";
			template1.P0_Description = "TEST 1 DESC";
			template1.P0_OH_Client = org1.PK;
			template1.P0_LoadPortCountry = "AUSYD";
			template1.P0_DischargePortCountry = "NZAKL";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = template1.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template2.P0_Name = "TEST 2";
			template2.P0_Description = "TEST 2 DESC";
			template2.P0_OH_Client = org2.PK;
			template2.P0_LoadPortCountry = "AUSYD";
			template2.P0_DischargePortCountry = "NZAKL";
			var customField3 = template2.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "decimalField";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;

			var template3 = Factory.New<ProcessTaskTemplate>();
			template3.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template3.P0_Name = "TEST 3";
			template3.P0_Description = "TEST 3 DESC";
			template3.P0_OH_Client = org2.PK;
			template3.P0_LoadPortCountry = "AUMEL";
			template3.P0_DischargePortCountry = "NZAKL";
			var customField4 = template3.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "shortField";
			customField4.XC_Type = AddOnColumnDataType.Codes.Short;

			var template4 = Factory.New<ProcessTaskTemplate>();
			template4.P0_ProcessType = WorkflowDescriptors.CustomsHouseAirCargoCode;
			template4.P0_Name = "TEST 4";
			template4.P0_Description = "TEST 4 DESC";
			template4.P0_OH_Client = org2.PK;
			template4.P0_LoadPortCountry = "AUMEL";
			template4.P0_DischargePortCountry = "NZCHC";
			var customField5 = template4.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "datetimeField";
			customField5.XC_Type = AddOnColumnDataType.Codes.Datetime;
			Factory.Save();

			var hawb = Factory.New<CusHAWB>();
			hawb.CS_OH_Consignee = org1.PK;
			hawb.CS_RL_NKOrigin = "AUSYD";
			hawb.CS_RL_NKDestination = "NZAKL";
			var resetCount = 0;
			hawb.OnResetCustomBusinessObject = () => resetCount++;
			ICustomFieldProvider customFieldProvider = hawb;
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
			AssertEquals("resetCount", 0, resetCount);

			hawb.CS_OH_Consignee = org2.PK;
			AssertEquals("resetCount", 1, resetCount);

			var oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));

			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("DECIMALFIELD", properties[0]);
			AssertEquals("resetCount", 1, resetCount);

			hawb.CS_RL_NKOrigin = "AUMEL";
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

			hawb.CS_RL_NKDestination = "NZCHC";
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

		public void TestGetTemplateSelectionCriteria_ForConsignee()
		{
			hawb.CS_OH_Consignee = hawb.Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertGetTemplateFilterCriteria(hawb.CS_OH_ConsigneeInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForOrigin()
		{
			hawb.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(hawb.CS_RL_NKOriginInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForDestination()
		{
			hawb.Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(hawb.CS_RL_NKDestinationInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CustomsHouseAirCargoCode;

		protected override CusHAWB GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObject(factory);
			result.CS_CM = factory.New<CusMAWB>().PK;
			return result;
		}

		CusHAWB hawb => BusinessObject;
	}
}
