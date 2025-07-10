using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[SystemDefinedValues]
	public class QuarantineExDocEstablishmentAndTime : AutoQuarantineExDocEstablishmentAndTime
		, IClusterKeyWorker, ICusCodeDataTypeSupporter, ITemplateCopyable
	{
		public QuarantineExDocEstablishmentAndTime(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public abstract new class Schema : AutoQuarantineExDocEstablishmentAndTime.Schema
		{
			public const string EE_E2_fAddress = "EE_E2_fAddress";
			public const string EE_EstablishmentPostedStatusDescription = "EE_EstablishmentPostedStatusDescription";
		}

		const int IngredientsMaxCount = 8;
		string IngredientsMaxCountError => Res.GetString("QuarantineExDocEstablishmentAndTime|ingredientsMaxCountError", "You are only allowed a maximum of {0} Treatment Active Ingredient here.", IngredientsMaxCount);

		[ChildEditable(true)]
		public TreatmentActiveIngredientCollection Ingredients
		{
			get
			{
				if (ingredients == null)
				{
					ingredients = new TreatmentActiveIngredientCollection(this);
					ingredients.Load();
					ingredients.Sort(CusCodeDataSchema.Constants.CY_Order);
					RegisterEditableChildObject(ingredients);
					ingredients.MaxCountValidationEnable(IngredientsMaxCount, IngredientsMaxCountError);
				}
				return ingredients;
			}
		}
		TreatmentActiveIngredientCollection ingredients;

		public QuarantineExDocLine QuarantineExDocLine => Factory.Load<QuarantineExDocLine>(EE_QL);

		public QuarantineExDocHeader QuarantineExDocHeader
		{
			get
			{
				if (quarantineExDocHeader == null && QuarantineExDocLine != null)
				{
					quarantineExDocHeader = QuarantineExDocLine.QuarantineExDocHeader;
				}
				return quarantineExDocHeader;
			}
		}
		QuarantineExDocHeader quarantineExDocHeader;

		internal QuarantineExDocEstablishmentAndTimeCollection ParentCollection;

		public bool IsNEXDOCSActive => QuarantineExDocHeader?.IsNEXDOCSActive ?? false;

		internal JobDeclaration JobDeclaration => QuarantineExDocLine?.InvoiceLine?.InvoiceHeader?.JobDeclaration;

		public QuarantineExDocEstablishmentAndTime Clone(Dictionary<ZGuid, ZGuid> jobDocAddressPKPairs)
		{
			var result = CloneWithExclusions();
			((IBusinessObjectInternals)result).IsCopying = true;

			try
			{
				var addressPK = ZGuid.Empty;
				jobDocAddressPKPairs?.TryGetValue(EE_E2_Address, out addressPK);
				result.EE_E2_Address = addressPK;
			}
			finally
			{
				((IBusinessObjectInternals)result).IsCopying = false;
			}

			return result;
		}

		QuarantineExDocEstablishmentAndTime CloneWithExclusions()
		{
			var excludedProperties = new string[] { QuarantineExDocEstablishmentAndTimeSchema.Constants.EE_EstablishmentPostedStatus };
			return (QuarantineExDocEstablishmentAndTime)base.Clone(new BusinessObjectCloneArgs(excludedProperties));
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region ITemplateCopyable

		public IBusiness TemplateCopy() => CloneWithExclusions();

		#endregion

		#region Overrides

		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentTemperature", Caption = "Treatment Temperature")]
		public override ZDecimal EE_TreatmentTemperature { get => base.EE_TreatmentTemperature; set => base.EE_TreatmentTemperature = value; }

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocEstablishmentAndTimeLookups.TreatmentTemperatureUQ))]
		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentTemperatureUQ", Caption = "Treatment Temperature UQ")]
		public override ZString EE_TreatmentTemperatureUQ { get => base.EE_TreatmentTemperatureUQ; set => base.EE_TreatmentTemperatureUQ = value; }

		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentDuration", Caption = "Treatment Duration")]
		public override ZDecimal EE_TreatmentDuration { get => base.EE_TreatmentDuration; set => base.EE_TreatmentDuration = value; }

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocEstablishmentAndTimeLookups.TreatmentDurationUQ))]
		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentDurationUQ", Caption = "Treatment Duration UQ")]
		public override ZString EE_TreatmentDurationUQ { get => base.EE_TreatmentDurationUQ; set => base.EE_TreatmentDurationUQ = value; }

		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentConcentration", Caption = "Treatment Concentration")]
		public override ZDecimal EE_TreatmentConcentration { get => base.EE_TreatmentConcentration; set => base.EE_TreatmentConcentration = value; }

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocEstablishmentAndTimeLookups.TreatmentConcentrationUQ))]
		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentConcentrationUQ", Caption = "Treatment Concentration UQ")]
		public override ZString EE_TreatmentConcentrationUQ { get => base.EE_TreatmentConcentrationUQ; set => base.EE_TreatmentConcentrationUQ = value; }

		[ResourceStringData("QuarantineExDocEstablishmentAndTime|EE_TreatmentInfo", Caption = "Treatment Info")]
		public override ZString EE_TreatmentInfo { get => base.EE_TreatmentInfo; set => base.EE_TreatmentInfo = value; }

		[RelatedBusinessObject(nameof(QuarantineExDocLine))]
		public override ZGuid EE_QL
		{
			get { return base.EE_QL; }
			set
			{
				bool hasChanged = base.EE_QL != value;
				base.EE_QL = value;
				if (hasChanged)
				{
					Ingredients.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocEstablishmentAndTimeLookups.ProcessingType))]
		public override ZString EE_ProcessingType
		{
			get { return base.EE_ProcessingType; }
			set
			{
				bool hasChanged = base.EE_ProcessingType != value;
				base.EE_ProcessingType = value;
				if (hasChanged)
				{
					if (QuarantineExDocLine != null)
					{
						QuarantineExDocLine.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString EE_AuthorisationEstablishmentID
		{
			get { return base.EE_AuthorisationEstablishmentID; }
			set
			{
				var changed = EE_AuthorisationEstablishmentID != value;
				base.EE_AuthorisationEstablishmentID = value;
				if (changed && !value.IsEmpty && !IsCopying)
				{
					SyncAddress();
				}
			}
		}

		public override ZGuid EE_E2_Address
		{
			get => base.EE_E2_Address;
			set
			{
				var changed = EE_E2_Address != value;
				base.EE_E2_Address = value;

				if (changed && !IsCopying)
				{
					SyncAuthorisationEstablishmentNumber();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(QuarantineExDocEstablishmentAndTimeLookups.EstablishmentIndicatorList))]
		public override ZString EE_EstablishmentIndicator
		{
			get => base.EE_EstablishmentIndicator;
			set => base.EE_EstablishmentIndicator = value;
		}

		public ZString EE_EstablishmentPostedStatusDescription => Lookups.EstablishmentPostedStatusList.GetDescriptionFromCode(EE_EstablishmentPostedStatus);

		// These properties are used in the GUI by RFPProcessUserControl
		public bool IsLodged => EE_EstablishmentPostedStatus == NEXDOCEstablishmentPostedStatus.Codes.Lodged;
		public bool IsDeletePending => EE_EstablishmentPostedStatus == NEXDOCEstablishmentPostedStatus.Codes.DeletePending;

		protected override QuarantineExDocEstablishmentAndTimeValidation GetNewValidation()
		{
			if (IsNEXDOCSActive)
			{
				return new NEXDOCSQuarantineExDocEstablishmentAndTimeValidation(this);
			}
			else
			{
				return base.GetNewValidation();
			}
		}

		protected override QuarantineExDocEstablishmentAndTimeLookups GetNewLookups()
		{
			if (IsNEXDOCSActive)
			{
				return new NEXDOCSQuarantineExDocEstablishmentAndTimeLookups(this);
			}
			else
			{
				return new EXDOCSQuarantineExDocEstablishmentAndTimeLookups(this);
			}
		}

		protected override bool IsLookupsCachedInBase => CalculateIsLookupsCached();

		public void ResetIsLookupsCached()
		{
			isLookupsCached = null;
		}

		bool CalculateIsLookupsCached()
		{
			var result = false;
			if (isLookupsCached.HasValue)
			{
				result = isLookupsCached.Value;
			}
			isLookupsCached = true;
			return result;
		}
		bool? isLookupsCached;

		#endregion // Overrides

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)EE_ClusterKeyInfo;
		Type IClusterKeyWorker.ParentBizObjType => typeof(QuarantineExDocLine);
		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)EE_QLInfo;
		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion

		#region Implementation

		bool isSyncInProgress;

		OrgHeader GetAuthorisationEstablishmentOrganisation() => Address?.Organisation;

		void SyncAuthorisationEstablishmentNumber()
		{
			if (!isSyncInProgress)
			{
				isSyncInProgress = true;
				try
				{
					var organisation = GetAuthorisationEstablishmentOrganisation();
					if (organisation != null)
					{
						EE_AuthorisationEstablishmentID = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber, Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia));
					}
				}
				finally
				{
					isSyncInProgress = false;
				}
			}
		}

		public void SyncAddress()
		{
			var declaration = JobDeclaration;
			if (declaration != null && !isSyncInProgress)
			{
				isSyncInProgress = true;
				try
				{
					using (!declaration.IsPersistent ? SuspendSettingHasChanges() : null)
					{
						var orgCusCodeQuery = new ZQuery(OrgCusCodeSchema.OK_CustomsRegNo, EE_AuthorisationEstablishmentID);
						orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber);
						orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Australia);
						var customsCode = Factory.LoadTop1<OrgCusCode>(orgCusCodeQuery);

						JobDocAddress docAddress = customsCode != null ? FindOrCreateAddressForOrganisation(customsCode.Header.PK) : null;
						EE_E2_Address = docAddress?.PK ?? ZGuid.Empty;
					}
				}
				finally
				{
					isSyncInProgress = false;
				}
			}
		}

		JobDocAddress FindOrCreateAddressForOrganisation(ZGuid organisationPK)
		{
			JobDocAddress result = null;

			var declaration = JobDeclaration;
			var addresses = (IDocAddresses)declaration.Shipment ?? declaration;
			var docAddresses = addresses.DocAddresses;
			var peAddresses = docAddresses.FindDocAddressesByType(DocAddressType.AQISProcessingEstablishment);
			result = peAddresses.FirstOrDefault(peAddress => peAddress.OrganisationPK == organisationPK) ?? peAddresses.FirstOrDefault(peAddress => peAddress.IsEmpty);
			if (result == null)
			{
				result = docAddresses.CreateWithRequirement(addresses.GetDocAddressRequirement(DocAddressType.AQISProcessingEstablishment));
				result.OrganisationPK = organisationPK;
			}

			return result;
		}

		#endregion // Implementation

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		public IDictionary<ZString, Type> GetCusCodeDataTypes() => new Dictionary<ZString, Type> { { CusCodeDataTypeList.Codes.EXDOCTreatmentActiveIngredient, typeof(TreatmentActiveIngredient) } };

		public override void Delete()
		{
			if (!IsDeleted && CanDelete)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		public override bool ReadOnly => !EE_EstablishmentPostedStatus.IsEmpty && IsNEXDOCSActive;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && !ReadOnly;
		}
	}
}
