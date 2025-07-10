using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackValidation : ManifestBase.AsycudaPackValidation
	{
		public AsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateLinePrice();
			ValidateLinePriceCurrency();
		}

		protected override void CheckContainerPK()
		{
			base.CheckContainerPK();
			var header = Parent.Bill?.Header;
			if ((header?.IsContainerized ?? false) && Parent.ContainerPK.IsEmpty)
			{
				Parent.ContainerPKInfo.AddMessageError("For containerized cargo, all packs must be linked to a container.");
			}
		}

		protected new AsycudaPack Parent => (AsycudaPack)base.Parent;

		protected override void CheckAPA_Weight()
		{
			base.CheckAPA_Weight();
			if (IsWeightRequired())
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.APA_WeightInfo);
			}
		}
		protected virtual bool IsWeightRequired() => !Parent.APA_WeightUQ.IsEmpty;

		protected override void CheckAPA_WeightUQ()
		{
			base.CheckAPA_WeightUQ();
			if (Parent.APA_Weight == CargoWise.Types.ZDecimal.Zero)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.APA_WeightUQInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_WeightUQInfo);
			}
		}

		protected override void CheckAPA_VolumeUQ()
		{
			base.CheckAPA_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.APA_VolumeUQInfo);
		}

		protected override void CheckAPA_Volume()
		{
			base.CheckAPA_Volume();
			if (IsVolumeRequired())
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.APA_VolumeInfo);
			}
		}

		protected virtual bool IsVolumeRequired() => !Parent.APA_VolumeUQ.IsEmpty;

		protected override void CheckAPA_PackQty()
		{
			base.CheckAPA_PackQty();

			if (NeedsToCheckAPA_PackQty)
			{
				if (IsPackQtyRequired())
				{
					CheckPackQtyIsZero();
				}

				if (!Parent.APA_VINNumber.IsEmpty && Parent.APA_PackQty != 1)
				{
					Parent.APA_PackQtyInfo.AddMessageError("The No of Packs must be 1 if the VIN Number is captured.");
				}
			}
		}

		protected virtual void CheckPackQtyIsZero()
		{
			MandatoryValidation.MessageErrorIfIsZero(Parent.APA_PackQtyInfo);
		}

		protected virtual ZBool NeedsToCheckAPA_PackQty => true;

		protected virtual bool IsPackQtyRequired() => !Parent.APA_PackUQ.IsEmpty;

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			if (NeedsToCheckAPA_PackUQ)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.APA_PackUQInfo);
				ValidateAPA_PackQty();
			}
		}

		protected virtual ZBool NeedsToCheckAPA_PackUQ => true;

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();
			if (Parent.APA_MarksAndNumbers.IsEmpty)
			{
				if (Parent.Bill?.Header?.ZZValidationHelper is ZZDatabaseValidationHelper zzValidationHelper)
				{
					zzValidationHelper.CheckIsMandatoryWhenMatchingAttribute(Parent.APA_MarksAndNumbersInfo, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.PackageMarksByContainerMode,
																						Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.MANDATORYFORCONTAINERMODE,
																						Parent?.Bill?.Header?.AMA_ContainerMode ?? ZString.Empty);
				}
			}
		}

		protected override void CheckAPA_VINNumber()
		{
			base.CheckAPA_VINNumber();
			ValidateAPA_PackQty();
		}

		#region LinePrice

		public void ValidateLinePrice()
		{
			ValidateCalculatedProperty(Parent.LinePriceInfo);
		}

		protected virtual void CheckLinePrice()
		{
			ValidateLinePriceCurrency();
		}

		#endregion

		#region LinePriceCurrency

		public void ValidateLinePriceCurrency()
		{
			ValidateCalculatedProperty(Parent.LinePriceCurrencyInfo);
		}

		protected virtual void CheckLinePriceCurrency()
		{
			ValidationHelper.CheckCurrency(Parent.LinePriceCurrencyInfo, Parent.LinePrice);
		}

		#endregion
	}
}
