using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.FR.Business.FRConstants;
using static Enterprise.Customs.FR.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class SupplementaryCodeValidation : EU.Business.SupplementaryCodeValidation
	{
		public SupplementaryCodeValidation(BaseSupplementaryCode parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var parent = Parent;
			var invoiceLine = parent.CY_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? parent.Parent as JobComInvoiceLine : null;

			if (invoiceLine?.Declaration?.IsUCC6 ?? false)
			{
				if (parent.CY_Order == 2 && parent.CY_Code == SupplementaryCodes.PromotionalProductToDROMSupplementaryCode)
				{
					var additionalInfos = invoiceLine.AdditionalInfos.Cast<AdditionalInfo>().Union(invoiceLine.InvoiceHeader.AdditionalInfos.Cast<AdditionalInfo>());
					var declaration = invoiceLine.InvoiceHeader.JobDeclaration;
					if (declaration != null)
					{
						additionalInfos = additionalInfos.Union(declaration.AdditionalInfos.Cast<AdditionalInfo>());
					}

					if (!additionalInfos.Any(x => x.CSI_Code == RefCusCodeList.AdditionalInformationCodes.UnidentifiedVATLiableInFrance))
					{
						parent.CY_CodeInfo.AddMessageError(Res.GetString("7D0E6496-417D-4309-95D4-8F90882AA484", "You have applied for CANA 0090, so the special mention \"G0008 unidentified VAT payable in France\" must be served at GS level."));
					}
				}
			}
		}
	}
}
