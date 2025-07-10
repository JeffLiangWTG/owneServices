using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881ComplementaryInformationProvider : ITextAndLanguage
	{
		public IE881ComplementaryInformationProvider(LsdComplementaryInformationType complementaryInformation)
		{
			this.complementaryInformation = Argument.NotNull(complementaryInformation, nameof(complementaryInformation));
		}
		readonly LsdComplementaryInformationType complementaryInformation;

		public string Text => complementaryInformation.Value;

		public string Language => complementaryInformation.Language;
	}
}
