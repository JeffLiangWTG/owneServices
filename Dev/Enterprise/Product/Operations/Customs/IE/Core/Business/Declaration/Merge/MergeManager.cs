using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class MergeManager : EU.Business.Declaration.MergeManager
	{
		public MergeManager(JobDeclaration jobDec)
			: base(jobDec)
		{
		}

		protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);

		protected override string GetReasonCannotMerge()
		{
			ZString reason = base.GetReasonCannotMerge();

			if (reason.IsEmpty)
			{
				if (Declaration.HasInvoiceLineWithoutEntryInstruction)
				{
					reason = Res.GetString("E703FB83-13F8-47EC-80F2-D52FF530361B", "Entry Instruction should be selected on all Invoice Lines");
				}
				if (reason.IsEmpty && Declaration.IsExport)
				{
					foreach (CusEntryInstruction instruction in Declaration.CustomsEntryInstructions)
					{
						if (instruction.HasMultipleDeliveryTerms)
						{
							var invoice = instruction.Invoices.First();
							reason = Res.GetString("33170882-9DA5-4446-9F12-5FC38CDA1CBC", "Cannot merge as Entry Instructions '{0}' is linked to invoices with different delivery terms ({1}, {2}, {3}, {4}).", instruction.HumanReadableName, invoice.JZ_IncoTermInfo.HumanReadableName, invoice.JZ_IncoTermPlaceInfo.HumanReadableName, invoice.ZG_AgreedPlaceCodeInfo.HumanReadableName, invoice.JZ_AdditionalTermsInfo.HumanReadableName);
							break;
						}
						if (instruction.HasMultipleCurrencies)
						{
							reason = Res.GetString("31A6E6CA-590F-4C42-A8E7-E410CD043674", "Cannot merge as Entry Instructions '{0}' is linked to invoices with different currencies ({1}).", instruction.HumanReadableName, instruction.Invoices.First().JZ_RX_NKInvoice_CurrencyInfo.HumanReadableName);
							break;
						}
						if (Declaration.IsTransitionPeriodAES30 && instruction.HasMultipleNatureOfTransactions)
						{
							reason = Res.GetString("3C4DB5AD-7D2E-428A-BC91-6B93C27F413E", "Cannot merge as Entry Instructions '{0}' is linked to invoices with different Nature of Trans. ({1}).", instruction.HumanReadableName, instruction.Invoices.First().JZ_ValuationCodeInfo.HumanReadableName);
							break;
						}
					}
				}
			}

			return reason;
		}
	}
}
