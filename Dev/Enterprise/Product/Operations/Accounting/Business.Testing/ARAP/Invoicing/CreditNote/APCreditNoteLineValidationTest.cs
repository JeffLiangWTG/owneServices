using System;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APCreditNoteLine))]
	public class APCreditNoteLineValidationTest : InvoicingLineBaseValidationTest
	{
		protected override Type InvoiceLineType
		{
			get { return typeof(APCreditNoteLine); }
		}

		protected override Type InvoiceType
		{
			get { return typeof(APCreditNote); }
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestCheckAL_AG()
		{
			base.TestCheckAL_AG();

			APCreditNote invoice = Factory.New<APCreditNote>();
			invoice.AH_TransactionNum = "101";
			APCreditNoteLine line1 = (APCreditNoteLine)invoice.Lines.AddNew();
			Freight.Business.CommonShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			line1.AL_JH = job.PK;
			line1.AL_AC = TestCharge.PK;

			line1.Validation.ValidateAL_AG();

			Assert("Line AL_AG should not have error", !line1.AL_AGInfo.HasErrors());
			Assert("Line AL_AG should not have error", !line1.GenericChargeInfo.HasErrors());
		}

		[MasterFiles.Business.Testing.SuspendGLAccountAndChargeCodeCriticalValidation]
		public override void TestCheckAL_AC()
		{
			base.TestCheckAL_AC();

			APCreditNote invoice = Factory.New<APCreditNote>();
			invoice.AH_TransactionNum = "101";
			APCreditNoteLine line1 = (APCreditNoteLine)invoice.Lines.AddNew();
			Freight.Business.CommonShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment);
			Factory.Save();

			line1.AL_JH = job.PK;
			line1.AL_AC = TestCharge.PK;

			line1.Validation.ValidateAL_AC();

			Assert("Line AL_AC should not have error", !line1.AL_ACInfo.HasErrors());
			Assert("Line AL_AC should not have error", !line1.GenericChargeInfo.HasErrors());
		}

		protected AccChargeCode TestCharge
		{
			get
			{
				AccChargeCode result = Factory.NewWithValidTestData<AccChargeCode>();
				result.AC_AG_CostAccount = TestObjectCreator.GLHeader1.PK;
				return result;
			}
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(APCreditNote);
		}
	}
}
