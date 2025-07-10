using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport008BulkItem
	{
		ZString Type { get; }
		ZString ModelName { get; }
		ZString IdentificationNumber { get; }
		ZDecimal EngineDisplacement { get; }
		ZString ModelYear { get; }
		ZString ManufacturingCountry { get; }
		ZInt SeatingCapacity { get; }
		ZDate FirstRegistrationDate { get; }
		ZDate CurrentRegistrationDate { get; }
	}
}
