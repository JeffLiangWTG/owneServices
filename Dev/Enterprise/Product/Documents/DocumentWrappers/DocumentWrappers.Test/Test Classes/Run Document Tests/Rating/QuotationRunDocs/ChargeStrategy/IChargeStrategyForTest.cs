namespace Enterprise.DocumentWrappers.Testing
{
	internal interface IChargeStrategyForTest
	{
		string Charge0 { get; }
		string Charge1 { get; }
		string Charge2 { get; }
		string Charge3 { get; }
		string Charge4 { get; }
		string Charge5 { get; }
		string Charge6 { get; }

		void Setup();
	}
}
