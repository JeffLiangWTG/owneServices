namespace Enterprise.Integration
{
	public interface IAdditionalInformation
	{
		string AdditionalInformation { get; }
	}

	public interface IAdditionalInformationWithSetter : IAdditionalInformation
	{
		void SetAdditionalInformation(string additionalInformation);
	}
}
