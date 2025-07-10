using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class Import008Header : IImport008Header
	{
		public string ImportDeclarationNumber { get; set; }
		public string DeclarationCustomsOffice { get; set; }
		public string DeclarationCustomsDivision { get; set; }
		public Organisation Declarant { get; set; }
		public Import008Person Owner { get; set; }
		public string ForeignCountryCode { get; set; }
		public string ForeignCity { get; set; }
		public string DecType { get; set; }
		public string LoadingPort { get; set; }
		public string HBL { get; set; }
		public DateTime TransportationStartDate { get; set; }
		public DateTime TransportationArrivalDate { get; set; }
		[DecimalPlaces(DecimalPlacesConstants.Freight)]
		public decimal Freight { get; set; }
		public string ForeignCarrier { get; set; }
		public string DomesticCarrier { get; set; }
		public Import008DecQuestion[] QuestionsAndAnswers { get; set; }
		public Import008BulkItem Vehicle { get; set; }
		public Import008Person[] FamilyMembers { get; set; }
		public Import008Line[] Lines { get; set; }

		ZString IImport008Header.ImportDeclarationNumber => ImportDeclarationNumber;
		ZString IImport008Header.DeclarationCustomsOffice => DeclarationCustomsOffice;
		ZString IImport008Header.DeclarationCustomsDivision => DeclarationCustomsDivision;
		IOrganization IImport008Header.Declarant => Declarant;
		IImport008Person IImport008Header.Owner => Owner;
		ZString IImport008Header.ForeignCountry => ForeignCountryCode;
		ZString IImport008Header.ForeignCity => ForeignCity;
		ZString IImport008Header.DecType => DecType;
		ZString IImport008Header.LoadingPort => LoadingPort;
		ZString IImport008Header.HBL => HBL;
		ZDate IImport008Header.TransportationStartDate => new ZDate(TransportationStartDate);
		ZDate IImport008Header.TransportationArrivalDate => new ZDate(TransportationArrivalDate);
		ZDecimal IImport008Header.Freight => Freight;
		ZString IImport008Header.ForeignCarrier => ForeignCarrier;
		ZString IImport008Header.DomesticCarrier => DomesticCarrier;
		IEnumerable<IImport008DecQuestion> IImport008Header.QuestionsAndAnswers => QuestionsAndAnswers;
		IImport008BulkItem IImport008Header.Vehicle => Vehicle;
		IEnumerable<IImport008Person> IImport008Header.FamilyMembers => FamilyMembers;
		IEnumerable<IImport008Line> IImport008Header.Lines => Lines;
	}
}
