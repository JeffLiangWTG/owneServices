using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IE.PBN.Business
{
	public class AsycudaManifestHeader :
		ASYCUDA.Business.AsycudaManifestHeader,
		Integration.Customs.IE.IAsycudaManifestHeader,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		IMessageAttachee,
		IRelatedJob
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string IsEmptyVehicle = "IsEmptyVehicle";
		}

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch => Branch;

		GlbStaff IMessageAttachee.CustomsAgent => CustomsAgent;

		IRelatedJob IMessageAttachee.RelatedJob => this;

		ZString IMessageAttachee.LogicalStatus { get => AMA_MessageStatus; set => AMA_MessageStatus = value; }

		ZString IMessageAttachee.EntryStatus
		{
			get => RegistrationStatus;
			set
			{
				RegistrationStatus = value;
			}
		}

		ZString IMessageAttachee.MovementReferenceNumber => RegistrationEntryNumber?.CE_EntryNum ?? ZString.Empty;

		IEnumerable<Enterprise.Messaging.Business.EDIMessage> IMessageAttachee.Messages => Messages.Cast<Enterprise.Messaging.Business.EDIMessage>();

		#endregion

		protected override Type GetPersonTypeCore() => typeof(CusPerson);

		public new CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;

		protected override CusPersonCollection CreateNewCusPersonCollection()
		{
			return new CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> { { PBNReferenceItem.PBNReferenceType, typeof(PBNReferenceItem) } };
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.TransportModeList))]
		public override ZString AMA_TransportMode
		{
			get { return base.AMA_TransportMode; }
			set { base.AMA_TransportMode = value; }
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CarrierList))]
		public override ZString AMA_CarrierCode
		{
			get => base.AMA_CarrierCode;
			set => base.AMA_CarrierCode = value;
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Ireland;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ManifestType = PBNManifestTypes.Codes.PBN;
		}

		[ChildEditable]
		public PBNCustomsDeclarationItemCollection<PBNCustomsDeclarationItem> CustomsReferenceCollection
		{
			get
			{
				if (customsReferenceCollection == null)
				{
					customsReferenceCollection = new PBNCustomsDeclarationItemCollection<PBNCustomsDeclarationItem>(this, IEPBNDeclarationTypes.CustomsDeclarationItemCodes);
					customsReferenceCollection.Load();
					RegisterEditableChildObject(customsReferenceCollection);
				}
				return customsReferenceCollection;
			}
		}
		PBNCustomsDeclarationItemCollection<PBNCustomsDeclarationItem> customsReferenceCollection;

		[ChildEditable]
		public PBNCustomsDeclarationItemCollection<PBNTransitDeclarationItem> TransitDeclarationCollection
		{
			get
			{
				if (transitDeclarationCollection == null)
				{
					transitDeclarationCollection = new PBNCustomsDeclarationItemCollection<PBNTransitDeclarationItem>(this, IEPBNDeclarationTypes.TransitDeclarationItemCodes);
					transitDeclarationCollection.Load();
					RegisterEditableChildObject(transitDeclarationCollection);
				}
				return transitDeclarationCollection;
			}
		}
		PBNCustomsDeclarationItemCollection<PBNTransitDeclarationItem> transitDeclarationCollection;

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		[ResourceStringData("Enterprise.Customs.IE.PBN.AsycudaManifestHeader.EmptyVehicle", Caption = "Empty Vehicle?")]
		public ZBool IsEmptyVehicle
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsEmptyVehicle);
			set
			{
				var oldValue = IsEmptyVehicle;
				this.SetSystemDefinedValue(Schema.IsEmptyVehicle, value);
				IsEmptyVehicleInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo IsEmptyVehicleInfo => GetZPropertyInfo(Schema.IsEmptyVehicle);

		#region IRelatedJob members
		ZString IRelatedJob.JobNumber => AMA_JobReference;

		ZString IRelatedJob.JobDescription => HumanReadableName;

		ZString IRelatedJob.JobStatus => AMA_CustomsStatus;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.ASYCUDA.ASYCUDAPreBoardingNotification;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();
		#endregion

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);
	}
}
