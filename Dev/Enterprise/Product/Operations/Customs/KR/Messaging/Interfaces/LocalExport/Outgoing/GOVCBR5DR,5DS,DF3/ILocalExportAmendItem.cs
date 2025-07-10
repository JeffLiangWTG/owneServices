using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface ILocalExportAmendItem
	{
		ZString DataItemNo { get; }
		ZInt ItemSequenceNumber { get; }
		ZString AmendType { get; }
		ZString BeforeValue { get; }
		ZString AfterValue { get; }
	}
}
