using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class QuarantineExDocLineValidation : AutoQuarantineExDocLineValidation
	{
		public QuarantineExDocLineValidation(AutoQuarantineExDocLine parent)
			: base(parent)
		{
			this.parent = (QuarantineExDocLine)parent;
			dbValidationHelper = new ZZDBValidationHelper(parent.Factory);
		}

		readonly ZZDBValidationHelper dbValidationHelper;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateQL_ProduceType();
		}

		bool IsValidationRequired
		{
			get
			{
				var declaration = parent?.QuarantineExDocHeader?.Declaration;
				return (declaration != null && declaration.IsQuarantine);
			}
		}

		#region Fish catch dates, from errata 31

		internal const string errorFishNeedBothCatchDates = "Both catch start and end dates are needed when the produce type is Fish.  Supply both dates.";
		internal const string errorCatchDateOnyNeededForFish = "Catch dates must only be entered when the produce type is Fish. Remove the dates or change the type.";
		internal const string errorCatchDateOrder = "Catch end date must not be before start date.";

		protected override void CheckQL_CatchEndDate()
		{
			base.CheckQL_CatchEndDate();
			if (IsValidationRequired)
			{
				ValidateCatchEndAndStartShared(parent.QL_CatchEndDateInfo);
			}
		}

		protected override void CheckQL_CatchStartDate()
		{
			base.CheckQL_CatchStartDate();
			if (IsValidationRequired)
			{
				ValidateCatchEndAndStartShared(parent.QL_CatchStartDateInfo);
			}
		}

		void ValidateCatchEndAndStartShared(ZPropertyInfo infoToErrorUpon)
		{
			if (parent.QL_ProduceType == EXDOCCommodityCodes.Codes.Fish)
			{
				if (parent.QL_CatchStartDate.IsEmpty && parent.QL_CatchEndDate.IsEmpty)
				{
					return;  // both empty - no need to validate.
				}
				else
				{   // one or both missing 
					if (infoToErrorUpon.Value.IsEmpty)
					{
						infoToErrorUpon.AddMessageError(errorFishNeedBothCatchDates);
					}
				}
				if (!parent.QL_CatchStartDate.IsEmpty && !parent.QL_CatchEndDate.IsEmpty)
				{
					// Both present, check end is after start
					if (parent.QL_CatchStartDate > parent.QL_CatchEndDate)
					{
						infoToErrorUpon.AddMessageError(errorCatchDateOrder);
					}
				}
			}
			else
			{
				// Not Fish - should not have catch dates
				if (!parent.QL_CatchStartDate.IsEmpty || !parent.QL_CatchEndDate.IsEmpty)
				{
					infoToErrorUpon.AddMessageError(errorCatchDateOnyNeededForFish);
				}
			}
		}

		#endregion

		protected override void CheckQL_FarmType()
		{
			base.CheckQL_FarmType();
			if (!parent.QL_FarmType.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Eggs)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_FarmTypeInfo);
			}
		}

		protected override void CheckQL_ExtraCertificate()
		{
			base.CheckQL_ExtraCertificate();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Wool)
				{
					MandatoryValidation.MessageErrorIfIsEntered(parent.QL_ExtraCertificateInfo);
				}
			}
		}

		protected override void CheckQL_NatureOfCommodity()
		{
			base.CheckQL_NatureOfCommodity();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_NatureOfCommodityInfo, parent.Lookups.NatureOfCommodity);
				if (!parent.QL_NatureOfCommodity.IsEmpty)
				{
					EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(parent.QuarantineExDocHeader.QH_ProduceType, parent.QL_NatureOfCommodityInfo, "Nature of Commodity");
				}
			}
		}

		protected override void CheckQL_TreatmentType()
		{
			base.CheckQL_TreatmentType();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_TreatmentTypeInfo, parent.Lookups.TreatmentType);
			}
		}

		protected override void CheckQL_ProductType()
		{
			base.CheckQL_ProductType();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.IsNEXDOCSActive)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.QL_ProductTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(parent.QL_ProductTypeInfo, ProductTypeMessage);
			}
		}

		internal static IMultilingualString ProductTypeMessage
		{
			get { return ResString.GetMultilingualString("6678ecb9-9cf6-4101-a933-bfd262e940a3", "This product cannot be selected because it is not of the correct commodity type."); }
		}

		protected override void CheckQL_SupplimentaryCode()
		{
			base.CheckQL_SupplimentaryCode();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_SupplimentaryCodeInfo);
			}
		}

		protected override void CheckQL_PackType()
		{
			base.CheckQL_PackType();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.IsNEXDOCSActive)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.QL_PackTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(parent.QL_PackTypeInfo, parent.Lookups.PackType);
			}
		}

		protected override void CheckQL_PreservationType()
		{
			base.CheckQL_PreservationType();
			if (IsValidationRequired)
			{
				if (parent.QL_PreservationType.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish)
				{
					parent.QL_PreservationTypeInfo.AddMessageError("Preservation type is required when produce type is Fish.");
				}
				if (parent.QuarantineExDocHeader.IsNEXDOCSActive)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.QL_PreservationTypeInfo);
				}
				ListValidation.MessageErrorIfInvalidCode(parent.QL_PreservationTypeInfo, parent.Lookups.Preservation);
			}
		}

		protected override void CheckQL_CutCode()
		{
			base.CheckQL_CutCode();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_CutCodeInfo, CutCodeMessage);
			}
		}

		internal static IMultilingualString CutCodeMessage
		{
			get { return ResString.GetMultilingualString("e37e736e-5e44-4054-981a-a442c304a43e", "This cut code cannot be selected because it is not of the correct commodity type."); }
		}

		protected override void CheckQL_ProductDescriptionLocationQualifier()
		{
			base.CheckQL_ProductDescriptionLocationQualifier();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_ProductDescriptionLocationQualifierInfo, parent.Lookups.LocationQualifier);
				if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat &&
					parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Wool &&
					!parent.QL_ProductDescriptionLocationQualifier.IsEmpty)
				{
					parent.QL_ProductDescriptionLocationQualifierInfo.AddMessageError("Product location may only be present when produce type is Meat or Wool.");
				}
			}
		}

		protected override void CheckQL_ProductDescriptionQualityQualifier()
		{
			base.CheckQL_ProductDescriptionQualityQualifier();
			if (IsValidationRequired)
			{
				if (!parent.QL_ProductDescriptionQualityQualifier.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat)
				{
					parent.QL_ProductDescriptionQualityQualifierInfo.AddMessageError("Product quality may only be present when produce type is not Meat.");
				}
			}
		}

		protected override void CheckQL_LabelApprovalNumber()
		{
			base.CheckQL_LabelApprovalNumber();
			if (IsValidationRequired)
			{
				if (!parent.QL_LabelApprovalNumber.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
				{
					parent.QL_LabelApprovalNumberInfo.AddMessageError("Label approval number may only be present when produce type is Meat.");
				}
				if (parent.QL_LabelApprovalNumber.IsEmpty && parent.QL_LabelApprovalIndicator)
				{
					parent.QL_LabelApprovalNumberInfo.AddMessageError("Label approval number should be entered when the Label Approval Indicator is set.");
				}
			}
		}

		protected override void CheckQL_LabelApprovalIndicator()
		{
			base.CheckQL_LabelApprovalIndicator();
			if (IsValidationRequired)
			{
				if (parent.QL_LabelApprovalIndicator && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
				{
					parent.QL_LabelApprovalIndicatorInfo.AddMessageError("Label approval indicator may only be set when produce type is Meat.");
				}
				parent.Validation.ValidateQL_LabelApprovalNumber();
			}
		}

		protected override void CheckQL_UngradedProductIndicator()
		{
			base.CheckQL_UngradedProductIndicator();
			if (IsValidationRequired)
			{
				if (parent.QL_UngradedProductIndicator && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat)
				{
					parent.QL_UngradedProductIndicatorInfo.AddMessageError("Ungraded Product Indicator may only be set when produce type is Meat.");
				}
			}
		}

		protected override void CheckQL_NetQuantity()
		{
			base.CheckQL_NetQuantity();
			if (IsValidationRequired)
			{
				if (parent.QL_NetQuantity <= 0)
				{
					parent.QL_NetQuantityInfo.AddMessageError("Net quantity must be greater than 0.");
				}
				if (parent.QL_NetQuantityUnit == parent.QL_OuterPackWeightUnit &&
					!parent.QL_OuterPackWeight.IsEmpty &&
					!parent.QL_OuterPackCount.IsEmpty &&
					(parent.QL_OuterPackWeight * parent.QL_OuterPackCount) != parent.QL_NetQuantity)
				{
					parent.QL_NetQuantityInfo.AddWarning("Product of outer pack weight and count must be equal to net quantity.");
				}
			}
		}

		protected override void CheckQL_NetQuantityUnit()
		{
			base.CheckQL_NetQuantityUnit();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_NetQuantityUnitInfo);
			}
		}

		protected override void CheckQL_ImperialNetWeight()
		{
			base.CheckQL_ImperialNetWeight();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Eggs &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Wool &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
				{
					if (!parent.QL_ImperialNetWeight.IsEmpty)
					{
						parent.QL_ImperialNetWeightInfo.AddMessageError("Imperial net weight may only be present when produce type is Wool, Meat, Eggs or Fish.");
					}
				}
				else
				{
					if (parent.QL_ImperialNetWeight < 0)
					{
						parent.QL_ImperialNetWeightInfo.AddMessageError("Imperial net weight must be greater or equal to 0.");
					}
				}
			}
		}

		protected override void CheckQL_ImperialNetWeightUnit()
		{
			base.CheckQL_ImperialNetWeightUnit();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_ImperialNetWeightUnitInfo, parent.Lookups.NetImperialWeightUnit);
				if (!parent.QL_ImperialNetWeight.IsEmpty && parent.QL_ImperialNetWeightUnit.IsEmpty)
				{
					parent.QL_ImperialNetWeightUnitInfo.AddMessageError("Imperial net weight unit is required when imperial net weight is not empty.");
				}
			}
		}

		protected override void CheckQL_GrossMetricWeight()
		{
			base.CheckQL_GrossMetricWeight();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.IsNEXDOCSActive && parent.QL_GrossMetricWeight.IsEmpty)
				{
					parent.QL_GrossMetricWeightInfo.AddMessageError("You have not entered a Gross Metric Weight.");
				}
				else if (parent.QL_GrossMetricWeight < 0)
				{
					parent.QL_GrossMetricWeightInfo.AddMessageError("Gross metric weight must be greater or equal to 0.");
				}
			}
		}

		protected override void CheckQL_GrossMetricWeightUnit()
		{
			base.CheckQL_GrossMetricWeightUnit();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_GrossMetricWeightUnitInfo, parent.Lookups.Weight);
				if (!parent.QL_GrossMetricWeight.IsEmpty && parent.QL_GrossMetricWeightUnit.IsEmpty)
				{
					parent.QL_GrossMetricWeightUnitInfo.AddMessageError("Unit cannot be empty when a weight is entered.");
				}
				if (parent.QuarantineExDocHeader.QH_ObtainExportCustomsPermit &&
					parent.QL_GrossMetricWeightUnit != EXDOCMetricWeightUnitCodes.Codes.Kilogram &&
					parent.QL_GrossMetricWeightUnit != EXDOCMetricWeightUnitCodes.Codes.MetricTonne)
				{
					parent.QL_GrossMetricWeightUnitInfo.AddMessageError("Valid units are kilograms (KGM) or Metric Tonne (TNE) when Quarantine is reguired to obtain customs permit.");
				}
				if (parent.QL_GrossMetricWeightUnit == EXDOCMetricWeightUnitCodes.Codes.MetricTon &&
					parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.GrainsAndPlants)
				{
					parent.QL_GrossMetricWeightUnitInfo.AddMessageError("Metric tonne unit MTO can only be used when produce type is Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_OuterPackCount()
		{
			base.CheckQL_OuterPackCount();
			if (IsValidationRequired)
			{
				if (parent.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				parent.QL_OuterPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments)
				{
					if (parent.QL_OuterPackCount != 0)
					{
						parent.QL_OuterPackCountInfo.AddMessageError("Outer pack count must be zero when outer pack type is bulk or mixed shipments.");
					}
				}
				else
				{
					if (parent.QL_OuterPackCount <= 0)
					{
						parent.QL_OuterPackCountInfo.AddMessageError("Outer pack count must be greater than 0.");
					}
				}

				parent.Validation.ValidateQL_NetQuantity();
				parent.InvoiceLine.JI_InvoiceQuantityInfo.AddAllNotificationsFrom(parent.QL_OuterPackCountInfo);
			}
		}

		protected override void CheckQL_UseByStart()
		{
			base.CheckQL_UseByStart();
			if (IsValidationRequired && !parent.QL_UseByStart.IsEmpty)
			{
				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(parent.QuarantineExDocHeader.QH_ProduceType, parent.QL_UseByStartInfo, "Durability Start Date");
			}
		}

		protected override void CheckQL_UseByEnd()
		{
			base.CheckQL_UseByEnd();
			if (IsValidationRequired && !parent.QL_UseByEnd.IsEmpty)
			{
				EXDOCValidationHelper.CheckForProduceTypeIsHorticultureOrGrainsAndPlants(parent.QuarantineExDocHeader.QH_ProduceType, parent.QL_UseByEndInfo, "Durability End Date");
			}
		}

		protected override void CheckQL_OuterPackType()
		{
			base.CheckQL_OuterPackType();
			if (IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_OuterPackTypeInfo, parent.Lookups.PackageTypes);
				ValidateQL_OuterPackCount();
			}
		}

		protected override void CheckQL_OuterPackAccuracy()
		{
			base.CheckQL_OuterPackAccuracy();
			if (IsValidationRequired)
			{
				var header = parent.QuarantineExDocHeader;
				if (header.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish && header.IsNEXDOCSActive)
				{
					if (!parent.QL_OuterPackAccuracy.IsEmpty)
					{
						parent.QL_OuterPackAccuracyInfo.AddMessageError("Outer Pack accuracy is not allowed for this produce type.");
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_OuterPackAccuracyInfo, parent.Lookups.PackAccuracy);
				}
			}
		}

		protected override void CheckQL_OuterPackWeight()
		{
			base.CheckQL_OuterPackWeight();
			if (IsValidationRequired)
			{
				if (parent.QL_OuterPackWeight < 0)
				{
					parent.QL_OuterPackWeightInfo.AddMessageError("Outer pack weight must be greater than or equal to 0");
				}

				if (parent.QL_OuterPackWeight > 9999.999m)
				{
					parent.QL_OuterPackWeightInfo.AddMessageError("Outer pack weight cannot be greater than 9999.999");
				}

				else if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Dairy && parent.QL_OuterPackWeight.IsEmpty)
				{
					parent.QL_OuterPackWeightInfo.AddMessageError("Outer pack weight must be greater than 0");
				}

				else if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat &&
					!parent.QL_OuterPackWeight.IsEmpty &&
					parent.QuarantineExDocHeader.QH_ObtainExportCustomsPermit)
				{
					parent.QL_OuterPackWeightInfo.AddMessageError("Outer pack weight must be 0");
				}

				parent.Validation.ValidateQL_NetQuantity();
			}
		}

		protected override void CheckQL_OuterPackWeightUnit()
		{
			base.CheckQL_OuterPackWeightUnit();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Dairy || (parent.QL_OuterPackWeight > 0 && parent.QL_OuterPackWeightUnit.IsEmpty))
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_OuterPackWeightUnitInfo, parent.Lookups.Weight);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(parent.QL_OuterPackWeightUnitInfo, parent.Lookups.Weight);
				}

				parent.Validation.ValidateQL_NetQuantity();
			}
		}

		protected override void CheckQL_IntermediatePackCount()
		{
			base.CheckQL_IntermediatePackCount();
			if (IsValidationRequired)
			{
				if ((parent.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				parent.QL_IntermediatePackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments) &&
				parent.QL_IntermediatePackCount != 0)
				{
					parent.QL_IntermediatePackCountInfo.AddMessageError("Intermediate pack count must be zero when intermediate pack type is bulk or mixed shipments.");
				}

				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_IntermediatePackCount.IsEmpty)
				{
					parent.QL_IntermediatePackCountInfo.AddMessageError("Intermediate pack count must not be entered when product description is present.");
				}

				if (parent.QL_IntermediatePackCount < 0)
				{
					parent.QL_IntermediatePackCountInfo.AddMessageError("Intermediate pack count must be greater than or equal to 0.");
				}
			}
		}

		protected override void CheckQL_IntermediatePackType()
		{
			base.CheckQL_IntermediatePackType();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_IntermediatePackType.IsEmpty)
				{
					parent.QL_IntermediatePackTypeInfo.AddMessageError("Intermediate pack type must not be entered when exporter defined product description is present.");
				}

				ListValidation.MessageErrorIfInvalidCode(parent.QL_IntermediatePackTypeInfo, parent.Lookups.PackageTypes);
			}
		}

		protected override void CheckQL_IntermediatePackAccuracy()
		{
			base.CheckQL_IntermediatePackAccuracy();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_IntermediatePackAccuracy.IsEmpty)
				{
					parent.QL_IntermediatePackAccuracyInfo.AddMessageError("Intermediate pack accuracy must not be entered when exporter defined product description is present.");
				}

				ListValidation.MessageErrorIfInvalidCode(parent.QL_IntermediatePackAccuracyInfo, parent.Lookups.PackAccuracy);
			}
		}

		protected override void CheckQL_IntermediatePackWeight()
		{
			base.CheckQL_IntermediatePackWeight();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_IntermediatePackWeight.IsEmpty)
				{
					parent.QL_IntermediatePackWeightInfo.AddMessageError("Intermediate pack weight must not be entered when exporter defined product description is present.");
				}

				if (parent.QL_IntermediatePackWeight < 0)
				{
					parent.QL_IntermediatePackWeightInfo.AddMessageError("Intermediate pack weight must be greater than or equal to 0");
				}

				if (parent.QL_IntermediatePackWeight > 9999.999m)
				{
					parent.QL_IntermediatePackWeightInfo.AddMessageError("Intermediate pack weight cannot be greater than 9999.999");
				}
			}
		}

		protected override void CheckQL_IntermediatePackWeightUnit()
		{
			base.CheckQL_IntermediatePackWeightUnit();
			if (IsValidationRequired)
			{
				if (!parent.QL_IntermediatePackWeightUnit.IsEmpty)
				{
					if (!parent.InvoiceLine.JI_Description.IsEmpty)
					{
						parent.QL_IntermediatePackWeightUnitInfo.AddMessageError("Intermediate pack weight unit must not be entered when exporter defined product description is present.");
					}

					ListValidation.MessageErrorIfInvalidCode(parent.QL_IntermediatePackWeightUnitInfo, parent.Lookups.Weight);
				}
				else if (!parent.QL_IntermediatePackWeight.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_IntermediatePackWeightUnitInfo, parent.Lookups.Weight);
				}
			}
		}

		protected override void CheckQL_InnerPackCount()
		{
			base.CheckQL_InnerPackCount();
			if (IsValidationRequired)
			{
				if ((parent.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.Bulk ||
				parent.QL_InnerPackType == EXDOCPacakgeTypeCodes.Codes.MixedShipments) &&
				parent.QL_InnerPackCount != 0)
				{
					parent.QL_InnerPackCountInfo.AddMessageError("Inner pack count must be zero when inner pack type is bulk or mixed shipments.");
				}

				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_InnerPackCount.IsEmpty)
				{
					parent.QL_InnerPackCountInfo.AddMessageError("Inner pack count must not be entered when exporter defined product description is present.");
				}

				if (parent.QL_InnerPackCount < 0)
				{
					parent.QL_InnerPackCountInfo.AddMessageError("Inner pack count must be greater than or equal to 0.");
				}
			}
		}

		protected override void CheckQL_InnerPackType()
		{
			base.CheckQL_InnerPackType();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_InnerPackType.IsEmpty)
				{
					parent.QL_InnerPackTypeInfo.AddMessageError("Inner pack type must not be entered when exporter defined product description is present.");
				}

				ListValidation.MessageErrorIfInvalidCode(parent.QL_InnerPackTypeInfo, parent.Lookups.PackageTypes);
			}
		}

		protected override void CheckQL_InnerPackAccuracy()
		{
			base.CheckQL_InnerPackAccuracy();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_InnerPackAccuracy.IsEmpty)
				{
					parent.QL_InnerPackAccuracyInfo.AddMessageError("Inner pack accuracy must not be entered when exporter defined product description is present.");
				}

				ListValidation.MessageErrorIfInvalidCode(parent.QL_InnerPackAccuracyInfo, parent.Lookups.PackAccuracy);
			}
		}

		protected override void CheckQL_InnerPackWeight()
		{
			base.CheckQL_InnerPackWeight();
			if (IsValidationRequired)
			{
				if (!parent.InvoiceLine.JI_Description.IsEmpty && !parent.QL_InnerPackWeight.IsEmpty)
				{
					parent.QL_InnerPackWeightInfo.AddMessageError("Inner pack weight must not be entered when exporter defined product description is present.");
				}

				if (parent.QL_InnerPackWeight < 0)
				{
					parent.QL_InnerPackWeightInfo.AddMessageError("Inner pack weight must be greater than or equal 0");
				}

				if (parent.QL_InnerPackWeight > 9999.999m)
				{
					parent.QL_InnerPackWeightInfo.AddMessageError("Inner pack weight cannot be greater than 9999.999");
				}
			}
		}

		protected override void CheckQL_InnerPackWeightUnit()
		{
			base.CheckQL_InnerPackWeightUnit();
			if (IsValidationRequired)
			{
				if (!parent.QL_InnerPackWeightUnit.IsEmpty)
				{
					if (!parent.InvoiceLine.JI_Description.IsEmpty)
					{
						parent.QL_InnerPackWeightUnitInfo.AddMessageError("Inner pack weight unit must not be entered when exporter defined product description is present.");
					}

					ListValidation.MessageErrorIfInvalidCode(parent.QL_InnerPackWeightUnitInfo, parent.Lookups.Weight);
				}
				else if (!parent.QL_InnerPackWeight.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_InnerPackWeightUnitInfo, parent.Lookups.Weight);
				}
			}
		}

		public void ValidateQL_ProduceType()
		{
			((IValidationInternals)this).Validate(parent.QL_ProduceTypeInfo, delegate
			{
				CheckQL_ProduceType();
			});
		}

		void CheckQL_ProduceType()
		{
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader != null)
				{
					if (!parent.Processes.ProcessCounts.Processing &&
						(parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Dairy ||
						parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Eggs))
					{
						if (!parent.QuarantineExDocHeader.IsNEXDOCSActive)
						{
							parent.QL_ProduceTypeInfo.AddMessageError("At least one processing process is required when produce type is Dairy, Eggs or Fish");
						}
						else
						{
							parent.QL_ProduceTypeInfo.AddMessageError("At least one processing process is required when produce type is Dairy or Eggs.");
						}
					}

					if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish)
					{
						if (!parent.QuarantineExDocHeader.IsNEXDOCSActive)
						{
							if (!parent.Processes.ProcessCounts.Processing)
							{
								parent.QL_ProduceTypeInfo.AddMessageError("At least one processing process is required when produce type is Dairy, Eggs or Fish");
							}
						}
						else
						{
							if (!parent.Processes.ProcessCounts.Processing && parent.Processes.ProcessCounts.CatcherVessel == 0)
							{
								parent.QL_ProduceTypeInfo.AddMessageError("At least one processing process (PC) or catcher vessel (CT) is required when produce type is Fish.");
							}
						}
					}

					if (!parent.Processes.ProcessCounts.Packing && parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat)
					{
						parent.QL_ProduceTypeInfo.AddMessageError("At least one packing process is required when produce type is Meat");
					}

					if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Meat &&
						!parent.Processes.ProcessCounts.Slaughter &&
						parent.QuarantineExDocHeader.Declaration.FinalDestination != null &&
						parent.QuarantineExDocHeader.Declaration.FinalDestination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.Japan)
					{
						parent.QL_ProduceTypeInfo.AddMessageError("At least one slaughter process is required for Meat shipments to Japan.");
					}

					if (parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.Fish &&
						parent.QuarantineExDocHeader.Declaration.FinalDestination != null &&
						parent.QuarantineExDocHeader.Declaration.FinalDestination.RL_RN_NKCountryCode == Core.Constants.CountryCodes.China)
					{
						switch (parent.QL_SupplimentaryCode)
						{
							case "WO": // WILD ORIGIN
								if (!parent.QuarantineExDocHeader.IsNEXDOCSActive)
								{
									if (parent.Processes.ProcessCounts.CatcherVessel == 0)
									{
										parent.QL_ProduceTypeInfo.AddMessageError("Catcher vessel process is required for wild origin Fish shipments to China.");
									}
								}
								else
								{
									if (parent.Processes.ProcessCounts.CatcherVessel == 0 && parent.Processes.ProcessCounts.CatcherBoat == 0)
									{
										parent.QL_ProduceTypeInfo.AddMessageError("A catcher vessel (CT) or boat (CB) process is required for wild origin Fish shipments to China.");
									}
								}
								break;
							case "AQ": // AQUACULTURE
								if (IsAquacultureFarmProcessMandatoryForProductType(parent.QL_ProductType) &&
									parent.Processes.ProcessCounts.AquacultureFarm == 0)
								{
									var productDescription = GetProductTypeDescription(parent.QL_ProductType);
									if (productDescription.IsEmpty)
									{
										productDescription = parent.QL_ProductType;
									}

									parent.QL_ProduceTypeInfo.AddMessageError(
										"Aquaculture farm process is required for aquaculture " + productDescription + " shipments to China.");
								}
								break;
						}
					}
				}
			}
		}

		protected override void CheckQL_BeefVealWeightAmount()
		{
			base.CheckQL_BeefVealWeightAmount();
			if (IsValidationRequired)
			{
				if (!parent.QL_BeefVealWeightAmount.IsEmpty)
				{
					if (parent.QL_ProduceType == EXDOCCommodityCodes.Codes.Meat)
					{
						var cutCode = parent.Lookups.CutCodes.FindByCode(parent.QL_CutCode);
						if (cutCode == null)
						{
							parent.QL_BeefVealWeightAmountInfo.AddMessageError("This cut code cannot be selected because it is not of the correct commodity type.");
						}
						else if (cutCode.GetAttributesValues(EXDOCCutCodeCollection.AttributeIsBeefVeal).FirstOrDefault() != "Y")
						{
							parent.QL_BeefVealWeightAmountInfo.AddMessageError("Beef veal weight is invalid for selected cut code.");
						}
					}
					else
					{
						parent.QL_BeefVealWeightAmountInfo.AddMessageError("Beef veal weight may only be present when produce type is Meat.");
					}
				}
			}
		}

		protected override void CheckQL_Category()
		{
			base.CheckQL_Category();

			var header = parent?.QuarantineExDocHeader;
			if (IsValidationRequired && header != null && header.IsNEXDOCSActive)
			{
				var info = parent.QL_CategoryInfo;
				var category = parent.QL_Category;

				var productType = parent.QL_ProductType;
				if (productType.IsEmpty)
				{
					if (!category.IsEmpty)
					{
						info.AddMessageError(ProductTypeMustBeEnteredBeforeCategoryCode);
					}
				}
				else
				{
					var productTypeValues = dbValidationHelper.GetProductTypeValues(category, EXDOCCommodityCodesSingleChar.GetSingleCharFromThreeCharCode(parent.QuarantineExDocHeader.QH_ProduceType));
					if (!productTypeValues.Any(x => x.EqualsIgnoringCase(productType)))
					{
						info.AddMessageError(string.Format(CultureInfo.CurrentCulture, CategoryShouldHaveProductTypeAttribute, productType));
					}
				}

				var invoiceLine = parent.InvoiceLine;
				var tariff = invoiceLine?.TariffNumber ?? ZString.Empty;
				var isDairy = (parent.QuarantineExDocHeader?.QH_ProduceType ?? ZString.Empty) == EXDOCCommodityCodes.Codes.Dairy;

				if (!category.IsEmpty && !tariff.IsEmpty && (isDairy || (invoiceLine.Declaration?.IsDestinedOrTransitingThroughEU ?? false)))
				{
					var aheccValues = dbValidationHelper.GetAHECCValues(category);
					if (!aheccValues.Any(c => tariff.StartsWith(c, System.StringComparison.OrdinalIgnoreCase)))
					{
						info.AddMessageError(DontHaveMatchedAHECCAttribute);
					}
				}
			}
		}

		internal const string CategoryShouldHaveProductTypeAttribute = "The selected Category Code does not have the Product '{0}' as a reference attribute in the Category Code Product attributes field.";
		internal const string ProductTypeMustBeEnteredBeforeCategoryCode = "Product must be entered before a Category Code can be entered.";
		internal const string DontHaveMatchedAHECCAttribute = "There is no AHECC associated with this Category Code.";

		protected override void CheckQL_ChemicalLeanPercentage()
		{
			base.CheckQL_ChemicalLeanPercentage();
			if (IsValidationRequired)
			{
				if (!parent.QL_ChemicalLeanPercentage.IsEmpty)
				{
					if (parent.QL_ProduceType == EXDOCCommodityCodes.Codes.Meat)
					{
						var cutCode = parent.Lookups.CutCodes.FindByCode(parent.QL_CutCode);
						if (cutCode != null && cutCode.GetAttributesValues(EXDOCCutCodeCollection.AttributeIsChemicalLean).FirstOrDefault() != "Y")
						{
							parent.QL_ChemicalLeanPercentageInfo.AddMessageError("Chemical lean percentage is invalid for selected cut code.");
						}
						if (parent.QL_PackType != EXDOCPackTypeCodes.Codes.BulkPack)
						{
							parent.QL_ChemicalLeanPercentageInfo.AddMessageError("Chemical lean percentage is invalid for the pack type");
						}
					}
					else
					{
						parent.QL_ChemicalLeanPercentageInfo.AddMessageError("Chemical lean percentage may only be present when produce type is Meat.");
					}
				}
			}
		}

		protected override void CheckQL_GrowerNumber()
		{
			base.CheckQL_GrowerNumber();
			if (IsValidationRequired)
			{
				if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Horticulture && !parent.QL_GrowerNumber.IsEmpty)
				{
					parent.QL_GrowerNumberInfo.AddMessageError("Grower Number may only be present when produce type is Horticulture.");
				}
			}
		}

		protected override void CheckQL_HCFormatRequested()
		{
			base.CheckQL_HCFormatRequested();
			if (IsValidationRequired)
			{
				if (parent.QL_HCFormatRequested.Contains('/'))
				{
					int slashPos = parent.QL_HCFormatRequested.IndexOf('/');
					ZString hCTemplate = parent.QL_HCFormatRequested.Left(slashPos);
					ZString hCEndorsement = parent.QL_HCFormatRequested.SubstringSafe(slashPos + 1);
					if (hCTemplate.Length > 6)
					{
						parent.QL_HCFormatRequestedInfo.AddMessageError("Certificate template code may have a maximum of 6 characters");
					}
					if (hCEndorsement.Length > 4)
					{
						parent.QL_HCFormatRequestedInfo.AddMessageError("Certificate endorsement code may have a maximum of 4 characters");
					}
				}
				else if (parent.QL_HCFormatRequested.Length > 6)
				{
					parent.QL_HCFormatRequestedInfo.AddMessageError("Certificate template code may have a maximum of 6 characters");
				}
			}
		}

		protected override void CheckQL_StatementNumber1()
		{
			base.CheckQL_StatementNumber1();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementNumber1.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_StatementNumber1Info.AddMessageError("Statement number may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_StatementNumber2()
		{
			base.CheckQL_StatementNumber2();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementNumber2.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_StatementNumber2Info.AddMessageError("Statement number may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_StatementNumber3()
		{
			base.CheckQL_StatementNumber3();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementNumber3.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_StatementNumber3Info.AddMessageError("Statement number may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_StatementNumber4()
		{
			base.CheckQL_StatementNumber4();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementNumber4.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_StatementNumber4Info.AddMessageError("Statement number may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_StatementNumber5()
		{
			base.CheckQL_StatementNumber5();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementNumber5.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_StatementNumber5Info.AddMessageError("Statement number may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_StatementText()
		{
			base.CheckQL_StatementText();
			if (IsValidationRequired)
			{
				if (!parent.QL_StatementText.IsEmpty &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Horticulture)
				{
					parent.QL_StatementTextInfo.AddMessageError("Statement text may only be supplied when produce type is Horticulture.");
				}

				if (!parent.QL_StatementText.IsEmpty &&
					parent.InvoiceLine.JI_TempImportNum.IsEmpty)
				{
					parent.QL_StatementTextInfo.AddMessageError("Temporary import permit number must be present if.");
				}
			}
		}

		protected override void CheckQL_AddtionalDeclarationComments()
		{
			base.CheckQL_AddtionalDeclarationComments();
			if (IsValidationRequired)
			{
				if (!parent.QL_AddtionalDeclarationComments.IsEmpty && !IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_AddtionalDeclarationCommentsInfo.AddMessageError("Additional declaration may only be supplied when produce type is Horticulture or Grains and Plants.");
				}
			}
		}

		protected override void CheckQL_DrainedWeight()
		{
			base.CheckQL_DrainedWeight();
			if (IsValidationRequired)
			{
				if (!parent.QL_DrainedWeight.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
				{
					parent.QL_DrainedWeightInfo.AddMessageError("Drained weight may only be supplied when produce type is Fish.");
				}

				if (!parent.QL_DrainedWeightUnit.IsEmpty && parent.QL_DrainedWeight.IsEmpty)
				{
					parent.QL_DrainedWeightInfo.AddMessageError("Drained weight may not be empty when a unit has been entered.");
				}
			}
		}

		protected override void CheckQL_PercentOfMilkFat()
		{
			base.CheckQL_PercentOfMilkFat();
			if (IsValidationRequired)
			{
				if (!parent.QL_PercentOfMilkFat.IsEmpty)
				{
					if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
					{
						parent.QL_PercentOfMilkFatInfo.AddMessageError("Milk fat % may only be supplied when produce type is Dairy");
					}

					if (parent.QL_PercentOfMilkFat < 0 || parent.QL_PercentOfMilkFat > 100)
					{
						parent.QL_PercentOfMilkFatInfo.AddMessageError("Milk fat % must be greater than or equal to 0 and less than or equal to 100");
					}
				}
			}
		}

		protected override void CheckQL_PercentOfMilkProtein()
		{
			base.CheckQL_PercentOfMilkProtein();
			if (IsValidationRequired)
			{
				if (!parent.QL_PercentOfMilkProtein.IsEmpty)
				{
					if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
					{
						parent.QL_PercentOfMilkProteinInfo.AddMessageError("Milk protein % may only be supplied when produce type is Dairy");
					}

					if (parent.QL_PercentOfMilkProtein < 0 || parent.QL_PercentOfMilkProtein > 100)
					{
						parent.QL_PercentOfMilkProteinInfo.AddMessageError("Milk protein % must be greater than or equal to 0 and less than or equal to 100");
					}
				}
			}
		}

		protected override void CheckQL_TotalWeightOfMilkFatInMixtures()
		{
			base.CheckQL_TotalWeightOfMilkFatInMixtures();
			if (IsValidationRequired)
			{
				if (!parent.QL_TotalWeightOfMilkFatInMixtures.IsEmpty &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					parent.QL_TotalWeightOfMilkFatInMixturesInfo.AddMessageError("Total weight of milk fat may only be supplied when produce type is Dairy");
				}
			}
		}

		protected override void CheckQL_TotalWeightOfMilkProteinInMixtures()
		{
			base.CheckQL_TotalWeightOfMilkProteinInMixtures();
			if (IsValidationRequired)
			{
				if (!parent.QL_TotalWeightOfMilkProteinInMixtures.IsEmpty &&
				parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					parent.QL_TotalWeightOfMilkProteinInMixturesInfo.AddMessageError("Total weight of milk protein may only be supplied when produce type is Dairy");
				}
			}
		}

		protected override void CheckQL_DrainedWeightUnit()
		{
			base.CheckQL_DrainedWeightUnit();
			if (IsValidationRequired)
			{
				if (!parent.QL_DrainedWeightUnit.IsEmpty)
				{
					if (parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Fish)
					{
						parent.QL_DrainedWeightUnitInfo.AddMessageError("Drained weight unit may only be supplied when produce type is Fish.");
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(parent.QL_DrainedWeightUnitInfo, parent.Lookups.MetricWeight);
					}
				}
				else if (!parent.QL_DrainedWeight.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.QL_DrainedWeightUnitInfo, parent.Lookups.MetricWeight);
				}
			}
		}

		protected override void CheckQL_SaltingDate()
		{
			base.CheckQL_SaltingDate();
			if (IsValidationRequired)
			{
				if (!parent.QL_SaltingDate.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.SkinsAndHides)
				{
					parent.QL_SaltingDateInfo.AddMessageError("Salting date may only be supplied when produce type is Skins and Hides.");
				}
			}
		}

		protected override void CheckQL_MeatInspectionDescription()
		{
			base.CheckQL_MeatInspectionDescription();
			if (IsValidationRequired)
			{
				if (!parent.QL_MeatInspectionDescription.IsEmpty && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Meat && parent.QuarantineExDocHeader.QH_ProduceType != EXDOCCommodityCodes.Codes.Dairy)
				{
					parent.QL_MeatInspectionDescriptionInfo.AddMessageError(ResString.GetMultilingualString("1F7DC116-EDE4-4FA4-9D0C-E66E3FEECF34", "Line item description may only be supplied when produce type is Meat or Dairy."));
				}
			}
		}

		protected override void CheckQL_FishWaterIndicator()
		{
			base.CheckQL_FishWaterIndicator();
			ListValidation.MessageErrorIfInvalidCode(parent.QL_FishWaterIndicatorInfo);
		}

		protected override void CheckQL_CombinedNomenclature()
		{
			base.CheckQL_CombinedNomenclature();
			var combinedNomenclature = parent.QL_CombinedNomenclature;
			if (!combinedNomenclature.IsEmpty)
			{
				if (!(combinedNomenclature.IsNumbersOnlyOrEmpty && (combinedNomenclature.Length == 6 || combinedNomenclature.Length == 8)))
				{
					parent.QL_CombinedNomenclatureInfo.AddMessageError(ResString.GetMultilingualString("5B4CBF35-C30B-4F49-B043-952307C5083D", "Combined Nomenclature must be 6 or 8 digits."));
				}
			}
		}

		protected override void CheckQL_ProductPart()
		{
			base.CheckQL_ProductPart();

			if (!parent.QL_ProductPart.IsEmpty && IsValidationRequired)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.QL_ProductPartInfo);

				if (!IsProduceTypeHorticultureOrGrainsAndPlants)
				{
					parent.QL_ProductPartInfo.AddMessageError(ResString.GetMultilingualString("25F3B107-0922-47F2-BE4B-163980A1665C", ProductPartNotRequired));
				}
			}
		}

		const string ProductPartNotRequired = "Product Part may only be present when Produce Type is Horticulture or Grains and Plants.";

		readonly QuarantineExDocLine parent;

		bool IsAquacultureFarmProcessMandatoryForProductType(ZString code)
		{
			switch (code)
			{
				case "":
				case "CMS":
				case "COC":
				case "MUB":
				case "OYL":
				case "OYN":
				case "OYP":
				case "OYR":
				case "OYS":
				case "PIP":
				case "SCC":
				case "SCE":
				case "SCF":
				case "SCM":
				case "SCO":
				case "SCP":
				case "SCR":
				case "SCS":
				case "SCX":
					return false;
				default:
					return true;
			}
		}

		bool IsProduceTypeHorticultureOrGrainsAndPlants
		{
			get
			{
				var produceType = parent.QuarantineExDocHeader.QH_ProduceType;
				return produceType == EXDOCCommodityCodes.Codes.Horticulture || produceType == EXDOCCommodityCodes.Codes.GrainsAndPlants;
			}
		}

		ZString GetProductTypeDescription(ZString code)
		{
			var product = parent.Lookups.ProductTypes.FindByCode(code);
			return product != null ? product.ZZD_Description : ZString.Empty;
		}
	}
}
