namespace Enterprise.Integration
{
	//For Registry will be removed.
	public interface IeAdaptorNextOutboundConfig : IOAuth2Parameters
	{
		IeAdaptorNextOutboundConfig Clone();
		bool Equals(object obj);
		int GetHashCode();
	}
}
