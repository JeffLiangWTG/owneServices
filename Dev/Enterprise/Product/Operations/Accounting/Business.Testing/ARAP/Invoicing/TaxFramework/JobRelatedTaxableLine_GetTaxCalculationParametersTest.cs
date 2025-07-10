using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.TaxFramework
{
	abstract class JobRelatedTaxableLine_GetTaxCalculationParametersTest : InvoicingLineBaseTaxable_GetTaxCalculationParametersTest
	{
		public void TestGetTaxCalulcationParameter_TestCustomsStatusParameter()
		{
			var invoice = CreateInvoice(Creator.AALSHI);
			var (line, shipment) = CreateJobRelatedInvoice(invoice, attemptNumber: 1);

			var expectedCustomStatus = "XX";
			shipment.CustomsEntryNumberType = expectedCustomStatus;
			var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
			AssertEquals("Custom Status", expectedCustomStatus, taxCalcParams.CustomsStatus);
		}

		public override void TestGetTaxCalulcationParameters_TestPlaceOfSupplyParameterValue()
		{
			var allPOSEnabled = GetAllPOSEnabledFixedPlaceOfSupplyConfig();

			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allPOSEnabled))
			{
				var line = CreateInvoiceLine(attemptNumber: 1, Creator.AALSHI);
				var (job, jobInvoicingSupporterMock) = GetJobWithMockJobInvoicingPluginParent();
				line.AL_JH = job.PK;

				jobInvoicingSupporterMock.Setup(m => m.FixedPlaceOfSupply).Returns((ILocation)null);
				var taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertNull("Place of Supply is not set on job or line", taxCalcParams.FixedPlaceOfSupply);

				var ausyd = Factory.LoadFromUniqueKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, new ZString("AUSYD"));
				jobInvoicingSupporterMock.Setup(m => m.FixedPlaceOfSupply).Returns(ausyd);
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertEquals("Place of supply is set on job2", ausyd.Code, taxCalcParams.FixedPlaceOfSupply.Code);

				line.AL_PlaceOfSupply = "NSW";
				taxCalcParams = GetTaxCalculationParametersFromInvoiceLine(line);
				AssertEquals("Place of supply is set on line", "NSW", taxCalcParams.FixedPlaceOfSupply.Code);
			}
		}

		(InvoicingLineBase, ForwardingShipment) CreateJobRelatedInvoice(InvoicingBase invoice, int attemptNumber)
		{
			var line = Creator.CreateInvoiceLine(invoice, 100M);

			ForwardingShipment shipment = Creator.CreateShipment("S00" + attemptNumber, transportMode: TransportModes.Air, saveIt: false);
			Job job = Creator.CreateJob(shipment, false);
			line.AL_JH = job.PK;

			return (line, shipment);
		}

		protected override InvoicingLineBase CreateInvoiceLine(int attemptNumber, OrgHeader invoiceOrg)
		{
			var invoice = CreateInvoice(invoiceOrg);
			var (line, _) = CreateJobRelatedInvoice(invoice, attemptNumber);
			return line;
		}

		public (Job, Mock<IJobInvoicingSupporter>) GetJobWithMockJobInvoicingPluginParent()
		{
			var jobInvoicingSupporterMock = Creator.GetIJobInvoicingSupporterMock(JobInvoicingConsumerTypes.LocalCartage);
			var jobInvoicingPluginMock = Creator.GetTestShipmentPlugIn("S00001", jobInvoicingSupporterMock);
			var job = new Job.Loader(jobInvoicingPluginMock).TryCreate();

			return (job, jobInvoicingSupporterMock);
		}

		protected override ZString GetExpectedJobType()
		{
			return JobInvoicingConsumerTypes.Shipment.Code;
		}

		protected override ZString GetExpectedTransportMode()
		{
			return TransportModes.Air;
		}

		protected abstract InvoicingBase CreateInvoice(OrgHeader sendingOrg);
	}
}
