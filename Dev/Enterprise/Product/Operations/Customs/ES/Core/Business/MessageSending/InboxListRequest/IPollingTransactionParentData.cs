using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public interface IPollingTransactionParent : IESResponseBusinessObject
	{
		ZString CertificateName { get; }
		ZBool IsTest { get; }
		GlbStaff Broker { get; }
		OrgHeader Declarant { get; }
	}
}
