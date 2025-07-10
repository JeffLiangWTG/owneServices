using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICalculatedCusStatusCalculator
	{
		void DeriveStatusIfEmptyWithMessages();
		void DeriveStatusNow();
		ZString UserFriendlyStatusText { get; }
		void ResetToOriginal();
	}
}
