using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using EuOfficeCodesTypes = Enterprise.Customs.EU.Business.EuOfficeCodesTypes;
using TemporaryStorageOfficeCode = Enterprise.Customs.EU.Business.TemporaryStorageOfficeCode;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageHeader : EU.Business.CusTempStorage.TemporaryStorageHeader,
		Integration.Customs.IE.ITemporaryStorageHeader,
		IMessageAttachee,
		IRelatedJob,
		ICusGoodsLocationProviderWithUCCVersion
	{
		const string UCC6LayoutProviderKey = "IEUCC6";
		const string UCC5LayoutProviderKey = "IEUCC5";

		public TemporaryStorageHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsUCC5 => AMA_ManifestType == ImportDeclarationApplicationCodeList.Codes.V1;

		public bool IsUCC6 => AMA_ManifestType == ImportDeclarationApplicationCodeList.Codes.V2;

		public new TemporaryStorageBill MasterBill => (TemporaryStorageBill)base.MasterBill;

		[ChildEditable]
		public new EU.Business.CusTempStorage.ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader> Bills => (EU.Business.CusTempStorage.ITemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>)base.Bills;

		public new IEnumerable<TemporaryStorageBill> HouseBills => base.HouseBills.Cast<TemporaryStorageBill>();

		public new TemporaryStorageHeaderLookups Lookups => (TemporaryStorageHeaderLookups)base.Lookups;

		protected override AsycudaManifestHeaderLookups GetNewLookups()
		{
			return new TemporaryStorageHeaderLookups(this);
		}

		protected override AsycudaManifestHeaderValidation GetNewValidation()
		{
			return IsUCC5 ? new TemporaryStorageHeaderValidationUCC5(this) : base.GetNewValidation();
		}

		protected override EU.Business.CusTempStorage.ITemporaryStorageBillCollection<EU.Business.CusTempStorage.TemporaryStorageBill, EU.Business.CusTempStorage.TemporaryStorageHeader> CreateNewTemporaryStorageBillCollection()
		{
			return new EU.Business.CusTempStorage.TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(this);
		}

		protected override Type GetBillTypeCore() => typeof(TemporaryStorageBill);

		protected override Type GoodsLocationTypeCore => typeof(CusGoodsLocation);

		protected override EU.Business.CusTempStorage.TemporaryStorageMessageSendingConfiguration GetNewMessageSendingConfiguration() => new TemporaryStorageMessageSendingConfiguration();

		protected override bool ForceGenerateLrn => lrnUpdateRequiredByMessageRejection;

		bool lrnUpdateRequiredByMessageRejection;

		protected override ZString GenerateLocalReferenceNumber()
		{
			try
			{
				return new LRNGenerator(Factory, Branch, LRNGenerator.Constants.TemporaryStorage.LrnPrefix, LRNGenerator.Constants.TemporaryStorage.FountainPrefix)
					.GetLRNAndSetIfNeeded((ZPropertyInfoString)LRNInfo, () => false);
			}
			finally
			{
				lrnUpdateRequiredByMessageRejection = false;
			}
		}

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch => Branch;

		GlbStaff IMessageAttachee.CustomsAgent => CustomsAgent;

		IRelatedJob IMessageAttachee.RelatedJob => this;

		ZString IMessageAttachee.LogicalStatus { get => AMA_MessageStatus; set => AMA_MessageStatus = value; }

		ZString IMessageAttachee.EntryStatus
		{
			get => CustomsStatus;
			set
			{
				CustomsStatus = value;
				if (value == AISEntryStatusList.Codes.Rejected)
				{
					lrnUpdateRequiredByMessageRejection = true;
				}
			}
		}

		ZString IMessageAttachee.MovementReferenceNumber => MRN;

		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<Enterprise.Messaging.Business.EDIMessage>();

		#endregion

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => AMA_JobReference;

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => CustomsStatus;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.EU.UCC6TemporaryStorage;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		#region ICusGoodsLocationProvider

		ZString EU.Business.ICusGoodsLocationProvider.ProviderKey => AMA_RN_NKCountry + Customs.Business.GoodsLocationProviderApplications.Codes.JobDeclaration;

		#endregion

		public ZPropertyInfo IsUCC5Info => AMA_ManifestTypeInfo;

		[ResourceStringData("427208D2-A0EC-48CC-B5FF-26D8FBE312F7", Caption = "Message Version")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.ManifestTypeList))]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				var oldValue = AMA_ManifestType;
				base.AMA_ManifestType = value;
				if (!IsCopying && !IsValidationSuspended && oldValue != AMA_ManifestType)
				{
					MasterBill.MarkAsNeedingValidation();
					Bills.ForEach(bill => bill.MarkAsNeedingValidation());
				}
			}
		}

		[ResourceStringData("99576831-4811-437F-BB90-FB62C993871B", Caption = "Customs Office of Lodgement")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsOfficeCodeList))]
		public ZString CustomsOfficeOfLodgement
		{
			get => CustomsOfficeCodeOfLodgement.CY_Data;
			set
			{
				CustomsOfficeCodeOfLodgement.CY_Data = value;
				RegisterEditableChildObject(CustomsOfficeCodeOfLodgement);
				CustomsOfficeOfLodgementInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsOfficeOfLodgementInfo => GetWrappedZPropertyInfo(nameof(CustomsOfficeOfLodgement), x => CustomsOfficeCodeOfLodgement.CY_DataInfo);

		public TemporaryStorageOfficeCode CustomsOfficeCodeOfLodgement => Factory.GetValue(ref customsOfficeOfLodgement,
			() => TemporaryStorageOfficeCode.LoadOrCreate<TemporaryStorageOfficeCode>(this, EuOfficeCodesTypes.Codes.OfficeOfLodgement));
		CachedProperty<TemporaryStorageOfficeCode> customsOfficeOfLodgement;

		[ResourceStringData("26039AD9-4617-4EBC-A112-B324A2692FB7", Caption = "Customs Office of First Entry", FullDescription = "5/24 Customs Office of First Entry")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.CustomsOfficeCodeList))]
		public ZString CustomsOfficeOfFirstEntry
		{
			get => CustomsOfficeCodeOfFirstEntry.CY_Data;
			set
			{
				CustomsOfficeCodeOfFirstEntry.CY_Data = value;
				RegisterEditableChildObject(CustomsOfficeCodeOfFirstEntry);
				CustomsOfficeOfFirstEntryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsOfficeOfFirstEntryInfo => GetWrappedZPropertyInfo(nameof(CustomsOfficeOfFirstEntry), x => CustomsOfficeCodeOfFirstEntry.CY_DataInfo);

		public TemporaryStorageOfficeCode CustomsOfficeCodeOfFirstEntry => Factory.GetValue(ref customsOfficeOfFirstEntry,
			() => TemporaryStorageOfficeCode.LoadOrCreate<TemporaryStorageOfficeCode>(this, EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent));
		CachedProperty<TemporaryStorageOfficeCode> customsOfficeOfFirstEntry;

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.RepresentativeStatusCodeList))]
		[ResourceStringData("739E93C3-A9D9-4D3B-AA27-93FA69FDF8D1", Caption = "Rep. Status", FullDescription = "Representative Status")]
		public override ZString AMA_AgentType { get => base.AMA_AgentType; set => base.AMA_AgentType = value; }

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageHeaderLookups.MeansIdentityTypeList))]
		[ResourceStringData("72192E88-CCCE-4C16-99EA-5336DC3D1B51", Caption = "Border Transport Type")]
		public override ZString AMA_TransportMeans { get => base.AMA_TransportMeans; set => base.AMA_TransportMeans = value; }

		[ResourceStringData("BF808842-B951-4443-9A72-ABB948732004", Caption = "Border Transport ID")]
		public override ZString AMA_VesselName { get => base.AMA_VesselName; set => base.AMA_VesselName = value; }

		protected override ZString GetCustomsStatusDescriptionCore()
		{
			var customsStatus = CustomsStatus;

			return Lookups.CustomsStatusList is CodeDescriptionPairList statusList && !customsStatus.IsEmpty
				? statusList.GetDescriptionFromCode(customsStatus)
				: ZString.Empty;
		}

		protected override EU.Business.CusTempStorage.TemporaryStorageHeaderCustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new TemporaryStorageHeaderCustomsOfficeRequirementHelper(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = ImportDeclarationApplicationCodeList.Codes.V1;
			AMA_AgentType = ZString.Empty;
		}

		protected override ZString LayoutProviderKeyCore => IsUCC5 ? UCC5LayoutProviderKey : UCC6LayoutProviderKey;
	}
}
