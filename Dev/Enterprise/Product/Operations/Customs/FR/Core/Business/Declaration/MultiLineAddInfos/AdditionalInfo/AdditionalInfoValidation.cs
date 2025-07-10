using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			CheckRuleC0834_N01(Parent.CSI_CodeInfo);
			CheckRuleNAT_088Bis();
			CheckRuleNAT_041();
		}

		void CheckRuleC0834_N01(ZPropertyInfo propertyInfo)
		{
			var additionalInfo = Parent;

			if (additionalInfo.Parent is IAdditionalInfosProviderWithValidationDecider supporter && supporter.ValidationDecider is IAdditionalInfoValidationDecider { IsRuleC0834_N01Active: true } && additionalInfo.CSI_Code == RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance)
			{
				var parent = additionalInfo.Parent;
				if ((parent is JobComInvoiceLine invoiceLine && invoiceLine.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48)
					|| (parent is JobComInvoiceHeader invoiceHeader && invoiceHeader.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48))
					|| (parent is JobDeclaration declaration && declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48))
					|| (parent is CusEntryInstruction cusEntryInstruction && cusEntryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_Calc_Concession == UniversalReferenceConstants.RefCusProcedure.Concession.F48)))
				{
					propertyInfo.AddMessageError(Res.GetString("39E7E482-7552-4F46-865A-36AA88E7764E", "[C0834_N01] You have requested the concession F48, you cannot enter the special mention \"G0008 unidentified VAT person in France\"."));
				}
			}
		}

		void CheckRuleNAT_088Bis()
		{
			if (Parent.Parent is IAdditionalInfosProviderWithValidationDecider provider
				&& provider.ValidationDecider is IAdditionalInfoValidationDecider { IsRuleNAT_088BisActive: true }
				&& Parent.CSI_Code == RefCusCodeList.AdditionalInformationCodes.StandardDeclaration
				&& Parent.Parent is not JobDeclaration)
			{
				Parent.CSI_CodeInfo.AddMessageError(Res.GetString("93DDB5F1-5478-4E90-B037-425B6FA0D3A7", "[NAT_088_Bis] E0001 can be entered only at declaration level, in Misc. tab."));
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			CheckRuleNAT_228();
		}

		void CheckRuleNAT_228()
		{
			if (Parent.Parent is IAdditionalInfosProviderWithValidationDecider provider
				&& provider.ValidationDecider is IAdditionalInfoValidationDecider { IsRuleNAT_228Active: true }
				&& Parent.CSI_Code == RefCusCodeList.AdditionalInformationCodes.TariffBypassNeedingMotivation
				&& Parent.CSI_Description.IsEmpty)
			{
				Parent.CSI_DescriptionInfo.AddMessageError(Res.GetString("8D52929E-1543-465B-A065-FB9D998DF0E3", "[NAT_228] If code is K0001 description is mandatory."));
			}
		}

		void CheckRuleNAT_041()
		{
			var additionalInfo = Parent;

			if (additionalInfo.Parent is IAdditionalInfosProviderWithValidationDecider supporter && supporter.ValidationDecider is IAdditionalInfoValidationDecider { IsRuleNAT_041Active: true } && additionalInfo.Declaration.IsUCC6AndIsImport && additionalInfo.Declaration.ZG_VATDeferType == VATProcedureList.Codes._2 && (additionalInfo.CSI_Code == RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption || additionalInfo.CSI_Code == RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance))
			{
				var errorMsg = Res.GetString("A4D1F5E0-3B7C-4F8A-9E6C-0D3B2F1A5E7D", "[NAT_041] Both additional information G6090 and G0008 cannot be present in the same declaration.");
				ValidateMutualExclusion(additionalInfo, RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance, RefCusCodeList.AdditionalInformationCodes.AI2WithVisaExemption, errorMsg);
			}
		}

		void ValidateMutualExclusion(AdditionalInfo currentInfo, string conflictingCode1, string conflictingCode2, string errorMsg)
		{
			var parent = currentInfo.Parent;
			var declaration = parent switch
			{
				JobComInvoiceLine line => line.Declaration,
				JobComInvoiceHeader header => header.JobDeclaration,
				JobDeclaration decl => decl,
				_ => null
			};
			if (declaration != null)
			{
				var declInfos = declaration.AdditionalInfos?.Cast<AdditionalInfo>() ?? Enumerable.Empty<AdditionalInfo>();
				var headerInfos = declaration.Invoices?.Cast<JobComInvoiceHeader>().SelectMany(inv => inv.AdditionalInfos.Cast<AdditionalInfo>()) ?? Enumerable.Empty<AdditionalInfo>();
				var lineInfos = declaration.InvoiceLines?.Cast<JobComInvoiceLine>().SelectMany(li => li.AdditionalInfos.Cast<AdditionalInfo>()) ?? Enumerable.Empty<AdditionalInfo>();

				var allInfos = declInfos.Concat(headerInfos).Concat(lineInfos);

				if (allInfos.Any(ai => ai.CSI_Code == conflictingCode1) && allInfos.Any(ai => ai.CSI_Code == conflictingCode2))
				{
					currentInfo.CSI_CodeInfo.AddMessageError(errorMsg);
				}
			}
		}
	}
}
