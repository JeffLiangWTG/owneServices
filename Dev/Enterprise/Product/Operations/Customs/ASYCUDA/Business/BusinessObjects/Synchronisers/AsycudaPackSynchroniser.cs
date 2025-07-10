using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackSynchroniser : BusinessObjectSynchroniser
	{
		public AsycudaPackSynchroniser(AsycudaPack destination, PackLine source)
			: base(destination, source)
		{ }

		public new AsycudaPack Destination
		{
			get { return (AsycudaPack)base.Destination; }
		}

		public new PackLine Source
		{
			get { return (PackLine)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_PackUQInfo, GetPackType, GetSourcevaluesAffectingPackType, !Destination.IsPackUQSynchroniserReadonly && RefPacks == null));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_PackQtyInfo, GetPackQty, GetSourcevaluesAffectingPackQty));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_VolumeInfo, Source.JL_ActualVolumeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_VolumeUQInfo, GetVolumeUQ, GetActualVolumeUQInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_WeightInfo, Source.JL_ActualWeightInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_WeightUQInfo, Source.JL_ActualWeightUQInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_CommodityCodeInfo, Source.JL_RH_NKCommodityCodeInfo));
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_GoodsDescriptionInfo, Source.JL_DescriptionInfo));
			SynchronisersAMA_MarksAndNumbers();
			Synchronisers.Add(new FieldSynchroniser(Destination.UNDGs.FirstItemForBinding[0].DI_DGInfo, GetUnDgSubstance, GetSourceValuesAffectingUnDgSubstance));
			Synchronisers.Add(new FieldSynchroniser(Destination.PackedItem?.API_TariffInfo, GetTariff, GetSourceInfosAffectingTariff));
		}

		IZType GetVolumeUQ() => Source.JL_ActualVolume.IsEmpty ? ZString.Empty : Source.JL_ActualVolumeUQ;

		IEnumerable<ZPropertyInfo> GetActualVolumeUQInfo()
		{
			yield return Source.JL_ActualVolumeInfo;
			yield return Source.JL_ActualVolumeUQInfo;
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingPackType()
		{
			yield return Source.JL_F3_NKPackTypeInfo;
		}

		CusRefPacks RefPacks
		{
			get
			{
				return AsycudaPackHelper.LoadRefPackForManifestLine(Destination.Factory, Source.JL_F3_NKPackType, Destination.Bill?.CountryCode ?? string.Empty);
			}
		}

		IZType GetPackType()
		{
			var result = Source.JL_F3_NKPackType;
			if (Destination.IsPackUQNeedToConvert)
			{
				var refPacks = RefPacks;
				if (refPacks != null)
				{
					result = refPacks.RP_CustomsPack;
				}
			}
			return result;
		}

		IEnumerable<ZPropertyInfo> GetSourcevaluesAffectingPackQty()
		{
			yield return Source.JL_PackageCountInfo;
			yield return Source.JL_F3_NKPackTypeInfo;
		}

		IZType GetPackQty()
		{
			var result = Source.JL_PackageCount;
			if (Destination.IsPackUQNeedToConvert)
			{
				var refPacks = RefPacks;
				if (refPacks != null)
				{
					result = new ZDecimal(refPacks.ConversionFactor * Source.JL_PackageCount).ToZInt();
				}
			}
			return result;
		}

		IZType GetUnDgSubstance()
		{
			return Source.UNDGs.Any() ? Source.UNDGs.FirstItemForBinding[0].DI_DG : ZGuid.Empty;
		}

		IEnumerable<ZPropertyInfo> GetSourceValuesAffectingUnDgSubstance()
		{
			if (Source.UNDGs.Any())
			{
				yield return Source.UNDGs.FirstItemForBinding[0].DI_DGInfo;
				yield return Source.UNDGs.FirstItemForBinding[0].DI_IMOClassInfo;
			}
		}

		IZType GetTariff()
		{
			var jobPackLineHarmonisedCode = Source.HarmonisedCodes.FirstItemForBinding[0];
			var countryCode = Destination.Bill?.CountryCode ?? ZString.Empty;

			if (!countryCode.IsEmpty && jobPackLineHarmonisedCode.JLH_RN_NKCountry == countryCode && !jobPackLineHarmonisedCode.JLH_Code.IsEmpty)
			{
				return jobPackLineHarmonisedCode.JLH_Code;
			}

			return Source.JL_HarmonisedCode;
		}

		IEnumerable<ZPropertyInfo> GetSourceInfosAffectingTariff()
		{
			yield return Source.HarmonisedCodes.FirstItemForBinding[0].JLH_RN_NKCountryInfo;
			yield return Source.HarmonisedCodes.FirstItemForBinding[0].JLH_CodeInfo;
			yield return Source.JL_HarmonisedCodeInfo;
		}

		protected virtual void SynchronisersAMA_MarksAndNumbers()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_MarksAndNumbersInfo, Source.JL_MarksAndNumbersInfo));
		}
	}
}
