using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareShipmentChargeProcessorTest : TestCaseWithFactory
	{
		public void TestObjectProviderDependencyTypes()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, false);
			var processor = new ProfitShareShipmentChargeProcessor(Factory, shipment, job);

			var profitShareCalculator = processor.ObjectProvider.GetCalculator(Factory, null, shipment);
			AssertType<ProfitShareCalculator>(profitShareCalculator);

			var profitChargeCreator = processor.ObjectProvider.GetChargeCreator(new ProfitShareDetailCollection(), job, false);
			AssertType<ProfitShareShipmentChargeCreator>(profitChargeCreator);
		}

		public void TestProfitShareShipmentChargeProcessorConstructor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			ProfitShareShipmentChargeProcessor porcessor = null;
			var shipment = testObjectCreator.CreateShipment("S001001");
			var job = testObjectCreator.CreateJob(shipment, false);
			try
			{
				porcessor = new ProfitShareShipmentChargeProcessor(null, shipment, job);
				Fail();
			}
			catch (ArgumentException)
			{ }

			try
			{
				porcessor = new ProfitShareShipmentChargeProcessor(Factory, null, job);
				Fail();
			}
			catch (ArgumentException)
			{ }

			try
			{
				porcessor = new ProfitShareShipmentChargeProcessor(Factory, shipment, null);
				Fail();
			}
			catch (ArgumentException)
			{ }

			porcessor = new ProfitShareShipmentChargeProcessor(Factory, shipment, job);
			AssertNotNull(porcessor);
		}

		public void TestProfitShareShipmentChargeProcessor_Process_SavedInDatabase()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			GlbCompany.CurrentCompany.Factory.Save();
			var notificationHandler = new NotificationHandlerForTest();
			NotificationHandler.Instance = notificationHandler;
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = false; //this should make the creation of profit share charges fail
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			Assert("Charge is saved in DB", Job.Charges[0].IsInDatabase);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Process returns false as error occurred", !result);
			AssertEquals("No profit share charge created as error occurred", 1, Job.Charges.Count);
			Assert("Error occured and profitShareShipmentChargeProcessor_OnErrorOccurred method was invoked", ErrorCaughtInProcessor);
			AssertNullOrEmpty(notificationHandler.ReportInformationMessage);

			ErrorCaughtInProcessor = false; //Reset flag

			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Process returns true as no errors occurred", result);
			AssertEquals("Profit share charge created this time", 2, Job.Charges.Count);
			Assert("Profit share charge is saved in DB", Job.Charges[1].IsInDatabase);
			Assert("No error encountered", !ErrorCaughtInProcessor);
			AssertNullOrEmpty(notificationHandler.ReportInformationMessage);
		}

		public void TestProfitShareShipmentChargeProcessor_Process_NotSavedInDatabase()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			GlbCompany.CurrentCompany.Factory.Save();
			var notificationHandler = new NotificationHandlerForTest();
			NotificationHandler.Instance = notificationHandler;
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = false; //this should make the creation of profit share charges fail
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			Assert("Charge is saved in DB", Job.Charges[0].IsInDatabase);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job, false);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Process returns false as error occurred", !result);
			AssertEquals("No profit share charge created as error occurred", 1, Job.Charges.Count);
			Assert("Error occured and profitShareShipmentChargeProcessor_OnErrorOccurred method was invoked", ErrorCaughtInProcessor);
			AssertNullOrEmpty(notificationHandler.ReportInformationMessage);

			ErrorCaughtInProcessor = false; //Reset flag

			TestObjectCreator.Creditor1.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job, false);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Process returns true as no errors occurred", result);
			AssertEquals("Profit share charge created this time", 2, Job.Charges.Count);
			Assert("Profit share charge is not saved in DB", !Job.Charges[1].IsInDatabase);
			Assert("No error encountered", !ErrorCaughtInProcessor);
			AssertNullOrEmpty(notificationHandler.ReportInformationMessage);
		}

		public void TestProfitShareShipmentChargeProcessor_Process_ZSaveConcurrencyException()
		{
			var notificationHandler = new NotificationHandlerForTest();
			NotificationHandler.Instance = notificationHandler;
			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CreateProfitShareDetails("");
			TestObjectCreator.Agent.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			AssertEquals("Precondition: Only one charge before profit share", 1, Job.Charges.Count);
			AssertEquals("International Freight", Job.Charges[0].JR_Desc);
			AssertEquals(100M, Job.Charges[0].JR_LocalCostAmt);
			AssertEquals("WRK", Job.JH_Status);
			AssertEquals(false, Job.HasChanges);

			TestConnection.ExecuteNonQuery(string.Format(@"
UPDATE dbo.JOBHEADER
SET
	JH_STATUS = 'WHL',
	JH_SystemLastEditTimeUtc = GETUTCDATE(),
	JH_SystemLastEditUser = '~BP'
WHERE
	JH_PK = '{0}';", Job.PK));

			ErrorCaughtInProcessor = false;
			ErrorArgsOccurred = null;
			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;
			Assert("Process returns false as error occurred", !result);
			Assert("Error is caught before reaches processor", !ErrorCaughtInProcessor);
			Assert("Job and children have no errors, so saving is attempted", !Job.HasErrors);
			AssertEquals("Profit share charge is not saved as there is a handled concurrency conflict", 1, Job.Charges.Count(x => !x.IsInDatabase));
			AssertContains("While you have been working with this form, another user has made changes", notificationHandler.ReportInformationMessage);
		}

		public void TestProcess_RaisesOnErrorEvent_WhenValidationErrorCreatingCharges()
		{
			CreateProfitShareDetails("");
			ErrorCaughtInProcessor = false;
			ErrorArgsOccurred = null;

			var errorMessage = new ZStringBuilder();
			var mockChargeCreator = new Mock<IProfitShareChargeCreator>();
			mockChargeCreator.Setup(x => x.CreateCharges()).Returns(() =>
			{
				errorMessage.Append("Some validation error message triggered during creation of charges and posting transactions");
				return true;        // Charges were created, and a validation error occurred.
			});
			mockChargeCreator.Setup(x => x.ValidationErrors).Returns(() => errorMessage.ToString());
			var mockObjectProvider = new Mock<ProfitShareShipmentChargeProcessor.IObjectProvider>();
			mockObjectProvider.Setup(x => x.GetCalculator(It.IsAny<BusinessObjectFactory>(), It.IsAny<Integration.IJobCostingPlugIn>(), It.IsAny<IJobInvoicingPlugIn[]>())).Returns((BusinessObjectFactory factory, Integration.IJobCostingPlugIn consol, IJobInvoicingPlugIn[] shipments) => new ProfitShareCalculator(factory, null, shipments));
			mockObjectProvider.Setup(x => x.GetChargeCreator(It.IsAny<ProfitShareDetailCollection>(), It.IsAny<Job>(), It.IsAny<bool>())).Returns(mockChargeCreator.Object);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job, objectProvider: mockObjectProvider.Object);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Error event is raised", ErrorCaughtInProcessor);
			AssertNotNull("Error event is raised", ErrorArgsOccurred);
			AssertEquals("Error event message is the validation error", "Some validation error message triggered during creation of charges and posting transactions", ErrorArgsOccurred.Message);
			AssertEquals("Process() returns false as error occurred, even if charges were created", false, result);
			mockChargeCreator.Verify(x => x.CreateCharges(), Times.Once(), "CreateCharges() was called, even if nothing was created");
		}

		public void TestProcess_DoesNotCreateCharges_WhenPreCreateValidationError()
		{
			CreateProfitShareDetails("");
			ErrorCaughtInProcessor = false;
			ErrorArgsOccurred = null;

			var errorMessage = new ZStringBuilder();
			var mockChargeCreator = new Mock<IProfitShareChargeCreator>();
			mockChargeCreator.Setup(x => x.RunPreCreateValidation()).Callback(() =>
			{
				errorMessage.Append("Some validation error message triggered before creating charges");
			});
			mockChargeCreator.Setup(x => x.ValidationErrors).Returns(() => errorMessage.ToString());
			var mockObjectProvider = new Mock<ProfitShareShipmentChargeProcessor.IObjectProvider>();
			mockObjectProvider.Setup(x => x.GetCalculator(It.IsAny<BusinessObjectFactory>(), It.IsAny<Integration.IJobCostingPlugIn>(), It.IsAny<IJobInvoicingPlugIn[]>())).Returns((BusinessObjectFactory factory, Integration.IJobCostingPlugIn consol, IJobInvoicingPlugIn[] shipments) => new ProfitShareCalculator(factory, null, shipments));
			mockObjectProvider.Setup(x => x.GetChargeCreator(It.IsAny<ProfitShareDetailCollection>(), It.IsAny<Job>(), It.IsAny<bool>())).Returns(mockChargeCreator.Object);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job, objectProvider: mockObjectProvider.Object);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Error event is raised", ErrorCaughtInProcessor);
			AssertNotNull("Error event is raised", ErrorArgsOccurred);
			AssertEquals("Error event message is the validation error", "Some validation error message triggered before creating charges", ErrorArgsOccurred.Message);
			AssertEquals("Process() returns false as error occurred", false, result);
			mockChargeCreator.Verify(x => x.CreateCharges(), Times.Never(), "CreateCharges() was not called, because PreCreateValidation failed");
		}

		public void TestProcess_DoesNotRaisesOnErrorEvent_WhenNoValidationError()
		{
			CreateProfitShareDetails("");
			ErrorCaughtInProcessor = false;
			ErrorArgsOccurred = null;

			var mockChargeCreator = new Mock<IProfitShareChargeCreator>();
			mockChargeCreator.Setup(x => x.CreateCharges()).Returns(false);
			mockChargeCreator.Setup(x => x.ValidationErrors).Returns(ZString.Empty);
			var mockObjectProvider = new Mock<ProfitShareShipmentChargeProcessor.IObjectProvider>();
			mockObjectProvider.Setup(x => x.GetCalculator(It.IsAny<BusinessObjectFactory>(), It.IsAny<Integration.IJobCostingPlugIn>(), It.IsAny<IJobInvoicingPlugIn[]>())).Returns((BusinessObjectFactory factory, Integration.IJobCostingPlugIn consol, IJobInvoicingPlugIn[] shipments) => new ProfitShareCalculator(factory, null, shipments));
			mockObjectProvider.Setup(x => x.GetChargeCreator(It.IsAny<ProfitShareDetailCollection>(), It.IsAny<Job>(), It.IsAny<bool>())).Returns(mockChargeCreator.Object);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, Shipment, Job, objectProvider: mockObjectProvider.Object);
			profitShareShipmentChargeProcessor.OnErrorOccurred += profitShareShipmentChargeProcessor_OnErrorOccurred;
			var result = profitShareShipmentChargeProcessor.Process();
			profitShareShipmentChargeProcessor.OnErrorOccurred -= profitShareShipmentChargeProcessor_OnErrorOccurred;

			Assert("Error event is not raised", !ErrorCaughtInProcessor);
			AssertNull("Error event is not raised", ErrorArgsOccurred);
			AssertEquals("Process() returns false as nothing was created", false, result);
			mockChargeCreator.Verify(x => x.CreateCharges(), Times.Once(), "CreateCharges() was called, even if nothing was created");
		}

		bool ErrorCaughtInProcessor;
		ProfitShareChargeCreationEventArgs ErrorArgsOccurred;
		void profitShareShipmentChargeProcessor_OnErrorOccurred(object sender, ProfitShareChargeCreationEventArgs e)
		{
			ErrorCaughtInProcessor = true;
			ErrorArgsOccurred = e;
		}

		ForwardingShipment CreateProfitShareDetails(string rateBasis, string shipmentNumber = "", bool createNewConsol = true)
		{
			if (createNewConsol)
			{
				Consol = Factory.New<ForwardingConsol>();
				Consol.SetDefaultSendingForwarderAddress(TestObjectCreator.Agent);
				Consol.SetDefaultReceivingForwarderAddress(TestObjectCreator.Creditor1);
			}

			Shipment = Consol.Shipments.AddNew();
			Shipment.JS_UniqueConsignRef = string.IsNullOrEmpty(shipmentNumber) ? "S001001" : shipmentNumber;

			ProfitShareDetails = new ProfitShareDetailCollection();

			CreateProfitShareProfile(TestObjectCreator.Agent, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent, 30m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent, 18m, rateBasis, ProfitShareDetails, Shipment);
			CreateProfitShareProfile(TestObjectCreator.Creditor1, OrgProfitSharePartyLookups.PartyTypeCodes.ControllingAgent, 4m, rateBasis, ProfitShareDetails, Shipment);

			Job = TestObjectCreator.CreateJob(Shipment, false);
			Job.JH_GE = TestObjectCreator.FIADepartment.PK;
			Job.JH_OA_AgentCollectAddr = TestObjectCreator.Agent.MainAddress.PK;
			Job.PlugInData = Shipment;

			JobCharge = Job.Charges.AddNew();
			JobCharge.JR_AC = Env.Registry.FreightChargeCode;
			JobCharge.JR_IsIncludedInProfitShare = true;
			JobCharge.JR_LocalSellAmt = 600m;
			JobCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			JobCharge.JR_LocalCostAmt = 100m;

			Factory.Save();

			return Shipment;
		}

		void CreateProfitShareProfile(OrgHeader agent, string role, decimal percent, string rateBasis, ProfitShareDetailCollection profitShares, ForwardingShipment shipment)
		{
			OrgAgentRelationship agentProfile = Factory.LoadTop1<OrgAgentRelationship>(new ZQuery(OrgAgentRelationshipSchema.O3_OH_SendingAgent, agent.PK));
			if (agentProfile == null)
			{
				agentProfile = Factory.New<OrgAgentRelationship>();
				agentProfile.O3_ProfitShareType = OrgAgentRelationship.ProfitShareTypes.Standard;
				agentProfile.O3_OH_SendingAgent = agent.PK;
			}

			OrgProfitShareDetails profitShareAgreement = agentProfile.GenericProfitShareDetails.Count == 1 ? agentProfile.GenericProfitShareDetails[0] : agentProfile.GenericProfitShareDetails.AddNew();
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";

			OrgProfitShareParty party = profitShareAgreement.PartyDetails.AddNew();
			party.PS_PartyType = role;
			party.PS_PartyProfitSharePercent = percent;
			party.PS_PartyRateBasis = rateBasis;

			ProfitShareDetail detail = profitShares.AddNew();
			ProfitShareShipmentDetail shipmentDetail = new ProfitShareShipmentDetail(shipment, agent, role, Factory);
			detail.ProfitShareShipmentDetails.Add(shipmentDetail);
			shipmentDetail.ProfitShareAgreement = profitShareAgreement;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator fTestObjectCreator;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;
		JobCharge JobCharge;
		Job Job;
		ProfitShareDetailCollection ProfitShareDetails;
	}
}
