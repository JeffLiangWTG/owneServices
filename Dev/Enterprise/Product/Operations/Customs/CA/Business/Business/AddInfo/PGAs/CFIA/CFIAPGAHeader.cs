using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CFIAPGAHeader :
		AutoCFIAPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter,
		ICusCodeDataTypeSupporter,
		IPurgeValueParent
	{
		public CFIAPGAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCFIAPGAHeader.Schema
		{
			public const string RN_NKCountryOfSource = "RN_NKCountryOfSource";
			public const string RW_NKSourceState = "RW_NKSourceState";
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
					lpcos.HasChangesChanged += LPCOs_HasChangesChanged;
				}
				return lpcos;
			}
		}
		CusCALPCOCollection lpcos;

		void LPCOs_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && !IsDeleted)
			{
				InvoiceLine?.SetAVSStatusIfNeeded();
			}
		}

		[ChildEditable]
		[PurgeValue(nameof(IsProgramDisabled))]
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
				dec.LPCOViews.CFIACountChanged -= ParentCFIACountChanged;
				dec.LPCOViews.CFIACountChanged += ParentCFIACountChanged;
			}
		}

		void ParentCFIACountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		#endregion

		#region Overrides

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				if (lpcos != null)
				{
					lpcos.HasChangesChanged -= LPCOs_HasChangesChanged;
				}
				LPCOViews.RemoveAndDeleteAll();

				if (fAIRSRegistrationNumbers != null)
				{
					fAIRSRegistrationNumbers.HasChangesChanged -= FAIRSRegistrationNumbers_HasChangesChanged;
				}
				AIRSRegistrationNumbers.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case CFIAPGADepartmentCodes.Codes.ALL:
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
				case CFIAPGADepartmentCodes.Codes.ALL:
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

		#region Properties

		public override ZString CA_AllProgramInd
		{
			get => base.CA_AllProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoCFIAPGAHeader.Schema.CA_AllProgramInd))
				{
					var oldValue = base.CA_AllProgramInd;
					if (oldValue != value)
					{
						base.CA_AllProgramInd = value;
						PurgeValuesIfNeeded(oldValue);
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.EndUseCodes))]
		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_AIRSEndUse
		{
			get { return base.CA_AIRSEndUse; }
			set
			{
				var oldValue = CA_AIRSEndUse;
				base.CA_AIRSEndUse = value;
				if (!IsCopying && oldValue != CA_AIRSEndUse)
				{
					InvoiceLine?.SetAVSStatusIfNeeded();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.AirsMiscellaneous))]
		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_AIRSMiscellaneous
		{
			get { return base.CA_AIRSMiscellaneous; }
			set
			{
				var oldValue = CA_AIRSMiscellaneous;
				base.CA_AIRSMiscellaneous = value;
				if (!IsCopying && oldValue != CA_AIRSMiscellaneous)
				{
					InvoiceLine?.SetAVSStatusIfNeeded();
				}
			}
		}

		#region AIRSRegistrationNumbers

		[ChildEditable(true)]
		[PurgeValue(nameof(IsProgramDisabled))]
		public AIRSRegistrationNumberCollection AIRSRegistrationNumbers
		{
			get
			{
				if (fAIRSRegistrationNumbers == null)
				{
					fAIRSRegistrationNumbers = new AIRSRegistrationNumberCollection(this);
					fAIRSRegistrationNumbers.Load();
					RegisterEditableChildObject(fAIRSRegistrationNumbers);
					fAIRSRegistrationNumbers.HasChangesChanged += FAIRSRegistrationNumbers_HasChangesChanged;
					fAIRSRegistrationNumbers.MaxCountValidationEnable(maxAIRSRows);
				}
				return fAIRSRegistrationNumbers;
			}
		}
		int maxAIRSRows { get { return 99; } }

		void FAIRSRegistrationNumbers_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged)
			{
				if (InvoiceLine != null && !IsDeleted)
				{
					InvoiceLine.SetAVSStatusIfNeeded();
				}
			}
		}

		AIRSRegistrationNumberCollection fAIRSRegistrationNumbers;

		#endregion

		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_AIRSExtensionCode
		{
			get { return base.CA_AIRSExtensionCode; }
			set
			{
				var oldValue = CA_AIRSExtensionCode;
				base.CA_AIRSExtensionCode = value;
				if (!IsCopying && oldValue != CA_AIRSExtensionCode)
				{
					InvoiceLine?.SetAVSStatusIfNeeded();
				}
			}
		}

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.CFIA;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			CFIAPGAHeader header = (CFIAPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
			AIRSRegistrationNumbers.CopyPersistentValuesFrom(header.AIRSRegistrationNumbers);
		}

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this, true);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			return new Dictionary<ZString, Type>();
		}

		#endregion

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceline == null || invoiceline.PK != B7_ParentID || B7_ParentTableCode != JobComInvoiceLineSchema.Constants.Prefix)
				{
					invoiceline = B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? (JobComInvoiceLine)Parent : null;
				}
				return invoiceline;
			}
		}
		JobComInvoiceLine invoiceline;

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
					CusCALPCO.Schema.CLP_RefNo
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => true;
		#endregion

		#region IPurgeValueParent

		bool IsProgramDisabled => CA_AllProgramInd != YesNoList.Codes.Yes;

		void PurgeValuesIfNeeded(ZString oldValue)
		{
			if (!IsCopying && oldValue == YesNoList.Codes.Yes)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeWithMacroHelper ?? (purgeWithMacroHelper = new PurgeValueHelper<CFIAPGAHeader>(this)); }
		}
		PurgeValueHelper<CFIAPGAHeader> purgeWithMacroHelper;

		bool IPurgeValueParent.IsPurging { get; set; }

		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		IEnumerable<ZPropertyInfo> FieldsPropertyInfos
		{
			get
			{
				var result = this.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.Wrapping).Cast<ZPropertyInfo>().
				Where(x => x.Name != CFIAPGAHeaderAddInfoSchema.Constants.CA_AllProgramInd).ToList();
				var invoiceLine = InvoiceLine;
				if (invoiceline != null)
				{
					result.AddRange(new[] { invoiceLine.CA_RN_NKSourceInfo, invoiceLine.CA_StateOfSourceInfo, invoiceLine.JI_OA_ConsigneeAddressInfo });
				}

				return result;
			}
		}

		public bool IsBlank => FieldsPropertyInfos.All(x => x.Value.IsEmpty)
				&& LPCOs.Count == 0 && AIRSRegistrationNumbers.Count == 0;

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AIRSNumber, typeof(AIRSRegistrationNumber));
			return result;
		}

		#endregion

		#region IHasPGARequirements

		public IHasPGARequirements RequirementsParent => (IHasPGARequirements)Parent;

		[List(nameof(AddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.CountryOfSourceLookup))]
		[MaxLength(AutoCAAddInfo.Schema.CA_CFIACountryOfSourceMaxLength)]
		public ZString RN_NKCountryOfSource
		{
			get => RequirementsParent?.RN_NKCountryOfSource ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.RN_NKCountryOfSource = value;
				}
			}
		}
		public ZPropertyInfo RN_NKCountryOfSourceInfo => GetWrappedZPropertyInfo(nameof(RN_NKCountryOfSource), x => RequirementsParent?.RN_NKCountryOfSourceInfo ?? GetZPropertyInfo(Schema.RN_NKCountryOfSource));

		[List(nameof(AddInfoLookups) + "." + nameof(CFIAPGAHeaderAddInfoLookups.CountryOfSourceStateLookup))]
		[MaxLength(2)]
		public ZString RW_NKSourceState
		{
			get => RequirementsParent?.RW_NKCountryOfSourceState ?? ZString.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.RW_NKCountryOfSourceState = value;
				}
			}
		}
		public ZPropertyInfo RW_NKSourceStateInfo => GetWrappedZPropertyInfo(nameof(RW_NKSourceState), x => RequirementsParent?.RW_NKCountryOfSourceStateInfo ?? GetZPropertyInfo(Schema.RW_NKSourceState));

		#endregion

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion

		public void CopyAIRSToCFIA(AIRSWebpageNavigator navigator)
		{
			if (!navigator.AG_EndUseCode.IsEmpty)
			{
				this.CA_AIRSEndUse = navigator.AG_EndUseCode;
			}

			if (!navigator.AG_ExtensionCode.IsEmpty)
			{
				this.CA_AIRSExtensionCode = navigator.AG_ExtensionCode;
			}

			if (!navigator.AG_Miscellaneous.IsEmpty)
			{
				this.CA_AIRSMiscellaneous = navigator.AG_Miscellaneous;
			}

			var set = navigator.LPCOList.FirstOrDefault(x => x.AL_DataSetSelected);
			if (set != null)
			{
				set.MaterializedLPCOs?.GetAllCodes().ForEach(x => this.LPCOViews.AddNewIfNotExist(x));
				set.DeMaterializedLPCOs?.GetAllCodes().ForEach(x => this.LPCOViews.AddNewIfNotExist(x));
				set.AIRSRegistrations?.GetAllCodes().ForEach(x => this.AIRSRegistrationNumbers.AddNewIfNotExist(x));
			}
		}
	}
}
