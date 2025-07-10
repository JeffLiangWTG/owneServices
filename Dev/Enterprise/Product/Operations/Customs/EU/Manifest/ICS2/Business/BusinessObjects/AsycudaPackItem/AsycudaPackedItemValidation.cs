using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent) : base(parent)
		{
		}

		protected new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();

			if (Parent.Pack?.Bill is AsycudaBill bill)
			{
				if (Parent.API_Tariff.IsEmpty)
				{
					if (bill.ConsigneePersonType != EUICS2ScreeningAuthorizedPersonTypes.Codes.AP1 ||
						bill.ShipperPersonType != EUICS2ScreeningAuthorizedPersonTypes.Codes.AP1)
					{
						var specificCircumstanceCodes = new IZType[]
						{
						(ZString)EUICS2SpecificCircumstanceList.Codes.F14,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F15,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F22,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F24,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F26
						};

						if (specificCircumstanceCodes.Contains(bill.Header.SpecificCircumstanceIndicator))
						{
							MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.API_TariffInfo);
						}
					}

					if (bill.Header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F50))
					{
						MandatoryValidation.AddYouHaveNotEnteredMessage(Parent.API_TariffInfo);
					}
				}
				else if (Parent.API_Tariff.Length != 6 && Parent.API_Tariff.Length != 8)
				{
					var lengthErrorMessage = Res.GetString("11ECFFD8-BAD9-43FA-B2BE-A71C3672EA38", "Tariff Code must have 6 or 8 digits.");
					Parent.API_TariffInfo.AddMessageError(lengthErrorMessage);
				}
			}
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			var parent = Parent;
			if (parent.Pack?.Bill is AsycudaBill bill)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(parent.API_GoodsDescriptionInfo, bill.Header.SpecificCircumstanceIndicatorInfo,
				new IZType[] { (ZString)EUICS2SpecificCircumstanceList.Codes.F50, (ZString)EUICS2SpecificCircumstanceList.Codes.F14, (ZString)EUICS2SpecificCircumstanceList.Codes.F15 });

				ValidationHelper.CheckMaxLength(parent.API_GoodsDescriptionInfo, 512);
			}
		}

		protected override void CheckAPI_ChemicalSubstanceCode()
		{
			base.CheckAPI_ChemicalSubstanceCode();
			if (!Parent.API_ChemicalSubstanceCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.API_ChemicalSubstanceCodeInfo);
			}
		}

		protected override void CheckAPI_GoodsValue()
		{
			base.CheckAPI_GoodsValue();
			if (Bill.Header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled && HasAdditionalInfoWithCode10900)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsValueInfo, propertyDescription: Res.GetString("FF32DC87-14BC-4E99-8D64-72AE29199336", "Postal Value"));
			}
		}

		protected override void CheckAPI_TypeOfGoods()
		{
			base.CheckAPI_TypeOfGoods();
			var propertyInfo = Parent.API_TypeOfGoodsInfo;
			if (Bill.Header.IsPackedItemTypeOfGoodsAndGoodsValueEnabled)
			{
				if (Bill.AdditionalFiscalReferences.Count == 0 && HasAdditionalInfoWithCode10900)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(propertyInfo);
			}
		}

		protected override void TariffListValidationCore()
		{
			var parent = Parent;
			var tariff = parent.API_Tariff;

			if (tariff.Length == 4 || tariff.Length == 6)
			{
				if (!HasTariff())
				{
					parent.API_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessageError.ToString());
				}
				return;
			}

			base.TariffListValidationCore();

			bool HasTariff()
			{
				var tariffList = parent.Lookups.TariffList;
				var filter = tariffList.CompleteFilter;
				filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, tariff);
				return parent.Factory.LoadTop1<TariffView>(filter) != null;
			}
		}

		AsycudaBill Bill => Parent.Bill as AsycudaBill;

		bool HasAdditionalInfoWithCode10900 => Parent.Factory.GetValue(ref hasAdditionalInfoWithCode10900, () => Bill.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == EUICS2AdditionalInfoTypes.Codes.CL701_10900));
		CachedProperty<bool> hasAdditionalInfoWithCode10900;
	}
}
