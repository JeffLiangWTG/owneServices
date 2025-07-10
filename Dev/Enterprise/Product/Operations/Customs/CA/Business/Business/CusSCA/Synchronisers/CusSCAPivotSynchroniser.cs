namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.Customs.Business;
	using Enterprise.Customs.CA.Business.MessageBuilders;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Forwarding.Business;

	public class CusSCAPivotSynchroniser : BusinessObjectSynchroniser
	{
		readonly PackLine linkedOuterPackLine;

		public CusSCAPivotSynchroniser(CusSCAPivot destination, PackLine source)
			: base(destination, source)
		{
			linkedOuterPackLine = source;
		}

		public CusSCAPivotSynchroniser(CusSCAPivot destination, PackLine source, PackLine linkedOuterPackLine) : base(destination, source)
		{
			this.linkedOuterPackLine = Argument.NotNull(linkedOuterPackLine, nameof(linkedOuterPackLine));
		}

		PackLine SourcePackLine
		{
			get
			{
				if (sourcePackLine == null)
				{
					sourcePackLine = Source as PackLine;
				}
				return sourcePackLine;
			}
		}
		PackLine sourcePackLine;

		public new CusSCAPivot Destination
		{
			get { return (CusSCAPivot)base.Destination; }
		}

		protected override void HookSynchronisers()
		{
			HookSynchronisersForPackLine();

			if (linkedOuterPackLine != null)
			{
				foreach (ForwardingContainer container in linkedOuterPackLine.Containers)
				{
					container.JC_ContainerNumInfo.ValueChanged -= JC_ContainerNumInfo_ValueChanged;
					container.JC_ContainerNumInfo.ValueChanged += JC_ContainerNumInfo_ValueChanged;
				}
				linkedOuterPackLine.Containers.CountChanged -= Container_CountChanged;
				linkedOuterPackLine.Containers.CountChanged += Container_CountChanged;
			}
		}

		void HookSynchronisersForPackLine()
		{
			if (Source is PackLine source && !source.IsDeleted && Destination is CusSCAPivot destination && !destination.IsDeleted)
			{
				var shipment = source.Shipment;
				Synchronisers.Add(new FieldSynchroniser(destination.CV_PackageCountInfo, GetCount, () => new List<ZPropertyInfo>() { source.JL_PackageCountInfo, shipment.JS_TotalPackageCountInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_PackageTypeInfo, GetUnit, () => new List<ZPropertyInfo>() { source.JL_F3_NKPackTypeInfo, shipment.JS_F3_NKTotalCountPackTypeInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_WeightInfo, source.JL_ActualWeightInfo));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_WeightUQInfo, source.JL_ActualWeightUQInfo));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_VolumeInfo, source.JL_ActualVolumeInfo));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_VolumeUQInfo, source.JL_ActualVolumeUQInfo));

				Synchronisers.Add(new FieldSynchroniser(destination.CN_ContainerModeInfo, GetPackMode, () => new List<ZPropertyInfo>() { linkedOuterPackLine.JL_Calc_ContainerNumberInfo, shipment.JS_PackingModeInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_CNInfo, GetOceanBillContainerPK, () => new List<ZPropertyInfo>() { linkedOuterPackLine.JL_Calc_ContainerNumberInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_HarmonisedTariffNumsInfo, linkedOuterPackLine.JL_HarmonisedCodeInfo));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_GoodsDescriptionInfo, GetDescription,
					() => new List<ZPropertyInfo>() { source.JL_DescriptionInfo, source.JL_DetailedDescriptionInfo, linkedOuterPackLine.JL_DescriptionInfo, shipment.JS_GoodsDescriptionInfo, shipment.DetailedGoodsDescriptionNoteTextInfo }));
				Synchronisers.Add(new FieldSynchroniser(destination.CV_MarksAndNumbersInfo, GetMarks, () => new List<ZPropertyInfo>() { linkedOuterPackLine.JL_MarksAndNumbersInfo, shipment.JS_MarksAndNumbersInfo }));
				Synchronisers.Add(new UNDGCollectionSynchroniser(linkedOuterPackLine, destination));
			}
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			if (linkedOuterPackLine != null)
			{
				linkedOuterPackLine.Containers.CountChanged -= Container_CountChanged;
			}
		}

		void Container_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				var container = (ForwardingContainer)e.BizObject;
				container.JC_ContainerNumInfo.ValueChanged -= JC_ContainerNumInfo_ValueChanged;
				container.JC_ContainerNumInfo.ValueChanged += JC_ContainerNumInfo_ValueChanged;
			}
			else if (e.ItemAdded)
			{
				var container = (ForwardingContainer)e.BizObject;
				container.JC_ContainerNumInfo.ValueChanged -= JC_ContainerNumInfo_ValueChanged;
			}
			Synchronise();
		}

		void JC_ContainerNumInfo_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		void ShipmentInnerInfo_ValueChanged(object sender, EventArgs e)
		{
			Synchronise();
		}

		IZType GetOceanBillContainerPK()
		{
			var container = Container;
			return container == null ? ZGuid.Empty : container.PK;
		}

		IZType GetPackMode()
		{
			var result = Destination.CN_ContainerMode;
			var shipment = linkedOuterPackLine.Shipment;
			if (shipment != null && shipment.OuterPackLines.Count == 1)
			{
				var container = Container;
				if (container != null && Destination.OceanBill.ShouldSyncroniseWithConsol)
				{
					if (container.HookedContainer != null)
					{
						result = CusSCAContainerSynchroniser.GetContainerMode(container.HookedContainer);
					}

					var isContainerised = containerisedModes.Contains(shipment.JS_PackingMode);
					if (container.CN_ContainerNumber != CusSCAHouse.NonContaineriseID && !isContainerised)
					{
						result = ZString.Empty;
					}
				}
			}
			return result;
		}

		readonly List<string> containerisedModes = new List<string>(new string[] {
			Constants.ContainerModes.FCL,
			Constants.ContainerModes.FCLMixedShipper,
			Constants.ContainerModes.BuyersConsol,
			Constants.ContainerModes.Containerised,
			Constants.ContainerModes.Combination,
			Constants.ContainerModes.LCL
		});

		CusSCAContainer Container
		{
			get
			{
				CusSCAContainer result = null;
				var containerNumber = linkedOuterPackLine.JL_Calc_ContainerNum;
				var oceanBill = Destination.OceanBill;
				if (oceanBill != null)
				{
					if (containerNumber.IsEmpty)
					{
						containerNumber = linkedOuterPackLine.ContainerNumberForConsol(oceanBill.Consol);
					}
					if (containerNumber.IsEmpty)
					{
						containerNumber = CusSCAHouse.NonContaineriseID;
					}
					result = oceanBill.FindContainerByNumber(containerNumber);
				}
				return result;
			}
		}

		IZType GetDescription()
		{
			return GetDescriptionFromShipmentPackLine(SourcePackLine, linkedOuterPackLine);
		}

		IZType GetMarks()
		{
			return GetMarksFromShipmentPackLine(linkedOuterPackLine);
		}

		public static ZString GetDescriptionFromShipmentPackLine(PackLine shipmentPackLine, PackLine linkedOuterPackLine = null)
		{
			var result = shipmentPackLine.JL_DetailedDescription;
			if (result.IsEmpty)
			{
				result = shipmentPackLine.JL_Description;
				if (result.IsEmpty && linkedOuterPackLine != null)
				{
					result = linkedOuterPackLine.JL_Description;
				}
			}
			if (result.IsEmpty && shipmentPackLine.Shipment is ForwardingShipment shipment)
			{
				result = shipment.DetailedGoodsDescriptionNoteText;
				if (result.IsEmpty)
				{
					result = shipment.JS_GoodsDescription;
				}
			}
			return result.Left(AutoCusSCAPivot.Schema.CV_GoodsDescriptionMaxLength);
		}

		public static ZString GetMarksFromShipmentPackLine(PackLine shipmentPackLine)
		{
			var result = shipmentPackLine.JL_MarksAndNumbers;
			if (result.IsEmpty && shipmentPackLine.Shipment is ForwardingShipment shipment)
			{
				result = shipment.JS_MarksAndNumbers;
			}
			return result.Left(AutoCusSCAPivot.Schema.CV_MarksAndNumbersMaxLength);
		}

		IZType GetCount()
		{
			var data = SourcePackLine.GetEffectivePackLineQuantity();
			return data.Qty;
		}

		IZType GetUnit()
		{
			var data = SourcePackLine.GetEffectivePackLineQuantity();
			return D96AMessageUtilities.ConvertPackUnitToACROSSUnit(data.UQ);
		}
	}
}
