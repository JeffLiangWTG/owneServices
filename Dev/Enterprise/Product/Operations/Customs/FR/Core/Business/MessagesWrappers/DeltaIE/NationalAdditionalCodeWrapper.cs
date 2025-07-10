using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class NationalAdditionalCodeWrapper : INationalAdditionalCode
	{
		NationalAdditionalCodeWrapper(string nationalCode, string customsOffice)
		{
			this.nationalCode = Argument.NotNull(nationalCode, nameof(nationalCode));
			this.customsOffice = customsOffice;
		}

		readonly string nationalCode;
		readonly string customsOffice;

		public static NationalAdditionalCodeWrapper New(string nationalCode, string customsOffice) => nationalCode == null ? null : new NationalAdditionalCodeWrapper(nationalCode, customsOffice);

		public string CcQualifier => ccQualifier ?? (ccQualifier = customsOffice.StartsWith(Core.Constants.CountryCodes.France) ? string.Empty : Core.Constants.CountryCodes.France);
		string ccQualifier;

		public string NationalAdditionalCode => nationalAdditionalCode ?? (nationalAdditionalCode = nationalCode);
		string nationalAdditionalCode;
	}
}
