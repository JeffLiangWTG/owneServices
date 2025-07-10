using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class GoodsInfoProvider : IGoodsInfoProvider
{
	public GoodsInfoProvider(AsycudaPack asycudaPack)
	{
		Pack = Argument.NotNull(asycudaPack, nameof(asycudaPack));
	}
	AsycudaPack Pack { get; }

	public IGoodsDetailsProvider Goods => goods ??= new GoodsDetailsProvider(Pack);
	IGoodsDetailsProvider goods;

	public IFreeTextProvider GoodsDescription => goodsDescription
		??= new FreeTextProvider(TextSubjectCodeQualifierList.GoodsItemDescription, Pack.APA_GoodsDescription);
	IFreeTextProvider goodsDescription;

	public IReadOnlyCollection<IMeasurementProvider> Measurements => measurements ??= GetMeasurements();
	IReadOnlyCollection<IMeasurementProvider> measurements;

	public IGoodsContainerDetailsProvider GoodsContainer => CachedValueHelper.GetValue(ref goodsContainer, () => GetGoodsContainer());
	CachedValue<IGoodsContainerDetailsProvider> goodsContainer;

	public string GoodsMarksDescription => goodsMarksDescription ??= Pack.APA_MarksAndNumbers;
	string goodsMarksDescription;

	public string CustomsGoodsIdentifier => customsGoodsIdentifier ??= Pack.PackedItem.API_Tariff;
	string customsGoodsIdentifier;

	public ILocationProvider OriginCountry => originCountry ??= new LocationProvider(LocationFunctionCodeQualifierList.CountryOfOrigin, Pack.Bill.ABL_RL_NKOrigin.Left(2));
	ILocationProvider originCountry;

	List<MeasurementProvider> GetMeasurements()
	{
		var weightCode = AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Pack.Factory, Pack.APA_WeightUQ);
		var measurements = new List<MeasurementProvider>
		{
			new (MeasuredAttributeCodeList.GoodsItemGrossWeight, Utilities.Round(Convert.ToDecimal((double)Pack.APA_Weight), 6), weightCode)
		};

		if (!Pack.APA_Volume.IsEmpty)
		{
			var volumeCode = AEUniversalLookupsHelper.GetUN20CodeCustomsUQ(Pack.Factory, Pack.APA_VolumeUQ);
			measurements.Add(new(MeasuredAttributeCodeList.Volume, Utilities.Round(Convert.ToDecimal((double)Pack.APA_Volume), 6), volumeCode));
		}

		return measurements;
	}

	GoodsContainerDetailsProvider GetGoodsContainer()
	{
		return Pack.Container is { } container
			? new GoodsContainerDetailsProvider(container.ACN_ContainerNumber, container.ACN_NumberOfPackages) : null;
	}
}
