using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePackedItemValidation : AsycudaPackedItemValidation
	{
		public TemporaryStoragePackedItemValidation(AutoAsycudaPackedItem parent) : base(parent)
		{
		}

		public new TemporaryStoragePackedItem Parent => (TemporaryStoragePackedItem)base.Parent;

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();
			var parent = Parent;
			if (parent.API_Tariff.IsEmpty)
			{
				if (parent.ValidationDecider is ITemporaryStoragePackedItemValidationDecider validationDecider
					&& validationDecider.IsAPI_TariffMandatory)
				{
					var info = parent.API_TariffInfo;
					info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.Description));
				}
			}
			else
			{
				TariffView result;
				if (parent.API_Tariff.Length == 8)
				{
					var tariffList = parent.Lookups.TariffList;
					var filter = tariffList.CompleteFilter;
					filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, parent.API_Tariff);
					filter.OrderBy = TariffViewSchema.Constants.ZZ1_TariffCode;
					result = parent.Factory.LoadTop1<TariffView>(filter);
				}
				else
				{
					result = parent.UniversalTariff;
				}

				if (result == null)
				{
					parent.API_FormattedTariffInfo.AddMessageError(Res.GetString("3F62BD69-3039-45E5-8AE5-936107938E96", "The code you have selected is not in the list."));
				}

				if (ValidateDuplicatedTariff && Parent.Bill.PackedItems.Cast<TemporaryStoragePackedItem>().Count(x => !x.API_Tariff.IsEmpty && x.API_Tariff.Substring(0, 6) == Parent.API_Tariff.Substring(0, 6)) > 1)
				{
					Parent.API_FormattedTariffInfo.AddMessageError(Res.GetString("0D7FBACE-7A0F-4CB8-8812-E671572605C8", "Items under the same bill can’t have the same Tariff Sub Heading Value."));
				}
			}
		}

		protected virtual bool ValidateDuplicatedTariff => true;

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			var parent = Parent;
			if (parent.ValidationDecider is ITemporaryStoragePackedItemValidationDecider validationDecider
					&& validationDecider.IsAPI_GoodsDescriptionMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.API_GoodsDescriptionInfo);
			}
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			if (Parent.AsycudaPackPackedItemPivots.Count == 0)
			{
				if (Parent.API_GrossWeight <= 0)
				{
					Parent.API_GrossWeightInfo.AddMessageError(Res.GetString("79716CA2-A7F0-4636-BD4C-EBE2BE0867D3", "The entered value must be greater than 0."));
				}
			}
			else
			{
				if (Parent.API_GrossWeight < 0)
				{
					Parent.API_GrossWeightInfo.AddMessageError(Res.GetString("4E058D7F-C77E-42A9-972D-778B221989DE", "The entered value must be greater than or equal to 0."));
				}
			}
		}

		protected override void CheckAPI_GrossWeightUQ()
		{
			base.CheckAPI_GrossWeightUQ();
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.API_GrossWeightUQInfo, Parent.API_GrossWeightInfo);
		}

		protected override void CheckAPI_NetWeightUQ()
		{
			base.CheckAPI_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.API_NetWeightUQInfo);
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.API_NetWeightUQInfo, Parent.API_NetWeightInfo);
		}
	}
}
