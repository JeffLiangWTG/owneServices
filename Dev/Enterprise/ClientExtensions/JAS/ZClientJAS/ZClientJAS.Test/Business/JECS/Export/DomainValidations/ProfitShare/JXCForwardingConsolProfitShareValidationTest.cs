using System;
using CargoWise.ComponentModel;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingConsolProfitShareValidationTest : JXCForwardingConsolObsoleteValidationTest
	{
		public void TestPortOfLoadingAndDischargeShouldHaveErrorsIfNotSpecified()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "ITMIL";
			Assert("Valid UNLOCO, should not have any errors", !Consol.JK_RL_NKLoadPortInfo.HasErrors());
			Assert("Valid UNLOCO, should not have any errors", !Consol.JK_RL_NKDischargePortInfo.HasErrors());
			Consol.JK_RL_NKLoadPort = "";
			Consol.JK_RL_NKDischargePort = "";
			Assert("Empty UNLOCO, should have error. Add JXC validation if this fails", Consol.JK_RL_NKLoadPortInfo.HasErrors());
			Assert("Empty UNLOCO, should have error. Add JXC validation if this fails", Consol.JK_RL_NKDischargePortInfo.HasErrors());
		}

		public void TestValidateJK_RL_NKLoadPort()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			AssertHasNoJXCWarnings(Consol.JK_RL_NKLoadPortInfo);
			Consol.JK_RL_NKLoadPort = "";
			AssertHasNoJXCWarnings("PortOfLoading is not specified; Don't bother validating for JXC", Consol.JK_RL_NKLoadPortInfo);
			Consol.JK_RL_NKLoadPort = "AUSUH";
			Assert("Sanity check - change the UNLOCO if Surry Hills happens to have IATA code", Consol.LoadPort.RL_IATA.IsEmpty);
			AssertHasJXCWarning(Consol.JK_RL_NKLoadPortInfo, JXCConstants.JXCWarningPrefix + "Port does not have IATA Code");
		}

		public void TestValidateJK_RL_NKDischargePort()
		{
			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertHasNoJXCWarnings(Consol.JK_RL_NKDischargePortInfo);
			Consol.JK_RL_NKDischargePort = "";
			AssertHasNoJXCWarnings("PortOfDischarge is not specified; Don't bother validating for JXC", Consol.JK_RL_NKDischargePortInfo);
			Consol.JK_RL_NKDischargePort = "AUSUH";
			Assert("Sanity check - change the UNLOCO if Surry Hills happens to have IATA code", Consol.DischargePort.RL_IATA.IsEmpty);
			AssertHasJXCWarning(Consol.JK_RL_NKDischargePortInfo, JXCConstants.JXCWarningPrefix + "Port does not have IATA Code");
		}

		public void TestValidateJK_MasterBillNum()
		{
			Consol.JK_MasterBillNum = "08112345678";
			AssertHasNoJXCWarnings("Valid MasterBill Airline Prefix, should not have any errors", Consol.MasterBillAirlinePrefixInfo);
			AssertHasNoJXCWarnings("Valid MasterBill Airline Prefix, should not have any errors", Consol.MasterBillMAWBInfo);
			Consol.JK_MasterBillNum = "ABC1234567a";
			AssertHasNumericExactLengthJXCWarning(Consol.MasterBillAirlinePrefixInfo, 3);
			AssertHasNumericExactLengthJXCWarning(Consol.MasterBillMAWBInfo, 8);
		}

		protected override Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingConsolProfitShareValidation);
			}
		}
	}
}
