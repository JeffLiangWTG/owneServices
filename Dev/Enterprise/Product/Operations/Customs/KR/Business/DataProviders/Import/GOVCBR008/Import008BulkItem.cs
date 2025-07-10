using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import008BulkItem : IImport008BulkItem
	{
		public string Type { get; set; }
		public string ModelName { get; set; }
		public string IdentificationNumber { get; set; }
		[DecimalPlaces(Constants.DecimalPlacesConstants.EngineDisplacement)]
		public decimal EngineDisplacement { get; set; }
		public string ModelYear { get; set; }
		public string ManufacturingCountry { get; set; }
		public int SeatingCapacity { get; set; }
		public DateTime FirstRegistrationDate { get; set; }
		public DateTime CurrentRegistrationDate { get; set; }

		ZString IImport008BulkItem.Type => Type;
		ZString IImport008BulkItem.ModelName => ModelName;
		ZString IImport008BulkItem.IdentificationNumber => IdentificationNumber;
		ZDecimal IImport008BulkItem.EngineDisplacement => EngineDisplacement;
		ZString IImport008BulkItem.ModelYear => ModelYear;
		ZString IImport008BulkItem.ManufacturingCountry => ManufacturingCountry;
		ZInt IImport008BulkItem.SeatingCapacity => SeatingCapacity;
		ZDate IImport008BulkItem.FirstRegistrationDate => new ZDate(FirstRegistrationDate);
		ZDate IImport008BulkItem.CurrentRegistrationDate => new ZDate(CurrentRegistrationDate);
	}
}
