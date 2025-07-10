using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class ConsigneeDataProvider : BaseParticipentDataProvider, IConsignee
{
	public static ConsigneeDataProvider New(JobDocAddress docAddress) => docAddress == null || docAddress.IsEmpty ? null : new ConsigneeDataProvider(docAddress);

	ConsigneeDataProvider(JobDocAddress docAddress) : base(docAddress) { }

	public string ReferenceNumber => null;
}
