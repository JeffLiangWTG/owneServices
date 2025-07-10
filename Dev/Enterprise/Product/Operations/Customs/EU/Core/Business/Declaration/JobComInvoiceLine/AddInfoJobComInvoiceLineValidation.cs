using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.EU.Business.EUCommonConstants;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EUAddInfoValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected override void CheckZG_FecDST()
		{
			base.CheckZG_FecDST();
			AlertUserToMakeAChangeToFieldValueOrToTickFecBoxInOrderToClearFecChallenge(Parent.ZG_FecChallengeDST, Parent.ZG_FecDSTInfo);
		}

		void AlertUserToMakeAChangeToFieldValueOrToTickFecBoxInOrderToClearFecChallenge(bool fecChallenge, ZPropertyInfo propertyInfo)
		{
			if (fecChallenge)
			{
				propertyInfo.AddMessageError(Res.GetString("09005F29-043C-4C65-85D1-25BF53F79DE5", "A FEC challenge has been received from Customs for this field. Please change the field's value, override the challenge by ticking the FEC box, or tick 'request route F' on the Misc tab."));
			}
		}

		protected new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		protected AddInfoJobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override void CheckZG_TransNature()
		{
			base.CheckZG_TransNature();

			CheckRuleC0627();
		}

		void CheckRuleC0627()
		{
			var parent = Parent;
			if ((InvoiceLineValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0627Active) && !parent.ZG_TransNature.IsEmpty && (parent.Parent.EntryInstruction?.IsSimplifiedOrPreliminaryUnderCodeC ?? false))
			{
				parent.ZG_TransNatureInfo.AddMessageError(Res.GetString("1603636C-27E7-4BFC-BAA3-7E420817F3A6", "[C0627] This field must be empty in case of Declaration Sub Type C or F."));
			}
		}

		protected override void CheckZG_RL_NKPrincipalsRepresentativeCity()
		{
			base.CheckZG_RL_NKPrincipalsRepresentativeCity();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_RL_NKPrincipalsRepresentativeCityInfo);
		}

		protected override void CheckZG_SecondQuota()
		{
			if (Parent.Parent.IsImport)
			{
				var len = Parent.ZG_SecondQuota.Length;
				if (len > 0 && len != 6)
				{
					Parent.ZG_SecondQuotaInfo.AddMessageError(Res.GetString("B8EEABC1-AE89-4C98-8E86-FC5007B89191", "Quota number should be 6 characters long."));
				}
			}
		}

		protected override void CheckZG_CountryOfDestination()
		{
			base.CheckZG_CountryOfDestination();

			var declaration = Parent.Parent.Declaration;
			if (declaration != null && ((declaration.IsImport && declaration.Configuration.InvoiceLineConfiguration.CountryOfDestinationVisibleOnImportControl(declaration)) || (declaration.IsExport && declaration.Configuration.InvoiceLineConfiguration.CountryOfDestinationVisibleOnExportControl(declaration))))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CountryOfDestinationInfo);
			}

			CheckRuleC0002();
		}

		protected override void CheckZG_CommercialReference()
		{
			base.CheckZG_CommercialReference();
			if (!Parent.ZG_CommercialReference.IsEmpty)
			{
				var parentAsInvoice = Parent.Parent;
				if (parentAsInvoice != null && parentAsInvoice.Declaration != null && !parentAsInvoice.Declaration.JE_OwnerRef.IsEmpty)
				{
					Parent.ZG_CommercialReferenceInfo.AddMessageError(Res.GetString("70C1EA2E-9A16-457F-A5DB-BD3C03076BEA", "Commercial Reference Number at Invoice Line level should not be declared if [7] Declarant’s Ref is declared"));
				}
			}
		}

		protected override void CheckZG_CountryOfSupply()
		{
			base.CheckZG_CountryOfSupply();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CountryOfSupplyInfo);
			var parent = Parent;
			var invoiceLine = parent.Parent;

			if (InvoiceLineValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0710ActiveForJI_CountryOfOrigin)
			{
				JobComInvoiceLineValidation.CheckRuleC0710(parent.ZG_CountryOfSupplyInfo, invoiceLine);
			}

			CheckRuleCD5151(parent.Parent);
		}

		void CheckRuleCD5151(JobComInvoiceLine invoiceLine)
		{
			var parent = Parent;
			if ((InvoiceLineValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleCD5151ActiveForZG_CountryOfSupply) && invoiceLine.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				var style = entryInstruction.CEI_Style.ToUpperInvariant();
				if (RequiresCountryOfSupply(style))
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.ZG_CountryOfSupplyInfo);
				}

				if (style == ImportDeclarationTypeList.I1)
				{
					if (parent.ZG_CountryOfSupply.IsEmpty && IsPrimaryRefWeCareAbout(invoiceLine.JI_PrimaryPreference))
					{
						parent.ZG_CountryOfSupplyInfo.AddMessageError(Res.GetString("A5F0B65C-B123-45BA-A6FB-8CB1F2BE151C", "[CD5151] {0} is required when the first digit of {1} is '1', '4' or '5' and is not equal to Pref. Orig.", parent.ZG_CountryOfSupplyInfo.Description, invoiceLine.JI_PrimaryPreferenceInfo.Description));
					}
					else if (!invoiceLine.JI_CountryOfOrigin.IsEmpty && !parent.ZG_CountryOfSupply.EqualsIgnoringCase(invoiceLine.JI_CountryOfOrigin))
					{
						parent.ZG_CountryOfSupplyInfo.AddMessageError(Res.GetString("E427F999-2F72-452E-ADE4-35520F7AE73C", "[CD5151] {0} must be equal to {1} for Import and {2} of 'I1'.", parent.ZG_CountryOfSupplyInfo.Description, invoiceLine.JI_CountryOfOriginInfo.Description, entryInstruction.CEI_StyleInfo.Description));
					}
				}
			}
		}

		bool IsPrimaryRefWeCareAbout(ZString primaryRef)
		{
			return primaryRef.StartsWith("1")
			|| primaryRef.StartsWith("4")
			|| primaryRef.StartsWith("5");
		}

		bool RequiresCountryOfSupply(string style)
		{
			return style == ImportDeclarationTypeList.H1
				|| style == ImportDeclarationTypeList.H2
				|| style == ImportDeclarationTypeList.H3
				|| style == ImportDeclarationTypeList.H4
				|| style == ImportDeclarationTypeList.H5;
		}

		protected override void CheckZG_CusNumber()
		{
			base.CheckZG_CusNumber();
			var cusNumber = Parent.ZG_CusNumber;
			var propertyInfo = Parent.ZG_CusNumberInfo;

			ListValidation.MessageErrorIfInvalidCode(propertyInfo);

			if (!cusNumber.IsEmpty && cusNumber.Length != 9)
			{
				propertyInfo.AddMessageError(Res.GetString("D9D31CF6-FAD3-4098-BB5C-EFB16B2F57AB", "The number should be 9 characters."));
			}
		}

		protected override void CheckZG_RelatedIndicator2()
		{
			base.CheckZG_RelatedIndicator2();
			var invoiceLine = Parent.Parent;
			invoiceLine.Validation.CheckRuleC0624_InvoiceLine(invoiceLine, invoiceLine.RelatedIndicator2Info);
		}

		protected override void CheckZG_RelatedIndicator3()
		{
			base.CheckZG_RelatedIndicator3();
			var invoiceLine = Parent.Parent;
			invoiceLine.Validation.CheckRuleC0624_InvoiceLine(invoiceLine, invoiceLine.RelatedIndicator3Info);
		}

		protected override void CheckZG_RelatedIndicator4()
		{
			base.CheckZG_RelatedIndicator4();
			var invoiceLine = Parent.Parent;
			invoiceLine.Validation.CheckRuleC0624_InvoiceLine(invoiceLine, invoiceLine.RelatedIndicator4Info);
		}

		protected override void CheckZG_CountryOfDispatch()
		{
			base.CheckZG_CountryOfDispatch();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CountryOfDispatchInfo);
		}

		void CheckRuleC0002()
		{
			var parent = Parent;
			var entryInstruction = parent.Parent.EntryInstruction;

			if ((InvoiceLineValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0002Active)
				&& (parent.ZG_CountryOfDestination.IsEmpty && entryInstruction != null && entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.ZG_CountryOfDestination.IsEmpty)))
			{
				parent.ZG_CountryOfDestinationInfo.AddMessageError(Res.GetString("70BE7B5F-2A6C-46D9-83FF-53A8B5302E2B", "[C0002] In case data entered in Invoice line level, all related lines must have a value in this field."));
			}
		}

		IInvoiceLineValidationDecider InvoiceLineValidationDecider => Parent.Parent.Validation.ValidationDecider;
	}
}
