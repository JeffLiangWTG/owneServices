using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED837HeaderProvider : HeaderProvider, IED837Header
	{
		public ED837HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IExplanationOnDelay explanationOnDelay) : base(emcsJobDeclaration)
		{
			this.explanationOnDelay = Argument.NotNull(explanationOnDelay, nameof(explanationOnDelay));
			helper = new Message837HeaderProviderHelper(emcsJobDeclaration, explanationOnDelay);
		}
		readonly IExplanationOnDelay explanationOnDelay;
		readonly Message837HeaderProviderHelper helper;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, ZInt.Zero));
		CachedValue<int> sequenceNumberCache;

		public string ExplanationCode => helper.ExplanationCode;

		public string MessageRole => helper.MessageRole;

		public string SubmitterType => helper.SubmitterType;

		public string SubmitterIdentification => helper.SubmitterIdentification;

		public ITextAndLanguage ComplementaryInformation
		{
			get
			{
				if (complementaryInformation == null)
				{
					var information = explanationOnDelay.Information;
					switch (emcsJobDeclaration.JE_DeclarantType)
					{
						case EMCSEntryTypeList.Codes.Consignor:
							complementaryInformation = GetComplementaryInformationWithLanguage(information, emcsJobDeclaration.Supplier?.OH_Language);
							break;
						case EMCSEntryTypeList.Codes.Consignee:
							complementaryInformation = GetComplementaryInformationWithLanguage(information, emcsJobDeclaration.Consignee?.OH_Language);
							break;
						default:
							complementaryInformation = GetComplementaryInformation(information);
							break;
					}
				}
				return complementaryInformation;
			}
		}

		ITextAndLanguage complementaryInformation;

		public bool IsDeclarantTypeConsignor => SubmitterType == EMCSEntryTypeList.Codes.Consignor;

		public bool ComplementaryInformationSpecified => helper.ComplementaryInformationSpecified;
	}
}
