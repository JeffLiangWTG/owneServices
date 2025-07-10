using Enterprise.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Ccsuk.Declaration
{
	public class CcsukCDSInterchangeSender : CcsukInterchangeSender
	{
		public CcsukCDSInterchangeSender(ILogger serviceLogger) : base(serviceLogger)
		{
		}

		public override string ApplicationCode
		{
			get { return ApplicationCodeList.Codes.GbCDSViaCCSUK; }
		}
	}
}
