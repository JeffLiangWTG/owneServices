using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public partial class JobDeclarationValidation
	{
		JobDeclaration Declaration => Parent;

		protected override void CheckJE_Box18TransportNationality()
		{
			base.CheckJE_Box18TransportNationality();
			if (!Parent.ZG_Box18TransportNationality.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_Box18TransportNationalityInfo);
			}
		}

		protected override void CheckJE_ShipmentType()
		{
			base.CheckJE_ShipmentType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_ShipmentTypeInfo);
		}

		protected override void CheckJE_AgreedPlaceCode()
		{
			base.CheckJE_AgreedPlaceCode();

			var codeInfo = Parent.ZG_AgreedPlaceCodeInfo;
			var incoTermPlaceCode = Declaration.ZG_AgreedPlaceCode;

			if (Declaration.Configuration.IsIncoTermOnDeclarationRequiredToBeSameAsIncoTermOnInvoice)
			{
				if (Declaration.Invoices.Cast<JobComInvoiceHeader>().Any(x => x.ZG_AgreedPlaceCode != incoTermPlaceCode))
				{
					AddMessageErrorOrWarningForMismatch();
				}
			}

			if (Declaration.AgreedPlaceCodeSupport && Declaration.ZG_AgreedPlaceCodeValidationSupport)
			{
				if (!incoTermPlaceCode.IsEmpty)
				{
					switch (incoTermPlaceCode.Length)
					{
						case 5:
							ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("54F25DC6-7DE1-4031-AC0B-AA4D91E03A76", "{0} must be a valid UNLOCODE", codeInfo.HumanReadableName));
							break;
						case 2:
							ListValidation.MessageErrorIfInvalidCode(codeInfo, ResString.GetMultilingualString("A05EEC7A-3B35-4C54-892C-29AB86C6D0B0", "{0} must be a valid country", codeInfo.HumanReadableName));
							Declaration?.Validation.ValidateJE_ShipmentIncoTermPlace();
							break;
						default:
							codeInfo.AddMessageError(Res.GetString("48F82AE0-F865-4F23-B4FD-B2E62BF11E0F", "{0} must either be a valid country or a valid UNLOCODE", codeInfo.HumanReadableName));
							break;
					}
				}
				else
				{
					AddMessageErrorForRequiredZG_AgreedPlaceCode();
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(codeInfo);
			}
		}

		protected override void CheckJE_TypeOfSecurity()
		{
			base.CheckJE_TypeOfSecurity();
			CheckRuleC0211();
		}

		protected virtual void AddMessageErrorOrWarningForMismatch()
		{
			Parent.ZG_AgreedPlaceCodeInfo.AddMessageError(DeclarationValidationConstants.IncotermPlaceCodeMismatch);
		}

		protected virtual void AddMessageErrorForRequiredZG_AgreedPlaceCode()
		{
			Parent.ZG_AgreedPlaceCodeInfo.AddMessageError(DeclarationValidationConstants.RequiredAgreedPlaceCode);
		}

		protected override void CheckJE_MethodOfPayment()
		{
			base.CheckJE_MethodOfPayment();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_MethodOfPaymentInfo);
		}

		protected override void CheckJE_BorderTransportMeans()
		{
			base.CheckJE_BorderTransportMeans();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_BorderTransportMeansInfo);
		}

		protected override void CheckJE_Box18TransportID()
		{
			base.CheckJE_Box18TransportID();

			var parent = Parent;
			if (!Parent.JE_Box18TransportID.IsEmpty)
			{
				CheckRuleC0623(parent.JE_Box18TransportIDInfo);
				CheckRuleC0646(parent.JE_Box18TransportIDInfo);
			}
		}

		void CheckRuleC0211()
		{
			if (Declaration.Validation.ValidationDecider is IDeclarationValidationDecider validationDecider
				&& validationDecider.IsRuleC0211Active
				&& Parent.ZG_TypeOfSecurity == ExportSecurityTypeList.Codes.EXS && Declaration.ItineraryCountries.Count == 0)
			{
				Parent.ZG_TypeOfSecurityInfo.AddMessageError(Res.GetString("8690b495-3949-445b-ba62-939b192aa9fd", "[C0211] If security is 2 Itinerary countries are required."));
			}
		}
	}
}
