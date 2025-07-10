using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusClassPartPivotAddInfoValidation : AUAddInfoValidation
	{
		public CusClassPartPivotAddInfoValidation(CusClassPartPivotAddInfo parent)
			: base(parent)
		{
			dbValidationHelper = new ZZDBValidationHelper(parent.Factory);
		}

		readonly ZZDBValidationHelper dbValidationHelper;

		protected override void CheckZA_AQISProduceType_Hidden()
		{
			base.CheckZA_AQISProduceType_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISProduceType_HiddenInfo);
			}
		}

		protected override void CheckZA_AQISCategoryCode_Hidden()
		{
			base.CheckZA_AQISCategoryCode_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				if (Parent.ZA_AQISProduct_Hidden.IsEmpty)
				{
					if (!Parent.ZA_AQISCategoryCode_Hidden.IsEmpty)
					{
						Parent.ZA_AQISCategoryCode_HiddenInfo.AddMessageError(ProductTypeMustBeEnteredBeforeCategoryCode);
					}
				}
				else
				{
					var productType = Parent.ZA_AQISProduct_Hidden;

					var productTypeInDB = dbValidationHelper.GetProductTypeValues(Parent.ZA_AQISCategoryCode_Hidden, (ZString)EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(Parent.ZA_AQISProduceType_Hidden));
					if (!productTypeInDB.Any(x => x.EqualsIgnoringCase(productType)))
					{
						Parent.ZA_AQISCategoryCode_HiddenInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, CategoryShouldHaveProductTypeAttribute, productType));
					}
				}
			}
		}

		internal const string CategoryShouldHaveProductTypeAttribute = "The selected Category Code does not have the Product '{0}' as a reference attribute in the Category Code Product attributes field.";
		internal const string ProductTypeMustBeEnteredBeforeCategoryCode = "Product must be entered before a Category Code can be entered";

		protected override void CheckZA_AQISProduct_Hidden()
		{
			base.CheckZA_AQISProduct_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISProduct_HiddenInfo, ProductTypeMessage);
			}
		}

		internal static IMultilingualString ProductTypeMessage
		{
			get { return ResString.GetMultilingualString("E9FDEF92-7F59-4351-880D-E09C939BB92B", "This product cannot be selected because it is not of the correct commodity type."); }
		}

		protected override void CheckZA_AQISSupplementaryCode_Hidden()
		{
			base.CheckZA_AQISSupplementaryCode_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISSupplementaryCode_HiddenInfo);
			}
		}

		protected override void CheckZA_AQISPackType_Hidden()
		{
			base.CheckZA_AQISPackType_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISPackType_HiddenInfo);
			}
		}

		protected override void CheckZA_AQISPreservation_Hidden()
		{
			base.CheckZA_AQISPreservation_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				if (Parent.ZA_AQISPreservation_Hidden.IsEmpty && Parent.ZA_AQISProduceType_Hidden == EXDOCCommodityCodes.Codes.Fish)
				{
					Parent.ZA_AQISPreservation_HiddenInfo.AddMessageError("Preservation type is required when produce type is fish.");
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISPreservation_HiddenInfo);
			}
		}

		protected override void CheckZA_AQISCutCode_Hidden()
		{
			base.CheckZA_AQISCutCode_Hidden();
			if (IsEXDOCSValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_AQISCutCode_HiddenInfo, CutCodeMessage);
			}
		}

		internal static IMultilingualString CutCodeMessage
		{
			get { return ResString.GetMultilingualString("B847CE92-F2BD-493C-87AD-E21BAA61DAFB", "This cut code cannot be selected because it is not of the correct commodity type."); }
		}

		protected override void CheckZA_WAR()
		{
			base.CheckZA_WAR();
			if (!Parent.ZA_WAR.IsEmpty && (Pivot?.IsImport ?? false) && new EstablishmentCodeValidation(Parent).ValidateEstablishmentCode(Parent.ZA_WARInfo))
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ZA_WARInfo, Parent.Lookups.EstablishmentCodes);
			}
		}

		protected new CusClassPartPivotAddInfo Parent
		{
			get { return (CusClassPartPivotAddInfo)base.Parent; }
		}
		CusClassPartPivot Pivot
		{
			get { return Parent?.Parent; }
		}

		bool IsEXDOCSValidationRequired => Pivot?.IsExport ?? false;
	}
}
