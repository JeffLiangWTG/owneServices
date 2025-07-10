using System.Collections.Generic;
using Enterprise.Dash.Integration;
using Enterprise.Dash.Integration.Validation;

namespace Enterprise.Dash.Business.Validation
{
	public sealed class DashCommercialInvoiceValidator : IValidator<DashCommercialInvoice>
	{
		public string Validate(DashCommercialInvoice businessObject)
		{
			foreach (var item in ValidationRules)
			{
				var errorMessage = item.Validate(businessObject);

				if (!string.IsNullOrWhiteSpace(errorMessage))
				{
					return errorMessage;
				}
			}

			return string.Empty;
		}

		readonly static IEnumerable<IValidationRule<DashCommercialInvoice>> ValidationRules = new List<IValidationRule<DashCommercialInvoice>>()
		{
			new ValidateMandatoryDataRule(),
			new ValidateInvoiceTotalsRule()
		};
}
}
