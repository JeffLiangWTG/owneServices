using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationGovernmentAgencyGoodsItemProvider : IDeclarationGovernmentAgencyGoodsItem
	{
		public DeclarationGovernmentAgencyGoodsItemProvider(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		readonly CusEntryLine entryLine;

		JobComInvoiceLine invoiceLine => entryLine.RandomLine;

		public decimal CustomsValueAmount => invoiceLine.JI_Calc_FOB;
		public decimal ValueAmount => invoiceLine.JI_Calc_InvAmount;
		public decimal FinancedValueAmount => ZDecimal.Zero;
		public int SequenceNumeric => entryLine.CL_LineNumber;
		public string DestinationCountry => invoiceLine.JI_RN_NKCountryOfExport;

		public IEnumerable<string> LPCONumbers => invoiceLine.LPCOJobComInvLineRefsCollection.Cast<LPCOJobComInvLineRefs>().Where(x => !x.JG_ReferenceNumber.IsEmpty).Select(x => x.JG_ReferenceNumber.ToString());

		public IEnumerable<IDeclarationDrawback> SuspensionDrawbacks
		{
			get
			{
				if (suspensionDrawbacks == null)
				{
					suspensionDrawbacks = new List<IDeclarationDrawback>();
					foreach (var drawback in invoiceLine.SuspensionDrawbackCollection.Cast<SuspensionDrawback>())
					{
						suspensionDrawbacks.Add(new DeclarationDrawbackProvider(drawback));
					}
				}
				return suspensionDrawbacks;
			}
		}
		List<IDeclarationDrawback> suspensionDrawbacks;

		public IEnumerable<IDeclarationPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					previousDocuments = new List<IDeclarationPreviousDocument>();
					foreach (var previousDocument in invoiceLine.PreviousDocuments.Cast<PreviousDocument>())
					{
						previousDocuments.Add(new DeclarationPreviousDocumentProvider(previousDocument));
					}
				}
				return previousDocuments;
			}
		}
		List<IDeclarationPreviousDocument> previousDocuments;

		public IDeclarationCommodity Commodity => fCommodity ?? (fCommodity = new DeclarationCommodityProvider(invoiceLine));
		IDeclarationCommodity fCommodity;
		public string DigitalServiceDossiers => invoiceLine.JI_DigitalServiceDossier;
		public int IntendedTerms => invoiceLine.JI_IntendedTermDays;
		public string CargoPriority => invoiceLine.JI_CargoPriority;
		public decimal NetWeight => invoiceLine.JI_NetWeight;
		public string CPCCode => invoiceLine.JI_Procedure;
		public decimal AgentCommission => invoiceLine.JI_AgentCommissionPercentage;
		public short NfeItemNumber => ZShort.ParseSafe(invoiceLine.JI_NFeItemNumber, ZShort.Zero);
		public string CPCCodeSecond => invoiceLine.JI_SecondCPC;
		public string CPCCodeThird => invoiceLine.JI_ThirdCPC;
		public string CPCCodeFourth => invoiceLine.JI_FourthCPC;
		public decimal CustomsQuantity => invoiceLine.JI_CustomsQuantity;
		public decimal InvoiceQuantity => invoiceLine.JI_InvoiceQuantity;
		public string InvoiceQuantityUQCode => invoiceLine.JI_InvoiceUQ;
		public string Justification => invoiceLine.JI_ExportJustificationInfo;
	}
}
