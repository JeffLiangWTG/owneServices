using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class DeltaIEJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		public DeltaIEJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_OA_ExporterAddress()
		{
			base.CheckJI_OA_ExporterAddress();

			CheckRuleR0520(Parent.ExporterAddress, Parent.JI_OA_ExporterAddressInfo);
		}

		protected override void CheckJI_OA_ConsigneeAddress()
		{
			base.CheckJI_OA_ConsigneeAddress();

			CheckRuleR0520(Parent.ConsigneeAddress, Parent.JI_OA_ConsigneeAddressInfo);
		}

		public void ValidateBuyerDocAddress(JobDocAddressValidation validation)
		{
			CheckRuleR0520(Parent.BuyerDocAddress, validation.Parent.OrganisationPKInfo);
		}

		public void ValidateSellerDocAddress(JobDocAddressValidation validation)
		{
			CheckRuleR0520(Parent.SellerDocAddress, validation.Parent.OrganisationPKInfo);
		}

		void CheckRuleR0520(OrgAddress address, ZPropertyInfo propertyInfo)
		{
			DeltaIEDeclarationValidationHelper.CheckEORIOrFullAddress(propertyInfo, address);
		}

		void CheckRuleR0520(JobDocAddress address, ZPropertyInfo propertyInfo)
		{
			DeltaIEDeclarationValidationHelper.CheckEORIOrFullAddress(propertyInfo, address);
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			CheckRuleNat_044(Parent.JI_ProcedureInfo);
		}

		void CheckRuleNat_044(ZPropertyInfo propertyInfo)
		{
			var parent = Parent;
			if (parent.JI_Calc_Concession == RefCusProcedure.Concession.C08)
			{
				var additionalInfos = parent.AdditionalInfos.Cast<AdditionalInfo>().Union(parent.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>());
				var declaration = parent.InvoiceHeader.JobDeclaration;
				if (declaration != null)
				{
					additionalInfos = additionalInfos.Union(declaration.AdditionalInfos.Cast<AdditionalInfo>());
				}

				if (!additionalInfos.Any(x => x.CSI_Code == RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance))
				{
					propertyInfo.AddMessageError(Res.GetString("94D129BD-C8ED-43BF-B073-A84883A6B2F8", "You have entered the supplementary scheme code C08, so the special mention G0008 \"unidentified VAT debtor in France\" must be served at GS level."));
				}
			}
		}
	}
}
