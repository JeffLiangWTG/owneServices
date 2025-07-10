using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public DeltaIEJobComInvoiceHeaderValidation(JobComInvoiceHeader parent) : base(parent)
		{
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_RX_NKInvoice_CurrencyInfo);
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();

			CheckRuleR0520(Parent.SupplierAddress, Parent.JZ_OA_SupplierAddressInfo);
		}

		protected override void CheckJZ_OA_BuyerAddress()
		{
			base.CheckJZ_OA_BuyerAddress();

			CheckRuleR0520(Parent.BuyerAddress, Parent.JZ_OA_BuyerAddressInfo);
		}

		protected override void CheckJZ_OA_SellerAddress()
		{
			base.CheckJZ_OA_SellerAddress();

			CheckRuleR0520(Parent.SellerAddress, Parent.JZ_OA_SellerAddressInfo);
		}

		void CheckRuleR0520(OrgAddress address, ZPropertyInfo propertyInfo)
		{
			DeltaIEDeclarationValidationHelper.CheckEORIOrFullAddress(propertyInfo, address);
		}

		protected override void CheckJZ_AdditionalTerms()
		{
			base.CheckJZ_AdditionalTerms();

			if (IncotermShouldHaveAdditionalTerms)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_AdditionalTermsInfo);
			}
		}

		protected override ZBool ShouldCheckMissingPreviousDocuments => false;

		ZBool IncotermShouldHaveAdditionalTerms => Parent.JZ_IncoTerm == Core.Constants.IncoTerms.Other;
		protected override ZBool RequireJZ_IncoTermPlaceMandatory => Parent.JZ_IncoTerm != Core.Constants.IncoTerms.Other && Parent.ZG_AgreedPlaceCode.IsEmpty;
	}
}
