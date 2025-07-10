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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class HCPGAHeader :
		AutoHCPGAHeader,
		IPGAProgramRequirementProvider,
		ICusAddInfoTypeSupporter,
		IPurgeValueParent,
		ILPCOCollectionParent,
		ICADeclarationProvider,
		ILPCODefaulter
	{
		public HCPGAHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoHCPGAHeader.Schema
		{
			public const string OA_Manufacturer = "OA_Manufacturer";
		}

		#region Related

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.UNDGCodeList))]
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

		public ZPropertyInfo DangerousGoodsDGSubsInfo => GetZPropertyInfo(nameof(DangerousGoodsDGSubs));

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
		[PurgeValue(nameof(AllProgramsDisabled))]
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
				dec.LPCOViews.HCCountChanged -= ParentHCCountChanged;
				dec.LPCOViews.HCCountChanged += ParentHCCountChanged;
			}
		}

		void ParentHCCountChanged(object sender, EventArgs e)
		{
			lpcoViews = null;
			this.RefreshBindingIncludingChildren();
		}

		[ChildEditable]
		[PurgeValue(nameof(APIOCSPESDisabled))]
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

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new HCPGAHeaderValidation(this);
		}
		public new HCPGAHeaderValidation Validation => (HCPGAHeaderValidation)base.Validation;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				LPCOViews.RemoveAndDeleteAll();
				Components.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public override ZString CA_APIProgramInd
		{
			get => base.CA_APIProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_APIProgramInd))
				{
					var oldValue = base.CA_APIProgramInd;
					if (oldValue != value)
					{
						base.CA_APIProgramInd = value;
						if (CA_APIProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();

							SetDefaultIntendedUseCode(HCPGADepartmentCodes.Codes.API, CA_IntendedUseCodeAPI);
							SetDefaultCategory(HCPGADepartmentCodes.Codes.API, AddInfoLookups.CategoryCodesAPI, CA_CategoryAPI);
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_BBCProgramInd
		{
			get => base.CA_BBCProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_BBCProgramInd))
				{
					var oldValue = base.CA_BBCProgramInd;
					if (oldValue != value)
					{
						base.CA_BBCProgramInd = value;

						PurgeValuesIfNeed(oldValue);
						if (CA_BBCProgramInd == YesNoList.Codes.Yes)
						{
							SetDefaultIntendedUseCode(HCPGADepartmentCodes.Codes.BBC, CA_IntendedUseCodeBBC);
							SetDefaultCategory(HCPGADepartmentCodes.Codes.BBC, AddInfoLookups.CategoryCodesBBC, CA_CategoryBBC);
						}
					}
				}
			}
		}

		public override ZString CA_CPRProgramInd
		{
			get => base.CA_CPRProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_CPRProgramInd))
				{
					var oldValue = base.CA_CPRProgramInd;
					if (oldValue != value)
					{
						InvoiceLine?.InvoiceHeader?.RefreshInvoiceLinesWithCPRIndOnHCPGA();
						base.CA_CPRProgramInd = value;
						if (CA_CPRProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_CTOProgramInd
		{
			get => base.CA_CTOProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_CTOProgramInd))
				{
					var oldValue = base.CA_CTOProgramInd;
					if (oldValue != value)
					{
						base.CA_CTOProgramInd = value;
						if (CA_CTOProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
							SetDefaultIntendedUseCode(HCPGADepartmentCodes.Codes.CTO, CA_IntendedUseCodeCTO);
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_DSEProgramInd
		{
			get => base.CA_DSEProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_DSEProgramInd))
				{
					var oldValue = base.CA_DSEProgramInd;
					if (oldValue != value)
					{
						base.CA_DSEProgramInd = value;
						if (CA_DSEProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
							SetDefaultIntendedUseCode(HCPGADepartmentCodes.Codes.DSE, CA_IntendedUseCodeDSE);
							SetDefaultCategory(HCPGADepartmentCodes.Codes.DSE, AddInfoLookups.CategoryCodesDSE, CA_CategoryDSE);
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_HDRProgramInd
		{
			get => base.CA_HDRProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_HDRProgramInd))
				{
					var oldValue = base.CA_HDRProgramInd;
					if (oldValue != value)
					{
						base.CA_HDRProgramInd = value;
						if (CA_HDRProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_MDEProgramInd
		{
			get => base.CA_MDEProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_MDEProgramInd))
				{
					var oldValue = base.CA_MDEProgramInd;
					if (oldValue != value)
					{
						base.CA_MDEProgramInd = value;
						if (CA_MDEProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_NHPProgramInd
		{
			get => base.CA_NHPProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_NHPProgramInd))
				{
					var oldValue = base.CA_NHPProgramInd;
					if (oldValue != value)
					{
						base.CA_NHPProgramInd = value;
						if (CA_NHPProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
							SetDefaultIntendedUseCode(HCPGADepartmentCodes.Codes.NHP, CA_IntendedUseCodeNHP);
							SetDefaultCategory(HCPGADepartmentCodes.Codes.NHP, AddInfoLookups.CategoryCodesNHP, CA_CategoryNHP);
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_OCSProgramInd
		{
			get => base.CA_OCSProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_OCSProgramInd))
				{
					var oldValue = base.CA_OCSProgramInd;
					if (oldValue != value)
					{
						base.CA_OCSProgramInd = value;
						if (CA_OCSProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_PESProgramInd
		{
			get => base.CA_PESProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_PESProgramInd))
				{
					var oldValue = base.CA_PESProgramInd;
					if (oldValue != value)
					{
						InvoiceLine?.InvoiceHeader?.RefreshInvoiceLinesWithPESIndOnHCPGA();
						base.CA_PESProgramInd = value;
						if (CA_PESProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_REDProgramInd
		{
			get => base.CA_REDProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_REDProgramInd))
				{
					var oldValue = base.CA_REDProgramInd;
					if (oldValue != value)
					{
						base.CA_REDProgramInd = value;
						if (CA_REDProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		public override ZString CA_VETProgramInd
		{
			get => base.CA_VETProgramInd;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(AutoHCPGAHeader.Schema.CA_VETProgramInd))
				{
					var oldValue = base.CA_VETProgramInd;
					if (oldValue != value)
					{
						base.CA_VETProgramInd = value;
						if (CA_VETProgramInd == YesNoList.Codes.Yes)
						{
							LPCOViews.AddDefaultLPCOs();
						}

						PurgeValuesIfNeed(oldValue);
					}
				}
			}
		}

		void PurgeValuesIfNeed(ZString oldValue)
		{
			if (!IsCopying && oldValue == YesNoList.Codes.Yes)
			{
				((IPurgeValueParent)this).PurgeHelper.PurgeAllValues();
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesAPI))]
		public override ZString CA_IntendedUseCodeAPI
		{
			get => base.CA_IntendedUseCodeAPI;
			set
			{
				if (base.CA_IntendedUseCodeAPI != value)
				{
					base.CA_IntendedUseCodeAPI = value;
					if (AddInfoLookups.IntendedUseCodesAPI.ContainsCode(CA_IntendedUseCodeAPI))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.API, AddInfoLookups.CategoryCodesAPI, CA_CategoryAPI);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesAPI))]
		public override ZString CA_CategoryAPI
		{
			get => base.CA_CategoryAPI;
			set
			{
				if (base.CA_CategoryAPI != value)
				{
					base.CA_CategoryAPI = value;
					if (AddInfoLookups.CategoryCodesAPI.ContainsCode(CA_CategoryAPI))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesBBC))]
		public override ZString CA_IntendedUseCodeBBC
		{
			get => base.CA_IntendedUseCodeBBC;
			set
			{
				if (base.CA_IntendedUseCodeBBC != value)
				{
					base.CA_IntendedUseCodeBBC = value;
					if (AddInfoLookups.IntendedUseCodesBBC.ContainsCode(CA_IntendedUseCodeBBC))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.BBC, AddInfoLookups.CategoryCodesBBC, CA_CategoryBBC);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesBBC))]
		public override ZString CA_CategoryBBC
		{
			get => base.CA_CategoryBBC;
			set
			{
				if (base.CA_CategoryBBC != value)
				{
					base.CA_CategoryBBC = value;
					if (AddInfoLookups.CategoryCodesBBC.ContainsCode(CA_CategoryBBC))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesCTO))]
		public override ZString CA_IntendedUseCodeCTO
		{
			get => base.CA_IntendedUseCodeCTO;
			set
			{
				if (base.CA_IntendedUseCodeCTO != value)
				{
					base.CA_IntendedUseCodeCTO = value;
					if (AddInfoLookups.IntendedUseCodesBBC.ContainsCode(CA_IntendedUseCodeCTO))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.CTO, AddInfoLookups.CategoryCodesCTO, CA_CategoryCTO);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesCTO))]
		public override ZString CA_CategoryCTO
		{
			get => base.CA_CategoryCTO;
			set
			{
				if (base.CA_CategoryCTO != value)
				{
					base.CA_CategoryCTO = value;
					if (AddInfoLookups.CategoryCodesCTO.ContainsCode(CA_CategoryCTO))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesCPR))]
		public override ZString CA_IntendedUseCodeCPR
		{
			get => base.CA_IntendedUseCodeCPR;
			set
			{
				if (base.CA_IntendedUseCodeCPR != value)
				{
					base.CA_IntendedUseCodeCPR = value;
					if (AddInfoLookups.IntendedUseCodesBBC.ContainsCode(CA_IntendedUseCodeCPR))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.CPR, AddInfoLookups.CategoryCodesCPR, CA_CategoryCPR);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesCPR))]
		public override ZString CA_CategoryCPR
		{
			get => base.CA_CategoryCPR;
			set
			{
				if (base.CA_CategoryCPR != value)
				{
					base.CA_CategoryCPR = value;
					if (AddInfoLookups.CategoryCodesCPR.ContainsCode(CA_CategoryCPR))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesDSE))]
		public override ZString CA_IntendedUseCodeDSE
		{
			get => base.CA_IntendedUseCodeDSE;
			set
			{
				if (base.CA_IntendedUseCodeDSE != value)
				{
					base.CA_IntendedUseCodeDSE = value;
					if (AddInfoLookups.IntendedUseCodesDSE.ContainsCode(CA_IntendedUseCodeDSE))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.DSE, AddInfoLookups.CategoryCodesDSE, CA_CategoryDSE);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesDSE))]
		public override ZString CA_CategoryDSE
		{
			get => base.CA_CategoryDSE;
			set
			{
				if (base.CA_CategoryDSE != value)
				{
					base.CA_CategoryDSE = value;
					if (AddInfoLookups.CategoryCodesDSE.ContainsCode(CA_CategoryDSE))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesHDR))]
		public override ZString CA_IntendedUseCodeHDR
		{
			get => base.CA_IntendedUseCodeHDR;
			set
			{
				if (base.CA_IntendedUseCodeHDR != value)
				{
					base.CA_IntendedUseCodeHDR = value;
					if (AddInfoLookups.IntendedUseCodesHDR.ContainsCode(CA_IntendedUseCodeHDR))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.HDR, AddInfoLookups.CategoryCodesHDR, CA_CategoryHDR);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesHDR))]
		public override ZString CA_CategoryHDR
		{
			get => base.CA_CategoryHDR;
			set
			{
				if (base.CA_CategoryHDR != value)
				{
					base.CA_CategoryHDR = value;
					if (AddInfoLookups.CategoryCodesHDR.ContainsCode(CA_CategoryHDR))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesOCS))]
		public override ZString CA_IntendedUseCodeOCS
		{
			get => base.CA_IntendedUseCodeOCS;
			set
			{
				if (base.CA_IntendedUseCodeOCS != value)
				{
					base.CA_IntendedUseCodeOCS = value;
					if (AddInfoLookups.IntendedUseCodesOCS.ContainsCode(CA_IntendedUseCodeOCS))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.OCS, AddInfoLookups.CategoryCodesOCS, CA_CategoryOCS);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesOCS))]
		public override ZString CA_CategoryOCS
		{
			get => base.CA_CategoryOCS;
			set
			{
				if (base.CA_CategoryOCS != value)
				{
					base.CA_CategoryOCS = value;
					if (AddInfoLookups.CategoryCodesOCS.ContainsCode(CA_CategoryOCS))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesMDE))]
		public override ZString CA_IntendedUseCodeMDE
		{
			get => base.CA_IntendedUseCodeMDE;
			set
			{
				if (base.CA_IntendedUseCodeMDE != value)
				{
					base.CA_IntendedUseCodeMDE = value;
					if (AddInfoLookups.IntendedUseCodesMDE.ContainsCode(CA_IntendedUseCodeMDE))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.MDE, AddInfoLookups.CategoryCodesMDE, CA_CategoryMDE);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesMDE))]
		public override ZString CA_CategoryMDE
		{
			get => base.CA_CategoryMDE;
			set
			{
				if (base.CA_CategoryMDE != value)
				{
					base.CA_CategoryMDE = value;
					if (AddInfoLookups.CategoryCodesMDE.ContainsCode(CA_CategoryMDE))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesNHP))]
		public override ZString CA_IntendedUseCodeNHP
		{
			get => base.CA_IntendedUseCodeNHP;
			set
			{
				if (base.CA_IntendedUseCodeNHP != value)
				{
					base.CA_IntendedUseCodeNHP = value;
					if (AddInfoLookups.IntendedUseCodesNHP.ContainsCode(CA_IntendedUseCodeNHP))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.NHP, AddInfoLookups.CategoryCodesNHP, CA_CategoryNHP);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesNHP))]
		public override ZString CA_CategoryNHP
		{
			get => base.CA_CategoryNHP;
			set
			{
				if (base.CA_CategoryNHP != value)
				{
					base.CA_CategoryNHP = value;
					if (AddInfoLookups.CategoryCodesNHP.ContainsCode(CA_CategoryNHP))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesPES))]
		public override ZString CA_IntendedUseCodePES
		{
			get => base.CA_IntendedUseCodePES;
			set
			{
				if (base.CA_IntendedUseCodePES != value)
				{
					base.CA_IntendedUseCodePES = value;
					if (AddInfoLookups.IntendedUseCodesPES.ContainsCode(CA_IntendedUseCodePES))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.PES, AddInfoLookups.CategoryCodesPES, CA_CategoryPES);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesPES))]
		public override ZString CA_CategoryPES
		{
			get => base.CA_CategoryPES;
			set
			{
				if (base.CA_CategoryPES != value)
				{
					base.CA_CategoryPES = value;
					if (AddInfoLookups.CategoryCodesPES.ContainsCode(CA_CategoryPES))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesRED))]
		public override ZString CA_IntendedUseCodeRED
		{
			get => base.CA_IntendedUseCodeRED;
			set
			{
				if (base.CA_IntendedUseCodeRED != value)
				{
					base.CA_IntendedUseCodeRED = value;
					if (AddInfoLookups.IntendedUseCodesRED.ContainsCode(CA_IntendedUseCodeRED))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.RED, AddInfoLookups.CategoryCodesRED, CA_CategoryRED);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesRED))]
		public override ZString CA_CategoryRED
		{
			get => base.CA_CategoryRED;
			set
			{
				if (base.CA_CategoryRED != value)
				{
					base.CA_CategoryRED = value;
					if (AddInfoLookups.CategoryCodesRED.ContainsCode(CA_CategoryRED))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.IntendedUseCodesVET))]
		public override ZString CA_IntendedUseCodeVET
		{
			get => base.CA_IntendedUseCodeVET;
			set
			{
				if (base.CA_IntendedUseCodeVET != value)
				{
					base.CA_IntendedUseCodeVET = value;
					if (AddInfoLookups.IntendedUseCodesVET.ContainsCode(CA_IntendedUseCodeVET))
					{
						LPCOViews.AddDefaultLPCOs();
						SetDefaultCategory(HCPGADepartmentCodes.Codes.VET, AddInfoLookups.CategoryCodesVET, CA_CategoryVET);
					}
					AddInfoValidation.ValidateCA_ComplianceStatement();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.CategoryCodesVET))]
		public override ZString CA_CategoryVET
		{
			get => base.CA_CategoryVET;
			set
			{
				if (base.CA_CategoryVET != value)
				{
					base.CA_CategoryVET = value;
					if (AddInfoLookups.CategoryCodesVET.ContainsCode(CA_CategoryVET))
					{
						LPCOViews.AddDefaultLPCOs();
					}
				}
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void SetFirstIntendedUseCode(ZString programCode, ZString firstCode)
		{
			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					CA_IntendedUseCodeAPI = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.BBC:
					CA_IntendedUseCodeBBC = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.CTO:
					CA_IntendedUseCodeCTO = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.CPR:
					CA_IntendedUseCodeCPR = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.DSE:
					CA_IntendedUseCodeDSE = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.HDR:
					CA_IntendedUseCodeHDR = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.NHP:
					CA_IntendedUseCodeNHP = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.OCS:
					CA_IntendedUseCodeOCS = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.MDE:
					CA_IntendedUseCodeMDE = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.PES:
					CA_IntendedUseCodePES = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.RED:
					CA_IntendedUseCodeRED = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.VET:
					CA_IntendedUseCodeVET = firstCode;
					break;
				default:
					break;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void SetFirstCategory(ZString programCode, ZString firstCode)
		{
			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					CA_CategoryAPI = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.BBC:
					CA_CategoryBBC = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.CTO:
					CA_CategoryCTO = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.CPR:
					CA_CategoryCPR = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.DSE:
					CA_CategoryDSE = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.HDR:
					CA_CategoryHDR = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.NHP:
					CA_CategoryNHP = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.OCS:
					CA_CategoryOCS = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.MDE:
					CA_CategoryMDE = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.PES:
					CA_CategoryPES = firstCode;
					break;
				case HCPGADepartmentCodes.Codes.VET:
					CA_CategoryVET = firstCode;
					break;
				default:
					break;
			}
		}

		void SetDefaultIntendedUseCode(ZString programCode, ZString intendedUseCode)
		{
			if (intendedUseCode.IsEmpty && !IsDefaultValuesSuspended)
			{
				var intendedUseCodeList = HCIntendedUseCode.GetIntendedCodeForProgram(programCode);
				if (intendedUseCodeList.Count == 1)
				{
					SetFirstIntendedUseCode(programCode, intendedUseCodeList[0].Code);
				}
			}
		}

		void SetDefaultCategory(ZString programCode, CodeDescriptionPairList categoryList, ZString categoryValue)
		{
			if (categoryValue.IsEmpty && !IsDefaultValuesSuspended)
			{
				if (categoryList.Count == 1)
				{
					SetFirstCategory(programCode, categoryList[0].Code);
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.ExceptProcessingCodes))]
		[PurgeValue(nameof(CTOPESMDEDisabled))]
		public override ZString CA_ExceptProcessingCode1
		{
			get => base.CA_ExceptProcessingCode1;
			set => base.CA_ExceptProcessingCode1 = value;
		}

		[List(nameof(AddInfoLookups) + "." + nameof(HCPGAHeaderAddInfoLookups.ExceptProcessingCodes))]
		public override ZString CA_ExceptProcessingCode2
		{
			get => base.CA_ExceptProcessingCode2;
			set => base.CA_ExceptProcessingCode2 = value;
		}

		[PurgeValue(nameof(AllBatchLotNumberDisabled))]
		public override ZString CA_BatchLotNumber
		{
			get => base.CA_BatchLotNumber;
			set => base.CA_BatchLotNumber = value;
		}

		[PurgeValue(nameof(AllGTINDisabled))]
		public override ZString CA_GTINNumber
		{
			get => base.CA_GTINNumber;
			set => base.CA_GTINNumber = value;
		}

		#endregion

		#region IHasPGARequirements

		[List(nameof(OA_Manufacturer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid OA_Manufacturer
		{
			get => RequirementsParent?.OA_Manufacturer ?? ZGuid.Empty;
			set
			{
				if (RequirementsParent is IHasPGARequirements requirementsParent)
				{
					requirementsParent.OA_Manufacturer = value;
				}
			}
		}
		public ZPropertyInfo OA_ManufacturerInfo => GetWrappedZPropertyInfo(nameof(OA_Manufacturer), x => RequirementsParent?.OA_ManufacturerInfo ?? GetZPropertyInfo(Schema.OA_Manufacturer));

		public ZAddress OA_Manufacturer_ZAddress
		{
			get
			{
				if (cachedOA_Manufacturer_ZAddress == null)
				{
					cachedOA_Manufacturer_ZAddress = new CachedProperty<ZAddress>(Factory, () =>
					{
						if (RequirementsParent is IHasPGARequirements requirementsParent)
						{
							return requirementsParent.OA_ManufacturerAddress_ZAddress;
						}
						else
						{
							return new ZAddress(OA_ManufacturerInfo);
						}
					});
				}
				return cachedOA_Manufacturer_ZAddress.Value;
			}
		}
		CachedProperty<ZAddress> cachedOA_Manufacturer_ZAddress;

		public IHasPGARequirements RequirementsParent => (IHasPGARequirements)Parent;

		#endregion

		#region IPGAHeader

		ZString IPGAHeader.GovAgencyIDCode => PGACodes.Codes.HC;

		IHasPGARequirements IPGAHeader.Parent => Parent as IHasPGARequirements;

		void IPGAHeader.CopyPersistentValuesFrom(IPGAHeader source)
		{
			HCPGAHeader header = (HCPGAHeader)source;

			CopyPersistentValuesFrom(header);
			header.lpcoViews = null;
			LPCOViews.CopyValueFrom(header.LPCOViews);
			Components.CopyPersistentValuesFrom(header.Components);
		}

		#endregion

		#region IPGAProgramRequirementProvider Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ZPropertyInfo IPGAProgramRequirementProvider.GetProgramIndicatorInfo(ZString programCode)
		{
			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					return CA_APIProgramIndInfo;
				case HCPGADepartmentCodes.Codes.BBC:
					return CA_BBCProgramIndInfo;
				case HCPGADepartmentCodes.Codes.CPR:
					return CA_CPRProgramIndInfo;
				case HCPGADepartmentCodes.Codes.CTO:
					return CA_CTOProgramIndInfo;
				case HCPGADepartmentCodes.Codes.DSE:
					return CA_DSEProgramIndInfo;
				case HCPGADepartmentCodes.Codes.HDR:
					return CA_HDRProgramIndInfo;
				case HCPGADepartmentCodes.Codes.MDE:
					return CA_MDEProgramIndInfo;
				case HCPGADepartmentCodes.Codes.NHP:
					return CA_NHPProgramIndInfo;
				case HCPGADepartmentCodes.Codes.OCS:
					return CA_OCSProgramIndInfo;
				case HCPGADepartmentCodes.Codes.PES:
					return CA_PESProgramIndInfo;
				case HCPGADepartmentCodes.Codes.RED:
					return CA_REDProgramIndInfo;
				case HCPGADepartmentCodes.Codes.VET:
					return CA_VETProgramIndInfo;
			}

			return null;
		}

		CodeDescriptionPairList IPGAProgramRequirementProvider.GetProgramCodesList()
		{
			return AddInfoLookups.ProgramCodesList;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void IPGAProgramRequirementProvider.ValidateProgramIndicator(ZString programCode)
		{
			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					AddInfoValidation.ValidateCA_APIProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.BBC:
					AddInfoValidation.ValidateCA_BBCProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.CPR:
					AddInfoValidation.ValidateCA_CPRProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.CTO:
					AddInfoValidation.ValidateCA_CTOProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.DSE:
					AddInfoValidation.ValidateCA_DSEProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.HDR:
					AddInfoValidation.ValidateCA_HDRProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.MDE:
					AddInfoValidation.ValidateCA_MDEProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.NHP:
					AddInfoValidation.ValidateCA_NHPProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.OCS:
					AddInfoValidation.ValidateCA_OCSProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.PES:
					AddInfoValidation.ValidateCA_PESProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.RED:
					AddInfoValidation.ValidateCA_REDProgramInd();
					break;
				case HCPGADepartmentCodes.Codes.VET:
					AddInfoValidation.ValidateCA_VETProgramInd();
					break;
			}
		}

		IDisposable IPGAProgramRequirementProvider.SuspendSettingDefaultValues()
		{
			return new SettingDefaultValuesSuspender(this);
		}

		internal bool IsDefaultValuesSuspended
		{
			get { return suspenderIndex > 0; }
		}

		byte suspenderIndex;

		sealed class SettingDefaultValuesSuspender : IDisposable
		{
			public SettingDefaultValuesSuspender(HCPGAHeader pgaheader)
			{
				this.pgaheader = pgaheader;
				pgaheader.suspenderIndex++;
			}
			readonly HCPGAHeader pgaheader;

			void IDisposable.Dispose()
			{
				pgaheader.suspenderIndex--;
			}
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
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.CAComponent, typeof(Component));
			return result;
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

		public CusClassPartPivot PartPivot
		{
			get
			{
				if (cachedPartPivot == null || cachedPartPivot.PK != B7_ParentID || B7_ParentTableCode != CusClassPartPivotSchema.Constants.Prefix)
				{
					cachedPartPivot = B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? (CusClassPartPivot)Parent : null;
				}
				return cachedPartPivot;
			}
		}
		CusClassPartPivot cachedPartPivot;

		#region Properties

		public ZBool CA_MDE_LEX
		{
			get => base.CA_ExceptProcessingCode1 == HCExceptProcessingCodes.Codes.HC02;
			set
			{
				if (value.IsValid && value)
				{
					base.CA_ExceptProcessingCode1 = HCExceptProcessingCodes.Codes.HC02;
				}
				else
				{
					base.CA_ExceptProcessingCode1 = ZString.Empty;
				}
				CA_MDE_LEXInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_MDE_LEXInfo
		{
			get { return GetZPropertyInfo(nameof(CA_MDE_LEX)); }
		}

		public ZBool CA_PES_SPCP
		{
			get => base.CA_ExceptProcessingCode1 == HCExceptProcessingCodes.Codes.HC03;
			set
			{
				if (value.IsValid && value)
				{
					base.CA_ExceptProcessingCode1 = HCExceptProcessingCodes.Codes.HC03;
				}
				else
				{
					base.CA_ExceptProcessingCode1 = ZString.Empty;
				}
				CA_PES_SPCPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_PES_SPCPInfo
		{
			get { return GetZPropertyInfo(nameof(CA_PES_SPCP)); }
		}

		public ZBool CA_PES_EPCP
		{
			get => base.CA_ExceptProcessingCode2 == HCExceptProcessingCodes.Codes.HC04;
			set
			{
				if (value.IsValid && value)
				{
					base.CA_ExceptProcessingCode2 = HCExceptProcessingCodes.Codes.HC04;
				}
				else
				{
					base.CA_ExceptProcessingCode2 = ZString.Empty;
				}
				CA_PES_EPCPInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_PES_EPCPInfo
		{
			get { return GetZPropertyInfo(nameof(CA_PES_EPCP)); }
		}
		#endregion

		#region Cells Tissues and Organs

		public ZBool CA_CTO_LCO
		{
			get => base.CA_ExceptProcessingCode1 == HCExceptProcessingCodes.Codes.HC01;
			set
			{
				if (value.IsValid && value)
				{
					base.CA_ExceptProcessingCode1 = HCExceptProcessingCodes.Codes.HC01;
				}
				else
				{
					base.CA_ExceptProcessingCode1 = ZString.Empty;
				}
				CA_CTO_LCOInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CA_CTO_LCOInfo
		{
			get { return GetZPropertyInfo(nameof(CA_CTO_LCO)); }
		}

		#endregion

		#region Properties For Purge

		bool AllProgramsDisabled => IsAPIDisabled && IsBBCDisabled && IsCTODisabled && IsCPRDisabled && IsDSEDisabled && IsHDRDisabled && IsOCSDisabled && IsMDEDisabled && IsNHPDisabled && IsPESDisabled && IsREDDisabled && IsVETDisabled;

		bool AllBatchLotNumberDisabled => IsAPIDisabled && IsCPRDisabled && IsHDRDisabled && IsOCSDisabled && IsMDEDisabled && IsNHPDisabled && IsPESDisabled && IsVETDisabled;

		bool AllGTINDisabled => IsAPIDisabled && IsBBCDisabled && IsCTODisabled && IsCPRDisabled && IsHDRDisabled && IsMDEDisabled && IsNHPDisabled && IsVETDisabled;

		bool AllProgramsDisabledExceptAPI => IsBBCDisabled && IsCTODisabled && IsCPRDisabled && IsDSEDisabled && IsHDRDisabled && IsOCSDisabled && IsMDEDisabled && IsNHPDisabled && IsPESDisabled && IsREDDisabled && IsVETDisabled;

		bool APIOCSPESDisabled => IsAPIDisabled && IsOCSDisabled && IsPESDisabled;

		bool CTOPESMDEDisabled => IsCTODisabled && IsPESDisabled && IsMDEDisabled;

		ZBool IsAPIDisabled => CA_APIProgramInd != YesNoList.Codes.Yes;

		ZBool IsBBCDisabled => CA_BBCProgramInd != YesNoList.Codes.Yes;

		ZBool IsCTODisabled => CA_CTOProgramInd != YesNoList.Codes.Yes;

		ZBool IsCPRDisabled => CA_CPRProgramInd != YesNoList.Codes.Yes;

		ZBool IsDSEDisabled => CA_DSEProgramInd != YesNoList.Codes.Yes;

		ZBool IsHDRDisabled => CA_HDRProgramInd != YesNoList.Codes.Yes;

		ZBool IsOCSDisabled => CA_OCSProgramInd != YesNoList.Codes.Yes;

		ZBool IsMDEDisabled => CA_MDEProgramInd != YesNoList.Codes.Yes;

		ZBool IsNHPDisabled => CA_NHPProgramInd != YesNoList.Codes.Yes;

		ZBool IsPESDisabled => CA_PESProgramInd != YesNoList.Codes.Yes;

		ZBool IsREDDisabled => CA_REDProgramInd != YesNoList.Codes.Yes;

		ZBool IsVETDisabled => CA_VETProgramInd != YesNoList.Codes.Yes;

		#endregion

		#region IPurgeValueParent

		bool IPurgeValueParent.IsPurging { get; set; }

		IPurgeValueHelper IPurgeValueParent.PurgeHelper
		{
			get => purgeHelper ?? (purgeHelper = new PurgeValueHelper<HCPGAHeader>(this));
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
					CusCALPCO.Schema.CLP_RefNo
				};
			}
		}

		ZBool ILPCODefaulter.ShouldDefaultLPCOFields => !AllProgramsDisabledExceptAPI;
		#endregion

		public static ImmutableList<string> AvailableLPCOFields { get; } = ImmutableList.Create
		(
			CusCALPCO.Schema.CLP_DIFRefNumberOrLocation,
			CusCALPCO.Schema.CLP_RefNo,
			CusCALPCO.Schema.CLP_Type
		);

		#region SetterSuspender

		public SetterSuspender SetterSuspender => setterSuspender ?? (setterSuspender = new SetterSuspender());
		SetterSuspender setterSuspender;

		#endregion
	}
}
