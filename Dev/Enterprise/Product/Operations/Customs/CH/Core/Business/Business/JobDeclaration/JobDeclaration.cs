using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class JobDeclaration : AutoCHJobDeclaration
	, Integration.Customs.CH.IJobDeclaration
	, IMessageSendingDeclaration
	, ISupportMultipleResourceStringData
	, IDateOfValuationProvider
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new class Schema : AutoCHJobDeclaration.Schema
	{
		public const string SelectionResult = nameof(JobDeclaration.SelectionResult);
		public const string SelectionResultDescription = nameof(JobDeclaration.SelectionResultDescription);
	}

	public new JobDeclaration Clone() => (JobDeclaration)base.Clone();

	[ChildEditable(true)]
	public new ICusContainerCollection<CusContainer> CusContainers => (ICusContainerCollection<CusContainer>)base.CusContainers;

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	[ChildEditable(true)]
	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.EntryStatusList))]
	public new InvoiceLineViewCollection FilteredInvoiceLines => (InvoiceLineViewCollection)base.FilteredInvoiceLines;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	[ChildEditable(true)]
	public new BillCollection<Bill, JobDeclaration> Bills => (BillCollection<Bill, JobDeclaration>)base.Bills;

	public new ActiveCusEntryHeaderCollection ActiveEntryHeaders => (ActiveCusEntryHeaderCollection)base.ActiveEntryHeaders;

	public override ZBool AreMultipleEntryInstructionsAllowed => !IsExportDeclarationActivation;

	#region Properties

	public override ZGuid JE_GB
	{
		get => base.JE_GB;
		set
		{
			var oldValue = JE_GB;
			base.JE_GB = value;
			if (oldValue != value && !IsCopying)
			{
				InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				Packages.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid JE_GC
	{
		get => base.JE_GC;
		set
		{
			var oldValue = JE_GC;
			base.JE_GC = value;
			if (oldValue != value && !IsCopying)
			{
				InvoiceLines.MarkAsNeedingValidationIncludingChildren();
				Packages.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString DeclarationNumber
	{
		get => base.DeclarationNumber;
		set
		{
			if (IsExportDeclarationActivation)
			{
				if (!IsCopying && DeclarationNumber != value)
				{
					(ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault() ?? ActiveEntryHeaders.AddNew()).EntryNumber = value;
					HasChanges = true;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarationNumber();
				}
			}
		}
	}

	public override ZString JE_MessageType
	{
		get => base.JE_MessageType;
		set
		{
			var oldValue = JE_MessageType;
			var oldIsImport = IsImport;
			base.JE_MessageType = value;
			if (!IsCopying && oldValue != JE_MessageType)
			{
				Validation.ValidateJE_TransportMode();
				Invoices.MarkAsNeedingValidation();
				Packages.MarkAsNeedingValidation();

				CustomsEntryInstructions.RefreshMaxCount();
				CustomsEntryInstructions.Cast<CusEntryInstruction>().ForEach(x => x.MarkAsNeedingValidation());

				var shouldRebuidAdditionalTaxes = !oldIsImport && IsImport;

				InvoiceLines?.Cast<JobComInvoiceLine>().ForEach(x =>
				{
					x.NotifyCustomsOffices.MarkAsNeedingValidation();

					if (shouldRebuidAdditionalTaxes)
					{
						x.AdditionalTaxes.RebuildAdditionalTaxes();
					}

					if (x.Vehicles is CusVehicleCollection vehicleCollection)
					{
						vehicleCollection.RefreshMaxCount();
						vehicleCollection.MarkAsNeedingValidation();
					}

					x.Tobaccos.RefreshMaxCount();
					x.Tobaccos.MarkAsNeedingValidation();
				});
			}
		}
	}

	protected override void JE_MessageTypeChanged(ZString oldValue, ZString newValue)
	{
		base.JE_MessageTypeChanged(oldValue, newValue);
		SetDefaultVehicleType();
		SetDefaultConsignee();
		SetDefaultPaymentMethods();
		SetDefaultLocationOfGoodsExport();
		SetDefaultLocationOfGoodsImport(true);
	}

	[ResourceStringData("CH.JobDeclaration.JE_MessageSubType", Caption = "Activation Type")]
	public override ZString JE_MessageSubType { get => base.JE_MessageSubType; set => base.JE_MessageSubType = value; }

	public override ZString JE_TransportMode
	{
		get => base.JE_TransportMode;
		set
		{
			var oldValue = JE_TransportMode;
			base.JE_TransportMode = value;
			if (oldValue != value && !IsCopying)
			{
				SetDefaultVehicleType();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_VehicleType();
				Validation.ValidateJE_VesselName();
				Validation.ValidateJE_RN_NKTransportNationality();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_VehicleType", Caption = "Vehicle Type")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportationTypeList))]
	public override ZString JE_VehicleType
	{
		get => base.JE_VehicleType;
		set => base.JE_VehicleType = value;
	}

	void SetDefaultVehicleType()
	{
		if (IsImport && IsRoad)
		{
			if (JE_VehicleType.IsEmpty)
			{
				JE_VehicleType = UniversalReferenceConstants.TransportationTypeCodes.Truck;
			}
		}
		else
		{
			JE_VehicleType = ZString.Empty;
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_CustomsOffice", Caption = "Customs Office")]
	[MaxLength(8)]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.CustomsOffices))]
	public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

	[ResourceStringData("CH.JobDeclaration.JE_OA_DeclarantAddress", Caption = "Declarant")]
	public override ZGuid JE_OA_DeclarantAddress
	{
		get => base.JE_OA_DeclarantAddress;
		set
		{
			base.JE_OA_DeclarantAddress = value;
			SetDefaultLocationOfGoodsExport();

			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_OA_Representative", Caption = "Authorized Consignee", MediumCaption = "Auth. Consignee", ShortCaption = "Auth. Cons.", FullDescription = "The freight forwarder is working as authorized consignee for all his import shipments to avoid import handling at the border.")]
	public override ZGuid JE_OA_Representative
	{
		get => base.JE_OA_Representative;
		set
		{
			var oldValue = JE_OA_Representative;
			base.JE_OA_Representative = value;
			if (!IsCopying && oldValue != value)
			{
				SetDefaultLocationOfGoodsImport(true);
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_PaymentMethod", Caption = "Duty paid by", ShortCaption = "Duty paid")]
	[MaxLength(3)]
	public override ZString JE_PaymentMethod
	{
		get => base.JE_PaymentMethod;
		set
		{
			base.JE_PaymentMethod = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_LocationOfGoods", Caption = "Goods Location", ShortCaption = "Goods Loc.")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.AuthorizationsList))]
	public override ZString JE_LocationOfGoods
	{
		get => base.JE_LocationOfGoods;
		set
		{
			var oldValue = base.JE_LocationOfGoods;
			base.JE_LocationOfGoods = value;
			if (!IsCopying && oldValue != value)
			{
				SetDefaultCustomsOffice();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_DeclarationLanguage", Caption = "Language", ShortCaption = "Lang.")]
	public override ZString JE_DeclarationLanguage { get => base.JE_DeclarationLanguage; set => base.JE_DeclarationLanguage = value; }

	[ResourceStringData("CH.JobDeclaration.JE_DispatchCountryConfirmation", FullDescription = "Dispatch Country Confirmation", Caption = "Confirm")]
	public override ZBool JE_DispatchCountryConfirmation
	{
		get => base.JE_DispatchCountryConfirmation;
		set
		{
			var oldValue = JE_DispatchCountryConfirmation;
			base.JE_DispatchCountryConfirmation = value;
			if (!IsValidationSuspended && !IsCopying && oldValue != value)
			{
				Validation.ValidateDispatchCountryCode();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_VATPaidBy", Caption = "VAT paid by", ShortCaption = "VAT paid")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.PaymentPartyList))]
	public override ZString JE_VATPaidBy
	{
		get => base.JE_VATPaidBy;
		set
		{
			var oldValue = JE_VATPaidBy;
			base.JE_VATPaidBy = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
			}
			if (!IsCopying && oldValue != JE_VATPaidBy)
			{
				MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_ClearanceLocation", Caption = "Clearance Location", ShortCaption = "Clearance Loc.")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.ClearanceLocationList))]
	public override ZString JE_ClearanceLocation
	{
		get => base.JE_ClearanceLocation;
		set
		{
			var oldValue = JE_ClearanceLocation;
			base.JE_ClearanceLocation = value;
			if (!IsCopying && oldValue != JE_ClearanceLocation)
			{
				MarkAsNeedingValidation();
				SetDefaultRepresentative();
				SetDefaultLocationOfGoodsImport(false);
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_LocationOfGoods();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_AdditionalDecisionInfo", FullDescription = "Additional Decision Info", Caption = "Assessment decision receipt for EU")]
	public override ZBool JE_AdditionalDecisionInfo
	{
		get => base.JE_AdditionalDecisionInfo;
		set => base.JE_AdditionalDecisionInfo = value;
	}

	public override ZGuid JE_OH_Consignee
	{
		get => base.JE_OH_Consignee;
		set
		{
			base.JE_OH_Consignee = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	public override ZGuid JE_OH_Supplier
	{
		get => base.JE_OH_Supplier; set
		{
			base.JE_OH_Supplier = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	protected override void JE_OH_SupplierChanged(ZGuid oldValue, ZGuid newValue)
	{
		base.JE_OH_SupplierChanged(oldValue, newValue);
		SetDefaultPaymentMethods();

		if (IsExport)
		{
			SetDefaultDeclarationLanguage(Supplier);
		}
	}

	protected override void SupplierDocumentaryAddressChanged(Object sender, EventArgs e)
	{
		if (IsExportOrExportDeclarationActivation && SupplierDocumentaryAddress.E2_Contact.IsEmpty)
		{
			SetDefaultCusContact(SupplierDocumentaryAddress);
		}
	}

	void SetDefaultCusContact(JobDocAddress address)
	{
		var contact = address.Organisation?.Contacts.GetContactForAllocation(OrgConstants.ContactAllocationType.CUS);
		address.E2_Contact = contact != null ? contact.OC_ContactName : ZString.Empty;
	}

	public override ZGuid JE_OH_Importer
	{
		get => base.JE_OH_Importer;
		set
		{
			base.JE_OH_Importer = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	public override ZGuid JE_OH_Forwarder
	{
		get => base.JE_OH_Forwarder;
		set
		{
			base.JE_OH_Forwarder = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_PaymentMethod();
				Validation.ValidateJE_VATPaidBy();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_OH_Buyer", Caption = "Buyer")]
	public override ZGuid JE_OH_Buyer { get => base.JE_OH_Buyer; set => base.JE_OH_Buyer = value; }

	[ResourceStringData("CH.JobDeclaration.JE_SpecificCircumstanceIndicator", Caption = "Specific Circumstance Indicator", ShortCaption = "Spec. Circumstance")]
	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.SpecificCircumstanceIndicatorList))]
	public override ZString JE_SpecificCircumstanceIndicator
	{
		get => base.JE_SpecificCircumstanceIndicator;
		set
		{
			base.JE_SpecificCircumstanceIndicator = value;
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_UCR();
			}
		}
	}

	protected override void JE_OH_ImporterChanged(ZGuid oldValue, ZGuid newValue)
	{
		base.JE_OH_ImporterChanged(oldValue, newValue);
		SetDefaultConsignee();
		SetDefaultPaymentMethods();

		if (IsImport)
		{
			SetDefaultDeclarationLanguage(Importer);
		}
	}

	void SetDefaultConsignee()
	{
		if (IsImport)
		{
			if (JE_OH_Consignee.IsEmpty)
			{
				JE_OH_Consignee = JE_OH_Importer;
			}
		}
	}

	public override ZString JE_ShipmentIncoTerm
	{
		get => base.JE_ShipmentIncoTerm;
		set
		{
			base.JE_ShipmentIncoTerm = value;
			SetDefaultPaymentMethods();
		}
	}

	[ReadOnly(true)]
	public override ZString JE_MergeBy { get => base.JE_MergeBy; set => base.JE_MergeBy = value; }

	[ResourceStringData("CH.JobDeclaration.DispatchCountryCode", Caption = "Dispatch Country")]
	public ZString DispatchCountryCode { get => base.JE_RL_NKPortOfLoading.SubstringSafe(0, 2); }

	public ZPropertyInfo DispatchCountryCodeInfo => GetZPropertyInfo(nameof(DispatchCountryCode));

	[ResourceStringData("CH.JobDeclaration.DutyPaidByAccountNo", Caption = "Duty Account")]
	public ZString DutyPaidByAccountNo => (dutyPaidByAccountNo ?? (dutyPaidByAccountNo = new CachedProperty<ZString>(Factory,
		() => GetPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAD, JE_PaymentMethod)))).Value;
	CachedProperty<ZString> dutyPaidByAccountNo;

	public ZPropertyInfo DutyPaidByAccountNoInfo => GetZPropertyInfo(nameof(DutyPaidByAccountNo));

	[ResourceStringData("CH.JobDeclaration.VATPaidByAccountNo", Caption = "VAT Account")]
	public ZString VATPaidByAccountNo => (vATPaidByAccountNo ?? (vATPaidByAccountNo = new CachedProperty<ZString>(Factory,
		() => GetPaidByAccountNo(OrgCusCode.SwissCodeTypes.CAV, JE_VATPaidBy)))).Value;
	CachedProperty<ZString> vATPaidByAccountNo;

	public ZPropertyInfo VATPaidByAccountNoInfo => GetZPropertyInfo(nameof(VATPaidByAccountNo));

	ZString GetPaidByAccountNo(ZString codeType, ZString paidBy)
	{
		var (addressName, organization) = GetPaymentOrganization(codeType, paidBy);
		return addressName.IsEmpty || organization == null ? ZString.Empty : organization.GetCHCustomsRegNo(codeType);
	}

	public override ZString JE_RL_NKPortOfLoading
	{
		get => base.JE_RL_NKPortOfLoading;
		set
		{
			var oldValue = JE_RL_NKPortOfLoading;
			base.JE_RL_NKPortOfLoading = value;
			if (!IsCopying && oldValue != value)
			{
				if (!IsValidationSuspended)
				{
					Validation.ValidateDispatchCountryCode();
				}
			}
		}
	}

	public override ZString JE_RL_NKPortOfArrival
	{
		get => base.JE_RL_NKPortOfArrival;
		set
		{
			var oldValue = JE_RL_NKPortOfArrival;
			base.JE_RL_NKPortOfArrival = value;
			if (!IsCopying && oldValue != value)
			{
				SetDefaultDestinationCountryCode();
			}
		}
	}

	void SetDefaultDestinationCountryCode()
	{
		if (IsExportOrExportDeclarationActivation)
		{
			if (JE_GoodsDestination.IsEmpty)
			{
				JE_GoodsDestination = JE_RL_NKPortOfArrival.SubstringSafe(0, 2);
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_GoodsDestination", Caption = "Destination Country")]
	public override ZString JE_GoodsDestination
	{
		get => base.JE_GoodsDestination;
		set
		{
			var oldValue = JE_GoodsDestination;
			base.JE_GoodsDestination = value;
			if (!IsCopying && oldValue != JE_GoodsDestination)
			{
				InvoiceLines.MarkAsNeedingValidation();
			}
		}
	}

	public override ZDateTime JE_ExportDate
	{
		get => base.JE_ExportDate;
		set
		{
			var oldValue = JE_ExportDate;
			base.JE_ExportDate = value;
			if (!IsCopying && oldValue != JE_ExportDate)
			{
				Packages.MarkAsNeedingValidation();
				InvoiceLines?.Cast<JobComInvoiceLine>()?.ForEach(x => x.NotifyCustomsOffices.MarkAsNeedingValidation());
			}
		}
	}

	public override ZDate JE_ValuationDate
	{
		get => base.JE_ValuationDate;
		set
		{
			var oldValue = JE_ValuationDate;
			base.JE_ValuationDate = value;
			if (!IsCopying && oldValue != JE_ValuationDate)
			{
				Packages.MarkAsNeedingValidation();
				InvoiceLines?.Cast<JobComInvoiceLine>()?.ForEach(x => x.NotifyCustomsOffices.MarkAsNeedingValidation());
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateJE_UCR();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_UCR", Caption = "Ref.No./UCR")]
	public override ZString JE_UCR
	{
		get => base.JE_UCR;
		set
		{
			var oldValue = JE_UCR;
			base.JE_UCR = value;
			if (oldValue != JE_UCR)
			{
				Invoices.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("CH.JobDeclaration.JE_SubLocationOfGoods", Caption = "Agreed Location Of Goods", ShortCaption = "Agreed Loc. Goods")]
	public override ZString JE_SubLocationOfGoods { get => base.JE_SubLocationOfGoods; set => base.JE_SubLocationOfGoods = value; }

	public override ZString JE_EntryStatus => Factory.GetCached(ref entryStatus, () => GetUnifiedValueOrMultiple(ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_EntryStatus), base.JE_EntryStatus));
	CachedProperty<ZString> entryStatus;

	public override ZString JE_EntryStatusDescription
	{
		get
		{
			var entryStatus = JE_EntryStatus;
			return entryStatus == CommonEntryStatusList.Codes.MultipleEntryStatus
				? CommonEntryStatusList.Descriptions.MultipleEntryStatus
				: Lookups.EntryStatusList.GetDescriptionFromCode(entryStatus);
		}
	}

	public override ZString JE_MessageStatus => Factory.GetCached(ref messageStatus, () => GetUnifiedValueOrMultiple(ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.CH_Status), base.JE_MessageStatus));
	CachedProperty<ZString> messageStatus;

	[ResourceStringData("CH.Business.JobDeclaration.MessageStatusDescription", Caption = "Messaging Status")]
	public override ZString JE_MessageStatusDescription
	{
		get
		{
			return JE_MessageStatus == CommonMessageStatusList.Codes.MultipleMessageStatus
				? CommonMessageStatusList.Descriptions.MultipleMessageStatus
				: Lookups.MessageStatusList.GetDescriptionFromCode(JE_MessageStatus);
		}
	}

	public ZString SelectionResult => Factory.GetCached(ref selectionResult, () => GetUnifiedValueOrMultiple(ActiveEntryHeaders.Cast<CusEntryHeader>().Select(entry => entry.SelectionResult)));
	CachedProperty<ZString> selectionResult;

	public ZPropertyInfo SelectionResultInfo => GetZPropertyInfo(Schema.SelectionResult);

	[ResourceStringData("CH.Business.JobDeclaration|SelectionResultDescription", Caption = "Selection Result")]
	public ZString SelectionResultDescription
	{
		get
		{
			return SelectionResult == CommonMessageStatusList.Codes.MultipleMessageStatus
				? CommonMessageStatusList.Descriptions.MultipleMessageStatus
				: Lookups.SelectionResultList.GetDescriptionFromCode(SelectionResult);
		}
	}

	public ZPropertyInfo SelectionResultDescriptionInfo => GetZPropertyInfo(Schema.SelectionResultDescription);

	ZString GetUnifiedValueOrMultiple(IEnumerable<ZString> collection, ZString defaultValue = default)
	{
		using var enumerator = collection.Distinct().GetEnumerator();
		var firstValue = enumerator.MoveNext() ? (enumerator.Current.IsEmpty ? defaultValue : enumerator.Current) : defaultValue;
		return enumerator.MoveNext() ? CommonEntryStatusList.Codes.MultipleEntryStatus : firstValue;
	}
	#endregion

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override bool ShouldKeepDeletedLinesOnAmendmentCore => true;

	public override bool ShouldKeepDeletedLinesOnAmendmentCleared => false;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Switzerland;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("CH"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	public override bool ContainerModeVisible => true;

	public GlbExternalPassword_CHD CHDPassword => CHGlbStaffWrapper.Get(CusAgent)?.CHDPassword;

	public new Event CustomsClearedEventType => base.CustomsClearedEventType;

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		if (!Factory.IsConstructingNullBusinessObject)
		{
			JE_PaymentMethod = ZString.Empty;
			var proxyMainAddress = Branch?.OrgProxy?.MainAddress.PK ?? ZGuid.Empty;
			if (!proxyMainAddress.IsEmpty)
			{
				JE_OA_DeclarantAddress = proxyMainAddress;
			}
		}

		JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
	}

	protected override void DefaultJE_MergeByFromLocalParty(OrgHeader localParty)
	{
	}

	void SetDefaultPaymentMethods()
	{
		if (IsImport)
		{
			SetDefaultPaymentMethod(OrgCusCode.SwissCodeTypes.CAD, JE_PaymentMethodInfo);
			SetDefaultPaymentMethod(OrgCusCode.SwissCodeTypes.CAV, JE_VATPaidByInfo);
		}
	}

	void SetDefaultPaymentMethod(ZString customsCodeType, ZPropertyInfo paymentMethodPropertyInfo)
	{
		if (!JE_ShipmentIncoTerm.IsEmpty && !JE_OH_Supplier.IsEmpty && !JE_OH_Importer.IsEmpty)
		{
			ZString paymentMethod;
			OrgHeader orgHeader;
			if (JE_ShipmentIncoTerm == Core.Constants.IncoTerms.DeliveredDutyPaid)
			{
				paymentMethod = DeclarationPayerList.Codes.Consignor;
				orgHeader = Supplier;
			}
			else
			{
				paymentMethod = DeclarationPayerList.Codes.Importer;
				orgHeader = Importer;
			}
			if (orgHeader == null || orgHeader.GetCHCustomsRegNo(customsCodeType).IsEmpty)
			{
				paymentMethod = DeclarationPayerList.Codes.Declarant;
			}
			paymentMethodPropertyInfo.Value = paymentMethod;
		}
	}

	void SetDefaultDeclarationLanguage(OrgHeader orgHeader)
	{
		var swissCustomsLanguageList = Lookups.DeclarationLanguageList;

		if (JE_DeclarationLanguage.IsEmpty)
		{
			var language = ReturnLanguageIfSwissCustomsContainsIt(orgHeader?.OH_Language.SubstringSafe(0, 2) ?? ZString.Empty);

			if (language == ZString.Empty)
			{
				language = ReturnLanguageIfSwissCustomsContainsIt(GlbCompany.CurrentCompany.OrgProxy.OH_Language.SubstringSafe(0, 2));
			}

			if (language == ZString.Empty)
			{
				language = ReturnLanguageIfSwissCustomsContainsIt(GlbStaff.CurrentUser.GS_WorkingLanguage.SubstringSafe(0, 2));
			}

			JE_DeclarationLanguage = language.IsEmpty ? SwissCustomsLanguageList.Codes.German : language;
		}

		ZString ReturnLanguageIfSwissCustomsContainsIt(ZString language) => swissCustomsLanguageList.ContainsCode(language) ? language : ZString.Empty;
	}

	void SetDefaultLocationOfGoodsExport()
	{
		if (IsExportOrExportDeclarationActivation && JE_LocationOfGoods.IsEmpty)
		{
			if (Lookups.AuthorizationsList.Count == 1)
			{
				JE_LocationOfGoods = Lookups.AuthorizationsList[0].Code;
			}
		}
	}

	void SetDefaultLocationOfGoodsImport(bool overwrite)
	{
		if (IsImportDomicile)
		{
			var lookup = Lookups.AuthorizationsList;
			if (lookup.Count == 1)
			{
				JE_LocationOfGoods = lookup[0].Code;
			}
			else if (overwrite)
			{
				JE_LocationOfGoods = ZString.Empty;
			}
		}
	}

	void SetDefaultRepresentative()
	{
		if (IsImportDomicile && JE_OA_Representative.IsEmpty)
		{
			JE_OA_Representative = JE_OA_DeclarantAddress;
		}
	}

	public void SetDefaultCustomsOffice()
	{
		if (IsImportDomicile)
		{
			var rulePK = Lookups.AuthorizationsList[JE_LocationOfGoods]?.PK as ZGuid? ?? ZGuid.Empty;
			var rule = Factory.Load<CusAuthorisationRule>(rulePK);
			JE_CustomsOffice = rule?.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice)?.CPR_ValueFrom ?? ZString.Empty;
		}
	}

	public bool IsRelocationProcedureOrProcessingTraffic => (isRelocationProcedureOrProcessingTraffic ?? (isRelocationProcedureOrProcessingTraffic = new CachedProperty<bool>(Factory,
		() => InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.JI_ZZF_NKTaxType == UniversalReferenceConstants.TaxCodes.RelocationProcedure || l.JI_ZZF_NKTaxType == UniversalReferenceConstants.TaxCodes.ProcessingTraffic)))).Value;
	CachedProperty<bool> isRelocationProcedureOrProcessingTraffic;

	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

	protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;

	protected override ICusContainerCollection<BaseCusContainer> NewCusContainersCollection() => new BaseCusContainerCollection<CusContainer>(this, Factory);

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	protected override Customs.Business.JobDeclarationValidation GetNewValidation()
	{
		if (IsImport)
		{
			return new ImportJobDeclarationValidation(this);
		}
		if (IsExportOrExportDeclarationActivation)
		{
			return new ExportJobDeclarationValidation(this);
		}
		return new JobDeclarationValidation(this);
	}

	protected override Customs.Business.JobDeclarationLookups GetNewLookups() => new JobDeclarationLookups(this);

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new InvoiceLineViewCollection(this);

	protected override IBillCollection<Customs.Business.Bill, BaseJobDeclaration> CreateNewBillCollection() => new BillCollection<Bill, JobDeclaration>(this, Factory);

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	protected override Customs.Business.ActiveCusEntryHeaderCollection GetActiveEntryHeaderCollection() => new ActiveCusEntryHeaderCollection(this);

	public override Customs.Business.SupportingDocSendingObject GetSupportingDocSendingObject() => new SupportingDocSendingObject(this);

	protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

	protected override bool DoMergeCore(ISendsMessagesToCustoms notifier)
	{
		return !WasExportDeclaration && !IsExportDeclarationActivation && base.DoMergeCore(notifier);
	}

	#region Packing Groups/ Packages

	[ChildEditable(true)]
	public new DeclarationLevelPackingGroupCollection PackingGroups => (DeclarationLevelPackingGroupCollection)base.PackingGroups;

	protected override BaseDeclarationLevelPackingGroupCollection CreateNewPackingGroups() => new DeclarationLevelPackingGroupCollection(this);

	[ChildEditable(true)]
	public new BaseDeclarationLevelPackageCollection<Package> Packages => (BaseDeclarationLevelPackageCollection<Package>)base.Packages;

	protected override IDeclarationLevelPackageCollection<BasePackage> GetPackagesCollection() => new BaseDeclarationLevelPackageCollection<Package>(this);

	#endregion

	[ChildEditable(true)]
	[ChildEditableTestExclude]
	[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
	public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;

	public (ZString AddressName, OrgHeader Organization) GetPaymentOrganization(ZString codeType, ZString paidBy)
	{
		switch (paidBy)
		{
			case DeclarationPayerList.Codes.Cash:
				return (ZString.Empty, null);
			case DeclarationPayerList.Codes.Consignee:
				return (JE_OH_ConsigneeInfo.HumanReadableName, IntermConsignee);
			case DeclarationPayerList.Codes.Consignor:
				return (JE_OH_SupplierInfo.HumanReadableName, Supplier);
			case DeclarationPayerList.Codes.Importer:
				return (JE_OH_ImporterInfo.HumanReadableName, Importer);
			case DeclarationPayerList.Codes.Forwarder:
				return (JE_OH_ForwarderInfo.HumanReadableName, Forwarder);
			case DeclarationPayerList.Codes.Declarant:
				var declarantAddress = DeclarantAddress?.Header;
				if (declarantAddress != null && !declarantAddress.GetCHCustomsRegNo(codeType).IsEmpty)
				{
					return (JE_OA_DeclarantAddressInfo.HumanReadableName, declarantAddress);
				}
				else
				{
					return (Res.GetString("E7087175-C0B3-4729-934D-598CE629F3D1", "Current Company"), GlbCompany.CurrentCompany.OrgProxy);
				}
			default:
				return (ZString.Empty, null);
		}
	}

	public ZString SupplierCTPNumber => (supplierCTPNumber ?? (supplierCTPNumber = new CachedProperty<ZString>(Factory,
		() => Supplier?.GetCHCustomsRegNo(OrgCusCode.SwissCodeTypes.CTP) ?? ZString.Empty))).Value;
	CachedProperty<ZString> supplierCTPNumber;

	protected override void OnFactorySaving()
	{
		base.OnFactorySaving();
		AddDefaultEntryInstructionIfRequired();
		AddOrRemoveCusEntryHeaderForEDA();
	}

	public CusEntryInstruction AddDefaultEntryInstructionIfRequired()
	{
		CusEntryInstruction entryInstruction = null;

		if (IsPersistent && CustomsEntryInstructions.Count == 0)
		{
			if (IsImport)
			{
				entryInstruction = CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = UniversalReferenceConstants.DeclarationTypeCodes.Definitive;
				entryInstruction.CEI_SubStyle = UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms;
			}
			else if (IsExportOrExportDeclarationActivation)
			{
				entryInstruction = CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Procedure = UniversalReferenceConstants.ProcedureCodesPassar.ExportFromFreeCirculation;

				if (IsExportDeclarationActivation)
				{
					entryInstruction.CEI_SubStyle = UniversalReferenceConstants.DeclarationTimeCodes.PresentationToCustoms;
				}
			}
		}

		return entryInstruction;
	}

	void AddOrRemoveCusEntryHeaderForEDA()
	{
		if (IsPersistent)
		{
			if (IsExportDeclarationActivation)
			{
				var entryHeader = CustomsEntryHeaders.FirstOrDefault();
				if (entryHeader == null)
				{
					entryHeader = CustomsEntryHeaders.AddNew();
				}
				else
				{
					entryHeader.AllEntryLines.RemoveAndDeleteAll();
				}
				if (entryHeader.CH_CEI_Instruction.IsEmpty)
				{
					var entryInstruction = CustomsEntryInstructions.Cast<CusEntryInstruction>().FirstOrDefault();
					entryHeader.CH_CEI_Instruction = entryInstruction?.PK ?? ZGuid.Empty;
				}
			}

			if (WasExportDeclaration)
			{
				ActiveEntryHeaders.RemoveAndDeleteAll();
			}
		}
	}

	public bool WasExportDeclaration => JE_MessageTypeInfo.HasChanges && (ZString)JE_MessageTypeInfo.OriginalValue == CHJobMessageTypeList.Codes.ExportDeclarationActivation;

	protected override DocAddressType[] SupportedAddressTypesCore
	{
		get
		{
			var addressTypes = base.SupportedAddressTypesCore.ToList();
			if (IsExportOrExportDeclarationActivation)
			{
				addressTypes.Add(DocAddressType.ConsignorDocumentaryAddress);
			}
			return addressTypes.ToArray();
		}
	}

	#region ConsignorDocAddress

	public JobDocAddress ConsignorDocAddress
	{
		get
		{
			if (consignorDocAddress == null || consignorDocAddress.IsDeleted)
			{
				if (consignorDocAddress != null)
				{
					consignorDocAddress.OrgHeaderAfterChange -= ConsignorAddress_OrgHeaderAfterChange;
				}
				consignorDocAddress = DocAddresses.FindOrCreateWithRequirement(ConsignorDocAddressRequirement);
				consignorDocAddress.OrgHeaderAfterChange += ConsignorAddress_OrgHeaderAfterChange;
			}
			return consignorDocAddress;
		}
	}
	JobDocAddress consignorDocAddress;

	void ConsignorAddress_OrgHeaderAfterChange(object sender, EventArgs e)
	{
		if (IsExportOrExportDeclarationActivation && ConsignorDocAddress.E2_Contact.IsEmpty)
		{
			SetDefaultCusContact(ConsignorDocAddress);
		}
	}

	JobDocAddressRequirement ConsignorDocAddressRequirement
	{
		get
		{
			if (consignorDocAddressRequirement == null)
			{
				consignorDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress, ContactType.Consignor);
				JobDocAddressRequirement jobDocAddressRequirement = consignorDocAddressRequirement;
				consignorDocAddressRequirement.ValidateOrganisationPK = (JobDocAddressRequirement.ValidationDelegate)Delegate.Combine(jobDocAddressRequirement.ValidateOrganisationPK, new JobDocAddressRequirement.ValidationDelegate(ConsignorDocAddressRequirement_ValidateOrganisationPK));
				DocAddressManager.AddRequirement(consignorDocAddressRequirement);
			}
			return consignorDocAddressRequirement;
		}
	}
	JobDocAddressRequirement consignorDocAddressRequirement;

	void ConsignorDocAddressRequirement_ValidateOrganisationPK(JobDocAddressValidation validation)
	{
		Validation.PlausiValidation.CheckNP70172(validation.Parent.OrganisationPKInfo, validation.Parent.Organisation?.MainAddress, DocAddressType.ConsignorDocumentaryAddress);
		Validation.PlausiValidation.CheckNS30003_NotSent(validation.Parent.OrganisationPKInfo, this);
	}

	#endregion

	protected override JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
	{
		switch (addressType)
		{
			case DocAddressType.ConsignorDocumentaryAddress:
				return ConsignorDocAddressRequirement;
		}

		return base.GetDocAddressRequirement(addressType);
	}

	protected override Customs.Business.JobDeclarationSynchroniser GetNewShipmentSynchroniser()
	{
		return new JobDeclarationSynchroniser(this);
	}

	public override ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
	{
		if (addressToValidate.E2_AddressType == DocAddressTypes.Codes.ConsignorDocumentaryAddress)
		{
			return new ConsignorJobDocAddressValidation(addressToValidate, this);
		}
		return base.PiggyBackedDocAddressValidation(addressToValidate);
	}

	public bool IsGoodsDestinationInNCL0147CountryList
	{
		get
		{
			return Factory.GetCachedValue("CH.JobDeclaration.IsGoodsDestinationInNCL0147CountryList." + JE_GoodsDestination, () =>
			{
				var query = new ZDBOnlyQuery(typeof(RefCusTradeGroup));
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				query.AddToFilter(RefCusTradeGroupSchema.ZZA_TradeGroup, Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUForSafetyAndSecurity);

				var subQuery = new ZDBOnlySubQuery(typeof(RefCusTradeGroupCountry), RefCusTradeGroupCountrySchema.ZZB_ZZA_TradeGroup);
				subQuery.AddToFilter(RefCusTradeGroupCountrySchema.ZZB_RN_NKTradeGroupCountryCode, JE_GoodsDestination);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return Factory.LoadTop1<RefCusTradeGroup>(query) != null;
			});
		}
	}

	protected override bool IsDeclarationNumberReadOnly => !IsExportDeclarationActivation;
	public ZBool IsExportDeclarationActivation => JE_MessageType == CHJobMessageTypeList.Codes.ExportDeclarationActivation;
	public ZBool IsExportOrExportDeclarationActivation => IsExport || IsExportDeclarationActivation;
	public ZBool IsExportActivationEdec => IsExportDeclarationActivation && JE_MessageSubType == ActivationTypeList.Codes.Edec;
	public ZBool IsExportActivationPassar => IsExportDeclarationActivation && JE_MessageSubType == ActivationTypeList.Codes.Passar;
	public ZBool IsExportAndOwnPropulsion => IsExportOrExportDeclarationActivation && IsOwnPropulsion;
	public bool IsImportDomicile => IsImport && JE_ClearanceLocation == ClearanceLocation.Domicile;

	IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => new[] { IsExportOrExportDeclarationActivation ? CHJobMessageTypeList.Codes.Export : CHJobMessageTypeList.Codes.Import };

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new JobDeclarationFetchStrategy(this);

	public bool HasTransportDocuments => Factory.GetCached(ref hasTransportDocuments, () => Invoices.Cast<JobComInvoiceHeader>().Any(x => x.TransportDocuments.Any()));
	CachedProperty<bool> hasTransportDocuments;

	JobDeclaration IMessageSendingDeclaration.WrappedDeclaration => this;

	protected override Customs.Business.JobDeclarationDeepCloneStrategy GetTemplateCopyStrategy(BusinessObjectFactory alternateFactory, CloneType cloneType)
	{
		return new JobDeclarationDeepCloneStrategy(this, cloneType);
	}
}
