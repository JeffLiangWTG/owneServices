namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public interface ITotalValueAndUnits
	{
		ValueAndUnitSelfTotaller GetNewForTotalling();
		void AddSelfToResult(ValueAndUnitSelfTotaller result);
	}
}