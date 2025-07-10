using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE837HeaderProvider : HeaderProvider, IIE837Header
	{
		public IE837HeaderProvider(EMCSJobDeclaration emcsJobDeclaration, IExplanationOnDelay explanationOnDelay) : base(emcsJobDeclaration)
		{
			this.explanationOnDelay = Argument.NotNull(explanationOnDelay, nameof(explanationOnDelay));
		}
		readonly IExplanationOnDelay explanationOnDelay;

		public int SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCache, () => emcsJobDeclaration.SequenceNumber == "0" ? DefaultSequenceNumber : ZInt.ParseSafe(emcsJobDeclaration.SequenceNumber, DefaultSequenceNumber));
		CachedValue<int> sequenceNumberCache;

		public string Information => explanationOnDelay.Information;

		public string ExplanationCode => explanationOnDelay.ExplanationCode;

		public string MessageRole => explanationOnDelay.MessageRole;

		public string SubmitterType => emcsJobDeclaration.JE_DeclarantType;

		public string SubmitterIdentification => CachedValueHelper.GetValue(ref submitterIdentification, () =>
		{
			var result = string.Empty;
			switch (emcsJobDeclaration.JE_DeclarantType)
			{
				case EMCSEntryTypeList.Codes.Consignor:
					result = emcsJobDeclaration.Supplier.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
					break;
				case EMCSEntryTypeList.Codes.Consignee:
					result = emcsJobDeclaration.Importer.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber);
					break;
			}
			return result;
		});
		CachedValue<string> submitterIdentification;

		public ITextAndLanguage ComplementaryInformation
		{
			get
			{
				if (complementaryInformation == null)
				{
					switch (emcsJobDeclaration.JE_DeclarantType)
					{
						case EMCSEntryTypeList.Codes.Consignor:
							complementaryInformation = GetComplementaryInformation(Information, emcsJobDeclaration.Supplier?.OH_Language);
							break;
						case EMCSEntryTypeList.Codes.Consignee:
							complementaryInformation = GetComplementaryInformation(Information, emcsJobDeclaration.Consignee?.OH_Language);
							break;
						default:
							complementaryInformation = GetComplementaryInformation(Information);
							break;
					}
				}
				return complementaryInformation;
			}
		}
		ITextAndLanguage complementaryInformation;

		public bool ComplementaryInformationSpecified => ExplanationCode == EMCSExplanationOnDelayCodeList.Codes.Other || !string.IsNullOrEmpty(Information);

		public DateTime? DateAndTimeOfValidationOfExplanationOnDelay => this.IsValidationAttributeAllowed ? ZDateTime.Now.ToDateTime().ToUnspecifiedKindWithSecondsPrecision() : null;
	}
}
