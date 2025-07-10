using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CTOCusHAWB))]
	sealed class CTOCusHAWBTest : CusHAWBBaseAbstractTest
	{
		public void TestIDataExportCSVFileNameProviderMembers()
		{
			var hAWB = GetHAWBToTest();
			var fileNameProvider = hAWB as IDataExportCSVFileNameProvider;
			hAWB.CS_HAWB = "123456";
			AssertEquals("File name suffix", "123456", fileNameProvider.FileNameSuffix);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var hAWB = (CTOCusHAWB)GetHAWBToTest();
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)hAWB).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)hAWB).DefaultTranshipmentPort);
		}

		public void TestInvoicingJobNumber()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			hAWB.CS_MessageReference = "Blah";

			IJobInvoicingPlugIn invoicing = hAWB;
			AssertEquals("JobNumber", "NBlah", invoicing.JobNumber);
		}

		public void TestInvoicingUnitsAndMeasures()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			hAWB.CS_ChargableWeight = 11m;
			hAWB.CS_Weight = 10m;
			hAWB.CS_WeightUQ = Core.Constants.Weight.Kilograms;

			IJobInvoicingPlugIn invoicing = hAWB;

			AssertEquals("ActualChargeable", 11m, invoicing.InvoicingSupporter.ActualChargeable);
			AssertEquals("ActualChargeableUnit", Core.Constants.Weight.Kilograms, invoicing.InvoicingSupporter.ActualChargeableUnit);
			AssertEquals("ActualWeight", 10m, invoicing.InvoicingSupporter.ActualWeight, invoicing.InvoicingSupporter.ActualWeight);
			AssertEquals("ActualWeightUnit", Core.Constants.Weight.Kilograms, invoicing.InvoicingSupporter.ActualWeightUnit);
			AssertEquals("ActualVolume", 0m, invoicing.InvoicingSupporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", "", invoicing.InvoicingSupporter.ActualVolumeUnit);
		}

		public void TestInvoicingConsumerType()
		{
			IJobInvoicingPlugIn invoicing = (IJobInvoicingPlugIn)GetHAWBToTest();
			AssertEquals("ConsumerType", JobInvoicingConsumerTypes.CTOCusImportHAWB.Code, invoicing.InvoicingSupporter.ConsumerType.Code);
		}

		public void TestInvoicingPortsAndDates()
		{
			ZDateTime now = ZDateTime.Now;

			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			hAWB.MAWB.CM_ArrivalDate = now;
			hAWB.CS_RL_NKOrigin = "SGSIN";
			hAWB.CS_RL_NKDestination = "AUBNE";

			IJobInvoicingPlugIn invoicing = hAWB;

			AssertEquals("Origin", "SGSIN", invoicing.InvoicingSupporter.Origin.RL_Code);
			AssertEquals("Destination", "AUBNE", invoicing.InvoicingSupporter.Destination.RL_Code);
			AssertEquals("ETD", ZDateTime.Empty, invoicing.InvoicingSupporter.ETD);
			AssertEquals("ETA", now, invoicing.InvoicingSupporter.ETA);
			AssertEquals("IsImport", true, invoicing.InvoicingSupporter.IsImport);

			hAWB.CS_RL_NKOrigin = "AUBNE";
			hAWB.CS_RL_NKDestination = "SGSIN";

			AssertEquals("IsImport", false, invoicing.InvoicingSupporter.IsImport);
		}

		public void TestInvoicingModes()
		{
			IJobInvoicingPlugIn invoicing = (IJobInvoicingPlugIn)GetHAWBToTest();
			AssertEquals("TransportMode", Core.Constants.TransportModes.Air, invoicing.InvoicingSupporter.TransportMode);
			AssertEquals("ContainerMode", "", invoicing.InvoicingSupporter.ContainerMode);
		}

		public void TestInvoicingBillNumbers()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			hAWB.MAWB.CM_MAWB = "MAWB";
			hAWB.CS_HAWB = "HAWB";

			IJobInvoicingPlugIn invoicing = hAWB;
			AssertEquals("MasterBillNumber", "MAWB", invoicing.InvoicingSupporter.MasterBillNumber);
			AssertEquals("HouseBillNumber", "HAWB", invoicing.InvoicingSupporter.HouseBillNumber);
		}

		public void TestInvoicingSecurity()
		{
			IJobInvoicingPlugIn invoicing = (IJobInvoicingPlugIn)GetHAWBToTest();
			AssertEquals("AuditSecurity", Env.Security.AUCustomsAirCTOImportAuditBilling.LookupKey, invoicing.InvoicingSupporter.AuditSecurity.LookupKey);
			AssertEquals("JobInvoicingSecurity", Env.Security.AUCustomsAirCTOImportJobInvoicing.LookupKey, invoicing.InvoicingSupporter.JobInvoicingSecurity.LookupKey);
		}

		public void TestInvoicingEditSecurity()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CTOCusHAWB>();
			AssertEquals("EditSecurityCheckPoint should be Empty", Env.Security.None, testJob.InvoicingSupporter.EditSecurityCheckpoint);
			AssertEquals("EditSecuritytMessage should be Empty", ZString.Empty, testJob.InvoicingSupporter.EditSecurityMessage);
			AssertEquals("EditSecurityLock should be False", ZBool.False, testJob.InvoicingSupporter.EditSecurityLock);
		}

		public void TestInvoicingDefaultChargeGroup()
		{
			IJobInvoicingPlugIn testJob = Factory.New<CTOCusHAWB>();
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, testJob.InvoicingSupporter.DefaultChargeGroup);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent ctoCusHAWB = Factory.New<CTOCusHAWB>();
			Assert(ctoCusHAWB.AllowInvoiceDeletion);
		}

		public void TestSetMessageReferenceNumber()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			AssertNotNull("HAWB is not null", hAWB);
			Factory.Save();
			AssertEquals("Saved", true, hAWB.IsInDatabase);
			var messageReference = hAWB.CS_MessageReference;
			AssertEquals("Message reference number is set", false, messageReference.IsEmpty);
			AssertEquals("Message reference number starts with 'M'", true, messageReference.StartsWith("M"));

			hAWB.CS_MessageReference = "";
			AssertEquals("HasChanges", true, hAWB.HasChanges);
			Factory.Save();
			AssertNotEquals("Allocates a new Message Reference", messageReference, hAWB.CS_MessageReference);
			messageReference = hAWB.CS_MessageReference;
			AssertEquals("Message reference number is set", false, messageReference.IsEmpty);
			AssertEquals("Message reference number starts with 'M'", true, messageReference.StartsWith("M"));
		}

		public void TestUnderbondHumanReadableName()
		{
			CTOCusHAWB hAWB = (CTOCusHAWB)GetHAWBToTest();
			AssertEquals("UnderbondHumanReadableName is MasterBill", "MasterBill", ((ICusUnderbondDependentCollectionParent)hAWB).UnderbondHumanReadableName);
			hAWB.CS_HAWB = "123";
			AssertEquals("UnderbondHumanReadableName is MasterBill", "MasterBill 123", ((ICusUnderbondDependentCollectionParent)hAWB).UnderbondHumanReadableName);
		}

		public void TestSetDefaultValuesFromCTOCusHAWB()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			mAWB.CM_FlightNo = "QF123";
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_PiecesManifested = 125;
			CusUnderbond testUnderbond = hAWB.Underbonds.AddNew();
			testUnderbond.C4_ParentID = mAWB.PK;
			testUnderbond.C4_ParentTableCode = "CM";
			testUnderbond.SetDefaultValuesFromParent();
			AssertEquals("QF123", testUnderbond.C4_FlightNo);
			AssertEquals(125u, testUnderbond.C4_PiecesManifested);
		}

		public void TestCTOUnderbonds()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB houseBill = masterBill.ChildBills.AddNew();
			AssertNotNull(houseBill.AllUnderbonds);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CTOCusHAWBValidation), GetHAWBToTest().Validation.GetType());
		}

		public void TestLoadFromSendersReference()
		{
			var masterBill = Factory.New<CTOCusMAWB>();
			CTOCusHAWB houseBill = masterBill.ChildBills.AddNew();
			houseBill.CS_MessageReference = "1234";
			AssertNotNull(CTOCusHAWB.LoadFromSendersReference(Factory, "1234"));
		}

		public void TestLoadCusHAWB()
		{
			TestHelperCTOHAWBInformation info = new TestHelperCTOHAWBInformation();
			info.ArrivalDate = ZDateTime.Now;
			info.MAWB = "1";
			info.FlightNumber = "QF123";
			AssertNull(CTOCusHAWB.Load(Factory, info));
			CreateAndSaveHAWB(info);
			AssertNotNull(CTOCusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBWithNullDate()
		{
			TestHelperCTOHAWBInformation info = new TestHelperCTOHAWBInformation();
			info.ArrivalDate = ZDateTime.Empty;
			AssertNull(CTOCusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBWithInvalidDate()
		{
			TestHelperCTOHAWBInformation info = new TestHelperCTOHAWBInformation();
			info.ArrivalDate = ZDateTime.Invalid;
			AssertNull(CTOCusHAWB.Load(Factory, info));
		}

		public void TestLoadCusHAWBDeveloperError()
		{
			TestHelperCTOHAWBInformation info = new TestHelperCTOHAWBInformation();
			info.ArrivalDate = ZDateTime.Now;
			info.MAWB = "1";
			info.FlightNumber = "QF123";
			CreateAndSaveHAWB(info);
			AssertNotNull(CTOCusHAWB.Load(Factory, info));
			CreateAndSaveHAWB(info);
			AssertNotNull(CTOCusHAWB.Load(Factory, info));
			AssertNotNull("ErrorReported", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestWeStartInTheRightStatus()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			AssertEquals("CS_CustomsMainStatus", CMRBaseStatuses.Codes.NotSent, hAWB.CS_CustomsStatus);
		}

		public override void TestCS_PaymentTypeCaption()
		{
			var mAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB hAWB = mAWB.ChildBills.AddNew();
			AssertEquals("Method of Payment:", hAWB.CS_PaymentTypeCaption);
		}

		public void TestCTOCusHAWBImplementsIEDIMessageParent()
		{
			var hAWB = Factory.New<CTOCusHAWB>();
			hAWB.CS_HAWB = "08143515511";
			AssertEquals("MessageHistoryHeading", hAWB.UnderbondHumanReadableName, ((IDetailsTabPageHeadingProvider)hAWB).Heading);
			AssertEquals("MessageHistoryHeading", "MasterBill 08143515511", ((IDetailsTabPageHeadingProvider)hAWB).Heading);
		}

		protected override CusHAWBBase GetHAWBToTest(BusinessObjectFactory factory) => factory.New<CTOCusMAWB>().ChildBills.AddNew();

		protected override Type GetExpectedParentObjectType() => typeof(CTOCusMAWB);

		protected override BusinessObject GetNewBusinessObject() => GetHAWBToTest();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetHAWBToTest();

		void CreateAndSaveHAWB(ICTOCusHAWBInformationProvider information)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var mawb = factory.New<CTOCusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = information.MAWB;
			mawb.CM_FlightNo = information.FlightNumber;
			mawb.CM_ArrivalDate = information.ArrivalDate;
			factory.Save();
		}

		internal sealed class TestHelperCTOHAWBInformation : ICTOCusHAWBInformationProvider
		{
			ZString mawb;
			public ZString MAWB
			{
				get => mawb;
				set => mawb = value;
			}

			ZDateTime arrivalDate;
			public ZDateTime ArrivalDate
			{
				get => arrivalDate;
				set => arrivalDate = value;
			}

			ZString flightNumber;
			public ZString FlightNumber
			{
				get => flightNumber;
				set => flightNumber = value;
			}
		}
	}
}
