using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill : EU.H7.Business.AsycudaBill,
		Integration.Customs.IEH7.IAsycudaBill,
		IAISMessageAttacheeWithEmailLogic,
		IRequestedDocumentsProvider,
		ILRNProvider
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_ShipmentType = SubStyleCodeList.Codes.NormalDeclaration;
			ABL_SellerRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
		}

		protected override (ZString RegNumber, ZString RegNumberType) GetPartyOrgAddressRegNoAndType(OrgAddress address, AsycudaBillAddress.AddressType addressType, ZString[] regNoTypes)
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return GetConsigneeAddressRegNoAndType(address, regNoTypes);
				case AsycudaBillAddress.AddressType.Shipper:
					return GetShipperAddressRegNoAndType(address);
				default:
					return base.GetPartyOrgAddressRegNoAndType(address, addressType, regNoTypes);
			}
		}

		(ZString RegNumber, ZString RegNumberType) GetConsigneeAddressRegNoAndType(OrgAddress address, ZString[] regNoTypes)
		{
			var (regNumber, regNumberType) = GetRegNoAndTypeForParty(address, regNoTypes);
			if (regNumberType == ImporterIdentificationTypes.Codes.EOR)
			{
				regNumber = CountryCodes.Ireland + regNumber;
			}
			else if (regNumber.IsEmpty && regNumberType.IsEmpty)
			{
				var orgCusCode = address?.Header?.CustomsCodes?.GetOrgCusCodesForCodeIgnoringCountry(ImporterIdentificationTypes.Codes.EOR).FirstOrDefault();
				if (orgCusCode != null)
				{
					(regNumber, regNumberType) = (orgCusCode.OK_RN_NKCodeCountry + orgCusCode.OK_CustomsRegNo, orgCusCode.OK_CodeType);
				}
			}
			return (regNumber, regNumberType);
		}

		public override ZString[] ConsigneeRegNoTypes()
		{
			return new ZString[]
			{
				ImporterIdentificationTypes.Codes.EOR,
				ImporterIdentificationTypes.Codes.CGT,
				ImporterIdentificationTypes.Codes.ITX,
				ImporterIdentificationTypes.Codes.PYE
			};
		}

		(ZString RegNumber, ZString RegNumberType) GetShipperAddressRegNoAndType(OrgAddress address)
		{
			var eoriCode = address?.Header.GetEORI(CountryCodes.Ireland, ignoreCountryOfIssuanceIfNotMatched: true) ?? ZString.Empty;
			ZString regNoType = ZString.Empty;
			if (!eoriCode.IsEmpty)
			{
				regNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			}
			return (eoriCode, regNoType);
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SubStyleList))]
		[ResourceStringData("Enterprise.Customs.IE.H7.Business.AsycudaBill|ABL_ShipmentType", Caption = "Additional Declaration Type", ShortCaption = "Add. Decl. Type", FullDescription = "[11 02 001 000] Additional Declaration Type")]
		public override ZString ABL_ShipmentType
		{
			get => base.ABL_ShipmentType;
			set => base.ABL_ShipmentType = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ImporterIdentificationTypeList))]
		[ResourceStringData("Enterprise.Customs.IE.H7.Business.AsycudaBill|ABL_ConsigneeRegNoType", Caption = "ID NO. Type")]
		public override ZString ABL_ConsigneeRegNoType
		{
			get => base.ABL_ConsigneeRegNoType;
			set => base.ABL_ConsigneeRegNoType = value;
		}

		[ResourceStringData("Enterprise.Customs.IE.H7.Business.AsycudaBill|ABL_BillStatus", Caption = "Customs Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BillStatusList))]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set => base.ABL_BillStatus = value;
		}

		[ResourceStringData("Enterprise.Customs.IE.H7.Business.AsycudaBill|ABL_MessageStatus", Caption = "Message Status")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.MessageStatusList))]
		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set => base.ABL_MessageStatus = value;
		}

		#endregion

		public new ValidationConfiguration ValidationConfiguration => (ValidationConfiguration)base.ValidationConfiguration;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override EU.H7.Business.ValidationConfiguration GetNewValidationConfiguration() => new ValidationConfiguration();

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override void CalculateShipmentTypeCore(ASYCUDA.Business.AsycudaBill bill)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (!string.Equals(ABL_RX_NKTransportValueCurrency, ABL_RX_NKInsuranceValueCurrency, StringComparison.OrdinalIgnoreCase))
			{
				if (ABL_InsuranceValue != 0)
				{
					var insuranceMoney = new Money(ABL_InsuranceValue, new Currency(ABL_RX_NKInsuranceValueCurrency));
					var convertedInsuranceMoney = Header.CurrencyConverter.ConvertRounded(insuranceMoney, new Currency(ABL_RX_NKTransportValueCurrency));
					if (convertedInsuranceMoney.Amount != 0)
					{
						ABL_InsuranceValue = convertedInsuranceMoney.Amount;
						ABL_RX_NKInsuranceValueCurrency = ABL_RX_NKTransportValueCurrency;
					}
				}
				else
				{
					ABL_RX_NKInsuranceValueCurrency = ABL_RX_NKTransportValueCurrency;
				}
			}
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		public new EU.H7.Business.IAdditionalDocumentCollection<AdditionalDocument> AdditionalDocuments => (EU.H7.Business.IAdditionalDocumentCollection<AdditionalDocument>)base.AdditionalDocuments;

		public new EU.H7.Business.ISupportingDocumentCollection<SupportingDocument> SupportingDocuments => (EU.H7.Business.ISupportingDocumentCollection<SupportingDocument>)base.SupportingDocuments;

		public new EU.H7.Business.IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (EU.H7.Business.IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override EU.H7.Business.IAdditionalDocumentCollection<EU.H7.Business.AdditionalDocument> CreateNewAdditionalDocumentCollection() => new EU.H7.Business.AdditionalDocumentCollection<AdditionalDocument>(this);

		protected override EU.H7.Business.ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new EU.H7.Business.SupportingDocumentCollection<SupportingDocument>(this);

		protected override EU.H7.Business.IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new EU.H7.Business.PreviousDocumentCollection<PreviousDocument>(this);

		protected override IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

		#region ICusSupportingInfoTypeSupporter

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument] = typeof(RequestedDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			result[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return result;
		}

		#endregion

		public string GetLRNAndSetIfNeeded()
		{
			return new LRNGenerator(Factory, Header.Branch).GetLRNAndSetIfNeeded((ZPropertyInfoString)LocalReferenceNumberInfo, () => false);
		}

		#region IAISMessageAttacheeWithEmailLogic

		IRelatedJob IMessageAttachee.RelatedJob => Header;

		GlbBranch IMessageAttachee.Branch => Header.Branch;

		GlbStaff IMessageAttachee.CustomsAgent => Header.CustomsAgent;

		IRequestedDocumentsProvider IAISMessageAttachee.RequestedDocumentsProvider => this;

		ZString IMessageAttachee.LogicalStatus
		{
			get => ABL_MessageStatus;
			set => ABL_MessageStatus = value;
		}
		ZString IMessageAttachee.EntryStatus
		{
			get => ABL_BillStatus;
			set => ABL_BillStatus = value;
		}

		IEnumerable<EDIMessage> IMessageAttachee.Messages => Messages.OfType<EDIMessage>();

		public void MovementReferenceNumberSetter(ZString mrn, ZDateTime? issueDate = null, ZString? entryStatus = null, ZDateTime? expiryDate = null)
		{
			MovementReferenceNumber = mrn;

			if (issueDate.HasValue)
			{
				MRNEntryNum.CE_IssueDate = issueDate.Value;
			}

			if (entryStatus.HasValue)
			{
				cusEntryNumber.CE_EntryStatus = entryStatus.Value;
			}

			if (expiryDate.HasValue)
			{
				cusEntryNumber.CE_ExpiryDate = expiryDate.Value;
			}
		}

		ABLEntryNum MRNEntryNum => CusEntryNumber;

		void IAISMessageAttachee.SetSimplifiedDeclarationMRN(ZString mrn)
		{
		}

		void IAISMessageAttachee.SetCustomsRegistrationNumber(ZString crn)
		{
		}

		void IAISMessageAttachee.SetEntryReleaseDate(ZDateTime releaseDate)
		{
			ABL_ReleaseDate = releaseDate.Date;
		}

		void IAISMessageAttachee.PopulateConfirmedDutiesAndTaxes(IEnumerable<IGoodsItemProvider> goodsItems)
		{
		}

		bool IAISMessageAttacheeWithEmailLogic.ShouldSendEmailNotification(EDIMessage message) => false;

		#endregion
	}
}
