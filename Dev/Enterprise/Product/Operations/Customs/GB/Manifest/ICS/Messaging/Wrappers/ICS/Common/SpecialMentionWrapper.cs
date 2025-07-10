using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	internal class SpecialMentionWrapper : ISpecialMention
	{
		readonly ZString additionalInformationCoded;

		public SpecialMentionWrapper(ZString additionalInformationCoded)
		{
			this.additionalInformationCoded = additionalInformationCoded;
		}

		ZString ISpecialMention.AdditionalInformationCoded => additionalInformationCoded;
	}
}
