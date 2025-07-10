using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaManifestHeader :
		ASYCUDA.Business.AsycudaManifestHeader,
		ICusCodeDataTypeSupporter,
		ICusSupportingInfoTypeSupporter,
		ICusStorageDocPivotTypeSupporter,
		ICusReferenceTypeSupporter,
		Integration.Customs.ASYCUDA.EUICS2.IAsycudaManifestHeader
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
			if (IsAwaitingResponse())
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ManifestBase.AsycudaManifestHeader.Schema
		{
			public const string ReEntryIndicator = nameof(AsycudaManifestHeader.ReEntryIndicator);
			public const string SplitConsignmentIndicator = nameof(AsycudaManifestHeader.SplitConsignmentIndicator);
			public const string PreviousMRN = nameof(AsycudaManifestHeader.PreviousMRN);
			public const string LocalReferenceNumber = nameof(AsycudaManifestHeader.LocalReferenceNumber);
			public const string MOTIdentifier = nameof(AsycudaManifestHeader.MOTIdentifier);
			public const string MOTIdentifierType = nameof(AsycudaManifestHeader.MOTIdentifierType);
			public const string RegistrationNumberForBinding = nameof(AsycudaManifestHeader.RegistrationNumberForBinding);
			public const string ArrivalReferenceNumber = nameof(AsycudaManifestHeader.ArrivalReferenceNumber);
			public const string SpecificCircumstanceIndicator = nameof(AsycudaManifestHeader.SpecificCircumstanceIndicator);
			public const string AddressedMemberState = nameof(AsycudaManifestHeader.AddressedMemberState);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var branch = Branch;
			AMA_OA_Declarant = branch.OrgProxy?.MainAddress?.PK ?? ZGuid.Empty;
			var countryCode = branch.Company.Country.Code;
			if (Lookups.CountryCodeICS2MS.ContainsCode(countryCode))
			{
				AddressedMemberState = countryCode;
			}
		}

		protected override ZString NameFromCode(ZString countryCode)
		{
			return Lookups.CountryCodeICS2MS.GetDescriptionFromCode(countryCode) ?? countryCode;
		}

		public override ResourceStringData VoyageFlightNoLabel =>
			IsAir
				? Res.GetData("9F70E63C-C367-4359-8BEA-4C79255C0DFA", "Flight")
				: IsSea || IsInlandWaterway
					? Res.GetData("07B19E8F-B8CB-4FB7-B37E-D5A57CEA94D3", "Voyage")
					: Res.GetData("58F5477F-82E6-41E4-9764-583395ACE685", "Flight/Voyage");

		#region Properties

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.AMA_GB", Caption = "Branch")]
		public override ZGuid AMA_GB
		{
			get => base.AMA_GB;
			set => base.AMA_GB = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MessageStatusList))]
		public override ZString AMA_MessageStatus
		{
			get => base.AMA_MessageStatus;
			set
			{
				var hasChanges = base.AMA_MessageStatus != value;
				base.AMA_MessageStatus = value;
				if (!IsCopying && hasChanges)
				{
					SetReadOnlyIncludingChildren(IsAwaitingResponse());
				}
			}
		}

		[BusinessObjectTestExclude]
		[ReadOnly(true)]
		public override ZString AMA_ManifestType
		{
			get => base.AMA_ManifestType;
			set
			{
				bool hasChanges = base.AMA_ManifestType != value;
				base.AMA_ManifestType = value;
				if (!IsCopying && hasChanges)
				{
					AMA_TransportMode = IsCarrierManifest ? (IsTransportModeEnabledRAI ? TransportModes.Rail : ZString.Empty) : TransportModes.Air;
				}
			}
		}

		public override ZString AMA_RN_NKCountry
		{
			get => base.AMA_RN_NKCountry;
			set
			{
				var oldValue = base.AMA_RN_NKCountry;
				base.AMA_RN_NKCountry = value;

				if (!IsCopying && oldValue != value)
				{
					AddressedMemberState = value;
				}
			}
		}

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = base.AMA_OA_Carrier;
				if (oldValue != value)
				{
					base.AMA_OA_Carrier = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAMA_OA_ShippingAgent();
					}
				}
			}
		}

		public override ZGuid AMA_OA_ShippingAgent
		{
			get => base.AMA_OA_ShippingAgent;
			set
			{
				var oldValue = base.AMA_OA_ShippingAgent;
				if (oldValue != value)
				{
					base.AMA_OA_ShippingAgent = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAMA_OA_Carrier();
					}
				}
			}
		}

		public override ZString AMA_AgentType
		{
			get => base.AMA_AgentType;
			set
			{
				var oldValue = base.AMA_AgentType;
				if (oldValue != value)
				{
					base.AMA_AgentType = value;
					DefaultTransportDocumentTypeForBill();
				}
			}
		}

		public override ZString AMA_ApplicationCode
		{
			get => base.AMA_ApplicationCode;
			set
			{
				var oldValue = base.AMA_ApplicationCode;
				if (oldValue != value)
				{
					base.AMA_ApplicationCode = value;
					foreach (var bill in Bills)
					{
						bill.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString AMA_RL_NKPortOfLoading
		{
			get => base.AMA_RL_NKPortOfLoading;
			set
			{
				var oldValue = base.AMA_RL_NKPortOfLoading;
				if (oldValue != value)
				{
					base.AMA_RL_NKPortOfLoading = value;
					Itinerary.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var oldValue = base.AMA_RL_NKPortOfDischarge;
				if (oldValue != value)
				{
					base.AMA_RL_NKPortOfDischarge = value;
					Itinerary.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set
			{
				var oldValue = base.AMA_CustomsOffice;
				if (oldValue != value)
				{
					base.AMA_CustomsOffice = value;
					Itinerary.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.AMA_RN_NKConveyanceNationality", ShortCaption = "Ctry", MediumCaption = "Country", Caption = "Nationality/Country code.")]
		public override ZString AMA_RN_NKConveyanceNationality
		{
			get => base.AMA_RN_NKConveyanceNationality;
			set => base.AMA_RN_NKConveyanceNationality = value;
		}

		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.SpecificCircumstanceIndicator", Caption = "Type")]
		public override ZString SpecificCircumstanceIndicator
		{
			get => base.SpecificCircumstanceIndicator;
			set
			{
				if (!IsCopying && base.SpecificCircumstanceIndicator != value)
				{
					base.SpecificCircumstanceIndicator = value;

					ClearPreviousMRNWhenSpecificCircumstanceIndicatorUnapplicable();
					MarkNeedingValidationOnDocumentsAndPackItems();
					DefaultTransportDocumentTypeForBill();
					DefaultTransportMode();
					NeedPersonsTabInfo.RefreshBinding();
					IsAsycudaTransportMeansEnabledInfo.RefreshBinding();
					Bills.RefreshMaxCountValidation();
				}
			}
		}

		void ClearPreviousMRNWhenSpecificCircumstanceIndicatorUnapplicable()
		{
			if (!IndicatorHelper.IsSpecificCircumstanceIndicatorApplicableForPreviousMRN(SpecificCircumstanceIndicator))
			{
				PreviousMRNInfo.ClearValue();
			}
		}

		void MarkNeedingValidationOnDocumentsAndPackItems()
		{
			SupportingDocuments.MarkAsNeedingValidation();
			foreach (AsycudaBill bill in Bills)
			{
				bill.SupportingDocuments.MarkAsNeedingValidation();
				foreach (AsycudaPack pack in bill.Packs)
				{
					pack.PackedItem?.Validation?.ValidateAPI_Tariff();
					pack.SupportingDocuments.MarkAsNeedingValidation();
				}
			}
		}

		void DefaultTransportMode()
		{
			if (SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F50)
			{
				AMA_TransportMode = TransportModes.Road;
			}
		}

		protected override bool SpecificCircumstanceIndicator_ReadOnly => false;

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.ReEntryIndicator", Caption = "Re-Entry Indicator")]
		public ZBool ReEntryIndicator
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.ReEntryIndicator);
			set
			{
				var oldValue = ReEntryIndicator;

				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.ReEntryIndicator, value);
					ReEntryIndicatorInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo ReEntryIndicatorInfo => GetZPropertyInfo(Schema.ReEntryIndicator);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.SplitConsignmentIndicator", Caption = "Split Consignment")]
		public ZBool SplitConsignmentIndicator
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.SplitConsignmentIndicator);
			set
			{
				var oldValue = SplitConsignmentIndicator;

				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.SplitConsignmentIndicator, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSplitConsignmentIndicator();
					}

					SplitConsignmentIndicatorInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo SplitConsignmentIndicatorInfo => GetZPropertyInfo(Schema.SplitConsignmentIndicator);

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.PreviousMRN", Caption = "Previous MRN")]
		[ReadOnlyMember(nameof(PreviousMRN_ReadOnly))]
		public ZString PreviousMRN
		{
			get => PREEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var oldValue = PREEntryNumber?.CE_EntryNum ?? ZString.Empty;
				UpdateOrCreateEntryNumber(PREEntryNumber, CusEntryNumberTypes.EU.PRE, AMA_RN_NKCountry, value, newCusEntryNumber =>
				{
					preEntryNumber = newCusEntryNumber;
					RegisterEditableChildObject(preEntryNumber);
				});

				if (oldValue != value)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidatePreviousMRN();
					}

					PreviousMRNInfo.RefreshBinding(oldValue);
				}
			}
		}
		public ZPropertyInfo PreviousMRNInfo => GetZPropertyInfo(Schema.PreviousMRN);

		bool PreviousMRN_ReadOnly => !IndicatorHelper.IsSpecificCircumstanceIndicatorApplicableForPreviousMRN(SpecificCircumstanceIndicator);

		public CusEntryNumber PREEntryNumber
		{
			get
			{
				if (preEntryNumber == null || preEntryNumber.IsDeleted || preEntryNumber.CE_EntryType != CusEntryNumberTypes.EU.PRE)
				{
					preEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.PRE, AMA_RN_NKCountry);
					if (preEntryNumber != null)
					{
						RegisterEditableChildObject(preEntryNumber);
					}
				}
				return preEntryNumber;
			}
		}
		CusEntryNumber preEntryNumber;

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.MOTIdentifier", Caption = "Mode of Transport Identifier", ShortCaption = "MOT Identifier")]
		public ZString MOTIdentifier
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.MOTIdentifier);
			set
			{
				CheckMaximumLength(MOTIdentifierInfo, value);
				var oldValue = MOTIdentifier;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.MOTIdentifier, value);
					MOTIdentifierInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo MOTIdentifierInfo => GetZPropertyInfo(Schema.MOTIdentifier);

		[MaxLength(2)]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MOTIdentifierTypeList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.MOTIdentifierType", Caption = "Mode of Transport Identifier Type", ShortCaption = "MOT Identifier Type")]
		public ZString MOTIdentifierType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.MOTIdentifierType);
			set
			{
				CheckMaximumLength(MOTIdentifierTypeInfo, value);
				var oldValue = MOTIdentifierType;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.MOTIdentifierType, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateMOTIdentifierType();
					}

					MOTIdentifierTypeInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo MOTIdentifierTypeInfo => GetZPropertyInfo(Schema.MOTIdentifierType);

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.LocalReferenceNumber", Caption = "Local Reference Number", ShortCaption = "LRN")]
		public ZString LocalReferenceNumber => LRNEntryNumber?.CE_EntryNum ?? ZString.Empty;

		ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

		CusEntryNumber LRNEntryNumber
		{
			get
			{
				if (lrnEntryNumber == null || lrnEntryNumber.IsDeleted || lrnEntryNumber.CE_EntryType != CusEntryNumberTypes.Standard.LocalReferenceNumber)
				{
					lrnEntryNumber = LoadLatestLRNEntryNumber();
					if (lrnEntryNumber != null)
					{
						RegisterEditableChildObject(lrnEntryNumber);
					}
				}
				return lrnEntryNumber;
			}
		}
		CusEntryNumber lrnEntryNumber;

		internal void GenerateNewLocalReferenceNumber()
		{
			var oldLrn = LRNEntryNumber?.CE_EntryNum ?? ZString.Empty;

			var newLrn = LRNHelper.GenerateLRN(Factory, GlbCompany.CurrentCompany, GlbBranch.CurrentBranch, SpecificCircumstanceIndicator);

			lrnEntryNumber = CusEntryNumber.New(this, CusEntryNumberTypes.Standard.LocalReferenceNumber, AMA_RN_NKCountry);
			lrnEntryNumber.CE_EntryNum = newLrn;

			RegisterEditableChildObject(lrnEntryNumber);
			LocalReferenceNumberInfo.RefreshBinding(oldLrn);
		}

		internal void RollBackLocalReferenceNumberOnSavingFailed(string oldLRN)
		{
			var currentLRN = LocalReferenceNumber;
			if (currentLRN != oldLRN)
			{
				lrnEntryNumber.Delete();

				if (string.IsNullOrEmpty(oldLRN))
				{
					lrnEntryNumber = null;
				}
				else
				{
					lrnEntryNumber = CusEntryNumber.Load(Factory, CusEntryNumberTypes.Standard.LocalReferenceNumber, oldLRN, AMA_RN_NKCountry).FirstOrDefault();
				}

				RegisterEditableChildObject(lrnEntryNumber);
				LocalReferenceNumberInfo.RefreshBinding(currentLRN);
			}
		}

		CusEntryNumber LoadLatestLRNEntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, PK);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Standard.LocalReferenceNumber);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, AMA_RN_NKCountry);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name + " DESC";

			return Factory.LoadTop1<CusEntryNumber>(query);
		}

		void UpdateOrCreateEntryNumber(CusEntryNumber cusEntryNumber, ZString entryType, ZString countryCode, ZString entryNumber, Action<CusEntryNumber> postCreateAction)
		{
			if (!entryNumber.IsEmpty)
			{
				if (cusEntryNumber == null)
				{
					cusEntryNumber = CusEntryNumber.LoadOrCreate(this, entryType, countryCode);
					postCreateAction?.Invoke(cusEntryNumber);
				}
				cusEntryNumber.CE_EntryNum = entryNumber;
			}
			else
			{
				if (cusEntryNumber != null)
				{
					cusEntryNumber.CE_EntryNum = entryNumber;
				}
			}
		}

		public bool IsForwarderManifest => AMA_ApplicationCode == ApplicationCodeTypeList.Codes.Consolidator;

		public bool IsCarrierManifest => AMA_ApplicationCode == ApplicationCodeTypeList.Codes.ShippingLine;

		CusEntryNumber ARNEntryNumber
		{
			get
			{
				if (arnEntryNumber == null || arnEntryNumber.IsDeleted || arnEntryNumber.CE_EntryType != CusEntryNumberTypes.EU.ArrivalReferenceNumber)
				{
					arnEntryNumber = CusEntryNumber.Load(this, CusEntryNumberTypes.EU.ArrivalReferenceNumber, AMA_RN_NKCountry);
					if (arnEntryNumber != null)
					{
						RegisterEditableChildObject(arnEntryNumber);
					}
				}
				return arnEntryNumber;
			}
		}
		CusEntryNumber arnEntryNumber;

		[MaxLength(35)]
		public ZString ArrivalReferenceNumber
		{
			get => ARNEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				var oldValue = ARNEntryNumber?.CE_EntryNum ?? ZString.Empty;
				UpdateOrCreateEntryNumber(ARNEntryNumber, CusEntryNumberTypes.EU.ArrivalReferenceNumber, AMA_RN_NKCountry, value, newCusEntryNumber =>
				{
					arnEntryNumber = newCusEntryNumber;
					RegisterEditableChildObject(arnEntryNumber);
				});

				ArrivalReferenceNumberInfo.RefreshBinding(oldValue);
				registrationNumberForBinding = string.Empty;
				RegistrationNumberForBindingInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo ArrivalReferenceNumberInfo => GetZPropertyInfo(Schema.ArrivalReferenceNumber);

		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.RegistrationNumber", Caption = "Registration Number", ShortCaption = "Reg. Number")]
		public ZString RegistrationNumberForBinding => registrationNumberForBinding.IsEmpty ? (registrationNumberForBinding = GetRegistrationNumberForBinding()) : registrationNumberForBinding;
		ZString registrationNumberForBinding;

		public ZPropertyInfo RegistrationNumberForBindingInfo => GetZPropertyInfo(Schema.RegistrationNumberForBinding);

		ZString GetRegistrationNumberForBinding()
		{
			var result = new StringBuilder(RegistrationNumber);
			if (!ArrivalReferenceNumber.IsEmpty)
			{
				result.Append(",");
				result.Append(ArrivalReferenceNumber);
			}

			return result.ToString();
		}

		public override ZString RegistrationNumber
		{
			get => base.RegistrationNumber;
			set
			{
				base.RegistrationNumber = value;
				registrationNumberForBinding = string.Empty;
				RegistrationNumberForBindingInfo.RefreshBinding();
			}
		}

		[MaxLength(3)]
		[ReadOnlyMember(nameof(CustomsProfileReadOnly))]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CustomsProfileList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.CustomsProfile", Caption = "Profile")]
		public override ZString AMA_CustomsProfile
		{
			get => base.AMA_CustomsProfile;
			set => base.AMA_CustomsProfile = value;
		}

		protected override bool NeedPersonsTabCore => false;

		bool CustomsProfileReadOnly => !AMA_CustomsProfile.IsEmpty && !AMA_MessageStatus.IsEmpty && Messages.Count > 0;

		public GlbCompany ProfileCompany => Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, AMA_CustomsProfile);

		public IEnumerable<GlbCompanyCredentialICS2> ValidICS2Credentials => CachedValueHelper.GetValue(ref validICS2CredentialsCache, GetValidICS2Credentials);
		CachedValue<IEnumerable<GlbCompanyCredentialICS2>> validICS2CredentialsCache;

		public GlbCompanyCredentialICS2 ICS2Credential => ValidICS2Credentials.FirstOrDefault(x => x.Company.GC_Code == AMA_CustomsProfile);

		IEnumerable<GlbCompanyCredentialICS2> GetValidICS2Credentials()
		{
			var query = new ZQuery(GlbExternalPasswordSchema.GP_PasswordType, PasswordTypesList.Codes.IC2);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Valid);
			query.AddToFilter(GlbExternalPasswordSchema.GP_ExpiryDate, SQLComparisonOperator.GreaterThan, ZDateTime.Today);
			query.AddToFilter(GlbExternalPasswordSchema.GP_GS, SQLComparisonOperator.Equal, null);
			return Factory.Load<GlbCompanyCredentialICS2>(query);
		}

		#region TransportModes

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				if (base.AMA_TransportMode != value)
				{
					base.AMA_TransportMode = value;
					DefaultSpecificCircumstanceIndicator();
					DefaultTransportDocumentTypeForBill();
					DefaultTransportMeans();
					IsAsycudaTransportMeansEnabledInfo.RefreshBinding();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MeansOfTransportTypeList))]
		[ResourceStringData("010985F8-6A3B-4936-A324-EB4026BF86C1", Caption = "Type of Means of Transport", MediumCaption = "Trans. Means Type")]
		public override ZString AMA_TransportMeans
		{
			get => base.AMA_TransportMeans;
			set => base.AMA_TransportMeans = value;
		}

		[ResourceStringData("C9B43F85-FF5B-4065-B464-BD58311426DB", Caption = "Declarant")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DeclarantAddresses))]
		public override ZGuid AMA_OA_Declarant { get => base.AMA_OA_Declarant; set => base.AMA_OA_Declarant = value; }

		public ZString DeclarantEori => Declarant?.Header.GetICS2EoriDetails() ?? ZString.Empty;

		protected override ZAddress GetNewAMA_OA_Declarant_ZAddress()
		{
			var result = base.GetNewAMA_OA_Declarant_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "The max length of SpecificCircumstanceIndicator is 3.")]
		void DefaultSpecificCircumstanceIndicator()
		{
			if (AMA_TransportMode == TransportModes.Air && IsForwarderManifest)
			{
				SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;
			}
			else if (AMA_TransportMode == TransportModes.Road && IsCarrierManifest)
			{
				SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
			}
			else if (AMA_TransportMode == TransportModes.Sea || AMA_TransportMode == TransportModes.InlandWaterwayTransport)
			{
				SpecificCircumstanceIndicator = ZString.Empty;
			}
		}

		void DefaultTransportMeans()
		{
			AMA_TransportMeans = ZString.Empty;
		}

		#endregion

		#endregion

		[ChildEditable(true)]
		public RouteEntryCollection Itinerary
		{
			get
			{
				if (itinerary == null)
				{
					itinerary = new RouteEntryCollection(this);
					itinerary.Load();
					RegisterEditableChildObject(itinerary);
				}
				return itinerary;
			}
		}
		RouteEntryCollection itinerary;

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

		public IAsycudaBillScreeningCollection<AsycudaBillScreening, AsycudaBill> BillScreenings => MasterBill.BillScreenings;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		public override void OnSaving()
		{
			if (PREEntryNumber != null && PREEntryNumber.CE_EntryNum.IsEmpty)
			{
				PREEntryNumber.Delete();
			}

			base.OnSaving();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			CleanDisabledData();
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.CountryCodeICS2MS))]
		[MaxLength(2)]
		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(AddressedMemberStateReadOnly))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.AddressedMemberState", Caption = "Country")]
		public ZString AddressedMemberState
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.AddressedMemberState);
			set
			{
				CheckMaximumLength(AddressedMemberStateInfo, value);
				var oldValue = AddressedMemberState;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.AddressedMemberState, value);
					if (Lookups.CountryCodeICS2MS.ContainsCode(value))
					{
						using (GetCheckBusinessObjectTypeSuspender())
						{
							base.AMA_RN_NKCountry = value;
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateAddressedMemberState();
					}

					AddressedMemberStateInfo.RefreshBinding(oldValue);
				}
			}
		}
		protected override Type GetEUMemberStateCommunicationTypeCore() => typeof(RequestHeader);

		public ZPropertyInfo AddressedMemberStateInfo => GetZPropertyInfo(nameof(AddressedMemberState));

		bool AddressedMemberStateReadOnly => !AMA_MessageStatus.IsEmpty && Messages.Count > 0 && !AddressedMemberState.IsEmpty;

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.One;

		protected override ASYCUDA.Business.ZZDatabaseValidationHelper GetNewZZValidationHelper() => new ZZDatabaseValidationHelper(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ZString GetDefaultCountryCode() => CountryCodes.EuropeanUnion;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		void DefaultTransportDocumentTypeForBill()
		{
			MasterBill.DefaultTransportDocumentType(AMA_TransportMode, SpecificCircumstanceIndicator, AMA_AgentType);

			Bills.AsEnumerable().ForEach((bill) => bill.DefaultTransportDocumentType(AMA_TransportMode, SpecificCircumstanceIndicator, AMA_AgentType));
		}

		protected override ASYCUDA.Business.AsycudaManifestHeaderDocWrapper GetDocWrapperCore() => new AsycudaManifestHeaderDocWrapper(this);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

		public bool IsAwaitingResponse() => AMA_MessageStatus == MessageStatusCodeList.Codes.Awaiting;

		public bool IsTransportModeEnabledRAI = FuncsHelper.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ICS2TransportModeRAI, ZDateTime.Today, options: FuncsHelper.ValidationOptions.UseCountryDefinitionFirstOtherwiseEUN, priorityToPilotFunctionality: true);

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.PaymentMethodList))]
		[ResourceStringData("Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader.AMA_PaymentMethod", Caption = "Method of Payment")]
		public override ZString AMA_PaymentMethod
		{
			get => base.AMA_PaymentMethod;
			set => base.AMA_PaymentMethod = value;
		}

		public bool IsSupplementaryDeclarantsEnabled => SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F24 && SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F50;
		public ZWrappedPropertyInfo IsSupplementaryDeclarantsEnabledInfo => GetWrappedZPropertyInfo(nameof(IsSupplementaryDeclarantsEnabled), _ => SpecificCircumstanceIndicatorInfo);

		public bool IsBillScreeningsEnabled => SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F50;
		public ZWrappedPropertyInfo IsBillScreeningsEnabledInfo => GetWrappedZPropertyInfo(nameof(IsBillScreeningsEnabled), _ => SpecificCircumstanceIndicatorInfo);

		public bool IsCusSupplyChainActorReferencesEnabled => SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F24;
		public ZWrappedPropertyInfo IsCusSupplyChainActorReferencesEnabledInfo => GetWrappedZPropertyInfo(nameof(IsCusSupplyChainActorReferencesEnabled), _ => SpecificCircumstanceIndicatorInfo);

		public ZBool IsAsycudaTransportMeansEnabled => AMA_TransportMode == TransportModes.Road || SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F51;
		public ZPropertyInfo IsAsycudaTransportMeansEnabledInfo => GetZPropertyInfo(nameof(IsAsycudaTransportMeansEnabled));

		public bool IsAdditionalInfosEnabled => SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F50;
		public ZWrappedPropertyInfo IsAdditionalInfosEnabledInfo => GetWrappedZPropertyInfo(nameof(IsAdditionalInfosEnabled), _ => SpecificCircumstanceIndicatorInfo);

		public bool IsAdditionalFiscalReferenceEnabled => SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F43;
		public ZWrappedPropertyInfo IsAdditionalFiscalReferenceEnabledInfo => GetWrappedZPropertyInfo(nameof(IsAdditionalFiscalReferenceEnabled), _ => SpecificCircumstanceIndicatorInfo);

		public bool IsPackedItemTypeOfGoodsAndGoodsValueEnabled => Factory.GetValue(ref isPackedItemTypeOfGoodsAndGoodsValueEnabled, () => SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F43));
		CachedProperty<bool> isPackedItemTypeOfGoodsAndGoodsValueEnabled;

		CachedProperty<bool> isInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator;
		public bool IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator => Factory.GetValue(
			ref isInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator,
			() => IsCarrierManifest
				&& (IsInlandWaterway || IsSea)
				&& (SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F11 || SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F12)
		);

		public ZWrappedPropertyInfo IsPackedItemTypeOfGoodsAndGoodsValueEnabledInfo => GetWrappedZPropertyInfo(nameof(IsPackedItemTypeOfGoodsAndGoodsValueEnabled), _ => SpecificCircumstanceIndicatorInfo);

		void CleanDisabledData()
		{
			CleanCollectionInAllBillsIfNotEnabled(IsSupplementaryDeclarantsEnabled, b => b.SupplementaryDeclarants);
			CleanCollectionInAllBillsIfNotEnabled(IsBillScreeningsEnabled, b => b.BillScreenings);
			CleanCollectionInAllBillsIfNotEnabled(IsCusSupplyChainActorReferencesEnabled, b => b.CusSupplyChainActorReferences);
			CleanCollectionInAllBillsIfNotEnabled(IsAsycudaTransportMeansEnabled, b => b.AsycudaTransportMeans);
			CleanCollectionInAllBillsIfNotEnabled(IsAdditionalInfosEnabled, b => b.AdditionalInfos);
			CleanCollectionInAllBillsIfNotEnabled(IsAdditionalFiscalReferenceEnabled, b => b.AdditionalFiscalReferences);
			CleanPackedItemsValues();

			void CleanCollectionInAllBillsIfNotEnabled(bool isEnabled, Func<AsycudaBill, IBusinessObjectCollection> collectionGetter)
			{
				if (!isEnabled)
				{
					foreach (var bill in Bills)
					{
						foreach (var o in collectionGetter(bill).ToArray())
						{
							collectionGetter(bill).RemoveFromRelationship(o);
							o.Delete();
						}
					}
				}
			}

			void CleanPackedItemsValues()
			{
				if (!IsPackedItemTypeOfGoodsAndGoodsValueEnabled)
				{
					foreach (var packedItem in Bills.Cast<AsycudaBill>().SelectMany(x => x.PackedItems).ToArray())
					{
						packedItem.API_TypeOfGoods = ZString.Empty;
						packedItem.API_GoodsValue = ZDecimal.Zero;
						packedItem.API_RX_NKGoodsValueCurrency = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("787C6C41-CEFC-473C-8E0C-CD981B1FDBB4", Caption = "Receptacle(s)")]
		public ZString ReceptacleId => Receptacles.AsString;

		public ZPropertyInfo ReceptacleIdInfo => GetZPropertyInfo(nameof(ReceptacleId));

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public ReceptacleCollection Receptacles
		{
			get
			{
				if (receptacles == null)
				{
					receptacles = new ReceptacleCollection(this);
					receptacles.Load();
					RegisterEditableChildObject(receptacles);
				}

				return receptacles;
			}
		}
		ReceptacleCollection receptacles;

		#region ICusCodeDataTypeSupporter

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) },
				{ CusSupportingInfoTypeList.Codes.ScreeningMethod, typeof(ScreeningMethod) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		#endregion

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = base.SupportedCusCodeDataTypes;
				result.Add(CusCodeDataTypeList.Codes.EUICS2RouteEntry, typeof(RouteEntry));
				result.Add(CusCodeDataTypeList.Codes.EUICS2Receptacle, typeof(Receptacle));
				return result;
			}
		}

		#endregion

		#region Additional Infos

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new AdditionalInfoCollection(this);
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}

		AdditionalInfoCollection additionalInfos;

		#endregion

		#region Cus Supply Chain Actor Reference

		[ChildEditable(true)]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}
				return cusSupplyChainActorReferences;
			}
		}

		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		#endregion

		#region Screening Method

		[ChildEditable(true)]
		public ScreeningMethodCollection ScreeningMethods
		{
			get
			{
				if (screeningMethods == null)
				{
					screeningMethods = new ScreeningMethodCollection(this);
					screeningMethods.Load();
					RegisterEditableChildObject(screeningMethods);
				}
				return screeningMethods;
			}
		}

		ScreeningMethodCollection screeningMethods;

		#endregion

		#region ICusStorageDocPivotTypeSupporter

		bool IsEDocPivotCollectionLoaded => eDocPivotCollection != null && eDocPivotCollection.IsLoaded;

		public void ReloadCollection()
		{
			if (IsEDocPivotCollectionLoaded)
			{
				EDocPivotCollection.Reload(true);
			}
		}

		public Type CusStorageDocPivotType => typeof(CusStorageDocPivot);

		public IEnumerable<IStorageDocsBaseCollection> EDocCollections => EDocsHelper.GetEDocCollections(this);

		[ChildEditable(true)]
		public CusStorageDocPivotCollection EDocPivotCollection
		{
			get
			{
				if (eDocPivotCollection == null)
				{
					eDocPivotCollection = new CusStorageDocPivotCollection(this);
					eDocPivotCollection.Load();
					RegisterEditableChildObject(eDocPivotCollection);
				}

				return eDocPivotCollection;
			}
		}
		CusStorageDocPivotCollection eDocPivotCollection;

		#endregion

		#region ICusReferenceTypeSupporter

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, typeof(CusSupplyChainActorReference) },
		};

		#endregion

		#region Request Headers

		[ChildEditable(true)]
		public RequestHeaderCollection RequestHeaders
		{
			get
			{
				if (requestHeaders == null)
				{
					requestHeaders = new RequestHeaderCollection(this);
					requestHeaders.Load();
					RegisterEditableChildObject(requestHeaders);
				}

				return requestHeaders;
			}
		}

		RequestHeaderCollection requestHeaders;

		#endregion

		public override BaseMessageSendingNotificationHelper GetMessageSendingNotificationHelper()
		{
			return new MessageSendingNotificationHelper(this);
		}

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => base.GetShouldPropertiesBeReadOnly(property) || !CanBeAmended(property.Name);

		public bool CanBeAmended(string propertyName)
		{
			return RegistrationNumber.IsEmpty || RegistrationStatus == EUICS2CustomsStatusList.Codes.CAN || propertyName switch
			{
				nameof(SpecificCircumstanceIndicator) => !IsF14F15F16(),
				nameof(AMA_OA_Carrier) => !IsF14F15F16(),
				nameof(AMA_OA_ShippingAgent) => !IsF14F15F16(),
				nameof(AMA_TransportMode) => !IsF14F15(),
				nameof(AMA_OA_Declarant) => !IsF14F15F16(),
				nameof(AMA_MasterBill) => !IsF14F15F16(),
				nameof(AddressedMemberState) => !IsF14F15F16(),
				nameof(AsycudaBill.ABL_BillNumber) => !IsF14F15F16(),
				nameof(AsycudaPack.APA_LineNo) => !IsF14F15(),
				_ => true,
			};

			bool IsF14F15F16() => IsF14F15() || SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F16;

			bool IsF14F15() => SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F14 || SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F15;
		}
	}
}
