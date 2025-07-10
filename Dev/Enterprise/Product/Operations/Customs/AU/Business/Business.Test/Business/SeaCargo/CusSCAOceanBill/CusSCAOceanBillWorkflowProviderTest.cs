using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBill))]
	sealed class CusSCAOceanBillWorkflowProviderTest : WorkflowProviderTest<CusSCAOceanBill, ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>>
	{
		class CusSCAOceanBillForTesting : CusSCAOceanBill
		{
			public CusSCAOceanBillForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void ResetCustomBusinessObjectForTesting() => ResetCustomBusinessObject();
		}

		public void TestCustomBusinessObject()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode;
			template1.P0_Name = "TEST 1";
			template1.P0_Description = "TEST 1 DESC";
			var customField1 = template1.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "stringField";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;

			var customField2 = template1.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "intField";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			Factory.Save();

			var oceanBill = Factory.New<CusSCAOceanBillForTesting>();
			var resetCount = 0;
			oceanBill.OnResetCustomBusinessObject = () => resetCount++;
			ICustomFieldProvider customFieldProvider = oceanBill;
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
			oceanBill.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 1, resetCount);
			var oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			customField2.Delete();
			AssertEquals("resetCount", 1, resetCount);
			AssertSame(customBusinessObject, customFieldProvider.GetCustomBusinessObject());
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 2, properties.Length);
			AssertContains("INTFIELD", properties[0]);
			AssertContains("STRINGFIELD", properties[1]);
			oceanBill.ResetCustomBusinessObjectForTesting();
			AssertEquals("resetCount", 2, resetCount);
			oldCustomBusinessObject = customBusinessObject;
			customBusinessObject = customFieldProvider.GetCustomBusinessObject();
			Assert(!object.ReferenceEquals(oldCustomBusinessObject, customBusinessObject));
			properties = customBusinessObject.CustomProperties.Select(x => x.Identifier).OrderBy(x => x).ToArray();
			AssertEquals("properties", 1, properties.Length);
			AssertContains("STRINGFIELD", properties[0]);
			if (ErrorReporter.LastKeyReported == "CUS-OceanBill-WorkflowProviderTypeUnknownCountry")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestGetTemplateSelectionCriteria_ForDestination()
		{
			Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(OceanBill.CB_RL_NKPortOfDischargeInfo, ProcessTaskTemplate.P0_DischargePortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForOrigin()
		{
			Factory.Save();
			AssertGetTemplateFilterCriteria<ZString>(OceanBill.CB_RL_NKPortOfLoadingInfo, ProcessTaskTemplate.P0_LoadPortCountryInfo, "AUSYD", "MYPKG", ZString.Empty);
		}

		public void TestGetTemplateSelectionCriteria_ForShippingLine()
		{
			OceanBill.CB_OH_ShippingLine = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();
			AssertGetTemplateFilterCriteria(OceanBill.CB_OH_ShippingLineInfo, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		[TestDate(2012, 8, 2, 06, 15, 00)] // Thursday
		public void TestDeferredScheduledMessages()
		{
			RefUNLOCO cbrLoco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR");
			DateTime GetLocationDateTime() => cbrLoco.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()); // portCBR.LocationDateTime is cached so it will return the same time over and over
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 16, GetLocationDateTime().Hour);
			AssertEquals("Pre-condition - lateTimeframe", 48, AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.Value);

			OceanBill.CB_RL_NKPortOfDischarge = "AUCBR";

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 4, 18, 30, 0);
			AssertEquals("Immediate when Scheduled time is within the 48 hour late window", ZDateTime.Empty, OceanBill.DeferredScheduledMessagesDateTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 4, 19, 30, 0);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 48 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 2, 19, 0, 0), OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 09, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 19, GetLocationDateTime().Hour);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (7PM-6AM)", new ZDateTime(2012, 8, 2, 19, 15, 0), OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 19, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 5, GetLocationDateTime().Hour);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 5, 19, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (7PM-6AM)", new ZDateTime(2012, 8, 3, 05, 15, 0), OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 2, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 6, GetLocationDateTime().Hour);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 3, 19, 0, 0), OceanBill.DeferredScheduledMessagesDateTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 5, 05, 30, 0);
			AssertEquals("Immediate when Scheduled Time is within the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", ZDateTime.Empty, OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 3, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Saturday, GetLocationDateTime().DayOfWeek);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 6, 05, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, OceanBill.DeferredScheduledMessagesDateTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 6, 19, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (SAT/SUN)", new ZDateTime(2012, 8, 4, 06, 15, 0), OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 4, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Sunday, GetLocationDateTime().DayOfWeek);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 7, 05, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, OceanBill.DeferredScheduledMessagesDateTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 7, 19, 30, 0);
			AssertEquals("Scheduled for Now when Scheduled Time is outside the 4 hour Late Window and CBR Time is in the Dark Window (SAT/SUN)", new ZDateTime(2012, 8, 5, 06, 15, 0), OceanBill.DeferredScheduledMessagesDateTime);

			TestDateAttribute.Date = new DateTime(2012, 8, 5, 20, 15, 00);
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", DayOfWeek.Monday, GetLocationDateTime().DayOfWeek);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 8, 05, 30, 0);
			AssertEquals("Immediate when within the 4 hour late window", ZDateTime.Empty, OceanBill.DeferredScheduledMessagesDateTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2012, 8, 8, 19, 30, 0);
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM MON-FRI)", new ZDateTime(2012, 8, 6, 19, 0, 0), OceanBill.DeferredScheduledMessagesDateTime);
		}

		[TestDate(2013, 7, 9, 10, 15, 0)] // this is UTC, in Canberra its Tuesday (2013, 7, 9, 20, 15, 0)
		public void TestDeferredScheduledMessageLogChangesOnSaving()
		{
			var logs = new LogsForNominatedEvent(OceanBill.GetLogs(), Events.DeferredScheduledMessage);
			logs.AddNew();
			OceanBill.CB_RL_NKPortOfDischarge = "AUCBR";
			Factory.Save();
			AssertEquals("Precondition", new ZDateTime(2013, 7, 9, 10, 15, 0), logs[0].SL_EventTime);

			OceanBill.CB_DateOfArrival = new ZDateTime(2013, 7, 12, 6, 0, 0);
			Factory.Save();
			AssertEquals("Canberra event is Canberra Time", new ZDateTime(2013, 7, 9, 20, 15, 0), logs[0].SL_EventTime);

			OceanBill.CB_RL_NKPortOfDischarge = "AUPER";
			Factory.Save();
			AssertEquals("Perth event is Canberra Time", new ZDateTime(2013, 7, 9, 20, 15, 0), logs[0].SL_EventTime);
		}

		#region Implementation

		CusSCAOceanBill OceanBill
		{
			get { return BusinessObject; }
		}

		protected override ZString ExpectedWorkflowType
		{
			get { return CusSCAOceanBillWorkflowDescriptor.Constants.Code; }
		}

		#endregion // Implementation
	}
}
