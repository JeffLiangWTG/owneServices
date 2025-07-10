using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport008Header : IMessageDataProvider
	{
		ZString ImportDeclarationNumber { get; }
		ZString DeclarationCustomsOffice { get; }
		ZString DeclarationCustomsDivision { get; }
		IOrganization Declarant { get; }
		IImport008Person Owner { get; }
		ZString ForeignCountry { get; }
		ZString ForeignCity { get; }
		ZString DecType { get; }
		ZString LoadingPort { get; }
		ZString HBL { get; }
		ZDate TransportationStartDate { get; }
		ZDate TransportationArrivalDate { get; }
		ZDecimal Freight { get; }
		ZString ForeignCarrier { get; }
		ZString DomesticCarrier { get; }
		IEnumerable<IImport008DecQuestion> QuestionsAndAnswers { get; }
		IImport008BulkItem Vehicle { get; }
		IEnumerable<IImport008Person> FamilyMembers { get; }
		IEnumerable<IImport008Line> Lines { get; }
	}
}
