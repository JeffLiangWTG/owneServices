using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class ProcedureRegimeDecider : Customs.Business.ProcedureRegimeDecider
	{
		public override bool IsIntoRegime(RefCusProcedure procedure) => base.IsIntoRegime(procedure) || procedure.IsIntoVATWarehouse();
	}
}
