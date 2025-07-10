using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument : RefCountryValidationRule
	{
		public DispatchOfficeMustBeGermanIfDeclarationIsConsolidatedDocument(EMCSOfficeCode officeCode)
		{
			this.officeCode = officeCode;
			declaration = officeCode.Parent as EMCSJobDeclaration;
		}
		readonly EMCSOfficeCode officeCode;
		readonly EMCSJobDeclaration declaration;

		public override bool IsApplied => officeCode.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDispatch && declaration.IsConsolidatedDocument();

		protected override ValidationResult ValidateCore(RefCountry country)
		{
			return country.Code == Core.Constants.CountryCodes.Germany
				? ValidationResult.Valid
				: ValidationResult.Invalid(Res.GetString("C1C79A7B-9380-4BA7-BAC8-4E02C29982F8", "The entered Office Of Dispatch (DIS) has to be a German Customs Office"));
		}

		protected override INotificationType NotificationSeverityCore => CargoWise.EntityFramework.NotificationType.MessageError;
	}
}
