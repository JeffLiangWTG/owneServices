using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class SpecialMentionWrapper : ISpecialMention
	{
		public SpecialMentionWrapper(ZString additionalInformationCoded)
		{
			this.additionalInformationCoded = additionalInformationCoded;
		}
		readonly ZString additionalInformationCoded;

		public string AdditionalInformationCoded => additionalInformationCoded;
	}
}
