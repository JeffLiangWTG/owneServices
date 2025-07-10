using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message837HeaderProviderHelper : HeaderProviderHelper
	{
		public Message837HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration, IExplanationOnDelay explanationOnDelay) : base(emcsJobDeclaration)
		{
			this.explanationOnDelay = explanationOnDelay;
		}
		readonly IExplanationOnDelay explanationOnDelay;

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

		public bool ComplementaryInformationSpecified => ExplanationCode == EMCSExplanationOnDelayCodeList.Codes.Other;
	}
}
