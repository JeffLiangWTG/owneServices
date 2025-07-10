using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public interface ISendGBEMCSMessages : ISendEMCSMessages
	{
		void SendSplitting();
	}
}
