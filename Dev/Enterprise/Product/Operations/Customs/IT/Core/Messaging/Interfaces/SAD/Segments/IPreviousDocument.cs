using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IPreviousDocument : IPreviousAdministrativeReference
{
	ZString Category { get; }
	ZString DocType { get; }
	ZString Mrn { get; }
	ZString ComplementOfInformation { get; }
}
