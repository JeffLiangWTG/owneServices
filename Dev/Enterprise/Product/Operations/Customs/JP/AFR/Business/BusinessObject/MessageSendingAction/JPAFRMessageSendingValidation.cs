using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRMessageSendingValidation : MessageSendingValidation
	{
		protected JPAFRMessageSendingValidation(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
			: base(topLevelBusinessObjectForValidation, messageErrors, Env.Security.JPAFRReportingSendWithMessageErrors)
		{
		}

		public static MessageSendingValidation New(BusinessObject topLevelBusinessObjectForValidation, IEnumerable<INotification> messageErrors)
		{
			MessageSendingValidation result;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(topLevelBusinessObjectForValidation, messageErrors);
			}
			else
			{
				result = new JPAFRMessageSendingValidation(topLevelBusinessObjectForValidation, messageErrors);
			}
			return result;
		}
	}
}
