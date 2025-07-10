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
	public class CNSCPGAHeader :
		AutoCNSCPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter,
		IPurgeValueParent
	{
		public CNSCPGAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
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
				dec.LPCOViews.CNSCCountChanged -= ParentCNSCCountChanged;
				dec.LPCOViews.CNSCCountChanged += ParentCNSCCountChanged;
			}
		}

		void ParentCNSCCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		[ChildEditable]
		[PurgeValue(nameof(IsProgramDisabled))]
		public ComponentCollection Components
		{
			get
			{
				if (components == null)
				{
					components = new ComponentCollection(this);
					components.Load();
					RegisterEditableChildObject(components);
				}
				return components;
			}
		}
		ComponentCollection components;

		#endregion

		#region Overrides

		#region Properties

		public override bool SupportsNotes
		{
			get { return false; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(CNSCPGAHeaderAddInfoLookups.Categories))]
		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_Category
		{
			get { return base.CA_Category; }
			set
			{
				var hasChanges = CA_Category != value;
				base.CA_Category = value;

				if (hasChanges && !IsCopying)
				{
					if (IsEquipment)
					{
						Components.RemoveAndDeleteAll();
					}
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						Validation.ValidateDangerousGoodsDGSubs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(CNSCPGAHeaderAddInfoLookups.PackUQList))]
		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_PackUQ
		{
			get { return base.CA_PackUQ; }
			set { base.CA_PackUQ = value; }
		}

		public override ZString CA_AllProgramInd
		{
			get => base.CA_AllProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.CA_AllProgramInd))
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

		[List(nameof(AddInfoLookups) + "." + nameof(CNSCPGAHeaderAddInfoLookups.UNDGCodeList))]
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

		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_NNIECRSchePartNo
		{
			get => base.CA_NNIECRSchePartNo;
			set => base.CA_NNIECRSchePartNo = value;
		}

		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZString CA_PackMarks
		{
			get => base.CA_PackMarks;
			set => base.CA_PackMarks = value;
		}

		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZDecimal CA_PackQty
		{
			get => base.CA_PackQty;
			set => base.CA_PackQty = value;
		}

		[PurgeValue(nameof(IsProgramDisabled))]
		public override ZInt CA_UnitQty
		{
			get => base.CA_UnitQty;
			set => base.CA_UnitQty = value;
		}

		#endregion

		#region Methods

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
				Components.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#endregion

		#endregion

		#region IPGAProgramRequirementProvider Members

		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case CNSCPGADepartmentCodes.Codes.ALL:
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
				case CNSCPGADepartmentCodes.Codes.ALL:
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

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.CNSC;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			CNSCPGAHeader header = (CNSCPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
			Components.CopyPersistentValuesFrom(header.Components);
		}

		#endregion

		#region Implementation

		#region Properties

		public ZBool IsEquipment
		{
			get { return CA_Category == CNSCCategories.Codes.NE; }
		}

		public ZBool IsRadiationDevice
		{
			get { return CA_Category == CNSCCategories.Codes.RD; }
		}

		public ZBool IsSubstance
		{
			get { return CA_Category == CNSCCategories.Codes.NS; }
		}

		public ZBool IsControlledSubstance
		{
			get { return CA_Category == CNSCCategories.Codes.CNS; }
		}

		#endregion

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusAddInfoTypeSupporterFetchStrategy(this, true);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CAComponent, typeof(Component));
			return result;
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
			get { return purgeWithMacroHelper ?? (purgeWithMacroHelper = new PurgeValueHelper<CNSCPGAHeader>(this)); }
		}
		PurgeValueHelper<CNSCPGAHeader> purgeWithMacroHelper;

		bool IPurgeValueParent.IsPurging { get; set; }

		#endregion

		#region Validation
		protected override CusAddInfoValidation GetNewValidation()
		{
			return new CNSCPGAHeaderValidation(this);
		}

		public new CNSCPGAHeaderValidation Validation => (CNSCPGAHeaderValidation)base.Validation;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && ((ICADeclarationProvider)this).IsValidationEnabled;
		}

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
					CusCALPCO.Schema.CLP_HolderType
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => true;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_IsHolderOverridden,
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_HolderType,
			CusCALPCO.Schema.LPCOHolderOrgPK,
			CusCALPCO.Schema.CLP_OA_Holder,
			CusCALPCO.Schema.CLP_HolderName,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
