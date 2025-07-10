using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineTraderJobDocAddressValidation : JobDocAddressValidation
	{
		public InvoiceLineTraderJobDocAddressValidation(JobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckOrganisationPK()
		{
			base.CheckOrganisationPK();

			var parentOrgPK = Parent.OrganisationPK;
			var parentOrgPKInfo = Parent.OrganisationPKInfo;

			var line = Parent.Parent as JobComInvoiceLine;
			bool isBuyer = Parent.DocAddressType == DocAddressType.BuyingParty;
			bool isSeller = Parent.DocAddressType == DocAddressType.SellingParty;
			var isBuyerOrSeller = isBuyer || isSeller;

			CheckRuleR0012(parentOrgPK, parentOrgPKInfo);
			CheckRuleC0728(parentOrgPK, parentOrgPKInfo);

			void CheckRuleC0728(ZGuid lineOrgPK, ZPropertyInfo lineOrgPKInfo)
			{
				if ((line.Validation.ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleC0728Active) && isBuyerOrSeller && lineOrgPK.IsValid && line.ShouldNotHaveTraders())
				{
					lineOrgPKInfo.AddMessageError(Res.GetString("F01A2BDC-575B-4726-B9F9-7BEC0D66190A", "[C0728] – This field must be empty for this Requested Procedure / Additional Declaration / Declaration Sub Type."));
				}
			}

			void CheckRuleR0012(ZGuid lineOrgPK, ZPropertyInfo lineOrgPKInfo)
			{
				if (isBuyerOrSeller && (line.Validation.ValidationDecider is IImportInvoiceLineValidationDecider importValidationDecider && importValidationDecider.IsRuleR0012Active))
				{
					var invoiceOrgPK = isBuyer ? line.InvoiceHeader.BuyerOrgPK : line.InvoiceHeader.SellerOrgPK;
					if (invoiceOrgPK.IsValid && lineOrgPK.IsValid)
					{
						lineOrgPKInfo.AddMessageError(Res.GetString("fa093c0d-88d7-4e1f-b9e8-788b38c32146", "[R0012] Value can't be entered in both invoice header and invoice lines"));
					}
				}
			}
		}
	}
}
