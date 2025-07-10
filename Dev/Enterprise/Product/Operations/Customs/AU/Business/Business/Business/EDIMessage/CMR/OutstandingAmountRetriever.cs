using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IOutstandingPaymentInfoProvider
	{
		SegmentGroup5MessageSection Group5Section { get; }
		GISSegmentMessageSection GISSection { get; }
	}

	public class OutstandingAmountRetriever
	{
		public OutstandingAmountRetriever(IOutstandingPaymentInfoProvider provider)
		{
			this.provider = provider;
		}
		readonly IOutstandingPaymentInfoProvider provider;

		public ZDecimal OutstandingAmount
		{
			get
			{
				if (provider.Group5Section != null)
				{
					foreach (SegmentGroup5 currentGroup5 in provider.Group5Section)
					{
						var monetaryAmount = currentGroup5.MOA[0].MonetaryAmount;
						if (monetaryAmount.MonetaryAmountTypeCodeQualifier?.ToString() == MonetaryAmountTypeCodeQualifierList.TotalAmount)
						{
							ZDecimal result = 0m;
							ZDecimal.TryParse(monetaryAmount.MonetaryAmountValue, out result);
							return result;
						}
					}
				}
				return 0m;
			}
		}

		public ZBool HasPaymentPendingProcessingIndicator
		{
			get
			{
				if (provider.GISSection != null)
				{
					foreach (GISSegment gis in provider.GISSection)
					{
						var processingIndicator = gis.ProcessingIndicator_X;
						if (processingIndicator.ProcessingIndicatorDescriptionCode?.ToString() == "Y"
							&& processingIndicator.CodeListIdentificationCode?.ToString() == CodeListIdentificationCodeList.CustomsProcedure
							&& processingIndicator.CodeListResponsibleAgencyCode?.ToString() == CodeListResponsibleAgencyCodeList.AuAustralianCustomsService)
						{
							return true;
						}
					}
				}
				return false;
			}
		}
	}
}
