using Enterprise.Edifact;
using Enterprise.Edifact.D00A;
using Enterprise.Edifact.D11B;
using Enterprise.Edifact.D96A;
using Enterprise.Edifact.D99B;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class CaEdifactMessageFactory : MessageFactory
	{
		public CaEdifactMessageFactory()
			: base(new D96AMessageFactory(), new EdifactD00AMessageFactory(), new EdifactS99BMessageFactory(), new D11BMessageFactory())
		{
			AddRegisteredMessage(new MessageRegistration(typeof(Enterprise.Edifact.D96A.Messages.CUSREP.CUSREPMessage), "UN", "D", "96A", "CUSREP"));
		}

		class EdifactS99BMessageFactory : EdifactD99BMessageFactory
		{
			public EdifactS99BMessageFactory()
			{
				AddRegisteredMessage(new MessageRegistration(typeof(Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage), "UN", "S", "99B", "CUSRES"));
				AddRegisteredMessage(new MessageRegistration(typeof(Enterprise.Edifact.CA.D99B.Messages.CUSDEC.CUSDECMessage), "UN", "S", "99B", "CUSDEC"));
				AddRegisteredMessage(new MessageRegistration(typeof(Enterprise.Edifact.D99B.Messages.CUSPED.CUSPEDMessage), "UN", "S", "99B", "CUSPED"));
			}
		}
	}
}
