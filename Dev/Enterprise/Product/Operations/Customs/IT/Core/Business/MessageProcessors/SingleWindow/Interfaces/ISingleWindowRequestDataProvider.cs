using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface ISingleWindowRequestDataProvider
{
	ZGuid PK { get; }

	ZString ApplicationReference { get; }

	ZDate IssueDate { get; }

	ZString RegisterIncludingSeries { get; }

	ZString RegistrationNumberWithoutCin { get; }

	ZString CustomsOffice { get; }

	ZString TableName { get; }
}
