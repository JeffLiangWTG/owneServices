//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRFPNumberAddInfoValidation
//
//    This class should be used for overriding validation in AutoRFPNumberAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPNumberAddInfoValidation : AutoRFPNumberAddInfoValidation
	{
		public RFPNumberAddInfoValidation(AutoRFPNumberAddInfo parent)
			: base(parent)
		{
		}

		#region CheckZA_RFPNumber

		protected override void CheckZA_RFPNumber()
		{
			base.CheckZA_RFPNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_RFPNumberInfo);
		}

		#endregion

		#region CheckZA_RFPLine

		protected override void CheckZA_RFPLine()
		{
			base.CheckZA_RFPLine();
			const string description = "RFP Line Number";
			MandatoryValidation.MessageErrorIfIsNegative(Parent.ZA_RFPLineInfo, description);
			MandatoryValidation.MessageErrorIfIsZero(Parent.ZA_RFPLineInfo, description);
		}

		#endregion

		#region CheckZA_RFPNetQuantity

		protected override void CheckZA_RFPNetQuantity()
		{
			base.CheckZA_RFPNetQuantity();
			const string description = "Net Quantity";
			MandatoryValidation.MessageErrorIfIsNegative(Parent.ZA_RFPNetQuantityInfo, description);
			MandatoryValidation.MessageErrorIfIsZero(Parent.ZA_RFPNetQuantityInfo, description);
		}

		#endregion

		#region CheckZA_RFPQtyUM

		protected override void CheckZA_RFPQtyUM()
		{
			base.CheckZA_RFPQtyUM();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_RFPQtyUMInfo, Parent.Lookups.NetQuantityUnits);
		}

		#endregion

		#region CheckZA_RFPPackCount

		protected override void CheckZA_RFPPackCount()
		{
			base.CheckZA_RFPPackCount();
			const string description = "Pack Count";
			MandatoryValidation.MessageErrorIfIsNegative(Parent.ZA_RFPPackCountInfo, description);
			MandatoryValidation.MessageErrorIfIsZero(Parent.ZA_RFPPackCountInfo, description);
		}

		#endregion

		#region CheckZA_RFPPackType

		protected override void CheckZA_RFPPackType()
		{
			base.CheckZA_RFPPackType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZA_RFPPackTypeInfo, Parent.Lookups.PackageTypes);
		}

		#endregion
	}
}
