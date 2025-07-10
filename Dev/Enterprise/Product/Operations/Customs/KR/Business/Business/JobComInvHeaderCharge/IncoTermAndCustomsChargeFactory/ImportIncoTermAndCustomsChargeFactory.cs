using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ITOTIncoTerm;

namespace Enterprise.Customs.KR.Business
{
	public class ImportIncoTermAndCustomsChargeFactory : IncoTermAndCustomsChargeFactory
	{
		protected override void SetupIncotermChargeConfigurations()
		{
			SetUpForCFRAndCPT(IncotermList.Codes.CostAndFreight);
			SetUpForCFRAndCPT(IncotermList.Codes.CarriagePaidTo);
			SetUpForCIN();
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.CostInsuranceAndFreight);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.CarriageAndInsurancePaidTo);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredAtFrontier);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredAtPlace);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredAtTerminal);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredDutyPaid);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredDutyUnpaid);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredExQuay);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredExShip);
			SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(IncotermList.Codes.DeliveredAtPlaceUnloaded);
			SetUpForEXW_FAS_FCAAndFOB(IncotermList.Codes.ExWorks);
			SetUpForEXW_FAS_FCAAndFOB(IncotermList.Codes.FreeAlongsideShip);
			SetUpForEXW_FAS_FCAAndFOB(IncotermList.Codes.FreeCarrier);
			SetUpForEXW_FAS_FCAAndFOB(IncotermList.Codes.FreeOnBoard);
		}

		void SetUpForCFRAndCPT(string incoterm)
		{
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A102, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A104, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A105, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A106, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A107, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A108, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A109, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A110, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A111, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A112, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A114, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A115, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A116, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A118, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A119, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A120, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A121, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B303, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B304, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B305, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B306, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B307, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B309, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B310, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B311, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B312, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B313, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B404, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B405, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B406, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B407, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B408, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B409, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B410, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B411, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B501, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B502, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B503, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
		}
		void SetUpForCIN()
		{
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A102, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A104, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A105, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A106, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A107, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A108, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A109, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A110, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A111, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A112, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A114, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A115, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A116, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A118, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A119, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A120, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.A121, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });

			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B303, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B304, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B305, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B306, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B307, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B309, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B310, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B311, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B312, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B313, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B404, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B405, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B406, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B407, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B408, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B409, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B410, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B411, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B501, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B502, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(IncotermList.Codes.CostAndInsurance, CustomsChargeCodeProvider.B503, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
		}

		void SetUpForCIF_CIP_DAF_DAP_DAT_DDP_DDU_DEQ_DESAndDPU(string incoterm)
		{
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A102, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A104, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A105, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A106, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A107, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A108, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A109, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A110, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A111, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A112, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A114, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A115, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A116, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A118, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A119, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A120, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A121, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B303, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B304, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B305, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B306, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B307, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B309, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B310, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B311, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B312, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B313, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B404, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B405, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B406, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B407, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B408, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B409, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B410, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B411, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B501, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B502, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B503, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
		}

		void SetUpForEXW_FAS_FCAAndFOB(string incoterm)
		{
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A102, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A104, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A105, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A106, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A107, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A108, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A109, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A110, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A111, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A112, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A114, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A115, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A116, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A118, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A119, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A120, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.A121, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B303, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B304, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B305, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B306, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B307, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B309, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B310, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B311, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B312, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = false });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B313, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B404, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B405, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B406, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B407, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B408, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B409, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B410, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B411, new ChargeConfiguration() { IsIncludedInInvoice = true, IsIncludedInInvoiceAmountFixed = true });

			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B501, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B502, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
			AddChargeConfiguration(incoterm, CustomsChargeCodeProvider.B503, new ChargeConfiguration() { IsIncludedInInvoice = false, IsIncludedInInvoiceAmountFixed = true });
		}

		protected override void SetupErrorConfiguration()
		{
		}

		protected override ICustomsChargeCode[] GetCharges() => CustomsChargeCodeProvider.Charges;

		protected override IEnumerable<IITOTIncoTermCalculator> GetIITOTIncoTermCalculators()
		{
			throw new System.NotImplementedException();
		}

		public override bool MakeFlagsReadOnlyWhenDeemed => true;
	}
}
