using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IAggregatedAddInfo
	{
		ZDateTime DateOfValuation { get; }
		ZDateTime EffectiveDutyDate { get; }
		BusinessObjectFactory Factory { get; }
		bool HasChanges { get; set; }
		ZString AggregatedZA_ORG { get; }
		ZString AggregatedZA_PRF { get; }
		IZType AggregatedValue(string propertyName);
		bool IsCopying { get; }
		bool ReadOnly { get; }
		bool IsDeleted { get; }
		void MarkAsNeedingValidation();
		bool LightValidationIsValid { get; }
		bool LightValidationEnabled { get; }
		bool IsMarkingAsNeedingValidationSuspended { get; }
		IDisposable SuspendMarkingAsNeedingValidation();
		IDisposable SuspendSettingHasChanges();
		bool IsInDatabase { get; }
	}

	public interface IAddInfo : IAggregatedAddInfo
	{
		AUAddInfo AddInfo { get; }
	}

	public interface ITariffNumberProvider
	{
		ZString TariffAndStatNumber { get; }
	}

	[SingleObjectAroundARow(), TestedAsNonPersistentBusinessObject]
	public class AUAddInfo : SerialisableAUAddInfo, IAggregatedAddInfo, ICurrencyConverterDataProvider, Customs.Business.IAddInfo
	{
		#region Constants

		public const char InstrumentSeparator = ':';

		public new class Schema : AutoAUAddInfo.Schema
		{
			public const string Prefix = "ZA";
			public const string AdjustmentCurrency_Hidden = "AdjustmentCurrency_Hidden";
			public const string AdjustmentAmount_Hidden = "AdjustmentAmount_Hidden";
			public const string AdjustmentDollarPercentage_Hidden = "AdjustmentDollarPercentage_Hidden";
			public const string AddInfoLine = "AddInfoLine";
			public const string TCI_InstrumentType = "TCI_InstrumentType";
			public const string TCI_InstrumentNo = "TCI_InstrumentNo";
			public const string PRI_InstrumentType = "PRI_InstrumentType";
			public const string PRI_InstrumentNo = "PRI_InstrumentNo";
			public const string TI2_InstrumentType = "TI2_InstrumentType";
			public const string TI2_InstrumentNo = "TI2_InstrumentNo";
			public const string UseBondedWarehouseAutomation = "UseBondedWarehouseAutomation";
			public const string WarehouseOrgFK = "WarehouseOrgFK";

			public const int TCI_InstrumentTypeMaxLength = 3;
			public const int PRI_InstrumentTypeMaxLength = 3;
			public const int TI2_InstrumentTypeMaxLength = 3;
			public const int TCI_InstrumentNoMaxLength = 8;
			public const int PRI_InstrumentNoMaxLength = 8;
			public const int TI2_InstrumentNoMaxLength = 8;
		}

		public const string GeneralPreferenceRate = "GEN";

		#endregion

		#region Constructors
		/// <summary>
		/// Constructor for parented AddInfos
		/// </summary>
		/// <param name="parent"></param>
		public AUAddInfo(IAggregatedAddInfo parent)
			: base(parent.Factory)
		{
			using (SuspendSettingHasChanges())
			{
				Parent = parent;
				isInitialised = true;
			}
		}

		/// <summary>
		/// Preferred constructor - Allows automatic updating of associated AddInfo Text property
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="addInfoProperty"></param>
		public AUAddInfo(IAggregatedAddInfo parent, ZPropertyInfo addInfoProperty)
			: this(parent)
		{
			using (SuspendSettingHasChanges())
			{
				isInitialised = false;
				AddInfoProperty = addInfoProperty;

				using (GetValidationSuspender())
				using (parent.SuspendSettingHasChanges())
				{
					LoadPropertiesFromString((ZString)addInfoProperty.Value, false);
				}
				isInitialised = true;
			}
		}

		public readonly IAggregatedAddInfo Parent;
		protected readonly ZPropertyInfo AddInfoProperty;
		readonly bool isInitialised;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			ZA_AUState_HiddenInfo.HumanReadableName = "AU State";
			ZA_AQISCustomsWt_HiddenInfo.HumanReadableName = "Customs Weight";
		}

		#endregion

		#region Hooks to allow GUI to work until Matt figures out what is wrong with Lookups/AddInfo combination
		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public CodeDescriptionPairList ZA_DCX_List
		{
			get { return Lookups.ZA_DCX_List; }
		}

		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public CodeDescriptionPairList ZA_DRC_List
		{
			get { return Lookups.ZA_DRC_List; }
		}

		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public RefCountryCollection ZA_ORG_List
		{
			get { return Lookups.ZA_ORG_List; }
		}

		public RefCountryCollection CountryCode_List
		{
			get { return Lookups.CountryCode_List; }
		}

		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public CodeDescriptionPairList ZA_PRFList
		{
			get { return Lookups.ZA_PRFList; }
		}

		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public CodeDescriptionPairList ZA_WETE_List
		{
			get { return Lookups.ZA_WETE_List; }
		}

		//		[Obsolete("Move to lookups once they work for AddInfo")]
		public CodeDescriptionPairList TreatmentCodeList
		{
			get { return Lookups.TreatmentCodeList; }
		}

		public CodeDescriptionPairList ZA_ValuationBasis_Hidden_List
		{
			get { return Lookups.ZA_ValuationBasis_Hidden_List; }
		}

		public override ZString ZA_InstrumentCode_Hidden
		{
			get { return base.ZA_InstrumentCode_Hidden; }
			set
			{
				bool hasChanged = ZA_InstrumentCode_Hidden != value;
				base.ZA_InstrumentCode_Hidden = value;
				if (hasChanged && InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		public override ZString ZA_InstrumentType_Hidden
		{
			set
			{
				if (isInitialised)
				{
					JobDeclaration declaration = JobDeclaration;
					if (declaration != null && declaration.IsImport)
					{
						if (IsImportCMR)
						{
							value = CMRInstrumentTypeList.GetInstrumentTypeMappedFromLegacy(value);
						}
						else
						{
							value = CMRInstrumentTypeList.GetInstrumentTypeMappedFromCMR(value);
						}
					}
				}
				base.ZA_InstrumentType_Hidden = value;
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			// Do not call base - Non persistent object
		}
#endif

		protected override void MarkAsNeedingValidationCore()
		{
			if (Parent.LightValidationEnabled &&
				Parent.LightValidationIsValid &&
				!Parent.IsMarkingAsNeedingValidationSuspended)
			{
				Parent.MarkAsNeedingValidation();
			}
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			AUAddInfoValidation result = null;

			if (LineLevelAddInfo && !PartAddInfo)
			{
				if (IsImportCMR)
				{
					if (IsSACWithoutLines)
					{
						result = new SACWithoutLinesAddInfoLineValidation(this);
					}
					else
					{
						result = new CMRAddInfoInvLineValidation(this);
					}
				}
				else if (IsQuarantine)
				{
					result = new EXDOCAddInfoJobComInvoicelineValidation(this);
				}
				else if (IsDrawback)
				{
					result = new DrawbackAddinfoInvLineValidation(this);
				}
				else
				{
					result = new AUAddInfoLineValidation(this);
				}
			}
			else if (HeaderLevelAddInfo)
			{
				if (IsImportCMR)
				{
					result = new CMRAddInfoInvHeaderValidation(this);
				}
				else if (IsDrawback)
				{
					result = new DrawbackAddinfoInvHeaderValidation(this);
				}
				else
				{
					result = new AUAddInfoHeaderValidation(this);
				}
			}
			else if (DeclarationLevelAddInfo)
			{
				if (IsImportCMR)
				{
					result = new CMRAddInfoDeclarationValidation(this);
				}
				else if (IsDrawback)
				{
					result = new DrawbackAddInfoDeclarationValidation(this);
				}
				else if (IsExport)
				{
					result = new EXDAddInfoDeclarationValidation(this);
				}
				else
				{
					result = new AUAddInfoValidation(this);
				}
			}
			else
			{
				result = new AUAddInfoValidation(this);
			}
			return result;
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZBool UseBondedWarehouseAutomation
		{
			get { return ZA_UseBondedWarehouseAutomation_Hidden == new ZString("Y"); }
			set
			{
				ZA_UseBondedWarehouseAutomation_Hidden = value ? new ZString("Y") : ZString.Empty;
				UseBondedWarehouseAutomationInfo.RefreshBinding();
			}
		}

		public bool ZA_DrawbackID_ReadOnly
		{
			get { return !HeaderLevelAddInfo; }
		}

		public override ZString ZA_WRN
		{
			get
			{
				return base.ZA_WRN;
			}
			set
			{
				EntryLineCodeParser parser = new EntryLineCodeParser(value);
				if (isInitialised && parser.IsCompleteCode)
				{
					base.ZA_WRN = parser.EntryNumber;
					ZA_WRL = parser.LineNumber;
				}
				else
				{
					base.ZA_WRN = value;
				}
				if (isInitialised && InvoiceLine != null)
				{
					InvoiceLine.MarkAsNeedingValidation();
					if (InvoiceLine.InvoiceHeader != null)
					{
						InvoiceLine.InvoiceHeader.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZInt ZA_WRL
		{
			get { return base.ZA_WRL; }
			set
			{
				var oldValue = ZA_WRL;
				base.ZA_WRL = value;
				if (!IsCopying && oldValue != ZA_WRL && IsImport)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						invoiceLine.MarkAsNeedingValidation();
					}
					else
					{
						var declaration = Parent as JobDeclaration;
						if (declaration != null)
						{
							declaration.InvoiceLines.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public ZPropertyInfo UseBondedWarehouseAutomationInfo
		{
			get { return GetZPropertyInfo(Schema.UseBondedWarehouseAutomation); }
		}

		#region Aggregated properties

		public ZString AggregatedZA_ORG
		{
			get { return ZA_PRF.IsEmpty && ZA_ORG.IsEmpty && !Parent.IsDeleted ? Parent.AggregatedZA_ORG : ZA_ORG; }
		}

		public ZString AggregatedZA_POC
		{
			get { return InvoiceLine != null ? InvoiceLine.AggregatedZA_POC : ZA_POC; }
		}

		public ZString AggregatedPST
		{
			get { return InvoiceLine != null ? InvoiceLine.AggregatedZA_PST : ZA_PST; }
		}

		public ZString AggregatedPOCFallBackToORG
		{
			get { return AggregatedZA_POC.IsEmpty ? AggregatedZA_ORG : AggregatedZA_POC; }
		}

		public ZString AggregatedZA_PRF
		{
			get { return ZA_PRF.IsEmpty && ZA_ORG.IsEmpty && !Parent.IsDeleted ? Parent.AggregatedZA_PRF : ZA_PRF; }
		}

		public ZString AggregatedZA_ValuationBasis_Hidden
		{
			get { return (ZString)AggregatedValue(AUAddInfo.Schema.ZA_ValuationBasis_Hidden); }
		}

		public ZString AggregatedZA_VALB_Hidden
		{
			get { return (ZString)AggregatedValue(AUAddInfo.Schema.ZA_VALB_Hidden); }
		}

		#endregion

		public void FillEmptyPropertiesFrom(AUAddInfo addInfo)
		{
			foreach (ZPropertyInfo info in ZPropertyInfoHash)
			{
				if (info.IsPersistent && info.Value.IsDefault && !((IZType)addInfo[info.Name]).IsDefault)
				{
					info.Value = (IZType)addInfo[info.Name];
				}
			}
		}

		#region BusinessObject overrides

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = new AUAddInfo(Parent);
			using (result.GetValidationSuspender())
			using (result.SuspendSettingHasChanges())
			{
				result.CopyPersistentValuesFrom(this, args);
			}
			return result;
		}

		public new AUAddInfo Clone()
		{
			return (AUAddInfo)base.Clone();
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges && isInitialised)    // Stops firing on Factory.Save
				{
					UpdateRelatedPropertyInfo();
				}
			}
		}

		protected override void OnElementChanged()
		{
			base.OnElementChanged();
			RefreshParentBizObj();
		}

		protected override void OnElementReset()
		{
			base.OnElementReset();
			RefreshParentBizObj();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!Parent.IsDeleted)
			{
				if (!IsImport)
				{
					foreach (PropertyDescriptor property in GetProperties().IncludingFlattened)
					{
						if (property.Attributes[typeof(ImportAddInfoAttribute)] != null)
						{
							property.SetValue(this, ZString.Empty);
						}
					}
				}

				if ((HasChanges || !Parent.IsInDatabase) && isInitialised)//when HasChanges is suspended
				{
					UpdateRelatedPropertyInfo();
				}
			}
		}

		#endregion

		#region Property overrides

		#region ZA_EDITransmitDate

		[BusinessObjectTestExclude]
		public override ZDateTime ZA_EDITransmitDate
		{
			get => base.ZA_EDITransmitDate;
			set
			{
				if (value != ZA_EDITransmitDate)
				{
					base.ZA_EDITransmitDate = value;
				}
			}
		}

		#endregion

		[BusinessObjectTestExclude]
		public override ZString ZA_CustShipNoOverride_Hidden
		{
			get { return base.ZA_CustShipNoOverride_Hidden; }
			set
			{
				base.ZA_CustShipNoOverride_Hidden = value;
				if (JobDeclaration != null)
				{
					JobDeclaration.MarkAsNeedingValidation();
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_CustShipNo_Hidden
		{
			get { return base.ZA_CustShipNo_Hidden; }
			set
			{
				base.ZA_CustShipNo_Hidden = value;
				if (JobDeclaration != null)
				{
					JobDeclaration.MarkAsNeedingValidation();
				}
			}
		}

		[ImportAddInfo]
		public override ZString ZA_EFTReceiptPrinter_Hidden
		{
			get { return base.ZA_EFTReceiptPrinter_Hidden; }
			set
			{
				base.ZA_EFTReceiptPrinter_Hidden = value;
			}
		}

		[ImportAddInfo]
		public override ZString ZA_ClearanceAdvicePrinter_Hidden
		{
			get { return base.ZA_ClearanceAdvicePrinter_Hidden; }
			set
			{
				base.ZA_ClearanceAdvicePrinter_Hidden = value;
			}
		}

		[ImportAddInfo]
		public override ZString ZA_PrinterNumber_Hidden
		{
			get { return base.ZA_PrinterNumber_Hidden; }
			set
			{
				base.ZA_PrinterNumber_Hidden = value;
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_STD
		{
			get { return base.ZA_STD; }
			set { base.ZA_STD = ZArchitecture.Core.Utilities.Round(value, 2); }
		}

		[BusinessObjectTestExclude]//TODO: Nick: find a better way to fix the failing test casused by too many items in the addinfo
		public override ZString ZA_CPDecDefaultAnswerTrue_Hidden
		{
			get
			{
				return base.ZA_CPDecDefaultAnswerTrue_Hidden;
			}
			set
			{
				base.ZA_CPDecDefaultAnswerTrue_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TRN
		{
			get { return base.ZA_TRN; }
			set { base.ZA_TRN = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_DXT
		{
			get { return base.ZA_DXT; }
			set { base.ZA_DXT = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_ISC
		{
			get { return base.ZA_ISC; }
			set { base.ZA_ISC = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_WMC
		{
			get { return base.ZA_WMC; }
			set
			{
				var oldValue = ZA_WMC;
				base.ZA_WMC = value;
				if (!IsCopying && oldValue != ZA_WMC && IsImportCMR)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						invoiceLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZDateTime ZA_SettlementPeriodStartDate_Hidden
		{
			get => base.ZA_SettlementPeriodStartDate_Hidden;
			set
			{
				var oldValue = ZA_SettlementPeriodStartDate_Hidden;
				base.ZA_SettlementPeriodStartDate_Hidden = value;
				if (!IsCopying && oldValue != ZA_SettlementPeriodStartDate_Hidden)
				{
					UpdateZA_SettlementPeriodEndDate_Hidden();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AUAddInfoLookups.SettlementPeriodTypeList))]
		public override ZString ZA_SettlementPeriodType_Hidden
		{
			get => base.ZA_SettlementPeriodType_Hidden;
			set
			{
				var oldValue = ZA_SettlementPeriodType_Hidden;
				base.ZA_SettlementPeriodType_Hidden = value;
				if (!IsCopying && oldValue != ZA_SettlementPeriodType_Hidden)
				{
					UpdateZA_SettlementPeriodEndDate_Hidden();
					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		void UpdateZA_SettlementPeriodEndDate_Hidden()
		{
			var startDate = ZA_SettlementPeriodStartDate_Hidden;
			if (startDate.IsValid)
			{
				switch (ZA_SettlementPeriodType_Hidden)
				{
					case SettlementPeriodTypeList.Codes.Weekly:
					case SettlementPeriodTypeList.Codes.WeeklyLegacy:
						ZA_SettlementPeriodEndDate_Hidden = startDate.AddDays(6);
						break;
					case SettlementPeriodTypeList.Codes.Monthly:
						ZA_SettlementPeriodEndDate_Hidden = new ZDateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
						break;
					case SettlementPeriodTypeList.Codes.Quarterly:
						ZA_SettlementPeriodEndDate_Hidden = new ZDateTime(startDate.Year, startDate.Month, 1).AddMonths(3).AddDays(-1);
						break;
				}
			}
		}

		[ReadOnlyMember(nameof(ZA_SettlementPeriodEndDate_Hidden_ReadOnly))]
		public override ZDateTime ZA_SettlementPeriodEndDate_Hidden
		{
			get => base.ZA_SettlementPeriodEndDate_Hidden;
			set => base.ZA_SettlementPeriodEndDate_Hidden = value;
		}

		protected bool ZA_SettlementPeriodEndDate_Hidden_ReadOnly => !ZA_SettlementPeriodType_Hidden.IsEmpty;

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_LCP
		{
			get { return base.ZA_LCP; }
			set { base.ZA_LCP = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_ELA
		{
			get { return base.ZA_ELA; }
			set { base.ZA_ELA = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_FOD
		{
			get { return base.ZA_FOD; }
			set
			{
				if (ZA_FOD != value)
				{
					FOD = Get_ddMMyy_FormattedDateFromString(value);
					base.ZA_FOD = value;
				}
			}
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_PRI
		{
			get { return base.ZA_PRI; }
			set { base.ZA_PRI = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TCI
		{
			get { return base.ZA_TCI; }
			set { base.ZA_TCI = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_CL2
		{
			get { return base.ZA_CL2; }
			set { base.ZA_CL2 = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TR2
		{
			get { return base.ZA_TR2; }
			set { base.ZA_TR2 = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		public override ZString ZA_TI2
		{
			get { return base.ZA_TI2; }
			set { base.ZA_TI2 = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_VID
		{
			get { return base.ZA_VID; }
			set { base.ZA_VID = value; }
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		public override ZString ZA_AQISInspectLocation_Hidden
		{
			get { return base.ZA_AQISInspectLocation_Hidden; }
			set
			{
				base.ZA_AQISInspectLocation_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]//TODO: Deb: find a better way to fix the failing test casused by too many items in the addinfo
		public override ZString ZA_AQISConcern_Hidden
		{
			get { return base.ZA_AQISConcern_Hidden; }
			set
			{
				base.ZA_AQISConcern_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISPremIdProcessType_Hidden
		{
			get { return base.ZA_AQISPremIdProcessType_Hidden; }
			set
			{
				base.ZA_AQISPremIdProcessType_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISDocuments_Hidden
		{
			get { return base.ZA_AQISDocuments_Hidden; }
			set
			{
				base.ZA_AQISDocuments_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISPackageType_Hidden
		{
			get { return base.ZA_AQISPackageType_Hidden; }
			set
			{
				base.ZA_AQISPackageType_Hidden = value;
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_IsAQISCertificateRequest_Hidden
		{
			get { return base.ZA_IsAQISCertificateRequest_Hidden; }
			set
			{
				bool hasChanged = ZA_IsAQISCertificateRequest_Hidden != value;
				base.ZA_IsAQISCertificateRequest_Hidden = value;
				if (hasChanged && JobDeclaration != null)
				{
					JobDeclaration.MarkAsNeedingValidation();
					JobDeclaration.Invoices.MarkAsNeedingValidation();
					JobDeclaration.InvoiceLines.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsImporterNotMiscOrg))]
		public override ZString ZA_ConsigneeNameHidden
		{
			get { return base.ZA_ConsigneeNameHidden; }
		}

		[ReadOnlyMember(nameof(IsImporterNotMiscOrg))]
		public override ZString ZA_ConsigneeCityHidden
		{
			get { return base.ZA_ConsigneeCityHidden; }
		}

		[ReadOnlyMember(nameof(IsSupplierNotMiscOrg))]
		public override ZString ZA_GoodsOwnerPartyIDHidden
		{
			get { return base.ZA_GoodsOwnerPartyIDHidden; }
		}

		protected bool IsImporterNotMiscOrg
		{
			get
			{
				return !DeclarationLevelAddInfo ||
					(!JobDeclaration.IsDeleted && (JobDeclaration.JE_OH_ImporterInfo.ReadOnly
						|| JobDeclaration.JE_OH_Importer != OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation));
			}
		}

		protected bool IsSupplierNotMiscOrg
		{
			get
			{
				return !DeclarationLevelAddInfo ||
					(!JobDeclaration.IsDeleted && (JobDeclaration.JE_OH_SupplierInfo.ReadOnly
						|| JobDeclaration.JE_OH_Supplier != OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation));
			}
		}

		#endregion

		#region Overrides

		public override bool ReadOnly
		{
			get { return Parent != null && Parent.ReadOnly; }
		}

		protected override bool IsCopying
		{
			get { return base.IsCopying || Parent.IsCopying; }
		}

		bool settingAddInfoProperty;

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_WET
		{
			get { return base.ZA_WET; }
			//TODO: Push this to the property info
			set { base.ZA_WET = decimal.Round(value, 2); }
		}

		bool isSynchronisingInvoiceQtyAndWRU;
		public bool ShouldSynchroniseInvoiceQtyAndWRU
		{
			get
			{
				return !isSynchronisingInvoiceQtyAndWRU && !IsCopying && isInitialised &&
					InvoiceLine != null && JobDeclaration != null &&
					(JobDeclaration.IsExWarehouse || InvoiceLine.JI_IsPackToBondForLine)
					&& InvoiceLine.JI_CustomsUnitQty.IsEmpty && InvoiceLine.JI_CustomsQuantity == 0m
					&& (ZA_WRU.IsEmpty || ZA_WRU == InvoiceLine.JI_InvoiceUQ)
					&& (ZA_WRQ != InvoiceLine.JI_InvoiceQuantity || (ZA_WRQ == 0 && InvoiceLine.JI_InvoiceQuantity == 0));
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_WRQ
		{
			set
			{
				base.ZA_WRQ = value;
				SynchroniseInvoiceQtyAndWRUIfRequired(SyncDirection.FromAddInfoToInvoiceLine);
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_WRU
		{
			set
			{
				base.ZA_WRU = value;
				SynchroniseInvoiceQtyAndWRUIfRequired(SyncDirection.FromAddInfoToInvoiceLine);
			}
		}

		public override ZString ZA_LinePrefix_Hidden
		{
			get { return base.ZA_LinePrefix_Hidden; }
			set
			{
				base.ZA_LinePrefix_Hidden = value;
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		protected bool IsGeneralPreferenceRate
		{
			get { return ZA_PST == AUAddInfo.GeneralPreferenceRate; }
		}

		[ReadOnlyMember(nameof(IsGeneralPreferenceRate))]
		public override ZString ZA_POC
		{
			get { return base.ZA_POC; }
			set
			{
				base.ZA_POC = value.ToUpper();
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsGeneralPreferenceRate))]
		public override ZString ZA_PRT
		{
			get { return base.ZA_PRT; }
			set
			{
				base.ZA_PRT = value.ToUpper();
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_RNO
		{
			get { return base.ZA_RNO; }
			set
			{
				base.ZA_RNO = value;
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_LCTI
		{
			get { return base.ZA_LCTI; }
			set
			{
				base.ZA_LCTI = value;
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		void MarkInvoiceLineAsNeedingValidation()
		{
			if (!InvoiceLine.IsMarkingAsNeedingValidationSuspended)
			{
				InvoiceLine.IsMarkAsNeedingValidationCalledForAddInfo = true;
				try
				{
					InvoiceLine.MarkAsNeedingValidation();
				}
				finally
				{
					InvoiceLine.IsMarkAsNeedingValidationCalledForAddInfo = false;
				}
			}
		}

		public enum SyncDirection { FromInvoiceLineToAddInfo, FromAddInfoToInvoiceLine }

		public void SynchroniseInvoiceQtyAndWRUIfRequired(SyncDirection direction)
		{
			if (ShouldSynchroniseInvoiceQtyAndWRU)
			{
				isSynchronisingInvoiceQtyAndWRU = true;

				if (direction == SyncDirection.FromAddInfoToInvoiceLine)
				{
					if (!ZA_WRU.IsEmpty && ZA_WRQ > 0)
					{
						InvoiceLine.JI_InvoiceQuantity = ZA_WRQ;
						InvoiceLine.JI_InvoiceUQ = ZA_WRU;
					}
				}
				else
				{
					ZA_WRQ = InvoiceLine.JI_InvoiceQuantity;
					ZA_WRU = InvoiceLine.JI_InvoiceUQ;
				}

				isSynchronisingInvoiceQtyAndWRU = false;
			}
		}

		public void ClearWarehouseRelatedProperties()
		{
			ZA_WRU = ZString.Empty;
			ZA_WRQ = ZDecimal.Zero;
			ZA_WUV = ZDecimal.Zero;
		}

		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_ISS
		{
			get { return base.ZA_ISS; }
			set
			{
				value = ZArchitecture.Core.Utilities.Round(value, 2);
				if (base.ZA_ISS != value)
				{
					base.ZA_ISS = value;
					CalculateLitresOfAlcoholFromQT2();
				}
			}
		}

		bool isSettingQT2;
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_QT2
		{
			set
			{
				value = ZArchitecture.Core.Utilities.Round(value, 2);
				if (base.ZA_QT2 != value)
				{
					if (!isSettingQT2)
					{
						isSettingQT2 = true;
						try
						{
							base.ZA_QT2 = value;
							if (!IsCopying)
							{
								CalculateLitresOfAlcoholFromQT2();
								if (InvoiceLine != null && isInitialised)
								{
									InvoiceLine.CalculateLineWeightOrQuantityFromCustomsQty();
								}
							}
						}
						finally
						{
							isSettingQT2 = false;
						}
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[BusinessObjectTestExclude]
		public override ZString ZA_ADJ
		{
			get { return base.ZA_ADJ; }
			set
			{
				base.ZA_ADJ = value.Right(ZA_ADJInfo.MaxLength);

				if (!IsCopying)
				{
					if (isInitialised && InvoiceLine != null && InvoiceLine.Declaration != null)
					{
						InvoiceLine.Declaration.MarkApportionmentDirty();
					}

					if (!value.IsEmpty && !IsValidationSuspended)
					{
						if (Parent != null && !AggregatedZA_ValuationBasis_Hidden.IsEmpty)
						{
							if (ZA_ValuationBasis_Hidden != AggregatedZA_ValuationBasis_Hidden)
							{
								ZA_ValuationBasis_Hidden = AggregatedZA_ValuationBasis_Hidden;
							}
						}

						if (AdjustmentDollarPercentage_Hidden == "$" && AdjustmentCurrency_Hidden.IsEmpty && JobDeclaration != null)
						{
							ZString currencyString = "";
							JobComInvoiceHeader header = null;
							if (Parent is JobComInvoiceLine)
							{
								JobComInvoiceLine line = (JobComInvoiceLine)Parent;
								header = line.Master as JobComInvoiceHeader;
							}
							else if (Parent is JobComInvoiceHeader)
							{
								header = (JobComInvoiceHeader)Parent;
							}
							if (header != null && header.Invoice_Currency != null)
							{
								currencyString = header.Invoice_Currency.RX_Code;
								if (!currencyString.IsEmpty)
								{
									ZString tempValue = value + currencyString;
									base.ZA_ADJ = tempValue.Right(ZA_ADJInfo.MaxLength);
								}
							}
						}
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString ZA_TILV
		{
			get { return base.ZA_TILV; }
			set
			{
				ZString valueToSet = value;
				bool isDifferent = ZA_TILV != valueToSet;

				if (isDifferent)
				{
					ZDecimal tILVAmount = GetTILVAmount(valueToSet);
					RefCurrency tILVCurrencyCached = GetTILVCurrency(valueToSet);

					if (tILVAmount > 0 && tILVCurrencyCached == null)
					{
						valueToSet = tILVAmount.Round(2).ToString(2) + JobDeclaration.LocalCurrencyConstantCode;
					}
					else if (tILVCurrencyCached != null)
					{
						valueToSet = tILVAmount.Round(2).ToString(2) + tILVCurrencyCached.RX_Code;
					}
				}

				base.ZA_TILV = valueToSet;

				if (isDifferent && isInitialised)
				{
					if (InvoiceLine != null || InvoiceHeader != null)
					{
						if (!valueToSet.IsEmpty)
						{
							ZA_CalcTILV_Hidden = ZString.Empty;
						}

						JobDeclaration declaration = JobDeclaration;
						if (declaration != null)
						{
							declaration.MarkApportionmentDirty();
						}

						if (InvoiceHeader != null)
						{
							InvoiceHeader.MarkAsNeedingValidation();
						}

						JobComInvoiceLine invoiceLine = this.InvoiceLine;
						if (invoiceLine != null)
						{
							invoiceLine.MarkAsNeedingValidation();

							if (invoiceLine.InvoiceHeader != null)
							{
								invoiceLine.InvoiceHeader.MarkAsNeedingValidation();
							}
						}
					}
				}
			}
		}

		public override ZString ZA_CalcTILV_Hidden
		{
			get { return base.ZA_CalcTILV_Hidden; }
			set
			{
				bool isDifferent = ZA_CalcTILV_Hidden != value;

				base.ZA_CalcTILV_Hidden = value;

				if (isDifferent && isInitialised)
				{
					JobComInvoiceLine invoiceLine = this.InvoiceLine;
					if (invoiceLine != null && invoiceLine.InvoiceHeader != null)
					{
						invoiceLine.InvoiceHeader.MarkAsNeedingValidation();
						invoiceLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		public ZString EffectiveTILVString
		{
			get { return !ZA_TILV.IsEmpty ? ZA_TILV : ZA_CalcTILV_Hidden; }
		}

		public Money TILVMoney
		{
			get
			{
				Money result = Money.Empty;

				ZString tilvString = EffectiveTILVString;

				if (!tilvString.IsEmpty)
				{
					RefCurrency currency = GetTILVCurrency(tilvString);

					if (currency != null)
					{
						ZDecimal amount = GetTILVAmount(tilvString);
						result = new Money(amount, currency);
					}
				}
				return result;
			}
		}

		protected ZDecimal GetTILVAmount(ZString value)
		{
			var amountString = value.KeepChars("0123456789.-");
			return !amountString.IsEmpty && ZDecimal.TryParse(amountString, out var amount) ? amount : ZDecimal.Zero;
		}

		protected internal RefCurrency GetTILVCurrency(ZString value)
		{
			ZString currencyString = value.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			RefCurrency result = null;
			if (!currencyString.IsEmpty)
			{
				result = RefCurrency.LoadFromCurrencyCode(Factory, currencyString);
			}
			return result;
		}

		public ZDecimal TILVInAUD
		{
			get
			{
				Money result = Money.Empty;

				Money tilvMoney = TILVMoney;

				if (tilvMoney.Currency != null)
				{
					result = CurrencyConverter.ConvertExact(tilvMoney, JobDeclaration.GetLocalCurrency());
				}
				return result.Amount;
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool ZA_UPEIndicator_Hidden
		{
			get { return base.ZA_UPEIndicator_Hidden; }
			set
			{
				var oldValue = ZA_UPEIndicator_Hidden;

				base.ZA_UPEIndicator_Hidden = value;

				var declaration = JobDeclaration;

				if (isInitialised && !IsCopying && declaration != null)
				{
					if (!oldValue && ZA_UPEIndicator_Hidden && declaration.IsImportCMR && !declaration.IsNonTransportDeclarationType)
					{
						RemoveInvalidQuestions(declaration);
					}

					var invoices = declaration.Invoices;
					if (invoices != null)
					{
						foreach (JobComInvoiceHeader header in invoices)
						{
							header.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		void RemoveInvalidQuestions(JobDeclaration declaration)
		{
			var questionsList = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Select(c => c.Questions);

			if (questionsList != null)
			{
				foreach (var questions in questionsList)
				{
					var invalidQuestions = questions.Cast<CMRCusEntryCPDec>().Where(c => c.ON_CPDecNum == 3 || c.ON_CPDecNum == 375).ToArray();

					foreach (var invalidQuestion in invalidQuestions)
					{
						questions.RemoveAndDelete(invalidQuestion);
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZBool ZA_SOFAIndicator_Hidden
		{
			get { return base.ZA_SOFAIndicator_Hidden; }
			set
			{
				base.ZA_SOFAIndicator_Hidden = value;

				if (isInitialised && !IsCopying && JobDeclaration != null)
				{
					InvoiceHeaderActiveCollection invoices = JobDeclaration.Invoices;
					if (invoices != null)
					{
						foreach (JobComInvoiceHeader header in invoices)
						{
							header.MarkAsNeedingValidation();
							header.InvoiceLines.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		public override ZString ZA_IsPackToBondForLine_Hidden
		{
			get { return base.ZA_IsPackToBondForLine_Hidden; }
			set
			{
				base.ZA_IsPackToBondForLine_Hidden = value;

				if (isInitialised && !IsCopying && JobDeclaration != null)
				{
					InvoiceHeaderActiveCollection invoices = JobDeclaration.Invoices;
					if (invoices != null)
					{
						foreach (JobComInvoiceHeader header in invoices)
						{
							header.InvoiceLines.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		protected internal CurrencyConverter CurrencyConverter
		{
			get
			{
				if (fCurrencyConverter == null || !fCurrencyConverter.IsConverterValid)
				{
					if (Parent is ICurrencyConverterProvider)
					{
						fCurrencyConverter = ((ICurrencyConverterProvider)Parent).CurrencyConverter;
					}
					else
					{
						fCurrencyConverter = new CurrencyConverterWithDataProvider(Factory, this);
					}
				}
				return fCurrencyConverter;
			}
		}
		CurrencyConverter fCurrencyConverter;

		public override ZString ZA_PST
		{
			get { return base.ZA_PST; }
			set
			{
				ZString valueToUpper = value.ToUpper();
				bool hasChanged = base.ZA_PST != valueToUpper;
				base.ZA_PST = valueToUpper;
				if (hasChanged && isInitialised && !IsCopying)
				{
					if (IsGeneralRate)
					{
						ZA_POC = "";
						ZA_PRT = "";
					}
					DefaultTariffRateNumber();

					if (InvoiceHeader != null)
					{
						InvoiceHeader.JobComInvoiceLines.MarkAsNeedingValidation();
					}
					if (InvoiceLine != null)
					{
						MarkInvoiceLineAsNeedingValidation();
					}
				}
			}
		}

		public override ZString ZA_PRF
		{
			get { return base.ZA_PRF; }
			set
			{
				bool hasChanged = base.ZA_PRF != value;
				base.ZA_PRF = value;
				if (hasChanged && isInitialised && !IsCopying)
				{
					if (GroupInvoice != null)
					{
						GroupInvoice.AllJobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString ZA_ORG
		{
			get { return base.ZA_ORG; }
			set
			{
				ZString valueToUpper = value.ToUpper();
				bool hasChanged = base.ZA_ORG != valueToUpper;
				base.ZA_ORG = valueToUpper;
				if (hasChanged && isInitialised && !IsCopying)
				{
					if (GroupInvoice != null)
					{
						GroupInvoice.AllJobComInvoiceLines.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString ZA_DDN_Hidden
		{
			get { return base.ZA_DDN_Hidden; }
			set
			{
				value = value.TrimEnd(' ');
				if (base.ZA_DDN_Hidden != value && IsDrawback && isInitialised && (Parent is JobComInvoiceLine invoiceLine))
				{
					invoiceLine.RefreshDrawbackImportEntryLine();
					EntryLineCodeParser parser = new EntryLineCodeParser(value);
					if (parser.IsCompleteCode)
					{
						base.ZA_DDN_Hidden = parser.EntryNumber;
						ZA_DDL_Hidden = parser.LineNumber;
					}
					else
					{
						base.ZA_DDN_Hidden = value;
					}
					if (value.IsEmpty)
					{
						invoiceLine.CalculateClaimAmount();
					}
					else
					{
						invoiceLine.DefaultDrawbackDataOnChangeOfImportDeclarationNumber();
					}
				}
				else
				{
					base.ZA_DDN_Hidden = value;
				}
			}
		}

		public override ZInt ZA_DDL_Hidden
		{
			get { return base.ZA_DDL_Hidden; }
			set
			{
				bool hasChanged = value != base.ZA_DDL_Hidden;
				base.ZA_DDL_Hidden = value;
				if (hasChanged && IsDrawback && isInitialised && (Parent is JobComInvoiceLine invoiceLine))
				{
					invoiceLine.RefreshDrawbackImportEntryLine();
					invoiceLine.DefaultDrawbackDataOnChangeOfImportDeclarationNumber();
				}
			}
		}

		protected bool IsDrawbackLineValueNotOverriden
		{
			get { return InvoiceLine != null && !InvoiceLine.IsDrawbackLineValueOverriden; }
		}

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsDrawbackLineValueNotOverriden))]
		public override ZDecimal ZA_DTR_Hidden
		{
			get { return base.ZA_DTR_Hidden; }
		}

		[ReadOnlyMember(nameof(IsDrawbackLineValueNotOverriden))]
		public override ZDecimal ZA_DDT_Hidden
		{
			get { return base.ZA_DDT_Hidden; }
			set
			{
				bool hasChanged = value != base.ZA_DDT_Hidden;
				base.ZA_DDT_Hidden = value;
				if (hasChanged && isInitialised && (Parent is JobComInvoiceLine))
				{
					InvoiceLine.DrawbackDutyAmountOverriden = 0m;
				}
			}
		}

		[ReadOnlyMember(nameof(IsDrawbackLineValueNotOverriden))]
		public override ZDecimal ZA_DCV_Hidden
		{
			get { return base.ZA_DCV_Hidden; }
			set
			{
				bool hasChanged = value != base.ZA_DCV_Hidden;
				base.ZA_DCV_Hidden = value;
				if (hasChanged && isInitialised && (Parent is JobComInvoiceLine))
				{
					InvoiceLine.DrawbackCustomsValueOverriden = 0m;
				}
			}
		}

		public override ZString ZA_DAM_Hidden
		{
			get { return base.ZA_DAM_Hidden; }
			set
			{
				ZString oldValue = base.ZA_DAM_Hidden;
				bool hasChanged = value != oldValue;
				base.ZA_DAM_Hidden = value;
				if (hasChanged && isInitialised && !value.IsEmpty)
				{
					if (DeclarationLevelAddInfo && JobDeclaration != null)
					{
						foreach (JobComInvoiceHeader drawbackInvoiceHeader in JobDeclaration.Invoices)
						{
							if (drawbackInvoiceHeader.AddInfo.ZA_DAM_Hidden.IsEmpty || drawbackInvoiceHeader.AddInfo.ZA_DAM_Hidden == oldValue)
							{
								drawbackInvoiceHeader.AddInfo.ZA_DAM_Hidden = value;
							}
						}
					}
					else if (HeaderLevelAddInfo && InvoiceHeader != null)
					{
						foreach (JobComInvoiceLine drawbackLine in InvoiceHeader.JobComInvoiceLines)
						{
							if (drawbackLine.AddInfo.ZA_DAM_Hidden.IsEmpty || drawbackLine.AddInfo.ZA_DAM_Hidden == oldValue)
							{
								drawbackLine.AddInfo.ZA_DAM_Hidden = value;
							}
						}
					}
					else if (Parent is JobComInvoiceLine && InvoiceLine != null)
					{
						InvoiceLine.CalculateClaimAmount();
					}
				}
			}
		}

		public override ZString ZA_EDN_Hidden
		{
			get { return base.ZA_EDN_Hidden; }
			set
			{
				var oldValue = base.ZA_EDN_Hidden;
				bool hasChanged = value != oldValue;
				base.ZA_EDN_Hidden = value;

				if (hasChanged && isInitialised && !value.IsEmpty)
				{
					if (DeclarationLevelAddInfo && JobDeclaration != null)
					{
						foreach (JobComInvoiceHeader invoiceHeader in JobDeclaration.Invoices)
						{
							if (invoiceHeader.AddInfo.ZA_EDN_Hidden.IsEmpty || invoiceHeader.AddInfo.ZA_EDN_Hidden == oldValue)
							{
								invoiceHeader.AddInfo.ZA_EDN_Hidden = value;
							}
						}
					}
					else if (HeaderLevelAddInfo && InvoiceHeader != null)
					{
						foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.JobComInvoiceLines)
						{
							if (invoiceLine.AddInfo.ZA_EDN_Hidden.IsEmpty || invoiceLine.AddInfo.ZA_EDN_Hidden == oldValue)
							{
								invoiceLine.AddInfo.ZA_EDN_Hidden = value;
							}
						}
					}
				}
			}
		}

		public override ZString ZA_DOV_Hidden
		{
			get
			{
				return base.ZA_DOV_Hidden;
			}
			set
			{
				base.ZA_DOV_Hidden = value;
				if (InvoiceLine != null)
				{
					MarkInvoiceLineAsNeedingValidation();
				}
			}
		}

		public override ZString ZA_TreatmentCode_Hidden
		{
			get { return base.ZA_TreatmentCode_Hidden; }
			set
			{
				bool hasChanged = value != base.ZA_TreatmentCode_Hidden;
				base.ZA_TreatmentCode_Hidden = value;
				if (IsDrawback && hasChanged && isInitialised && Parent is JobComInvoiceLine)
				{
					InvoiceLine.CalculateClaimAmountIfImputationMethod();
				}

				if (Parent is JobComInvoiceLine)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AUAddInfoLookups.CMRGSTEList))]
		public override ZString ZA_GSTE
		{
			get { return base.ZA_GSTE; }
			set { base.ZA_GSTE = value; }
		}

		[List(nameof(Lookups) + "." + nameof(AUAddInfoLookups.EstablishmentCodes))]
		public override ZString ZA_WAR { get => base.ZA_WAR; set => base.ZA_WAR = value; }

		#endregion

		public void DefaultTariffRateNumber()
		{
			if (IsImportCMR)
			{
				CodeDescriptionPairList rateNumberList = Lookups.ZA_RNO_List;

				if (rateNumberList != null && rateNumberList.Count == 1)
				{
					ZString rateNumber = rateNumberList[0].Code;
					if (ZA_RNO != rateNumber)
					{
						ZA_RNO = rateNumber;
					}
				}
			}
		}

		#region Date Fields

		[ReadOnlyMember(nameof(LineLevelAddInfo))]
		public override ZString ZA_EFD
		{
			get { return base.ZA_EFD; }
			set
			{
				if (ZA_EFD != value)
				{
					EFD = Get_ddMMyy_FormattedDateFromString(value);
					base.ZA_EFD = value;
					if (InvoiceHeader != null)
					{
						InvoiceHeader.NotifyEffectiveDutyDateDirty();
					}
				}
			}
		}

		public ZDateTime EFD
		{
			get { return fEFD; }
			set { fEFD = value; }
		}
		ZDateTime fEFD;

		public ZDateTime FOD
		{
			get { return fFOD; }
			set { fFOD = value; }
		}
		ZDateTime fFOD;

		ZDateTime Get_ddMMyy_FormattedDateFromString(ZString date)
		{
			ZDateTime result = ZDateTime.Empty;

			if (date.Length == 6)
			{
				try
				{
					result = DateTime.ParseExact(date, "ddMMyy", new CultureInfo("en-AU"));
				}
				catch (FormatException)
				{
					// leave date empty
				}
			}

			return result;
		}

		#endregion

		#region Properties

		#region WarehouseOrg

		public ZGuid WarehouseOrgFK
		{
			get
			{
				return WarehouseAddress != null ? WarehouseAddress.Header.PK : fWarehouseOrgFK;
			}
			set
			{
				if (fWarehouseOrgFK.IsEmpty || fWarehouseOrgFK != value)
				{
					SetNonPersistentPropertyValue(WarehouseOrgFKInfo, ref fWarehouseOrgFK, value);
					var org = Factory.Load<OrgHeader>(fWarehouseOrgFK);
					if (org != null)
					{
						OrgAddressDependentCollection addresses = org.Addresses;
						ZA_OA_WarehouseAddress_Hidden = addresses.Count > 0 ? addresses[0].PK : ZGuid.Empty;
					}
					else
					{
						ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
					}
				}
			}
		}
		ZGuid fWarehouseOrgFK;

		public ZPropertyInfo WarehouseOrgFKInfo
		{
			get { return GetZPropertyInfo(nameof(WarehouseOrgFK)); }
		}

		public OrgAddress WarehouseAddress
		{
			get
			{
				var result = Factory.Load<OrgAddress>(ZA_OA_WarehouseAddress_Hidden);
				if (result != null)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						var declaration = invoiceLine.Declaration;
						if (declaration != null && !declaration.IsExWarehouse && declaration.IsWHSUniversalXMLActive)
						{
							result = null;
						}
					}
				}
				return result;
			}
		}

		public override ZGuid ZA_OA_WarehouseAddress_Hidden
		{
			get { return base.ZA_OA_WarehouseAddress_Hidden; }
			set
			{
				var invoiceLine = InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				base.ZA_OA_WarehouseAddress_Hidden = declaration != null && !declaration.IsExWarehouse && declaration.IsWHSUniversalXMLActive ? ZGuid.Empty : value;
			}
		}

		internal void ClearZA_OA_WarehouseAddress()
		{
			base.ZA_OA_WarehouseAddress_Hidden = ZGuid.Empty;
		}

		#endregion

		public CollectionCache CollectionCache
		{
			get
			{
				if (fCollectionCache == null)
				{
					if (JobDeclaration != null)
					{
						fCollectionCache = JobDeclaration.CollectionCache;
					}
					else
					{
						fCollectionCache = new CollectionCache(Factory);
					}
				}
				return fCollectionCache;
			}
		}

		public JobDeclaration JobDeclaration
		{
			get
			{
				if (fJobDeclaration == null && Parent is Customs.Business.IDeclarationProvider && !Parent.IsDeleted)
				{
					try
					{
						fJobDeclaration = (JobDeclaration)((Customs.Business.IDeclarationProvider)Parent).Declaration;
					}
					catch (InvalidCastException ex)
					{
						var declaration = ((Customs.Business.IDeclarationProvider)Parent).Declaration;
						var invoiceHeader = Parent as JobComInvoiceHeader;
						ErrorReporter.ReportOnce("AUAddInfo|GetJobDeclaration", $@"{ex.Message}
Debug info
Parent Type: {Parent.GetType().FullName}
Parent PK: {((BusinessObject)Parent).PK}
Declaration PK: {declaration?.PK}
Declaration country: {declaration?.CountryCode}
Invoice header country: {invoiceHeader?.InvoiceCountry?.Code}
Invoice header stack trace:
{invoiceHeader?.constructStackTrace}", ex);
					}
				}
				return fJobDeclaration;
			}
		}

		public RefCountry CountryOfOrigin
		{
			get
			{
				RefCountry result = null;
				ZString countryCode = ZA_ORG;
				if (countryCode.Length > 2)
				{
					countryCode = AUCCountryCodeToRefCountryCode.GetRefCountryCode(countryCode);
				}

				if (!countryCode.IsEmpty)
				{
					result = RefCountry.LoadFromCountryCode(Factory, countryCode);
				}

				return result;
			}
		}

		public bool IsExport
		{
			get { return JobDeclaration != null && JobDeclaration.IsExport; }
		}

		public bool IsImport
		{
			get { return JobDeclaration != null && JobDeclaration.IsImport; }
		}

		public bool IsAir
		{
			get { return JobDeclaration != null && JobDeclaration.IsAir; }
		}

		public bool IsSea
		{
			get { return JobDeclaration != null && JobDeclaration.IsSea; }
		}

		public bool IsFCL
		{
			get { return JobDeclaration != null && JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.FCL; }
		}

		public bool IsLCL
		{
			get { return JobDeclaration != null && JobDeclaration.JE_ContainerMode == Core.Constants.ContainerModes.LCL; }
		}

		public bool IsImportCMR
		{
			get { return JobDeclaration != null && JobDeclaration.IsImportCMR; }
		}

		public bool IsDrawback
		{
			get { return JobDeclaration != null && JobDeclaration.IsDrawback; }
		}

		public bool IsExWarehouse
		{
			get { return JobDeclaration != null && JobDeclaration.IsExWarehouse; }
		}

		public bool IsQuarantine
		{
			get { return JobDeclaration != null && JobDeclaration.IsQuarantine; }
		}

		public bool IsImportEdifice
		{
			get { return JobDeclaration != null && JobDeclaration.IsImportEdifice; }
		}

		bool IsSACWithoutLines
		{
			get { return JobDeclaration != null && JobDeclaration.IsSACWithoutLines; }
		}

		public bool IsGeneralRate
		{
			get
			{
				bool result = ZA_PST == AUAddInfo.GeneralPreferenceRate;
				if (InvoiceLine != null)
				{
					result = InvoiceLine.IsGeneralRate;
				}
				return result;
			}
		}

		public IEnumerable<ZString> Permits
		{
			get
			{
				if (ZA_PermitNumbers_Hidden.Length > 0)
				{
					ZString[] permits = ZA_PermitNumbers_Hidden.Split(',');
					for (int i = 0; i < permits.Length; i++)
					{
						int indexPos = permits[i].IndexOf(':');
						if (indexPos == -1)
						{
							// Do nothing - only a permit number
						}
						else if (indexPos == 0)
						{
							permits[i] = "";    // Invalid data entry - starts with a colon
						}
						else
						{
							permits[i] = permits[i].Left(indexPos);
						}
					}
					return permits;
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		public IEnumerable<ZString> EncryptionNumbers
		{
			get
			{
				if (ZA_PermitNumbers_Hidden.Length > 0)
				{
					ZString[] encryptions = ZA_PermitNumbers_Hidden.Split(',');
					for (int i = 0; i < encryptions.Length; i++)
					{
						int indexPos = encryptions[i].IndexOf(':');
						if (indexPos >= 0)
						{
							encryptions[i] = encryptions[i].Substring(indexPos + 1);
						}
						else
						{
							encryptions[i] = "";
						}
					}
					return encryptions;
				}
				else
				{
					return Array.Empty<ZString>();
				}
			}
		}

		public ZString TariffAndStatNumber
		{
			get
			{
				ZString result = "";
				if (!Parent.IsDeleted)
				{
					ITariffNumberProvider parentProvider = Parent as ITariffNumberProvider;
					if (parentProvider != null)
					{
						result = parentProvider.TariffAndStatNumber;
					}
				}
				return result;
			}
		}

		#endregion

		#region Calculated Fields

		#region AddInfoLineForAddInfoForm

		[BusinessObjectTestExclude()]
		[MaxLength(JobComInvoiceLine.Schema.JI_AddInfoMaxLength)]
		public ZString AddInfoLineForAddInfoForm
		{
			get { return AddInfoLine; }
			set
			{
				AddInfoLine = value;
				AddInfoLineForAddInfoFormInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AddInfoLineForAddInfoFormInfo
		{
			get { return GetZPropertyInfo(nameof(AddInfoLineForAddInfoForm)); }
		}

		#endregion

		#region Add Info Line

		[BusinessObjectTestExclude()]
		[MaxLength(JobComInvoiceLine.Schema.JI_AddInfoMaxLength)]
		public ZString AddInfoLine
		{
			get
			{
				return Factory.GetValue(ref addInfoLineCached, delegate
				{ return SerialiseAddInfo(false); });
			}
			set
			{
				string hiddenValues = GetHiddenValues();
				string newValue = value;
				if (hiddenValues.Length > 0 && newValue.IndexOf("Hidden", StringComparison.Ordinal) < 0)
				{
					newValue = value + SeperationCharacter + hiddenValues;
				}
				LoadPropertiesFromString(newValue);
				AddInfoLineInfo.RefreshBinding();
				Validation.ValidateAddInfoLine();
				if (IsDrawback && isInitialised && Parent is JobComInvoiceLine)
				{
					InvoiceLine.CalculateClaimAmountIfImputationMethod();
				}
			}
		}
		CachedProperty<ZString> addInfoLineCached;

		public bool AddInfoLine_ReadOnly { get; set; }

		string SerialiseAddInfo(bool shouldSerialiseHiddenFields)
		{
			ZStringBuilder result = new ZStringBuilder();
			foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
			{
				if (propertyInfo.HasSetter
					&& propertyInfo.Name.StartsWith("ZA_", StringComparison.Ordinal)
					&& (shouldSerialiseHiddenFields || !propertyInfo.Name.EndsWith("_Hidden", StringComparison.Ordinal)))
				{
					IZType value = propertyInfo.Value;

					if (!value.IsDefault || (propertyInfo.Name == AUAddInfoSchema.ZA_DTY.Name && ZA_SendZeroDutyOverride_Hidden))
					{
						if (!result.IsEmpty)
						{
							result.Append(SeperationCharacter.ToString());
						}

						if (value is ZDateTime)
						{
							value = new ZString(((ZDateTime)value).ToString(GetDateTimePropertyFormat(propertyInfo), CultureInfo.InvariantCulture));
						}

						result.Append(GetKey(propertyInfo.Name) + "=" + value.ToString());
					}
				}
			}
			return result.ToString();
		}

		static string GetKey(string propertyName)
		{
			return propertyName.Substring(3);
		}

		protected ZString GetHiddenValues()
		{
			ZString result = ZString.Empty;
			foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
			{
				if (propertyInfo.HasSetter && propertyInfo.Name.StartsWith("ZA_", StringComparison.Ordinal) && propertyInfo.Name.EndsWith("_Hidden", StringComparison.Ordinal))
				{
					IZType value = propertyInfo.Value;

					if (!value.IsDefault)
					{
						if (result.Length > 0)
						{
							result += SeperationCharacter;
						}

						result += GetKey(propertyInfo.Name) + "=" + value.ToString();
					}
				}
			}
			return result;
		}

		public ZPropertyInfo AddInfoLineInfo
		{
			get { return GetZPropertyInfo(Schema.AddInfoLine); }
		}

		#endregion

		public ZString MergeAddInfoString
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				if (DontMergeThisLine)
				{
					result.Append(ZGuid.NewZGuid().ToStringKey());
				}
				else
				{
					foreach (ZPropertyInfo info in ZPropertyInfoHash)
					{
						if (info.Name.StartsWith(TablePrefix, StringComparison.Ordinal) && !info.Name.EndsWith("_Hidden", StringComparison.Ordinal))
						{
							IZType aggregatedInfo = AggregatedValue(info.Name);
							if (!aggregatedInfo.IsEmpty)
							{
								result.Append(aggregatedInfo.ToString());
							}
						}
					}
				}
				return result.ToString();
			}
		}

		protected bool DontMergeThisLine
		{
			get
			{
				return
					ZA_LCT > 0
					|| ZA_CON > 0;
			}
		}

		#region AdjustmentCurrency_Hidden
		[BusinessObjectTestExclude()]
		public ZString AdjustmentCurrency_Hidden
		{
			get
			{
				return ZA_ADJ.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			}
			set
			{
				if (AdjustmentDollarPercentage_Hidden == "$")
				{
					ZA_ADJ = AdjustmentAmount_Hidden.ToString() + value.Left(3);
				}
				else
				{
					ZA_ADJ = AdjustmentAmount_Hidden.ToString() + "%";
				}
				AdjustmentCurrency_HiddenInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AdjustmentCurrency_HiddenInfo
		{
			get { return GetZPropertyInfo(Schema.AdjustmentCurrency_Hidden); }
		}

		public RefCurrency AdjustmentCurrency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, AdjustmentCurrency_Hidden); }
		}

		RefCurrencyCollection fZA_AdjustmentCurrencyList_Hidden;
		public RefCurrencyCollection ZA_AdjustmentCurrencyList_Hidden
		{
			get
			{
				if (fZA_AdjustmentCurrencyList_Hidden == null)
				{
					fZA_AdjustmentCurrencyList_Hidden = new RefCurrencyCollection(Factory);
				}
				return fZA_AdjustmentCurrencyList_Hidden;
			}
		}
		#endregion

		#region AdjustmentAmount_Hidden

		[DecimalPlaces("AdjustmentAmountDecimalPlaces")]
		public ZDecimal AdjustmentAmount_Hidden
		{
			get
			{
				ZDecimal result;

				string amount = ZA_ADJ.KeepChars("0123456789.-");
				if (double.TryParse(amount, NumberStyles.Number, null, out _))
				{
					result = ZDecimal.Parse(amount);
				}
				else
				{
					result = 0;
				}
				return result;
			}
			set
			{
				if (AdjustmentDollarPercentage_Hidden == "$")
				{
					ZA_ADJ = value.ToString() + AdjustmentCurrency_Hidden;
				}
				else
				{
					ZA_ADJ = value.ToString() + "%";
				}
				AdjustmentAmount_HiddenInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AdjustmentAmount_HiddenInfo
		{
			get { return GetZPropertyInfo(Schema.AdjustmentAmount_Hidden); }
		}

		protected int AdjustmentAmountDecimalPlaces
		{
			get { return AdjustmentDollarPercentage_Hidden != "%" ? 2 : 4; }
		}

		#endregion

		#region AdjustmentDollarPercentage_Hidden

		[BusinessObjectTestExclude()]
		[MaxLength(1)]
		public ZString AdjustmentDollarPercentage_Hidden
		{
			get
			{
				var result = ZA_ADJ.KeepChars("%$");
				if (result.IsEmpty && !ZA_ADJ.IsEmpty)
				{
					result = "$";
				}
				return result;
			}
			set
			{
				if (value == "%")
				{
					AdjustmentCurrency_Hidden = "";
					ZA_ADJ = AdjustmentAmount_Hidden.ToString() + value;
				}
				else if (value == "$")
				{
					ZA_ADJ = AdjustmentAmount_Hidden.ToString() + AdjustmentCurrency_Hidden;
				}
				AdjustmentDollarPercentage_HiddenInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo AdjustmentDollarPercentage_HiddenInfo
		{
			get { return GetZPropertyInfo(Schema.AdjustmentDollarPercentage_Hidden); }
		}

		CodeDescriptionPairList fZA_AdjustmentDollarPercentageList_Hidden;
		public CodeDescriptionPairList ZA_AdjustmentDollarPercentageList_Hidden
		{
			get
			{
				if (fZA_AdjustmentDollarPercentageList_Hidden == null)
				{
					fZA_AdjustmentDollarPercentageList_Hidden = new AUAddInfoAdjustmentDollarPercentageList();
				}
				return fZA_AdjustmentDollarPercentageList_Hidden;
			}
		}
		#endregion

		#region DumpingExportAmount

		public ZDecimal DumpingExportAmount
		{
			get
			{
				ZDecimal result;

				double junk = 0;
				string amount = ZA_DXP.KeepChars("0123456789.");
				if (double.TryParse(amount, NumberStyles.Number, null, out junk))
				{
					result = ZDecimal.Parse(amount);
				}
				else
				{
					result = 0;
				}
				return result;
			}
		}

		#endregion

		#region DumpingExportCurrecy

		public RefCurrency DumpingExportCurrency
		{
			get
			{
				string currencyCode = ZA_DXP.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
				RefCurrency result = null;
				if (currencyCode.Length == 3)
				{
					result = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				}
				else
				{
					result = JobDeclaration.GetLocalCurrency();
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region AQIS

		#region AQIS Commodity Code

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISCommCodes_Hidden
		{
			get
			{
				return base.ZA_AQISCommCodes_Hidden;
			}
			set
			{
				if (ParentAQISInfo != null && base.ZA_AQISCommCodes_Hidden != value)
				{
					ParentAQISInfo.AQISCommodityCodes.SplitAndAddAQISElements(value);
					ReBuildAQISCommodityCodes();
				}
			}
		}

		public void ReBuildAQISCommodityCodes()
		{
			if (ParentAQISInfo != null)
			{
				base.ZA_AQISCommCodes_Hidden = ParentAQISInfo.AQISCommodityCodes.ReBuildAQISElements();
			}
		}

		#endregion

		#region AQIS Entity Ids

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISEntityIds_Hidden
		{
			get
			{
				return base.ZA_AQISEntityIds_Hidden;
			}
			set
			{
				if (ParentAQISInfo != null && base.ZA_AQISEntityIds_Hidden != value)
				{
					ParentAQISInfo.AQISEntityIds.SplitAndAddAQISElements(value);
					base.ZA_AQISEntityIds_Hidden = ParentAQISInfo.AQISEntityIds.ReBuildAQISElements();
				}
			}
		}

		public void ReBuildAQISEntityIds()
		{
			if (ParentAQISInfo != null)
			{
				base.ZA_AQISEntityIds_Hidden = ParentAQISInfo.AQISEntityIds.ReBuildAQISElements();
			}
		}

		#endregion

		#region AQIS Permit Nos

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISPermitIds_Hidden
		{
			get
			{
				return base.ZA_AQISPermitIds_Hidden;
			}
			set
			{
				if (ParentAQISInfo != null && base.ZA_AQISPermitIds_Hidden != value)
				{
					ParentAQISInfo.AQISPermitIds.SplitAndAddAQISElements(value);
					base.ZA_AQISPermitIds_Hidden = ParentAQISInfo.AQISPermitIds.ReBuildAQISElements();
				}
			}
		}

		public void ReBuildAQISPermitIds()
		{
			if (ParentAQISInfo != null)
			{
				base.ZA_AQISPermitIds_Hidden = ParentAQISInfo.AQISPermitIds.ReBuildAQISElements();
			}
		}

		#endregion

		#region AQIS Producer Codes

		[BusinessObjectTestExclude]
		public override ZString ZA_AQISProducerCodes_Hidden
		{
			get
			{
				return base.ZA_AQISProducerCodes_Hidden;
			}
			set
			{
				if (ParentAQISInfo != null && base.ZA_AQISProducerCodes_Hidden != value)
				{
					ParentAQISInfo.AQISProducerCodes.SplitAndAddAQISElements(value);
					base.ZA_AQISProducerCodes_Hidden = ParentAQISInfo.AQISProducerCodes.ReBuildAQISElements();
				}
			}
		}

		public void ReBuildAQISProducerCodes()
		{
			if (ParentAQISInfo != null)
			{
				base.ZA_AQISProducerCodes_Hidden = ParentAQISInfo.AQISProducerCodes.ReBuildAQISElements();
			}
		}

		#endregion

		public void SetAQISFieldsForSave()
		{
			if (ParentAQISInfo != null)
			{
				ParentAQISInfo.AQISDocuments.ReBuildAndSaveAQISElements();
				ParentAQISInfo.AQISPremisesIdAndProcessingTypes.ReBuildAndSaveAQISElements();
				ZA_AQISEntityIds_Hidden = ParentAQISInfo.AQISEntityIds.ReBuildAQISElements();
				ZA_AQISProducerCodes_Hidden = ParentAQISInfo.AQISProducerCodes.ReBuildAQISElements();
				ZA_AQISPermitIds_Hidden = ParentAQISInfo.AQISPermitIds.ReBuildAQISElements();
				ZA_AQISCommCodes_Hidden = ParentAQISInfo.AQISCommodityCodes.ReBuildAQISElements();
			}
		}

		public IAQIS ParentAQISInfo
		{
			get
			{
				if (fParentAQISInfo == null)
				{
					if (Parent is IAQIS && !Parent.IsDeleted)
					{
						fParentAQISInfo = Parent as IAQIS;
					}
				}

				return fParentAQISInfo;
			}
		}
		IAQIS fParentAQISInfo;

		#endregion

		#region New Fields for CMR

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(3)]
		public ZString TCI_InstrumentType
		{
			get { return ZA_TCI.IndexOf(InstrumentSeparator) >= 0 ? ZA_TCI.Split(InstrumentSeparator)[0] : ZString.Empty; }
			set
			{
				if (TCI_InstrumentType != value)
				{
					ZString result = value + InstrumentSeparator + TCI_InstrumentNo;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(TCI_InstrumentTypeInfo, value);
					base.ZA_TCI = result;
				}
				TCI_InstrumentTypeInfo.RefreshBinding();
				Validation.ValidateTCI_InstrumentType();
			}
		}

		public ZPropertyInfo TCI_InstrumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TCI_InstrumentType); }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(8)]
		public ZString TCI_InstrumentNo
		{
			get { return ZA_TCI.IndexOf(InstrumentSeparator) >= 0 ? ZA_TCI.Split(InstrumentSeparator)[1] : ZString.Empty; }
			set
			{
				if (TCI_InstrumentNo != value)
				{
					ZString result = TCI_InstrumentType + InstrumentSeparator + value;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(TCI_InstrumentNoInfo, value);
					base.ZA_TCI = result;
				}
				TCI_InstrumentNoInfo.RefreshBinding();
				Validation.ValidateTCI_InstrumentNo();
			}
		}

		public ZPropertyInfo TCI_InstrumentNoInfo
		{
			get { return GetZPropertyInfo(Schema.TCI_InstrumentNo); }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(3)]
		public ZString PRI_InstrumentType
		{
			get { return ZA_PRI.IndexOf(InstrumentSeparator) >= 0 ? ZA_PRI.Split(InstrumentSeparator)[0] : ZString.Empty; }
			set
			{
				if (PRI_InstrumentType != value)
				{
					ZString result = value + InstrumentSeparator + PRI_InstrumentNo;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(PRI_InstrumentTypeInfo, value);
					base.ZA_PRI = result;
				}
				PRI_InstrumentTypeInfo.RefreshBinding();
				Validation.ValidatePRI_InstrumentType();
			}
		}

		public ZPropertyInfo PRI_InstrumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.PRI_InstrumentType); }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(8)]
		public ZString PRI_InstrumentNo
		{
			get { return ZA_PRI.IndexOf(InstrumentSeparator) >= 0 ? ZA_PRI.Split(InstrumentSeparator)[1] : ZString.Empty; }
			set
			{
				if (PRI_InstrumentNo != value)
				{
					ZString result = PRI_InstrumentType + InstrumentSeparator + value;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(PRI_InstrumentNoInfo, value);
					base.ZA_PRI = result;
				}
				PRI_InstrumentNoInfo.RefreshBinding();
				Validation.ValidatePRI_InstrumentNo();
			}
		}

		public ZPropertyInfo PRI_InstrumentNoInfo
		{
			get { return GetZPropertyInfo(Schema.PRI_InstrumentNo); }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(3)]
		public ZString TI2_InstrumentType
		{
			get { return ZA_TI2.IndexOf(InstrumentSeparator) >= 0 ? ZA_TI2.Split(InstrumentSeparator)[0] : ZString.Empty; }
			set
			{
				if (TI2_InstrumentType != value)
				{
					ZString result = value + InstrumentSeparator + TI2_InstrumentNo;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(TI2_InstrumentTypeInfo, value);
					base.ZA_TI2 = result;
				}
				TI2_InstrumentTypeInfo.RefreshBinding();
				Validation.ValidateTI2_InstrumentType();
			}
		}

		public ZPropertyInfo TI2_InstrumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TI2_InstrumentType); }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		[MaxLength(8)]
		public ZString TI2_InstrumentNo
		{
			get { return ZA_TI2.IndexOf(InstrumentSeparator) >= 0 ? ZA_TI2.Split(InstrumentSeparator)[1] : ZString.Empty; }
			set
			{
				if (TI2_InstrumentNo != value)
				{
					ZString result = TI2_InstrumentType + InstrumentSeparator + value;
					if (result == InstrumentSeparator.ToString())
					{
						result = "";
					}

					CheckMaximumLength(TI2_InstrumentNoInfo, value);
					base.ZA_TI2 = result;
				}
				TI2_InstrumentNoInfo.RefreshBinding();
				Validation.ValidateTI2_InstrumentNo();
			}
		}

		public ZPropertyInfo TI2_InstrumentNoInfo
		{
			get { return GetZPropertyInfo(Schema.TI2_InstrumentNo); }
		}

		#endregion

		#region Assay

		#region ZA_AssayAU_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayAU_Hidden
		{
			get { return base.ZA_AssayAU_Hidden; }
			set { base.ZA_AssayAU_Hidden = value; }
		}

		#endregion

		#region ZA_AssayNI_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayNI_Hidden
		{
			get { return base.ZA_AssayNI_Hidden; }
			set { base.ZA_AssayNI_Hidden = value; }
		}

		#endregion

		#region ZA_AssaySN_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssaySN_Hidden
		{
			get { return base.ZA_AssaySN_Hidden; }
			set { base.ZA_AssaySN_Hidden = value; }
		}

		#endregion

		#region ZA_AssayPT_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayPT_Hidden
		{
			get { return base.ZA_AssayPT_Hidden; }
			set { base.ZA_AssayPT_Hidden = value; }
		}

		#endregion

		#region ZA_AssayPB_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayPB_Hidden
		{
			get { return base.ZA_AssayPB_Hidden; }
			set { base.ZA_AssayPB_Hidden = value; }
		}

		#endregion

		#region ZA_AssayCU_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayCU_Hidden
		{
			get { return base.ZA_AssayCU_Hidden; }
			set { base.ZA_AssayCU_Hidden = value; }
		}

		#endregion

		#region ZA_AssayAG_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayAG_Hidden
		{
			get { return base.ZA_AssayAG_Hidden; }
			set { base.ZA_AssayAG_Hidden = value; }
		}

		#endregion

		#region ZA_AssayWO_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayWO_Hidden
		{
			get { return base.ZA_AssayWO_Hidden; }
			set { base.ZA_AssayWO_Hidden = value; }
		}

		#endregion

		#region ZA_AssayZN_Hidden

		[DecimalPlaces(5)]
		public override ZDecimal ZA_AssayZN_Hidden
		{
			get { return base.ZA_AssayZN_Hidden; }
			set { base.ZA_AssayZN_Hidden = value; }
		}

		#endregion

		#endregion

		public override string ToString()
		{
			return SerialiseAddInfo(true);
		}

		public void LoadPropertiesFromString(ZString addInfoLine)
		{
			LoadPropertiesFromString(addInfoLine, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void LoadPropertiesFromString(ZString addInfoLine, bool clearExisting)
		{
			const string ORG = "ORG";
			const string PRF = "PRF";
			const string PST = "PST";
			if (settingAddInfoProperty)
			{
				return;
			}

			if (addInfoLine.Length > 0 || clearExisting)
			{
				bool factoryIsReadOnly = ((IBusinessObjectFactoryInternals)Factory).ReadOnly;
				((IBusinessObjectFactoryInternals)Factory).ReadOnly = false;
				using (Parent.SuspendMarkingAsNeedingValidation())
				{
					try
					{
						Hashtable addInfoHash = new Hashtable();
						var oRG = ZString.Empty;
						var pRF = ZString.Empty;
						var pST = ZString.Empty;
						if (!addInfoLine.IsEmpty)
						{
							ZString[] addInfoItems = addInfoLine.Split(SeperationCharacter);
							foreach (var addInfoItem in addInfoItems)
							{
								int equalsPosition = addInfoItem.IndexOf('=');
								var key1 = ZString.Empty;
								var value = ZString.Empty;
								if (equalsPosition > -1)
								{
									key1 = addInfoItem.Left(equalsPosition);
									value = addInfoItem.SubstringSafe(equalsPosition + 1);
								}
								if (!key1.IsEmpty)
								{
									switch (key1)
									{
										case ORG:
											oRG = value.Left(ZA_ORGInfo.MaxLength);
											break;

										case PRF:
											pRF = value.Left(ZA_PRFInfo.MaxLength);
											break;

										case PST:
											pST = value.Left(ZA_PSTInfo.MaxLength);
											break;

										default:
											addInfoHash[key1] = value;
											break;
									}
								}
							}
						}

						bool hasElementChanged = false;
						using (((ISingleElementListInternal)this).SuspendListChanged())
						{
							if (clearExisting)
							{
								foreach (ZPropertyInfo propertyInfo in ZPropertyInfoHash)
								{
									if (propertyInfo.HasSetter && propertyInfo.Name.StartsWith(TablePrefix, StringComparison.Ordinal))
									{
										string addInfoPropertyName = propertyInfo.Name;
										IZType addInfoPropertyValue = (IZType)this[addInfoPropertyName];
										string key = GetKey(addInfoPropertyName);
										if (!addInfoPropertyValue.IsDefault && !addInfoHash.ContainsKey(key) &&
											(key != ORG || oRG.IsEmpty) &&
											(key != PRF || pRF.IsEmpty) &&
											(key != PST || pST.IsEmpty))
										{
											this[addInfoPropertyName] = addInfoPropertyValue.Default;
											hasElementChanged = true;
										}
									}
								}
							}

							foreach (DictionaryEntry entry in addInfoHash)
							{
								string addInfoPropertyName = TablePrefix + entry.Key.ToString();
								try
								{
									IZType currentValue = (IZType)this[addInfoPropertyName];

									string hashValue = entry.Value.ToString();
									if (currentValue.ToString() != hashValue)
									{
										this[addInfoPropertyName] = GetTypedValue(ZPropertyInfoHash[addInfoPropertyName], hashValue);
										hasElementChanged = true;
									}
								}
								catch (ArgumentException)
								{
									// Invalid PropertyName - due to user typing in crap in addinfo - Don't care
								}
							}

							if (!oRG.IsEmpty)
							{
								if (ZA_ORG != oRG)
								{
									ZA_ORG = oRG;
									hasElementChanged = true;
								}
								if (ZA_PRF != pRF)
								{
									ZA_PRF = pRF;
									hasElementChanged = true;
								}
								if (ZA_PST != pST)
								{
									ZA_PST = pST;
									hasElementChanged = true;
								}
							}
							else
							{
								if (!pRF.IsEmpty && ZA_PRF != pRF)
								{
									ZA_PRF = pRF;
									hasElementChanged = true;
								}
								if (!pST.IsEmpty && ZA_PST != pST)
								{
									ZA_PST = pST;
									hasElementChanged = true;
								}
							}
						}
						if (hasElementChanged)
						{
							OnElementChanged();
						}
					}
					finally
					{
						((IBusinessObjectFactoryInternals)Factory).ReadOnly = factoryIsReadOnly;
					}
				}
			}
		}

		public const string DateFormat = "dd/MM/yyyy";
		public const string DateTimeFormat = "dd/MM/yyyy HH:mm";

		string GetDateTimePropertyFormat(ZPropertyInfo propertyInfo)
		{
			switch (propertyInfo.Name)
			{
				case AUAddInfoSchema.Constants.ZA_ScheduledPaymentDate_Hidden:
					return DateTimeFormat;
				default:
					return DateFormat;
			}
		}

		IZType GetTypedValue(ZPropertyInfo propertyInfo, string hashValue)
		{
			IZType addInfoPropertyValue = propertyInfo.Value;

			IZType result;
			if (propertyInfo.Value is ZDecimal)
			{
				try
				{
					result = ZDecimal.Parse(hashValue);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = ZDecimal.Zero;
				}
			}
			else if (addInfoPropertyValue is ZInt)
			{
				try
				{
					result = ZInt.Parse(hashValue);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = ZInt.Zero;
				}
			}
			else if (addInfoPropertyValue is ZShort)
			{
				try
				{
					result = ZShort.Parse(hashValue);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					result = ZShort.Zero;
				}
			}
			else if (addInfoPropertyValue is ZString)
			{
				if (propertyInfo.MaxLength > 0)
				{
					result = new ZString(hashValue).Left(propertyInfo.MaxLength);
				}
				else
				{
					result = new ZString(hashValue);
				}
			}
			else if (addInfoPropertyValue is ZBool)
			{
				result = new ZBool(hashValue);
			}
			else if (addInfoPropertyValue is ZGuid)
			{
				result = new ZGuid(hashValue);
			}
			else if (addInfoPropertyValue is ZDateTime)
			{
				var dateTimeFormat = GetDateTimePropertyFormat(propertyInfo);
				var valueToParse = hashValue;

				if (!dateTimeFormat.Contains(' '))
				{
					var splitValues = hashValue.Split(' ');
					if (splitValues.Length > 0)
					{
						valueToParse = splitValues[0].Trim();
					}
				}

				return ZDateTime.TryParseExact(valueToParse, out var parsedDateTime, dateTimeFormat)
					? parsedDateTime
					: ZDateTime.Empty;
			}
			else
			{
				throw new ArgumentException("HashValue");
			}
			return result;
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		#region Implementation

		protected void UpdateRelatedPropertyInfo()
		{
			if (AddInfoProperty != null && !IsUpdateRelatedPropertyInfoDisabled)
			{
				if (!settingAddInfoProperty && !Parent.IsDeleted)
				{
					settingAddInfoProperty = true;
					IDisposable suspender = Parent.LightValidationEnabled ? Parent.SuspendMarkingAsNeedingValidation() : null;
					try
					{
						AddInfoProperty.Value = (ZString)this.ToString();
					}
					finally
					{
						settingAddInfoProperty = false;
						if (suspender != null)
						{
							suspender.Dispose();
						}
					}
				}
			}
		}

		protected void RefreshParentBizObj()
		{
			BusinessObject parentBizObj = Parent as BusinessObject;
			if (parentBizObj != null && !IsCopying && !IsValidationSuspended)
			{
				parentBizObj.RefreshBinding();
			}
		}

		public bool LineLevelAddInfo
		{
			get { return Parent is JobComInvoiceLine || PartAddInfo; }
		}

		public bool HeaderLevelAddInfo
		{
			get { return Parent is JobComInvoiceHeader; }
		}

		public bool DeclarationLevelAddInfo
		{
			get { return Parent is JobDeclaration; }
		}

		public bool PartAddInfo
		{
			get
			{
				return Parent is AUOrgSupplierPart
					|| Parent is CusClassPartPivot
					|| Parent is Classification;
			}
		}

		public bool IsALineLevelProperty(string propertyName)
		{
			switch (propertyName)
			{
				case AUAddInfoSchema.Constants.ZA_ADJ:
				case AUAddInfoSchema.Constants.ZA_AQIS:
				case AUAddInfoSchema.Constants.ZA_CL2:
				case AUAddInfoSchema.Constants.ZA_CON:
				case AUAddInfoSchema.Constants.ZA_CSA:
				case AUAddInfoSchema.Constants.ZA_CSC:
				case AUAddInfoSchema.Constants.ZA_CVD:
				case AUAddInfoSchema.Constants.ZA_DCX:
				case AUAddInfoSchema.Constants.ZA_DMP:
				case AUAddInfoSchema.Constants.ZA_DRC:
				case AUAddInfoSchema.Constants.ZA_DRE:
				case AUAddInfoSchema.Constants.ZA_DSA:
				case AUAddInfoSchema.Constants.ZA_DSN:
				case AUAddInfoSchema.Constants.ZA_DTY:
				case AUAddInfoSchema.Constants.ZA_DXP:
				case AUAddInfoSchema.Constants.ZA_DXT:
				case AUAddInfoSchema.Constants.ZA_ELA:
				case AUAddInfoSchema.Constants.ZA_FOD:
				case AUAddInfoSchema.Constants.ZA_ICN:
				case AUAddInfoSchema.Constants.ZA_ICV:
				case AUAddInfoSchema.Constants.ZA_IDP:
				case AUAddInfoSchema.Constants.ZA_ISC:
				case AUAddInfoSchema.Constants.ZA_ISS:
				case AUAddInfoSchema.Constants.ZA_LCP:
				case AUAddInfoSchema.Constants.ZA_LCT:
				case AUAddInfoSchema.Constants.ZA_LCTE:
				case AUAddInfoSchema.Constants.ZA_LCTI:
				case AUAddInfoSchema.Constants.ZA_LCTQ:
				case AUAddInfoSchema.Constants.ZA_MD2:
				case AUAddInfoSchema.Constants.ZA_MLP:
				case AUAddInfoSchema.Constants.ZA_MLPI:
				case AUAddInfoSchema.Constants.ZA_ODF:
				case AUAddInfoSchema.Constants.ZA_PIQ:
				case AUAddInfoSchema.Constants.ZA_PRI:
				case AUAddInfoSchema.Constants.ZA_PUP:
				case AUAddInfoSchema.Constants.ZA_QIN:
				case AUAddInfoSchema.Constants.ZA_QSC:
				case AUAddInfoSchema.Constants.ZA_QT2:
				case AUAddInfoSchema.Constants.ZA_RNO:
				case AUAddInfoSchema.Constants.ZA_SEC:
				case AUAddInfoSchema.Constants.ZA_STD:
				case AUAddInfoSchema.Constants.ZA_TAN:
				case AUAddInfoSchema.Constants.ZA_TC2:
				case AUAddInfoSchema.Constants.ZA_TCI:
				case AUAddInfoSchema.Constants.ZA_TFQ:
				case AUAddInfoSchema.Constants.ZA_TILV:
				case AUAddInfoSchema.Constants.ZA_TR2:
				case AUAddInfoSchema.Constants.ZA_TRN:
				case AUAddInfoSchema.Constants.ZA_UQ2:
				case AUAddInfoSchema.Constants.ZA_VID:
				case AUAddInfoSchema.Constants.ZA_WAR:
				case AUAddInfoSchema.Constants.ZA_WET:
				case AUAddInfoSchema.Constants.ZA_WETE:
				case AUAddInfoSchema.Constants.ZA_WETQ:
				case AUAddInfoSchema.Constants.ZA_WMC:
				case AUAddInfoSchema.Constants.ZA_WUV:
				case AUAddInfoSchema.Constants.ZA_WRQ:
				case AUAddInfoSchema.Constants.ZA_WRU:
					return true;
				case AUAddInfo.Schema.PRI_InstrumentNo:
				case AUAddInfo.Schema.PRI_InstrumentType:
				case AUAddInfo.Schema.TCI_InstrumentNo:
				case AUAddInfo.Schema.TCI_InstrumentType:
				case AUAddInfo.Schema.TI2_InstrumentNo:
				case AUAddInfo.Schema.TI2_InstrumentType:
					return true;

				default:
					return false;
			}
		}

		protected JobDeclaration fJobDeclaration;
		protected CollectionCache fCollectionCache;

		bool isCalculatingISSLitresOfAlcohol;
		public void CalculateLitresOfAlcoholFromQT2()
		{
			if (!isCalculatingISSLitresOfAlcohol && isInitialised)
			{
				isCalculatingISSLitresOfAlcohol = true;
				try
				{
					if (InvoiceLine != null
						&& !IsCopying
						&& InvoiceLine.DutiableInvoiceSpiritStrengthPercentage > 0
						&& InvoiceLine.JI_CustomsUnitQty.Trim() == "LA"
						&& ZA_UQ2 == "L")
					{
						InvoiceLine.JI_CustomsQuantity = ZA_QT2 * InvoiceLine.DutiableInvoiceSpiritStrengthPercentage / 100;
					}
				}
				finally
				{
					isCalculatingISSLitresOfAlcohol = false;
				}
			}
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return Parent as JobComInvoiceLine; }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get { return Parent as JobComInvoiceHeader; }
		}

		public JobComInvoiceGroupHeader GroupInvoice
		{
			get { return Parent as JobComInvoiceGroupHeader; }
		}

		#region ZPropertyInfos

		protected bool IsNotLineLevelAddInfo
		{
			get { return !LineLevelAddInfo; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_MD2
		{
			get { return base.ZA_MD2; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TC2
		{
			get { return base.ZA_TC2; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TFQ
		{
			get { return base.ZA_TFQ; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_WETQ
		{
			get { return base.ZA_WETQ; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_CON
		{
			get { return base.ZA_CON; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_MLP
		{
			get { return base.ZA_MLP; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_IDP
		{
			get { return base.ZA_IDP; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_ICV
		{
			get { return base.ZA_ICV; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_QSC
		{
			get { return base.ZA_QSC; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_TAN
		{
			get { return base.ZA_TAN; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_DXP
		{
			get { return base.ZA_DXP; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_WETE
		{
			get { return base.ZA_WETE; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_LCTQ
		{
			get { return base.ZA_LCTQ; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_LCT
		{
			get { return base.ZA_LCT; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_LCTE
		{
			get { return base.ZA_LCTE; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_WUV
		{
			get { return base.ZA_WUV; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_UQ2
		{
			get { return base.ZA_UQ2; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_AQIS
		{
			get { return base.ZA_AQIS; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_PIQ
		{
			get { return base.ZA_PIQ; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_ODF
		{
			get { return base.ZA_ODF; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_DSA
		{
			get { return base.ZA_DSA; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_DSN
		{
			get { return base.ZA_DSN; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_CSC
		{
			get { return base.ZA_CSC; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_DTY
		{
			get { return base.ZA_DTY; }
		}

		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_DRE
		{
			get { return base.ZA_DRE; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_DMP
		{
			get { return base.ZA_DMP; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_CVD
		{
			get { return base.ZA_CVD; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZDecimal ZA_CSA
		{
			get { return base.ZA_CSA; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_QIN
		{
			get { return base.ZA_QIN; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_DCX
		{
			get { return base.ZA_DCX; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_DRC
		{
			get { return base.ZA_DRC; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_ICN
		{
			get { return base.ZA_ICN; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_MLPI
		{
			get { return base.ZA_MLPI; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_SEC
		{
			get { return base.ZA_SEC; }
		}

		[ReadOnlyMember(nameof(IsNotLineLevelAddInfo))]
		public override ZString ZA_PUP
		{
			get { return base.ZA_PUP; }
			set
			{
				base.ZA_PUP = value;
				JobDeclaration?.MarkAsNeedingValidation();
			}
		}

		#endregion

		#endregion

		#region IAggregatedAddInfo Members

		public ZDateTime DateOfValuation
		{
			get { return (Parent != null && !Parent.IsDeleted && Parent.DateOfValuation.IsValid) ? Parent.DateOfValuation : ZDateTime.Today; }
		}

		public ZDateTime EffectiveDutyDate
		{
			get { return (Parent != null && !Parent.IsDeleted && Parent.EffectiveDutyDate.IsValid) ? Parent.EffectiveDutyDate : ZDateTime.Today; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			IZType result = ZString.Empty;
			if (IsAnAggregatableValue(propertyName))
			{
				result = (IZType)this[propertyName];
				if (result.IsEmpty)
				{
					result = Parent.AggregatedValue(propertyName);
				}
			}
			return result;
		}

		bool IsAnAggregatableValue(string propertyName)
		{
			switch (propertyName)
			{
				case AUAddInfoSchema.Constants.ZA_ADJ:
				case AUAddInfoSchema.Constants.ZA_CSA:
				case AUAddInfoSchema.Constants.ZA_CVD:
				case AUAddInfoSchema.Constants.ZA_DMP:
				case AUAddInfoSchema.Constants.ZA_DSA:
				case AUAddInfoSchema.Constants.ZA_DTY:
				case AUAddInfoSchema.Constants.ZA_DXP:
				case AUAddInfoSchema.Constants.ZA_ICV:
				case AUAddInfoSchema.Constants.ZA_IDP:
				case AUAddInfoSchema.Constants.ZA_ODF:
				case AUAddInfoSchema.Constants.ZA_QT2:
				case AUAddInfoSchema.Constants.ZA_STD:
				case AUAddInfoSchema.Constants.ZA_TILV:
				case AUAddInfoSchema.Constants.ZA_VID:
				case AUAddInfoSchema.Constants.ZA_WAR:
				case AUAddInfoSchema.Constants.ZA_WET:
				case AUAddInfoSchema.Constants.ZA_WRQ:
				case Schema.AddInfoLine:
					return false;

				default:
					return true;
			}
		}

		bool IAggregatedAddInfo.IsCopying => IsCopying;

		#endregion

		#region ICurrencyConverterDataProvider Members

		ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get
			{
				return ZArchitecture.Core.ExchangeRateType.All;
			}
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get
			{
				return Customs.Business.BaseJobDeclaration.CurrencyConverterMaximumDaysToFallBack;
			}
		}

		//for Product or Classification
		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get
			{
				return GlbCompany.CurrentCompany;
			}
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return null; }
		}

		#endregion

		#region Customs.Business.IAddInfo Members

		void Customs.Business.IAddInfo.UpdateRelatedPropertyInfo()
		{
			UpdateRelatedPropertyInfo();
		}

		ZString Customs.Business.IAddInfo.GetKey(string propertyName)
		{
			return GetKey(propertyName);
		}

		IDictionary<ZString, Customs.Business.AddInfoPropertyNameAndValueParser> Customs.Business.IAddInfo.GetKeys()
		{
			var typeDictionary = Factory.GetCachedValue<IDictionary<Type, IDictionary<ZString, Customs.Business.AddInfoPropertyNameAndValueParser>>>("AUAddInfo.GetKeys", () => new Dictionary<Type, IDictionary<ZString, Customs.Business.AddInfoPropertyNameAndValueParser>>());
			var currentType = GetType();
			if (!typeDictionary.TryGetValue(currentType, out var dictionary))
			{
				dictionary = GetKeys(ZPropertyInfoHash, TablePrefix);
				typeDictionary.Add(currentType, dictionary);
			}
			return dictionary;
		}

		static IDictionary<ZString, Customs.Business.AddInfoPropertyNameAndValueParser> GetKeys(ZPropertyInfoHashtable zPropertyInfoHash, string tablePrefix)
		{
			var result = new Dictionary<ZString, Customs.Business.AddInfoPropertyNameAndValueParser>();
			foreach (ZPropertyInfo propertyInfo in zPropertyInfoHash)
			{
				if (propertyInfo.HasSetter)
				{
					var propertyName = propertyInfo.Name;
					if (propertyName.Substring(0, 3) == tablePrefix)
					{
						var key = GetKey(propertyName);
						result.Add(key, new Customs.Business.AddInfoPropertyNameAndValueParser() { PropertyName = propertyName, Parser = GetAddInfoValueFunction(key, propertyInfo) });
					}
				}
			}
			return result;
		}

		void Customs.Business.IAddInfo.SetValue(string propertyName, ZString value)
		{
			var info = ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (info != null)
			{
				var zValue = GetTypedValue(info, value);
				if (zValue is ZString)
				{
					this[propertyName] = ((ZString)zValue).Left(info.MaxLength);
				}
				else
				{
					this[propertyName] = zValue;
				}
			}
		}

		static Customs.Business.AddInfoValueParser GetAddInfoValueFunction(string key, ZPropertyInfo info)
		{
			var stringInfo = info as ZPropertyInfoString;
			return stringInfo == null ? null : new Customs.Business.AddInfoValueParser((logger, stringValue) =>
			{
				var maxLength = stringInfo.MaxLength;
				var result = stringValue.TrimEnd(' ');
				if (maxLength > 0 && result.Length > maxLength)
				{
					logger.Log(LogType.Warning, GetMaximumLengthTruncateMessage(key, maxLength, result));
					result = result.Left(maxLength);
				}
				return result;
			});
		}

		public static string GetMaximumLengthTruncateMessage(string key, int maxLength, string value)
		{
			return Res.GetString("710D78BB-A5FF-4936-9DE6-4315F405E088", "The maximum length of Add Info '{0}' is {1} characters, but '{2}' was specified; value has been truncated.", key, maxLength, value);
		}

		public bool IsUpdateRelatedPropertyInfoDisabled { get; set; }

		void Customs.Business.IAddInfo.UpdateAddInfoFromString(ZString addInfoString)
		{
			LoadPropertiesFromString(addInfoString);
			HasChanges = true;
		}

		IZType Customs.Business.IAddInfo.GetEffectiveValue(string propertyName)
		{
			IZType result = null;
			var info = ZPropertyInfoHash.GetPropertySafe(propertyName);
			if (info != null && info.HasSetter && info.Name.StartsWith(TablePrefix, StringComparison.Ordinal))
			{
				result = info.Value;
			}
			return result;
		}

		#endregion

		BusinessObject Integration.Customs.IAddInfoBase.Parent
		{
			get { return Parent as BusinessObject; }
		}
	}
}
