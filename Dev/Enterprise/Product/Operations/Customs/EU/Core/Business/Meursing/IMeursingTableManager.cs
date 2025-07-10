namespace Enterprise.Customs.EU.Business.Meursing
{
	public interface IMeursingTableManager
	{
		MeursingTable MeursingTable { get; }
		void Execute();
		IMeursingTarget meursingTarget { get; }
	}
}
