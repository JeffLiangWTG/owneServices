using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobDeclarationValidation : JobDeclarationValidation
	{
		public DeltaIEJobDeclarationValidation(JobDeclaration parent) : base(parent)
		{
		}

		public override void ValidateSupplierDocumentaryAddress(JobDocAddressValidation validation)
		{
			base.ValidateSupplierDocumentaryAddress(validation);

			CheckRuleR0520(Parent.SupplierDocumentaryAddress, validation.Parent.OrganisationPKInfo);
		}

		public override void ValidateImporterDocumentaryAddress(JobDocAddressValidation validation)
		{
			base.ValidateImporterDocumentaryAddress(validation);

			CheckRuleR0520(Parent.ImporterDocumentaryAddress, validation.Parent.OrganisationPKInfo);
		}

		protected void CheckJE_DeltaMode()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_DeltaModeInfo);
		}

		protected override void CheckJE_OA_DeclarantAddress()
		{
			base.CheckJE_OA_DeclarantAddress();

			if (DeclarantEoriDiffersFromRepresentativeEORI())
			{
				Parent.JE_OA_DeclarantAddressInfo.AddMessageError(declarantVsRepresentativeCheckError);
			}

			CheckRuleR0520(Parent.DeclarantAddress, Parent.JE_OA_DeclarantAddressInfo);
		}

		protected override bool IsAccountDeltaModeMandatory => false;

		protected override void CheckJE_OA_Representative()
		{
			base.CheckJE_OA_Representative();

			if (DeclarantEoriDiffersFromRepresentativeEORI())
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(declarantVsRepresentativeCheckError);
			}

			CheckRuleR0520(Parent.Representative, Parent.JE_OA_RepresentativeInfo);
		}

		protected override void CheckJE_OH_Buyer()
		{
			base.CheckJE_OH_Buyer();

			if (Parent.Buyer != null)
			{
				CheckRuleR0520(Parent.Buyer.MainAddress, Parent.JE_OH_BuyerInfo);
			}
		}

		protected override void CheckJE_OA_SellerAddress()
		{
			base.CheckJE_OA_SellerAddress();

			CheckRuleR0520(Parent.SellerAddress, Parent.JE_OA_SellerAddressInfo);
		}

		bool DeclarantEoriDiffersFromRepresentativeEORI()
		{
			bool result = false;
			var declarant = Parent.Declarant;
			var representative = Parent.Representative;
			var declarantEORI = declarant?.GetEORI() ?? ZString.Empty;
			var representativeEORI = representative?.GetEORI() ?? ZString.Empty;
			if (!representativeEORI.IsEmpty && !declarantEORI.IsEmpty && declarantEORI == representativeEORI)
			{
				result = true;
			}
			return result;
		}

		void CheckRuleR0520(OrgAddress address, ZPropertyInfo propertyInfo)
		{
			DeltaIEDeclarationValidationHelper.CheckEORIOrFullAddress(propertyInfo, address);
		}

		void CheckRuleR0520(JobDocAddress address, ZPropertyInfo propertyInfo)
		{
			DeltaIEDeclarationValidationHelper.CheckEORIOrFullAddress(propertyInfo, address);
		}

		public static string declarantVsRepresentativeCheckError => Res.GetString("ACE86B43-86AE-431E-A67E-8CAA17DBBF5E", "Declarant and Representative can’t be the same, please check their EORI numbers");

		protected override void CheckJE_GoodsOrigin()
		{
			base.CheckJE_GoodsOrigin();

			var declaration = Parent;
			var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();

			if (!declaration.JE_GoodsOrigin.IsEmpty && invoiceLines.Any(x => !x.ZG_CountryOfDispatch.IsEmpty) && invoiceLines.Any(x => x.ZG_CountryOfDispatch.IsEmpty))
			{
				declaration.JE_GoodsOriginInfo.AddWarning(Res.GetString("23BE1230-DE13-4BD8-8730-1D5BEFE6BE7F", "Invoice lines without value in Country of Dispatch will be mapped from the Declaration tab."));
			}

			CheckRuleR0012AndC0002(declaration.JE_GoodsOriginInfo);
		}

		protected override void CheckJE_GoodsDestination()
		{
			base.CheckJE_GoodsDestination();

			CheckRuleR0012AndC0002(Parent.JE_GoodsDestinationInfo);
		}

		protected override void CheckJE_DefermentAccountNumberLength()
		{
			if (Parent.IsImport)
			{
				var accountNumber = Parent.JE_DefermentAccountNumber;
				if (!Regex.IsMatch(accountNumber, "^[0-9]{2}D[NA][A-Z]{4}[0-9]{9}$"))
				{
					Parent.JE_DefermentAccountNumberInfo.AddMessageError(Res.GetString("9100C55B-EFCF-455F-92A9-4F13F663A495", "The value should be i) starting with two numbers, ii) followed by 'D', iii) followed by 'N' or 'A', iv) followed by four alphabets, v) ends with exactly nine digits."));
				}
			}
		}

		void CheckRuleR0012AndC0002(ZPropertyInfo propertyInfo)
		{
			var declaration = Parent;
			var invoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>();

			if (propertyInfo == declaration.JE_GoodsOriginInfo && declaration.JE_GoodsOrigin.IsEmpty && invoiceLines.Any(x => !x.ZG_CountryOfDispatch.IsEmpty) && invoiceLines.Any(x => x.ZG_CountryOfDispatch.IsEmpty))
			{
				propertyInfo.AddMessageError(Res.GetString("90E510E8-6CEC-4106-9689-A1EDF2EC9F2E", "[R0012 & C0002] If Country of Dispatch is specified in one of Invoice lines then it must be specified for all invoice lines or in Declaration tab."));
			}
			else if (propertyInfo == declaration.JE_GoodsDestinationInfo && declaration.JE_GoodsDestination.IsEmpty && invoiceLines.Any(x => !x.ZG_CountryOfDestination.IsEmpty) && invoiceLines.Any(x => x.ZG_CountryOfDestination.IsEmpty))
			{
				propertyInfo.AddMessageError(Res.GetString("F6F14576-34D9-4EBC-9A9C-040F979CA8BE", "[R0012 & C0002] If Country of Destination is specified in one of Invoice lines then it must be specified for all invoice lines or in Declaration tab."));
			}
		}

		protected override void CheckChargePaymentOrDestinationID()
		{
			base.CheckChargePaymentOrDestinationID();

			var parent = Parent;
			if (!parent.ChargePaymentOrDestinationID.IsEmpty)
			{
				var portCodeAdditionalReference = DeltaIEDeclarationValidationHelper.GetPortCodeAdditionalReference(parent.AdditionalInfos);

				if (portCodeAdditionalReference == null || portCodeAdditionalReference.CSI_ReferenceNumber != parent.ChargePaymentOrDestinationID)
				{
					parent.ChargePaymentOrDestinationIDInfo.AddMessageError(Res.GetString("79619B46-CFC0-4681-839E-422EF53A1705", "You must include a 1CPT Additional Reference in Additional Documents Tab to declare the port code."));
				}
			}
		}

		protected override void CheckJE_CustomsGuaranteeNumber()
		{
			var parent = Parent;
			if (parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(inv => inv.HasGuaranteeConsumingProcedure))
			{
				if (parent.JE_CustomsGuaranteeNumber.IsEmpty || parent.CustomsGuarantee == null || parent.CustomsGuarantee.GetApplicationSpecificReference(GuaranteeTypeList.Codes.COD).IsEmpty)
				{
					var message = Res.GetString("D0E87498-C47D-48E9-B7AB-CD80D9AC2570", "No guarantee was set, although one is required. Check that the {0} organization or the Declarant owns a valid guarantee of type COD", parent.IsImport ? Res.GetString("72fa58b9-cfa6-4839-9d2a-ae36869479ab", "Importer") : Res.GetString("5e36f4eb-63bf-47d8-bf31-7636217139e5", "Supplier"));
					parent.JE_CustomsGuaranteeNumberInfo.AddMessageError(message + AdditionalSpecificErrorMessageForNeededACODGuarantee + ".");
				}
			}
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();
			var declaration = Parent;
			if (declaration.JE_CustomsProfile.IsEmpty && declaration.Lookups.ProfileList.Count > 0)
			{
				declaration.JE_CustomsProfileInfo.AddMessageError(Res.GetString("A87B9E3C-F149-4FB9-B5D9-DE6451C811AF", "There is more than one convenient account found. Please select one manually."));
			}
		}
	}
}
