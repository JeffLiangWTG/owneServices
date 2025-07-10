using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5TEHeader
	{
		ZString ImportDeclarationNumber { get; }
		ZInt SequenceNo { get; }
		ZString ApplicationReason { get; }
		ZString BrokerID { get; }
		IOrganization Payer { get; }
	}
}
