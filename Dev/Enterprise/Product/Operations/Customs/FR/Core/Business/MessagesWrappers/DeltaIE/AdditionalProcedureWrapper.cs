using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class AdditionalProcedureWrapper : IAdditionalProcedure
	{
		AdditionalProcedureWrapper(string additionalProcedureCode, string customsOffice)
		{
			this.additionalProcedureCode = Argument.NotNull(additionalProcedureCode, nameof(additionalProcedureCode));
			this.customsOffice = customsOffice;
		}
		readonly string additionalProcedureCode;
		readonly string customsOffice;

		public static AdditionalProcedureWrapper New(string additionalProcedureCode, string customsOffice) => additionalProcedureCode == null ? null : new AdditionalProcedureWrapper(additionalProcedureCode, customsOffice);

		public string AdditionalProcedure => additionalProcedure ?? (additionalProcedure = additionalProcedureCode);
		string additionalProcedure;

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;
	}
}
