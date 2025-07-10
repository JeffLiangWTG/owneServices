using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CUSPRLCusTempStorageDec), "CusTempStorageLines")]
	[SystemDefinedValues]
	public class CUSPRLCusTempStorageLine : CusTempStorageLine, IDocAddresses, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public CUSPRLCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			snapshot = new CusTempStorageLineProvider(this);
		}

		CusTempStorageLineProvider snapshot;

		#region Schema

		public new class Schema : CusTempStorageLine.Schema
		{
			public new const int TSL_ReferenceNumberMaxLength = 18;
			public const int TSL_ReferenceNumberLineMaxLength = 3;
			public const int TSL_ReferenceNumber2LineMaxLength = 5;
			public const string Receptacle = "Receptacle";
			public const int ReceptacleMaxLength = 35;
			public const string ContainerNumber = "ContainerNumber";
			public const int ContainerNumberMaxLength = 17;
		}

		#endregion

		#region Dec

		public new CUSPRLCusTempStorageDec Dec => Factory.Load<CUSPRLCusTempStorageDec>(TSL_STH);

		#endregion

		#region Sequence Number Generator

		protected override bool SequenceNumberEnabledCore => Dec?.ReferenceNumber.IsEmpty ?? true;

		#endregion

		#region Properties

		public TransportDocumentMaster TransportDocumentMaster
		{
			get
			{
				if (transportDocumentMaster == null || transportDocumentMaster.IsDeleted || transportDocumentMaster.CSI_ParentID != PK)
				{
					transportDocumentMaster =  TransportDocumentMaster.LoadOrCreate(this);
					RegisterEditableChildObject(transportDocumentMaster);
				}
				return transportDocumentMaster;
			}
		}
		TransportDocumentMaster transportDocumentMaster;

		[MaxLength(Schema.ReceptacleMaxLength)]
		[ResourceStringData("1CF3DD87-408C-4DD8-A29D-EBFE203BB7CB", Caption = "Receptacle")]
		public ZString Receptacle
		{
			get => Factory.GetValue(ref receptacle, () => this.GetSystemDefinedValue<ZString>(ReceptacleGenAddOnPropertyName));
			set
			{
				var oldValue = Receptacle;
				if (value != Receptacle)
				{
					CheckMaximumLength(ReceptacleInfo, value);
					this.SetSystemDefinedValue(ReceptacleGenAddOnPropertyName, value);
					ReceptacleInfo.RefreshBinding(oldValue);
				}
			}
		}
		CachedProperty<ZString> receptacle;

		public ZPropertyInfo ReceptacleInfo => GetZPropertyInfo(Schema.Receptacle);

		[ResourceStringData("36D33B1A-3EFA-4AB2-90AD-BAD18A8E9CE5", Caption = "Container Number")]
		[MaxLength(Schema.ContainerNumberMaxLength)]
		public ZString ContainerNumber
		{
			get => Factory.GetValue(ref containerNumber, () => this.GetSystemDefinedValue<ZString>(ContainerNumberGenAddOnPropertyName));
			set
			{
				var oldValue = ContainerNumber;
				if (value != ContainerNumber)
				{
					CheckMaximumLength(ContainerNumberInfo, value);
					this.SetSystemDefinedValue(ContainerNumberGenAddOnPropertyName, value);
					ContainerNumberInfo.RefreshBinding(oldValue);
				}
			}
		}
		CachedProperty<ZString> containerNumber;

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(Schema.ContainerNumber);

		public JobDocAddress CarrierDocAddress
		{
			get
			{
				if (carrierDocAddress == null || carrierDocAddress.IsDeleted)
				{
					carrierDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.Carrier);
				}
				return carrierDocAddress;
			}
		}
		JobDocAddress carrierDocAddress;

		public JobDocAddressRequirement CarrierDocAddressRequirement
		{
			get
			{
				if (carrierDocAddressRequirement == null)
				{
					carrierDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.Carrier);
					carrierDocAddressRequirement.ValidateOrganisationPK = ValidateCarrier;
					JobDocAddressManager.AddRequirement(carrierDocAddressRequirement);
				}
				return carrierDocAddressRequirement;
			}
		}
		JobDocAddressRequirement carrierDocAddressRequirement;

		void ValidateCarrier(JobDocAddressValidation validation)
		{
			var header = Dec.StorageHeader;
			var info = CarrierDocAddress.OrganisationPKInfo;
			if (header.SJH_PreviousReferenceType == PreviousReferenceType.Codes._N355 &&
				transportDocumentTypesForMandatoryCarrier.Contains(TSL_TransportNumberType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(info, DocAddressTypes.Descriptions.Carrier);
			}

			if (CarrierDocAddress.Organisation != null)
			{
				var eori = CarrierDocAddress.Address?.GetEORI();
				if (!eori.HasValue || eori.Value.IsEmpty)
				{
					var orgCode = CarrierDocAddress.Organisation.OH_Code;
					info.AddMessageError(Res.GetString("C36EC177-0E93-4828-9515-F9863882D1BE",
						"No EORI code (EOR) exists in the organization registration numbers for organization {0}.",
						orgCode));
				}
			}
		}

		readonly ImmutableHashSet<ZString> transportDocumentTypesForMandatoryCarrier = ImmutableHashSet.Create<ZString>(TransportDocumentTypes.N703, TransportDocumentTypes.N740);

		[MaxLength(Schema.TSL_ReferenceNumberMaxLength)]
		public override ZString TSL_ReferenceNumber { get => base.TSL_ReferenceNumber; set => base.TSL_ReferenceNumber = value; }

		[MaxLength(Schema.TSL_ReferenceNumberLineMaxLength)]
		[ResourceStringData("288BE3ED-B597-48F4-AF3B-242B0DC1750F", Caption = "Reference No. Line")]
		public override ZInt TSL_ReferenceNumberLine { get => base.TSL_ReferenceNumberLine; set => base.TSL_ReferenceNumberLine = value; }

		[ResourceStringData("039589CC-48AE-48EA-B150-BDA6879CB0E6", Caption = "Reference No.")]
		public override ZString TSL_ReferenceNumber2 { get => base.TSL_ReferenceNumber2; set => base.TSL_ReferenceNumber2 = value; }

		[MaxLength(Schema.TSL_ReferenceNumber2LineMaxLength)]
		[ResourceStringData("6D043696-7711-46D2-BB8D-D6A2F70176AC", Caption = "Reference No. Line")]
		public override ZInt TSL_ReferenceNumber2Line { get => base.TSL_ReferenceNumber2Line; set => base.TSL_ReferenceNumber2Line = value; }

		[ResourceStringData("151CF0F1-59B7-4022-94B9-A223290A8201", Caption = "Owner Reference Number", ShortCaption = "Ref. Num.", MediumCaption = "Owner Ref. Num.")]
		public override ZString TSL_OwnerReferenceNumber { get => base.TSL_OwnerReferenceNumber; set => base.TSL_OwnerReferenceNumber = value; }

		[ResourceStringData("C7D17BD4-16E5-47FD-B78E-D68B5FA5F731", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CUSPRLCusTempStorageLineLookups.TransportNumberTypeList))]
		public override ZString TSL_TransportNumberType { get => base.TSL_TransportNumberType; set => base.TSL_TransportNumberType = value; }

		[ResourceStringData("234EDC1C-716F-4EC7-94F2-2E2A54787F5A", Caption = "Reference Number")]
		public override ZString TSL_TransportNumber { get => base.TSL_TransportNumber; set => base.TSL_TransportNumber = value; }

		#endregion

		#region Base Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TSL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		protected override bool ReadOnlyTSL_LineNo => !TSL_CustomsStatus.IsEmpty || (Dec?.ReferenceNumber.IsEmpty ?? true);

		public override void OnSaving()
		{
			base.OnSaving();
			var dec = Dec;
			if (dec != null && !dec.STH_MessageStatus.IsEmpty && HasChanges)
			{
				var currentSnapshot = new CusTempStorageLineProvider(this);
				if (currentSnapshot != snapshot)
				{
					TSL_IsModified = ZBool.True;
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				snapshot = new CusTempStorageLineProvider(this);
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#endregion

		#region Lookups

		public new CUSPRLCusTempStorageLineLookups Lookups => (CUSPRLCusTempStorageLineLookups)base.Lookups;
		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CUSPRLCusTempStorageLineLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CUSPRLCusTempStorageLineValidation(this);

		#endregion

		#region JobDocAddress

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => new JobDocAddressValidation(addressToValidate);

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.Carrier:
					return CarrierDocAddressRequirement;
				default:
					return null;
			}
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => Lookups.OrgHeaderCollection;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new[] { DocAddressType.Carrier };

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}
				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		public JobDocAddressManager JobDocAddressManager
		{
			get { return jobDocAddressManager ??= new JobDocAddressManager(); }
		}
		JobDocAddressManager jobDocAddressManager;

		#endregion

		const string ReceptacleGenAddOnPropertyName = $"{CusTempStorageLineSchema.Constants.Prefix}_{nameof(Receptacle)}";

		const string ContainerNumberGenAddOnPropertyName = $"{CusTempStorageLineSchema.Constants.Prefix}_{nameof(ContainerNumber)}";
		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(TransportDocumentMaster) }
			};
			return result;
		}
	}
}
