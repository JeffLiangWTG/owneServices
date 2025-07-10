using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.CA.Services;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSValidationQueriedLineRegistrationCollection : List<AIRSValidationQueriedLineRegistration>
	{
		public AIRSValidationQueriedLineRegistration AddNew(string id, AIRSValidationQueriedLineRegistrationTypes type)
		{
			var result = new AIRSValidationQueriedLineRegistration();
			result.RegistrationId = id;
			result.RegistrationType = type;
			this.Add(result);
			return result;
		}

		public override bool Equals(object obj)
		{
			var compareObj = obj as AIRSValidationQueriedLineRegistrationCollection;
			return compareObj == null
				? base.Equals(obj)
				: string.Join("|", this.OrderBy(n => n.RegistrationId).Select(n => n.RegistrationId))
				  == string.Join("|", compareObj.OrderBy(n => n.RegistrationId).Select(n => n.RegistrationId))
				  && string.Join("|", this.OrderBy(n => n.RegistrationId).Select(n => n.RegistrationType))
				  == string.Join("|", compareObj.OrderBy(n => n.RegistrationId).Select(n => n.RegistrationType));
		}

		public override int GetHashCode()
		{
			var hash = base.GetHashCode();
			ForEach(registration => hash ^= registration.GetHashCode());
			return hash;
		}
	}
}
