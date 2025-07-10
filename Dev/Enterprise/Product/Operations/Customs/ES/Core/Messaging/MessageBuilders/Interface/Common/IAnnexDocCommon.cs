using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IAnnexDocCommon
	{
		ZString Description { get; }
		ZString ReferenceNumber { get; }
		ZBlob Image { get; }
		ZString Extension { get; }
	}
}
