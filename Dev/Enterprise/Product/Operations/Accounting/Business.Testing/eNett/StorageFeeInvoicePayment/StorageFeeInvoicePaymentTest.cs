using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.eNett_Integration.Testing
{
	[TestedType(typeof(StorageFeeInvoicePayment))]
	public sealed class StorageFeeInvoicePaymentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIApportionedChargesHeaderImplementation()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			Job job1 = testObjectCreator.CreateJob(testObjectCreator.AALSHI, 0M, testObjectCreator.Agent, 0M);
			Job job2 = testObjectCreator.CreateJob(testObjectCreator.AALSHI, 0M, testObjectCreator.Agent, 0M);
			Factory.Save();

			StorageFeeInvoicePayment testObject = (StorageFeeInvoicePayment)GetNewBusinessObject();
			APInvoiceLine line1 = (APInvoiceLine)testObject.Invoice.Lines.AddNew();
			APInvoiceLine line2 = (APInvoiceLine)testObject.Invoice.Lines.AddNew();
			line1.AL_JH = job1.PK;
			line2.AL_JH = job2.PK;

			testObject.ChargeCode = Factory.Load<AccChargeCode>(new ZQuery())[0].PK;
			testObject.ApportionmentMethod = AllocationMethod.Shipment;
			testObject.ChargeCode = testObjectCreator.CC4.PK;

			IApportionedChargesHeader testObjectAsHeader = testObject;
			AssertEquals("ApportionmentMethod", testObject.ApportionmentMethod, testObjectAsHeader.ApportionmentMethod);
			AssertEquals("ChargeCode", testObject.ChargeCodeBizObj, testObjectAsHeader.ChargeCode);
			AssertEquals("Currency", GlbCompany.CurrentCompany.LocalCurrency, testObjectAsHeader.Currency);
			AssertEquals("Charges.Length", 2, testObjectAsHeader.Charges.Length);
			AssertEquals("IsChargeReadyToPost Line1", line1.AL_LineAmount != 0M, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Line2", line2.AL_LineAmount != 0M, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[1]));

			line1.AL_LineAmount = 1M;
			line2.AL_LineAmount = 0M;
			AssertEquals("IsChargeReadyToPost Line1", true, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Line2", false, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[1]));

			line1.AL_LineAmount = 0M;
			line2.AL_LineAmount = 1M;
			AssertEquals("IsChargeReadyToPost Line1", false, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[0]));
			AssertEquals("IsChargeReadyToPost Line2", true, testObjectAsHeader.IsChargeReadyToPost(testObjectAsHeader.Charges[1]));
		}

		public void TestOverrideJobHeaderDefaulting()
		{
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 1;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 0;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Env.CurrentBranch.PK, Guid.Empty, rule);

			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var consol = testObjectCreator.CreateConsol("USCHI", "CNSHA", "10000");
			var shipment1 = testObjectCreator.CreateShipment("1");
			consol.Shipments.Add(shipment1);
			var shipment2 = testObjectCreator.CreateShipment("2");
			var job2 = testObjectCreator.CreateJob(shipment2);
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			factory.Save();

			var container = factory.NewWithValidTestData<CommonContainer>();
			container.JC_ContainerNum = "TEST_ContainerNumber";
			var pack = shipment1.OuterPackLines.AddNew();
			pack.Containers.Add(container);
			container.LinkedShipment = shipment1;

			var payment = new StorageFeeInvoicePayment(container);
			payment.Initialise();

			AssertNotNull("Line 1 should have a job that was created by system", payment.Invoice.Lines[0].Job);
			AssertEquals("line 1 should have job 1 that was created by system attached to shipment1", shipment1.PK, payment.Invoice.Lines[0].Job.JH_ParentID);

			var oldsystemgeneratedjob = payment.Invoice.Lines[0].Job;
			payment.Invoice.Lines[0].AL_JH = job2.PK; //User Overriding Job and deselecting the system generated job.
			payment.Invoice.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			oldsystemgeneratedjob.JH_GB = ZGuid.Empty; //Branch and Department set to null to create exception, but exception should not be thrown because job should be deleted.
			oldsystemgeneratedjob.JH_GE = ZGuid.Empty;

			AssertNoExceptionThrown(delegate
			{ factory.Save(); });
			AssertNoExceptionThrown(delegate
			{ payment.Factory.Save(); });

			Assert("System Generated Job should be deleted because it is not used", oldsystemgeneratedjob.IsDeleted);
		}

		public void TestJobNotBeDeleteWhenInitialisedJobWasAlreadyCreated()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var container = Factory.NewWithValidTestData<CommonContainer>();
			var shipment = testObjectCreator.CreateShipment("JS00001");
			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);

			var jobLoader = new Job.Loader(shipment);
			var job = jobLoader.TryCreateWithMutex();

			var job2 = testObjectCreator.JobHeader1.PK;
			var chargeCode = testObjectCreator.CreateChargeCode("XXX", "DESCRIPTION", "MRG", 100m, testObjectCreator.GST1, null);
			Factory.Save();

			var payment = new StorageFeeInvoicePayment(container);
			payment.Initialise();
			var line = payment.Invoice.Lines[0];
			line.AL_AC = chargeCode.PK;
			AssertEquals(job.PK, line.AL_JH);

			line.AL_JH = job2;
			AssertNoExceptionThrown(() =>
			{
				payment.Factory.Save();
			});
		}

		public void TestInitialiseWhenTheJobWasAlreadyCreatedInAnotherFactory()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var containerFactory = new BusinessObjectFactory();
			var container = containerFactory.NewWithValidTestData<CommonContainer>();
			containerFactory.Save();

			var shipment = testObjectCreator.CreateShipment("JS00001");
			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);
			Factory.Save();

			var jobLoader = new Job.Loader(shipment);
			using (var job = jobLoader.TryCreateWithMutex())
			{
				AssertNotNull(job);

				using (var payment = new StorageFeeInvoicePayment(container))
				{
					payment.Initialise();
					var paymentInvoiceLine = payment.Invoice.Lines[0];
					var expectErrorMsg = "You have created the job JS00001 on another form, but haven't saved it yet.\r\n" +
												"Please close or save other forms that use job JS00001 to continue.";
					AssertEquals(expectErrorMsg,paymentInvoiceLine.CreatingJobHeaderErrorMessage);
				}
			}
		}

		public void TestInitialiseInvoiceValidateExpectedInvoiceTotalFlag()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var containerFactory = new BusinessObjectFactory();
			var container = containerFactory.NewWithValidTestData<CommonContainer>();
			containerFactory.Save();

			var shipment = testObjectCreator.CreateShipment("JS00001");
			var pack = shipment.OuterPackLines.AddNew();
			pack.Containers.Add(container);
			Factory.Save();

			var jobLoader = new Job.Loader(shipment);
			using (var job = jobLoader.TryCreateWithMutex())
			using (var payment = new StorageFeeInvoicePayment(container))
			{
				payment.Initialise();
				Assert(!payment.Invoice.ValidateExpectedInvoiceTotal);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StorageFeeInvoicePayment(new MockContainerStorageDataProvider());
		}

		#endregion
	}
}
