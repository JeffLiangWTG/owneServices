using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	[DependentBusinessObject(typeof(AsycudaManifestHeader), nameof(AsycudaManifestHeader.Bills))]
	public class AsycudaBill : EU.H7.Business.AsycudaBill, IESMessageInfoProvider, IESResponseBOMessageStatus
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : EU.H7.Business.AsycudaBill.Schema
		{
			public const string DocumentationRequired = "H7_ES_DocumentationRequired";
			public const int DocumentationRequiredMaxLength = 1;
			public const string DocumentationRequiredDescription = "DocumentationRequiredDescription";
			public const string H7MovementReferenceNumber = "H7MovementReferenceNumber";
			public const string G3LocalReferenceNumber = "G3LocalReferenceNumber";
			public const string G3MovementReferenceNumber = "G3MovementReferenceNumber";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_SellerRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration; 
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public new CusGoodsLocation CusGoodsLocation => (CusGoodsLocation)base.CusGoodsLocation;

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		protected override Type GetPackTypeCore() => typeof(AsycudaPack);

		public new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill> Packs => (ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>)base.Packs;

		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>(this);

		public new IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill> PackedItems => (IAsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>)base.PackedItems;

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>(this);

		protected override ISupportingDocumentCollection<EU.H7.Business.SupportingDocument> CreateNewSupportingDocumentCollection() => new SupportingDocumentCollection<SupportingDocument>(this);

		public new IPreviousDocumentCollection<PreviousDocument> PreviousDocuments => (IPreviousDocumentCollection<PreviousDocument>)base.PreviousDocuments;

		protected override IPreviousDocumentCollection<EU.H7.Business.PreviousDocument> CreateNewPreviousDocumentCollection() => new PreviousDocumentCollection<PreviousDocument>(this);

		protected override (ZString RegNumber, ZString RegNumberType) GetPartyOrgAddressRegNoAndType(OrgAddress address, AsycudaBillAddress.AddressType addressType, ZString[] regNoTypes)
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return GetConsigneeAddressRegNoAndType(address, regNoTypes);
				default:
					return base.GetPartyOrgAddressRegNoAndType(address, addressType, regNoTypes);
			}
		}

		(ZString RegNumber, ZString RegNumberType) GetConsigneeAddressRegNoAndType(OrgAddress address, ZString[] regNoTypes)
		{
			var (regNumber, regNumberType) = GetRegNoAndTypeForParty(address, regNoTypes);
			if (regNumberType == ESH7ImporterIdentificationTypes.Codes.EOR)
			{
				regNumber = CountryCodes.Spain + regNumber;
			}
			else if (regNumber.IsEmpty && regNumberType.IsEmpty)
			{
				var orgCusCode = address?.Header?.CustomsCodes?.GetOrgCusCodesForCodeIgnoringCountry(ESH7ImporterIdentificationTypes.Codes.EOR).FirstOrDefault();
				if (orgCusCode != null)
				{
					(regNumber, regNumberType) = (orgCusCode.OK_RN_NKCodeCountry + orgCusCode.OK_CustomsRegNo, orgCusCode.OK_CodeType);
				}
			}
			return (regNumber, regNumberType);
		}

		public override ZString[] ConsigneeRegNoTypes() =>
			[
				ESH7ImporterIdentificationTypes.Codes.EOR,
				ESH7ImporterIdentificationTypes.Codes.NIF,
				ESH7ImporterIdentificationTypes.Codes.TIN
			];

		protected override bool GetIsDocumentationRequested()
		{
			return DocumentationRequired == ESH7DocumentationRequiredList.Codes.Yes;
		}

		public bool HasAcceptedG3DMessage()
		{
			return !Header.G3MRNToRevoke.IsEmpty
				&& Common.CusEntryNumber.Load<ABLEntryNum>(this, CusEntryNumberTypes.Standard.MovementReferenceNumber, Header.AMA_RN_NKCountry, G3DeclarationType)?.CE_EntryNum is ZString billMRN
				&& billMRN == Header.G3MRNToRevoke;
		}

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var cusSupportingInfoTypes = base.GetCusSupportingInfoTypesCore();
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.SupportingDocument] = typeof(SupportingDocument);
			cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.PreviousDocument] = typeof(PreviousDocument);
			return cusSupportingInfoTypes;
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ImporterIdentificationTypeList))]
		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill|ABL_ConsigneeRegNoType", Caption = "ID NO. Type")]
		public override ZString ABL_ConsigneeRegNoType
		{
			get => base.ABL_ConsigneeRegNoType;
			set => base.ABL_ConsigneeRegNoType = value;
		}

		protected override ABLEntryNum LoadOrCreateLRNEntryNumber(bool createIfMissing)
		{
			return createIfMissing
				? Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, Header.AMA_RN_NKCountry, string.Empty)
				: Common.CusEntryNumber.Load<ABLEntryNum>(this, CusEntryNumberTypes.EU.LocalReferenceNumber, Header.AMA_RN_NKCountry, string.Empty);
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill.G3LocalReferenceNumber", Caption = "G3 Local Reference Number", ShortCaption = "LRN (G3)", MediumCaption = "LRN (G3)", FullDescription = "A system-generated local reference number to uniquely identify each single G3 declaration.")]
		[MaxLength(35)]
		public ZString G3LocalReferenceNumber
		{
			get => G3LRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var currentG3LRNEntryNumber = G3LRNEntryNumber;
				var oldValue = currentG3LRNEntryNumber?.CE_EntryNum ?? ZString.Empty;
				if (oldValue != value)
				{
					if (currentG3LRNEntryNumber == null)
					{
						currentG3LRNEntryNumber = LoadEntryNumber(CusEntryNumberTypes.EU.LocalReferenceNumber, G3DeclarationType, true);
					}

					currentG3LRNEntryNumber.CE_EntryNum = value;
					G3LocalReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo G3LocalReferenceNumberInfo => GetZPropertyInfo(nameof(G3LocalReferenceNumber));

		public ZString ClearanceReferenceNumber => LoadEntryNumber(CusEntryNumberTypes.Spain.ClearanceCSV, H7MessageType)?.CE_EntryNum ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.DocumentationRequired))]
		[MaxLength(Schema.DocumentationRequiredMaxLength)]
		public ZString DocumentationRequired
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.DocumentationRequired);
			set
			{
				var oldValue = DocumentationRequired;
				if (oldValue != value)
				{
					CheckMaximumLength(DocumentationRequiredInfo, value);
					this.SetSystemDefinedValue(Schema.DocumentationRequired, value);
					DocumentationRequiredInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DocumentationRequiredInfo => GetZPropertyInfo(nameof(DocumentationRequired));

		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill.DocumentationRequiredDescription", Caption = "Documentation Required", FullDescription = "Indicates if documentation is required.", MediumCaption = "Doc. Required", ShortCaption = "Doc. Req.")]
		public ZString DocumentationRequiredDescription
		{
			get { return Lookups.DocumentationRequired.GetDescriptionFromCode(DocumentationRequired); }
		}

		public ZPropertyInfo DocumentationRequiredDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentationRequiredDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.AdditionalProcedureList))]
		public override ZString ABL_Procedure
		{
			get => base.ABL_Procedure;
			set => base.ABL_Procedure = value;
		}

		ABLEntryNum G3LRNEntryNumber
		{
			get
			{
				if (g3LrnEntryNumber == null || g3LrnEntryNumber.IsDeleted)
				{
					g3LrnEntryNumber = LoadEntryNumber(CusEntryNumberTypes.EU.LocalReferenceNumber, G3DeclarationType);
				}

				return g3LrnEntryNumber;
			}
		}

		ABLEntryNum g3LrnEntryNumber;

		ABLEntryNum LoadEntryNumber(ZString entryType, ZString entryLineReference, bool createIfMissing = false)
		{
			return createIfMissing ? Common.CusEntryNumber.LoadOrCreate<ABLEntryNum>(this, entryType, Header.AMA_RN_NKCountry, entryLineReference)
				: Common.CusEntryNumber.Load<ABLEntryNum>(this, entryType, Header.AMA_RN_NKCountry, entryLineReference);
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill.G3MovementReferenceNumber", Caption = "G3 Movement Reference Number", ShortCaption = "MRN (G3)", MediumCaption = "MRN (G3)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		[MaxLength(35)]
		public ZString G3MovementReferenceNumber
		{
			get => G3MrnEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var currentEntryNumber = G3MrnEntryNumber;
				var oldValue = currentEntryNumber?.CE_EntryNum ?? ZString.Empty;

				if (oldValue != value)
				{
					if (currentEntryNumber == null)
					{
						currentEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, G3DeclarationType, true);
					}

					currentEntryNumber.CE_EntryNum = value;
					G3MovementReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo G3MovementReferenceNumberInfo => GetZPropertyInfo(nameof(G3MovementReferenceNumber));

		ABLEntryNum G3MrnEntryNumber
		{
			get
			{
				if (g3MrnEntryNumber == null || g3MrnEntryNumber.IsDeleted)
				{
					g3MrnEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, G3DeclarationType);
				}

				return g3MrnEntryNumber;
			}
		}

		ABLEntryNum g3MrnEntryNumber;

		public ZString G3RevokedMovementReferenceNumber
		{
			get => G3RevokedMrnEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var currentEntryNumber = G3RevokedMrnEntryNumber;
				var oldValue = currentEntryNumber?.CE_EntryNum ?? ZString.Empty;

				if (oldValue != value)
				{
					if (currentEntryNumber == null)
					{
						currentEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, G3RevokeType, true);
					}

					currentEntryNumber.CE_EntryNum = value;
				}
			}
		}

		ABLEntryNum G3RevokedMrnEntryNumber
		{
			get
			{
				if (g3RevokedMrnEntryNumber == null || g3RevokedMrnEntryNumber.IsDeleted)
				{
					g3RevokedMrnEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, G3RevokeType);
				}

				return g3RevokedMrnEntryNumber;
			}
		}

		ABLEntryNum g3RevokedMrnEntryNumber;

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.ES.Manifest.H7.Business.AsycudaBill.H7MovementReferenceNumber", Caption = "H7 Movement Reference Number", ShortCaption = "MRN (H7)", MediumCaption = "MRN (H7)", FullDescription = "A unique identifier issued by the relevant Customs authority that enables the Customs authority to identify and process your shipment in the customs system.")]
		[MaxLength(35)]
		public ZString H7MovementReferenceNumber
		{
			get => H7MrnEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var currentEntryNumber = H7MrnEntryNumber;
				var oldValue = currentEntryNumber?.CE_EntryNum ?? ZString.Empty;

				if (oldValue != value)
				{
					if (currentEntryNumber == null)
					{
						currentEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, H7MessageType, true);
					}

					currentEntryNumber.CE_EntryNum = value;
					H7MovementReferenceNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo H7MovementReferenceNumberInfo => GetZPropertyInfo(nameof(H7MovementReferenceNumber));

		ABLEntryNum H7MrnEntryNumber
		{
			get
			{
				if (h7MrnEntryNumber == null || h7MrnEntryNumber.IsDeleted)
				{
					h7MrnEntryNumber = LoadEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, H7MessageType);
				}

				return h7MrnEntryNumber;
			}
		}

		ABLEntryNum h7MrnEntryNumber;

		public const string G3DeclarationType = "G3";
		public const string G3RevokeType = "G3REVOKED";
		public const string H7MessageType = "H7";

		#endregion

		#region IESMessageInfoProvider Members

		GlbStaff IESMessageInfoProvider.Broker => Header?.CustomsAgent;

		ZString IESMessageInfoProvider.MRN => MovementReferenceNumber;

		ZString IESMessageInfoProvider.DocumentJobReference => MovementReferenceNumber;

		#endregion

		#region IESResponseBOMessageStatus Members

		ZString IESResponseBOMessageStatus.MessageStatus { set => ABL_MessageStatus = value; }

		ZGuid IESResponseBusinessObject.BranchPK => Header?.Branch?.PK ?? GlbBranch.CurrentBranch.PK;

		EDIMessageCollection IESResponseBusinessObject.MessageCollection => Messages;

		ZString IESMessageBusinessObject.EntryReference => ABL_BillNumber;

		#endregion
	}
}
