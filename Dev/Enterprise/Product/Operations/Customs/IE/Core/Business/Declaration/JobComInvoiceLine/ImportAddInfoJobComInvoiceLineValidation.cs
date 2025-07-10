using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
	{
		public ImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_CountryOfDispatch()
		{
			base.CheckZG_CountryOfDispatch();
			if (Parent.ZG_CountryOfDispatch == Core.Constants.CountryCodes.Ireland)
			{
				Parent.ZG_CountryOfDispatchInfo.AddMessageError(Res.GetString("4208415D-DD0C-457E-8492-640876A79CBB", "[BR0514] Country of Dispatch cannot be IE when Message Type is IMP."));
			}
		}

		protected override void CheckZG_CountryOfSupply()
		{
			base.CheckZG_CountryOfSupply();

			var parent = Parent;
			CheckRuleCD5161(parent.ZG_CountryOfSupplyInfo, parent.JI_PrimaryPreference);
		}

		void CheckRuleCD5161(ZPropertyInfo countryOfSupplyInfo, ZString primaryReference)
		{
			if ((InvoiceLineValidationDecider is IIEInvoiceLineValidationDecider decider && decider.IsRuleCD5161ActiveForZG_CountryOfSupply) && countryOfSupplyInfo.Value.IsEmpty && IsPreferencialOriginRequired(primaryReference))
			{
				countryOfSupplyInfo.AddMessageError(Res.GetString("BC5969AD-D1EE-4FA8-BB5C-568200FD4B99", "[CD5161] Preferential Origin (Pref. Orig.) is required when the first digit of Preference is ‘2’, ‘3’, ‘4’ or ‘5’."));
			}
		}

		bool IsPreferencialOriginRequired(ZString primaryReference)
		{
			return primaryReference.StartsWith("2") || primaryReference.StartsWith("3") || primaryReference.StartsWith("4") || primaryReference.StartsWith("5");
		}

		protected override void CheckZG_CountryOfDestination()
		{
			base.CheckZG_CountryOfDestination();
			var parent = Parent;
			if (parent.EntryInstruction is CusEntryInstruction instruction)
			{
				if (parent.ZG_CountryOfDestination != Core.Constants.CountryCodes.Ireland &&
					ImportDeclarationTypeList.DeclarationTypeListBR4013.Contains(instruction.CEI_Style.ToUpperInvariant()) &&
					!parent.Charges.Any(x => IsValidForBR4013(x.J7_ChargeType)) &&
					!parent.ApportionedCharges.Any(x => IsValidForBR4013(x.J7_ChargeType)))
				{
					Parent.ZG_CountryOfDestinationInfo.AddMessageError(Res.GetString("B07965E3-A7FF-4801-80D6-46AF6D741786", "[BR4013] Please enter Charge 'AK' or 'AD' under Invoice Header > Invoice Charges when Destination is outside IE."));
				}
				if (IsValidToCheckForCountryOfDestination(instruction.CEI_Style))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_CountryOfDestinationInfo, (NoResString)"Country of Destination");
				}
			}
		}

		bool IsValidToCheckForCountryOfDestination(ZString cei_style)
		{
			return cei_style == ImportDeclarationTypeList.Codes.H1
				|| cei_style == ImportDeclarationTypeList.Codes.H2
				|| cei_style == ImportDeclarationTypeList.Codes.H3
				|| cei_style == ImportDeclarationTypeList.Codes.H4
				|| cei_style == ImportDeclarationTypeList.Codes.H5;
		}

		bool IsValidForBR4013(ZString chargeType)
		{
			switch (chargeType)
			{
				case AISChargeCodeList.Codes.AD:
				case AISChargeCodeList.Codes.AK:
					return true;
				default:
					return false;
			}
		}

		IInvoiceLineValidationDecider InvoiceLineValidationDecider => Parent.Validation.ValidationDecider;
	}
}
