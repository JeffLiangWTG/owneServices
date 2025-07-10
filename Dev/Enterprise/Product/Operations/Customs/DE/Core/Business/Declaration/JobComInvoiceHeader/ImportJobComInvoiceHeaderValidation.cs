using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ImportJobComInvoiceHeaderValidation : JobComInvoiceHeaderValidation
	{
		public ImportJobComInvoiceHeaderValidation(JobComInvoiceHeader header)
			: base(header)
		{
		}

		protected override CargoWise.ComponentModel.INotificationType AtLeastOneSupportingDocumentMessageNotificationType => NotificationType.Warning;

		protected override bool IsJZ_ValuationCodeMandatory => Parent.CusEntryInstructions.Any(x => x.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.EZA, ImportDeclarationTypeList.Codes.EAV })
			|| ImportDeclarationTypeList.IsSimplifiedFreeCirculation(x.CEI_Style)
			|| ImportDeclarationTypeList.IsSimplifiedInwardProcessing(x.CEI_Style));

		protected override ZBool RequireJZ_IncoTermPlaceMandatory => ZBool.True;

		protected override void CheckSellerOrgPK()
		{
			base.CheckSellerOrgPK();
			ListValidation.ErrorIfInvalidPK(Parent.SellerOrgPKInfo);
		}

		protected override void CheckBuyerOrgPK()
		{
			base.CheckBuyerOrgPK();
			ListValidation.ErrorIfInvalidPK(Parent.BuyerOrgPKInfo);
		}

		protected override bool ShouldValidateNeedAtLeastOneInvoiceSupportingDocument => !Parent.IsInwardProcessingAVABR && base.ShouldValidateNeedAtLeastOneInvoiceSupportingDocument;
	}
}
