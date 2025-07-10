using CargoWise.Types;
namespace Enterprise.Integration.Rating
{
	public interface IQuote
	{
		ZBool SameClientCopy { get; set; }
		ZBool AmendmentCopy { get; set; }
		ZBool TH_IsCancelled { get; set; }
		ZString TH_QuoteNumber { get; set; }
	}
}
