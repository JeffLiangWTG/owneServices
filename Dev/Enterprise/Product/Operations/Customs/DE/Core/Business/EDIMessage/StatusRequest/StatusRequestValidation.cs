using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public class StatusRequestValidation : EDIMessageValidation
	{
		public StatusRequestValidation(StatusRequest parent) : base(parent)
		{
		}

		protected new StatusRequest Parent => (StatusRequest)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateMovementReferenceNumber();
			ValidateIdentification();
			ValidateRole();
		}

		public void ValidateMovementReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.MovementReferenceNumberInfo);
		}

		protected void CheckMovementReferenceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.MovementReferenceNumberInfo);
			var mrn = Parent.MovementReferenceNumber;
			if (!mrn.IsEmpty)
			{
				var mrnError = MRNFormatValidator.CheckMRNFormat(mrn, Parent.Factory, ZString.Empty);
				if (!mrnError.IsEmpty)
				{
					Parent.MovementReferenceNumberInfo.AddWarning(mrnError);
				}
			}
		}

		public void ValidateIdentification()
		{
			ValidateCalculatedProperty(Parent.IdentificationInfo);
		}

		protected void CheckIdentification()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.IdentificationInfo);
			TypeValidation.CheckValidGuid(Parent.IdentificationInfo);
			if (!Parent.IdentificationOrg.HasEUEoriRegNo())
			{
				Parent.IdentificationInfo.AddMessageError(Res.GetString("db37934d-ac40-4493-8780-172bcfe07c6e", "A Registration Number / Code of type EOR is required."));
			}
		}

		public void ValidateRole()
		{
			ValidateCalculatedProperty(Parent.RoleInfo);
		}

		protected void CheckRole()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.RoleInfo);
		}
	}
}
