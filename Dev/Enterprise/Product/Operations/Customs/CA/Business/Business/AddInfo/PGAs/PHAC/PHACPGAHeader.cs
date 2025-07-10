using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PHACPGAHeader :
		AutoPHACPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter,
		IPurgeValueParent
	{
		public PHACPGAHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString DepartmentCode => PHACPGADepartmentCodes.Codes.HAP;

		public ZPropertyInfo DepartmentCodeInfo => GetZPropertyInfo(nameof(DepartmentCode));

		#region Related

		[ChildEditable]
		CusCALPCOCollection LPCOs
		{
			get
			{
				if (lpcos == null)
				{
					lpcos = new CusCALPCOCollection(this);
					lpcos.MaxCountValidationEnable(1);
					lpcos.Load();
					RegisterEditableChildObject(lpcos);
				}
				return lpcos;
			}
		}
		CusCALPCOCollection lpcos;

		[ChildEditable]
		[PurgeValue(nameof(IsHAPDisabled))]
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
				dec.LPCOViews.PHACCountChanged -= ParentPHACCountChanged;
				dec.LPCOViews.PHACCountChanged += ParentPHACCountChanged;
			}
		}

		void ParentPHACCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		#endregion

		#region Overrides

		#region Properties

		public override ZString CA_HAPProgramInd
		{
			get => base.CA_HAPProgramInd;
			set
			{
				var oldValue = base.CA_HAPProgramInd;
				if (oldValue != value)
				{
					base.CA_HAPProgramInd = value;
					PurgeValuesIfNeeded(oldValue);
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(PHACPGAHeaderAddInfoLookups.Categories))]
		[PurgeValue(nameof(IsHAPDisabled))]
		public override ZString CA_Category
		{
			get => base.CA_Category;
			set => base.CA_Category = value;
		}

		[PurgeValue(nameof(IsHAPDisabled))]
		public override ZBool CA_ExceptPathogenToxin
		{
			get => base.CA_ExceptPathogenToxin;
			set => base.CA_ExceptPathogenToxin = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(PHACPGAHeaderAddInfoLookups.IntendedUseCodes))]
		[PurgeValue(nameof(IsHAPDisabled))]
		public override ZString CA_IntendedUseCode
		{
			get => base.CA_IntendedUseCode;
			set
			{
				if (base.CA_IntendedUseCode != value)
				{
					base.CA_IntendedUseCode = value;
					if (!IsValidationSuspended)
					{
						AddInfoValidation.ValidateCA_Category();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(PHACPGAHeaderAddInfoLookups.UNDGCodeList))]
		public ZGuid DangerousGoodsDGSubs
		{
			get
			{
				return InvoiceLine?.DangerousGoodsDGSubs ?? ZGuid.Empty;
			}
			set
			{
				if (InvoiceLine != null)
				{
					var oldValue = InvoiceLine.DangerousGoodsDGSubs;

					InvoiceLine.DangerousGoodsDGSubs = value;
					DangerousGoodsDGSubsInfo.RefreshBinding();

					if (oldValue != InvoiceLine.DangerousGoodsDGSubs && !IsCopying)
					{
						Validation.ValidateDangerousGoodsDGSubs();
					}
				}
			}
		}

		public UNDGDataItem DangerousGoods => InvoiceLine?.DangerousGoods;

		public ZPropertyInfo DangerousGoodsDGSubsInfo => GetZPropertyInfo(nameof(DangerousGoodsDGSubs));

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override bool SupportsNotes => false;

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.PHAC;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			PHACPGAHeader header = (PHACPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
		}

		#endregion

		internal ZBool IsPathogenToxinRequired => !CA_ExceptPathogenToxin && PHACEndUseCodes.IsPathogenToxinLicenceMandatory(CA_IntendedUseCode, CA_Category);

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case PHACPGADepartmentCodes.Codes.HAP:
					return CA_HAPProgramIndInfo;
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
				case PHACPGADepartmentCodes.Codes.HAP:
					AddInfoValidation.ValidateCA_HAPProgramInd();
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

		#region Validation

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new PHACPGAHeaderValidation(this);
		}

		public new PHACPGAHeaderValidation Validation => (PHACPGAHeaderValidation)base.Validation;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && ((ICADeclarationProvider)this).IsValidationEnabled;
		}

		#endregion

		public JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (cachedInvoiceLine == null || cachedInvoiceLine.PK != B7_ParentID || B7_ParentTableCode != JobComInvoiceLineSchema.Constants.Prefix)
				{
					cachedInvoiceLine = B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? (JobComInvoiceLine)Parent : null;
				}
				return cachedInvoiceLine;
			}
		}
		JobComInvoiceLine cachedInvoiceLine;

		#region IPurgeValueParent

		bool IsHAPDisabled => CA_HAPProgramInd != YesNoList.Codes.Yes;

		void PurgeValuesIfNeeded(ZString oldValue)
		{
			if (!IsCopying && oldValue == YesNoList.Codes.Yes)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get { return purgeWithMacroHelper ?? (purgeWithMacroHelper = new PurgeValueHelper<PHACPGAHeader>(this)); }
		}
		PurgeValueHelper<PHACPGAHeader> purgeWithMacroHelper;

		bool IPurgeValueParent.IsPurging { get; set; }

		#endregion

		#region IDeclarationProvider
		BaseJobDeclaration IDeclarationProvider.Declaration
		{
			get { return declaration ?? (declaration = (InvoiceLine as IDeclarationProvider)?.Declaration); }
		}
		BaseJobDeclaration declaration;

		ZBool ICADeclarationProvider.IsValidationEnabled => this.IsPGAValidationEnabled();
		#endregion

		#region ILPCODefaulter
		IEnumerable<ZString> ILPCODefaulter.LPCOFieldsDefaultFromURN
		{
			get
			{
				return new List<ZString>
				{
					CusCALPCO.Schema.CLP_Type,
					CusCALPCO.Schema.CLP_RefNo,
					CusCALPCO.Schema.CLP_EndDate,
					CusCALPCO.Schema.CLP_StartDate
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => true;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_EndDate,
			CusCALPCO.Schema.CLP_StartDate,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
