using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAsycudaManifestHeaderSynchroniser : ASYCUDA.Business.AsycudaManifestHeaderSynchroniser
{
	public CGMAsycudaManifestHeaderSynchroniser(CGMAsycudaManifestHeader destination, ForwardingConsol sourceConsol) : base(destination, sourceConsol)
	{
	}

	new CGMAsycudaManifestHeader Destination => (CGMAsycudaManifestHeader)base.Destination;

	protected override void HookSynchronisers()
	{
		base.HookSynchronisers();
		Synchronisers.Add(portOfOriginSynchroniser = new FieldSynchroniser(Destination.AMA_RL_NKOriginInfo, GetPortOfLoading, GetSourceInfosAffectingPortOfLoading));
		Synchronisers.Add(portOfDestinationSynchroniser = new FieldSynchroniser(Destination.AMA_RL_NKFinalDestinationInfo, GetPortOfDischarge, GetSourceInfosAffectingPortOfDischarge));
		Synchronisers.Add(new FieldSynchroniser(Destination.ManifestQtyInfo, Source.JK_TotalShipmentQuantityInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.GrossWeightInfo, Source.JK_TotalShipmentWeightInfo));
		Synchronisers.Add(new FieldSynchroniser(Destination.GrossWeightUQInfo, Source.JK_TotalShipmentWeightUnitInfo));

		Source.JK_TransportModeInfo.ValueChanged -= JK_TransportModeInfo_ValueChanged;
		Source.JK_TransportModeInfo.ValueChanged += JK_TransportModeInfo_ValueChanged;
		HookAMA_GoodsDescriptionForAWBRateLinesChanges();
		Synchronisers.Add(goodsDescriptionSynchroniser = new FieldSynchroniser(Destination.AMA_GoodsDescriptionInfo, GetGoodsDescription, GetSourceInfosAffectingGoodsDescription));
	}

	protected override void UnHookSynchronisers()
	{
		Source.JK_TransportModeInfo.ValueChanged -= JK_TransportModeInfo_ValueChanged;
		if (Source.AWBHeader?.AWBRateLines is ExportAWBRateLineCollection rateLines)
		{
			rateLines.CountChanged -= RateLines_CountChanged;
		}
		portOfOriginSynchroniser = null;
		portOfDestinationSynchroniser = null;
		goodsDescriptionSynchroniser = null;
		base.UnHookSynchronisers();
	}

	protected override void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		base.Transports_CountChanged(sender, e);
		UpdateInfoEventsAndReSynchronise(portOfOriginSynchroniser);
		UpdateInfoEventsAndReSynchronise(portOfDestinationSynchroniser);
	}

	void JK_TransportModeInfo_ValueChanged(object sender, EventArgs e)
	{
		HookAMA_GoodsDescriptionForAWBRateLinesChanges();
	}

	void HookAMA_GoodsDescriptionForAWBRateLinesChanges()
	{
		if (Source.AWBHeader?.AWBRateLines is ExportAWBRateLineCollection rateLines)
		{
			rateLines.CountChanged -= RateLines_CountChanged;
			rateLines.CountChanged += RateLines_CountChanged;
		}
	}

	void RateLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
	{
		UpdateInfoEventsAndReSynchronise(goodsDescriptionSynchroniser);
	}

	IZType GetGoodsDescription()
	{
		var builder = new ZStringBuilder();
		AWBRateLines
			.Where(rateLine => rateLine.NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription)
			.ForEach(rateLine => builder.AppendIfNotEmpty(rateLine.NatureAndQtyOfGoodsText.Text));
		return new ZString(builder.ToStringWithDelimiterBetweenAppends(" "));
	}

	IEnumerable<ZPropertyInfo> GetSourceInfosAffectingGoodsDescription()
	{
		foreach (var rateLine in AWBRateLines)
		{
			yield return rateLine.NatureAndQtyOfGoodsText.TextInfo;
			yield return rateLine.NatureAndQtyOfGoodsTypeInfo;
		}

		foreach (var info in GetSourceInfosAffectingTransportMode())
		{
			yield return info;
		}
	}

	IEnumerable<ExportAWBRateLine> AWBRateLines => Source.AWBHeader?.AWBRateLines.OfType<ExportAWBRateLine>() ?? Array.Empty<ExportAWBRateLine>();

	FieldSynchroniser portOfOriginSynchroniser;
	FieldSynchroniser portOfDestinationSynchroniser;
	FieldSynchroniser goodsDescriptionSynchroniser;
}
