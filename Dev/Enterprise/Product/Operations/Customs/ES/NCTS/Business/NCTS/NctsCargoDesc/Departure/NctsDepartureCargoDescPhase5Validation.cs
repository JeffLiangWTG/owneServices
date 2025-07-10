
namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureCargoDescPhase5Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation
	{
		public NctsDepartureCargoDescPhase5Validation(NctsDepartureCargoDesc parent) : base(parent)
		{
		}

		protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateExciseCode();
		}

		protected override bool IsBY_CommercialReferenceNumberMandatory()
		{
			return (!Parent.Header?.IsPhaseStatusTNN ?? true) && base.IsBY_CommercialReferenceNumberMandatory();
		}

		public void ValidateExciseCode()
		{
			ValidateCalculatedProperty(Parent.ExciseCodeInfo);
		}

		public void ValidatePVPValue()
		{
			ValidateCalculatedProperty(Parent.PVPValueInfo);
		}

		public void ValidatePVPCurrency()
		{
			ValidateCalculatedProperty(Parent.PVPCurrencyInfo);
		}

		protected void CheckExciseCode()
		{
			var list = Parent.ESDepartureCargoDescLookups.ExciseCodeList;
			if (list.Count != 0 && (Parent.ExciseCode.IsEmpty || !list.ContainsCode(Parent.ExciseCode)))
			{
				Parent.ExciseCodeInfo.AddMessageError(Res.GetString("06E35209-DD66-4174-A67B-F80679F69B0F", "[TR0085] An excise code must be selected to calculate the liability amount."));
			}
		}
	}
}
