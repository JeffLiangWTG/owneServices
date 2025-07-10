using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public class CusEntryInstruction : AutoCHCusEntryInstruction
	, Integration.Customs.CH.ICusEntryInstruction
	, Integration.Customs.ICusSupportingInfoTypeSupporter
	, ICusReferenceTypeSupporter
	, ICusSupportingInfoParent
	, ISupportingDocumentParent
	, ITransportDocumentParent
	, IDateOfValuationProvider
{
	public CusEntryInstruction(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

	public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

	public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

	protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

	protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

	public ZDateTime DateOfValuation => JobDeclaration?.DateOfValuation ?? ZDateTime.Today;

	protected override bool SupportsCloneCore() => true;

	#region Properties

	public override ZGuid CEI_JE
	{
		get => base.CEI_JE;
		set
		{
			var oldValue = CEI_JE;
			base.CEI_JE = value;
			if (!IsCopying && oldValue != CEI_JE)
			{
				SetDefaultCEI_Style();
			}
		}
	}

	void SetDefaultCEI_Style()
	{
		if (JobDeclaration?.IsExportOrExportDeclarationActivation ?? false)
		{
			CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		}

		if (JobDeclaration?.IsExportDeclarationActivation ?? false)
		{
			CEI_SubStyle = UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms;
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_Style", Caption = "Declaration Type", ShortCaption = "Type")]
	[MaxLength(2)]
	public override ZString CEI_Style
	{
		get => base.CEI_Style;
		set
		{
			var oldValue = CEI_Style;
			base.CEI_Style = value;
			if (!IsCopying && oldValue != CEI_Style)
			{
				SetCEI_Description();
				if (JobDeclaration?.IsImport ?? false)
				{
					ClearCEI_DeclarationReasonIfNotApplicable();
				}
				JobDeclaration?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_SubStyle", Caption = "Declaration Time", ShortCaption = "Time")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
	[MaxLength(2)]
	public override ZString CEI_SubStyle
	{
		get => base.CEI_SubStyle;
		set
		{
			var oldValue = CEI_SubStyle;
			base.CEI_SubStyle = value;
			if (!IsCopying && oldValue != CEI_SubStyle)
			{
				SetCEI_Description();
			}
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_Description", Caption = "Description")]
	public override ZString CEI_Description
	{
		get => base.CEI_Description;
		set
		{
			var oldValue = CEI_Description;
			base.CEI_Description = value;
			if (!IsCopying && oldValue != CEI_Description)
			{
				SetCEI_Description();
			}
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_OH_Owner", Caption = "Owner")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.Owners))]
	[ReadOnlyMember(nameof(CEI_WarehouseTypeIsNotBondedWarehouse))]
	public override ZGuid CEI_OH_Owner
	{
		get => base.CEI_OH_Owner;
		set => base.CEI_OH_Owner = value;
	}

	[ReadOnlyMember(nameof(CEI_WarehouseTypeIsNotBondedWarehouse))]
	public override ZGuid CEI_OA_Warehouse
	{
		get => base.CEI_OA_Warehouse;
		set => base.CEI_OA_Warehouse = value;
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.WarehouseTypeList))]
	public override ZString CEI_WarehouseType
	{
		get => base.CEI_WarehouseType;
		set
		{
			var oldValue = CEI_WarehouseType;
			base.CEI_WarehouseType = value;
			if (!IsCopying && oldValue != CEI_WarehouseType)
			{
				ClearWarehouseAndOwnerIfNotApplicable();
			}
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_DeclarationReason", Caption = "Reason")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.DeclarationReasonList))]
	[ReadOnlyMember(nameof(CEI_DeclarationReason_ReadOnly))]
	public override ZString CEI_DeclarationReason { get => base.CEI_DeclarationReason; set => base.CEI_DeclarationReason = value; }

	[ResourceStringData("CH.CusEntryInstruction.CEI_TransportChargesMethodOfPayment", Caption = "Method of Payment of Transport Charges", ShortCaption = "Transport MoP")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.TransportChargesMethodOfPaymentList))]
	public override ZString CEI_TransportChargesMethodOfPayment { get => base.CEI_TransportChargesMethodOfPayment; set => base.CEI_TransportChargesMethodOfPayment = value; }

	[ResourceStringData("CH.CusEntryInstruction.CEI_NextProcedure", Caption = "Next Procedure", ShortCaption = "Next")]
	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.NextProcedureList))]
	public override ZString CEI_NextProcedure { get => base.CEI_NextProcedure; set => base.CEI_NextProcedure = value; }

	public void ClearWarehouseAndOwnerIfNotApplicable()
	{
		if (CEI_WarehouseTypeIsNotBondedWarehouse)
		{
			CEI_OA_Warehouse_ZAddress.OrgPK = ZGuid.Empty;
			CEI_OH_Owner = ZGuid.Empty;
		}
	}

	public ZBool CEI_DeclarationReason_ReadOnly => CEI_Style != UniversalReferenceConstants.DeclarationTypeCodes.Provisional;

	public ZBool CEI_WarehouseTypeIsNotBondedWarehouse => CEI_WarehouseType != UniversalReferenceConstants.WarehouseTypeCodes.BondedWarehouse;

	void SetCEI_Description()
	{
		if (CEI_Description.IsEmpty && !CEI_Style.IsEmpty && JobDeclaration is JobDeclaration declaration)
		{
			if (declaration.IsImport && !CEI_SubStyle.IsEmpty)
			{
				CEI_Description = ((ZString)(Lookups.StyleList.GetDescriptionFromCode(CEI_Style) + " - " + Lookups.EntrySubStyleList.GetDescriptionFromCode(CEI_SubStyle))).Left(CusEntryInstruction.Schema.CEI_DescriptionMaxLength);
			}
			else if (declaration.IsExportOrExportDeclarationActivation && !CEI_Procedure.IsEmpty)
			{
				CEI_Description = ((ZString)($"{CEI_Procedure} - {Lookups.StyleList.GetDescriptionFromCode(CEI_Style)}")).Left(CusEntryInstruction.Schema.CEI_DescriptionMaxLength);
			}
		}
	}

	void ClearCEI_DeclarationReasonIfNotApplicable()
	{
		if (CEI_DeclarationReason_ReadOnly)
		{
			CEI_DeclarationReason = ZString.Empty;
		}
	}

	[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ProcedureCodeList))]
	[ResourceStringData("CH.CusEntryInstruction.CEI_Procedure", Caption = "Procedure Code")]
	public override ZString CEI_Procedure
	{
		get => base.CEI_Procedure;
		set
		{
			var oldValue = CEI_Procedure;
			base.CEI_Procedure = value;
			if (!IsCopying && oldValue != CEI_Procedure)
			{
				SetCEI_Description();
			}
		}
	}

	[ResourceStringData("CH.CusEntryInstruction.CEI_DateForDuty", Caption = "Assessment Date")]
	[ReadOnly(true)]
	public override ZDateTime CEI_DateForDuty { get => base.CEI_DateForDuty; set => base.CEI_DateForDuty = value; }

	public bool IsProcessingTransaction => CEI_Procedure == UniversalReferenceConstants.ProcedureCodesPassar.ReExportAfterInwardProcessing || CEI_Procedure == UniversalReferenceConstants.ProcedureCodesPassar.OutwardProcessing;

	#endregion

	#region Cached Properties

	[ChildEditable]
	public CusSupplyChainActorReferenceCollection SupplyChainActors
	{
		get
		{
			if (cusSupplyChainActorReferences == null)
			{
				cusSupplyChainActorReferences = new CusSupplyChainActorReferenceCollection(this);
				cusSupplyChainActorReferences.Load();
				RegisterEditableChildObject(cusSupplyChainActorReferences);
			}

			return cusSupplyChainActorReferences;
		}
	}
	CusSupplyChainActorReferenceCollection cusSupplyChainActorReferences;

	#region PreviousDocuments

	[ChildEditable(true)]
	public PreviousDocumentCollection PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				previousDocuments = new PreviousDocumentCollection(this);
				previousDocuments.Load();
				RegisterEditableChildObject(previousDocuments);
			}
			return previousDocuments;
		}
	}
	PreviousDocumentCollection previousDocuments;

	#endregion

	public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
	{
		var result = new Dictionary<ZString, Type>
		{
			{ Common.CH.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.TransportDocument, typeof(TransportDocument) },
			{ Common.CH.CusSupportingInfoTypeList.Codes.AdditionalInformation, typeof(AdditionalInformation) }
		};
		return result;
	}

	public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
	{
		yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
	}

	[ChildEditable]
	public SupportingDocumentCollection SupportingDocuments => supportingDocuments ??= GetSupportingDocuments();
	SupportingDocumentCollection supportingDocuments;

	SupportingDocumentCollection GetSupportingDocuments()
	{
		var result = CreateNewSupportingDocumentCollection();

		result.Load();
		RegisterEditableChildObject(result);

		return result;
	}

	[ChildEditable(true)]
	public AdditionalInformationCollection AdditionalInformations
	{
		get
		{
			if (additionalInformations == null)
			{
				additionalInformations = new AdditionalInformationCollection(this);
				additionalInformations.Load();
				RegisterEditableChildObject(additionalInformations);
			}
			return additionalInformations;
		}
	}
	AdditionalInformationCollection additionalInformations;

	SupportingDocumentCollection CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

	[ChildEditable]
	public TransportDocumentCollection TransportDocuments => transportDocuments ??= GetTransportDocuments();
	TransportDocumentCollection transportDocuments;

	TransportDocumentCollection GetTransportDocuments()
	{
		var transportDocuments = new TransportDocumentCollection(this);
		transportDocuments.Load();
		RegisterEditableChildObject(transportDocuments);
		return transportDocuments;
	}

	public bool AllInvoiceLinesAreReturnedGoods => Factory.GetCached(ref allInvoiceLinesAreReturnedGoods, GetAllInvoiceLinesAreReturnedGoods);
	CachedProperty<bool> allInvoiceLinesAreReturnedGoods;
	bool GetAllInvoiceLinesAreReturnedGoods() => InvoiceLines.Any() && InvoiceLines.All(x => x.JI_Procedure == UniversalReferenceConstants.ProcedureCodesEdec.ReturnedGoods);

	public bool HasPreferentialTariffPreference => Factory.GetCached(ref hasPreferentialTariffPreference, GetHasPreferentialTariffPreference);

	public bool IsGSPCertificateRequired => false;

	public IEnumerable<SupportingDocument> SupportingDocumentsIncludingInherited => SupportingDocuments.Cast<SupportingDocument>();

	public ZDateTime EffectiveAssessmentDate => DateOfValuation;

	public HugeSequenceNumberGenerator AdditionalInformationLineNumberGenerator => null;

	public void ValidateNonTradingGoods() { }

	CachedProperty<bool> hasPreferentialTariffPreference;
	bool GetHasPreferentialTariffPreference() => InvoiceLines.Any(x => x.JI_PrimaryPreference == UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff);

	public bool IsSimplified => CEI_Style == UniversalReferenceConstants.InputControlCodes.Simplified;

	public bool IsOrdinary => CEI_Style == UniversalReferenceConstants.InputControlCodes.Ordinary;

	public ZDecimal TotalPriceInCHF => Factory.GetCached(ref fTotalPriceInCHF, () =>
	{
		var currency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Switzerland);
		ZDecimal totalPriceInCHF = 0;
		foreach (JobComInvoiceLine line in InvoiceLines)
		{
			if (line.InvoiceHeader.JZ_RX_NKInvoice_Currency.IsEmpty)
			{
				return 0;
			}
			totalPriceInCHF += line.CurrencyConverter.ConvertExact(line.JI_LinePriceMoney, currency).Amount;
		}
		return totalPriceInCHF;
	});
	CachedProperty<ZDecimal> fTotalPriceInCHF;

	public ZDecimal TotalWeightInKG => Factory.GetCached(ref fTotalWeightInKG, () => InvoiceLines.Sum(line => new ZWeight(line.JI_Weight, line.JI_WeightUQ).InKilogramsSafe));
	CachedProperty<ZDecimal> fTotalWeightInKG;

	public bool HasAdditionalInformationV1201 => Factory.GetCached(ref hasAdditionalInformationV1201,
		() => AdditionalInformations.Cast<AdditionalInformation>().Any(x => x.CSI_Code == UniversalReferenceConstants.AdditionalInformationTypeCodes.PartialShipmentNumber));
	CachedProperty<bool> hasAdditionalInformationV1201;

	#endregion

	#region ICusReferenceTypeSupporter Members

	IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
	{
		{ CusReferenceTypeList.Codes.SupplyChainActor, typeof(CusSupplyChainActorReference) },
	};

	#endregion

	#region

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new Strategy(this);

	class Strategy : CusEntryInstructionFetchStrategy
	{
		public Strategy(CusEntryInstruction entryInstruction)
			: base(entryInstruction)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
		}
	}

	#endregion

	protected override void OnFactorySaving()
	{
		if (!JobDeclaration?.IsExportOrExportDeclarationActivation ?? false)
		{
			SupportingDocuments.RemoveAndDeleteAll();
			SupplyChainActors.RemoveAndDeleteAll();
			PreviousDocuments.RemoveAndDeleteAll();
			AdditionalInformations.RemoveAndDeleteAll();
			TransportDocuments.RemoveAndDeleteAll();
		}
		else
		{
			foreach (var item in InvoiceLines)
			{
				item.JI_Procedure = CEI_Procedure;
			}
		}

		base.OnFactorySaving();
	}

	public override void Delete()
	{
		if (!IsDeleted)
		{
			SupportingDocuments.RemoveAndDeleteAll();
			SupplyChainActors.RemoveAndDeleteAll();
			PreviousDocuments.RemoveAndDeleteAll();
			AdditionalInformations.RemoveAndDeleteAll();
			TransportDocuments.RemoveAndDeleteAll();
		}

		base.Delete();
	}
}
