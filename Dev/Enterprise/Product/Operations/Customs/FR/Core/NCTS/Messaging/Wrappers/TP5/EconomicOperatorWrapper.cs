using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class EconomicOperatorWrapper : IEconomicOperator
	{
		EconomicOperatorWrapper(string identificationNumber)
		{
			this.identificationNumber = Argument.NotNull(identificationNumber, nameof(identificationNumber));
		}
		readonly string identificationNumber;

		public static EconomicOperatorWrapper New(string identificationNumber) => string.IsNullOrEmpty(identificationNumber) ? null : new EconomicOperatorWrapper(identificationNumber);

		public string IdentificationNumber => identificationNumber.StartsWith(Core.Constants.CountryCodes.France) ? identificationNumber : Core.Constants.CountryCodes.France + identificationNumber;
	}
}
