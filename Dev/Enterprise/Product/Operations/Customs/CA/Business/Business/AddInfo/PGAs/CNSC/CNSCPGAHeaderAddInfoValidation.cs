//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCNSCPGAHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoCNSCPGAHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class CNSCPGAHeaderAddInfoValidation : AutoCNSCPGAHeaderAddInfoValidation
	{
		public CNSCPGAHeaderAddInfoValidation(AutoCNSCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		CNSCPGAHeader CNSC
		{
			get { return (CNSCPGAHeader)((CNSCPGAHeaderAddInfo)Parent).Parent; }
		}

		protected override void CheckCA_Category()
		{
			base.CheckCA_Category();
			if (Parent.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_CategoryInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_CategoryInfo, Parent.Lookups.Categories);
			}
			if (CNSC != null && CNSC.IsEquipment)
			{
				ValidateCA_NNIECRSchePartNo();
			}
		}
		protected override void CheckCA_PackQty()
		{
			base.CheckCA_PackQty();
			if (CNSC.CA_Category == CNSCCategories.Codes.RD || CNSC.CA_Category == CNSCCategories.Codes.NS || CNSC.CA_Category == CNSCCategories.Codes.CNS)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CA_PackQtyInfo);
			}
		}

		protected override void CheckCA_NNIECRSchePartNo()
		{
			base.CheckCA_NNIECRSchePartNo();
			if (CNSC.IsEquipment)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_NNIECRSchePartNoInfo);
			}
		}

		protected override void CheckCA_PackUQ()
		{
			base.CheckCA_PackUQ();

			if (Parent.CA_PackQty > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_PackUQInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_PackUQInfo, Parent.Lookups.PackUQList);
		}
	}
}
