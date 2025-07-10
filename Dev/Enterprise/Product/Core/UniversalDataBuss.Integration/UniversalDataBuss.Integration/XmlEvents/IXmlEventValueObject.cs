using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlEventValueObject : ITopLevelDataObject
	{
		ZString EventType { get; }
		ZString EventReference { get; }
		ZDateTimeOffset EventTime { get; }

		ZDateTimeOffset CreatedTime { get; }
		ZBool IsEstimate { get; }
		ZBool IsCancelled { get; }

		IXmlEventValueObjectContextValueList Context { get; }
	}
}
