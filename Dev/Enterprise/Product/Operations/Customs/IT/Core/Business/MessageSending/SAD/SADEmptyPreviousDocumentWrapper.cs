using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADEmptyPreviousDocumentWrapper : IPreviousDocument
{
	public ZString Category => ZString.Empty;

	public ZString DocType => ZString.Empty;

	public ZString Mrn => ZString.Empty;

	public ZString ComplementOfInformation => ZString.Empty;

	public ZString Register => ZString.Empty;

	public ZString ReferenceNumber => ZString.Empty;

	public ZString ReferenceCIN => ZString.Empty;

	public ZDate Date => ZDate.Empty;

	public ZString Series => ZString.Empty;

	public ZString CustomsOffice => ZString.Empty;

	public ZInt? ItemNumber => null;
}
