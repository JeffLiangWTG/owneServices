using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARInvoiceLineValidation : InvoiceLineValidation
	{
		public ARInvoiceLineValidation(InvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckGenericCharge()
		{
			base.CheckGenericCharge();
			if (!AccountingConfigurationRegistry.Instance.ENettRegistration.Value.RegistrationCode.IsEmpty &&
									 Parent.TransactionHeader.Header != null &&
										!Parent.TransactionHeader.Header.ENettRegistrationNumber.IsEmpty)
			{
				if (!Parent.AL_AC.IsEmpty && Parent.AL_AC.IsValid)
				{
					if (Parent.ChargeCode.GetMappingForOrganisation(AccountingConfigurationRegistry.Instance.ENettRegistration.Value.OrganisationPK).IsEmpty)
					{
						InvoiceLine.GenericChargeInfo.AddWarning(eNettHelper.NoEnettMappingError);
					}
				}
				else if (!Parent.AL_AG.IsEmpty && Parent.AL_AG.IsValid)
				{
					InvoiceLine.GenericChargeInfo.AddWarning(Res.GetString("544607ff-c806-4a21-b191-cc78f5ca0ffd", "This GL Account doesn't have an eNett mapping."));
				}
			}

			CheckCommentChargeLines();
		}

		void CheckCommentChargeLines()
		{
			if (InvoiceLine.IsCommentCharge)
			{
				var commentChargeLineRegistryValue = AccountingConfigurationRegistry.Instance.CommentChargeLineARInvoiceWarning.Value;
				if (commentChargeLineRegistryValue == AccountingConstants.CommentChargeLineARInvoiceWarningOptions.WarningValidation)
				{
					InvoiceLine.GenericChargeInfo.AddWarning(AccountingConstants.CommentChargeLineValidationMessage);
				}
				else if (commentChargeLineRegistryValue == AccountingConstants.CommentChargeLineARInvoiceWarningOptions.ErrorValidation)
				{
					InvoiceLine.GenericChargeInfo.AddError(AccountingConstants.CommentChargeLineValidationMessage);
				}
			}
		}

		#region Implementation

		ARInvoiceLine InvoiceLine => (ARInvoiceLine)Parent;

		#endregion
	}
}
