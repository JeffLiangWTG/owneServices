using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class GbDeclarationDropOffMessageActionMethodApplicatorValidation : AutoGbDeclarationDropOffMessageActionMethodApplicatorValidation
	{
		public GbDeclarationDropOffMessageActionMethodApplicatorValidation(AutoGbDeclarationDropOffMessageActionMethodApplicator parent)
			: base(parent)
		{ }

		protected override void CheckETA()
		{
			base.CheckETA();
			MandatoryValidation.CheckEntered(Parent.ETAInfo);
		}

		protected override void CheckETD()
		{
			base.CheckETD();
			MandatoryValidation.CheckEntered(Parent.ETDInfo);
		}

		protected override void CheckVehicle()
		{
			base.CheckVehicle();
			MandatoryValidation.CheckEntered(Parent.VehicleInfo);
		}
	}
}
