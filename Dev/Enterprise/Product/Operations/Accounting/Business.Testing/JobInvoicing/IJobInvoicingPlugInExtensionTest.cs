using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class IJobInvoicingPlugInExtensionTest : TestCaseWithFactory
	{
		public void TestGetAllLinkedConsols()
		{
			var setup = TestObjectCreator.CreateGatewayConsolsAndShipments();
			Factory.Save();

			//			C0001		gC0002		C0003		C0004
			//	AUBNE	-	AUSYD	-	SGSIN	-	HKHKG	-	USLAX
			//												 \
			//													C0005
			//														\
			//															USNYC
			//				|-	-	-	-	-	S0001	-	-	-|
			//	|-	-	-	-	-	-	-	S0002	-	-	-	-	-	-|
			//	|-	-	-	S0003	-	-|

			var result = setup.s0003.GetAllLinkedConsols();
			Assert("gC0001 should be linked to S0003", result.Contains(setup.gC0001));
			Assert("gC0002 should be linked to S0003", result.Contains(setup.gC0002));
			Assert("C0003 should not be linked to S0003", !result.Contains(setup.c0003));
			Assert("C0004 should not be linked to S0003", !result.Contains(setup.c0004));
			Assert("C0005 should not be linked to S0003", !result.Contains(setup.c0005));
		}

		public void TestGetAllLinkedConsols_NullReference()
		{
			IJobInvoicingPlugIn relatedshipment = null;
			var result = relatedshipment.GetAllLinkedConsols();
			AssertNotNull(result);
			AssertEquals("result.Count should be 0", 0, result.Count);
		}

		public void TestGetDefaultJobDescription()
		{
			var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
			var item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "GCN";
			item.Mode = "AIR";
			item.DirectionCode = "IMP";
			item.InvoiceDescription = "Air Import Consol: <ConsolNumber>";

			item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "GCN";
			item.Mode = "AIR";
			item.DirectionCode = "EXP";
			item.InvoiceDescription = "Air Export Consol: <ConsolNumber>";

			item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "GCN";
			item.Mode = "AIR";
			item.DirectionCode = "DOM";
			item.InvoiceDescription = "Air Domestic Consol: <ConsolNumber>";

			AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

			var consol1 = TestObjectCreator.CreateGatewayConsol("USCHI", "AUSYD", "C001001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob1 = new Job.Loader(consol1).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(consolJob1);
			AssertEquals("JobDescription", "Air Import Consol: C001001", consol1.GetDefaultJobDescription());

			var consol2 = TestObjectCreator.CreateGatewayConsol("AUSYD", "USCHI", "C001002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob2 = new Job.Loader(consol2).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(consolJob2);
			AssertEquals("JobDescription", "Air Export Consol: C001002", consol2.GetDefaultJobDescription());

			var consol3 = TestObjectCreator.CreateGatewayConsol("AUSYD", "AUMEL", "C0123456789012345678", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob3 = new Job.Loader(consol3).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(consolJob3);
			AssertEquals("JobDescription", "Air Domestic Consol: C0123456789012345678", consol3.GetDefaultJobDescription());

			item.InvoiceDescription = "Consol:<ConsolNumber>-<ConsolNumber>-<ConsolNumber>-<ConsolNumber>-<ConsolNumber>-<ConsolNumber>"; // Should exceed JH_Description max length (100) when parsed
			AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

			Assert("JobDescription", consol3.GetDefaultJobDescription().StartsWith("Consol:C0123456789012345678-"));
			AssertEquals("JobDescription Length", consolJob2.JH_DescriptionInfo.MaxLength, consol3.GetDefaultJobDescription().Length);
		}

		public void TestGetDefaultJobDescriptionForAllJobs()
		{
			var jobInvoiceDescriptionConfig = AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.Value;
			var item = jobInvoiceDescriptionConfig.AddNew();
			item.JobType = "ALL";
			Assert(item.Mode.IsEmpty);
			Assert(item.DirectionCode.IsEmpty);
			item.InvoiceDescription = "My Job: <JobNumber>";

			AccountingConfigurationRegistry.Instance.JobInvoiceDescriptionConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, jobInvoiceDescriptionConfig);

			var shipment = TestObjectCreator.CreateShipment("S00010001");
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(shipmentJob);
			AssertEquals("JobDescription", "My Job: S00010001", shipment.GetDefaultJobDescription());

			var consol = TestObjectCreator.CreateGatewayConsol("USCHI", "AUSYD", "C001001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();
			AssertNotNull(consolJob);
			AssertEquals("JobDescription", "My Job: C001001", consol.GetDefaultJobDescription());
		}

		#region Implementation

		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		#endregion
	}
}
