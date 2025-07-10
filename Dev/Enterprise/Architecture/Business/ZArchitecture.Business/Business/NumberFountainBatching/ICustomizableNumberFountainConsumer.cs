namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomizableNumberFountainConsumer : INumberFountainEntityWithID
	{
		ICustomizableNumberFormatter NumberFormatter { get; }
	}
}
