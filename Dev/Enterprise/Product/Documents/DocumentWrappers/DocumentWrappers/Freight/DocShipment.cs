using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.Freight;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers
{
	public class DocShipment : DocBaseWrapperWithJobHeader, Integration.DocumentWrappers.IDocShipment, IPreAlert, ITimeSlotRequest, IDocCartageAdvice, IContainsSuppressedFields, IDocServicesParent, IDocTypeCode
	{
		#region Constructors

		protected DocShipment(CommonShipment shipment, BusinessObjectFactory factoryToWrap)
			: base(shipment, factoryToWrap)
		{
		}

		public static DocShipment New(CommonShipment shipment, BusinessObjectFactory factoryToWrap)
		{
			DocShipment result = null;

			ForwardingShipment forwardingShipment = shipment as ForwardingShipment;
			AgencyShipment agencyShipment = shipment as AgencyShipment;

			if (forwardingShipment != null)
			{
				var overridden = OverridableForwardingNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden(forwardingShipment, factoryToWrap);
				}
				else if (shipment != null)
				{
					result = DocForwardingShipment.New(forwardingShipment, factoryToWrap);
				}
			}
			else if (agencyShipment != null)
			{
				var overridden = OverridableAgencyNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden(agencyShipment, factoryToWrap);
				}
				else if (shipment != null)
				{
					result = DocAgencyShipment.New(agencyShipment, factoryToWrap);
				}
			}
			else
			{
				var overridden = OverridableNewDelegate.Value;
				if (overridden != null)
				{
					result = overridden(shipment, factoryToWrap);
				}
				else if (shipment != null)
				{
					result = new DocShipment(shipment, factoryToWrap);
				}
			}

			return result;
		}

		static string AsAbove
		{
			get { return Res.GetString("d07988b5-9edb-4774-9ca9-e62d2de646ae", "As above - no amendment required"); }
		}

		public static DocShipment New(DocumentShipment documentShipment, BusinessObjectFactory factoryToWrap)
		{
			DocShipment wrapper = null;

			if (documentShipment != null && documentShipment.Shipment != null)
			{
				wrapper = New(documentShipment.Shipment, factoryToWrap);
				if (documentShipment.DataContext == Constants.DataContext.FreightLabels)
				{
					wrapper.IncludeConsignee = documentShipment.IncludeConsignee;
					wrapper.IncludeConsignor = documentShipment.IncludeConsignor;
					wrapper.IncludeNone = documentShipment.IncludeNone;
					wrapper.IncludeSendingAgent = documentShipment.IncludeSendingAgent;
					wrapper.NumberOfLabelsToPrint = documentShipment.NumberOfLabelsToPrint;
				}
				else if (documentShipment.DataContext == Constants.DataContext.LetterOfIndemnity)
				{
					wrapper.OldMarksAndNumbers = documentShipment.OldMarksAndNumbers;
					wrapper.OldGoodsDescription = documentShipment.OldGoodsDescription;
					wrapper.OldWeight = documentShipment.OldWeight.ToString() + " " + documentShipment.OldWeightUnit;
					wrapper.OldVolume = documentShipment.OldVolume.ToString() + " " + documentShipment.OldVolumeUnit;
					wrapper.NewMarksAndNumbers = documentShipment.ChangeMarksAndNumbers ? documentShipment.NewMarksAndNumbers : (ZString)AsAbove;
					wrapper.NewGoodsDescription = documentShipment.ChangeGoodsDescription ? documentShipment.NewGoodsDescription : (ZString)AsAbove;
					wrapper.NewWeight = documentShipment.ChangeWeight ? documentShipment.NewWeight.ToString() + " " + documentShipment.NewWeightUnit : AsAbove;
					wrapper.NewVolume = documentShipment.ChangeVolume ? documentShipment.NewVolume.ToString() + " " + documentShipment.NewVolumeUnit : AsAbove;
				}
				else if (documentShipment.DataContext == Constants.DataContext.ChargeSheet)
				{
					wrapper.Debtor = documentShipment.Debtor.Debtor;
				}
				else if (documentShipment.DataContext == Constants.DataContext.Shipment)
				{
					wrapper.IsBillOfLading = documentShipment.IsBillOfLading;
				}

				wrapper.ConfirmationToPrint = DocPickupDeliveryConfirm.New(documentShipment.ConfirmationToPrint, factoryToWrap);
			}

			return wrapper;
		}

		public static DocShipment New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<CommonShipment>(pK), factory);
		}

		protected internal CommonShipment CommonShipment
		{
			get { return (CommonShipment)WrappedObject; }
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			return ShipmentNumber;
		}

		#endregion

		#region Freight Labels Properties

		public ZBool IncludeConsignee { get; private set; }
		public ZBool IncludeConsignor { get; private set; }
		public ZBool IncludeNone { get; private set; }
		public ZBool IncludeSendingAgent { get; private set; }
		public ZInt NumberOfLabelsToPrint { get; private set; }

		#endregion

		#region New Details - Letter Of Indemnity

		public ZString OldMarksAndNumbers { get; private set; }
		public ZString OldGoodsDescription { get; private set; }
		public ZString OldWeight { get; private set; }
		public ZString OldVolume { get; private set; }
		public ZString NewMarksAndNumbers { get; private set; }
		public ZString NewGoodsDescription { get; private set; }
		public ZString NewWeight { get; private set; }
		public ZString NewVolume { get; private set; }

		#endregion

		#region Charge Sheet

		protected internal OrgHeader Debtor;

		#endregion

		#region Standard Shipping Note

		public DocPickupDeliveryConfirm ConfirmationToPrint { get; set; }

		#endregion

		#region Container Legs

		public DocPickupDeliveryConfirmCollection DeliveryLegs
		{
			get
			{
				if (deliveryLegs == null)
				{
					deliveryLegs = new DocPickupDeliveryConfirmCollection(CommonShipment.DeliveryConfirms);
					deliveryLegs.Sort(AscendingSort);
				}

				return deliveryLegs;
			}
		}

		DocPickupDeliveryConfirmCollection deliveryLegs;

		readonly Comparison<DocPickupDeliveryConfirm> AscendingSort = (DocPickupDeliveryConfirm x, DocPickupDeliveryConfirm y) =>
		{
			int result;
			result = x.DeliverTimeIn.CompareTo(y.DeliverTimeIn);
			if (result == 0)
			{
				result = x.PackagesDelivered.CompareTo(y.PackagesDelivered);
			}

			if (result == 0)
			{
				result = x.DeliverySignedFor.CompareTo(y.DeliverySignedFor);
			}

			return result;
		};

		#endregion

		#region Specific Outer Pack Lines

		public DocPackLines OuterPack1
		{
			get { return OuterPackLineCollection.Count > 0 ? OuterPackLineCollection[0] : null; }
		}

		public DocPackLines OuterPack2
		{
			get { return OuterPackLineCollection.Count > 1 ? OuterPackLineCollection[1] : null; }
		}

		public DocPackLines OuterPack3
		{
			get { return OuterPackLineCollection.Count > 2 ? OuterPackLineCollection[2] : null; }
		}

		public DocPackLines OuterPack4
		{
			get { return OuterPackLineCollection.Count > 3 ? OuterPackLineCollection[3] : null; }
		}

		public DocPackLines OuterPack5
		{
			get { return OuterPackLineCollection.Count > 4 ? OuterPackLineCollection[4] : null; }
		}

		public DocPackLines OuterPack6
		{
			get { return OuterPackLineCollection.Count > 5 ? OuterPackLineCollection[5] : null; }
		}

		public DocPackLines OuterPack7
		{
			get { return OuterPackLineCollection.Count > 6 ? OuterPackLineCollection[6] : null; }
		}

		public DocPackLines OuterPack8
		{
			get { return OuterPackLineCollection.Count > 7 ? OuterPackLineCollection[7] : null; }
		}

		#endregion

		#region Template Constants

		public ZString HBLCode
		{
			get { return GetTemplateConstantValue<ZString>(DocumentEngineIntegration.Constants.TemplateDefined.HBLCode); }
		}

		public ZInt ShowContainerGross
		{
			get
			{
				return Env.Registry.DisplayTareAndGrossWeightOnHBOL && CommonShipment.JS_PackingMode == "FCL"
						? GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerGross, 1)
						: (ZInt)0;
			}
		}

		public ZInt ShowContainerTare
		{
			get
			{
				return Env.Registry.DisplayTareAndGrossWeightOnHBOL && CommonShipment.JS_PackingMode == "FCL"
						? GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTare, 1)
						: (ZInt)0;
			}
		}

		public ZBool ChargeInfoEnableColumnarFormat
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoEnableColumnarFormat); }
		}

		#region Bill of Lading Constants

		public ZBool UseMultiPage { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.UseMultiPage, false); } }
		public ZBool UseImperialUnits { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.UseImperialUnits, false); } }
		public ZBool ShowDetailHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowDetailHeadingInMainBody, false); } }
		public ZBool ShowContainerHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerHeadingInMainBody, false); } }
		public ZBool ShowChargesHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowChargesHeadingInMainBody, false); } }
		public ZBool ShowExtraSectionHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowExtraSectionHeadingInMainBody, false); } }
		public ZBool ShowBOLClauseSectionHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowBOLClauseSectionHeadingInMainBody, false); } }
		public ZBool IncludeBOLClauseInGoodsDescription { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseInGoodsDescription, true); } }
		public ZBool IncludeContainersInMarksAndNumbersSection { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeContainersInMarksAndNumbersSection, false); } }
		public ZBool IncludeExtraSectionInMarksAndNumbersSection { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeExtraSectionInMarksAndNumbersSection, false); } }
		public ZBool IncludeBOLClauseSectionInMarksAndNumbersSection { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeBOLClauseSectionInMarksAndNumbersSection, false); } }
		public ZBool ShowRORHeadingInMainBody { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ShowRORHeadingInMainBody, true); } }
		public ZBool IncludeRORInMarksAndNumbersSection { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeRORInMarksAndNumbersSection, true); } }
		public ZBool InterleavePacksAndContainers { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.InterleavePacksAndContainers, false); } }
		public ZBool IncludeEmergencyContactWithUNDG { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.IncludeEmergencyContactWithUNDG, false); } }
		public ZBool SuppressZeroCharges { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.SuppressZeroCharges, false); } }
		public ZBool ConvertUnits { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ConvertUnits, true); } }

		public ZString BOLWeightUnit
		{
			get { return UseImperialUnits ? Constants.Weight.Pounds : Constants.Weight.Kilograms; }
		}

		public ZString BOLVolumeUnit
		{
			get { return UseImperialUnits ? Constants.Volume.CubicFeet : Constants.Volume.CubicMetres; }
		}

		public ZString BOLLengthUnit
		{
			get { return UseImperialUnits ? Constants.Length.Feet : Constants.Length.Metres; }
		}

		public ZInt ChargeDescriptionColumnWidth
		{
			get
			{
				// Try correct spelling first, fallback to incorrect spelling if it doesn't exist. (For compatibility with legacy / client documents)
				return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionColumnWidth, ChargesDiscriptionWidth);
			}
		}

		public ZInt ChargesDescriptionFollowOnPageWidth
		{
			get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDescriptionFollowOnPageWidth, 80); }
		}

		public ZInt ContainerWeightAndUQWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQWidth, 14); } }
		public ZInt ContainerTareAndUQWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQWidth, 14); } }
		public ZInt ContainerGrossAndUQWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQWidth, 14); } }
		public ZInt ContainerVolumeAndUQWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQWidth, 14); } }
		public ZInt ChargeCodeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeColumnWidth, 0); } }
		public ZInt ChargeCodeDescColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeDescColumnWidth, 20); } }
		public ZInt CollectChargesColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnWidth, 11); } }
		public ZInt CollectCurrencyColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnWidth, 3); } }
		public ZInt PrepaidChargesColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnWidth, 11); } }
		public ZInt PrepaidCurrencyColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnWidth, 3); } }
		public ZInt AllChargesColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnWidth, 11); } }
		public ZInt AllChargesCurrencyColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnWidth, 3); } }
		public ZInt PackRefNumberColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnWidth, 20); } }
		public ZInt PackDescriptionColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnWidth, 0); } }
		public ZInt PackShortDescriptionColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnWidth, 20); } }
		public ZInt PackMarksAndNumbersColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnWidth, 16); } }
		public ZInt PackContainsUNDGColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnWidth, 1); } }
		public ZInt PackUNDGColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnWidth, 0); } }
		public ZInt PackDescriptionAndUNDGColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnWidth, 0); } }
		public ZInt PackUNDGAndShortDescriptionColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnWidth, 0); } }
		public ZInt PackRefNumberAndMarksAndNumbersColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnWidth, 0); } }
		public ZInt PackCommodityCodeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnWidth, 4); } }
		public ZInt PackCommodityDescriptionColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnWidth, 16); } }
		public ZInt PackCountColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnWidth, 9); } }
		public ZInt PackCountAndTypeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnWidth, 11); } }
		public ZInt PackWeightColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnWidth, 14); } }
		public ZInt PackWeightAndUQColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnWidth, 14); } }
		public ZInt PackVolumeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnWidth, 14); } }
		public ZInt PackVolumeAndUQColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnWidth, 14); } }
		public ZInt PackLengthColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnWidth, 11); } }
		public ZInt PackLengthAndUQColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnWidth, 11); } }
		public ZInt PackWidthColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnWidth, 11); } }
		public ZInt PackWidthAndUQColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnWidth, 11); } }
		public ZInt PackHeightColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnWidth, 11); } }
		public ZInt PackHeightAndUQColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnWidth, 11); } }
		public ZInt PackAreaColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnWidth, 14); } }
		public ZInt PackDimensionsColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnWidth, 20); } }
		public ZInt PackHarmonizedCodeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnWidth, 15); } }
		public ZInt PackVehicleColourColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnWidth, 11); } }
		public ZInt PackVehicleMakeColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnWidth, 14); } }
		public ZInt PackVehicleModelColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnWidth, 14); } }
		public ZInt PackVehicleNumberOfDoorsColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnWidth, 5); } }
		public ZInt PackVehicleTransmissionColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnWidth, 6); } }
		public ZInt PackVehicleYearColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnWidth, 4); } }
		public ZInt PackVehicleFullDetailsColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnWidth, 36); } }
		public ZInt BOLClauseColumnWidth { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnWidth, 20); } }

		public ZInt MarksAndNumbersIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersIndex, 1); } }
		public ZInt PackagesIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackagesIndex, 0); } }
		public ZInt GoodsDescIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescIndex, 3); } }
		public ZInt GrossWeightIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightIndex, 4); } }
		public ZInt VolumeMeasurementIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementIndex, 5); } }
		public ZInt ContainerNumberIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberIndex, 1); } }
		public ZInt ContainerSealIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealIndex, 2); } }
		public ZInt ContainerTypeIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeIndex, 3); } }
		public ZInt ContainerWeightIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightIndex, 4); } }
		public ZInt ContainerWeightAndUQIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQIndex, 0); } }
		public ZInt ContainerTareIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareIndex, 5); } }
		public ZInt ContainerTareAndUQIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQIndex, 0); } }
		public ZInt ContainerGrossIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossIndex, 6); } }
		public ZInt ContainerGrossAndUQIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQIndex, 0); } }
		public ZInt ContainerVolumeIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeIndex, 7); } }
		public ZInt ContainerVolumeAndUQIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQIndex, 0); } }
		public ZInt ContainerPackagesIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesIndex, 8); } }
		public ZInt ContainerModeIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeIndex, 0); } }
		public ZInt ContainerTemperatureSettingIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingIndex, 0); } }
		public ZInt ContainerHumiditySettingIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingIndex, 0); } }
		public ZInt ChargeCodeDescIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeDescIndex, 0); } }
		public ZInt ChargeCodeIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeIndex, 0); } }
		public ZInt ChargeDescriptionIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionIndex, 1); } }
		public ZInt CollectChargesColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnIndex, 2); } }
		public ZInt CollectCurrencyColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnIndex, 3); } }
		public ZInt PrepaidChargesColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnIndex, 4); } }
		public ZInt PrepaidCurrencyColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnIndex, 5); } }
		public ZInt AllChargesColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnIndex, 0); } }
		public ZInt AllChargesCurrencyColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnIndex, 0); } }
		public ZInt PackRefNumberColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnIndex, 1); } }
		public ZInt PackDescriptionColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnIndex, 0); } }
		public ZInt PackShortDescriptionColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnIndex, 0); } }
		public ZInt PackMarksAndNumbersColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnIndex, 0); } }
		public ZInt PackContainsUNDGColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnIndex, 0); } }
		public ZInt PackUNDGColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnIndex, 0); } }
		public ZInt PackDescriptionAndUNDGColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnIndex, 0); } }
		public ZInt PackUNDGAndShortDescriptionColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnIndex, 0); } }
		public ZInt PackRefNumberAndMarksAndNumbersColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnIndex, 0); } }
		public ZInt PackCommodityCodeColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnIndex, 0); } }
		public ZInt PackCommodityDescriptionColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnIndex, 0); } }
		public ZInt PackCountColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnIndex, 2); } }
		public ZInt PackCountAndTypeColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnIndex, 0); } }
		public ZInt PackWeightColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnIndex, 3); } }
		public ZInt PackWeightAndUQColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnIndex, 0); } }
		public ZInt PackVolumeColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnIndex, 4); } }
		public ZInt PackVolumeAndUQColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnIndex, 0); } }
		public ZInt PackLengthColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnIndex, 5); } }
		public ZInt PackLengthAndUQColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnIndex, 0); } }
		public ZInt PackWidthColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnIndex, 6); } }
		public ZInt PackWidthAndUQColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnIndex, 0); } }
		public ZInt PackHeightColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnIndex, 7); } }
		public ZInt PackHeightAndUQColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnIndex, 0); } }
		public ZInt PackAreaColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnIndex, 8); } }
		public ZInt PackDimensionsColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnIndex, 0); } }
		public ZInt PackHarmonizedCodeColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnIndex, 0); } }
		public ZInt PackVehicleColourColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnIndex, 0); } }
		public ZInt PackVehicleMakeColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnIndex, 0); } }
		public ZInt PackVehicleModelColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnIndex, 0); } }
		public ZInt PackVehicleNumberOfDoorsColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnIndex, 0); } }
		public ZInt PackVehicleTransmissionColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnIndex, 0); } }
		public ZInt PackVehicleYearColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnIndex, 0); } }
		public ZInt PackVehicleFullDetailsColumnIndex { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnIndex, 0); } }

		public ZInt MarksAndNumbersLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersLeftPadding, 0); } }
		public ZInt PackagesLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackagesLeftPadding, 1); } }
		public ZInt GoodsDescLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescLeftPadding, 1); } }
		public ZInt GrossWeightLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightLeftPadding, 1); } }
		public ZInt VolumeMeasurementLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementLeftPadding, 1); } }
		public ZInt ContainerNumberLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberLeftPadding, 0); } }
		public ZInt ContainerSealLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealLeftPadding, 1); } }
		public ZInt ContainerTypeLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeLeftPadding, 1); } }
		public ZInt ContainerWeightLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightLeftPadding, 1); } }
		public ZInt ContainerWeightAndUQLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQLeftPadding, 1); } }
		public ZInt ContainerTareLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareLeftPadding, 1); } }
		public ZInt ContainerTareAndUQLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQLeftPadding, 1); } }
		public ZInt ContainerGrossLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossLeftPadding, 1); } }
		public ZInt ContainerGrossAndUQLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQLeftPadding, 1); } }
		public ZInt ContainerVolumeLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeLeftPadding, 1); } }
		public ZInt ContainerVolumeAndUQLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQLeftPadding, 1); } }
		public ZInt ContainerPackagesLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesLeftPadding, 1); } }
		public ZInt ContainerModeLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeLeftPadding, 1); } }
		public ZInt ChargeCodeLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeLeftPadding, 0); } }
		public ZInt ChargeCodeDescLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeDescLeftPadding, 0); } }
		public ZInt ChargeDescriptionLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionLeftPadding, 0); } }
		public ZInt CollectChargesColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnLeftPadding, 1); } }
		public ZInt CollectCurrencyColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnLeftPadding, 1); } }
		public ZInt PrepaidChargesColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnLeftPadding, 1); } }
		public ZInt AllChargesCurrencyColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnLeftPadding, 1); } }
		public ZInt AllChargesColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnLeftPadding, 1); } }
		public ZInt PrepaidCurrencyColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnLeftPadding, 1); } }
		public ZInt PackRefNumberColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnLeftPadding, 0); } }
		public ZInt PackDescriptionColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnLeftPadding, 0); } }
		public ZInt PackShortDescriptionColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnLeftPadding, 1); } }
		public ZInt PackMarksAndNumbersColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnLeftPadding, 1); } }
		public ZInt PackContainsUNDGColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnLeftPadding, 1); } }
		public ZInt PackUNDGColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnLeftPadding, 1); } }
		public ZInt PackDescriptionAndUNDGColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnLeftPadding, 1); } }
		public ZInt PackUNDGAndShortDescriptionColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnLeftPadding, 1); } }
		public ZInt PackRefNumberAndMarksAndNumbersColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnLeftPadding, 0); } }
		public ZInt PackCommodityCodeColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnLeftPadding, 1); } }
		public ZInt PackCommodityDescriptionColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnLeftPadding, 1); } }
		public ZInt PackCountColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnLeftPadding, 1); } }
		public ZInt PackCountAndTypeColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnLeftPadding, 1); } }
		public ZInt PackWeightColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnLeftPadding, 1); } }
		public ZInt PackWeightAndUQColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnLeftPadding, 1); } }
		public ZInt PackVolumeColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnLeftPadding, 1); } }
		public ZInt PackVolumeAndUQColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnLeftPadding, 1); } }
		public ZInt PackLengthColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnLeftPadding, 1); } }
		public ZInt PackLengthAndUQColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnLeftPadding, 1); } }
		public ZInt PackWidthColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnLeftPadding, 1); } }
		public ZInt PackWidthAndUQColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnLeftPadding, 1); } }
		public ZInt PackHeightColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnLeftPadding, 1); } }
		public ZInt PackHeightAndUQColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnLeftPadding, 1); } }
		public ZInt PackAreaColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnLeftPadding, 1); } }
		public ZInt PackDimensionsColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnLeftPadding, 1); } }
		public ZInt PackHarmonizedCodeColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnLeftPadding, 1); } }
		public ZInt PackVehicleColourColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnLeftPadding, 1); } }
		public ZInt PackVehicleMakeColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnLeftPadding, 1); } }
		public ZInt PackVehicleModelColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnLeftPadding, 1); } }
		public ZInt PackVehicleNumberOfDoorsColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnLeftPadding, 1); } }
		public ZInt PackVehicleTransmissionColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnLeftPadding, 1); } }
		public ZInt PackVehicleYearColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnLeftPadding, 1); } }
		public ZInt PackVehicleFullDetailsColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnLeftPadding, 1); } }
		public ZInt BOLClauseColumnLeftPadding { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnLeftPadding, 0); } }

		public ZString MarksAndNumbersCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersCaption, Res.GetString("c5f898d1-df59-438e-8449-264986ac5ab0", "Marks & Numbers")); } }
		public ZString PackagesCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackagesCaption, Res.GetString("1feacd0f-0d8c-4f7c-a71a-9a83f5425aeb", "Packs")); } }
		public ZString GoodsDescCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescCaption, Res.GetString("1b3d9354-f6c3-463f-b906-1e7e401286a6", "Goods Description")); } }
		public ZString GrossWeightCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightCaption, Res.GetString("84cf849d-322b-40b2-802c-6885e12d52f0", "Gross Wt")); } }
		public ZString VolumeMeasurementCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementCaption, Res.GetString("4096a100-b5a2-4b7e-95ee-2cd1d08641f0", "Volume")); } }
		public ZString ContainerNumberCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberCaption, Res.GetString("baf4f96f-2591-4daa-82be-7eb52704063e", "Cn. No")); } }
		public ZString ContainerSealCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealCaption, Res.GetString("eb5832f7-2e2e-4868-86a5-059480198179", "Seal")); } }
		public ZString ContainerTypeCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeCaption, Res.GetString("9ac18b23-423c-4e43-8156-04e2cd70b39d", "Type")); } }
		public ZString ContainerWeightCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightCaption, Res.GetString("f4bebb9e-1e05-479a-9c6b-ec502000a278", "Net (kg)")); } }
		public ZString ContainerWeightAndUQCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndUQCaption, Res.GetString("e1e1c0e8-0a5f-437e-b1ad-18c6ad986f1d", "Net")); } }
		public ZString ContainerTareCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareCaption, Res.GetString("8c8abe55-b7ad-4c48-9d3a-61e37102bdf4", "Tare (kg)")); } }
		public ZString ContainerTareAndUQCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndUQCaption, Res.GetString("93ad3e75-3b86-42d6-8f52-a5623fbce030", "Tare")); } }
		public ZString ContainerGrossCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossCaption, Res.GetString("99337ab7-3481-40a6-bcb2-6a1325db07a0", "Gross (kg)")); } }
		public ZString ContainerGrossAndUQCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndUQCaption, Res.GetString("3a209a34-64be-4dc0-b2ec-ad837d7cc932", "Gross")); } }
		public ZString ContainerVolumeCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeCaption, Res.GetString("1310a826-591a-44b7-bd25-cc24a5994b00", "Volume (M3)")); } }
		public ZString ContainerVolumeAndUQCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndUQCaption, Res.GetString("db761214-2e0c-4a21-8dc1-cc7f936b4067", "Volume")); } }
		public ZString ContainerPackagesCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesCaption, Res.GetString("bf9d3d7c-8555-4686-81a6-94fb6a0346d2", "Packs")); } }
		public ZString ContainerModeCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeCaption, Res.GetString("a1cdca06-e0e5-4bb3-98cf-f5478c58b20e", "Mode")); } }
		public ZString ContainerTemperatureSettingCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingCaption, Res.GetString("d4278bf3-6e48-4c34-b450-97a614ad39a4", "Temp.")); } }
		public ZString ContainerHumiditySettingCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingCaption, Res.GetString("4751679d-f3af-46cf-abdd-2f1ab19edf71", "Humidity")); } }
		public ZString ChargeCodeCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeCaption, Res.GetString("fa492a36-7bc0-411c-8951-ade6c4b883f4", "Code")); } }
		public ZString ChargeCodeDescCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeCodeDescCaption, Res.GetString("FD9B7B82-76D4-4AF5-9115-1F0B1505673C", "Code Description")); } }
		public ZString ChargeDescriptionCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ChargeDescriptionCaption, Res.GetString("bc363d90-7d31-42af-99b6-b7cf1dab0b43", "Charge Description")); } }
		public ZString CollectChargesColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesColumnCaption, Res.GetString("d312bcab-3109-492f-8272-ee9f580d6c7f", "Collect")); } }
		public ZString CollectCurrencyColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.CollectCurrencyColumnCaption, ""); } }
		public ZString PrepaidChargesColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidChargesColumnCaption, Res.GetString("0fb1f5d6-9c06-40d1-bbaa-942d3efc97ac", "Prepaid")); } }
		public ZString PrepaidCurrencyColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PrepaidCurrencyColumnCaption, ""); } }
		public ZString AllChargesColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesColumnCaption, Res.GetString("e5be93b4-cd1f-416d-a24d-74e0e2ef7ca1", "Amount")); } }
		public ZString AllChargesCurrencyColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AllChargesCurrencyColumnCaption, ""); } }
		public ZString PackRefNumberColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberColumnCaption, "VIN/Serial"); } }
		public ZString PackDescriptionColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionColumnCaption, Res.GetString("d946d980-54a6-4391-acc3-597e3973a305", "Description")); } }
		public ZString PackShortDescriptionColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackShortDescriptionColumnCaption, Res.GetString("65da71cf-5265-4ba1-871b-17fb3a8c1468", "Description")); } }
		public ZString PackMarksAndNumbersColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackMarksAndNumbersColumnCaption, Res.GetString("bbaaf132-19d1-4da5-a9a1-e21e1edf5007", "Marks & Numbers")); } }
		public ZString PackContainsUNDGColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackContainsUNDGColumnCaption, Res.GetString("d5f06458-d024-42ff-8809-47e3b2ae5bfc", "UNDG")); } }
		public ZString PackUNDGColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGColumnCaption, Res.GetString("50e7f41b-e6e8-4a3e-a309-b2ab26117d0b", "UNDG")); } }
		public ZString PackDescriptionAndUNDGColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDescriptionAndUNDGColumnCaption, Res.GetString("71beadea-29f6-4d40-9a8a-64564edeac09", "Description")); } }
		public ZString PackUNDGAndShortDescriptionColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackUNDGAndShortDescriptionColumnCaption, Res.GetString("19a6cba0-1067-4abf-942f-674918034031", "Description")); } }
		public ZString PackRefNumberAndMarksAndNumbersColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackRefNumberAndMarksAndNumbersColumnCaption, Res.GetString("15a7439b-bfff-4bc1-bd41-b45fd3571245", "VIN/Serial")); } }
		public ZString PackCommodityCodeColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityCodeColumnCaption, Res.GetString("ac3054e3-6a56-4a2d-9fcb-0b9caca36ff5", "Code")); } }
		public ZString PackCommodityDescriptionColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCommodityDescriptionColumnCaption, Res.GetString("c7138693-cde4-441f-9f72-8d7cbd487872", "Commodity")); } }
		public ZString PackCountColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountColumnCaption, Res.GetString("3ca88c46-b341-495d-9e70-c3ec06591536", "Count")); } }
		public ZString PackCountAndTypeColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackCountAndTypeColumnCaption, Res.GetString("fb29a800-26cb-42f3-a50a-739981584f16", "Packs")); } }
		public ZString PackWeightColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightColumnCaption, Res.GetString("61842c5e-fd4d-4919-a94e-0ba6ed19a16f", "Weight (KG)")); } }
		public ZString PackWeightAndUQColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWeightAndUQColumnCaption, Res.GetString("6b40417d-7a6d-407a-8765-ec0d519fbb50", "Weight")); } }
		public ZString PackVolumeColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeColumnCaption, Res.GetString("f0e8cc98-c393-4bb4-bfe0-89c855e1fd01", "Volume (M3)")); } }
		public ZString PackVolumeAndUQColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVolumeAndUQColumnCaption, Res.GetString("6b6dcc26-05d8-4b7f-a83e-0b5523fdb2c4", "Volume")); } }
		public ZString PackLengthColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthColumnCaption, Res.GetString("dd6cadf2-898c-4ce8-b6bc-8bdc05623f41", "Length (M)")); } }
		public ZString PackLengthAndUQColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackLengthAndUQColumnCaption, Res.GetString("cb040289-6e8d-4b7e-a29e-0203eac879b5", "Length")); } }
		public ZString PackWidthColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthColumnCaption, Res.GetString("07ba966d-8894-4b34-974f-4f8059d1c5ae", "Width (M)")); } }
		public ZString PackWidthAndUQColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackWidthAndUQColumnCaption, Res.GetString("5c7de78a-b11f-4a62-8c88-b23d95be96e9", "Width")); } }
		public ZString PackHeightColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightColumnCaption, Res.GetString("2d00ece8-137d-46b7-bcd2-20031ade1a9b", "Height (M)")); } }
		public ZString PackHeightAndUQColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHeightAndUQColumnCaption, Res.GetString("07876581-317e-4eec-8169-aeef456d28b2", "Height")); } }
		public ZString PackAreaColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackAreaColumnCaption, Res.GetString("0185398c-8172-40aa-9f32-69714a0a3613", "M2")); } }
		public ZString PackDimensionsColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackDimensionsColumnCaption, Res.GetString("ee37ceb5-65cb-406e-bff1-d5b4a7ab8543", "Vol M3 / Dims M")); } }
		public ZString PackHarmonizedCodeColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackHarmonizedCodeColumnCaption, Res.GetString("f3c56104-2eef-40f1-872c-0b6587e8da9e", "Harmonized Code")); } }
		public ZString PackVehicleColourColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleColorColumnCaption, Res.GetString("3d48c35b-4338-4699-a19d-d7612b4efd95", "Color")); } }
		public ZString PackVehicleMakeColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleMakeColumnCaption, Res.GetString("4ae4dae5-23cd-40c8-b83a-1157751ee195", "Make")); } }
		public ZString PackVehicleModelColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleModelColumnCaption, Res.GetString("8b0aafb9-c91e-4b66-887f-9aade9e5aa59", "Model")); } }
		public ZString PackVehicleNumberOfDoorsColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleNumberOfDoorsColumnCaption, Res.GetString("c563651e-0855-454f-871d-f627aa8515cd", "Doors")); } }
		public ZString PackVehicleTransmissionColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleTransmissionColumnCaption, Res.GetString("a4324720-8cf7-4d03-9aa0-a0ef2945ee7f", "Trans.")); } }
		public ZString PackVehicleYearColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleYearColumnCaption, Res.GetString("54afea0b-82ce-47fa-817e-6ebb1525d80a", "Year")); } }
		public ZString PackVehicleFullDetailsColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.PackVehicleFullDetailsColumnCaption, Res.GetString("32a8858d-b735-4b67-818a-c462d030d2fb", "Vehicle Details")); } }
		public ZString BOLClauseColumnCaption { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.BOLClauseColumnCaption, Res.GetString("7327b9cd-3f01-45a5-abe7-3a2332a5b2cc", "BOL Clause")); } }

		public ZInt NumberOfBOLClauseRows { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfBOLClauseRows, 3); } }
		public ZInt NumberOfRORRows { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfRORRows, 6); } }
		public ZInt DimensionsDecimalPlaces { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.DimensionsDecimalPlaces, 3); } }
		public ZInt AreaDecimalPlaces { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.AreaDecimalPlaces, 3); } }

		public ZInt NoOfExtraSectionRows { get { return GetValue(DocumentEngineIntegration.Constants.TemplateDefined.NoOfExtraSectionRows, 6); } }

		internal List<object[]> ExtraSection
		{
			get
			{
				List<object[]> result = new List<object[]>();

				for (int i = 1; i <= GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_Count, 0); i++)
				{
					string heading = GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_Heading + i, "");
					string path = GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_Path + i, "");
					int left = GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_LeftPadding + i, 1);
					int width = GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_Width + i, 20);
					bool hideHeadingWhenNoData = GetValue(DocumentEngineIntegration.Constants.TemplateDefined.ExtraSection_HideHeadingWhenNoData + i, false);

					result.Add(new Object[] { heading, path, left, width, hideHeadingWhenNoData });
				}

				return result;
			}
		}

		ZBool GetValue(string key, ZBool defaultValue)
		{
			return GetTemplateConstantValue(key, defaultValue);
		}

		ZInt GetValue(string key, ZInt defaultValue)
		{
			return GetTemplateConstantValue(key, defaultValue);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching a constant string value")]
		ZString GetValue(string key, ZString defaultValue)
		{
			ZString result = GetTemplateConstantValue(key, defaultValue);
			return result.Equals("<blank>") ? ZString.Empty : result;
		}

		#endregion

		#region Legacy BOL Constants

		public ZBool ShowPacklineCommodityCode
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineCommodityCode, true); }
		}

		public ZBool ShowPacklineCommodityDescription
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineCommodityDescription, false); }
		}

		public ZBool ShowPacklineVolume
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineVolume, false); }
		}

		public ZBool ShowPacklineMarksAndNumbers
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineMarksAndNumbers, false); }
		}

		public ZInt GapBetweenTopAndBottomSection
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GapBetweenTopAndBottomSection, 1); }
		}

		public ZInt ContainerHeadingsAreInFixedPosition
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHeadingsAreInFixedPosition, 0); }
		}

		public ZInt CollectChargesWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.CollectChargesWidth, 1); }
		}

		public ZInt MarksAndNumbersWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 1); }
		}

		public ZInt GoodsDescWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 1); }
		}

		public ZInt PackagesWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.PackagesWidth, 1); }
		}

		public ZInt GrossWeightWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightWidth, 10); }
		}

		public ZInt VolumeMeasurementWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.VolumeMeasurementWidth, 10); }
		}

		public ZInt MarksAndNumbsAndDescHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionHeight, 1); }
		}

		public ZInt MarksAndNumbersHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumberHeight, 1); }
		}

		public ZInt MarksAndNumbersAndDescGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersAndGoodsDescriptionGap, 3); }
		}

		public ZInt GoodsDescriptionAndGrossWeightGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionAndGrossWeightGap, 0); }
		}

		public ZInt GrossWeightAndMeasurementGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GrossWeightAndMeasurementGap, 0); }
		}

		public ZInt GoodsDescriptionHeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionHeight, 1); }
		}

		public ZInt NoOfContainerRows
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfContainerRows, 1); }
		}

		public ZInt NoOfCollectChargesRows
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfCollectChargesRows, 0); }
		}

		public ZInt ChargesDiscriptionWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionWidth, 6); }
		}

		public ZInt IncludePackageCountInBOLGoodsDescription
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.IncludePackageCountInBOLGoodsDescription, 1); }
		}

		public ZInt ShowMasterHeadingWithBillNumber
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowMasterHeadingWithBillNumber, 1); }
		}

		public ZInt ShowPackageCount
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowPackageCount, 0); }
		}

		#region Freight Charges Gap Widths

		public ZInt ChargesDiscriptionAndPrepaidChargesGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndPrepaidChargesGap, 1); }
		}

		public ZInt ChargesDiscriptionAndCollectChargesGap
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargesDiscriptionAndCollectChargesGap, 1); }
		}

		#endregion

		#region Show Container Columns?

		public ZInt ShowContainerSeal
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerSeal, 1); }
		}

		public ZInt ShowContainerType
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerType, 1); }
		}

		public ZInt ShowContainerWeight
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerWeight, 1); }
		}

		public ZInt ShowContainerVolume
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerVolume, 1); }
		}

		public ZInt ShowContainerPackages
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerPackages, 1); }
		}

		public ZInt ShowContainerMode
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerMode, 1); }
		}

		public ZBool ShowContainerTemperatureSetting
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerTemperatureSetting, false); }
		}

		public ZBool ShowContainerHumiditySetting
		{
			get { return GetTemplateConstantValue<ZBool>(DocumentEngineIntegration.Constants.TemplateDefined.ShowContainerHumiditySetting, false); }
		}

		#endregion

		#region Container Column Widths

		ZInt GetTemplateConstantValueWithDefaultAndMin(string key, int min, int defaultValue)
		{
			int result = GetTemplateConstantValue<ZInt>(key, defaultValue);
			return result >= min ? result : defaultValue;
		}

		public ZInt ContainerNumberWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberWidth, 11, 15); }
		}

		public ZInt ContainerSealWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealWidth, 4, 20); }
		}

		public ZInt ContainerTypeWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeWidth, 8, 8); }
		}

		public ZInt ContainerWeightWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightWidth, 10, 14); }
		}

		public ZInt ContainerTareWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareWidth, 10, 10); }
		}

		public ZInt ContainerGrossWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossWidth, 10, 10); }
		}

		public ZInt ContainerVolumeWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeWidth, 10, 14); }
		}

		public ZInt ContainerPackagesWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesWidth, 8, 12); }
		}

		public ZInt ContainerModeWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerModeWidth, 4, 10); }
		}

		public ZInt ContainerTemperatureSettingWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingWidth, 5, 5); }
		}

		public ZInt ContainerHumiditySettingWidth
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingWidth, 8, 8); }
		}

		#endregion

		#region Container Gap Widths

		public ZInt ContainerNumberAndSealGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerNumberAndSealGap, 2, 1); }
		}

		public ZInt ContainerSealAndTypeGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerSealAndTypeGap, 2, 1); }
		}

		public ZInt ContainerTypeAndWeightGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTypeAndWeightGap, 2, 1); }
		}

		public ZInt ContainerWeightAndTareGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndTareGap, 2, 1); }
		}

		public ZInt ContainerTareAndGrossGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTareAndGrossGap, 2, 1); }
		}

		public ZInt ContainerGrossAndVolumeGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerGrossAndVolumeGap, 2, 1); }
		}

		public ZInt ContainerWeightAndVolumeGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerWeightAndVolumeGap, 2, 1); }
		}

		public ZInt ContainerVolumeAndPackagesGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerVolumeAndPackagesGap, 2, 1); }
		}

		public ZInt ContainerPackagesAndModeGap
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerPackagesAndModeGap, 2, 1); }
		}

		public ZInt ContainerTemperatureSettingLeftPadding
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerTemperatureSettingLeftPadding, 0, 1); }
		}

		public ZInt ContainerHumiditySettingLeftPadding
		{
			get { return GetTemplateConstantValueWithDefaultAndMin(DocumentEngineIntegration.Constants.TemplateDefined.ContainerHumiditySettingLeftPadding, 0, 1); }
		}

		#endregion

		public ZInt MaxLineLength
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.MaxLineLength, 112); }
		}

		#region ShowAdditionalSeals

		public ZInt ShowAdditionalSeals
		{
			get { return fShowAdditionalSeals; }
			set { fShowAdditionalSeals = value; }
		}

		ZInt fShowAdditionalSeals = 0;

		#endregion

		#endregion

		public ZInt NoOfTransportPlanningRows
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.NumberOfTransportPlanningRows, 1); }
		}

		#endregion

		#region Commodity And UNDG

		public ZString CommodityCodes
		{
			get { return Commodity.Code; }
		}

		public DocCommodityCollection Commodity
		{
			get
			{
				DocCommodityCollection coll = new DocCommodityCollection(CommonShipment.Factory);
				foreach (DocPackLines lines in OuterPackLineCollection)
				{
					if (lines.Commodity != null)
					{
						coll.Add(lines.Commodity);
					}
				}
				return coll;
			}
		}

		public ZString HazCat
		{
			get
			{
				ZString result = "";
				ArrayList uNDGCodes = new ArrayList();
				foreach (DocPackLines line in OuterPackLineCollection)
				{
					if (line.IsHazardous)
					{
						foreach (UNDGSubstanceWrapper dgItem in line.UNDGs)
						{
							if (!uNDGCodes.Contains(dgItem.UNNumber))
							{
								uNDGCodes.Add(dgItem.UNNumber);

								result += result.IsEmpty ? ZString.Empty : new ZString("; ");
								result += line.GetHazardousDescriptionWithoutWeight(dgItem);
							}
						}
					}
				}

				return result;
			}
		}

		public ZString CommodityAndHazCat
		{
			get
			{
				ZString coms = "";
				ZString hazes = "";
				List<ZString> uniqueCommodities = new List<ZString>();

				foreach (DocPackLines line in OuterPackLineCollection)
				{
					ZString com = line.Commodity != null ? line.Commodity.Code.ToString() : "";

					if (!com.IsEmpty)
					{
						if (line.UNDGs.Length > 0)
						{
							foreach (UNDGSubstanceWrapper undg in line.UNDGs)
							{
								ZString undgCode = undg.UNNumber;
								if (!uniqueCommodities.Contains(com + undgCode))
								{
									uniqueCommodities.Add(com + undgCode);
									hazes += hazes.IsEmpty ? "" : "; ";
									hazes += line.GetHazardousDescriptionWithoutWeight(undg);
								}
							}
						}
						else
						{
							if (!uniqueCommodities.Contains(com))
							{
								uniqueCommodities.Add(com);
								coms += coms.IsEmpty ? "" : "; ";
								coms += line.Commodity.Code + "-" + line.Commodity.Description;
							}
						}
					}
				}

				ZString sep = !coms.IsEmpty && !hazes.IsEmpty ? "; " : "";
				return coms + sep + hazes;
			}
		}

		public ZString CommodityDescriptionWithHeading
		{
			get
			{
				ZString result = ZString.Empty;
				if (!Commodity.Description.IsEmpty)
				{
					result = Res.GetString("b9f87930-9f9f-4551-b291-7f387bf4533f", "Type: {0}", Commodity.Description);
					if (Commodity.ContainsHazardous)
					{
						result += "  " + HazCatCodeWithIMOHeading;
					}
				}
				return result;
			}
		}

		public ZString HazCatCodeWithIMOHeading
		{
			get { return (!HazCat.IsEmpty) ? Res.GetString("8e41061b-006a-4813-a459-19b967b3ff6d", "IMO Code: {0}", HazCat) : ""; }
		}

		#endregion

		#region ZString Fields

		public virtual ZString OrderNumbers
		{
			get { return ""; }
		}

		public ZString HAWBTerms
		{
			get { return IsDomestic ? DocumentsDataRegistry.Instance.DomesticHAWBTerms.Value : DocumentsDataRegistry.Instance.BookingInternationalHAWBTerms.Value; }
		}

		public ZString CompanyCode
		{
			get { return GlbCompany.CurrentCompany.GC_Code; }
		}

		public ZString CountryCode
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; }
		}

		public ZString ShippersRef
		{
			get { return CommonShipment.JS_BookingReference; }
		}

		public ZString PackLineCustomAttributes
		{
			get
			{
				ZString result = "";
				foreach (DocPackLines line in OuterPackLineCollection)
				{
					if (!line.CustomAttribute1.IsEmpty
							|| !line.CustomAttribute2.IsEmpty
							|| !line.CustomAttribute3.IsEmpty
							|| !line.CustomAttribute4.IsEmpty)
					{
						if (!line.CustomAttribute1.IsEmpty)
						{
							result = line.CustomAttribute1;
						}

						if (!line.CustomAttribute2.IsEmpty)
						{
							result += result.IsEmpty ? line.CustomAttribute2.ToString() : "\n" + line.CustomAttribute2;
						}

						if (!line.CustomAttribute3.IsEmpty)
						{
							result += result.IsEmpty ? line.CustomAttribute3.ToString() : "\n" + line.CustomAttribute3;
						}

						if (!line.CustomAttribute4.IsEmpty)
						{
							result += result.IsEmpty ? line.CustomAttribute4.ToString() : "\n" + line.CustomAttribute4;
						}

						break;
					}
				}
				return result;
			}
		}

		public ZString Context
		{
			get { return "SHIPMENT"; }
		}

		public ZString DisbursementNoteTitle
		{
			get { return Env.Registry.DisbursementNoteTitle.Trim().ToUpper(); }
		}

		public ZString ContainerPackModeOverride
		{
			get { return CommonShipment.JS_HBLContainerPackModeOverride; }
		}

		public ZString HouseBillOfLadingType
		{
			get { return CommonShipment.JS_HouseBillOfLadingType; }
		}

		public virtual ZString ReceivalDepotReference
		{
			get
			{
				if (Consol != null)
				{
					return Consol.BookingReference;
				}
				else if (!CommonShipment.JS_CFSReference.IsEmpty)
				{
					return CommonShipment.JS_CFSReference;
				}
				else
				{
					return (Sailing != null) ? Sailing.ReservedMasterBill : ZString.Empty;
				}
			}
		}

		#region OnBoard

		public ZString OnBoard => GetOnBoard(false);

		public ZString OnBoardUntranslated => GetOnBoard(true);

		ZString GetOnBoard(bool isTranslatableDocument)
		{
			if (!ShippedOnBoard.IsEmpty)
			{
				ZString onBoardDesc = new ZString(CommonShipment.Lookups.JS_ShippedOnBoard_List.GetDescriptionFromCode(ShippedOnBoard));
				if (!ShippedOnBoard.Equals("RFS"))
				{
					return isTranslatableDocument ? onBoardDesc.ToUpper() + (NoResString)" ON BOARD"
						: onBoardDesc.ToUpper() + " " + Res.GetString("6995d986-3b5b-4ff1-9c03-d6a3ac77553c", "ON BOARD");
				}
				else
				{
					return onBoardDesc.ToUpper();
				}
			}
			return ZString.Empty;
		}

		#endregion

		public ZString OuterPacksDescription
		{
			get { return SequenceNumber.ToString() + " " + Res.GetString("e3724f01-a2a3-4aae-a497-a5daf9061aa9", "of {0}", CommonShipment.JS_OuterPacks); }
		}

		public ZString CHGSCode
		{
			get { return PrepaidCollectCode.IsEmpty ? ZString.Empty : PrepaidCollect.Substring(0, 2); }
		}

		public ZString DeclaredValueForCustoms
		{
			get
			{
				if (!GoodsValue.IsEmpty)
				{
					return GoodsValue.ToString();
				}

				return "NCV";
			}
		}

		public ZString ThreeLetterOriginPort
		{
			get { return CommonShipment.Origin != null ? OriginLoco.PortName.Substring(0, 3) : ZString.Empty; }
		}

		public ZString PortCode3Char
		{
			get { return (CommonShipment.Origin != null) ? OriginLoco.Code.Substring(2) : ZString.Empty; }
		}

		public ZString FreightCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZString City
		{
			get { return GlbBranch.CurrentBranch.GB_City; }
		}

		public ZString FullNameOfUser
		{
			get { return GlbStaff.CurrentUser.GS_FullName; }
		}

		public ZString Signature
		{
			get { return GlbStaff.CurrentUser.GS_FullName + " " + GlbStaff.CurrentUser.DangerousGoodsCertificateNumber; }
		}

		public ZString DescriptionForGoods
		{
			get { return DetailedDescriptionOfGoods; }
		}

		public ZString HBLShipmentConsolMode
		{
			get
			{
				ZString result = ZString.Empty;

				if (PackingMode == Core.Constants.ContainerModes.FCL)
				{
					result = "CY/CY";
				}
				else if (PackingMode == Core.Constants.ContainerModes.BuyersConsol)
				{
					result = "CFS/CY";
				}
				else if (PackingMode == Core.Constants.ContainerModes.LCL)
				{
					result = "CFS/CFS";
				}

				return result;
			}
		}

		public ZString Packages
		{
			get
			{
				ZString innerPack = ZString.Empty;
				ZString packageDetails = ZString.Empty;

				if (!OuterPacks.IsEmpty)
				{
					packageDetails = OuterPacks + " " + OuterPacksPackType + " (OUTER)";
				}

				if (!InnerPacks.IsEmpty)
				{
					innerPack = InnerPacks + " " + InnerPacksPackType + " (INNER)";
				}

				if (innerPack != ZString.Empty)
				{
					if (packageDetails != ZString.Empty)
					{
						packageDetails += ", " + innerPack;
					}
					else
					{
						packageDetails = innerPack;
					}
				}

				return packageDetails;
			}
		}

		public ZString InvoiceNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocOrder order in Orders)
				{
					result += order.InvoiceNumber.IsEmpty ? "" : order.InvoiceNumber + ", ";
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString HXDNumber
		{
			get { return CommonShipment.DocsAndCartage.JP_Calc_HXDNumber; }
		}

		public ZString Origin
		{
			get
			{
				if (OriginLoco != null)
				{
					return OriginLoco.Code + " = " + OriginLoco.PortName + ", " + OriginLoco.CountryName;
				}

				return ZString.Empty;
			}
		}

		public ZString Destination
		{
			get
			{
				if (DestinationLoco != null)
				{
					return DestinationLoco.Code + " = " + DestinationLoco.PortName + ", " + DestinationLoco.CountryName;
				}

				return ZString.Empty;
			}
		}

		public ZString ShipmentContainerNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocFreightBaseContainer currentContainer in Containers)
				{
					result += currentContainer.ContainerNumber + ", ";
				}

				if (result != ZString.Empty)
				{
					result = result.Substring(0, result.Length - 2);
				}

				return result;
			}
		}

		public ZString OuterPacksDetail
		{
			get
			{
				ZString result = GetOuterPacksDetail(Consol);
				if (ReportName.Contains((NoResString)"Interim Receipt") && !PrintContainerDetailsOnInterimReceipt)
				{
					result = ZString.Empty;
				}

				return result;
			}
		}

		public ZString FreightChargeType
		{
			get
			{
				if (PrepaidCollect.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (PrepaidCollect == Res.GetString("155e235c-6914-4503-8ebf-fa3903837970", "Prepaid"))
				{
					return Res.GetString("5a8cfeb9-a19b-4d57-bf47-81e8993e1c97", "FREIGHT PREPAID");
				}
				else
				{
					return Res.GetString("126f9512-3fe4-413a-ac39-266d0931f239", "FREIGHT COLLECT");
				}
			}
		}

		public ZString PlaceOfReceipt
		{
			get
			{
				return OriginLoco != null ? OriginLoco.PortName : ZString.Empty;
			}
		}

		public ZString PlaceOfDelivery
		{
			get
			{
				return DestinationLoco != null ? DestinationLoco.PortName : ZString.Empty;
			}
		}

		public ZString PrepaidCollect
		{
			get
			{
				if (!PrepaidCollectCode.IsEmpty)
				{
					return PrepaidCollectCode == Constants.PaymentType.Prepaid ? Res.GetString("c87e5868-34c4-4d5c-9d83-f2a088768397", "Prepaid") : Res.GetString("d400622a-f128-46d0-9b42-292c609315d3", "Collect");
				}

				return ZString.Empty;
			}
		}

		public ZString UltimateNotification
		{
			get
			{
				ZString result = ZString.Empty;
				var coloadMasterShipment = ColoadMasterShipment;
				var consignee = coloadMasterShipment == null ? null : coloadMasterShipment.Consignee;
				var consigneeMiscServ = consignee == null ? null : consignee.MiscServ;
				if (consigneeMiscServ != null && !consigneeMiscServ.FWDealDirectlyWithUltimates)
				{
					result = Res.GetString("945d0eb7-3ede-432d-8ce8-18c8d0cdd9eb", "ULTIMATE") + " ";
				}

				return result;
			}
		}

		public ZString FreightPayableAt
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsPrepaid)
				{
					if (OriginLoco != null)
					{
						result = OriginLoco.PortName;
					}
				}
				else if (IsCollect)
				{
					if (DestinationLoco != null)
					{
						result = DestinationLoco.PortName;
					}
				}

				return result;
			}
		}

		public ZString ReleaseType
		{
			get
			{
				ZString result = "";

				if (!ReleaseTypeCode.IsEmpty)
				{
					result = CommonShipment.Lookups.JS_ReleaseType_List.GetDescriptionFromCode(ReleaseTypeCode);
				}

				return result;
			}
		}

		public ZString AlertText
		{
			get { return DocumentsDataRegistry.Instance.ShipmentDelayAlertAlertText.Value; }
		}

		public ZString ReleaseTypeWithHeading
		{
			get
			{
				if (!ReleaseTypeCode.IsEmpty)
				{
					return Res.GetString("855347d9-b165-404e-ba9b-8f84c32931b9", "Release:") + " " + CommonShipment.Lookups.JS_ReleaseType_List.GetDescriptionFromCode(ReleaseTypeCode);
				}

				return ZString.Empty;
			}
		}

		public ZString CurrencyCode
		{
			get
			{
				if (GoodsCurr != null)
				{
					return GoodsCurr.Code;
				}

				return ZString.Empty;
			}
		}

		public ZString FirstLetterWeightUnit
		{
			get { return UnitOfWeight.IsEmpty ? "K" : UnitOfWeight.ToString(); }
		}

		public ZString SCACValueForManifest
		{
			get
			{
				if (IsSeaConsol && IsExportConsol && IsUSConsol)
				{
					ZString sCACValue = ConsignorSCAC;

					if (sCACValue != ZString.Empty)
					{
						return sCACValue;
					}
					else
					{
						return Res.GetString("1e0eac81-1719-45f9-9e40-d5d99d06b588", "*MISSING*");
					}
				}

				return ZString.Empty;
			}
		}

		public ZString ConsignorSCAC
		{
			get
			{
				if (CommonShipment.Consignor != null)
				{
					return Consignor.SCAC;
				}

				return ZString.Empty;
			}
		}

		public virtual ZString ETAString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETA); }
		}

		public ZString OuterPackLineTotalPacks
		{
			get
			{
				if (CommonShipment.OuterPackLines != null)
				{
					return CommonShipment.TotalOuterPacks.ToString();
				}

				return ZString.Empty;
			}
		}

		public ZString OuterPackLineTotalWeight
		{
			get
			{
				if (CommonShipment.OuterPackLines != null)
				{
					return CommonShipment.TotalOuterPacksWeight.ToString();
				}

				return ZString.Empty;
			}
		}

		public ZString OuterPackLineTotalWeightInTonnes
		{
			get
			{
				if (CommonShipment.OuterPackLines != null)
				{
					return FormatNumber(Enterprise.Core.Constants.Weight.ConvertSafe(CommonShipment.TotalOuterPacksWeight, Env.Registry.FreightWeightUnit, Core.Constants.Weight.Tonnes)).ToString();
				}

				return ZString.Empty;
			}
		}

		public ZString OuterPackLineTotalVolume
		{
			get
			{
				if (CommonShipment.OuterPackLines != null)
				{
					return CommonShipment.TotalOuterPacksVolume.ToString();
				}

				return ZString.Empty;
			}
		}

		public ZString OuterPackLineComments
		{
			get
			{
				ZString result = ZString.Empty;

				if (CommonShipment.TotalOuterPacksPillaged != 0)
				{
					result = Res.GetString("5723f95d-163d-4371-9ef7-6971ef14bcef", "Total Pillaged:") + " " + CommonShipment.TotalOuterPacksPillaged.ToString() + "\r\n";
				}

				if (CommonShipment.TotalOuterPacksDamaged != 0)
				{
					result += Res.GetString("58f1068e-b6c0-494a-a055-93c6397a20b2", "Total Damaged:") + " " + CommonShipment.TotalOuterPacksDamaged.ToString() + "\r\n";
				}

				foreach (PackLine pack in CommonShipment.OuterPackLines)
				{
					if (!pack.JL_OutturnComment.IsEmpty)
					{
						result += pack.JL_OutturnComment + " ";
					}
				}

				return result;
			}
		}

		public ZString ContainerOuterPackLineComments
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocPackLines pack in OuterPackLineCollection)
				{
					result += pack.OutturnComments + "\n";
				}
				result = result.TrimEnd();
				return result;
			}
		}

		public ZString ContainerOuterPackLineDimensionAndComments
		{
			get
			{
				ZString result = OuterPackLineDimensions;
				if (!result.IsEmpty)
				{
					result += "\n\n";
				}

				result += ContainerOuterPackLineComments;
				return result.TrimEnd();
			}
		}

		public ZString OuterPackLineDimensions
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocPackLines line in OuterPackLineCollection)
				{
					if (!line.Dimensions.IsEmpty)
					{
						result += line.Dimensions + "\n";
					}
				}
				return result.TrimEnd();
			}
		}

		public ZString OuterPackLineCountAndDimensions
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocPackLines line in OuterPackLineCollection)
				{
					if (line.PackageCount > 0 || !line.Dimensions.IsEmpty)
					{
						if (!line.Dimensions.IsEmpty)
						{
							result += line.Dimensions + " X ";
						}

						result += line.PackageCount.ToString() + " " + line.PackType + "\n";
					}
				}

				return result.TrimEnd();
			}
		}

		public ZString StatementFooterAddress
		{
			get
			{
				ZString result = ZString.Empty;

				if (CurrentBranch != null && CurrentBranch.MailToAddress != null)
				{
					result = BrandName + "\n"
							+ CurrentBranch.MailToAddress.Address1 + ", " + CurrentBranch.MailToAddress.Address2 + "\n"
							+ CurrentBranch.MailToAddress.City + " " + CurrentBranch.MailToAddress.State + " "
							+ CurrentBranch.MailToAddress.PostCode + " " + CurrentBranch.MailToAddress.Country;
				}

				return result;
			}
		}

		public ZString ETDString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETD); }
		}

		ZString ATDString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, DocsAndCartage.PickupCartageCompleted); }
		}

		public ZString NotifyPartyPostalAddress
		{
			get { return (NotifyParty != null) ? NotifyParty.PostalAddress : ZString.Empty; }
		}

		public ZString ImportBrokerName
		{
			get { return (ImportBroker != null) ? ImportBroker.Name : ZString.Empty; }
		}

		public ZString HBLContainerPackModeOverride
		{
			get { return CommonShipment.JS_HBLContainerPackModeOverride; }
		}

		public ZString CustomsEntryNumberType
		{
			get { return CommonShipment.CustomsEntryNumberType; }
		}

		public ZString CustomsEntryNumber
		{
			get { return CommonShipment.CustomsEntryNumber; }
		}

		public ZString UniqueConsignmentReference
		{
			get
			{
				BaseJobDeclaration dec = ((BaseJobDeclaration)CommonShipment.DeclarationForDocuments);
				return dec?.CustomsEntryHeaders?.FirstOrDefault()?.CH_BGMReference ?? ZString.Empty;
			}
		}

		public ZString ShippedOnBoard
		{
			get { return CommonShipment.JS_ShippedOnBoard; }
		}

		public ZString TotalOuterPackLineWeightUnit
		{
			get { return CommonShipment.TotalPackLineWeightUnit; }
		}

		public ZString TotalOuterPackLineVolumeUnit
		{
			get { return CommonShipment.TotalPackLineVolumeUnit; }
		}

		public ZString InnerPacksPackType
		{
			get { return CommonShipment.JS_F3_NKTotalCountPackType; }
		}

		public ZString OutturnComments
		{
			get { return CommonShipment.OutturnComments; }
		}

		public ZString WeightUnit
		{
			get { return UnitOfWeight; }
		}

		public ZString VolumeUnit
		{
			get { return UnitOfVolume; }
		}

		public ZString ChargeableUnit
		{
			get { return CommonShipment.JS_ChargeableUnit; }
		}

		public ZString NotifyPartyDisplay
		{
			get { return CommonShipment.JS_Calc_NotifyPartyDisplay; }
		}

		public virtual ZString EquipmentType
		{
			get
			{
				ZString result = ZString.Empty;
				if (IsExportDocument)
				{
					result = CommonShipment.DocsAndCartage.JP_FCLPickupEquipmentNeeded;
					if (!result.IsEmpty)
					{
						result += " - " + CommonShipment.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(result);
					}
				}
				else if (IsImportDocument)
				{
					result = CommonShipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded;
					if (!result.IsEmpty)
					{
						result += " - " + CommonShipment.DocsAndCartage.Lookups.DeliveryEquipmentNeededList.GetDescriptionFromCode(result);
					}
				}

				return result;
			}
		}

		public virtual ZString BookingReference
		{
			get { return CommonShipment.JS_BookingReference; }
		}

		public virtual ZString CarrierBookingRef
		{
			get { return (IsExportDocument && Consol != null) ? Consol.BookingReference : ZString.Empty; }
		}

		public ZString CFSReference
		{
			get { return CommonShipment.JS_CFSReference; }
		}

		public ZString ConsolReference
		{
			get { return CommonShipment.JS_ConsolReference; }
		}

		public ZString GoodsDescription
		{
			get { return CommonShipment.JS_GoodsDescription; }
		}

		public ZString ShipmentNoHouseBillNoAndPorts
		{
			get
			{
				ZString result = Res.GetString("24a2431a-623a-4fb8-aef2-0b115092f00f", "REF: {0}", ShipmentNumber);

				if (ShipmentNumber != HouseBill && !HouseBill.IsEmpty)
				{
					result += ", " + HouseBill;
				}

				ZString originCode = OriginLoco != null ? OriginLoco.Code : ZString.Empty;
				ZString destinationCode = DestinationLoco != null ? DestinationLoco.Code : ZString.Empty;

				result += " (" + originCode + "/" + destinationCode + ")";

				return result;
			}
		}

		public ZString ClientOwnerOrderReferenceHeading
		{
			get
			{
				ZString result = Res.GetString("6d8c783e-40c2-41ed-ac00-273dfdd092d8", "CLIENT") + " / ";

				if (DeclarationForShipmentBranch != null)
				{
					result += Res.GetString("5357daf3-32b3-4e60-b546-b444a9b233c4", "OWNER") + " / ";
				}

				result += Res.GetString("d4df1e2e-1fd5-4200-9ea0-b191a60bf27d", "ORDER REFERENCE");

				return result;
			}
		}

		public ZString ClientOwnerOrderReference
		{
			get
			{
				ZString result = ShippersReference + " / ";

				if (DeclarationForShipmentBranch != null && !OrderNumbers.IsEmpty && !DeclarationForShipmentBranch.OwnerRef.IsEmpty)
				{
					if (OrderNumbers.ToUpper() != DeclarationForShipmentBranch.OwnerRef.ToUpper().Replace(", ", ","))
					{
						result += DeclarationForShipmentBranch.OwnerRef + " / ";
					}
				}

				result += OrderNumbers;

				return result;
			}
		}

		public ZString HouseBill
		{
			get { return CommonShipment.JS_HouseBill; }
		}

		public ZString InspectionType
		{
			get { return CommonShipment.JS_InspectionTypeCode; }
		}

		public ZString INCO
		{
			get { return Constants.IncoTerms.GetMappedOfficialIncoterm(CommonShipment.JS_INCO); }
		}

		public ZString INCODescription
		{
			get { return CommonShipment.Lookups.JS_INCO_List.GetDescriptionFromCode(INCO); }
		}

		public ZString INCOEnglishDescription
		{
			get
			{
				var multilingualDescription = CommonShipment.Lookups.JS_INCO_List.GetMultilingualDescriptionFromCode(CommonShipment.JS_INCO);
				return multilingualDescription == null ? string.Empty : multilingualDescription.GetUnresolvedString();
			}
		}

		public ZString AdditionalTerms
		{
			get { return CommonShipment.JS_AdditionalTerms; }
		}

		public ZString PaymentTerm
		{
			get { return CommonShipment.JS_PaymentTerm; }
		}

		public ZString PaymentTermDisplay
		{
			get { return CommonShipment.JS_PaymentTermDisplay; }
		}

		public ZString InterimReceipt
		{
			get { return CommonShipment.JS_InterimReceipt; }
		}

		public ZString OuterPacksPackType
		{
			get { return CommonShipment.JS_F3_NKPackType; }
		}

		public ZString OuterPacksPackTypeDescription
		{
			get
			{
				var type = CommonShipment.Lookups.JS_PackType_List.GetDescriptionFromCode(OuterPacksPackType);
				return !string.IsNullOrEmpty((string)type) ? type.ToString() + Res.GetString("c0af9134-1b37-4ef2-8feb-bfeaed4fe8f2", "(s)") : "";
			}
		}

		public ZString PackingMode
		{
			get { return CommonShipment.JS_PackingMode; }
		}

		public ZString PrepaidCollectCode
		{
			get { return CommonShipment.JS_PaymentTerm; }
		}

		public ZString ReleaseTypeCode
		{
			get { return CommonShipment.JS_ReleaseType; }
		}

		public ZString ServiceLevelCode
		{
			get { return CommonShipment.ServiceLevel != null ? CommonShipment.ServiceLevel.RS_Code : ZString.Empty; }
		}

		public ZString ServiceLevelDesc
		{
			get { return CommonShipment.ServiceLevel != null ? CommonShipment.ServiceLevel.RS_DescriptionMultilingual : ZString.Empty; }
		}

		public ZString ShipmentStatus
		{
			get { return CommonShipment.JS_ShipmentStatus; }
		}

		public ZString TransportMode
		{
			get { return CommonShipment.JS_TransportMode; }
		}

		public ZString TransportModeAndPackingMode
		{
			get
			{
				ZString result = "\r\n" + Res.GetString("87cba1b5-e8c5-441f-a6b9-5afe15e13b98", "Transport Mode {0}    Container Mode {1}", TransportModeDescription, CommonShipment.JS_PackingMode);

				if (Consol != null && IsExportDocument && Consol.IsDirect)
				{
					return ZString.Empty;
				}

				return result;
			}
		}

		public ZString TransportModeDescription
		{
			get { return CommonShipment.Lookups.JS_TransportMode_List.GetDescriptionFromCode(TransportMode); }
		}

		public ZString InitialTransportMode
		{
			get
			{
				if (Consol != null)
				{
					if (Consol.TransportPlanningCount > 0)
					{
						return Consol.TransportPlanning[0].TransportMode;
					}
				}
				return String.Empty;
			}
		}

		public ZString InitialTransportModeDescription
		{
			get
			{
				ZString mode = InitialTransportMode;

				switch (mode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("56dafa76-f774-4bcb-b997-5e5c6300b9bd", "Air Freight");
					case Core.Constants.TransportModes.Rail:
						return Res.GetString("a4b0f019-4d81-488f-89c3-1a42750f49c7", "Rail Freight");
					case Core.Constants.TransportModes.Road:
						return Res.GetString("e0ca0f2d-ec87-451d-89d6-4e3294a23d79", "Road Freight");
					case Core.Constants.TransportModes.Sea:
						return Res.GetString("c50512cc-1b46-47fa-956c-49f5d5c49ec8", "Sea Freight");
					case Core.Constants.TransportModes.Storage:
						return Res.GetString("fe8a6229-2c72-46cf-94f2-a828b9113484", "Storage");
					default:
						return mode;
				}
			}
		}

		public ZString ShipmentNumber
		{
			get { return CommonShipment.JS_UniqueConsignRef; }
		}

		public ZString UnitOfVolume
		{
			get { return CommonShipment.JS_UnitOfVolume; }
		}

		public ZString UnitOfWeight
		{
			get { return CommonShipment.JS_UnitOfWeight; }
		}

		public ZString ChargesDisplay
		{
			get { return CommonShipment.JS_HBLAWBChargesDisplay; }
		}

		public ZString GoodsDescriptionForManifest
		{
			get
			{
				if (ReportName.StartsWith((NoResString)"Manifest Detailed") || ReportName.StartsWith((NoResString)"Manifest Freighted"))
				{
					return ManifestDetailedDescriptionOfGoods;
				}
				return ManifestDescriptionOfGoods;
			}
		}

		public ZString ActualRCVString
		{
			get
			{
				if (ActualRCV != ZDateTime.Empty)
				{
					return (ActualRCV.ToShortDateString() + " " + ActualRCV.ToShortTimeString());
				}
				return "";
			}
		}

		public ZString InterimAndDateReceived
		{
			get
			{
				ZString result = "";
				if (InterimReceipt != "" && ActualRCVString != "")
				{
					result = (Res.GetString("c620e756-e61a-40dc-8abf-a30268633282", "Interim: {0}\r\nReceived: {1}", InterimReceipt, ActualRCVString));
				}
				else if (InterimReceipt != "" && ActualRCVString == "")
				{
					result = (Res.GetString("e0961d27-6e11-4c1c-a0c4-cf0a80b50719", "Interim: {0}", InterimReceipt));
				}
				else if (InterimReceipt == "" && ActualRCVString != "")
				{
					result = (Res.GetString("19ca8a31-5244-4cb2-832d-e086cebe2cdd", "Received: {0}", ActualRCVString));
				}

				return result;
			}
		}

		public ZString ContainerDeliveryMode
		{
			get { return Containers.Count > 0 ? Containers[0].DeliveryMode : ZString.Empty; }
		}

		public ZString AgentNotes
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.AgentNotes.Description, CommonShipment); }
		}

		public ZString OwnerRefAndOrderRef
		{
			get
			{
				ZString result = ZString.Empty;

				if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.IsImportMessage
						&& !DeclarationForCurrentBranch.OwnerRef.IsEmpty && !OrderNumbers.Contains(DeclarationForCurrentBranch.OwnerRef))
				{
					result += DeclarationForCurrentBranch.OwnerRef + " ";
				}

				result += OrderNumbers.Replace("\n", " ");

				return result;
			}
		}

		public ZString FreightCODAmountWithCurrency
		{
			get { return FreightCODAmount.Round(2) + " " + Env.CurrentCompany.LocalCurrency.Code; }
		}

		public ZString WarehouseLocation
		{
			get { return CommonShipment.JS_IsForwardRegistered ? ZString.Empty : CommonShipment.JS_WarehouseLocation; }
		}

		#region Headings

		public ZString JobNumberHeading
		{
			get { return Res.GetString("b1bd7c63-66c6-4441-9377-45a81643f2c1", "SHIPMENT"); }
		}

		public ZString HeadingTransportMode
		{
			get
			{
				ZString heading = ZString.Empty;

				if (TransportMode == Core.Constants.TransportModes.Sea && (PackingMode == Core.Constants.ContainerModes.FCL || PackingMode == Core.Constants.ContainerModes.LCL))
				{
					heading += PackingMode + " ";
				}

				heading += TransportModeDescription.Trim();

				string wordToRemove = Res.GetString("6383cd38-dc7a-4f54-be08-542fd0b8b14b", "Freight");
				if (heading.EndsWith(wordToRemove))
				{
					heading = heading.ToString().Remove(heading.LastIndexOf(wordToRemove), wordToRemove.Length);
				}

				return heading.Trim();
			}
		}

		public ZString ColoadMasterHouseBillHeading
		{
			get { return (ColoadShipments.Count > 0) ? new ZString(Res.GetString("6f6556b7-6428-4236-8744-a3d642de589a", "MASTER HOUSE")) : HouseBillHeading; }
		}

		public ZString ColoadMasterHouseBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(ColoadMasterHouseBillHeading, HouseBillIssueDate); }
		}

		public virtual ZString HouseBillHeading
		{
			get
			{
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("09ba6e1f-6641-488c-a82f-231a0b86d4bf", "HAWB");

					case Core.Constants.TransportModes.Sea:
						return Res.GetString("0edb79ef-67a0-4ec2-8e11-f2665cafdba3", "HOUSE BILL OF LADING");

					default:
						return Res.GetString("92b8233e-2592-4393-81d6-e941c40245c5", "HOUSE");
				}
			}
		}

		public ZString HouseBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(HouseBillHeading, HouseBillIssueDate); }
		}

		public ZString ContainerWeightHeading
		{
			get
			{
				foreach (IDocContainer container in Containers)
				{
					if (container.TotalAllocatedShipmentWeight != 0M)
					{
						return Res.GetString("9d367b39-b202-4542-9efb-357a58a97831", "WEIGHT");
					}
				}

				if (!NotAllocatedWeight.IsEmpty)
				{
					return Res.GetString("9d367b39-b202-4542-9efb-357a58a97831", "WEIGHT");
				}

				return "";
			}
		}

		public ZString ContainerVolumeHeading
		{
			get
			{
				foreach (IDocContainer container in Containers)
				{
					if (container.TotalAllocatedShipmentVolume != 0M)
					{
						return Res.GetString("e918ed2e-b9e2-4434-853f-3f1bdb152a7e", "VOLUME");
					}
				}

				if (!NotAllocatedVolume.IsEmpty)
				{
					return Res.GetString("e918ed2e-b9e2-4434-853f-3f1bdb152a7e", "VOLUME");
				}

				return "";
			}
		}

		public ZString ContainerPackageHeading
		{
			get
			{
				foreach (IDocContainer container in Containers)
				{
					if (container.TotalAllocatedShipmentPackages != 0)
					{
						return Res.GetString("53f89a0b-0817-4ee3-b123-e120634165b9", "PACKS");
					}
				}

				if (!NotAllocatedPackages.IsEmpty)
				{
					return Res.GetString("53f89a0b-0817-4ee3-b123-e120634165b9", "PACKS");
				}

				return "";
			}
		}

		public ZString ReturnEmptyToHeading
		{
			get
			{
				if (PackingMode == Core.Constants.ContainerModes.FCL || (Consol != null && Consol.ConsolMode == Core.Constants.ContainerModes.BuyersConsol))
				{
					return Res.GetString("5d2198af-ef0a-4dcc-ab91-6b00484da629", "RETURN EMPTY TO");
				}
				return ZString.Empty;
			}
		}

		public ZString ECNHeading
		{
			get
			{
				if (ECN != ZString.Empty)
				{
					return "ECN: ";
				}
				return ZString.Empty;
			}
		}

		public ZString CusEntryNumHeadingForManifest
		{
			get
			{
				if (!CommonShipment.CustomsEntryNumber.IsEmpty)
				{
					return CommonShipment.CustomsEntryNumberType + ":";
				}
				return ZString.Empty;
			}
		}

		public ZString CusEntryNumHeading
		{
			get
			{
				if (CommonShipment.CusEntryNumbers.Count == 0)
				{
					return ZString.Empty;
				}
				else if (!CommonShipment.CustomsEntryNumber.IsEmpty && !CommonShipment.CustomsEntryNumberType.IsEmpty)
				{
					return CusEntryNumberTypes.UserFriendlyEntryType(CommonShipment.CustomsEntryNumberType) + ": ";
				}
				else
				{
					return CommonShipment.CustomsEntryNumberType;
				}
			}
		}

		public ZString ColoadHouseDisplay
		{
			get { return ColoadMasterShipment != null ? Res.GetString("1236fd01-30b7-4d06-baef-6cebc86f4d9b", "(co-load house)") : ""; }
		}

		public ZString MasterHouseDisplay
		{
			get { return ColoadMasterShipment != null ? Res.GetString("48b6e61a-1457-45f3-84bd-9e946589c648", "Master:") : ""; }
		}

		public ZString StaffTrainingAccreditationNumber
		{
			get { return new GenRegCertAccredMaintList.Loader(Factory).GetByCertificateType(GlbStaff.CurrentUser, CertificateTypePairList.Codes.DT1); }
		}

		#endregion

		#region Customs Entry Number

		public ZString CustomsEntryNumForECN
		{
			get
			{
				if (CommonShipment.CustomsEntryNumberType == CusEntryNumberTypes.Australia.ECN)
				{
					return CommonShipment.CustomsEntryNumber;
				}

				return ZString.Empty;
			}
		}

		public ZString CusEntryNumForManifest
		{
			get { return CommonShipment.CustomsEntryNumber; }
		}

		public ZString ECN
		{
			get
			{
				ZString result = GetECNFromCusEntryNum();

				if (result == ZString.Empty)
				{
					result = GetECNFromDeclaration();
				}

				return result;
			}
		}

		protected ZString GetECNFromCusEntryNum()
		{
			ZString result = ZString.Empty;

			RefUNLOCO currentUNLOCO = (RefUNLOCO)CommonShipment.Factory.LoadFromNaturalKey(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			if (currentUNLOCO != null)
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(CusEntryNumSchema.CE_ParentID, CommonShipment.PK);
				filter.AddToFilter(CusEntryNumSchema.CE_ParentTable, JobShipmentSchema.Constants.TableName);
				filter.AddToFilter(CusEntryNumSchema.CE_EntryType, "EXP");
				filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, currentUNLOCO.RL_RN_NKCountryCode);
				CusEntryNumber[] cusEntryNumberCollection = (CusEntryNumber[])CommonShipment.Factory.Load(typeof(CusEntryNumber), filter);

				if (cusEntryNumberCollection != null && cusEntryNumberCollection.Length > 0)
				{
					foreach (CusEntryNumber currentNumber in cusEntryNumberCollection)
					{
						result += currentNumber.CE_EntryNum + ", ";
					}
				}
			}

			return result.TrimEndIncludingWhiteSpace(',');
		}

		protected ZString GetECNFromDeclaration()
		{
			ZString result = ZString.Empty;

			if (ShipmentDeclarations.Length > 0)
			{
				result = ShipmentDeclarations[0].DeclarationNumber;
			}

			return result;
		}

		public ZString AWBECN
		{
			get { return CommonShipment.CustomsEntryNumberType + ": " + CommonShipment.CustomsEntryNumber; }
		}

		public ZString HBLCustomsEntryNumber
		{
			get
			{
				return CusEntryNumHeading + CustomsEntryNumber;
			}
		}

		public ZString HBLCustomsEntryNumberList
		{
			get
			{
				if (hblCustomsEntryNumberList.IsEmpty)
				{
					foreach (CusEntryNumber num in CommonShipment.CusEntryNumbers)
					{
						if (!hblCustomsEntryNumberList.IsEmpty)
						{
							hblCustomsEntryNumberList += ", ";
						}
						hblCustomsEntryNumberList += CusEntryNumberTypes.UserFriendlyEntryType(num.CE_EntryType);
						hblCustomsEntryNumberList += (num.CE_EntryNum != ZString.Empty ? ": " : "") + num.CE_EntryNum;
					}
				}
				return hblCustomsEntryNumberList;
			}
		}

		ZString hblCustomsEntryNumberList;

		public ZString ColoadShipmentPermitNumbers
		{
			get
			{
				ZString result = "";
				foreach (DocShipment shipment in ColoadShipments)
				{
					result += shipment.CusEntryNumHeading + shipment.CustomsEntryNumber;
					result += "\n";
				}
				result = result.TrimEnd();
				return result;
			}
		}

		#region Iceland Sendingarnumer

		public ZString Sendingarnumer
		{
			get
			{
				if (CommonShipment.CustomsEntryNumberType == CusEntryNumberTypes.Iceland.CRN)
				{
					return CommonShipment.CustomsEntryNumber;
				}

				return ZString.Empty;
			}
		}

		public ZString SendingarnumerWithoutCheckDigit
		{
			get
			{
				if (CommonShipment.CustomsEntryNumberType == CusEntryNumberTypes.Iceland.CRN)
				{
					Sendingarnumer sc = new Sendingarnumer(CommonShipment.CustomsEntryNumber);
					return sc.CodeWithoutCheckDigit;
				}

				return ZString.Empty;
			}
		}

		public ZString SendingarnumerGGGG
		{
			get
			{
				if (CommonShipment.CustomsEntryNumberType == CusEntryNumberTypes.Iceland.CRN)
				{
					Sendingarnumer sc = new Sendingarnumer(CommonShipment.CustomsEntryNumber);
					return sc.CarrierNumber;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region Notes

		public ZString MarksAndNumbers
		{
			get
			{
				ZString result = GetNotes(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, CommonShipment);

				if (result.IsEmpty)
				{
					result = FreightHelperClass.MergePackMarksAndNumbers(CommonShipment.OuterPackLines);
				}

				return result;
			}
		}

		public ZString MarksAndNumbersLine
		{
			get
			{
				return MarksAndNumbers.Replace("\n", " ");
			}
		}

		public ZString[] MarksAndNumbersArray
		{
			get
			{
				return GetNotesInStringArray(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, CommonShipment);
			}
		}

		public ZString[] GoodsDescriptionArray
		{
			get
			{
				return GetNotesInStringArray(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, CommonShipment);
			}
		}

		public ZString DetailedDescriptionOfGoods
		{
			get
			{
				ZString result = GetNotes(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, CommonShipment);
				return !result.IsEmpty ? result : CommonShipment.JS_GoodsDescription;
			}
		}

		public ZString DetailedDescriptionOfGoodsLine
		{
			get
			{
				return DetailedDescriptionOfGoods.Replace("\n", " ");
			}
		}

		public ZString DetailedDescription
		{
			get
			{
				return (DetailedDescriptionOfGoods.Length > GoodsDescription.Length) ? DetailedDescriptionOfGoodsLine : ZString.Empty;
			}
		}

		//Do not use - to be deleted
		public ZString ShortOrLongDescriptionOfGoods
		{
			[DocumentEngineObsoleteField("Obsolete")]
			get
			{
				return DetailedDescriptionOfGoods;
			}
		}

		#region Manifest Goods Descriptions

		public ZString ManifestGoodsDescription
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.ManifestGoodsDescription.Description, CommonShipment); }
		}

		public ZString ManifestDescriptionOfGoods
		{
			get
			{
				ZString description = ManifestGoodsDescription;
				if (description.IsEmpty)
				{
					description = GoodsDescription;
				}
				return AddExportStatement(description);
			}
		}

		public ZString ManifestDetailedDescriptionOfGoods
		{
			get
			{
				ZString description = ManifestGoodsDescription;
				if (description.IsEmpty)
				{
					description = DetailedDescriptionOfGoods;
				}
				return AddExportStatement(description);
			}
		}

		public ZString ManifestDetailedDescriptionOfGoodsLine
		{
			get
			{
				return ManifestDetailedDescriptionOfGoods.Replace("\n", " ");
			}
		}

		#endregion

		public ZString HandlingInstructions
		{
			get { return PickupOrDeliveryHandlingInstructions(); }
		}

		public ZString ShipmentAndOrgHandlingInstructions
		{
			get
			{
				ZString result = HandlingInstructions;

				ZString pickupOrDeliveryHandlingInstructions = GetOrgHandlingInstructions();
				if (!DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, pickupOrDeliveryHandlingInstructions))
				{
					result += "\n" + pickupOrDeliveryHandlingInstructions;
				}
				return result.TrimEnd('\n');
			}
		}

		public ZString ShipmentOrOrgHandlingInstructions
		{
			get
			{
				ZString result = (HandlingInstructions != ZString.Empty) ? HandlingInstructions : GetOrgHandlingInstructions();

				return result.TrimEnd('\n');
			}
		}

		ZString GetOrgHandlingInstructions()
		{
			ZString handlingInstructions = ZString.Empty;

			if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP) && PickupAddressOrganisation != null)
			{
				handlingInstructions = PickupAddressOrganisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), TransportMode, ContainerMode);
			}
			else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV) && DeliveryAddressOrganisation != null)
			{
				handlingInstructions = DeliveryAddressOrganisation.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), TransportMode, ContainerMode);
			}
			return handlingInstructions;
		}

		public ZString CertificateOfOriginNote
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, CommonShipment);
			}
		}

		public virtual ZString CartageInstructions
		{
			get
			{
				ZString result = "";
				result = PickupOrDeliveryCartageInstructions(TransportMode);
				if (result.IsEmpty)
				{
					result = PickupOrDeliveryCartageInstructions();
				}
				return result;
			}
		}

		public virtual ZString CartageInstructionsNoContext
		{
			get { return PickupOrDeliveryCartageInstructions(); }
		}

		public ZString ShipmentAndOrgCartageInstruction
		{
			get
			{
				ZString result = ZString.Empty;
				if (CartageInstructions != ZString.Empty)
				{
					result = (CartageInstructions + "\n");
				}
				ZString pickupCartageInstruction = PickupAddressOrganisation != null ? PickupAddressOrganisation.GetCartageInstructionsByTransportOrContainerMode(TransportMode, ContainerMode) : ZString.Empty;
				ZString deliveryCartageInstructions = DeliveryAddressOrganisation != null ? DeliveryAddressOrganisation.GetCartageInstructionsByTransportOrContainerMode(TransportMode, ContainerMode) : ZString.Empty;

				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					if (!deliveryCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, deliveryCartageInstructions))
					{
						result += deliveryCartageInstructions + "\n";
					}

					if (!pickupCartageInstruction.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, pickupCartageInstruction))
					{
						result += pickupCartageInstruction + "\n";
					}
				}
				else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
				{
					if (!pickupCartageInstruction.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, pickupCartageInstruction))
					{
						result += pickupCartageInstruction + "\n";
					}

					if (!deliveryCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, deliveryCartageInstructions))
					{
						result += deliveryCartageInstructions + "\n";
					}
				}
				return result.Trim('\n');
			}
		}

		public ZString ShipmentOrOrgCartageInstruction
		{
			get
			{
				ZString result = ZString.Empty;
				if (CartageInstructionsNoContext != ZString.Empty)
				{
					result = (CartageInstructionsNoContext + "\n");
				}
				else
				{
					ZString pickupCartageInstruction = PickupAddressOrganisation != null ? PickupAddressOrganisation.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.E), TransportMode, ContainerMode) : ZString.Empty;
					ZString deliveryCartageInstructions = DeliveryAddressOrganisation != null ? DeliveryAddressOrganisation.GetCartageInstructionsByDirectionAndTransportOrContainerMode(nameof(StmNoteContextDirection.I), TransportMode, ContainerMode) : ZString.Empty;

					if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
					{
						if (!deliveryCartageInstructions.IsEmpty && DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, deliveryCartageInstructions))
						{
							result = result.ReplaceIgnoringCase(deliveryCartageInstructions, "");
						}

						if (!pickupCartageInstruction.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, pickupCartageInstruction))
						{
							result += pickupCartageInstruction;
						}
					}
					else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
					{
						if (!pickupCartageInstruction.IsEmpty && DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, pickupCartageInstruction))
						{
							result = result.ReplaceIgnoringCase(pickupCartageInstruction, "");
						}

						if (!deliveryCartageInstructions.IsEmpty && !DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(result, deliveryCartageInstructions))
						{
							result += deliveryCartageInstructions;
						}
					}
				}
				return result.Trim('\n');
			}
		}

		public ZString OutturnNotes
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.OutturnNotes.Description, CommonShipment);
			}
		}

		public ZString[] OutturnNoteArray
		{
			get
			{
				return GetNotesInStringArray(PredefinedNoteTypes.Instance.OutturnNotes.Description, CommonShipment);
			}
		}

		public ZString PreAlertArrivalNoticeRemarks
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, CommonShipment);
			}
		}

		public ZString BookingNotes
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.BookingNotes.Description, CommonShipment);
			}
		}

		public ZString SpecialInstructions
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.SpecialInstructions.Description, CommonShipment);
			}
		}

		public ZString[] AllNotes
		{
			get
			{
				return GetAllNotesInStringArray(CommonShipment);
			}
		}

		public ZString DeliveryRemarks
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes.Description, CommonShipment);
			}
		}

		public ZString DangerousGoodsStatement
		{
			get
			{
				return DocumentsDataRegistry.Instance.DangerousGoodsStatement.Value;
			}
		}

		public ZString DangerousGoodsHandlingInstruction
		{
			get
			{
				return GetNotes(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, CommonShipment);
			}
		}
		#endregion

		#region Consignee Consignor Fields
		public ZString ConsigneeAddress
		{
			get
			{
				ZString result = "";
				if (!IsManufacturerBillOfLading && DocsAndCartage != null)
				{
					if (DocsAndCartage.ConsigneeDocumentaryAddress != null)
					{
						return DocsAndCartage.ConsigneeDocumentaryAddress.PostalAddress;
					}
				}

				if (result.IsEmpty)
				{
					result = Consignee != null ? Consignee.PostalAddress : ZString.Empty;
				}
				return result;
			}
		}

		public ZString RegistrationNumber
		{
			get
			{
				ZString result = ZString.Empty;
				RefCountry brazil = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);

				if (Consignee != null && Consignee.Country != null)
				{
					switch (Consignee.Country.Code)
					{
						case Core.Constants.CountryCodes.Brazil:
							result = Consignee.CustomCodes.GetCustomsRegNoForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, brazil);
							break;
					}
				}
				if (!result.IsEmpty)
				{
					result = "CNPJ: " + result;
				}
				return result;
			}
		}

		public ZString ConsignorAddress
		{
			get
			{
				ZString result = ZString.Empty;
				if (!IsManufacturerBillOfLading && DocsAndCartage != null && DocsAndCartage.ConsignorDocumentaryAddress != null)
				{
					result = DocsAndCartage.ConsignorDocumentaryAddress.PostalAddress;
				}
				else if (Consignor != null)
				{
					result = Consignor.PostalAddress;
				}
				return result;
			}
		}

		public ZString ConsigneeContactName
		{
			get
			{
				ZString result = ShipmentConsignee.E2_Contact;
				return result.IsEmpty ? GetContactCode(Consignee, ContactType.Consignee) : result;
			}
		}

		public ZString ConsigneeContactPhone
		{
			get { return ShipmentConsignee.E2_Phone_Formatted; }
		}

		public ZString ConsigneeContactFax
		{
			get { return ShipmentConsignee.E2_Fax_Formatted; }
		}

		public ZString ConsignorContactName
		{
			get
			{
				ZString result = ShipmentConsignor.E2_Contact;
				return result.IsEmpty ? GetContactCode(Consignor, ContactType.Consignor) : result;
			}
		}

		public ZString ConsignorContactPhone
		{
			get { return ShipmentConsignor.E2_Phone_Formatted; }
		}

		public ZString ConsignorContactFax
		{
			get { return ShipmentConsignor.E2_Fax_Formatted; }
		}

		public ZString ConsignorContact
		{
			[DocumentEngineObsoleteField("Replace ConsignorContact with <ConsignorContactName>")]
			get { return ConsignorContactName; }
		}

		public ZString ConsigneeContact
		{
			[DocumentEngineObsoleteField("Replace ConsigneeContact with <ConsigneeContactName>")]
			get { return ConsigneeContactName; }
		}

		public ZString ConsigneeKennitala
		{
			get
			{
				ZString result = "";
				if (CommonShipment.Consignee != null)
				{
					ZQuery query = new ZQuery(OrgCusCodeSchema.OK_OH, CommonShipment.Consignee.PK);
					query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					query.AddToFilter(JoinCondition.And, OrgCusCodeSchema.OK_CodeType, OrgCusCode.IcelandCodeTypes.Kennitala);
					var orgCusCode = Factory.LoadTop1<OrgCusCode>(query);

					if (orgCusCode != null)
					{
						result = orgCusCode.OK_CustomsRegNo;
					}
				}
				return result;
			}
		}

		#endregion

		public virtual ZString CustomsAgentStaffAssignmentName
		{
			get
			{
				ZString result = "";
				if (Consol != null && Consol.ReceivingForwarder != null)
				{
					OrgHeader receivingForwarder = OrgHeader.LoadFromCode(Factory, Consol.ReceivingForwarder.Code);
					if (receivingForwarder != null)
					{
						ZString glbStaffNK = receivingForwarder.StaffAssignments.GetStaffAssignment(StaffAssignmentRoles.Codes.CustomsAgent, OrgStaffAssignmentsLookups.AllServices);
						GlbStaff glbStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, glbStaffNK);
						if (glbStaff != null)
						{
							result = glbStaff.GS_FullName;
						}
					}
				}
				return result;
			}
		}

		public ZString ColoadHouseBills
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocShipment coload in ColoadShipments)
				{
					if (!coload.HouseBill.IsEmpty)
					{
						result += coload.HouseBill + ", ";
					}
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		#region Request For Missing Documents

		public ZString RequestForMissingDocumentsInstruction
		{
			get
			{
				return Env.Registry.ShipmentRequestForMissingDocumentsClause;
			}
		}

		public ZString MissingRequiredDocuments
		{
			get
			{
				return DocsAndCartage.JobDocumentsRequired.MissingRequiredDocuments;
			}
		}

		public ZString ShipmentOrBrokerageNumber
		{
			get
			{
				return ShipmentNumber;
			}
		}

		public ZString DeclarationOrConsolNumber
		{
			get
			{
				return Consol != null ? Consol.ConsolNumber : ZString.Empty;
			}
		}

		#endregion

		public DocOrganisation LineOrg
		{
			get
			{
				DocOrganisation result = null;

				if (CommonShipment.BookedShippingLine != null)
				{
					result = DocOrganisation.New(Factory, CommonShipment.BookedShippingLine.PK);
				}
				else if (Sailing != null && Sailing.Voyage != null)
				{
					result = Sailing.Voyage.Line;
				}

				return result;
			}
		}

		#endregion

		#region IsBillOfLading

		protected bool IsBillOfLading
		{
			get; set;
		}

		#endregion

		#region Related Objects

		#region Consol

		public DocBaseConsol CurrentConsol { get; set; }

		DocShipmentConsol BillOfLadingConsol
		{
			get
			{
				if (!billOfLadingConsolHasBeenInitialised)
				{
					CommonConsol result = DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV)
											? MovementLegComparer.LastOrDefaultLegForTransportMode(CommonShipment.Consols.Cast<CommonConsol>(), CommonShipment.TransportMode)
											: MovementLegComparer.FirstOrDefaultLegForTransportMode(CommonShipment.Consols.Cast<CommonConsol>(), CommonShipment.TransportMode);

					billOfLadingConsol = DocShipmentConsol.New(result, Factory);
					billOfLadingConsolHasBeenInitialised = true;
				}

				return billOfLadingConsol;
			}
		}
		bool billOfLadingConsolHasBeenInitialised;
		DocShipmentConsol billOfLadingConsol;

		internal CommonConsol DepartureCommonConsol
		{
			get => CommonShipment.MostInterestingDepartureConsol ?? CommonShipment.DepartureConsolForDocuments;
		}

		internal CommonConsol ArrivalCommonConsol
		{
			get => CommonShipment.ArrivalConsolForDocuments;
		}

		public virtual DocShipmentConsol Consol
		{
			get
			{
				if (IsBillOfLading)
				{
					return BillOfLadingConsol;
				}

				CommonConsol result = null;

				if (IsImportDocument)
				{
					result = ArrivalCommonConsol;
				}
				else if (IsExportDocument)
				{
					result = DepartureCommonConsol;
				}
				else if (CommonShipment.Consols.Count > 0)
				{
					CommonShipment.Consols.Sort(CommonConsol.Schema.JK_UniqueConsignRef, System.ComponentModel.ListSortDirection.Ascending);
					result = CommonShipment.Consols[0];
				}

				return DocShipmentConsol.New(result, Factory);
			}
		}

		#endregion

		public virtual DocSailing Sailing
		{
			get
			{
				return Consol?.Sailing ?? DocSailing.New(CommonShipment.Sailing, Factory);
			}
		}

		public override DocJobHeader JobHeader
		{
			get
			{
				ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				filter.AddToFilter(JobHeaderSchema.JH_ParentID, CommonShipment.PK);
				filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				JobHeader header = (JobHeader)CommonShipment.Factory.LoadTop1(typeof(JobHeader), filter);

				return DocJobHeader.New(header, Debtor, Factory);
			}
		}

		public virtual DocJobInvoicingJob Job
		{
			get
			{
				ZQuery filter = new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				filter.AddToFilter(JobHeaderSchema.JH_ParentID, CommonShipment.PK);
				filter.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Enterprise.Accounting.Business.JobInvoicing.Job jobObj = CommonShipment.Factory.LoadTop1<Enterprise.Accounting.Business.JobInvoicing.Job>(filter);

				return DocJobInvoicingJob.New(jobObj, Factory);
			}
		}

		public DocBaseJobDeclaration DeclarationForDocuments
		{
			get
			{
				var decPk = ((CommonShipment)WrappedObject).DeclarationForDocuments?.PK;
				return decPk != null ? DocBaseJobDeclaration.New(Factory.Load<BaseJobDeclaration>(decPk.Value), Factory) : null;
			}
		}

		public DocBankAccount BankAccount
		{
			get
			{
				if (ReceiptBankAccount == null)
				{
					AccBankAccount fReceiptBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, GlbBranch.CurrentBranch, Factory);
					if (fReceiptBankAccount != null)
					{
						ReceiptBankAccount = DocBankAccount.New(fReceiptBankAccount, Factory);
					}
				}

				return ReceiptBankAccount;
			}
		}

		DocBankAccount ReceiptBankAccount;

		JobDocAddress ShipmentConsignee => IsManufacturerBillOfLading ? CommonShipment.ConsignorDocumentaryAddress : CommonShipment.ConsigneeDocumentaryAddress;

		JobDocAddress ShipmentConsignor => IsManufacturerBillOfLading ? CommonShipment.ManufacturerDocAddress : CommonShipment.ConsignorDocumentaryAddress;

		public DocOrganisation Consignee => DocOrganisation.New(ShipmentConsignee, Factory);

		public DocOrganisation Consignor => DocOrganisation.New(ShipmentConsignor, Factory);

		public DocDocAddress ManufacturerAddress => DocDocAddress.New(CommonShipment.ManufacturerDocAddress, Factory);

		public DocOrganisation InterimReceiptConsignor
		{
			get { return Consignor; }
		}

		public DocOrganisation InterimReceiptConsignee
		{
			get { return Consignee; }
		}

		public DocOrganisation DeliveryAgent
		{
			get { return DocOrganisation.New(CommonShipment.DeliveryAgent, Factory); }
		}

		public DocOrganisation ExportBroker
		{
			get { return DocOrganisation.New(CommonShipment.ExportBroker, Factory); }
		}

		public DocOrganisation ExportCartage
		{
			get { return DocOrganisation.New(CommonShipment.DocsAndCartage.PickupCartageCo, Factory); }
		}

		public DocOrganisation HandledOnBehalfOfForwarder
		{
			get { return DocOrganisation.New(CommonShipment.HandledOnBehalfOfForwarder, Factory); }
		}

		public DocOrganisation ImportBroker
		{
			get { return DocOrganisation.New(CommonShipment.ImportBroker, Factory); }
		}

		public DocOrganisation ImportCartage
		{
			get { return DocOrganisation.New(CommonShipment.DocsAndCartage.DeliveryCartageCo, Factory); }
		}

		public DocOrganisation TranshipAgent
		{
			get { return DocOrganisation.New(CommonShipment.TranshipAgent, Factory); }
		}

		public DocOrganisation ImportCartageOrganisation
		{
			get
			{
				if (ImportCartage == null)
				{
					return GetCartageCompanyFromRegistry();
				}
				else
				{
					return ImportCartage;
				}
			}
		}

		public DocOrganisation ExportCartageOrganisation
		{
			get
			{
				if (ExportCartage == null)
				{
					return GetCartageCompanyFromRegistry();
				}
				else
				{
					return ExportCartage;
				}
			}
		}

		public DocOrganisation ShippersDeliveryAgent
		{
			get
			{
				if (DeliveryAgent != null)
				{
					return DeliveryAgent;
				}
				else if (Consol != null)
				{
					return Consol.ReceivingForwarder;
				}
				else
				{
					return null;
				}
			}
		}

		public DocOrganisation BillTo
		{
			get
			{
				DocOrganisation billToOrganisation = null;
				DocJobHeader docJob = JobHeader;

				if (docJob != null && docJob.LocalCharges != null)
				{
					if (docJob.LocalCharges.MiscServ != null)
					{
						if (CommonShipment.IsImport())
						{
							billToOrganisation = docJob.LocalCharges.IMFreightBillTo;
						}
						else if (CommonShipment.IsExport())
						{
							billToOrganisation = docJob.LocalCharges.EXFreightBillTo;
						}
					}

					if (billToOrganisation == null || billToOrganisation.PostalAddress == ZString.Empty)
					{
						billToOrganisation = docJob.LocalCharges;
					}
				}

				return billToOrganisation;
			}
		}

		public DocDocAddress RequestedBillToAddress
		{
			get { return DocDocAddress.New(CommonShipment.DocAddresses.FindByDocAddressType(DocAddressType.ClientRequestedBillingParty), Factory); }
		}

		public DocOrganisation Supplier
		{
			// this field is used as a default UDF value for Declaration Export doc (Bank Draft).
			get { return Consignor; }
		}

		public DocDocAddress ImportPickUpAddress
		{
			get
			{
				return DocDocAddress.New(CommonShipment.ImportPickUpAddress, Factory);
			}
		}

		public DocShipment ColoadMasterShipment
		{
			get { return CommonShipment.JS_JS_ColoadMasterShipment.IsValid ? DocShipment.New(CommonShipment.Factory, CommonShipment.JS_JS_ColoadMasterShipment) : null; }
		}

		public DocDocAddress DeliveryAddress
		{
			get { return DocsAndCartage.DeliveryAddress ?? ConsigneeDeliverDocAddress; }
		}

		DocDocAddress ConsigneeDeliverDocAddress
		{
			get { return Consignee != null ? Consignee.DeliverDocAddress : null; }
		}

		public DocDocAddress PickupAddress
		{
			get { return DocsAndCartage.PickupAddress ?? ConsignorPickUpDocAddress; }
		}

		DocDocAddress ConsignorPickUpDocAddress
		{
			get { return Consignor != null ? Consignor.PickUpDocAddress : null; }
		}

		public DocOrganisation DeliveryAddressOrganisation
		{
			get { return DeliveryAddress != null ? DeliveryAddress.Organisation : null; }
		}

		public DocOrganisation PickupAddressOrganisation
		{
			get { return PickupAddress != null ? PickupAddress.Organisation : null; }
		}

		public virtual ZInt NotifyPartyCount
		{
			get { return 1; }
		}

		public virtual DocContacts NotifyParty
		{
			get { return DocContacts.New(CommonShipment.NotifyPartyDocumentaryAddress, Factory); }
		}

		public virtual DocContacts NotifyParty2
		{
			get { return DocContacts.New(CommonShipment.NotifyParty2DocumentaryAddress, Factory); }
		}

		public virtual DocContacts NotifyParty3
		{
			get { return DocContacts.New(CommonShipment.NotifyParty3DocumentaryAddress, Factory); }
		}

		public DocUNLOCO DestinationLoco
		{
			get { return DocUNLOCO.New(CommonShipment.Destination, Factory); }
		}

		public DocUNLOCO OriginLoco
		{
			get { return DocUNLOCO.New(CommonShipment.Origin, Factory); }
		}

		public DocCurrency GoodsCurr
		{
			get { return DocCurrency.New(CommonShipment.GoodsValueCurr, Factory); }
		}

		public DocCurrency InsuranceCurrency
		{
			get { return DocCurrency.New(CommonShipment.InsuranceCurrency, Factory); }
		}

		public DocJobDocsAndCartage DocsAndCartage
		{
			get { return DocJobDocsAndCartage.New(CommonShipment.DocsAndCartage, Factory); }
		}

		#endregion

		#region ZInt Fields

		public ZInt SequenceNumber { get; set; }

		public ZInt TotalOuterPacksPillaged
		{
			get { return CommonShipment.TotalOuterPacksPillaged; }
		}

		public ZInt TotalOuterPacksDamaged
		{
			get { return CommonShipment.TotalOuterPacksDamaged; }
		}

		public ZInt TotalOuterPacksOutturned
		{
			get { return Convert.ToInt32(TotalCalculation.GetTotal(OuterPackLineCollection, nameof(DocPackLines.Outturn))); }
		}

		public ZInt ContainerPackagesCount
		{
			get
			{
				if (PackingMode == Core.Constants.ContainerModes.FCL)
				{
					return Containers.Count;
				}
				else if (PackingMode == Core.Constants.ContainerModes.LCL)
				{
					return OuterPacks;
				}

				return 0;
			}
		}

		public ZInt NoOfOriginalBills
		{
			get { return Convert.ToInt16(CommonShipment.JS_NoOriginalBills); }
		}

		public ZInt NoOfCopyBills
		{
			get { return Convert.ToInt16(CommonShipment.JS_NoCopyBills); }
		}

		public ZInt TotalOuterPackLinePackages
		{
			get { return CommonShipment.TotalOuterPacks; }
		}

		public ZInt InnerPacks
		{
			get { return CommonShipment.JS_TotalPackageCount; }
		}

		public ZInt OuterPacks
		{
			get { return CommonShipment.JS_OuterPacks; }
		}

		public ZInt PackingOrder
		{
			get { return CommonShipment.JS_PackingOrder; }
		}

		public ZString PackageSizesAndCounts
		{
			get
			{
				ZString result = ZString.Empty;
				int i = 0;
				foreach (PackLine packLine in CommonShipment.OuterPackLines)
				{
					if (i != 0)
					{
						result += "\r\n";
					}

					ZDecimal lengthCM = Enterprise.Core.Constants.Length.ConvertSafe(packLine.JL_Length, packLine.JL_UnitOfDimension, Enterprise.Core.Constants.Length.Centimetres);
					ZDecimal widthCM = Enterprise.Core.Constants.Length.ConvertSafe(packLine.JL_Width, packLine.JL_UnitOfDimension, Enterprise.Core.Constants.Length.Centimetres);
					ZDecimal heightCM = Enterprise.Core.Constants.Length.ConvertSafe(packLine.JL_Height, packLine.JL_UnitOfDimension, Enterprise.Core.Constants.Length.Centimetres);
					result += FormatNumber(lengthCM) + "X" + FormatNumber(widthCM) + "X" + FormatNumber(heightCM) + "/" + packLine.JL_PackageCount.ToString();
					i++;
				}
				return result;
			}
		}

		#region Germany specific

		public ZString InsuranceCovered
		{
			get
			{
				bool isCovered = (CommonShipment.JS_InsuranceValue > 0);
				return isCovered ? DocumentsDataRegistry.Instance.FOBAndienungInsuranceCoveredText.Value : DocumentsDataRegistry.Instance.FOBAndienungInsuranceNotCoveredText.Value;
			}
		}

		#endregion

		#endregion

		#region ZDecimal Fields

		public ZDecimal ShipperCODAmount
		{
			get { return CommonShipment.JS_ShipperCODAmount; }
		}

		public ZDecimal ActualWeightRounded
		{
			get
			{
				double result = 0D;
				double actualWeight = Convert.ToDouble(CommonShipment.JS_ActualWeight);

				double decimalPart = actualWeight - Math.Floor(actualWeight);
				if (decimalPart > 0.25D && decimalPart < 0.75D)
				{
					result = Math.Floor(actualWeight) + 0.5D;
				}
				else if (decimalPart >= 0.75D)
				{
					result = Math.Ceiling(actualWeight);
				}
				else
				{
					result = Math.Floor(actualWeight);
				}

				return new ZDecimal(result);
			}
		}

		public ZDecimal ChargeableRate
		{
			get { return CommonShipment.JS_UnitFreightRate; }
		}

		public virtual ZDecimal GoodsValue
		{
			get { return CommonShipment.JS_GoodsValue; }
		}

		public ZDecimal GoodsCurrencyExchangeRate
		{
			get { return CommonShipment.GoodsValueCurr != null ? CommonShipment.GoodsValueCurr.GetRateForDate(ZArchitecture.Core.ExchangeRateType.Buy, new ZDateTime(Env.Time.CurrentLocalDate), 0) : new ZDecimal(0); }
		}

		public virtual ZDecimal InsuranceValue
		{
			get { return CommonShipment.JS_InsuranceValue; }
		}

		public ZDecimal TotalWeightOutturned
		{
			get { return (ZDecimal)TotalCalculation.GetTotal(OuterPackLineCollection, "OutturnedWeight"); }
		}

		public ZDecimal TotalVolumeOutturned
		{
			get { return (ZDecimal)TotalCalculation.GetTotal(OuterPackLineCollection, "OutturnedVolume"); }
		}

		public ZDecimal FreightCollectAmount
		{
			get
			{
				ZDecimal result = 0M;

				if (IsCollect)
				{
					if (JobHeader != null)
					{
						if (JobHeader.JobChargesForAgentCollect != null)
						{
							foreach (DocJobCharge jobCharge in JobHeader.JobChargesForAgentCollect)
							{
								if (jobCharge.ChargeCode.ChargeGroup == "FRT")
								{
									result += jobCharge.LocalSellAmount;
								}
							}
						}
					}
				}

				return result;
			}
		}

		public ZDecimal FCCODAmount
		{
			get { return Job != null ? Job.JH_TotalRevenue : ZDecimal.Zero; }
		}

		public ZDecimal FreightCODAmount
		{
			get
			{
				ZDecimal result = 0M;

				if (JobHeader != null)
				{
					foreach (DocJobCharge jobCharge in JobHeader.JobChargesForAgentCollect)
					{
						if (jobCharge.IsRevenuePosted)
						{
							result += jobCharge.LocalSellAmount;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region ZBool Fields

		public ZBool ShowConsignorConsignee
		{
			get { return Consignor != null || Consignee != null; }
		}

		public virtual ZBool ShowChargeable
		{
			get { return true; }
		}

		public ZBool HasDangerousGoods
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (PackLine line in CommonShipment.OuterPackLines)
				{
					foreach (UNDGDataItem dgItem in line.UNDGs)
					{
						if (dgItem.Substance != null)
						{
							result = ZBool.True;
							break;
						}
					}
				}
				return result;
			}
		}

		public ZBool IsCFSRegistered
		{
			get { return CommonShipment.JS_IsCFSRegistered; }
		}

		public ZBool IsForwardRegistered
		{
			get { return CommonShipment.JS_IsForwardRegistered; }
		}

		public ZBool IsShipping
		{
			get { return CommonShipment.JS_IsShipping; }
		}

		public ZBool IsBooking
		{
			get { return CommonShipment.JS_IsBooking; }
		}

		public ZBool IsAgencyShipping
		{
			get { return !IsCFSRegistered && !IsForwardRegistered && IsShipping; }
		}

		public ZBool IsAgencyShipmentBooking
		{
			get { return IsAgencyShipping && ShipmentStatusHelperMethods.GetBookingStageStatus().Contains(CommonShipment.JS_ShipmentStatus.ToString()); }
		}

		public ZBool IsBuyersConsolMaster
		{
			get { return CommonShipment.IsBuyersConsolLead; }
		}

		public ZBool IsCoload
		{
			get { return CommonShipment.IsCoLoadMaster || CommonShipment.IsBlindCoLoadMaster; }
		}

		public ZBool HasProhibitedPackaging
		{
			get { return CommonShipment.DocsAndCartage.JP_HasProhibitedPackaging; }
		}

		public ZBool HasTransportPlanningDetails
		{
			get { return (CommonShipment.Transports != null && CommonShipment.Transports.Count > 0); }
		}

		public ZBool IsDangerousGoodsShipment
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocPackLines line in OuterPackLineCollection)
				{
					if (line.UNDGs.Length > 0)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}

		public ZBool IsDomesticThirdParty
		{
			get { return (CommonShipment.JS_INCO == Constants.DomesticPaymentTerms.CollectThirdParty); }
		}

		public ZBool IsDomesticPrepaid
		{
			get { return (CommonShipment.JS_INCO == Constants.DomesticPaymentTerms.Prepaid); }
		}

		public ZBool IsDomesticCollect
		{
			get { return (CommonShipment.JS_INCO == Constants.DomesticPaymentTerms.Collect); }
		}

		public ZBool IsPrepaid
		{
			get { return (PrepaidCollectCode == Constants.PaymentType.Prepaid); }
		}

		public ZBool IsCollect
		{
			get { return (PrepaidCollectCode == Constants.PaymentType.Collect); }
		}

		public ZBool IsMasterCoLoadShipment
		{
			get { return (IsCoload || ColoadShipments.Count > 0); }
		}

		public ZBool IsMasterCoLoadShipmentWithSubShipments
		{
			get { return (IsCoload && ColoadShipments.Count > 0); }
		}

		public ZBool IsExport
		{
			get { return CommonShipment.IsExport(); }
		}

		public ZBool IsImport
		{
			get { return CommonShipment.IsImport(); }
		}

		public ZBool IsOffshore
		{
			get { return CommonShipment.IsCrossTrade(); }
		}

		public ZBool IsDomestic
		{
			get { return CommonShipment.IsDomestic(); }
		}

		public ZBool IsDomesticAndFreightCOD
		{
			get { return IsDomestic && INCO == Constants.DomesticPaymentTerms.CollectCOD; }
		}

		public ZBool IsUnattachedOrder
		{
			get { return ZBool.False; }
		}

		public ZBool PrintContainerDetailsOnInterimReceipt
		{
			get { return DocumentsDataRegistry.Instance.DisplayContainerDetailsOnShipment.Value; }
		}

		public ZBool DisplayLogo
		{
			get { return (ZBool)DocumentsDataRegistry.Instance.DisplayLogo.Value; }
		}

		public ZBool IsRoad
		{
			get { return CommonShipment.IsRoad; }
		}

		#endregion

		#region Cartage Advice

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		public ZString ReportNameWithoutCFS
		{
			get { return ReportName.Replace(" - CFS", ""); }
		}

		#region Addresses

		#region PickUpFrom Addresss

		#region PickUpFromAddress

		public DocDocAddress PickUpFromAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExportDocument)
				{
					result = PickUpFromAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = PickUpFromAddressForImport;
				}

				return result;
			}
		}

		#endregion

		#region PickUpFromAddressForExport

		public virtual DocDocAddress PickUpFromAddressForExport
		{
			get
			{
				DocDocAddress result = null;

				if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.MessageType == "EXP" && DeclarationForCurrentBranch.ClientPickupDeliveryAddress != null)
				{
					result = DeclarationForCurrentBranch.ClientPickupDeliveryAddress;
				}
				else if (PickupAddress != null)
				{
					result = PickupAddress;
				}
				else if (Consignor != null)
				{
					result = Consignor.PickUpDocAddress;
				}

				return result;
			}
		}

		#endregion

		#region PickUpFromAddressForImport

		public virtual DocDocAddress PickUpFromAddressForImport
		{
			get
			{
				DocDocAddress result = null;

				if (ImportReleaseDepot != null)
				{
					result = ImportReleaseDepot;
				}
				else if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.MessageType == "IMP" && DeclarationForCurrentBranch.WarehouseAddress != null)
				{
					result = DeclarationForCurrentBranch.WarehouseAddress;
				}
				else if (Consol != null)
				{
					result = Consol.UnpackDepotAddress;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region DeliverTo Addresses

		#region DeliverToAddress

		public DocDocAddress DeliverToAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExportDocument)
				{
					result = DeliverToAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = DeliverToAddressForImport;
				}

				return result;
			}
		}

		#endregion

		#region DeliverToAddressForExport

		public virtual DocDocAddress DeliverToAddressForExport
		{
			get
			{
				DocDocAddress result = null;

				if (ExportReceivingDepot != null)
				{
					result = ExportReceivingDepot;
				}
				else if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.MessageType == "EXP" && DeclarationForCurrentBranch.DepotAddress != null)
				{
					result = DeclarationForCurrentBranch.DepotAddress;
				}
				else if (Consol != null)
				{
					result = Consol.PackDepotAddress;
				}

				return result;
			}
		}

		#endregion

		#region DeliverToAddressForImport

		public virtual DocDocAddress DeliverToAddressForImport
		{
			get
			{
				DocDocAddress result = null;

				if (!IsBookingShipment)
				{
					if (DeclarationForCurrentBranch != null && DeclarationForCurrentBranch.MessageType == "IMP" && DeclarationForCurrentBranch.ClientPickupDeliveryAddress != null)
					{
						result = DeclarationForCurrentBranch.ClientPickupDeliveryAddress;
					}
					else if (DeliveryAddress != null)
					{
						result = DeliveryAddress;
					}
					else if (Consignee != null)
					{
						result = Consignee.DeliverDocAddress;
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region ExportReceivingDepot

		public virtual DocDocAddress ExportReceivingDepot
		{
			get
			{
				DocDocAddress exportDepot = null;

				if (CommonShipment.ExportReceivingDepot != null)
				{
					exportDepot = DocDocAddress.New(CommonShipment.ExportReceivingDepot, Factory);
				}
				else if (Consol != null)
				{
					switch (TransportMode)
					{
						case Core.Constants.TransportModes.Rail:
						case Core.Constants.TransportModes.Sea:
							if (PackingMode == Core.Constants.ContainerModes.FCL)
							{
								exportDepot = Consol.DepartureCTOAddress;
							}
							else
							{
								exportDepot = Consol.PackDepotAddress;
							}
							break;

						default:
						case Core.Constants.TransportModes.Air:
							exportDepot = Consol.PackDepotAddress;
							break;
					}
				}
				return exportDepot;
			}
		}

		#endregion

		#region ImportReleaseDepot

		public virtual DocDocAddress ImportReleaseDepot
		{
			get { return DocDocAddress.New(CommonShipment.ImportReleaseDepot, Factory); }
		}

		#endregion

		#endregion

		#region Contacts

		public DocContacts ImportUnpackDepotContact
		{
			get
			{
				DocContacts result = null;

				if (ImportTranshipmentConsol != null && ImportTranshipmentConsol.UnpackDepotAddress != null)
				{
					result = ImportTranshipmentConsol.UnpackDepotAddress.GetContact(ContactType.LocalTransport);
				}

				return result;
			}
		}

		public DocContacts ExportPackDepotContact
		{
			get
			{
				DocContacts result = null;

				if (ExportTranshipmentConsol != null && ExportTranshipmentConsol.PackDepotAddress != null)
				{
					result = ExportTranshipmentConsol.PackDepotAddress.GetContact(ContactType.LocalTransport);
				}

				return result;
			}
		}

		#region PickUpFromContact Details

		public DocContacts PickUpFromContact
		{
			get
			{
				return (PickUpFromAddress != null) ? PickUpFromAddress.GetContact(ContactType.LocalTransport) : null;
			}
		}

		public ZString PickUpFromContactName
		{
			get
			{
				return (PickUpFromContact != null) ? PickUpFromContact.ContactName : ZString.Empty;
			}
		}

		public ZString PickUpFromContactPhone
		{
			get
			{
				ZString result = ZString.Empty;

				if (PickUpFromContact != null)
				{
					result = PickUpFromContact.Phone;
				}
				if (result.IsEmpty && PickUpFromAddress != null)
				{
					result = PickUpFromAddress.Phone;
				}

				return result;
			}
		}

		#endregion

		#region DeliverToContact Details

		public DocContacts DeliverToContact
		{
			get
			{
				return (DeliverToAddress != null) ? DeliverToAddress.GetContact(ContactType.LocalTransport) : null;
			}
		}

		public ZString DeliverToContactName
		{
			get
			{
				return (DeliverToContact != null) ? DeliverToContact.ContactName : ZString.Empty;
			}
		}

		public ZString DeliverToContactPhone
		{
			get
			{
				ZString result = ZString.Empty;

				if (DeliverToContact != null)
				{
					result = DeliverToContact.Phone;
				}
				if (result.IsEmpty && DeliverToAddress != null)
				{
					result = DeliverToAddress.Phone;
				}

				return result;
			}
		}

		#endregion

		#region ExportReceivingDepotContact Details

		public DocContacts ExportReceivingDepotContact
		{
			get
			{
				return (ExportReceivingDepot != null) ? ExportReceivingDepot.GetContact(ContactType.LocalTransport) : null;
			}
		}

		public ZString ExportReceivingDepotContactName
		{
			get
			{
				return (ExportReceivingDepotContact != null) ? ExportReceivingDepotContact.ContactName : ZString.Empty;
			}
		}

		public ZString ExportReceivingDepotContactPhone
		{
			get
			{
				ZString result = ZString.Empty;

				if (ExportReceivingDepotContact != null)
				{
					result = ExportReceivingDepotContact.Phone;
				}
				if (result.IsEmpty && ExportReceivingDepot != null)
				{
					result = ExportReceivingDepot.Phone;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region DeclarationForCurrentBranch

		public DocBaseJobDeclaration DeclarationForCurrentBranch
		{
			get
			{
				DocBaseJobDeclaration result = null;

				if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
				{
					foreach (BaseJobDeclaration currentDec in ShipmentDeclarations)
					{
						if (currentDec.Branch != null)
						{
							if (currentDec.Branch.GB_Code == CurrentBranch.Code)
							{
								result = DocBaseJobDeclaration.New(currentDec, Factory);
								break;
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region DeclarationForShipmentBranch

		public DocBaseJobDeclaration DeclarationForShipmentBranch
		{
			get
			{
				DocBaseJobDeclaration result = null;

				foreach (BaseJobDeclaration currentDec in ShipmentDeclarations)
				{
					if (JobHeader != null && JobHeader.Branch != null && currentDec.Branch != null && JobHeader.Branch.Code == currentDec.Branch.GB_Code)
					{
						result = DocBaseJobDeclaration.New(currentDec, Factory);
						break;
					}
				}

				return result;
			}
		}

		#endregion

		public ZBool IsAir
		{
			get { return TransportModeIsAir; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime PickUpFromDate
		{
			get { return CommonShipment.IsDeleted ? ZDateTime.Empty : CommonShipment.DocsAndCartage.JP_EstimatedPickup; }
		}

		public ZDateTime ShipmentCreateDate
		{
			get { return CommonShipment.JS_SystemCreateTimeUtc; }
		}

		public virtual ZDateTime ETA
		{
			get { return CommonShipment.JS_E_ARV; }
		}

		public virtual ZDateTime ETD
		{
			get { return CommonShipment.JS_E_DEP; }
		}

		public ZDateTime ExportDate
		{
			get { return (Consol != null) ? Consol.ATD : ZDateTime.Empty; }
		}

		public ZDateTime DateOfArrival
		{
			get { return (Consol != null) ? Consol.ATA : ZDateTime.Empty; }
		}

		public ZDateTime DateOfIssue
		{
			get { return (!HouseBillIssueDate.IsEmpty) ? HouseBillIssueDate.Date : ZDateTime.Empty; }
		}

		public ZDateTime OnBoardDate
		{
			get { return (!ShippedOnBoardDate.IsEmpty) ? ShippedOnBoardDate.Date : ZDateTime.Empty; }
		}

		public ZDateTime ShippedOnBoardDate
		{
			get { return CommonShipment.JS_ShippedOnBoardDate; }
		}

		public ZDateTime HouseBillIssueDate
		{
			get { return CommonShipment.JS_HouseBillIssueDate; }
		}

		public ZString HouseBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(HouseBill, HouseBillIssueDate); }
		}

		public ZDateTime MasterBillIssueDate
		{
			get { return Consol != null ? Consol.MasterBillIssueDate : ZDateTime.Empty; }
		}

		public ZString MasterBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(MasterBillNum, MasterBillIssueDate); }
		}

		public ZDateTime ActualBKD
		{
			get { return CommonShipment.JS_A_BKD; }
		}

		public ZDateTime ActualRCV
		{
			get { return CommonShipment.JS_A_RCV; }
		}

		public ZDateTime PickupDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (CommonShipment.DocsAndCartage != null)
				{
					result = CommonShipment.DocsAndCartage.JP_EstimatedPickup;
				}
				return result;
			}
		}

		public ZDateTime CartageCutOffDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (CommonShipment.TransportsIncludingRelated.FirstLeg != null)
				{
					result = PackingMode == Core.Constants.ContainerModes.FCL ?
						CommonShipment.TransportsIncludingRelated.FirstLeg.JW_TerminalCutOff :
						CommonShipment.TransportsIncludingRelated.FirstLeg.JW_DepotCutOff;
				}
				return result;
			}
		}

		public ZDateTime CartageAvailableDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					CommonShipment.DocsAndCartage.JP_FCLAvailable :
					CommonShipment.DocsAndCartage.JP_LCLAvailable;
			}
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExportDocument)
				{
					result = BookingCutOffDate;
				}
				else if (IsImportDocument)
				{
					result = AvailableDate;
				}
				return result;
			}
		}

		public ZDateTime CartageReceivalDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (CommonShipment.TransportsIncludingRelated.FirstLeg != null)
				{
					result = PackingMode == Core.Constants.ContainerModes.FCL ?
						CommonShipment.TransportsIncludingRelated.FirstLeg.JW_TerminalReceivalCommences :
						CommonShipment.TransportsIncludingRelated.FirstLeg.JW_DepotReceivalCommences;
				}
				return result;
			}
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get
			{
				return PackingMode == Core.Constants.ContainerModes.FCL ?
					CommonShipment.DocsAndCartage.JP_FCLStorageCommences :
					CommonShipment.DocsAndCartage.JP_LCLStorageCommences;
			}
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (IsExportDocument)
				{
					result = PickupDate;
				}
				else if (IsImportDocument)
				{
					result = StorageCommenceDate;
				}
				return result;
			}
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsExportDocument)
				{
					result = CartageAdvice.PickupDateHeading;
				}
				else if (IsImportDocument)
				{
					result = CartageAdvice.StorageCommencesHeading;
				}

				return result;
			}
		}

		public ZDateTime ClientRequestedETA
		{
			get { return CommonShipment.JS_ClientRequestedETA; }
		}

		public ZDateTime FCLAvailableDate
		{
			get { return DocsAndCartage.FCLAvailable; }
		}

		public ZDateTime FCLStorageDate
		{
			get { return DocsAndCartage.FCLStorageCommences; }
		}

		internal ZDateTime FCLReceivalsDate
		{
			get { return CommonShipment.MostInterestingTransport != null ? CommonShipment.MostInterestingTransport.JW_TerminalReceivalCommences : ZDateTime.Empty; }
		}

		internal ZDateTime FCLCutOffDate
		{
			get { return CommonShipment.MostInterestingTransport != null ? CommonShipment.MostInterestingTransport.JW_TerminalCutOff : ZDateTime.Empty; }
		}

		public ZDateTime LCLAvailableDate
		{
			get { return DocsAndCartage.LCLAvailable; }
		}

		public ZDateTime LCLStorageDate
		{
			get { return DocsAndCartage.LCLStorageCommences; }
		}

		internal ZDateTime LCLReceivalsDate
		{
			get { return CommonShipment.MostInterestingTransport != null ? CommonShipment.MostInterestingTransport.JW_DepotReceivalCommences : ZDateTime.Empty; }
		}

		internal ZDateTime LCLCutOffDate
		{
			get { return CommonShipment.MostInterestingTransport != null ? CommonShipment.MostInterestingTransport.JW_DepotCutOff : ZDateTime.Empty; }
		}

		public ZDateTime BondDate
		{
			get { return (ETA != ZDateTime.Empty) ? ETA.AddMonths(2).AddDays(-ETA.Day) : ZDateTime.Empty; }
		}

		#endregion

		#region Collections

		#region Milestones

		public DocWorkFlowCollection Milestones
		{
			get
			{
				if (fMilestones == null)
				{
					if (CommonShipment is IWorkflowProvider)
					{
						IWorkflowProvider workflowProvider = (IWorkflowProvider)CommonShipment;
						fMilestones = new DocWorkFlowCollection(workflowProvider.WorkflowItems.MilestonesIncludingRelated, CommonShipment.Factory);
					}
					else
					{
						fMilestones = new DocWorkFlowCollection(Factory);
					}
				}
				return fMilestones;
			}
		}
		DocWorkFlowCollection fMilestones;

		public ZBool ShouldPrintMilestones
		{
			get
			{
				return (ZBool)DocumentsDataRegistry.Instance.ShowMilestonesOnPODDocument.Value;
			}
		}

		public ZBool HasEstimatedInTheList
		{
			get
			{
				return Milestones.HasEstimatedInTheList;
			}
		}

		#endregion

		#region ShipmentDeclarations

		BaseJobDeclaration[] ShipmentDeclarations
		{
			get
			{
				fShipmentDeclarations = (BaseJobDeclaration[])Factory.Load(typeof(BaseJobDeclaration), new ZQuery(JobDeclarationSchema.JE_JS, CommonShipment.PK));
				return fShipmentDeclarations;
			}
		}

		BaseJobDeclaration[] fShipmentDeclarations;

		public DocBaseJobDeclaration DeclarationForShipmentCompany
		{
			get
			{
				DocBaseJobDeclaration result = null;

				foreach (BaseJobDeclaration currentDec in this.ShipmentDeclarations)
				{
					if (this.JobHeader != null && this.JobHeader.Branch.CurrentCompany != null && currentDec.Branch.Company != null && this.JobHeader.CurrentCompany.Code == currentDec.Branch.Company.GC_Code)
					{
						result = DocBaseJobDeclaration.New(currentDec, Factory);
						break;
					}
				}

				return result;
			}
		}

		#endregion

		public DocTransportCollection TransportsIncludingRelated
		{
			get
			{
				if (transportsIncludingRelated == null)
				{
					transportsIncludingRelated = new DocTransportCollection(((IRoutingSupport)CommonShipment).TransportsIncludingRelated, Factory);
				}
				return transportsIncludingRelated;
			}
		}
		DocTransportCollection transportsIncludingRelated;

		public DocTransportCollection ShipmentConsolsTransportPlannings
		{
			get
			{
				DocTransportCollection transportCollection = new DocTransportCollection(CommonShipment.Factory);
				foreach (DocBaseConsol consol in ShipmentConsols)
				{
					transportCollection.AddRange(consol.TransportPlanning);
				}

				return transportCollection;
			}
		}

		public DocBaseConsolCollection ShipmentConsols
		{
			get { return GetDocNewConsolCollection(CommonShipment.Consols, Factory); }
		}

		protected virtual DocBaseConsolCollection GetDocNewConsolCollection(ConsolCollection consols, BusinessObjectFactory factory)
		{
			return new DocBaseConsolCollection(CommonShipment.Consols, factory);
		}

		public DocOrderCollection Orders
		{
			get
			{
				DocOrderCollection orderCollection = new DocOrderCollection(CommonShipment.Factory);
				ForwardingShipment shipmentWithOrder = CommonShipment as ForwardingShipment;
				if (shipmentWithOrder != null)
				{
					foreach (Order order in shipmentWithOrder.AttachedOrders)
					{
						DocOrder orderToAdd = DocOrder.New(order, Factory);
						orderCollection.Add(orderToAdd);
					}
				}
				return orderCollection;
			}
		}

		public DocPackLinesCollection PackLines
		{
			get
			{
				DocPackLinesCollection result = new DocPackLinesCollection(CommonShipment.InnerPackLines, Factory);
				foreach (DocPackLines packLine in result)
				{
					packLine.CurrentConsol = CurrentConsol;
				}
				return result;
			}
		}

		public DocPackLinesCollection OuterPackLineCollection
		{
			get
			{
				DocPackLinesCollection docPackLineCollection = new DocPackLinesCollection(Factory);

				foreach (PackLine packLine in CommonShipment.OuterPackLines)
				{
					if (packLine.JL_FreightMode == "OUT")
					{
						DocPackLines docPackLine = DocPackLines.New(packLine, CommonShipment, Factory);
						docPackLine.CurrentConsol = CurrentConsol;
						docPackLineCollection.Add(docPackLine);
					}
				}

				return docPackLineCollection;
			}
		}

		#region Containers

		public IDocContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = NewContainersCollection();
				}

				return fContainers;
			}
		}

		protected IDocSimpleContainerCollection NewSimpleContainersCollection()
		{
			IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(CommonShipment.Factory);
			result.AddRange(NewContainersCollection());
			return result;
		}

		protected virtual IDocContainerCollection NewContainersCollection()
		{
			IDocContainerCollection result = new IDocContainerCollection(CommonShipment.Factory);

			if (IsImportDocument || IsExportDocument)
			{
				result.AddRange(GetContainersInConsol(Consol));
			}
			else
			{
				if (CommonShipment.Containers != null)
				{
					foreach (CommonContainer thisContainer in CommonShipment.Containers)
					{
						result.Add(DocContainer.New(thisContainer, CommonShipment, Factory));
					}
				}
			}

			if (result.Count == 0 && PackingMode == Constants.ContainerModes.FCL && Consol != null && Consol.Shipments.Count == 1)
			{
				foreach (DocContainer container in Consol.Containers)
				{
					result.Add(DocContainer.New((CommonContainer)container.WrappedObject, CommonShipment, Factory));
				}
			}

			return result;
		}

		#endregion

		#region ColoadShipments

		public virtual DocShipmentCollection ColoadShipments
		{
			get
			{
				if (fColoadShipments == null)
				{
					fColoadShipments = new DocShipmentCollection(CommonShipment.Factory);
					foreach (CommonShipment shipment in CommonShipment.CoLoadShipments)
					{
						fColoadShipments.Add(GetNewDocShipment(CommonShipment.Factory, shipment.PK));
					}

					fColoadShipments.SortOnHBL();
				}
				return fColoadShipments;
			}
		}
		DocShipmentCollection fColoadShipments;

		protected virtual DocShipment GetNewDocShipment(BusinessObjectFactory factory, ZGuid shipmenPK)
		{
			return DocShipment.New(CommonShipment.Factory, shipmenPK);
		}

		#endregion

		// This factory never gets saved
		BusinessObjectFactory DummyFactory
		{
			get
			{
				if (fDummyFactory == null)
				{
					fDummyFactory = new BusinessObjectFactory();
				}

				return fDummyFactory;
			}
		}

		BusinessObjectFactory fDummyFactory;

		public DocBaseConsolCollection ConsolHack // This is to hack the break of sections in the template
		{
			get
			{
				DocBaseConsolCollection result = new DocBaseConsolCollection(DummyFactory);
				result.Add(DocBaseConsol.New(DummyFactory.New<ForwardingConsol>(), Factory));

				return result;
			}
		}

		public DocTransportCollection ShipmentTransportPlanning
		{
			get { return new DocTransportCollection(CommonShipment.Transports, Factory); }
		}

		protected DynamicBusinessObjectCollection fContainerPackageCountCollection;
		internal DynamicBusinessObjectCollection ContainerPackageCountCollection
		{
			get
			{
				if (fContainerPackageCountCollection == null)
				{
					fContainerPackageCountCollection = new DynamicBusinessObjectCollection(Factory);

					ZString sqlString = "select " + RefContainerSchema.Constants.RC_Code + ", count(distinct " + JobContainerSchema.Constants.JC_ContainerNum + ") as Containers, sum(" + JobPackLinesSchema.Constants.JL_PackageCount + ") as Packages From " + JobContainerSchema.Constants.SqlSchemaName + "." + JobContainerSchema.Constants.TableName
							+ " inner join " + RefContainerSchema.Constants.SqlSchemaName + "." + RefContainerSchema.Constants.TableName + " on " + JobContainerSchema.Constants.JC_RC + " = " + RefContainerSchema.Constants.PK
							+ " inner join " + JobContainerPackPivotSchema.Constants.SqlSchemaName + "." + JobContainerPackPivotSchema.Constants.TableName + " on " + JobContainerPackPivotSchema.Constants.J6_JC + " = " + JobContainerSchema.Constants.PK
							+ " inner join " + JobPackLinesSchema.Constants.SqlSchemaName + "." + JobPackLinesSchema.Constants.TableName + " on " + JobContainerPackPivotSchema.Constants.J6_JL + " = " + JobPackLinesSchema.Constants.PK
							+ " inner join " + JobShipmentSchema.Constants.SqlSchemaName + "." + JobShipmentSchema.Constants.TableName + " on " + JobPackLinesSchema.Constants.JL_JS + " = " + JobShipmentSchema.Constants.PK
							+ " where " + ((IsMasterCoLoadShipmentWithSubShipments) ? JobShipmentSchema.Constants.JS_JS_ColoadMasterShipment : JobShipmentSchema.Constants.PK) + " = @ShipmentPK and "
							+ JobContainerSchema.Constants.JC_ContainerMode + " = @ContainerMode"
							+ " group by " + RefContainerSchema.Constants.RC_Code
							+ " order by Containers";

					ZSqlParameterCollection @params = new ZSqlParameterCollection();
					@params.Add("@ShipmentPK", ((BusinessObject)WrappedObject).PK, JobShipmentSchema.PK);
					@params.Add("@ContainerMode", Core.Constants.ContainerModes.FCL, JobContainerSchema.JC_ContainerMode);

					fContainerPackageCountCollection.Load(sqlString, @params);
				}

				return fContainerPackageCountCollection;
			}
		}

		#endregion

		#region Consol Details

		public ZBool IsExportConsol { get; set; }
		public ZBool IsUSConsol { get; set; }
		public ZBool IsSeaConsol { get; set; }

		#endregion

		#region Container Fields

		public ZString SealNumberHeading
		{
			get { return (TransportMode == Core.Constants.TransportModes.Air) ? Res.GetString("c1defc8a-b13a-4bd2-92c9-fb6d28abad1a", "RATE CLASS") : Res.GetString("ce23c9ab-0c37-44d2-a63b-ccd7b7c5fde0", "SEAL NO."); }
		}

		public ZString SealHeading
		{
			get { return (TransportMode == Core.Constants.TransportModes.Air) ? Res.GetString("c3366e7b-e8c9-402a-9d3f-5f68aea36e11", "RATE CLASS") : Res.GetString("5870e409-6b64-40d2-8279-8cdc24b964cd", "SEAL"); }
		}

		[Obsolete("Use AvailableDate instead", false)]
		public ZDateTime ImportContainerAvailableDate
		{
			get { return AvailableDate; }
		}

		public virtual ZDateTime AvailableDate
		{
			get
			{
				if (PackingMode == Core.Constants.ContainerModes.FCL ||
						PackingMode == Core.Constants.ContainerModes.ULD ||
						PackingMode == Core.Constants.ContainerModes.Bulk ||
						PackingMode == Core.Constants.ContainerModes.Liquid ||
						(Consol != null && Consol.ConsolMode == Constants.ContainerModes.BuyersConsol))
				{
					return DocsAndCartage.FCLAvailable;
				}
				else
				{
					return DocsAndCartage.LCLAvailable;
				}
			}
		}

		[Obsolete("Use StorageCommenceDate instead", false)]
		public ZDateTime ImportContainerStorageCommenceDate
		{
			get { return StorageCommenceDate; }
		}

		public virtual ZDateTime StorageCommenceDate
		{
			get
			{
				if (PackingMode == Core.Constants.ContainerModes.FCL ||
						PackingMode == Core.Constants.ContainerModes.ULD ||
						PackingMode == Core.Constants.ContainerModes.Bulk ||
						PackingMode == Core.Constants.ContainerModes.Liquid ||
						(Consol != null && Consol.ConsolMode == Constants.ContainerModes.BuyersConsol))
				{
					return DocsAndCartage.FCLStorageCommences;
				}
				else
				{
					return DocsAndCartage.LCLStorageCommences;
				}
			}
		}

		protected ZDateTime GetContainerDates(ZBool getAvailabilityDate)
		{
			ZDateTime latest = ZDateTime.Empty;
			ZDateTime curDate = ZDateTime.Empty;

			if (Containers.Count > 0 && !Containers[0].ContainerNumber.IsEmpty)
			{
				foreach (IDocContainer allocatedContainer in Containers)
				{
					curDate = GetContainerDatesWithPackingModeRule(allocatedContainer, getAvailabilityDate);
					latest = (curDate.CompareTo(latest) > 0) ? curDate : latest;
				}
			}
			else if (Consol != null && Consol.Shipments.Count == 1) // containers need to be allocated if ship.>1
			{
				foreach (IDocContainer currentContainer in Consol.Containers)
				{
					curDate = GetContainerDatesWithPackingModeRule(currentContainer, getAvailabilityDate);
					latest = (curDate.CompareTo(latest) > 0) ? curDate : latest;
				}
			}
			if (latest.IsEmpty)
			{
				latest = GetContainerDatesWithPackingModeRule(null, getAvailabilityDate);
			}
			return latest;
		}

		protected ZDateTime GetContainerDatesWithPackingModeRule(IDocContainer container, ZBool getAvailabilityDate)
		{
			ZDateTime result = ZDateTime.Empty;
			ZBool getFCLDate = ZBool.False;
			switch (PackingMode)
			{
				case Core.Constants.ContainerModes.FCL:
					getFCLDate = ZBool.True;
					result = GetContainerDateFromAContainer(container, getAvailabilityDate, getFCLDate);
					break;

				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
				case Core.Constants.ContainerModes.BreakBulk:
					getFCLDate = ZBool.False;
					result = GetContainerDateFromAContainer(container, getAvailabilityDate, getFCLDate);
					break;

				default:
					if (PackingMode == Core.Constants.ContainerModes.LCL &&
							Consol != null &&
							Consol.ConsolMode == Constants.ContainerModes.BuyersConsol)
					{
						getFCLDate = ZBool.True;
						result = GetContainerDateFromAContainer(container, getAvailabilityDate, getFCLDate);
					}
					else
					{
						getFCLDate = ZBool.False;
						result = GetContainerDateFromAContainer(container, getAvailabilityDate, getFCLDate);
					}
					break;
			}
			if (container == null)
			{
				result = GetContainerDateFromConsol(getAvailabilityDate, getFCLDate);
			}
			return result;
		}

		protected ZDateTime GetContainerDateFromAContainer(IDocContainer container, ZBool getAvailabilityDate, ZBool getFCLDate)
		{
			ZDateTime result = ZDateTime.Empty;
			if (container != null)
			{
				if (getAvailabilityDate)
				{
					result = (getFCLDate) ? container.ContainerAvailable : container.LCLAvailable;
				}
				else
				{
					result = (getFCLDate) ? container.StorageCommences : container.LCLStorageCommences;
				}
			}
			return result;
		}

		protected ZDateTime GetContainerDateFromConsol(ZBool getAvailabilityDate, ZBool getFCLDate)
		{
			if (Consol != null)
			{
				if (getAvailabilityDate)
				{
					return (getFCLDate) ? Consol.AvailabilityDate : Consol.LCLAvailabilityDate;
				}
				else
				{
					return (getFCLDate) ? Consol.StorageCommences : Consol.LCLStorageCommences;
				}
			}
			return ZDateTime.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching a constant string value")]
		public ZString ContainerLine
		{
			get
			{
				ZString result = "";
				int count = 0;

				foreach (DocContainer container in Containers)
				{
					if (container != null && container.ContainerNumber != "ALL CONTAINERS")
					{
						count++;

						if (count > MaximumContainersOnALine)
						{
							result = result.TrimEndIncludingWhiteSpace(',') + " ...";
							break;
						}

						result += container.ContainerNumber + ", ";
					}
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		const int MaximumContainersOnALine = 7;

		DocContainerCollectionHelper FreightContainerSupport
		{
			get { return freightContainerSupport ?? (freightContainerSupport = new DocContainerCollectionHelper(MaximumContainersWithTypeOnALine, false)); }
		}
		DocContainerCollectionHelper freightContainerSupport;

		public ZString ContainerNumberAndTypeLine
		{
			get { return FreightContainerSupport.ContainerNumberAndType(NewSimpleContainersCollection()); }
		}

		public ZString ContainerNumberTypeAndClientRefLine
		{
			get { return FreightContainerSupport.ContainerNumberAndType(NewSimpleContainersCollection(), true); }
		}

		public ZString ContainerNumberOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocContainer currentContainer in Containers)
				{
					if (currentContainer != null)
					{
						result += currentContainer.ContainerNumber + System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public ZString ContainerTypeOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocContainer currentContainer in Containers)
				{
					if (currentContainer != null)
					{
						result += currentContainer.Container != null ?
									currentContainer.Container.Code + System.Environment.NewLine :
									System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public ZBool PrintPageWithContainerNumber
		{
			get { return (Containers.Count > MaximumContainersWithTypeOnALine && AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value); }
		}

		const int MaximumContainersWithTypeOnALine = 5;

		#region Not Allocated

		public ZString NotAllocatedWeight
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal remainingWeight = 0;
				ZDecimal total = 0;
				if (Consol != null)
				{
					if (Consol.Containers.Count > 0 && OuterPackLineCollection.Count > 0 && PackingMode != Core.Constants.ContainerModes.BreakBulk && PackingMode != Core.Constants.ContainerModes.Bulk && PackingMode != Core.Constants.ContainerModes.Liquid)
					{
						foreach (DocContainer container in Containers)
						{
							total += Core.Constants.Weight.ConvertSafe(container.TotalAllocatedShipmentWeight, container.GrossWeightUQ, CommonShipment.JS_UnitOfWeight);
						}
						string displayOption = GetWeightVolumeDisplayOption();
						ZDecimal shipmentWeight = CommonShipment.GetWeightForDoc(displayOption);

						if (shipmentWeight > total)
						{
							remainingWeight = (shipmentWeight - total);
							result = FormatNumber(remainingWeight) + " " + WeightUnit;
						}
					}
				}

				return result;
			}
		}

		public ZString NotAllocatedVolume
		{
			get
			{
				ZString result = ZString.Empty;
				ZDecimal remainingVolume = 0;
				ZDecimal total = 0;
				if (Consol != null)
				{
					if (Consol.Containers.Count > 0 && OuterPackLineCollection.Count > 0 && PackingMode != Core.Constants.ContainerModes.BreakBulk && PackingMode != Core.Constants.ContainerModes.Bulk && PackingMode != Core.Constants.ContainerModes.Liquid)
					{
						foreach (DocContainer container in Containers)
						{
							total += container.TotalAllocatedShipmentVolume;
						}
						string displayOption = GetWeightVolumeDisplayOption();
						ZDecimal shipmentVolume = CommonShipment.GetVolumeForDoc(displayOption);

						if (shipmentVolume > total)
						{
							remainingVolume = (shipmentVolume - total);
							result = FormatNumber(remainingVolume) + " " + VolumeUnit;
						}
					}
				}
				return result;
			}
		}

		public ZString NotAllocatedPackages
		{
			get
			{
				ZString result = ZString.Empty;
				ZInt remainingPackages = 0;
				ZInt total = 0;
				if (Consol != null)
				{
					if (Consol.Containers.Count > 0 && OuterPackLineCollection.Count > 0 && PackingMode != Core.Constants.ContainerModes.BreakBulk && PackingMode != Core.Constants.ContainerModes.Bulk && PackingMode != Core.Constants.ContainerModes.Liquid)
					{
						foreach (DocContainer container in Containers)
						{
							total += container.TotalAllocatedShipmentPackages;
						}

						if (OuterPacks > total)
						{
							remainingPackages = (OuterPacks - total);
							result = remainingPackages.ToString();
						}
					}
				}
				return result;
			}
		}

		public ZBool IsNotAllocatedPresent
		{
			get
			{
				ZBool result = false;

				ZDecimal totalWeight = 0;
				ZDecimal totalVolume = 0;
				ZInt totalPacks = 0;

				if (Consol != null)
				{
					if (Consol.Containers.Count > 0 && OuterPackLineCollection.Count > 0 && PackingMode != Core.Constants.ContainerModes.BreakBulk && PackingMode != Core.Constants.ContainerModes.Bulk && PackingMode != Core.Constants.ContainerModes.Liquid)
					{
						foreach (DocContainer container in Containers)
						{
							totalWeight += container.TotalAllocatedShipmentWeight;
							totalVolume += container.TotalAllocatedShipmentVolume;
							totalPacks += container.TotalAllocatedShipmentPackages;
						}

						string displayOption = GetWeightVolumeDisplayOption();
						ZDecimal shipmentWeight = CommonShipment.GetWeightForDoc(displayOption);
						ZDecimal shipmentVolume = CommonShipment.GetVolumeForDoc(displayOption);

						result = (shipmentWeight > totalWeight || shipmentVolume > totalVolume || OuterPacks > totalPacks);
					}
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Shipment Weight Volume Chargeable

		public virtual ZString Weight
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(CommonShipment.GetWeightForDoc(displayOption));
			}
		}

		public virtual ZDecimal WeightValue
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return CommonShipment.GetWeightForDoc(displayOption);
			}
		}

		public virtual ZString Volume
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(CommonShipment.GetVolumeForDoc(displayOption));
			}
		}

		public virtual ZDecimal VolumeValue
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return CommonShipment.GetVolumeForDoc(displayOption);
			}
		}

		public ZString Chargeable
		{
			get
			{
				string displayOption = GetWeightVolumeDisplayOption();
				return FormatNumber(CommonShipment.GetChargeableForDoc(displayOption));
			}
		}

		public ZDecimal ActualChargeable
		{
			get
			{
				return CommonShipment.JS_ActualChargeable;
			}
		}

		public ZDecimal ActualVolume
		{
			get
			{
				return CommonShipment.JS_ActualVolume;
			}
		}

		public ZDecimal ActualWeight
		{
			get
			{
				return CommonShipment.JS_ActualWeight;
			}
		}

		public ZDecimal DocumentedChargeable
		{
			get
			{
				return CommonShipment.JS_DocumentedChargeable;
			}
		}

		public ZDecimal DocumentedVolume
		{
			get
			{
				return CommonShipment.JS_DocumentedVolume;
			}
		}

		public ZDecimal DocumentedWeight
		{
			get
			{
				return CommonShipment.JS_DocumentedWeight;
			}
		}

		public ZDecimal ManifestedWeight
		{
			get
			{
				return CommonShipment.JS_ManifestedWeight;
			}
		}

		public ZDecimal ManifestedVolume
		{
			get
			{
				return CommonShipment.JS_ManifestedVolume;
			}
		}

		public ZDecimal ManifestedChargeable
		{
			get
			{
				return CommonShipment.JS_ManifestedChargeable;
			}
		}

		public ZString ColoadMasterManifestVolume
		{
			get
			{
				ZString result = Volume + " " + VolumeUnit;
				if (TransportMode == Constants.TransportModes.Air)
				{
					return Env.Registry.CoLoadMasterManifestDisplayVolumeWhenAir ? result : ZString.Empty;
				}
				else
				{
					return result;
				}
			}
		}

		public ZString ColoadMasterManifestChargeable
		{
			get
			{
				ZString result = Chargeable + " " + ChargeableUnit;
				if (TransportMode == Constants.TransportModes.Air)
				{
					return Env.Registry.CoLoadMasterManifestDisplayChargeableWhenAir ? result : ZString.Empty;
				}
				else if (TransportMode == Constants.TransportModes.Sea)
				{
					return Env.Registry.CoLoadMasterManifestDisplayChargeableWhenSea ? result : ZString.Empty;
				}
				else
				{
					return result;
				}
			}
		}

		public ZString ColoadMasterManifestVolumeHeading
		{
			get
			{
				if (TransportMode == Constants.TransportModes.Air)
				{
					return Env.Registry.CoLoadMasterManifestDisplayVolumeWhenAir ? Res.GetString("e918ed2e-b9e2-4434-853f-3f1bdb152a7e", "VOLUME") : "";
				}
				else
				{
					return Res.GetString("e918ed2e-b9e2-4434-853f-3f1bdb152a7e", "VOLUME");
				}
			}
		}

		public ZString ColoadMasterManifestChargeableHeading
		{
			get
			{
				if (TransportMode == Constants.TransportModes.Air)
				{
					return Env.Registry.CoLoadMasterManifestDisplayChargeableWhenAir ? Res.GetString("07c534d1-d13c-4d38-b3ce-5bfdd79056c6", "CHARGEABLE") : "";
				}
				else if (TransportMode == Constants.TransportModes.Sea)
				{
					return Env.Registry.CoLoadMasterManifestDisplayChargeableWhenSea ? Res.GetString("07c534d1-d13c-4d38-b3ce-5bfdd79056c6", "CHARGEABLE") : "";
				}
				else
				{
					return Res.GetString("07c534d1-d13c-4d38-b3ce-5bfdd79056c6", "CHARGEABLE");
				}
			}
		}

		public ZString ExportManifestVolume
		{
			get
			{
				ZString result = Volume + " " + VolumeUnit;
				if (TransportMode == Core.Constants.TransportModes.Air && IsExportDocument)
				{
					return (Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir) ? result : ZString.Empty;
				}
				else
				{
					return result;
				}
			}
		}

		public ZString ExportManifestVolumeHeading
		{
			get
			{
				if (TransportMode == Core.Constants.TransportModes.Air && IsExportDocument)
				{
					return (Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir) ? Res.GetString("8d77b95a-78cc-4e3a-92ee-2d644bb8c8c8", "Volume:") : "";
				}
				else
				{
					return Res.GetString("ba737fda-eb5f-4699-9f84-16e3a15774ba", "Volume:");
				}
			}
		}

		public ZString WeightInKG
		{
			get
			{
				ZString result = FormatNumber(ActualWeight);
				if (WeightUnit != Core.Constants.Weight.Kilograms)
				{
					result = FormatNumber(Core.Constants.Weight.ConvertSafe(ActualWeight, WeightUnit, Core.Constants.Weight.Kilograms));
				}
				return result;
			}
		}

		public ZString VolumeInM3
		{
			get
			{
				ZString result = FormatNumber(ActualVolume);
				if (VolumeUnit != Core.Constants.Volume.CubicMetres)
				{
					result = FormatNumber(Core.Constants.Volume.ConvertSafe(ActualVolume, VolumeUnit, Core.Constants.Volume.CubicMetres));
				}
				return result;
			}
		}

		#endregion

		#region Coload Master Manifest

		public ZString ColoadMasterManifestTransportHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsExportDocument)
				{
					if (Consol != null)
					{
						result = Consol.TransportHeading;
					}
				}
				else if (Consol != null)
				{
					result = Consol.TransportHeading;
				}

				return result;
			}
		}

		public ZString ColoadMasterManifestTransportInfo
		{
			get
			{
				if (IsExportDocument)
				{
					return (Consol != null) ? Consol.ConsolTransportInfo : ZString.Empty;
				}
				else
				{
					return (Consol != null) ? Consol.ConsolTransportInfo : ZString.Empty;
				}
			}
		}

		public ZString ColoadMasterManifestShippingLine
		{
			get
			{
				if (IsExportDocument)
				{
					return (Consol != null && Consol.ShippingLine != null) ? Consol.ShippingLine.Name : ZString.Empty;
				}
				else
				{
					return (Consol != null && Consol.ShippingLine != null) ? Consol.ShippingLine.Name : ZString.Empty;
				}
			}
		}

		#endregion

		#region CFS Labels

		public ZString ExportPortOfDischarge
		{
			get { return (Sailing != null && Sailing.PortOfDischarge != null) ? Sailing.PortOfDischarge.Code : ZString.Empty; }
		}

		public ZString DestinationCode
		{
			get { return CommonShipment.JS_RL_NKDestination; }
		}

		public ZString ConNote
		{
			get { return CommonShipment.JS_CartageWaybill; }
		}

		#endregion

		#region Labels

		public DocOuterPackCollection ShipmentLabels
		{
			get
			{
				DocOuterPackCollection result = new DocOuterPackCollection(Factory);
				ZInt pieceCount = 0;

				foreach (DocPackLines line in OuterPackLineCollection)
				{
					bool @continue = true;

					for (int i = 0; i < line.PackageCount; i++)
					{
						OuterPack outerPack = new OuterPack();
						outerPack.DocShipment = this;
						outerPack.DocPackLines = line;
						outerPack.Number = ++pieceCount;
						outerPack.PackType = line.PackType;

						TextBarcode primaryBarcode = new TextBarcode(ShipmentNumber);
						outerPack.PrimaryBarcode = primaryBarcode.TextAs128sFontString;
						outerPack.PrimaryBarcodeDisplayText = primaryBarcode.TextToEncode;

						result.Add(DocOuterPack.New(outerPack, Factory));
						if (NumberOfLabelsToPrint > 0 &&
								pieceCount >= NumberOfLabelsToPrint)
						{
							@continue = false;
							break;
						}
					}

					if (!@continue)
					{
						break;
					}
				}
				return result;
			}
		}

		#endregion

		#region Booking

		#region Wrapper Fields
		public virtual DocOrganisation Carrier
		{
			get
			{
				return (Consol != null) ? Consol.ShippingLine : null;
			}
		}

		public virtual DocUNLOCO BookingPortOfLoading
		{
			get
			{
				return (Consol != null) ? Consol.PortOfLoading : null;
			}
		}

		public virtual DocUNLOCO BookingPortOfDischarge
		{
			get
			{
				return (Consol != null) ? Consol.PortOfDischarge : null;
			}
		}

		public virtual DocDocAddress BookingExportReceivingDepot
		{
			get
			{
				return this.ExportReceivingDepot;
			}
		}

		public DocTransportCollection BookingTransportPlanning
		{
			get { return Suppression.GetObject(CompleteRouting, CommonShipment, SuppressFields.TransportInfo, new DocTransportCollection(Factory), DocumentContactType); }
		}

		#endregion

		#region ZString Fields

		public virtual ZString BookingTransportHeading
		{
			get
			{
				return (Consol != null) ? Consol.TransportHeading : ZString.Empty;
			}
		}

		public virtual ZString BookingTransport
		{
			get
			{
				return (Consol != null) ? Consol.ConsolTransportInfo : ZString.Empty;
			}
		}

		public virtual ZString BookingDepartureReference
		{
			get
			{
				return GetDepartureReferenceFromSailing(Sailing);
			}
		}

		public virtual ZString BookingDepartureReferenceHeading
		{
			get
			{
				return GetDepartureReferenceHeading(BookingPortOfLoading);
			}
		}

		public ZString BookingTransportPlanningHeading
		{
			get { return Res.GetString("21172f9e-7d86-4e45-bf81-67ba5648035e", "TRANSPORT PLANNING"); }
		}

		public ZString DepotOrCTOHeading
		{
			get
			{
				return CommonShipment.DepotOrCTOHeading;
			}
		}

		public virtual ZString ContainerCount
		{
			get
			{
				ZString result = ZString.Empty;
				Hashtable containerCountTable = new Hashtable();
				CountContainers(containerCountTable);
				foreach (object key in containerCountTable.Keys)
				{
					result += containerCountTable[key] + " X " + key.ToString() + ", ";
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString MarksAndNumbersHeading
		{
			get
			{
				ZString result = ZString.Empty;
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
					case Core.Constants.TransportModes.SeaAir:
						result = (PackingMode == Core.Constants.ContainerModes.Loose) ? Env.Registry.MarksAndNumbersHeadingForLSE : Env.Registry.MarksAndNumbersHeadingForNonLSE;
						break;

					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
						result = (PackingMode == Core.Constants.ContainerModes.FCL) ? Env.Registry.MarksAndNumbersHeadingForFCL : Env.Registry.MarksAndNumbersHeadingForNonFCL;
						break;
				}
				if (result.Trim().IsEmpty)
				{
					result = Res.GetString("261d9bed-5698-4a06-94fb-755680ea034c", "MARKS AND NUMBERS");
				}
				return result;
			}
		}

		public virtual ZString BookingContainerLayout
		{
			get
			{
				ZString result = ZString.Empty;
				if (Containers.Count == 1)
				{
					result = "1";
				}
				else if (Containers.Count > 1)
				{
					ZInt releaseNumCount = 0;
					ZInt emptyReqByCount = 0;
					ZInt pickupDateCount = 0;
					foreach (DocContainer container in Containers)
					{
						if (!container.ReleaseNum.IsEmpty)
						{
							releaseNumCount++;
						}

						if (!container.EmptyRequired.IsEmpty)
						{
							emptyReqByCount++;
						}

						if (!container.FullPickDate.IsEmpty)
						{
							pickupDateCount++;
						}
					}
					if (releaseNumCount <= 1 && emptyReqByCount <= 1 && pickupDateCount <= 1)
					{
						result = "2";
					}
					else
					{
						result = "3";
					}
				}
				return result;
			}
		}

		public virtual ZString BookingContainerReleaseNumber
		{
			get
			{
				foreach (DocContainer container in Containers)
				{
					if (!container.ReleaseNum.IsEmpty)
					{
						return container.ReleaseNum;
					}
				}
				return ZString.Empty;
			}
		}

		public virtual ZDateTime BookingContainerEmptyRequiredDate
		{
			get
			{
				foreach (DocContainer container in Containers)
				{
					if (!container.EmptyRequired.IsEmpty)
					{
						return container.EmptyRequired;
					}
				}
				return ZDateTime.Empty;
			}
		}

		public virtual ZDateTime BookingContainerPickupFullDate
		{
			get
			{
				foreach (DocContainer container in Containers)
				{
					if (!container.FullPickDate.IsEmpty)
					{
						return container.FullPickDate;
					}
				}
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region ZDateTime Fields

		public virtual ZString BookingETD
		{
			get { return (Consol != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Consol.ETD) : ZString.Empty; }
		}

		public virtual ZString BookingETA
		{
			get { return (Consol != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Consol.ETA) : ZString.Empty; }
		}

		ZString BookingATD
		{
			get { return (Consol != null) ? FreightHelperClass.FormatETAETDDate(TransportModeIsSea, Consol.ATD) : ZString.Empty; }
		}

		public virtual ZDateTime BookingCutOffDate
		{
			get
			{
				ZDateTime cutOff = ZDateTime.Empty;
				if (Consol != null)
				{
					cutOff = (PackingMode == Core.Constants.ContainerModes.FCL) ? Consol.FCLCutOff : Consol.LCLCutOff;
				}
				else if (Sailing != null)
				{
					cutOff = (PackingMode == Core.Constants.ContainerModes.FCL) ? Sailing.FCLCutOff : Sailing.LCLCutOff;
				}
				return cutOff;
			}
		}

		#endregion

		#region ZBool Fields

		public virtual ZBool ShowMarksAndNumbersOnBookingConfirmation
		{
			get
			{
				switch (TransportMode)
				{
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
					case Core.Constants.TransportModes.SeaAir:
						return (PackingMode == Core.Constants.ContainerModes.Loose) ? Env.Registry.ShowMarksAndNumbersForLSE : Env.Registry.ShowMarksAndNumbersForNonLSE;

					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.Rail:
					case Core.Constants.TransportModes.Road:
						return (PackingMode == Core.Constants.ContainerModes.FCL) ? Env.Registry.ShowMarksAndNumbersForFCL : Env.Registry.ShowMarksAndNumbersForNonFCL;

					default:
						return ZBool.True;
				}
			}
		}

		public virtual ZBool ShowEmptyRequiredByHeading
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocContainer container in Containers)
				{
					if (!container.EmptyRequired.IsEmpty)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}

		public virtual ZBool ShowFullPickupByHeading
		{
			get
			{
				ZBool result = ZBool.False;
				foreach (DocContainer container in Containers)
				{
					if (!container.FullPickDate.IsEmpty)
					{
						result = ZBool.True;
						break;
					}
				}
				return result;
			}
		}

		public ZBool IsBookingShipment
		{
			get
			{
				return IsBooking && !IsForwardRegistered;
			}
		}

		#endregion

		protected ZBool IsTransportPlanningMainTransport(DocTransport transport)
		{
			return (transport.TransportMode == Consol.TransportMode &&
					transport.VesselName == Consol.VesselName &&
					transport.VoyageFlight == Consol.VoyageNumber &&
					IsSameUNLOCO(transport.PortOfLoading, Consol.PortOfLoading) &&
					IsSameUNLOCO(transport.PortOfDischarge, Consol.PortOfDischarge));
		}

		protected ZBool IsSameUNLOCO(DocUNLOCO loco1, DocUNLOCO loco2)
		{
			if (loco1 != null && loco2 != null)
			{
				return (loco1.Code == loco2.Code);
			}
			else
			{
				return (loco1 == null && loco2 == null);
			}
		}

		void CountContainers(Hashtable containerCountTable)
		{
			foreach (DocFreightBaseContainer container in Containers)
			{
				if (container.Container != null)
				{
					CountContainers(containerCountTable, container.Container, container.ContainerCount);
				}
			}
		}

		protected void CountContainers(Hashtable containerCountTable, DocRefContainer container, ZInt containerCount)
		{
			if (container != null)
			{
				if (containerCountTable.ContainsKey(container.Code))
				{
					ZInt count = 0;
					if (ZInt.CanParse(containerCountTable[container.Code].ToString()))
					{
						count = ZInt.Parse(containerCountTable[container.Code].ToString());
						count = (containerCount > 1) ? count + containerCount : count + 1;
						containerCountTable[container.Code] = count;
					}
				}
				else
				{
					containerCountTable.Add(container.Code, containerCount);
				}
			}
		}

		protected ZString GetDepartureReferenceFromSailing(DocSailing sailing)
		{
			ZString result = "";
			if (TransportMode == Core.Constants.TransportModes.Sea || TransportMode == Core.Constants.TransportModes.Rail)
			{
				if (sailing != null)
				{
					result += sailing.DepartureReference;

					if (IsMalaysianBooking(BookingPortOfLoading))
					{
						if (sailing.VoyageOrigin != null)
						{
							if (!sailing.VoyageOrigin.Berth.IsEmpty)
							{
								result += " / " + sailing.VoyageOrigin.Berth;
							}
						}
					}
				}
			}

			return result;
		}

		protected ZString GetDepartureReferenceHeading(DocUNLOCO loadPort)
		{
			return IsMalaysianBooking(loadPort) ? Res.GetString("b236c636-d222-4f6e-91e1-b50aac41ed21", "SCN / Berth") : Res.GetString("82dc0f07-e782-42a9-8a34-de3522e6963d", "DEPARTURE REFERENCE");
		}

		protected ZBool IsMalaysianBooking(DocUNLOCO loadPort)
		{
			if (CurrentCompany.Country != null && CurrentCompany.Country.Code.StartsWith("MY"))
			{
				return (loadPort != null && loadPort.Code.StartsWith("MY"));
			}
			return ZBool.False;
		}

		#endregion

		#region Forwarding Instruction

		public ZString ShippersReference
		{
			get
			{
				return (!ShippersRef.IsEmpty) ? ShippersRef : ShipmentNumber;
			}
		}

		public ZString ExportPermit
		{
			get
			{
				ZString result = "";

				if (CommonShipment.CustomsEntryNumberTypeIsAnExemptionCode)
				{
					result = CustomsEntryNumberType;
				}
				else if (!CustomsEntryNumber.IsEmpty)
				{
					result = (CustomsEntryNumberType + " " + CustomsEntryNumber).Trim();
				}

				return result;
			}
		}

		public ZString FreightTerms
		{
			get
			{
				ZString result = ZString.Empty;

				if (!INCO.IsEmpty)
				{
					result = INCO + " (" + INCODescription + ")";
				}
				if (!PaymentTermDisplay.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						result += " - ";
					}
					result += PaymentTermDisplay;
				}

				return result;
			}
		}

		public ZString MakeBillOutTo
		{
			get
			{
				return Res.GetString("33a4ef82-0856-48d8-b709-73bc5e38a599", "Shipper");
			}
		}

		public ZString GoodsSummary
		{
			get
			{
				ZString result = ZString.Empty;

				switch (PackingMode)
				{
					case Constants.ContainerModes.FCL:
					case Constants.ContainerModes.Groupage:
					case Constants.ContainerModes.BuyersConsol:
						ZInt totalFCLPackages = 0;
						foreach (BusinessObject bizO in ContainerPackageCountCollection)
						{
							ZString type = new ZString(bizO[RefContainer.Schema.RC_Code]);
							ZInt containers = new ZInt(bizO[nameof(Containers)]);
							ZInt packages = new ZInt(bizO[nameof(Packages)]);
							totalFCLPackages += packages;
							result += Res.GetString("c112b8d0-b162-40bc-b410-fe0c1e50d638", "{0} x {1} Container {2}{3} Package(s);", containers.ToString(), type, STC_Label, packages.ToString()) + " ";
						}
						ZInt totalLCLPackages = OuterPacks - totalFCLPackages;
						if (totalLCLPackages > 0)
						{
							result += totalLCLPackages.ToString() + " " + Res.GetString("7f515a5d-077c-49e9-bc61-1c49382e8f0b", "LCL Package(s)");
						}
						break;

					default:
						result += Res.GetString("3a653787-e7ba-40dc-8805-618fdfb65f45", "Total Package(s):") + " " + OuterPacks.ToString();
						break;
				}

				return result;
			}
		}

		#region STC Label For US Bound

		public ZString STC_Label
		{
			get { return CommonShipment.ShipmentPassesThroughCountry(Core.Constants.CountryCodes.UnitedStates) ? "" : "STC "; }
		}

		#endregion

		public ZString LocationOfGoods
		{
			get { return CommonShipment.ConsignorPickupAddress != null ? DocDocAddress.New(CommonShipment.ConsignorPickupAddress, Factory).PostalAddress : ZString.Empty; }
		}

		public ZString LocationOfGoodsExcludeName
		{
			get
			{
				return (Consignor != null && Consignor.PickUpAddress != null) ? Consignor.PickUpAddress.PostalAddressExcludeName : ZString.Empty;
			}
		}

		public ZString GoodsReceivalPoint
		{
			get
			{
				ZString result = ZString.Empty;

				if (ExportReceivingDepot != null)
				{
					result = ExportReceivingDepot.PostalAddress;
				}
				else if (Consol != null)
				{
					if (PackingMode == Constants.ContainerModes.FCL)
					{
						if (Consol.DepartureCTOAddress != null)
						{
							result = Consol.DepartureCTOAddress.PostalAddress;
						}
					}
					else
					{
						if (Consol.PackDepotAddress != null)
						{
							result = Consol.PackDepotAddress.PostalAddress;
						}
					}
				}

				return result;
			}
		}

		public ZBool PrintContainerDetailsOnForwardingInstruction
		{
			get
			{
				return DocumentsDataRegistry.Instance.DisplayContainerDetailsOnConsol.Value;
			}
		}

		#endregion

		#region FCR
		public ZString FCRClause
		{
			get
			{
				return DocumentsDataRegistry.Instance.ForwardersCertificateOfReceiptFCRClause.Value;
			}
		}

		public System.Drawing.Image FCRLogo
		{
			get
			{
				return DocumentsDataRegistry.Instance.ForwardersCertificateOfReceiptLogo.Value;
			}
		}
		#endregion

		#region Letter of Guarantee
		public ZString GuaranteeDeclarationText
		{
			get
			{
				return DocumentsDataRegistry.Instance.GuaranteeDeclarationText.Value;
			}
		}
		#endregion

		#region Transhipment Properties
		public DocShipmentConsol ImportTranshipmentConsol
		{
			get
			{
				if (importTranshipmentConsol == null && CommonShipment.DepartureConsol != null)
				{
					importTranshipmentConsol = DocShipmentConsol.New(CommonShipment.DepartureConsol, Factory);
				}

				return importTranshipmentConsol;
			}
		}
		DocShipmentConsol importTranshipmentConsol;

		public DocShipmentConsol ExportTranshipmentConsol
		{
			get
			{
				if (exportTranshipmentConsol == null && CommonShipment.ArrivalConsol != null && CommonShipment.Consols.Count > 1)
				{
					exportTranshipmentConsol = DocShipmentConsol.New(CommonShipment.ArrivalConsol, Factory);
				}

				return exportTranshipmentConsol;
			}
		}
		DocShipmentConsol exportTranshipmentConsol;

		public IDocContainerCollection ImportTranshipmentContainers
		{
			get
			{
				return GetContainersInConsol(ImportTranshipmentConsol);
			}
		}

		public IDocContainerCollection ExportTranshipmentContainers
		{
			get
			{
				return GetContainersInConsol(ExportTranshipmentConsol);
			}
		}

		public ZString ImportTranshipmentContainerNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (IDocContainer aContainer in ImportTranshipmentContainers)
				{
					result += aContainer.ContainerNumber.IsEmpty ? "" : aContainer.ContainerNumber + ", ";
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString ExportTranshipmentContainerNumbers
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (IDocContainer aContainer in ExportTranshipmentContainers)
				{
					result += aContainer.ContainerNumber.IsEmpty ? "" : aContainer.ContainerNumber + ", ";
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get
			{
				return ShipmentNumber;
			}
		}

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (fDocManagerBarcode == null)
				{
					if (OriginLoco != null && DestinationLoco != null)
					{
						BarcodeGenerator generator = new BarcodeGenerator();
						string originLocation = (OriginLoco.IATA.IsEmpty) ? OriginLoco.Code.SubstringSafe(2, 3) : OriginLoco.IATA;
						string destinationLocation = (DestinationLoco.IATA.IsEmpty) ? DestinationLoco.Code.SubstringSafe(2, 3) : DestinationLoco.IATA;
						fDocManagerBarcode = generator.CreateShipmentBarcode(GlbCompany.CurrentCompany.GC_Code, ((IDocTypeCode)this).DocTypeCode, originLocation, destinationLocation, HouseBill);
					}
					else
					{
						fDocManagerBarcode = new TextBarcode(ZString.Empty);
					}
				}
				return fDocManagerBarcode;
			}
		}

		TextBarcode fDocManagerBarcode;

		#endregion

		#region Icelandic specific fields

		public ZDecimal TotalSellAmountOfFreightCharges
		{
			get
			{
				ZDecimal result = 0M;

				if (JobHeader != null)
				{
					if (JobHeader.JobChargesForDebtorOrLocalClient != null)
					{
						foreach (DocJobCharge jobCharge in JobHeader.JobChargesForDebtorOrLocalClient)
						{
							if (jobCharge.ChargeCode.Code == "FRT")
							{
								result += jobCharge.LocalSellAmount;
							}
						}
					}
				}

				return result;
			}
		}

		public ZDecimal SumTotalSellAndTaxAmountOfFreightCharges
		{
			get
			{
				return TotalSellAmountOfFreightCharges + TotalTaxAmountOfFreightCharges;
			}
		}

		public ZDecimal SumTotalSellAndTaxAmountOfOriginChargeGroupCharges
		{
			get
			{
				return TotalSellAmountOfOriginChargeGroupCharges + TotalTaxAmountOfOriginChargeGroupCharges;
			}
		}

		public ZDecimal TotalTaxAmountOfFreightCharges
		{
			get
			{
				ZDecimal result = 0M;

				if (JobHeader != null)
				{
					if (JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges != null)
					{
						foreach (DocJobCharge jobCharge in JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges)
						{
							if (jobCharge.ChargeCode.Code == "FRT")
							{
								result += jobCharge.TaxAmount;
							}
						}
					}
				}

				return result;
			}
		}

		public ZDecimal TotalSellAmountOfOriginChargeGroupCharges
		{
			get
			{
				ZDecimal result = 0M;

				if (JobHeader != null)
				{
					if (JobHeader.JobChargesForDebtorOrLocalClient != null)
					{
						foreach (DocJobCharge jobCharge in JobHeader.JobChargesForDebtorOrLocalClient)
						{
							if (jobCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.Origin)
							{
								result += jobCharge.LocalSellAmount;
							}
						}
					}
				}

				return result;
			}
		}

		public ZDecimal TotalTaxAmountOfOriginChargeGroupCharges
		{
			get
			{
				ZDecimal result = 0M;

				if (JobHeader != null)
				{
					if (JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges != null)
					{
						foreach (DocJobCharge jobCharge in JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges)
						{
							if (jobCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.Origin)
							{
								result += jobCharge.TaxAmount;
							}
						}
					}
				}

				return result;
			}
		}

		public DocJobChargeCollection NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges =>
			DocJobChargeCollection.GetCollection(this, nameof(NonZeroJobChargesWithoutFRTChargesAndOriginChargeGroupCharges),
				(collection) =>
				{
					if (JobHeader != null)
					{
						if (JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges != null)
						{
							foreach (DocJobCharge currentCharge in JobHeader.JobChargesForDebtorOrLocalClient.NonZeroJobCharges)
							{
								if (currentCharge.ChargeCode.Code != "FRT" && currentCharge.ChargeCode.ChargeGroup != ChargeCodeGroupList.Codes.Origin)
								{
									collection.Add(currentCharge);
								}
							}
						}
					}
					if (collection.Count > 1)
					{
						collection.Sort(new IcelandicChargesSort());
					}
				});

		class IcelandicChargesSort : IComparer
		{
			int GetChargeGroupPosition(ZString chargeCodeGroup)
			{
				switch (chargeCodeGroup)
				{
					case ChargeCodeGroupList.Codes.Insurance:
						return 1;
					case ChargeCodeGroupList.Codes.Freight:
						return 2;
					case ChargeCodeGroupList.Codes.Destination:
						return 3;
					default:
						return 4;
				}
			}

			public int Compare(object a, object b)
			{
				DocJobCharge chargeA = (DocJobCharge)a;
				DocJobCharge chargeB = (DocJobCharge)b;

				int result = 0;

				if (chargeA.ChargeCode != null && chargeB.ChargeCode != null)
				{
					result = GetChargeGroupPosition(chargeA.ChargeCode.ChargeGroup).CompareTo(GetChargeGroupPosition(chargeB.ChargeCode.ChargeGroup));
				}

				return result;
			}
		}

		#endregion

		#region IPreAlert Members

		public DocTransportCollection CompleteRouting
		{
			get
			{
				DocTransportCollection coll = new DocTransportCollection(CommonShipment.Factory);
				RoutingCollection transports = ((IRoutingSupport)CommonShipment).TransportsIncludingRelated;

				if (transports.Count == 0 && CommonShipment.JS_JX.IsValid)
				{
					JobSailing sailing = Factory.Load<JobSailing>(CommonShipment.JS_JX);
					coll.Add(DocTransport.New(sailing, Factory));
				}
				else
				{
					Transport[] sortedTransports = (Transport[])transports.ToArray(typeof(Transport));
					MovementLegComparer.SortMovementLegsByPorts(sortedTransports);

					foreach (Transport transport in sortedTransports)
					{
						coll.Add(DocTransport.New(CommonShipment, transport, Factory));
					}
				}

				return coll;
			}
		}

		public virtual ZString PortDisplayMode
		{
			get { return "LoadDischargeCollectDeliver"; }
		}

		public virtual ZBool ShowChargesOnArrivalNotice
		{
			get { return false; }
		}

		public ZBool ShowExchangeRatesOnArrivalNotice
		{
			get { return false; }
		}

		public virtual ZString PreAlertDocumentHeader
		{
			get
			{
				return UltimateNotification + HeadingTransportMode + " " + ReportName;
			}
		}

		public ZString JobNumber
		{
			get
			{
				return ShipmentNumber;
			}
		}

		public ZString SecondJobNumberHeading
		{
			get
			{
				return (Consol != null && !Consol.ConsolNumber.IsEmpty) ? Res.GetString("244ff2f6-3342-44f7-b273-ad7835b0ab70", "CONSOL:") : "";
			}
		}

		public ZString SecondJobNumber
		{
			get
			{
				return Consol != null ? Consol.ConsolNumber : ZString.Empty;
			}
		}

		public ZString AvailableDateHeading
		{
			get
			{
				return AvailableDate.IsEmpty ? "" : Res.GetString("660e7414-c0f2-4b6d-a581-261db60921b4", "AVAILABLE DATE:");
			}
		}

		public ZString StorageStartsHeading
		{
			get
			{
				return StorageCommenceDate.IsEmpty ? "" : Res.GetString("05513d43-5eea-4e6a-9e74-22146b56edd1", "STORAGE STARTS:");
			}
		}

		public ZString StorageStartsDate
		{
			get
			{
				return StorageCommenceDate.ToShortDateString();
			}
		}

		public ZString ContainerMode
		{
			get
			{
				return PackingMode;
			}
		}

		public ZString BrokerName
		{
			get
			{
				return ImportBrokerName;
			}
		}

		public ZString TransportHeadingFallBackToShipment
		{
			get
			{
				ZString heading = ZString.Empty;
				if (Consol != null)
				{
					heading = Consol.TransportHeading;
				}
				else if (!this.TransportMode.IsEmpty)
				{
					switch (this.TransportMode)
					{
						case Core.Constants.TransportModes.Air:
						case Core.Constants.TransportModes.AirSea:
							heading = Res.GetString("3fb3df33-0786-43fd-81a0-f25cc63d9074", "FLIGHT & DATE");
							break;

						case Core.Constants.TransportModes.Rail:
						case Core.Constants.TransportModes.Road:
							heading = Res.GetString("6b762dff-d01a-4e7b-b6f9-102e422b512a", "JOURNEY NAME / JOURNEY NUMBER");
							break;

						default:
						case Core.Constants.TransportModes.Sea:
						case Core.Constants.TransportModes.SeaAir:
							heading = Res.GetString("e3150e02-ff58-4d8a-99a1-9a5e3ca9f756", "VESSEL / VOYAGE / IMO(Lloyds)");
							break;
					}
				}
				return heading;
			}
		}

		public virtual ZString TransportHeading
		{
			get
			{
				return Consol != null ? Consol.TransportHeading : ZString.Empty;
			}
		}

		public virtual ZString TransportInfo
		{
			get
			{
				return Consol != null ? Consol.ConsolTransportInfo : ZString.Empty;
			}
		}

		public virtual ZString PreAlertReferenceHeading
		{
			get { return ""; }
		}

		public virtual ZString PreAlertReference
		{
			get { return ""; }
		}

		public ZString MasterBillHeadingFallBackToShipment
		{
			get
			{
				if (Consol != null)
				{
					return Consol.MasterBillHeading;
				}
				switch (this.TransportMode)
				{
					case Core.Constants.TransportModes.Air:
						return Res.GetString("e0e064a9-6126-40e7-9fbf-682e2499b6bb", "MAWB");
					case Core.Constants.TransportModes.Sea:
						return Res.GetString("7392eb4e-8a2e-4541-bd96-27544ae3009d", "OCEAN BILL OF LADING");
					default:
						return Res.GetString("dcab70b5-104f-4c5a-bd70-821642f3041e", "MASTER");
				}
			}
		}

		public ZString MasterBillHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (Consol != null)
				{
					result = Consol.MasterBillHeading;
				}
				else if ((CommonShipment.JS_IsDirectBooking) && (CommonShipment.JS_TransportMode == Core.Constants.TransportModes.Air))
				{
					result = Res.GetString("e0e064a9-6126-40e7-9fbf-682e2499b6bb", "MAWB");
				}

				return result;
			}
		}

		public ZString MasterBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(MasterBillHeading, MasterBillIssueDate); }
		}

		public virtual ZString MasterBillNum
		{
			get
			{
				ZString result = ZString.Empty;

				if (Consol != null)
				{
					result = splitMasterBillNum;
				}
				else if ((CommonShipment.JS_IsDirectBooking) && (CommonShipment.JS_TransportMode == Core.Constants.TransportModes.Air))
				{
					ZString bookingMAWB = CommonShipment.JS_HouseBill;

					ZString split = ZString.Empty;
					if (!bookingMAWB.IsEmpty)
					{ split = bookingMAWB.SubstringSafe(0, 3); }
					if (bookingMAWB.Length > 3)
					{ split += "-" + bookingMAWB.SubstringSafe(3, 4); }
					if (bookingMAWB.Length > 7)
					{ split += " " + bookingMAWB.SubstringSafe(7); }

					result = split;
				}

				return result;
			}
		}

		ZString splitMasterBillNum
		{
			get
			{
				ZString split = ZString.Empty;
				if (Consol.IsAir)
				{
					if (!Consol.MasterBillNum.IsEmpty)
					{ split = Consol.MasterBillNum.SubstringSafe(0, 3); }
					if (Consol.MasterBillNum.Length > 3)
					{ split += "-" + Consol.MasterBillNum.SubstringSafe(3, 4); }
					if (Consol.MasterBillNum.Length > 7)
					{ split += " " + Consol.MasterBillNum.SubstringSafe(7); }
				}
				else
				{
					split = Consol.MasterBillNum;
				}
				return split;
			}
		}

		public ZString CollectedFromETDString
		{
			get
			{
				return ETDString;
			}
		}

#if DEBUG
		public
#endif
 ZString CollectedFromATDString
		{
			get { return ATDString; }
		}

		public ZString DeliveredToETAString
		{
			get
			{
				return ETAString;
			}
		}

		public virtual ZString LoadingETDString
		{
			get { return Consol != null ? Consol.ETDString : ZString.Empty; }
		}

		public virtual ZString DischargeETAString
		{
			get
			{
				return Consol != null ? Consol.ETAString : ZString.Empty;
			}
		}

		public ZString ArrivalReference
		{
			get
			{
				return (Consol != null && Consol.Sailing != null) ? Consol.Sailing.ArrivalReference : ZString.Empty;
			}
		}

		public virtual ZString KANumber
		{
			get
			{
				return "";
			}
		}

		public ZString CTOArrivalBerth
		{
			get
			{
				ZString result = "";
				if (Consol != null)
				{
					if (Consol.ArrivalCTOAddress != null)
					{
						result = Consol.ArrivalCTOAddress.Organisation != null ? Consol.ArrivalCTOAddress.Organisation.Code : ZString.Empty;
					}
					if (Consol.Sailing != null && Consol.Sailing.VoyageDestination != null)
					{
						if (!result.IsEmpty)
						{
							result += " / ";
						}

						result += Consol.Sailing.VoyageDestination.Berth;
					}
				}
				return result;
			}
		}

		public ZString DeclarationNumber
		{
			get
			{
				return (DeclarationForCurrentBranch != null) ? DeclarationForCurrentBranch.DeclarationNumber : ZString.Empty;
			}
		}

		public DocDocAddress GoodsAvailableAt
		{
			get
			{
				DocDocAddress address = null;

				if (ImportReleaseDepot != null)
				{
					address = ImportReleaseDepot;
				}
				else if (Consol != null)
				{
					if (PackingMode == Core.Constants.ContainerModes.BreakBulk
							|| PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
					{
						address = Consol.ArrivalCTOAddress;
					}
					else
					{
						address = Consol.GoodsAvailableAt;
					}
				}
				return address;
			}
		}

		public DocDocAddress UnpackAt
		{
			get
			{
				DocDocAddress address = null;

				if (ImportReleaseDepot != null)
				{
					address = ImportReleaseDepot;
				}
				else if (Consol != null)
				{
					address = Consol.UnpackDepotAddress;
				}
				return address;
			}
		}

		public virtual DocOrganisation ShippingLine
		{
			get { return Consol != null ? Consol.ShippingLine : null; }
		}

		public virtual DocUNLOCO PortOfLoading
		{
			get { return Consol != null ? Consol.PortOfLoading : null; }
		}

		public virtual DocUNLOCO PortOfDischarge
		{
			get { return Consol != null ? Consol.PortOfDischarge : null; }
		}

		public TrackingConstants.BusinessContext TrackingBusinessContext
		{
			get { return TrackingConstants.BusinessContext.Shipment; }
		}

		public ZGuid TrackingBusinessObjectPK
		{
			get { return CommonShipment.PK; }
		}

		#endregion

		#region IDocJobDetail Properties

		public ZString OurReference
		{
			get
			{
				return ShipmentNumber;
			}
		}

		public ZString SupplierAsString
		{
			get
			{
				return Consignor != null ? Consignor.Name : ZString.Empty;
			}
		}

		public ZString VesselAndVoyage
		{
			get
			{
				return Consol != null ? Consol.ConsolTransportInfo : ZString.Empty;
			}
		}

		public ZString VesselAndVoyageWithDate
		{
			get
			{
				return Consol != null ? Consol.TransportInfoWithDate : ZString.Empty;
			}
		}

		public ZString MasterBillNumber
		{
			get
			{
				return MasterBillNum;
			}
		}

		public ZString ETAPortName
		{
			get
			{
				return DestinationLoco != null ? DestinationLoco.PortName : ZString.Empty;
			}
		}

		public ZString ETDPortName
		{
			get
			{
				return OriginLoco != null ? OriginLoco.PortName : ZString.Empty;
			}
		}

		public ZString Service
		{
			get
			{
				return PackingMode;
			}
		}

		public ZString PackageQuantity
		{
			get
			{
				return OuterPacks.ToString();
			}
		}

		public ZString PackageType
		{
			get
			{
				return OuterPacksPackTypeDescription;
			}
		}

		public ZString ConsolDepot
		{
			get
			{
				return ZString.Empty;
			}
		}

		public ZString WeightAsString
		{
			get
			{
				return Weight + " " + WeightUnit;
			}
		}

		public ZString VolumeAsString
		{
			get
			{
				return Volume + " " + VolumeUnit;
			}
		}

		public ZString ConsignorAsString
		{
			get
			{
				return Consignor != null ? Consignor.Name : ZString.Empty;
			}
		}

		public ZString ConsigneeAsString
		{
			get
			{
				return Consignee != null ? Consignee.Name : ZString.Empty;
			}
		}

		public ZString ShortContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerAndSealNumbers = ZString.Empty;

				if (Containers != null && Containers.Count > 0)
				{
					var container = Containers[0];
					containerAndSealNumbers += container.ContainerNumber + " / " + container.SealNumber;
					if (container.Container != null)
					{
						containerAndSealNumbers += " / " + container.Container.Code;
					}
				}
				return containerAndSealNumbers;
			}
		}

		public ZString LongContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerString = ZString.Empty;

				foreach (IDocContainer container in Containers)
				{
					containerString += "- " + container.ContainerNumber.PadRight(15, ' ');
					containerString += " - " + container.SealNumber.PadRight(20, ' ') + " - ";

					ZString containerCode = (container.Container != null) ? container.Container.Code : ZString.Empty;
					containerString += containerCode.PadRight(15, ' ') + "\n";
				}

				return containerString;
			}
		}

		public ZInt NumberOfContainers
		{
			get
			{
				return Containers != null ? Containers.Count : 0;
			}
		}

		public ZString MarksAndNumbersForInvoice
		{
			get
			{
				return MarksAndNumbers;
			}
		}

		public ZString ShortGoodsDescriptionForInvoice
		{
			get
			{
				return GoodsDescription;
			}
		}

		public ZString LongGoodsDescriptionForInvoice
		{
			get
			{
				return DescriptionForGoods;
			}
		}

		public ZDateTime ETADate
		{
			get
			{
				return ETA;
			}
		}

		public ZDateTime ETDDate
		{
			get
			{
				return ETD;
			}
		}

		public ZBool TransportModeIsAir
		{
			get
			{
				return TransportMode == Core.Constants.TransportModes.Air ? ZBool.True : ZBool.False;
			}
		}

		public ZBool TransportModeIsSea
		{
			get
			{
				return TransportMode == Core.Constants.TransportModes.Sea ? ZBool.True : ZBool.False;
			}
		}

		#endregion

		#region ITimeSlotRequest Members

		public ZString ConsolNumber
		{
			get
			{
				return Consol != null ? Consol.ConsolNumber : null;
			}
		}

		public DocDocAddress CTOAddress
		{
			get
			{
				DocDocAddress result = null;

				if (Consol != null)
				{
					result = IsImportDocument ? Consol.ArrivalCTOAddress : Consol.DepartureCTOAddress;
				}
				return result;
			}
		}

		public IDocSimpleContainerCollection SimpleContainers
		{
			get
			{
				IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
				if (Containers != null)
				{
					foreach (DocContainer container in Containers)
					{
						result.Add(container);
					}
				}
				return result;
			}
		}

		public DocContainerCollection DocContainers
		{
			get
			{
				if (docContainers == null)
				{
					docContainers = new DocContainerCollection(Factory);

					foreach (DocContainer container in Containers)
					{
						docContainers.Add(container);
					}
				}

				return docContainers;
			}
		}
		DocContainerCollection docContainers;

		#endregion

		#region Implementation

		protected delegate DocShipment NewDelegate(CommonShipment shipment, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		// hack so that both DocShipment.New() can handle forwarding shipments (delegates should prob be replaced by type decider)
		protected delegate DocForwardingShipment ForwardingNewDelegate(ForwardingShipment shipment, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<ForwardingNewDelegate> OverridableForwardingNewDelegate = new Overridable<ForwardingNewDelegate>();

		protected delegate DocForwardingShipment AgencyNewDelegate(AgencyShipment shipment, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<AgencyNewDelegate> OverridableAgencyNewDelegate = new Overridable<AgencyNewDelegate>();

		protected IDocContainerCollection fContainers;
		protected DocBillOfLading fBillOfLading;
		protected NorthPortDeliveryOrder fNPDeliveryOrder;

		protected DocOrganisation GetCartageCompanyFromRegistry()
		{
			DocOrganisation result = null;
			OrgHeader header = null;

			if (Consol != null && Consol.ConsolMode == Core.Constants.ContainerModes.BuyersConsol)
			{
				header = Factory.Load<OrgHeader>(FreightDataRegistry.Instance.FCLCartageCompany.Value);
				result = DocOrganisation.New(header, Factory);
			}
			else
			{
				switch (PackingMode)
				{
					case Core.Constants.ContainerModes.LCL:
						header = Factory.Load<OrgHeader>(FreightDataRegistry.Instance.LCLCartageCompany.Value);
						result = DocOrganisation.New(header, Factory);
						break;
					case Core.Constants.ContainerModes.FCL:
						header = Factory.Load<OrgHeader>(FreightDataRegistry.Instance.FCLCartageCompany.Value);
						result = DocOrganisation.New(header, Factory);
						break;
					case Core.Constants.ContainerModes.AIR:
						header = Factory.Load<OrgHeader>(FreightDataRegistry.Instance.AIRCartageCompany.Value);
						result = DocOrganisation.New(header, Factory);
						break;
				}
			}

			return result;
		}

		ZString GetContactCode(DocOrganisation docOrg, ContactType contactType)
		{
			ZString result = ZString.Empty;

			if (docOrg != null)
			{
				DocContacts contact = docOrg.DefaultContact(contactType);
				if (contact != null)
				{
					result = contact.Code;
				}
			}
			return result;
		}

		public IDocContainerCollection GetContainersInConsol(DocBaseConsol consol)
		{
			IDocContainerCollection coll = new IDocContainerCollection(CommonShipment.Factory);
			ArrayList alreadyAddedArray = new ArrayList();
			if (consol != null)
			{
				foreach (PackLine line in CommonShipment.OuterPackLines)
				{
					foreach (CommonContainer aContainer in line.Containers)
					{
						if (aContainer.JC_JK == consol.ConsolPK)
						{
							if (!alreadyAddedArray.Contains(aContainer.PK))
							{
								coll.Add(DocContainer.New(aContainer, CommonShipment, Factory));
								alreadyAddedArray.Add(aContainer.PK);
							}
						}
					}
				}
			}
			return coll;
		}

		public ZString GetOuterPacksDetail(DocBaseConsol consol)
		{
			ZString result = ZString.Empty;
			foreach (DocPackLines packLine in OuterPackLineCollection)
			{
				DocContainer container = packLine.GetContainerOnConsol(consol);
				if (container != null)
				{
					result += container.ContainerNumber;
				}

				result += "/ " + FormatNumber(packLine.ActualWeight) + packLine.ActualWeightUQ;
				result += "/ " + FormatNumber(packLine.ActualVolume) + packLine.ActualVolumeUQ;
				result += "/ " + packLine.PackageCount + packLine.PackType + System.Environment.NewLine;
			}

			if (!result.IsEmpty)
			{
				result = result.Insert(0, Res.GetString("87e7e34b-1390-44ae-8780-7b3a68927380", "CONTAINER/ WGT/ VOL/ PKG\r\n"));
			}

			return result.TrimEnd();
		}
		#endregion

		#region IShipperDepartureNotice Members

		public ZString ShipperDepartureNoticeDocumentHeader
		{
			get
			{
				return Res.GetString("37e32318-7b81-4a02-8b15-856e10040a98", "{0} Freight {1}", HeadingTransportMode, ReportName);
			}
		}

		public ZString AgentsBookingReference
		{
			get
			{
				ZString result = ZString.Empty;

				if (Consol != null)
				{
					result = Consol.AgentsReference;
				}

				return result;
			}
		}

		public ZString DepartureReference
		{
			get
			{
				ZString result = ZString.Empty;

				if (Consol != null && Consol.Sailing != null)
				{
					result = Consol.Sailing.DepartureReference;
				}

				return result;
			}
		}

		public virtual DocOrganisation ReceivingForwarder
		{
			get
			{
				DocOrganisation result = null;

				if (Consol != null)
				{
					result = Consol.ReceivingForwarder;
				}

				return result;
			}
		}

		public virtual DocOrganisation SendingForwarder
		{
			get
			{
				DocOrganisation result = null;

				if (Consol != null)
				{
					result = Consol.SendingForwarder;
				}

				return result;
			}
		}

		#endregion

		#region IDocCartageAdvice Members

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("88319b2e-48ae-465b-a8bf-cd95064ffdb2", "PICKUP");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4326c4eb-5291-4780-bc9a-3571d7bc3c73", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("d6dfa852-c28e-46e6-93a6-f31d07d469a7", "FULL"));
				}

				if (!PrintTwoJourneys)
				{
					if (IsExportDocument && DocsAndCartage.PickupRequiredBy.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5436f01f-f234-481d-85fb-9628bd272fab", "DATE {0}", DocsAndCartage.PickupRequiredBy.ToLongTimeString()));
					}
					else if (IsImportDocument && DocsAndCartage.EstimatedDelivery.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5436f01f-f234-481d-85fb-9628bd272fab", "DATE {0}", DocsAndCartage.EstimatedDelivery.ToLongTimeString()));
					}
				}
				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("f73c66f7-f96b-4564-b177-4358d8f51228", "DELIVER TO");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4326c4eb-5291-4780-bc9a-3571d7bc3c73", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("d6dfa852-c28e-46e6-93a6-f31d07d469a7", "FULL"));
				}

				if (!PrintTwoJourneys)
				{
					if (IsExportDocument && BookingCutOffDate.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5436f01f-f234-481d-85fb-9628bd272fab", "DATE {0}", BookingCutOffDate.ToLongTimeString()));
					}
					else if (IsImportDocument && DocsAndCartage.DeliveryRequiredBy.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("5436f01f-f234-481d-85fb-9628bd272fab", "DATE {0}", DocsAndCartage.DeliveryRequiredBy.ToLongTimeString()));
					}
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("88319b2e-48ae-465b-a8bf-cd95064ffdb2", "PICKUP");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4326c4eb-5291-4780-bc9a-3571d7bc3c73", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("d6dfa852-c28e-46e6-93a6-f31d07d469a7", "FULL"));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("7013cc34-564d-46a4-b9fb-64d178c9c7c9", "DELIVER TO");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4326c4eb-5291-4780-bc9a-3571d7bc3c73", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("d6dfa852-c28e-46e6-93a6-f31d07d469a7", "FULL"));
				}

				return result;
			}
		}

		#endregion

		#region Addresses

		#region JourneyOnePickUpAddress

		public virtual DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsImportDocument && IsBulkLike)
				{
					if (Consol != null)
					{
						result = Consol.ArrivalCTOAddress;
					}
				}
				else if (!PrintTwoJourneys)
				{
					result = PickUpFromAddress;
				}
				else if (Consol != null)
				{
					if (IsExportDocument)
					{
						result = Consol.ContainerParkEmptyPickupAddress;
					}
					else if (IsImportDocument)
					{
						result = Consol.ArrivalCTOAddress;
					}
				}

				return result;
			}
		}

		#endregion

		#region JourneyOneDeliverToAddress

		public virtual DocDocAddress JourneyOneDeliverToAddress
		{
			get
			{
				if (fJourneyOneDeliverToAddress == null)
				{
					if (IsExportDocument && IsBulkLike)
					{
						if (Consol != null)
						{
							fJourneyOneDeliverToAddress = Consol.DepartureCTOAddress;
						}
					}
					else if (!PrintTwoJourneys)
					{
						fJourneyOneDeliverToAddress = DeliverToAddress;
					}
					else if (IsExportDocument)
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForExport;
					}
					else if (IsImportDocument)
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForImport;
					}
				}
				return fJourneyOneDeliverToAddress;
			}
		}
		DocDocAddress fJourneyOneDeliverToAddress;

		public virtual DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get
			{
				DocDocAddress result = null;

				if (PickupAddress != null)
				{
					result = PickupAddress;
				}
				else if (Consignor != null)
				{
					result = Consignor.PickUpDocAddress;
				}

				return result;
			}
		}

		public virtual DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get
			{
				DocDocAddress result = null;

				if (DeliveryAddress != null)
				{
					result = DeliveryAddress;
				}
				else if (Consignee != null)
				{
					result = Consignee.DeliverDocAddress;
				}

				return result;
			}
		}
		#endregion

		#region JourneyTwoPickUpAddress

		public virtual DocDocAddress JourneyTwoPickUpAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExportDocument)
				{
					result = JourneyTwoPickUpAddressForExport;
				}
				else if (IsImportDocument)
				{
					result = JourneyTwoPickUpAddressForImport;
				}

				return result;
			}
		}

		public virtual DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get
			{
				if (fJourneyTwoPickUpAddressForExport == null)
				{
					if (PickupAddress != null)
					{
						fJourneyTwoPickUpAddressForExport = PickupAddress;
					}
					else if (Consignor != null)
					{
						fJourneyTwoPickUpAddressForExport = Consignor.PickUpDocAddress;
					}
				}
				return fJourneyTwoPickUpAddressForExport;
			}
		}
		DocDocAddress fJourneyTwoPickUpAddressForExport;

		public virtual DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get
			{
				if (fJourneyTwoPickUpAddressForImport == null)
				{
					if (DeliveryAddress != null)
					{
						fJourneyTwoPickUpAddressForImport = DeliveryAddress;
					}
					else if (Consignee != null)
					{
						fJourneyTwoPickUpAddressForImport = Consignee.DeliverDocAddress;
					}
				}
				return fJourneyTwoPickUpAddressForImport;
			}
		}
		DocDocAddress fJourneyTwoPickUpAddressForImport;

		#endregion

		#region JourneyTwoDeliverToAddress

		public virtual DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				DocDocAddress result = null;

				if (Consol != null)
				{
					if (IsExportDocument)
					{
						result = Consol.DepartureCTOAddress;
					}
					else if (IsImportDocument)
					{
						result = Consol.ContainerParkEmptyReturnAddress;
					}
				}
				else if (IsExportDocument)
				{
					result = DocDocAddress.New(CommonShipment.ExportReceivingDepot, Factory);
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString JourneyTwoDeliverToContactName
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#endregion

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);

				if (JourneyOnePickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
				}

				if (JourneyOneDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
				}

				if (PrintTwoJourneys)
				{
					if (JourneyTwoPickUpAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
					}

					if (JourneyTwoDeliverToAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
					}
				}
				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public virtual ZBool PrintAsContainers
		{
			get
			{
				return ((PackingMode == Core.Constants.ContainerModes.FCL) ||
						(DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV) &&
						Consol != null && Consol.ConsolMode == Core.Constants.ContainerModes.BuyersConsol));
			}
		}

		public ZBool PrintTwoJourneys
		{
			get { return PrintAsContainers; }
		}

		public virtual ZString EmailSubjectNumber
		{
			get { return ShipmentNumber; }
		}

		public ZString FullCartageInstructions
		{
			get { return ShipmentOrOrgCartageInstruction; }
		}

		public ZString FullHandlingInstructions
		{
			get
			{
				ZString result = ShipmentOrOrgHandlingInstructions;

				if (!InspectedShipmentText.IsEmpty)
				{
					if (!result.IsEmpty)
					{
						result += "\n";
					}

					result += InspectedShipmentText;
				}

				return result;
			}
		}

		public ZString InspectedShipmentText
		{
			get
			{
				ZString result = "";
				if (CommonShipment.IsAir && !CommonShipment.IsDomestic())
				{
					result = GetApprovedTextFromRegistry(CommonShipment.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code);
				}
				return result;
			}
		}

		string GetApprovedTextFromRegistry(bool isApproved)
		{
			if (isApproved)
			{
				return (string)FreightDataRegistry.Instance.AWBApprovedExporterText.Value;
			}
			else
			{
				return (string)FreightDataRegistry.Instance.AWBNonApprovedExporterText.Value;
			}
		}

		#region Implementation

		public ZBool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsExportDocument : IsImportDocument);
		}

		public ZBool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && (isJourneyOne ? IsImportDocument : IsExportDocument);
		}

		bool IsBulkLike
		{
			get
			{
				return PackingMode == Constants.ContainerModes.BreakBulk
						|| PackingMode == Constants.ContainerModes.Bulk
						|| PackingMode == Constants.ContainerModes.Liquid;
			}
		}

		#endregion

		#endregion

		#region IContainsSuppressedFields

		public ZBool SuppressFlightDetails
		{
			get { return false; }
		}

		public ZString MasterBill_OrSuppressed
		{
			get { return Suppression.GetValue(MasterBillNum, CommonShipment, SuppressFields.MasterBill, "*", DocumentContactType); }
		}

		public ZString MasterBillAndIssueDate_OrSuppressed
		{
			get { return Suppression.GetValue(FreightHelperClass.FormatBillAndIssueDate(MasterBillNum, MasterBillIssueDate), CommonShipment, SuppressFields.MasterBill, "*", DocumentContactType); }
		}

		public ZString TransportInfo_OrSuppressed
		{
			get { return Suppression.GetValue(BookingTransport, CommonShipment, SuppressFields.TransportInfo, "*", DocumentContactType); }
		}

		public ZString ETD_OrSuppressed
		{
			get { return Suppression.GetValue(ETDString, CommonShipment, SuppressFields.ETD, "*", DocumentContactType); }
		}

		public ZString ATD_OrSuppressed
		{
			get { return Suppression.GetValue(ATDString, CommonShipment, SuppressFields.ATD, "*", DocumentContactType); }
		}

		public ZString LoadingETD_OrSuppressed
		{
			get { return Suppression.GetValue(BookingETD, CommonShipment, SuppressFields.ETD, "*", DocumentContactType); }
		}

		public ZString LoadingATD_OrSuppressed
		{
			get { return Suppression.GetValue(BookingATD, CommonShipment, SuppressFields.ATD, "*", DocumentContactType); }
		}

		public ZString CarrierName_OrSuppressed
		{
			get
			{
				string result = Carrier == null ? string.Empty : Carrier.Name.ToString();
				return Suppression.GetValue(result, CommonShipment, SuppressFields.Carrier, "*", DocumentContactType);
			}
		}

		public ZString CarrierCCC_OrSuppressed
		{
			get
			{
				string result = Carrier == null ? string.Empty : Carrier.CCC.ToString();
				return Suppression.GetValue(result, CommonShipment, SuppressFields.Carrier, "*", DocumentContactType);
			}
		}

		public ZString SuppressFlightDetailsFooter
		{
			get
			{
				bool suppressionWasEnabled = Suppression.EnabledForAnyOfFields(CommonShipment,
																									new[]
																				{
																					SuppressFields.TransportInfo,
																					SuppressFields.MasterBill,
																					SuppressFields.ETD,
																					SuppressFields.ATD,
																					SuppressFields.ETD,
																					SuppressFields.ATD,
																					SuppressFields.Carrier,
																					SuppressFields.FlightNumber
																				},
																									DocumentContactType);
				return suppressionWasEnabled
								? Res.GetString("567ca6fa-2759-4d0b-b92f-a90f474d5339", "*Flight details suppressed for security reasons.")
								: string.Empty;
			}
		}

		#endregion

		#region IRequestForMissingDocuments Members

		public ZString ContainerNumbers
		{
			get
			{
				return ShipmentContainerNumbers;
			}
		}

		public ZString OwnerRefAndOrderRefHeading
		{
			get { return Res.GetString("93cb8183-a096-4eaa-aac7-349ca781e704", "ORDER NUMBERS / REFERENCE"); }
		}

		public ZString ConsigneeOrgHeading
		{
			get { return Res.GetString("fcdd6982-f3fa-400e-be38-5c959a596713", "CONSIGNEE"); }
		}

		public ZString ConsignorOrgHeading
		{
			get { return Res.GetString("8b7362d1-fdfb-469d-8b4a-9e46445484f7", "CONSIGNOR"); }
		}

		public DocOrganisation ConsigneeOrg
		{
			get { return Consignee; }
		}

		public DocOrganisation ConsignorOrg
		{
			get { return Consignor; }
		}

		#endregion

		#region Delivery Order

		public ZString DeliveryOrderPickup
		{
			get
			{
				return (PickupAddress == null) ? ConsignorAddress : PickupAddress.PostalAddress;
			}
		}

		public ZString DeliveryOrderDeliver
		{
			get
			{
				return (DeliveryAddress == null) ? ConsigneeAddress : DeliveryAddress.PostalAddress;
			}
		}

		#endregion

		#region Export Statement
		public ZString ExportStatement
		{
			get { return CommonShipment.ExportStatement; }
		}

		public DocExportStatementSetting ExportStatementSetting
		{
			get { return new DocExportStatementSetting(CommonShipment.ExportStatementSetting); }
		}

		ZString AddExportStatement(ZString description)
		{
			ZString exportStatement = this.ExportStatement;
			if (!exportStatement.IsEmpty)
			{
				return description + (description.IsEmpty ? "" : "\n") + exportStatement;
			}
			return description;
		}
		#endregion

		#region Sailing Properties

		public virtual DocUNLOCO LoadPort
		{
			get { return DocUNLOCO.New(Factory, CommonShipment.JS_Calc_CurrentLoadPort); }
		}

		public virtual DocUNLOCO DischargePort
		{
			get { return DocUNLOCO.New(Factory, CommonShipment.JS_Calc_CurrentDischargePort); }
		}

		#endregion

		#region Freight Location

		public ZString FreightLocation
		{
			get
			{
				ZString result = CommonShipment.JS_WarehouseLocation;
				if (result.IsEmpty)
				{
					result = CommonShipment.GetArrivalCFSDocAddress.AddressAsASingleLine;
				}
				return result;
			}
		}

		#endregion

		#region Flight No & Flight Date

		public ZString FlightNo
		{
			get
			{
				ZString result = Consol != null ? Consol.VoyageNumber : ZString.Empty;
				return Suppression.GetValue(result, CommonShipment, SuppressFields.FlightNumber, "*", DocumentContactType);
			}
		}

		public ZDateTime FlightDate
		{
			get
			{
				CommonConsol consol = CommonShipment.ArrivalConsolForDocuments;
				if (consol != null)
				{
					return !consol.JK_JX_JB_A_ARV.IsEmpty ? consol.JK_JX_JB_A_ARV : consol.JK_JX_JB_E_ARV;
				}
				return ZDateTime.Empty;
			}
		}

		#endregion

		#region ChargeInfo Column Properties

		public ZInt ChargeInfoDescriptionWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoDescriptionWidth, 25); }
		}

		public ZInt ChargeInfoGapWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoGapWidth, 1); }
		}

		public ZInt ChargeInfoAmountWidth
		{
			get { return GetTemplateConstantValue<ZInt>(DocumentEngineIntegration.Constants.TemplateDefined.ChargeInfoAmountWidth, 15); }
		}

		#endregion

		#region ACI Zones (US)

		public virtual ZString ShipperACIZone
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ConsigneeACIZone
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Reference Numbers

		public CusEntryNumAdditionalReferenceCollection Numbers
		{
			get { return CommonShipment.Numbers; }
		}

		#endregion

		#region Not Cleared by Agent

		public ZString NotClearedByAgentStatement
		{
			get
			{
				ZString result = ZString.Empty;
				CusEntryNumber num = NotClearedByAgentCusEntryNumber;
				if (num != null)
				{
					result = FreightDataRegistry.Instance.NotClearedByAgentStatement.Value;
				}

				return result;
			}
		}

		public ZString NotClearedByAgentNumber
		{
			get
			{
				ZString result = ZString.Empty;
				CusEntryNumber num = NotClearedByAgentCusEntryNumber;
				if (num != null)
				{
					result = num.CE_EntryNum;
				}

				return result;
			}
		}

		public ZDateTime NotClearedByAgentIssueDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CusEntryNumber num = NotClearedByAgentCusEntryNumber;
				if (num != null)
				{
					result = num.CE_IssueDate;
				}

				return result;
			}
		}

		public ZDateTime NotClearedByAgentExpiryDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				CusEntryNumber num = NotClearedByAgentCusEntryNumber;
				if (num != null)
				{
					result = num.CE_ExpiryDate;
				}

				return result;
			}
		}

		CusEntryNumber NotClearedByAgentCusEntryNumber
		{
			get
			{
				CusEntryNumber result = null;
				ZString notClearedByAgentType = CusEntryNumberTypes.NotClearedByAgentNumberType(CountryCode);
				if (!notClearedByAgentType.IsEmpty &&
					CommonShipment.CustomsEntryNumberType == notClearedByAgentType &&
					CommonShipment.CusEntryNumbers.Count == 1)
				{
					result = CommonShipment.CusEntryNumbers[0];
				}

				return result;
			}
		}

		#endregion

		#region IDocTypeCode

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion

		public ZBool IsManufacturerBillOfLading
		{
			get
			{
				if (!isManufacturerBillOfLading.HasValue)
				{
					isManufacturerBillOfLading = MenuTitle.StartsWith(CommonShipmentDocumentSupporter.DocumentNames.ManufacturerBillOfLading, StringComparison.OrdinalIgnoreCase) || MenuTitle.StartsWith(CommonShipmentDocumentSupporter.DocumentNames.LegacyManufacturerBillOfLading, StringComparison.OrdinalIgnoreCase);
				}

				return isManufacturerBillOfLading.Value;
			}
		}

		bool? isManufacturerBillOfLading;
	}
}
