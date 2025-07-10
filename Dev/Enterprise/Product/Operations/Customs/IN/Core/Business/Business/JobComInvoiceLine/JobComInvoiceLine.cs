using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.IN;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public partial class JobComInvoiceLine : AutoINJobComInvoiceLine, ISupportMultipleResourceStringData, ICusSupportingInfoWithSerialNoParent, IDateOfValuationProvider
{
	public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoINJobComInvoiceLine.Schema
	{
		public new const int JI_TariffMaxLength = 8;
		public const int AccessoryDescriptionMaxLength = 500;
		public const string AccessoryDescription = nameof(JobComInvoiceLine.AccessoryDescription);
	}

	public override ZGuid JI_JZ
	{
		get => base.JI_JZ;
		set
		{
			base.JI_JZ = value;
			RefreshGSTPayNotApplicable();
		}
	}

	[ReadOnlyMember(nameof(JI_LinePrice_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_LinePrice", Caption = "Total Price", MediumCaption = "Total Price", ShortCaption = "Price", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZDecimal JI_LinePrice { get => base.JI_LinePrice; set => base.JI_LinePrice = value; }

	void CalculateLinePriceForExportDeclaration()
	{
		if (IsExport)
		{
			JI_LinePrice = JI_UnitQuantity.IsEmpty ? 0 : Utilities.Round(UnitConverter.Convert(JI_InvoiceQuantity, JI_InvoiceUQ, JI_UnitUQ) * JI_UnitPrice / JI_UnitQuantity, 2);
		}
	}

	[MaxLength(2)]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.OriginStateList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_StateOrRegionOfOrigin", Caption = "Source State", MediumCaption = "State", ShortCaption = "State")]
	public override ZString JI_StateOrRegionOfOrigin { get => base.JI_StateOrRegionOfOrigin; set => base.JI_StateOrRegionOfOrigin = value; }

	ZBool JI_LinePrice_ReadOnly => IsExport;

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_ValuationMarkup", Caption = "PMV (INR)", MediumCaption = "PMV (INR)", ShortCaption = "PMV")]
	public override ZDecimal JI_ValuationMarkup
	{
		get => base.JI_ValuationMarkup;
		set
		{
			var oldValue = JI_ValuationMarkup;
			base.JI_ValuationMarkup = value;
			if (!IsCopying && oldValue != JI_ValuationMarkup)
			{
				CalculatePMVAndTotalPMV();
			}
		}
	}

	ZBool JI_PMV_ReadOnly => !JI_ValuationMarkup.IsEmpty;

	[ReadOnlyMember(nameof(JI_PMV_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_PMV", Caption = "PMV")]
	public override ZDecimal JI_PMV
	{
		get => base.JI_PMV;
		set
		{
			var oldValue = JI_PMV;
			base.JI_PMV = value;
			if (!IsCopying && oldValue != JI_PMV)
			{
				CalculateTotalPMV();
			}
		}
	}

	ZBool JI_TotalPMV_ReadOnly => !JI_ValuationMarkup.IsEmpty;

	[ReadOnlyMember(nameof(JI_TotalPMV_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_TotalPMV", Caption = "Total PMV Value", MediumCaption = "Total PMV", ShortCaption = "Total PMV", MultipleKey = SharedJobMessageTypeList.Codes.Export)]
	public override ZDecimal JI_TotalPMV
	{
		get => base.JI_TotalPMV;
		set
		{
			var oldValue = JI_TotalPMV;
			base.JI_TotalPMV = value;
			if (!IsCopying && oldValue != JI_TotalPMV)
			{
				CalculatePMV();
			}
		}
	}

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SWConstituentCollection SWConstituents
	{
		get
		{
			if (fSWConstituents == null)
			{
				fSWConstituents = new SWConstituentCollection(this);
				fSWConstituents.Load();
				RegisterEditableChildObject(fSWConstituents);
			}
			return fSWConstituents;
		}
	}

	SWConstituentCollection fSWConstituents;

	public HugeSequenceNumberGenerator SWConstituentLineNumberGenerator => fSWConstituentLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SWConstituents);
	HugeSequenceNumberGenerator fSWConstituentLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SWControlCollection SWControls
	{
		get
		{
			if (fSWControls == null)
			{
				fSWControls = new SWControlCollection(this);
				fSWControls.Load();
				RegisterEditableChildObject(fSWControls);
			}
			return fSWControls;
		}
	}

	SWControlCollection fSWControls;

	public HugeSequenceNumberGenerator SWControlsLineNumberGenerator => fSWControlsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SWControls);
	HugeSequenceNumberGenerator fSWControlsLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public DfiaExportItemDetailCollection DfiaExportItemDetails
	{
		get
		{
			if (fDfiaExportItemDetails == null)
			{
				fDfiaExportItemDetails = new DfiaExportItemDetailCollection(this);
				fDfiaExportItemDetails.Load();
				RegisterEditableChildObject(fDfiaExportItemDetails);
			}
			return fDfiaExportItemDetails;
		}
	}

	DfiaExportItemDetailCollection fDfiaExportItemDetails;

	public HugeSequenceNumberGenerator DfiaExportItemDetailsLineNumberGenerator => fDfiaExportItemDetailsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => DfiaExportItemDetails);
	HugeSequenceNumberGenerator fDfiaExportItemDetailsLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public JobWorkCollection JobWorks
	{
		get
		{
			if (fJobWorks == null)
			{
				fJobWorks = new JobWorkCollection(this);
				fJobWorks.Load();
				RegisterEditableChildObject(fJobWorks);
			}
			return fJobWorks;
		}
	}

	JobWorkCollection fJobWorks;

	public HugeSequenceNumberGenerator JobWorkLineNumberGenerator => fJobWorkLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => JobWorks);
	HugeSequenceNumberGenerator fJobWorkLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SupportingDocumentCollection SupportingDocuments
	{
		get
		{
			if (fSupportingDocument == null)
			{
				fSupportingDocument = new SupportingDocumentCollection(this);
				fSupportingDocument.Load();
				RegisterEditableChildObject(fSupportingDocument);
			}
			return fSupportingDocument;
		}
	}

	SupportingDocumentCollection fSupportingDocument;

	public HugeSequenceNumberGenerator SupportingDocumentLineNumberGenerator => fSupportingDocumentLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SupportingDocuments);
	HugeSequenceNumberGenerator fSupportingDocumentLineNumberGenerator;

	[UniversalCopyCollectionEntity(CusSupportingInfoSchema.Constants.TableName, CusSupportingInfoSchema.Constants.CSI_ParentTableCode)]
	[ChildEditable(true)]
	public SWProductionDetailsCollection SWProductions
	{
		get
		{
			if (fSWProductions == null)
			{
				fSWProductions = new SWProductionDetailsCollection(this);
				fSWProductions.Load();
				RegisterEditableChildObject(fSWProductions);
			}
			return fSWProductions;
		}
	}

	SWProductionDetailsCollection fSWProductions;

	public HugeSequenceNumberGenerator SWProductionsLineNumberGenerator => fSWProductionsLineNumberGenerator ??= new HugeSequenceNumberGenerator(() => SWProductions);
	HugeSequenceNumberGenerator fSWProductionsLineNumberGenerator;

	public override ZDecimal JI_Weight
	{
		get => base.JI_Weight;
		set
		{
			var oldValue = JI_Weight;
			base.JI_Weight = value;
			if (oldValue != JI_Weight)
			{
				EntryInstruction?.GrossWeightInfo.RefreshBinding();
			}
		}
	}

	public override ZDecimal JI_NetWeight
	{
		get => base.JI_NetWeight;
		set
		{
			var oldValue = JI_NetWeight;
			base.JI_NetWeight = value;
			if (oldValue != JI_NetWeight)
			{
				EntryInstruction?.NetWeightInfo.RefreshBinding();
			}
		}
	}

	public override ZGuid JI_CEI
	{
		get => base.JI_CEI;
		set
		{
			var oldEntryInstruction = EntryInstruction;
			var oldValue = JI_CEI;
			base.JI_CEI = value;
			if (oldValue != JI_CEI)
			{
				RefreshEntryInstructionBindings(oldEntryInstruction);
				RefreshEntryInstructionBindings(EntryInstruction);
				InvoiceHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString JI_InvoiceUQ
	{
		get => base.JI_InvoiceUQ;
		set
		{
			var oldValue = JI_InvoiceUQ;
			base.JI_InvoiceUQ = value;
			if (!IsCopying && oldValue != JI_InvoiceUQ)
			{
				if (JI_UnitUQ.IsEmpty && !JI_InvoiceUQ.IsEmpty)
				{
					JI_UnitUQ = JI_InvoiceUQ;
				}
				CalculateLinePriceForExportDeclaration();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.RewardItemList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_RewardItem", Caption = "Reward Item", MediumCaption = "Reward", ShortCaption = "Reward")]
	public override ZString JI_RewardItem { get => base.JI_RewardItem; set => base.JI_RewardItem = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_UnitPrice", Caption = "Rate")]
	public override ZDecimal JI_UnitPrice
	{
		get => base.JI_UnitPrice;
		set
		{
			var oldValue = JI_UnitPrice;
			base.JI_UnitPrice = value;
			if (!IsCopying && oldValue != JI_UnitPrice)
			{
				CalculatePMV();
				CalculateLinePriceForExportDeclaration();
			}
		}
	}

	public override ZDecimal JI_InvoiceQuantity
	{
		get => base.JI_InvoiceQuantity;
		set
		{
			var oldValue = JI_InvoiceQuantity;
			base.JI_InvoiceQuantity = value;
			if (!IsCopying && oldValue != JI_InvoiceQuantity)
			{
				CalculateTotalPMV();
				CalculateLinePriceForExportDeclaration();
			}
		}
	}

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_UnitQuantity", Caption = "Rate Per Unit", MediumCaption = "Per/UOM", ShortCaption = "Per/UOM")]
	public override ZInt JI_UnitQuantity
	{
		get => base.JI_UnitQuantity;
		set
		{
			var oldValue = JI_UnitQuantity;
			base.JI_UnitQuantity = value;
			if (!IsCopying && oldValue != JI_UnitQuantity)
			{
				CalculateLinePriceForExportDeclaration();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.UnitUQList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_UnitUQ", Caption = "UOM")]
	public override ZString JI_UnitUQ
	{
		get => base.JI_UnitUQ;
		set
		{
			var oldValue = JI_UnitUQ;
			base.JI_UnitUQ = value;
			if (!IsCopying && oldValue != JI_UnitUQ)
			{
				CalculateLinePriceForExportDeclaration();
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.CountryOfTransits))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_RN_NKCountryOfTransit", Caption = "Transit Country", MediumCaption = "Trans. Ctry.", ShortCaption = "Tr. Ctry.")]
	public override ZString JI_RN_NKCountryOfTransit { get => base.JI_RN_NKCountryOfTransit; set => base.JI_RN_NKCountryOfTransit = value; }

	[MaxLength(1)]
	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.AccessoryStatusList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_AccessoryStatus", Caption = "Accessory Status", MediumCaption = "Accessory Status", ShortCaption = "Acc. Status")]
	public override ZString JI_AccessoryStatus
	{
		get => base.JI_AccessoryStatus;
		set
		{
			var oldValue = JI_AccessoryStatus;
			base.JI_AccessoryStatus = value;
			if (!IsCopying && oldValue != JI_AccessoryStatus && AccessoryDescription_ReadOnly)
			{
				AccessoryDescription = ZString.Empty;
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.EndUseCodes))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_EndUse", Caption = "End Use")]
	public override ZString JI_EndUse { get => base.JI_EndUse; set => base.JI_EndUse = value; }

	[MaxLength(JobComInvoiceLine.Schema.JI_TariffMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_Tariff", Caption = "RITC Code", MediumCaption = "RITC", ShortCaption = "RITC")]
	public override ZString JI_Tariff { get => base.JI_Tariff; set => base.JI_Tariff = value; }

	public override ZString UniversalTariffType => Universal.Constants.TariffTypes.IndianTradeClassification;

	[ReadOnlyMember(nameof(JI_GSTPayNotApplicable_ReadOnly))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_GSTPayNotApplicable", Caption = "Payment Not Applicable", MediumCaption = "Pay. NA", ShortCaption = "P. NA")]
	public override ZBool JI_GSTPayNotApplicable { get => base.JI_GSTPayNotApplicable; set => base.JI_GSTPayNotApplicable = value; }

	public bool JI_GSTPayNotApplicable_ReadOnly => InvoiceHeader?.IGSTPaymentNotApplicable ?? false;

	public void RefreshGSTPayNotApplicable()
	{
		JI_GSTPayNotApplicable = InvoiceHeader?.IGSTPaymentNotApplicable ?? false;
	}

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_MPG_CodeType", Caption = "Code Type", MediumCaption = "Cd. Type", ShortCaption = "Cd. Ty.")]
	public override ZString JI_MPG_CodeType { get => base.JI_MPG_CodeType; set => base.JI_MPG_CodeType = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_MPG_Code", Caption = "Code", MediumCaption = "Code", ShortCaption = "Code")]
	public override ZString JI_MPG_Code { get => base.JI_MPG_Code; set => base.JI_MPG_Code = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_OA_ManufacturerAddress", Caption = "Organization")]
	public override ZGuid JI_OA_ManufacturerAddress { get => base.JI_OA_ManufacturerAddress; set => base.JI_OA_ManufacturerAddress = value; }

	public override void Delete()
	{
		using (this.GetLineNumberSuspenders())
		{
			RefreshEntryInstructionBindings(EntryInstruction);
			this.DeleteHiddenNotes();
			base.Delete();
		}
	}

	#region ICusSupportingInfoTypeSupporter

	IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
	{
		{ CusSupportingInfoTypeList.Codes.SingleWindowConstituent, typeof(SWConstituent) },
		{ CusSupportingInfoTypeList.Codes.SingleWindowControl, typeof(SWControl) },
		{ CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization, typeof(DfiaExportItemDetail) },
		{ CusSupportingInfoTypeList.Codes.JobWork, typeof(JobWork)  },
		{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
		{ CusSupportingInfoTypeList.Codes.SingleWindowProduction, typeof(SWProduction) }
	};

	IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	#endregion

	HugeSequenceNumberGenerator ICusSupportingInfoWithSerialNoParent.GetSequenceNumberGenerator(string type)
	{
		return type switch
		{
			CusSupportingInfoTypeList.Codes.SingleWindowConstituent => SWConstituentLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.SingleWindowControl => SWControlsLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.SingleWindowProduction => SWProductionsLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.DutyFreeImportAuthorization => DfiaExportItemDetailsLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.JobWork => JobWorkLineNumberGenerator,
			CusSupportingInfoTypeList.Codes.SupportingDocument => SupportingDocumentLineNumberGenerator,
			_ => null
		};
	}

	// GetTariffDescription - to be overridden once the Tariff is setup for a new country
	protected override ZString GetTariffDescription(ZString tariffCode) => "TARIFF_DESCRIPTION";
	protected override ZString CustomsCountryCodeCore => Core.Constants.CountryCodes.India;
	protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

	void RefreshEntryInstructionBindings(CusEntryInstruction entryInstruction)
	{
		entryInstruction?.GrossWeightInfo.RefreshBinding();
		entryInstruction?.NetWeightInfo.RefreshBinding();
	}

	public void CalculatePMV()
	{
		if (IsExport)
		{
			if (JI_ValuationMarkup > 0)
			{
				var unitPriceInLocalCurrency = CurrencyConverter.ConvertExact(new Money(JI_UnitPrice, LinePriceRefCurrency), LocalCurrency, roundToDestinationCurrencyDecimals: false).Amount;
				JI_PMV = unitPriceInLocalCurrency * JI_ValuationMarkup / 100;
			}
			else
			{
				JI_PMV = JI_InvoiceQuantity.IsEmpty ? 0 : JI_TotalPMV / JI_InvoiceQuantity;
			}
		}
	}

	void CalculateTotalPMV()
	{
		if (IsExport)
		{
			JI_TotalPMV = JI_PMV * JI_InvoiceQuantity;
		}
	}

	void CalculatePMVAndTotalPMV()
	{
		CalculatePMV();
		CalculateTotalPMV();
	}

	[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.JobWorkNotificationNoList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|JI_JobWorkNotificationNo", Caption = "Job Work Notification No.", ShortCaption = "Notification No.", MediumCaption = "Job Work Notif. No.")]
	public override ZString JI_JobWorkNotificationNo { get => base.JI_JobWorkNotificationNo; set => base.JI_JobWorkNotificationNo = value; }

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new string[] { Declaration?.JE_MessageType ?? ZString.Empty };

	HiddenTextNote AccessoryDescriptionNote => accessoryDescriptionNote ?? (accessoryDescriptionNote = new HiddenTextNote(this, PredefinedNoteTypes.Instance.AccessoryDescription.Description));
	HiddenTextNote accessoryDescriptionNote;

	public ZBool AccessoryDescription_ReadOnly => !(JI_AccessoryStatus == Constants.AccessoryStatus._1 || JI_AccessoryStatus == Constants.AccessoryStatus._2);

	[ReadOnlyMember(nameof(AccessoryDescription_ReadOnly))]
	[MaxLength(Schema.AccessoryDescriptionMaxLength)]
	[ResourceStringData("Enterprise.Customs.IN.Business.JobComInvoiceLine|AccessoryDescription", Caption = "Accessory Description", MediumCaption = "Accessory Desc.", ShortCaption = "Acsry. Desc.")]
	public ZString AccessoryDescription
	{
		get => AccessoryDescriptionNote.Text;
		set
		{
			AccessoryDescriptionNote.SetNoteText(this, AccessoryDescriptionInfo, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateAccessoryDescription();
			}
		}
	}

	public ZPropertyInfo AccessoryDescriptionInfo => GetZPropertyInfo(Schema.AccessoryDescription);

	ZDateTime IDateOfValuationProvider.DateOfValuation => EffectiveAssessmentDate;
}
