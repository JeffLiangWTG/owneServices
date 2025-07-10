using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EdificeJobComInvoiceLineValidationTest : SharedImportJobComInvoiceLineValidationTest
	{
		//public void TestExpiredCountryPreference()
		//{
		//    const string TariffCode = "55091100";

		//    JobDeclaration JobDec = JobDeclaration.New(Factory);
		//    JobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		//    JobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
		//    JobDec.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
		//    JobDec.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
		//    JobDec.JE_RL_NKOrigin = "KRINC";
		//    JobDec.JE_RL_NKFinalDestination = "AUSYD";
		//    JobDec.JE_RL_NKPortOfArrival = "AUSYD";
		//    JobDec.JE_ExportDate = new ZDateTime(2003, 06, 06);
		//    JobDec.JE_DateOfFirstArrival = new ZDateTime(2003, 06, 09);

		//    JobComInvoiceHeader Header = JobDec.Invoices.AddNew();
		//    Header.AddInfo.ZA_ValuationBasis_Hidden = "UT";
		//    Header.JZ_RX_NKInvoice_Currency = "AUD";

		//    JobComInvoiceLine Line = Header.JobComInvoiceLines.AddNew();
		//    Line.JI_AddInfo = "PRF=S*ORG=KR";
		//    Line.JI_LinePrice = 720m;
		//    Line.JI_InvoiceQuantity = 600m;
		//    Line.JI_InvoiceUQ = "KG";
		//    Line.JI_CustomsQuantity = 600m;
		//    Line.JI_CustomsUnitQty = "KG";
		//    Line.JI_Tariff = TariffCode;

		//    JobDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		//    JobDec.DoMerge();

		//    AssertNotNull("Duty Calculation Tariff Item", Line.DutyCalculator.TariffDutyElement);
		//    DutyCalculator TestDutyCalculator = new DutyCalculator(JobDec.CustomsEntryHeaders[0].MergedLines[0]);
		//    AssertEquals("Duty Amount", 36m, TestDutyCalculator.Duty.Amount.Amount);

		//    JobDec.JE_ExportDate = new ZDateTime(2004, 06, 06);
		//    JobDec.JE_DateOfFirstArrival = new ZDateTime(2004, 06, 09);
		//    Line.JI_AddInfo = "PRF=S*ORG=NZ";
		//    JobDec.DoMerge();
		//    TestDutyCalculator = new DutyCalculator(JobDec.CustomsEntryHeaders[0].MergedLines[0]);
		//    AssertEquals("Duty Amount", 0.0m, TestDutyCalculator.Duty.Amount.Amount);

		//    Line.JI_AddInfo = "PRF=S*ORG=KR";
		//    JobDec.DoMerge();
		//    TestDutyCalculator = new DutyCalculator(JobDec.CustomsEntryHeaders[0].MergedLines[0]);
		//    AssertEquals("Duty Amount", 0m, TestDutyCalculator.Duty.Amount.Amount);
		//    AssertEquals("Duty Amount Invalid", false, TestDutyCalculator.Duty.IsValid);
		//}

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine.Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine.Declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
		}
	}

	class EDIFICEJobComInvoiceLineValidationTest : ImportJobComInvoiceLineValidationTest
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			jobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			jobDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			jobDec.JE_ExportDate = new ZDateTime(2004, 3, 1);
		}

		protected override JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine)
		{
			return new EDIFICEJobComInvoiceLineValidationTestRig(invoiceLine);
		}

		#endregion
	}
}
