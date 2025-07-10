namespace Enterprise.Integration.Rating
{
	public interface IAutoRatingInteractor : ILogger
	{
		bool YesNoWarning(string message);
	}
}
