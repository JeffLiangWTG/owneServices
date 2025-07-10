using System.Collections.Generic;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Action = {SU_MenuName}")]
	public class OperationalAction : AutoStmMenuItem, ICanDelete, IOperationalActionMenuEditable, IDataVersionLoggingSupported, IAuditParent
	{
		#region Schema

		public new abstract class Schema : AutoStmMenuItem.Schema
		{
			public const string SU_Calc_IsPublished = "SU_Calc_IsPublished";
		}

		#endregion

		public OperationalAction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		MultilingualString menuNameMultilingual;
		public MultilingualString MenuNameMultilingual
		{
			get
			{
				if (menuNameMultilingual == null)
				{
					var menuItem = Factory.Load<StmMenuItem>(PK);
					if (menuItem != null)
					{
						menuNameMultilingual = menuItem.SU_MenuNameMultilingual;
					}
					else
					{
						menuNameMultilingual = (NoResString)SU_MenuName;
					}
				}
				return menuNameMultilingual;
			}
		}

		public OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context; }
			set
			{
				context = value;
				SU_BusinessContext = (value == null) ? ZString.Empty : (ZString)GetFullBusinessContext(value.Supporter.BusinessContext);
			}
		}

		public MenuEditingMode EditingMode
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return editingMode; }
			set
			{
				editingMode = value;
				if (documentPivots != null)
				{
					documentPivots.EditingMode = value;
				}
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || OperationalActionMenuEditableHelper.ReadOnly(this); }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.ReadOnly = value; }
		}

		public static string GetFullBusinessContext(BusinessContext businessContext)
		{
			return businessContext.ToString();
		}

		public void EnsureFiltersForMethod(OperationalActionMethod method)
		{
			FilterRequirementList list;

			if (method != null && (list = method.GetFilterRequirements()).Count > 0)
			{
				try
				{
					IFilterExpression baseExpression = FilterParser.Parse(SU_FilterList);
					IFilterExpression compound = baseExpression;
					bool changed = false;

					foreach (FilterRequirement requirement in list)
					{
						if (baseExpression.IsRequirementEnforced(requirement))
						{
							continue;
						}

						compound = FilterTools.AddRequirement(compound, requirement);
						changed = true;
					}

					if (changed)
					{
						SU_FilterList = compound.ToString();
					}
				}
				catch (ParseException)
				{
					// ignore
				}
			}
		}

		[ResourceStringData("8E00375F-B043-48B3-9240-8550076142A0", Caption = "Operational Action", FullDescription = "Perform Operational Actions on multiple documents")]
		[OperationalActionTranslatableDataField(Schema.SU_MenuName, MaxLength = Schema.SU_MenuNameMaxLength, Type = typeof(OperationalAction), Asmid = ResString.AssemblyId)]
		public override ZString SU_MenuName
		{
			get { return base.SU_MenuName; }
			set { base.SU_MenuName = value; }
		}

		public MultilingualString SU_MenuNameMultilingual
		{
			get { return GetMultilingual(SU_MenuNameInfo); }
		}

		[OperationalActionTranslatableDataField(Schema.SU_MenuPath, MaxLength = Schema.SU_MenuPathMaxLength, Type = typeof(OperationalAction), Asmid = ResString.AssemblyId)]
		public override ZString SU_MenuPath
		{
			get { return base.SU_MenuPath; }
			set { base.SU_MenuPath = value; }
		}

		public MultilingualString SU_MenuPathMultilingual
		{
			get { return GetMultilingual(SU_MenuPathInfo); }
		}

		#region Related BusinessObjects

		[ChildEditable(true)]
		public OperationalActionDocumentPivotCollection DocumentPivots
		{
			get
			{
				if (documentPivots == null)
				{
					documentPivots = new OperationalActionDocumentPivotCollection(this);
					documentPivots.EditingMode = EditingMode;
					documentPivots.Load();
					RegisterEditableChildObject(documentPivots);
				}
				return documentPivots;
			}
		}

		public OperationalActionDocumentPivotView DocumentPivotsView
		{
			get { return documentPivotsView ?? (documentPivotsView = new OperationalActionDocumentPivotView(DocumentPivots)); }
		}

		[ChildEditable(true)]
		public OperationalActionFieldDescriptorCollection FieldDescriptors
		{
			get
			{
				if (fieldDescriptors == null)
				{
					fieldDescriptors = new OperationalActionFieldDescriptorCollection(this);
					fieldDescriptors.LoadFromBlob(SU_ActionDataUpdateBlob);
					RegisterEditableChildObject(fieldDescriptors);
				}
				return fieldDescriptors;
			}
		}

		void ReloadFieldDescriptors()
		{
			if (fieldDescriptors != null)
			{
				fieldDescriptors.LoadFromBlob(SU_ActionDataUpdateBlob);
			}
		}

		[ChildEditable(true)]
		public OperationalActionMethodDescriptorCollection MethodDescriptors
		{
			get
			{
				if (methodDescriptors == null)
				{
					methodDescriptors = new OperationalActionMethodDescriptorCollection(this);
					methodDescriptors.LoadFromBlob(SU_ActionMenusAndMethodsBlob);
					RegisterEditableChildObject(methodDescriptors);
				}
				return methodDescriptors;
			}
		}

		void ReloadMethodDescriptors()
		{
			if (methodDescriptors != null)
			{
				methodDescriptors.LoadFromBlob(SU_ActionMenusAndMethodsBlob);
			}
		}

		#endregion

		#region Strategies

		public new OperationalActionLookups Lookups
		{
			get { return (OperationalActionLookups)base.Lookups; }
		}

		protected override StmMenuItemLookups GetNewLookups()
		{
			return new OperationalActionLookups(this);
		}

		public new OperationalActionValidation Validation
		{
			get { return (OperationalActionValidation)base.Validation; }
		}

		protected override StmMenuItemValidation GetNewValidation()
		{
			return new OperationalActionValidation(this);
		}

		#endregion

		#region Properties For Binding

		public override ZBlob SU_ActionDataUpdateBlob
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.SU_ActionDataUpdateBlob; }
			set
			{
				base.SU_ActionDataUpdateBlob = value;
				ReloadFieldDescriptors();
			}
		}

		public override ZBlob SU_ActionMenusAndMethodsBlob
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.SU_ActionMenusAndMethodsBlob; }
			set
			{
				base.SU_ActionMenusAndMethodsBlob = value;
				ReloadMethodDescriptors();
			}
		}

		[ResourceStringData("StmMenuItem|SU_Calc_IsPublished", ShortCaption = "Pub.", Caption = "Published", FullDescription = "Published actions are visible to other users of this system.\r\nUn-published actions are only visible to the user that created them.")]
		public ZBool SU_Calc_IsPublished
		{
			get { return SU_GS_NKStaffCode.IsEmpty; }
			set
			{
				SU_GS_NKStaffCode = value ? ZString.Empty : GlbStaff.CurrentUser.GS_Code;

				if (!IsValidationSuspended)
				{
					Validation.ValidateSU_Calc_IsPublished();
				}

				SU_Calc_IsPublishedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SU_Calc_IsPublishedInfo
		{
			get { return GetZPropertyInfo(Schema.SU_Calc_IsPublished); }
		}

		protected bool SU_IsSystemDefined_ReadOnly
		{
			get { return !OperationalActionMenuEditableHelper.CanEdit(this); }
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			OperationalActionDocumentPivotCollection documentPivots = DocumentPivots; // Can only be initialized before this OperationAction is deleted.
			base.Delete();
			documentPivots.RemoveAndDeleteAll();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if ((fieldDescriptors != null) && fieldDescriptors.HasChanges)
			{
				SU_ActionDataUpdateBlob = fieldDescriptors.SaveToBlob();
			}

			if ((methodDescriptors != null) && methodDescriptors.HasChanges)
			{
				SU_ActionMenusAndMethodsBlob = methodDescriptors.SaveToBlob();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SU_MenuType = Core.Constants.StmMenuItemTypes.OperationalActions;
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			ReloadFieldDescriptors();
			ReloadMethodDescriptors();
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !ReadOnly; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("OperationalAction|CannotDeleteSystemDefinedActions", "System-defined Operational Actions cannot be deleted."); }
		}

		#endregion

		#region IOperationalActionMenuEditable Members

		ZPropertyInfo IOperationalActionMenuEditable.SystemDefinedInfo
		{
			get { return SU_IsSystemDefinedInfo; }
		}

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => true;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region IAuditParent Members

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(StmMenuMenuPivotSchema.SF_SU_Inward, null);
			}
		}

		#endregion

		MenuEditingMode editingMode;
		OperationalActionContext context;
		OperationalActionDocumentPivotCollection documentPivots;
		OperationalActionDocumentPivotView documentPivotsView;
		OperationalActionFieldDescriptorCollection fieldDescriptors;
		OperationalActionMethodDescriptorCollection methodDescriptors;
	}
}
