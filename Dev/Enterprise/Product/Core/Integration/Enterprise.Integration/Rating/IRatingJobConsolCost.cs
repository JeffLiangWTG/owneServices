using Enterprise.Integration.Accounting;

namespace Enterprise.Integration.Rating
{
	public interface IRatingJobConsolCost : IChargeWithChargeCode
	{
		string RatingBehaviour { get; }
		decimal OSCostAmount { get; }
	}
}
