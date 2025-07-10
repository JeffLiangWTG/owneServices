using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public static class ValidationHelper
	{
		public static void CheckIATACode(BusinessObjectFactory factory, ZPropertyInfo targetInfo, string caption)
		{
			var iataCode = targetInfo.Value;
			if (iataCode.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("7BBB3E5C-8BD3-4B0D-BA52-6ED0C0167282", "You have not entered a {0} IATA Code.", caption));
			}
			else if (!factory.ExistsInDatabase(RefUNLOCOSchema.Constants.TableName, new ZQuery(RefUNLOCOSchema.RL_IATA, iataCode)))
			{
				targetInfo.AddMessageError(Res.GetString("16926411-CEEF-4253-B7B8-EB4AC279F455", "The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", caption));
			}
		}

		public static void AddMessageErrorIfNotEnteredForSpecificProcedureCodeAndAction(INotificationType notificationType, ZString procedureCode, ZString actionCode, ZString actionDescription, ZPropertyInfo propertyInfo)
		{
			propertyInfo.AddNotification(notificationType,
			Res.GetString("ABF593BA-F481-4B62-9DF0-165CB5974BB8", "[{0} - {1} - {2}] {3} cannot be empty.", procedureCode, actionCode, actionDescription, propertyInfo.HumanReadableName));
		}
	}
}
