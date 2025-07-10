using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GACPGAHeader :
		AutoGACPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		ILPCOContactParent,
		IPurgeValueParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter
	{
		public GACPGAHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoGACPGAHeader.Schema
		{
			public const string AreClothingAndTextileDetailsVisibility = "AreClothingAndTextileDetailsVisibility";
			public const string IsFTAProcessingCodeFA01 = "IsFTAProcessingCodeFA01";
		}

		public ZString DepartmentCode
		{
			get { return GACPGADepartmentCodes.Codes.ALL; }
		}

		public ZPropertyInfo DepartmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentCode)); }
		}

		#region Related

		[ChildEditable]
		CusCALPCOCollection LPCOs
		{
			get
			{
				if (lpcos == null)
				{
					lpcos = new CusCALPCOCollection(this);
					lpcos.Load();
					RegisterEditableChildObject(lpcos);
				}
				return lpcos;
			}
		}

		CusCALPCOCollection lpcos;

		[ChildEditable]
		public LPCOViewCollection LPCOViews
		{
			get
			{
				if (lpcoViews == null)
				{
					var dec = (InvoiceLine as IDeclarationProvider)?.Declaration as JobDeclaration;
					lpcoViews = new LPCOViewCollection(LPCOs, dec?.LPCOs, this);
					RegisterEditableChildObject(lpcoViews);
					HookParentLPCOViewsCountChange(dec);
				}
				return lpcoViews;
			}
		}
		LPCOViewCollection lpcoViews;

		void HookParentLPCOViewsCountChange(JobDeclaration dec)
		{
			if (dec != null && dec.LPCOViews != null)
			{
				dec.LPCOViews.GACCountChanged -= ParentGACCountChanged;
				dec.LPCOViews.GACCountChanged += ParentGACCountChanged;
			}
		}

		void ParentGACCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		#endregion

		#region Overrides

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		[PurgeValue(nameof(AreClothingAndTextileDetailsHidden))]
		public override ZString CA_FabricCountryOfOrigin
		{
			get => base.CA_FabricCountryOfOrigin;
			set => base.CA_FabricCountryOfOrigin = value;
		}

		[PurgeValue(nameof(AreClothingAndTextileDetailsHidden))]
		public override ZString CA_FibreCountryOfOrigin
		{
			get => base.CA_FibreCountryOfOrigin;
			set => base.CA_FibreCountryOfOrigin = value;
		}

		[PurgeValue(nameof(AreClothingAndTextileDetailsHidden))]
		public override ZString CA_FTACode
		{
			get => base.CA_FTACode;
			set => base.CA_FTACode = value;
		}

		[PurgeValue(nameof(AreClothingAndTextileDetailsHidden))]
		public override ZString CA_YarnCountryOfOrigin
		{
			get => base.CA_YarnCountryOfOrigin;
			set => base.CA_YarnCountryOfOrigin = value;
		}

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode
		{
			get { return PGACodes.Codes.GAC; }
		}

		IHasPGARequirements IPGAHeader.Parent
		{
			get { return Parent as IHasPGARequirements; }
		}

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			GACPGAHeader header = (GACPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
		}

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case GACPGADepartmentCodes.Codes.ALL:
					return CA_AllProgramIndInfo;
			}

			return null;
		}

		CodeDescriptionPairList IPGAProgramRequirementProvider.GetProgramCodesList()
		{
			return AddInfoLookups.ProgramCodesList;
		}

		void IPGAProgramRequirementProvider.ValidateProgramIndicator(ZString programCode)
		{
			switch (programCode)
			{
				case GACPGADepartmentCodes.Codes.ALL:
					AddInfoValidation.ValidateCA_AllProgramInd();
					break;
			}
		}

		IDisposable IPGAProgramRequirementProvider.SuspendSettingDefaultValues()
		{
			return null;
		}

		SetterSuspender IPGAProgramRequirementProvider.SetterSuspender => SetterSuspender;

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this, true);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return new Dictionary<ZString, Type>();
		}

		#endregion

		#region ILPCOContactParent

		IPGAContactDetails ILPCOContactParent.GetContactDetails(ZString type)
		{
			return (IPGAContactDetails)InvoiceLine?.GetContactDetails(type) ?? new PGAEmptyContactDetails();
		}

		#endregion

		internal const string The3rdPartyCountry = "3C";
		internal const string The3rdPartyCountryDescription = "3rd Party Country/Region";

		ZString GetDescriptionForCO(ZString countryCode, RefCountryCollection countryCollection)
		{
			var result = Enterprise.Core.Constants.FindBoxMessages.InvalidSelection.GetUnresolvedString();

			if (countryCode == The3rdPartyCountry)
			{
				result = The3rdPartyCountryDescription;
			}
			else if (!countryCode.IsEmpty)
			{
				var country = countryCollection.FirstOrDefault(x => x.RN_Code == countryCode);
				if (country != null)
				{
					result = country.RN_Desc;
				}
			}
			else
			{
				result = Enterprise.Core.Constants.FindBoxMessages.NoneSelected.GetUnresolvedString();
			}

			return result;
		}

		public ZString FabricCODescription => GetDescriptionForCO(CA_FabricCountryOfOrigin, AddInfoLookups.FabricOrigins);
		public ZString FibreCODescription => GetDescriptionForCO(CA_FibreCountryOfOrigin, AddInfoLookups.FibreOrigins);
		public ZString YarnCODescription => GetDescriptionForCO(CA_YarnCountryOfOrigin, AddInfoLookups.YarnOrigins);

		#region Related BO

		IHasPGARequirements PGAHeaderParent
		{
			get
			{
				if (fPGAHeaderParent == null)
				{
					if (Parent is IHasPGARequirements pgaHeaderParent)
					{
						fPGAHeaderParent = pgaHeaderParent;
						fPGAHeaderParent.TariffInfo.ValueChanged -= TariffInfoOnValueChanged;
						fPGAHeaderParent.TariffInfo.ValueChanged += TariffInfoOnValueChanged;
					}
				}
				return fPGAHeaderParent;
			}
		}
		IHasPGARequirements fPGAHeaderParent;

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceline == null && PGAHeaderParent is JobComInvoiceLine line)
				{
					invoiceline = line;
				}

				return invoiceline;
			}
		}
		JobComInvoiceLine invoiceline;

		void TariffInfoOnValueChanged(object sender, EventArgs eventArgs)
		{
			if (!IsCopying)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		#endregion

		#region Visibility
		public ZBool IsFTAProcessingCodeFA01 => AreClothingAndTextileDetailsVisibility && (CA_FTACode == FTAProcessingCodes.Codes.FA01);

		public ZBool AreClothingAndTextileDetailsVisibility
		{
			get
			{
				var tariff = PGAHeaderParent?.Tariff;

				if (tariff.HasValue)
				{
					var numberString = tariff.Value.SubstringSafe(0, 2);
					return numberString.CompareTo(ClothingTextileStartChaprter) >= 0 && numberString.CompareTo(ClothingTextileEndChaprter) <= 0;
				}

				return true;
			}
		}

		bool AreClothingAndTextileDetailsHidden => !AreClothingAndTextileDetailsVisibility;

		const string ClothingTextileStartChaprter = "50";
		const string ClothingTextileEndChaprter = "67";

		#endregion

		#region IPurgeValueParent

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeHelper ?? (purgeHelper = new PurgeValueHelper<GACPGAHeader>(this)); }
		}

		IPurgeValueHelper purgeHelper;

		#endregion

		#region IDeclarationProvider
		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return declaration ?? (declaration = (InvoiceLine as IDeclarationProvider)?.Declaration); }
		}
		BaseJobDeclaration declaration;

		ZBool ICADeclarationProvider.IsValidationEnabled => this.IsPGAValidationEnabled();
		#endregion

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && ((ICADeclarationProvider)this).IsValidationEnabled;
		}

		#region ILPCODefaulter
		IEnumerable<ZString> ILPCODefaulter.LPCOFieldsDefaultFromURN
		{
			get
			{
				return new List<ZString>
				{
					CusCALPCO.Schema.CLP_Type,
					CusCALPCO.Schema.CLP_RefNo,
					CusCALPCO.Schema.CLP_StartDate,
					CusCALPCO.Schema.CLP_HolderType
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => true;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_CommodityTypeCode,
			CusCALPCO.Schema.CLP_HolderContactEmail,
			CusCALPCO.Schema.CLP_HolderContactName,
			CusCALPCO.Schema.CLP_HolderContactPhone,
			CusCALPCO.Schema.CLP_IsHolderOverridden,
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.LPCOApplicantOrgPK,
			CusCALPCO.Schema.CLP_ApplicantType,
			CusCALPCO.Schema.CLP_ApplicantName,
			CusCALPCO.Schema.CLP_OA_Applicant,
			CusCALPCO.Schema.CLP_HolderType,
			CusCALPCO.Schema.LPCOHolderOrgPK,
			CusCALPCO.Schema.CLP_OA_Holder,
			CusCALPCO.Schema.CLP_HolderName,
			CusCALPCO.Schema.CLP_IssueDate,
			CusCALPCO.Schema.CLP_StartDate,
			CusCALPCO.Schema.CLP_ApplicantContactEmail,
			CusCALPCO.Schema.CLP_ApplicantContactName,
			CusCALPCO.Schema.CLP_ApplicantContactPhone,
			CusCALPCO.Schema.CLP_IsApplicantOverridden,
			CusCALPCO.Schema.CLP_AlternativeQuotaQuantity,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_SecondaryRefNo,
			CusCALPCO.Schema.CLP_Type,
			CusCALPCO.Schema.CLP_AlternativeQuotaUQ,
			CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
