using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class JPAirWayBillValidator : AirWayBillValidator
	{
		protected override ValidationResult GetValidationResult(string masterBillNumber) =>
			base.GetValidationResult(masterBillNumber)
			?? ValidateThreeDigitsAirlineIATACode(masterBillNumber);

		ValidationResult ValidateThreeDigitsAirlineIATACode(string masterBillNumber)
		{
			var airlineIATACode = masterBillNumber.Substring(0, 3);
			var exist = Factory.ExistsInDatabase(RefAirlineSchema.Constants.TableName, new ZQuery(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlineIATACode));
			return exist ? ValidationResult.Valid : new ValidationResult(InvalidThreeDigitsAirlineIATACodeErrorMessage, ThreeDigitsAirlineIATACodeValidationLevel);
		}

		protected string InvalidThreeDigitsAirlineIATACodeErrorMessage
		{
			get { return Res.GetString("0ED6FA7D-44DF-4B3F-A8D0-405D5134DDC7", "The first three characters should be a valid airline IATA code."); }
		}

		protected ValidationLevel ThreeDigitsAirlineIATACodeValidationLevel => ValidationLevel.MessageError;

		protected BusinessObjectFactory Factory => factory ??= new BusinessObjectFactory();
		BusinessObjectFactory factory;
	}
}
