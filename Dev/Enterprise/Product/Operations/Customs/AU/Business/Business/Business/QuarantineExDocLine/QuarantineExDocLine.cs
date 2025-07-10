using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocLine : AutoQuarantineExDocLine,
		Integration.Customs.AU.IQuarantineExdocLine,
		ICusCodeDataTypeSupporter,
		IClusterKeyWorker
	{
		public QuarantineExDocLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoQuarantineExDocLine.Schema
		{
			public const string QL_ImportAuthorityCode = "QL_ImportAuthorityCode";
			public const string QL_SendHCDesc = "QL_SendHCDesc";
			public const string QL_AqisCustomsWeight = "QL_AqisCustomsWeight";
			public const string QL_AqisCustomsWeightUQ = "QL_AqisCustomsWeightUQ";
			public const string QL_DominantProduct = "QL_DominantProduct";
			public const string QL_AdditionalProducts = "QL_AdditionalProducts";
			public const string QL_HalalProductIndicator = "QL_HalalProductIndicator";
			public const string QL_FormattedCombinedNomenclature = "QL_FormattedCombinedNomenclature";
		}

		public ZString ProductCode
		{
			get
			{
				ZString preservationType = QL_PreservationType.PadRight(1).IsEmpty ? (ZString)"X" : QL_PreservationType.PadRight(1);
				return preservationType + QL_ProductType.PadRight(3) + QL_PackType.PadRight(2) + QL_SupplimentaryCode.PadRight(2);
			}
		}

		[ChildEditable(true)]
		public QuarantineExDocEstablishmentAndTimeCollection Processes
		{
			get
			{
				if (fProcesses == null)
				{
					fProcesses = new QuarantineExDocEstablishmentAndTimeCollection(this);
					fProcesses.Load();
					RegisterEditableChildObject(fProcesses);
				}
				return fProcesses;
			}
		}
		QuarantineExDocEstablishmentAndTimeCollection fProcesses;

		public override void Delete()
		{
			base.Delete();
			Processes.RemoveAndDeleteAll();
			ProductConditions.RemoveAndDeleteAll();
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.Load<JobComInvoiceLine>(QL_JI)); }
		}
		JobComInvoiceLine invoiceLine;

		public QuarantineExDocHeader QuarantineExDocHeader
		{
			get { return InvoiceLine?.InvoiceHeader?.QuarantineExDocHeader; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			QL_OuterPackAccuracy = EXDOCPackAccuracyCodes.Codes.EqualTo;
		}

		public bool HasQuarantineLineBeenAccepted()
		{
			var quarantineHeader = InvoiceLine.InvoiceHeader?.QuarantineExDocHeader;
			return (quarantineHeader?.HasQuarantineHeaderBeenAccepted() ?? false)
				&& invoiceLine.JI_LineNo <= quarantineHeader.QH_QuarantineMessageMaxLine;
		}

		public void UpdateProcessesToLodged()
		{
			var processes = Processes.Cast<QuarantineExDocEstablishmentAndTime>().ToArray();
			foreach (var process in processes)
			{
				if (process.EE_EstablishmentPostedStatus == NEXDOCEstablishmentPostedStatus.Codes.DeletePending)
				{
					process.Delete();
				}
				else if (process.EE_EstablishmentPostedStatus != NEXDOCEstablishmentPostedStatus.Codes.Lodged)
				{
					process.EE_EstablishmentPostedStatus = NEXDOCEstablishmentPostedStatus.Codes.Lodged;
				}
			}
		}

		#region QL_BeefVealWeightAmount

		[DecimalPlaces(2)]
		public override ZDecimal QL_BeefVealWeightAmount
		{
			get { return base.QL_BeefVealWeightAmount; }
			set { base.QL_BeefVealWeightAmount = value; }
		}

		#endregion

		#region QL_CutCode

		[List(nameof(Lookups) + "+" + nameof(CommonQuarantineExDocLineLookups.CutCodes))]
		public override ZString QL_CutCode
		{
			get { return base.QL_CutCode; }
			set { base.QL_CutCode = value; }
		}

		#endregion

		#region QL_DrainedWeight

		[DecimalPlaces(2)]
		public override ZDecimal QL_DrainedWeight
		{
			get { return base.QL_DrainedWeight; }
			set { base.QL_DrainedWeight = value; }
		}

		#endregion

		#region Customs weight

		public ZDecimal QL_AqisCustomsWeight
		{
			get { return InvoiceLine.AddInfo.ZA_AQISCustomsWt_Hidden; }
			set
			{
				if (InvoiceLine.AddInfo.ZA_AQISCustomsWt_Hidden != value)
				{
					InvoiceLine.AddInfo.ZA_AQISCustomsWt_Hidden = value;
					if (!value.IsEmpty && QL_AqisCustomsWeightUQ.IsEmpty)
					{
						QL_AqisCustomsWeightUQ = EXDOCErrata32List36CustomsWeightUnits.Codes.KGM;
					}
				}
			}
		}

		public ZPropertyInfo QL_AqisCustomsWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_AqisCustomsWeight, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_AQISCustomsWt_HiddenInfo)); }
		}

		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.AqisCustomsWeightUqList))]
		public ZString QL_AqisCustomsWeightUQ
		{
			get { return InvoiceLine.AddInfo.ZA_AQISCustomsWtUQ_Hidden; }
			set
			{
				if (InvoiceLine.AddInfo.ZA_AQISCustomsWtUQ_Hidden != value)
				{
					InvoiceLine.AddInfo.ZA_AQISCustomsWtUQ_Hidden = value;
				}
			}
		}

		public ZPropertyInfo QL_AqisCustomsWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_AqisCustomsWeightUQ, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_AQISCustomsWtUQ_HiddenInfo)); }
		}

		#endregion

		#region QL_EffectiveShippingMarks
		public ZString QL_EffectiveShippingMarks
		{
			get
			{
				var declaration = QL_ShippingMarks.IsEmpty ? QuarantineExDocHeader?.Declaration : null;
				return declaration == null ? QL_ShippingMarks : declaration.JE_MarksAndNumbers;
			}
		}
		#endregion

		#region QL_EffectiveHealthCertificateDescription
		public ZString QL_EffectiveHealthCertificateDescription
		{
			get
			{
				if (QL_HealthCertificateDescription.IsEmpty && QL_SendHCDesc)
				{
					return InvoiceLine.JI_Description;
				}
				else
				{
					return QL_HealthCertificateDescription;
				}
			}
		}

		public ZPropertyInfo QL_EffectiveHealthCertificateDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(QL_EffectiveHealthCertificateDescription)); }
		}
		#endregion

		#region QL_FarmCode

		[ResourceStringData("AUQuarantineExDocLine|46D4F69F-A538-4ED5-8800-67FF2F6BA0F8", Caption = "Farm Code")]
		public override ZString QL_FarmCode
		{
			get => base.QL_FarmCode;
			set => base.QL_FarmCode = value;
		}

		#endregion

		#region QL_FarmType

		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.FarmType))]
		[ResourceStringData("AUQuarantineExDocLine|A2D3CC92-8AD7-4122-B13A-0790F889A932", Caption = "Farm Type")]
		public override ZString QL_FarmType
		{
			get => base.QL_FarmType;
			set => base.QL_FarmType = value;
		}

		#endregion

		#region QL_GrossMetricWeight

		[DecimalPlaces(3)]
		public override ZDecimal QL_GrossMetricWeight
		{
			get { return base.QL_GrossMetricWeight; }
			set
			{
				var weight = value.Round(3);        // Data import may have > 3 decimals
				if (QL_GrossMetricWeight != weight)
				{
					base.QL_GrossMetricWeight = weight;
					if (InvoiceLine.JI_Weight != weight)
					{
						InvoiceLine.JI_Weight = weight;
					}
				}
			}
		}

		#endregion

		#region QL_ImperialNetWeight

		[DecimalPlaces(3)]
		public override ZDecimal QL_ImperialNetWeight
		{
			get { return base.QL_ImperialNetWeight; }
			set { base.QL_ImperialNetWeight = value; }
		}

		#endregion

		#region QL_InnerPackWeight

		[DecimalPlaces(3)]
		public override ZDecimal QL_InnerPackWeight
		{
			get { return base.QL_InnerPackWeight; }
			set { base.QL_InnerPackWeight = value; }
		}

		#endregion

		#region QL_IntermediatePackWeight

		[DecimalPlaces(3)]
		public override ZDecimal QL_IntermediatePackWeight
		{
			get { return base.QL_IntermediatePackWeight; }
			set { base.QL_IntermediatePackWeight = value; }
		}

		#endregion

		#region QL_NetQuantity

		[DecimalPlaces(3)]
		public override ZDecimal QL_NetQuantity
		{
			get { return base.QL_NetQuantity; }
			set { base.QL_NetQuantity = value; }
		}

		#endregion

		#region QL_OuterPackWeight

		[DecimalPlaces(3)]
		public override ZDecimal QL_OuterPackWeight
		{
			get { return base.QL_OuterPackWeight; }
			set { base.QL_OuterPackWeight = value; }
		}

		#endregion

		#region QL_PercentOfMilkFat

		[DecimalPlaces(2)]
		public override ZDecimal QL_PercentOfMilkFat
		{
			get { return base.QL_PercentOfMilkFat; }
			set { base.QL_PercentOfMilkFat = value; }
		}

		#endregion

		#region QL_PercentOfMilkProtein

		[DecimalPlaces(2)]
		public override ZDecimal QL_PercentOfMilkProtein
		{
			get { return base.QL_PercentOfMilkProtein; }
			set { base.QL_PercentOfMilkProtein = value; }
		}

		#endregion

		#region QL_TotalWeightOfMilkFatInMixtures

		[DecimalPlaces(2)]
		public override ZDecimal QL_TotalWeightOfMilkFatInMixtures
		{
			get { return base.QL_TotalWeightOfMilkFatInMixtures; }
			set { base.QL_TotalWeightOfMilkFatInMixtures = value; }
		}

		#endregion

		#region QL_TotalWeightOfMilkProteinInMixtures

		[DecimalPlaces(2)]
		public override ZDecimal QL_TotalWeightOfMilkProteinInMixtures
		{
			get { return base.QL_TotalWeightOfMilkProteinInMixtures; }
			set { base.QL_TotalWeightOfMilkProteinInMixtures = value; }
		}

		#endregion

		#region QL_ProduceType
		public ZString QL_ProduceType
		{
			get { return QuarantineExDocHeader != null ? QuarantineExDocHeader.QH_ProduceType : ZString.Empty; }
		}

		public ZPropertyInfo QL_ProduceTypeInfo
		{
			get { return GetZPropertyInfo(nameof(QL_ProduceType)); }
		}
		#endregion

		#region QL_ProductType

		[List(nameof(Lookups) + "+" + nameof(CommonQuarantineExDocLineLookups.ProductTypes))]
		public override ZString QL_ProductType
		{
			get { return base.QL_ProductType; }
			set { base.QL_ProductType = value; }
		}

		#endregion

		#region QL_Category

		[List(nameof(Lookups) + "+" + nameof(CommonQuarantineExDocLineLookups.CategoryCodes))]
		public override ZString QL_Category
		{
			get { return base.QL_Category; }
			set { base.QL_Category = value; }
		}

		#endregion

		#region QL_SupplimentaryCode

		[List(nameof(Lookups) + "+" + nameof(CommonQuarantineExDocLineLookups.SupplementaryCodes))]
		public override ZString QL_SupplimentaryCode
		{
			get { return base.QL_SupplimentaryCode; }
			set { base.QL_SupplimentaryCode = value; }
		}

		#endregion

		#region QL_ImportAuthorityCode
		public ZString QL_ImportAuthorityCode
		{
			get
			{
				return InvoiceLine.AddInfo.ZA_ImportAuthorityCode_Hidden;
			}
			set
			{
				if (InvoiceLine.AddInfo.ZA_ImportAuthorityCode_Hidden != value)
				{
					InvoiceLine.AddInfo.ZA_ImportAuthorityCode_Hidden = value;
				}
			}
		}
		public ZPropertyInfo QL_ImportAuthorityCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_ImportAuthorityCode, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_ImportAuthorityCode_HiddenInfo)); }
		}
		#endregion

		#region QL_SendHCDesc
		public ZBool QL_SendHCDesc
		{
			get { return (InvoiceLine.AddInfo.ZA_SendHCDesc_Hidden == "Y"); }
			set
			{
				if (InvoiceLine.AddInfo.ZA_SendHCDesc_Hidden == "Y" ^ value)
				{
					InvoiceLine.AddInfo.ZA_SendHCDesc_Hidden = value ? "Y" : "";
				}
			}
		}
		public ZPropertyInfo QL_SendHCDescInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_SendHCDesc, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_SendHCDesc_HiddenInfo)); }
		}
		#endregion

		#region QL_HCFormatAllocated
		[ReadOnly(true)]
		public override ZString QL_HCFormatAllocated
		{
			get { return base.QL_HCFormatAllocated; }
		}
		#endregion

		#region QL_HCNumber
		[ReadOnly(true)]
		public override ZString QL_HCNumber
		{
			get { return base.QL_HCNumber; }
			set { base.QL_HCNumber = value; }
		}
		#endregion

		#region QL_FishWaterIndicator

		[List(nameof(Lookups) + "+" + nameof(CommonQuarantineExDocLineLookups.FishWaterIndicatorList))]
		public override ZString QL_FishWaterIndicator { get => base.QL_FishWaterIndicator; set => base.QL_FishWaterIndicator = value; }

		#endregion

		#region Cloning
		public QuarantineExDocLine Clone(JobComInvoiceLine invoiceLine, Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs)
		{
			QuarantineExDocLine clonedExdocLine = invoiceLine.QuarantineExDocLine;
			using (clonedExdocLine.SuspendSettingHasChanges())
			using (clonedExdocLine.GetValidationSuspender())
			{
				clonedExdocLine.IsCloning = true;
				try
				{
					List<string> excludedProperties = new List<string>();
					excludedProperties.Add(QuarantineExDocLineSchema.Constants.QL_JI);
					excludedProperties.Add(QuarantineExDocLineSchema.Constants.QL_HCFormatAllocated);
					excludedProperties.Add(QuarantineExDocLineSchema.Constants.QL_HCNumber);

					clonedExdocLine.CopyPersistentValuesFrom(this, new BusinessObjectCloneArgs(excludedProperties.ToArray()));
					clonedExdocLine.QL_JI = invoiceLine.PK;

					clonedExdocLine.Processes.Clone(Processes, jobDocAddressPKPairs);
				}
				finally
				{
					clonedExdocLine.IsCloning = false;
				}
			}
			return clonedExdocLine;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected internal bool IsCloning;
		#endregion

		#region Mark As Needing Validation

		[RelatedBusinessObject(nameof(InvoiceLine))]
		public override ZGuid QL_JI
		{
			get { return base.QL_JI; }
			set
			{
				bool hasChanged = base.QL_JI != value;
				if (hasChanged)
				{
					base.QL_JI = value;
					invoiceLine = null;
					Processes.MarkAsNeedingValidation();
					if (InvoiceLine != null)
					{
						InvoiceLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region RFP Document Properties
		public ZString OuterPackAccuracyDescription
		{
			get
			{
				CodeDescriptionPairList packAccuracy = new EXDOCPackAccuracyCodes();
				ZString result = packAccuracy.GetDescriptionFromCode(QL_OuterPackAccuracy);
				return result;
			}
		}

		public ZString IntermediatePackAccuracyDescription
		{
			get
			{
				CodeDescriptionPairList packAccuracy = new EXDOCPackAccuracyCodes();
				ZString result = packAccuracy.GetDescriptionFromCode(QL_IntermediatePackAccuracy);
				return result;
			}
		}

		public ZString InnerPackAccuracyDescription
		{
			get
			{
				CodeDescriptionPairList packAccuracy = new EXDOCPackAccuracyCodes();
				ZString result = packAccuracy.GetDescriptionFromCode(QL_InnerPackAccuracy);
				return result;
			}
		}

		public ZString FormattedProcesses
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				ZStringBuilder oneLine;
				CodeDescriptionPairList processTypes = new EXDOCProcessTypeCodes();
				foreach (QuarantineExDocEstablishmentAndTime process in Processes)
				{
					oneLine = new ZStringBuilder();
					oneLine.Append("Type: [" + processTypes.GetDescriptionFromCode(process.EE_ProcessingType) + "]");
					if (!process.EE_AuthorisationEstablishmentID.IsEmpty)
					{
						oneLine.Append("Establishment: [" + process.EE_AuthorisationEstablishmentID + "]");
					}

					if (!process.EE_EstablishmentIndicator.IsEmpty)
					{
						oneLine.Append("Establishment Indicator: [" + process.EE_EstablishmentIndicator + "]");
					}

					if (!process.EE_Depuration.IsEmpty)
					{
						oneLine.Append("Depuration Date: [" + process.EE_Depuration.ToShortDateString() + "]");
					}

					if (!process.EE_HarvestArea.IsEmpty)
					{
						oneLine.Append("Harvest Area: [" + process.EE_HarvestArea + "]");
					}

					if (!process.EE_StartDate.IsEmpty)
					{
						oneLine.Append("Start Date: [" + process.EE_StartDate.ToShortDateString() + "]");
					}

					if (!process.EE_EndDate.IsEmpty)
					{
						oneLine.Append("End Date: [" + process.EE_EndDate.ToShortDateString() + "]");
					}

					if (!process.EE_InspectionRequestedDate.IsEmpty)
					{
						oneLine.Append("Inspection Requested: [" + process.EE_InspectionRequestedDate.ToShortDateString() + "]");
					}

					if (!process.EE_LeaseNumber.IsEmpty)
					{
						oneLine.Append("Lease Number: [" + process.EE_LeaseNumber + "]");
					}

					if (!process.EE_TreatmentCode.IsEmpty)
					{
						oneLine.Append("Treatment Code: [" + process.EE_TreatmentCode + "]");
					}

					result.Append(oneLine.ToStringWithDelimiterBetweenAppends("      "));
					if (!process.EE_TreatmentInfo.IsEmpty)
					{
						result.Append("Treatment Information:- " + process.EE_TreatmentInfo);
						result.Append("");
					}
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString FormattedContainers
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				ZStringBuilder oneLine;
				foreach (CusContainerInvoiceLinePivot pivot in InvoiceLine.ContainersPivot)
				{
					var container = pivot.Container;
					oneLine = new ZStringBuilder();
					oneLine.Append(string.Format("Container Number: [{0}]", (container != null ? container.CO_ContainerNumber : ZString.Empty)));
					if (!container.CO_Seal.IsEmpty)
					{
						oneLine.Append(string.Format("Seal: [{0}]", container.CO_Seal));
					}

					if (!pivot.C2_NetWeight.IsEmpty)
					{
						oneLine.Append(string.Format("IMA1 Net Weight: [{0}]", pivot.C2_NetWeight));
					}

					if (!pivot.C2_GrossWeight.IsEmpty)
					{
						oneLine.Append(string.Format("IMA1 Gross Weight: [{0}]", pivot.C2_GrossWeight));
					}

					result.Append(oneLine.ToStringWithDelimiterBetweenAppends("      "));
				}
				return result.ToStringWithNewLineBetweenAppends();
			}
		}
		#endregion

		#region QL_NatureOfCommodity
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.NatureOfCommodity))]
		public override ZString QL_NatureOfCommodity
		{
			get { return base.QL_NatureOfCommodity; }
		}
		#endregion

		#region QL_TreatmentType
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.TreatmentType))]
		public override ZString QL_TreatmentType
		{
			get { return base.QL_TreatmentType; }
		}
		#endregion

		#region QL_DominantProduct
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.DominantProducts))]
		public ZString QL_DominantProduct
		{
			get { return InvoiceLine.AddInfo.ZA_AQISDominantProduct_Hidden; }
			set
			{
				if (InvoiceLine.AddInfo.ZA_AQISDominantProduct_Hidden != value)
				{
					InvoiceLine.AddInfo.ZA_AQISDominantProduct_Hidden = value;
				}
			}
		}
		public ZPropertyInfo QL_DominantProductInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_DominantProduct, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_AQISDominantProduct_HiddenInfo)); }
		}
		#endregion

		#region QL_AdditionalProducts
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.DominantProducts))]
		public ZString QL_AdditionalProducts
		{
			get { return InvoiceLine.AddInfo.ZA_AQISAdditionalProducts_Hidden; }
			set
			{
				if (InvoiceLine.AddInfo.ZA_AQISAdditionalProducts_Hidden != value)
				{
					InvoiceLine.AddInfo.ZA_AQISAdditionalProducts_Hidden = value;
				}
			}
		}
		public ZPropertyInfo QL_AdditionalProductsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_AdditionalProducts, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_AQISAdditionalProducts_HiddenInfo)); }
		}
		#endregion

		#region QL_HalalProductIndicator
		public ZBool QL_HalalProductIndicator
		{
			get { return (InvoiceLine.AddInfo.ZA_AQISHalal_Hidden == "Y"); }
			set
			{
				if (InvoiceLine.AddInfo.ZA_AQISHalal_Hidden == "Y" ^ value)
				{
					InvoiceLine.AddInfo.ZA_AQISHalal_Hidden = value ? "Y" : "";
				}
			}
		}
		public ZPropertyInfo QL_HalalProductIndicatorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_HalalProductIndicator, x => (InvoiceLine == null || InvoiceLine.AddInfo == null ? null : InvoiceLine.AddInfo.ZA_AQISHalal_HiddenInfo)); }
		}
		#endregion

		#region QL_NetQuantityUnit
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.NetQuantityUnits))]
		public override ZString QL_NetQuantityUnit { get => base.QL_NetQuantityUnit; set => base.QL_NetQuantityUnit = value; }

		#endregion

		#region QL_FinalConsumer
		[ResourceStringData("3A25CFA7-BAE2-4B07-8B29-9D62D62E56C2", Caption = "Final Consumer (for EU)")]
		public override ZBool QL_FinalConsumer { get => base.QL_FinalConsumer; set => base.QL_FinalConsumer = value; }
		#endregion

		#region QL_CombinedNomenclature

		[BusinessObjectTestExclude]
		[MaxLength(15)]
		[ResourceStringData("43D6890C-574A-4669-BD1D-015CF234A51B", Caption = "Combined Nomenclature (for EU)")]
		public ZString QL_FormattedCombinedNomenclature
		{
			get { return EUTariffFormatter.DisplayFormat(QL_CombinedNomenclature); }
			set { QL_CombinedNomenclature = value.KeepNumericCharacters().Left(8); }
		}

		public ZPropertyInfo QL_FormattedCombinedNomenclatureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.QL_FormattedCombinedNomenclature, x => QL_CombinedNomenclatureInfo); }
		}

		public ITariffFormatter EUTariffFormatter => euTariffFormatter ?? (euTariffFormatter = TariffFormatterDecider.GetByCountryCode(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN));
		ITariffFormatter euTariffFormatter;

		#endregion

		#region QL_ProductPart
		[List(nameof(Lookups) + "." + nameof(CommonQuarantineExDocLineLookups.ProductPart))]
		[ResourceStringData("4F51C858-6E27-4652-8131-46BF65310098", Caption = "Product Part")]
		public override ZString QL_ProductPart { get => base.QL_ProductPart; set => base.QL_ProductPart = value; }

		#endregion

		#region ProductCondtions

		[ChildEditable(true)]
		public ProductConditionCollection ProductConditions
		{
			get
			{
				if (fProductConditions == null)
				{
					fProductConditions = new ProductConditionCollection(this);
					fProductConditions.Load();
					fProductConditions.Sort(CusCodeDataSchema.Constants.CY_Order);
					RegisterEditableChildObject(fProductConditions);
				}
				return fProductConditions;
			}
		}
		ProductConditionCollection fProductConditions;

		#endregion

		#region Amend Permisssion Matrix processing

		protected bool QL_CutCode_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.CutCode); }
		}

		protected bool QL_GrowerNumber_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.GrowerNumber); }
		}

		protected bool QL_HalalProductIndicator_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.HalalProductIndicator); }
		}

		protected bool QL_NatureOfCommodity_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.NatureOfCommodity); }
		}

		protected bool QL_TreatmentType_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.TreatmentType); }
		}

		protected bool QL_UngradedProductIndicator_ReadOnly
		{
			get { return !QuarantineExDocHeader.IsChangeAllowed(EXDOCDataFields.UngradedProductIndicator); }
		}

		#endregion

		public new CommonQuarantineExDocLineLookups Lookups => (CommonQuarantineExDocLineLookups)base.Lookups;

		protected override QuarantineExDocLineLookups GetNewLookups()
		{
			if (QuarantineExDocHeader?.IsNEXDOCSActive ?? false)
			{
				return new NEXDOCSQuarantineExDocLineLookups(this);
			}
			else
			{
				return new EXDOCSQuarantineExDocLineLookups(this);
			}
		}

		protected override bool IsLookupsCachedInBase => CalculateIsLookupsCached();

		public void ResetIsLookupsCached()
		{
			isLookupsCached = null;
		}

		bool CalculateIsLookupsCached()
		{
			var result = false;
			if (isLookupsCached.HasValue)
			{
				result = isLookupsCached.Value;
			}
			isLookupsCached = true;
			return result;
		}
		bool? isLookupsCached;

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)QL_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(Customs.Business.BaseJobComInvoiceLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)QL_JIInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(QuarantineExDocEstablishmentAndTime), QuarantineExDocEstablishmentAndTimeSchema.EE_QL);
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ CusCodeDataTypeList.Codes.EXDOCProductCondition, typeof(ProductCondition) }
			};
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion
	}
}
