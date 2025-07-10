//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGACPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoGACPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GACPGAHeaderAddInfoValidation : AutoGACPGAHeaderAddInfoValidation
	{
		public GACPGAHeaderAddInfoValidation(AutoGACPGAHeaderAddInfo parent) : base(parent)
		{
		}

		GACPGAHeader PGAHeader => Parent.Parent as GACPGAHeader;

		protected override void CheckCA_FTACode()
		{
			base.CheckCA_FTACode();

			if (PGAHeader != null && PGAHeader.AreClothingAndTextileDetailsVisibility && PGAHeader.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CA_FTACodeInfo, Parent.Lookups.FTAProcessingCodes);
			}
		}

		protected override void CheckCA_FibreCountryOfOrigin()
		{
			base.CheckCA_FibreCountryOfOrigin();
			ValidateCodeWhenClothingAndTextileDetailsAreVisibility(Parent.CA_FibreCountryOfOriginInfo, Parent.Lookups.FibreOrigins);
		}

		protected override void CheckCA_YarnCountryOfOrigin()
		{
			base.CheckCA_YarnCountryOfOrigin();
			ValidateCodeWhenClothingAndTextileDetailsAreVisibility(Parent.CA_YarnCountryOfOriginInfo, Parent.Lookups.YarnOrigins);
		}

		protected override void CheckCA_FabricCountryOfOrigin()
		{
			base.CheckCA_FabricCountryOfOrigin();
			ValidateCodeWhenClothingAndTextileDetailsAreVisibility(Parent.CA_FabricCountryOfOriginInfo, Parent.Lookups.FabricOrigins);
		}

		void ValidateCodeWhenClothingAndTextileDetailsAreVisibility(ZPropertyInfo info, IBusinessObjectCollection collection)
		{
			if (PGAHeader != null && PGAHeader.AreClothingAndTextileDetailsVisibility && PGAHeader.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes && !(PGAHeader.IsFTAProcessingCodeFA01 && info.Value.ToString() == GACPGAHeader.The3rdPartyCountry))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
				ListValidation.MessageErrorIfInvalidCode(info, collection);
			}
		}

		protected override void CheckCA_ComplianceStatement()
		{
			base.CheckCA_ComplianceStatement();

			if (!Parent.CA_ComplianceStatement && PGAHeader.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes
				&& PGAHeader.LPCOViews.Cast<LPCOView>().Any(x => x.CLP_Type == LPCODocumentTypeQualifier.Codes._2001 || x.CLP_Type == LPCODocumentTypeQualifier.Codes._2003))
			{
				PGAHeader.CA_ComplianceStatementInfo.AddMessageError(Res.GetString("95d37c4a-247b-4653-aa67-a367c69afb42", "Certify should be ticked."));
			}
		}

		protected override void CheckCA_CommodityCode()
		{
			base.CheckCA_CommodityCode();

			var isPermitApplication = PGAHeader?.InvoiceLine?.Declaration?.CA_PermitApplication;

			if (isPermitApplication != null && isPermitApplication.Value && !Regex.IsMatch(Parent.CA_CommodityCode, @"\d{14}")
				&& PGAHeader != null && PGAHeader.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				Parent.CA_CommodityCodeInfo.AddMessageError(CommodityCodeMustBe14Digit);
			}
		}

		internal static string CommodityCodeMustBe14Digit
		{
			get { return Res.GetString("7C250319-8AEF-4CAD-8200-3FE3A84A6A45", "The commodity code must be 14-digit code."); }
		}
	}
}
