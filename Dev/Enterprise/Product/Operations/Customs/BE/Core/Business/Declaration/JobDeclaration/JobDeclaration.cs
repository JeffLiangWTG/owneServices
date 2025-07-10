using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobDeclaration : AutoBEJobDeclaration
	, Integration.Customs.BE.IJobDeclaration
{
	public JobDeclaration(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public bool IsExitSummary => JE_MessageType.EqualsIgnoringCase(BEJobMessageTypeList.Codes.ExitSummary);

	public bool IsReExport => JE_MessageType.EqualsIgnoringCase(BEJobMessageTypeList.Codes.ReExport);

	public override ZBool IsExport => IsReExport || IsExitSummary || base.IsExport;

	public new JobDeclarationValidation Validation => (JobDeclarationValidation)base.Validation;

	protected override Customs.Business.JobDeclarationValidation GetNewValidation()
	{
		return IsImport
			? new ImportJobDeclarationValidation(this)
			: IsExport
				? new ExportJobDeclarationValidation(this)
				: new JobDeclarationValidation(this);
	}

	protected override void DecorateDocAddressRequirement(JobDocAddressRequirement requirement, DocAddressType addressType)
	{
		base.DecorateDocAddressRequirement(requirement, addressType);

		switch (addressType)
		{
			case DocAddressType.ImporterDocumentaryAddress:
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
					+= Validation.ValidateImporterDocumentaryAddress;
				break;
			case DocAddressType.SupplierDocumentaryAddress:
				requirement.ValidateDelegateToBeFiredUponValidationOf_E2_OA_Address
					+= Validation.ValidateSupplierDocumentaryAddress;
				break;
		}
	}

	public new JobDeclarationLookups Lookups => (JobDeclarationLookups)base.Lookups;

	protected override Customs.Business.JobDeclarationLookups GetNewLookups()
	{
		return IsImport
			? new ImportJobDeclarationLookups(this)
			: IsExport
				? new ExportJobDeclarationLookups(this)
				: new JobDeclarationLookups(this);
	}

	protected override bool IsLookupsCachedInBase => false;

	public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;

	protected override Customs.Business.InvoiceHeaderActiveCollection CreateNewInvoiceHeaderCollection() => new InvoiceHeaderActiveCollection(this);

	[ChildEditable]
	public new InvoiceHeaderActiveCollection Invoices => (InvoiceHeaderActiveCollection)base.Invoices;

	[ChildEditable(true)]
	public new InvoiceLineCompleteCollection InvoiceLines => (InvoiceLineCompleteCollection)base.InvoiceLines;

	protected override EU.Business.JobDeclarationCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new JobDeclarationCustomsOfficeRequirementHelper(this);

	protected override Customs.Business.InvoiceLineCompleteCollection GetNewInvoiceLineCompleteCollection() => new InvoiceLineCompleteCollection(this);

	[ChildEditable(false)]
	public new IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader> JobComInvoiceGroupHeaders => (IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)base.JobComInvoiceGroupHeaders;

	protected override IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> CreateNewJobComInvoiceGroupHeaderCollection() => new BaseJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>(this);

	protected override IInvoiceLineViewCollection<BaseJobComInvoiceLine> GetNewInvoiceLineViewCollection() => new EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>(this);

	public new IInvoiceLineViewCollection<JobComInvoiceLine> FilteredInvoiceLines => (IInvoiceLineViewCollection<JobComInvoiceLine>)base.FilteredInvoiceLines;

	public new ICusEntryHeaderCollection<CusEntryHeader> CustomsEntryHeaders => (ICusEntryHeaderCollection<CusEntryHeader>)base.CustomsEntryHeaders;

	protected override ICusEntryHeaderCollection<Customs.Business.CusEntryHeader> NewCustomsEntryHeaders() => new EU.Business.Declaration.CusEntryHeaderCollection<CusEntryHeader>(this, Factory);

	[MaxLength(17)]
	[ResourceStringData("B9EC5749-5665-4093-AF1E-D35369EEDAC5", Caption = "Approval Defer No.")]
	public override ZString JE_DefermentAccountNumber
	{
		get => base.JE_DefermentAccountNumber;
		set => base.JE_DefermentAccountNumber = value;
	}

	protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this, new CusEntryInstructionComparer());

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.BELocationOfGoodsList))]
	public override ZString JE_LocationOfGoods
	{
		get => base.JE_LocationOfGoods;
		set
		{
			var hasChanged = JE_LocationOfGoods != value;
			base.JE_LocationOfGoods = value;
			if (hasChanged && !IsCopying)
			{
				var locationQualifierValue = ZString.Empty;

				if (!JE_LocationOfGoods.IsEmpty && Lookups.BELocationOfGoodsList.OfType<ZZRefCusCodeListCombined>().FirstOrDefault(c => c.ZZD_Code == value) is ZZRefCusCodeListCombined codeList)
				{
					locationQualifierValue = codeList.GetAttributesValues(UniversalReferenceConstants.Type).Any(v => v == UniversalReferenceConstants.DALocatie)
						? Constants.LocationQualifiers.A
						: Constants.LocationQualifiers.C;
				}

				JE_LocationQualifier = locationQualifierValue;
			}
		}
	}

	[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.JE_CustomsOfficeList))]
	public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

	[ResourceStringData("131C2B1B-DB0C-4A56-A10C-A368B4C0D2F2", Caption = "[UCC 3/26] Buyer")]
	public override ZGuid JE_OA_ConsigneeAddress
	{
		get => base.JE_OA_ConsigneeAddress;
		set => base.JE_OA_ConsigneeAddress = value;
	}

	public ResourceStringData JE_OA_SellerAddressLabel => IsExport ? Res.GetData("C780E9F9-1158-4345-B4E5-E6CAE4D4DAD5", "[2] Subcontractor") : Res.GetData("1F3D5756-9BA7-4FBF-BEB6-CE96192D2637", "[UCC 3/24] Seller");

	[ResourceStringData("5665404A-B34B-4460-87F6-ED128F52EF09", Caption = "Controlling Customer")]
	public override ZGuid JE_OH_ControllingCustomer { get => base.JE_OH_ControllingCustomer; set => base.JE_OH_ControllingCustomer = value; }

	[ResourceStringData("CD3F4731-3F18-40DE-AF1B-4D240A7F8C61", Caption = "Presentation Date", FullDescription = "[15 08 001 000] Date and Time of the presentation of the goods to customs")]
	public override ZDateTime ZG_PresentationStartDate
	{
		get => base.ZG_PresentationStartDate;
		set => base.ZG_PresentationStartDate = value;
	}

	[MaxLength(1)]
	public override ZString JE_RegionOfDestination
	{
		get => base.JE_RegionOfDestination;
		set => base.JE_RegionOfDestination = value;
	}

	public override ZString JE_MessageType
	{
		get => base.JE_MessageType;
		set
		{
			var oldValue = JE_MessageType;
			base.JE_MessageType = value;
			if (oldValue != JE_MessageType && !IsCopying)
			{
				Invoices.Cast<JobComInvoiceHeader>().ForEach(i =>
				{
					i.JobDeclarationMessageTypeChanged(value);
					var invoiceLines = i.InvoiceLines.Cast<JobComInvoiceLine>();
					invoiceLines.SelectMany(l => l.PreviousDocuments.Cast<PreviousDocument>())
						.ForEach(p => p.JobDeclarationMessageTypeChanged(value));
				});
				SetDefaultRegionOfDestination();
			}
		}
	}

	[ResourceStringData("80EC9920-6DF8-4570-8EC0-ADF952A4A1EA", Caption = "[1a] Entry Style")]
	public override ZString JE_MessageSubType { get => base.JE_MessageSubType; set => base.JE_MessageSubType = value; }

	[ResourceStringData("A0DD5F21-7257-4DA0-B1B4-25747B10A7EE", Caption = "D.V.1?")]
	public override ZBool ZG_IsHighValueOvrd
	{
		get => base.ZG_IsHighValueOvrd;
		set
		{
			base.ZG_IsHighValueOvrd = value;

			if (base.ZG_IsHighValueOvrd)
			{
				if (JE_OA_ConsigneeAddress.IsEmpty && !ImporterDocumentaryAddress.IsEmpty)
				{
					JE_OA_ConsigneeAddress = ImporterDocumentaryAddress.E2_OA_Address;
					JE_OA_ConsigneeAddressInfo.RefreshBinding();
				}
				if (JE_OA_SellerAddress.IsEmpty && !SupplierDocumentaryAddress.IsEmpty)
				{
					JE_OA_SellerAddress = SupplierDocumentaryAddress.E2_OA_Address;
					JE_OA_SellerAddressInfo.RefreshBinding();
				}
			}
		}
	}

	[ResourceStringData("13C48F12-C510-435D-9DDF-841AAE48BFF2", Caption = "Rep. Type", FullDescription = "[UCC 3/21] Rep. Type")]
	public override ZString JE_DeclarantType { get => base.JE_DeclarantType; set => base.JE_DeclarantType = value; }

	[ResourceStringData("AC69BD57-545B-4BBE-A28C-AF19D5E8F373", Caption = "Transport", FullDescription = "[UCC 7/4] Transport")]
	public override ZString JE_TransportMode { get => base.JE_TransportMode; set => base.JE_TransportMode = value; }

	[ResourceStringData("10E939DD-AA31-4CDB-BE03-EF4012FB177E", Caption = "[UCC 2/4] DUCR")]
	public override ZString JE_UCR { get => base.JE_UCR; set => base.JE_UCR = value; }

	[ResourceStringData("F4F70EF6-3718-43F7-A8D4-9E23808CA740", Caption = "[UCC 4/1] Incoterm")]
	public override ZString JE_ShipmentIncoTerm { get => base.JE_ShipmentIncoTerm; set => base.JE_ShipmentIncoTerm = value; }

	[ResourceStringData("08C73D0B-D967-40E8-8F4B-36699943147D", Caption = "Nationality", FullDescription = "[UCC 7/8] Nationality")]
	public override ZString JE_RN_NKTransportNationality { get => base.JE_RN_NKTransportNationality; set => base.JE_RN_NKTransportNationality = value; }

	[ResourceStringData("9218B44A-0EA9-49CF-859D-CDC7E070DD1C", Caption = "Circumstance", FullDescription = "[UCC 1/7] Circumstance")]
	public override ZString ZG_SpecificCircumstanceIndicator { get => base.ZG_SpecificCircumstanceIndicator; set => base.ZG_SpecificCircumstanceIndicator = value; }

	[ResourceStringData("DF0792E0-0485-40C0-B09E-8F9AED179D3A", Caption = "[UCC 7/7] Vessel")]
	public override ZString JE_VesselName { get => base.JE_VesselName; set => base.JE_VesselName = value; }

	protected override bool IsInventorySelectionEnabledCore => IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry;

	protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

	protected override bool IsCustomsLineAmendmentATotalReplacement => false;

	protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Belgium;

	protected override bool HasSplitEntriesCore
	{
		get
		{
			if (!GetType().FullName.Contains("BE"))
			{
				ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
			}
			return base.HasSplitEntriesCore;
		}
	}

	protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => EUCommonConstants.TransportModeSource.InlandTransportMode;

	void SetDefaultRegionOfDestination()
	{
		if (!IsImport)
		{
			ZG_RegionOfDestination = ZString.Empty;
		}
	}
}
