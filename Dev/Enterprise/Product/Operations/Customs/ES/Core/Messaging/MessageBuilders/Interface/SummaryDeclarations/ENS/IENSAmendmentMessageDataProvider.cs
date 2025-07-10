using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IENSAmendmentMessageDataProvider : IENSCommonMessageDataProvider
	{
		IENSAmendmentHeader Header { get; }
	}

	public interface IENSAmendmentHeader : IENSCommonHeader
	{
		ZString MRN { get; }
		ZString AmendmentPlace { get; }
		ZString AmendmentPlaceLanguage { get; }
		ZDateTime AmendmentDate { get; }
	}
}
