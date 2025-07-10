using CargoWise.Types;
using Enterprise.Customs.CA.Services;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSValidationQueriedLineRegistration : IAIRSValidationQueriedLineRegistration
	{
		#region Properties

		public ZString RegistrationId { get; set; }
		public AIRSValidationQueriedLineRegistrationTypes RegistrationType { get; set; }

		#endregion

		#region IAIRSValidationQueriedLineRegistration Members

		string IAIRSValidationQueriedLineRegistration.RegistrationId
		{
			get { return RegistrationId; }
		}

		AIRSValidationQueriedLineRegistrationTypes IAIRSValidationQueriedLineRegistration.RegistrationType
		{
			get { return RegistrationType; }
		}

		#endregion

		public override bool Equals(object obj)
		{
			var compareObj = obj as AIRSValidationQueriedLineRegistration;
			return compareObj == null ? base.Equals(obj)
				: this.RegistrationId == compareObj.RegistrationId &&
				this.RegistrationType == compareObj.RegistrationType;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ RegistrationId.GetHashCode() ^
				RegistrationType.GetHashCode();
		}
	}
}
