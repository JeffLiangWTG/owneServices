namespace Enterprise.Customs.DE.Business.Declaration
{
	public class StockMovementJobDeclarationValidation : ImportJobDeclarationValidation
	{
		public StockMovementJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		protected override void CheckJE_TransportMode()
		{
		}

		protected override void CheckJE_RL_NKOrigin()
		{
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
		}
	}
}
