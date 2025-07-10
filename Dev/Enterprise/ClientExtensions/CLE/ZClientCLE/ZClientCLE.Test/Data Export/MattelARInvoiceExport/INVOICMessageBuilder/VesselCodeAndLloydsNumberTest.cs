using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class VesselCodeAndLloydsNumberTest : TestCaseWithFactory
	{
		public void TestVesselCodeAndLloydsNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration jobDec = SetupJobDec(true, "");
			InvoicingBase[] invoices = new InvoicingBase[1];
			JobInvoiceRecord record = new JobInvoiceRecord(shipment, jobDec, invoices);
			INVOICMessageBuilderForTest mesgBuilder = new INVOICMessageBuilderForTest(record, Factory);
			AssertEquals("VesselCodeAndLloydsNumber", "9999999*Vessel", mesgBuilder.VesselCodeAndLloydsNumber);
			jobDec = SetupJobDec(true, "ABC");
			record = new JobInvoiceRecord(shipment, jobDec, invoices);
			mesgBuilder = new INVOICMessageBuilderForTest(record, Factory);
			AssertEquals("VesselCodeAndLloydsNumber", "ABC*Vessel", mesgBuilder.VesselCodeAndLloydsNumber);
			jobDec = SetupJobDec(false, "xxx");
			record = new JobInvoiceRecord(shipment, jobDec, invoices);
			mesgBuilder = new INVOICMessageBuilderForTest(record, Factory);
			AssertEquals("VesselCodeAndLloydsNumber", "9999999*", mesgBuilder.VesselCodeAndLloydsNumber);
		}

		BaseJobDeclaration SetupJobDec(bool linkedToVessel, ZString lloydsNumber)
		{
			BaseJobDeclaration jobDec = Factory.New<BaseJobDeclaration>();
			if (linkedToVessel)
			{
				jobDec.JE_VesselName = Vessel.RV_Code;
			}

			Vessel.RV_LloydsNumber = lloydsNumber;
			Factory.Save();
			return jobDec;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Vessel = Factory.New<RefVessel>();
			Vessel.RV_Code = "Vessel";
			Vessel.RV_VesselType = Core.Constants.VesselType.CargoVessel;
			Factory.Save();
		}

		RefVessel Vessel;
		class INVOICMessageBuilderForTest : INVOICMessageBuilder
		{
			public INVOICMessageBuilderForTest(JobInvoiceRecord record, BusinessObjectFactory factory) : base(record, factory)
			{
			}

			protected override ZDecimal DutyAmount
			{
				get
				{
					return 5m;
				}
			}

			protected override string DutyRate(BaseJobComInvoiceLine line)
			{
				return "0.00";
			}

			public new string VesselCodeAndLloydsNumber
			{
				get
				{
					return base.VesselCodeAndLloydsNumber;
				}
			}
		}
	}
}
