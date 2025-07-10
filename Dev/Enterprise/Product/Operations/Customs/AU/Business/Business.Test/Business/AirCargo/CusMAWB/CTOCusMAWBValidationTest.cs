using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CTOCusMAWBValidationTest : CusMAWBValidationAbstractTest
	{
		[TestDate(2011, 1, 1)]
		public void TestInvoiceAlreadyPostedValidation()
		{
			MAWB.CM_FlightNo = "QF123";
			MAWB.CM_RL_NKFirstArrivalPort = "INBOM";
			MAWB.CM_ArrivalDate = new ZDateTime(2006, 10, 23);

			AssertNoErrors(MAWB.CM_FlightNoInfo);
			AssertNoErrors(MAWB.CM_RL_NKFirstArrivalPortInfo);
			AssertNoErrors(MAWB.CM_ArrivalDateInfo);

			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_ConsolidatedInvoiceRef = "GQF123061023INBOM";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_TransactionNum = "111";
			invoice.AH_InvoiceDate = ZDateTime.Today;
			Factory.Save();

			MAWB.CM_FlightNo = "QF222";
			AssertHasErrors(MAWB.CM_FlightNoInfo);
			AssertNoErrors(MAWB.CM_RL_NKFirstArrivalPortInfo);
			AssertNoErrors(MAWB.CM_ArrivalDateInfo);

			MAWB.CM_RL_NKFirstArrivalPort = "AUSYD";
			AssertHasErrors(MAWB.CM_FlightNoInfo);
			AssertHasErrors(MAWB.CM_RL_NKFirstArrivalPortInfo);
			AssertNoErrors(MAWB.CM_ArrivalDateInfo);

			MAWB.CM_ArrivalDate = new ZDateTime(2006, 10, 25);
			AssertHasErrors(MAWB.CM_FlightNoInfo);
			AssertHasErrors(MAWB.CM_RL_NKFirstArrivalPortInfo);
			AssertHasErrors(MAWB.CM_ArrivalDateInfo);
		}

		public void TestCheckCM_FlightNo()
		{
			MAWB.Validation.ValidateCM_FlightNo();
			AssertHasMessageErrors("by default", MAWB.CM_FlightNoInfo);

			MAWB.CM_FlightNo = "!!";
			AssertHasWarnings("when invalid", MAWB.CM_FlightNoInfo);

			MAWB.CM_FlightNo = "QF123";
			AssertNoMessageErrors("when valid", MAWB.CM_FlightNoInfo);
			AssertNoErrors("when valid", MAWB.CM_FlightNoInfo);
		}

		public override void TestCheckCM_RL_NKDischargePort()
		{
			MAWB.CM_RL_NKDischargePort = "";
			AssertHasMessageError(MAWB.CM_RL_NKDischargePortInfo, "Valid discharge port is required.");

			MAWB.CM_RL_NKDischargePort = "USLAX";
			AssertNoMessageErrors(MAWB.CM_RL_NKDischargePortInfo);

			MAWB.CM_RL_NKDischargePort = "AAAAA";
			AssertHasMessageError(MAWB.CM_RL_NKDischargePortInfo, "Valid discharge port is required.");
		}

		protected override Customs.Business.CusMAWB GetNewMAWB() => Factory.New<CTOCusMAWB>();

		CTOCusMAWB mawb;
		CTOCusMAWB MAWB => mawb ?? (mawb = (CTOCusMAWB)GetNewMAWB());
	}
}
