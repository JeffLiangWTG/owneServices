using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACJobDeclarationValidation : ImportJobDeclarationValidation
	{
		public SACJobDeclarationValidation(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		public override bool IsMasterBillMandatory
		{
			get { return true; }
		}

		protected override void CheckJE_GoodsDescription()
		{
			base.CheckJE_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_GoodsDescriptionInfo);
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();
			MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_DateOfArrivalInfo);
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			MandatoryValidation.MessageErrorIfNotEntered(JobDeclaration.JE_RL_NKPortOfArrivalInfo);
			if (JobDeclaration.PortOfArrival == null)
			{
				JobDeclaration.JE_RL_NKPortOfArrivalInfo.AddMessageError("You have to enter a valid Port of Arrival");
			}
		}

		protected override void CheckJE_ExportDate()
		{
			this.MessageValidation.CheckInvalidValue(Parent.JE_ExportDateInfo);
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
		}

		protected override void CheckJE_HouseBill()
		{
		}

		protected override void CheckJE_RL_NKOrigin()
		{
		}

		protected override bool IsPortOfLoadingMandatory
		{
			get { return false; }
		}

		protected override bool IsVoyageFlightNoMandatory
		{
			get { return false; }
		}

		protected override bool IsVoyageFlightNoUsedInMessages
		{
			get { return false; }
		}

		protected override bool ShouldCheckCMRImporter
		{
			get { return true; }
		}
	}
}
