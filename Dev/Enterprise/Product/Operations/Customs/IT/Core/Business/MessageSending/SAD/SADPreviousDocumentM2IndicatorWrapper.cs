using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADPreviousDocumentM2IndicatorWrapper : IPreviousDocument
{
	public ZString Category => "ZZZ";

	public ZString DocType => "Z";

	public ZString Mrn => ZString.Empty;

	public ZString ComplementOfInformation => ZString.Empty;

	public ZString Register => "M2";

	public ZString ReferenceNumber => ZString.Empty;

	public ZString ReferenceCIN => ZString.Empty;

	public ZDate Date => ZDate.Empty;

	public ZString Series => ZString.Empty;

	public ZString CustomsOffice => ZString.Empty;

	public ZInt? ItemNumber => null;
}
