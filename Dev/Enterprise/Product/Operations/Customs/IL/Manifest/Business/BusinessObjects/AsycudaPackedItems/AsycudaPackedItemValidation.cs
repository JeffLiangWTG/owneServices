using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaPackedItemValidation : ASYCUDA.Business.AsycudaPackedItemValidation
	{
		public AsycudaPackedItemValidation(AsycudaPackedItem parent) : base(parent)
		{
		}

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateBillItemsLinkedToPackage();
		}

		protected override void CheckAPI_GoodsDescription()
		{
			base.CheckAPI_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.API_GoodsDescriptionInfo);
		}

		protected override void CheckAPI_GrossWeight()
		{
			base.CheckAPI_GrossWeight();
			if (Parent.API_GrossWeight <= 0)
			{
				Parent.API_GrossWeightInfo.AddMessageError(ValidationCaptions.AsycudaPackedItem.TheEnteredValueMustBeGreaterThanZero);
			}
		}

		protected override void CheckAPI_GrossWeightUQ()
		{
			base.CheckAPI_GrossWeightUQ();
			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.API_GrossWeightUQInfo, Parent.API_GrossWeightInfo);
		}

		protected override void CheckAPI_PackStatus()
		{
			base.CheckAPI_PackStatus();

			if (Parent.API_PackStatus == ILPackStatusList.Codes.D)
			{
				CheckMessageStatus_RoleCC_BR1_WCO_024();
				CheckMessageStatus_RoleCC_BR1_WCO_026();
			}
		}

		protected override void CheckAPI_Tariff()
		{
			base.CheckAPI_Tariff();

			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.API_TariffInfo);

			if (parent.API_Tariff.IsEmpty)
			{
				return;
			}

			if (parent.API_Tariff.Length < 4)
			{
				parent.API_TariffInfo.AddMessageError(ValidationCaptions.AsycudaPackedItem.HarmonizedCodeLessThan4Digits);
				return;
			}
		}

		protected override void TariffListValidationCore()
		{
			var parent = Parent;
			var tariff = parent.API_Tariff;
			if (tariff.Length < 4)
			{
				return;
			}

			var tariffList = parent.Lookups.TariffList;
			var filter = tariffList.CompleteFilter;
			var firstFourDigits = tariff.SubstringSafe(0, 4);
			filter.AddToFilter(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, firstFourDigits);
			var result = parent.Factory.LoadTop1<TariffView>(filter);

			if (result == null)
			{
				parent.API_TariffInfo.AddMessageError(ValidationCaptions.AsycudaPackedItem.TheCodeYouSelectedIsNotInTheList);
			}
		}

		void CheckMessageStatus_RoleCC_BR1_WCO_024()
		{
			var parent = Parent;
			var listOfUNDG = parent.UNDGs.Cast<UNDGDataItem>();
			if (!listOfUNDG.Any() || listOfUNDG.Any(r => r.SubstanceCode.IsEmpty))
			{
				parent.API_PackStatusInfo.AddMessageError(ValidationCaptions.AsycudaPackedItem.DGSubstanceMustBeProvidedWhenCargoStatusIsDangerous);
			}
		}
		void CheckMessageStatus_RoleCC_BR1_WCO_026()
		{
			var parent = Parent;
			var listOfUNDG = parent.UNDGs.Cast<UNDGDataItem>();
			if (!listOfUNDG.Any() || listOfUNDG.Any(r => r.DI_OC_DGContact.IsEmpty))
			{
				parent.API_PackStatusInfo.AddMessageError(ValidationCaptions.AsycudaPackedItem.UNDGContactMustBeProvidedWhenCargoStatusIsDangerous);
			}
		}

		void ValidateBillItemsLinkedToPackage()
		{
			var parent = Parent;
			if (parent.AsycudaLinkPackages.Any(r => r.IsLinked))
			{
				return;
			}

			parent.AddRowMessageError(ValidationCaptions.AsycudaPackedItem.AllBillItemsLinkedToPackage);
		}
	}
}
