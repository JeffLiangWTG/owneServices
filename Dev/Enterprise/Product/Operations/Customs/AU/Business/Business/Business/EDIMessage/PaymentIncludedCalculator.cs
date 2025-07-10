using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IPaymentIncluded
	{
		GISSegmentMessageSection GISSegments { get; }
		bool IsPaymentIncluded { get; }
	}

	public class IsPaymentIncludedCalculator
	{
		public IsPaymentIncludedCalculator(IPaymentIncluded message)
		{
			this.message = message;
		}

		public bool IsPaymentIncluded
		{
			get
			{
				if (needToRefreshIsPaymentIncluded)
				{
					if (message.GISSegments != null)
					{
						foreach (GISSegment gIS in message.GISSegments)
						{
							if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode.ToString() == "EPA")
							{
								fIsPaymentIncluded = true;
								break;
							}
						}
					}
					needToRefreshIsPaymentIncluded = false;
				}
				return fIsPaymentIncluded;
			}
		}
		bool needToRefreshIsPaymentIncluded = true;
		bool fIsPaymentIncluded;

		readonly IPaymentIncluded message;
	}
}
