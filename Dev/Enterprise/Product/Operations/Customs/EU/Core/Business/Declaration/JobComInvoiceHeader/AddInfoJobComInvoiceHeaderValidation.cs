using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceHeaderValidation : EUAddInfoValidation
	{
		public AddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected new AddInfoJobComInvoiceHeader Parent => (AddInfoJobComInvoiceHeader)base.Parent;

		protected AddInfoJobComInvoiceHeaderLookups Lookups => Parent.Lookups;

		protected override void CheckZG_AgreedPlaceCode()
		{
			base.CheckZG_AgreedPlaceCode();
			var parent = Parent;
			var codeInfo = parent.ZG_AgreedPlaceCodeInfo;

			var invoiceHeader = parent.Parent;
			if (invoiceHeader?.JobDeclaration is JobDeclaration declaration
				&& declaration.Configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice
				&& parent.ZG_AgreedPlaceCode != declaration.ZG_AgreedPlaceCode)
			{
				AddMessageErrorOrWarningForMismatch();
			}

			if (invoiceHeader?.AgreedPlaceCodeSupportAndVisible ?? false)
			{
				var code = parent.ZG_AgreedPlaceCode;
				if (!code.IsEmpty)
				{
					switch (code.Length)
					{
						case 5:
							if (!parent.IsAgreedUnloco)
							{
								ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("E2446967-7A6F-463E-A8DF-F988FFA8233D", "{0} must be a valid UNLOCODE", codeInfo.HumanReadableName));
							}
							break;
						case 2:
							ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("5377C3C9-E138-4D44-83C0-C515853EC485", "{0} must be a valid country", codeInfo.HumanReadableName));
							break;
						default:
							codeInfo.AddMessageError(ResString.GetMultilingualString("63EFFE52-EA6A-41B7-A0CB-88440C7CFDA6", "{0} must either be a valid country or a valid UNLOCODE", codeInfo.HumanReadableName));
							break;
					}
				}
				else
				{
					if (IsMandatoryZG_AgreedPlaceCode)
					{
						AddMessageErrorForRequiredZG_AgreedPlaceCode();
					}
				}
			}
		}

		protected virtual void AddMessageErrorOrWarningForMismatch()
		{
			Parent.ZG_AgreedPlaceCodeInfo.AddMessageError(DeclarationValidationConstants.IncotermPlaceCodeMismatch);
		}

		protected virtual void AddMessageErrorForRequiredZG_AgreedPlaceCode()
		{
			Parent.ZG_AgreedPlaceCodeInfo.AddMessageError(DeclarationValidationConstants.RequiredAgreedPlaceCode);
		}

		public bool IsMandatoryZG_AgreedPlaceCode => IsMandatoryZG_AgreedPlaceCodeCore;

		protected virtual bool IsMandatoryZG_AgreedPlaceCodeCore => true;

		protected override void CheckZG_TransportChargesMethodOfPayment()
		{
			base.CheckZG_TransportChargesMethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_TransportChargesMethodOfPaymentInfo);
		}

		protected override void CheckZG_RelatedIndicator2()
		{
			base.CheckZG_RelatedIndicator2();
			var invoice = Parent.Parent;
			invoice.Validation.CheckRuleC0624_InvoiceHeader(invoice, invoice.RelatedIndicator2Info);
		}

		protected override void CheckZG_RelatedIndicator3()
		{
			base.CheckZG_RelatedIndicator3();
			var invoice = Parent.Parent;
			invoice.Validation.CheckRuleC0624_InvoiceHeader(invoice, invoice.RelatedIndicator3Info);
		}

		protected override void CheckZG_RelatedIndicator4()
		{
			base.CheckZG_RelatedIndicator4();
			var invoice = Parent.Parent;
			invoice.Validation.CheckRuleC0624_InvoiceHeader(invoice, invoice.RelatedIndicator4Info);
		}
	}
}
