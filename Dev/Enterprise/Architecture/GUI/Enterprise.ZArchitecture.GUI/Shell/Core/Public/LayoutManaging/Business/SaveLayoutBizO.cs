using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Business.Internal
{
	public class SaveLayoutBizO : NonPersistentBusinessObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public SaveLayoutBizO(IModifyModuleAndGridLayout layoutSavable)
			: base(layoutSavable.Factory)
		{
			LayoutSavable = layoutSavable;
			var lastUsedLayout = layoutSavable.LayoutToDefault as StmModuleFilter;
			if (lastUsedLayout != null && !lastUsedLayout.IsDeleted)
			{
				LayoutName = lastUsedLayout.S9_FilterName;
				PublishLayout = lastUsedLayout.S9_IsPublished;
				PublishAcrossAllCompanies = lastUsedLayout.S9_GC.IsEmpty;
				IsPublishAcrossAllCompaniesOrginalValue = lastUsedLayout.S9_GC.IsEmpty;
				SaveColumnLayout = lastUsedLayout.S9_SaveColumnLayout;
				SaveGridColourLayout = lastUsedLayout.S9_SaveGridColourLayout;
				moduleId = lastUsedLayout.S9_ModuleID;
			}
			else
			{
				SaveColumnLayout = true;
				moduleId = layoutSavable.LayoutSetIdentifierToSaveANewLayoutWith;
			}

			HasChanges = false;
		}

		internal IModifyModuleAndGridLayout LayoutSavable { get; }

		internal bool IsPublishAcrossAllCompaniesOrginalValue;

		#region LayoutName

		[ResourceStringData("967BC2AE-E45D-431D-B17C-962A8C1ECCF3", Caption = "Layout Name", FullDescription = "Layout Name of the Find Filter")]
		[LinkedTranslatableDataField(typeof(StmModuleFilter), StmModuleFilter.Schema.S9_FilterName)]
		[MaxLength(StmModuleFilter.Schema.S9_FilterNameMaxLength)]
		public ZString LayoutName
		{
			get { return fLayoutName; }
			set
			{
				if (fLayoutName != value)
				{
					CheckMaximumLength(LayoutNameInfo, value);
					SetNonPersistentPropertyValue(LayoutNameInfo, ref fLayoutName, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateLayoutName();
						Validation.ValidateIsUserDefinedFilter();
					}
				}
			}
		}

		[ResourceStringData("3BCF2719-68BD-4621-9368-B724EB8F6F9F", Caption = "Multilingual Layout Name", FullDescription = "Multilingual Layout Name of the Find Filter")]
		[MaxLength(StmModuleFilter.Schema.S9_FilterNameMaxLength)]
		public MultilingualString LayoutNameMultilingual
		{
			get { return GetMultilingual(LayoutNameInfo); }
		}

		public ZPropertyInfo LayoutNameMultilingualInfo
		{
			get { return LayoutNameInfo; }
		}

		public bool LayoutNameMultilingual_ReadOnly
		{
			get { return LayoutNameInfo.ReadOnly; }
		}

		public ZPropertyInfo LayoutNameInfo
		{
			get
			{
#pragma warning disable EDI007
				return GetZPropertyInfo(nameof(LayoutName));
#pragma warning restore EDI007
			}
		}

		ZString fLayoutName;

		#endregion

		#region PublishLayout

		public ZBool PublishLayout
		{
			get { return fPublishLayout; }
			set
			{
				if (fPublishLayout != value)
				{
					SetNonPersistentPropertyValue(PublishLayoutInfo, ref fPublishLayout, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAll();
					}
					if (!value)
					{
						PublishAcrossAllCompanies = false;
					}
				}
			}
		}

		public ZPropertyInfo PublishLayoutInfo
		{
			get { return GetZPropertyInfo(nameof(PublishLayout)); }
		}

		ZBool fPublishLayout;

		#endregion

		#region PublishAcrossAllCompanies

		public ZBool PublishAcrossAllCompanies
		{
			get { return publishAcrossAllCompanies; }
			set
			{
				if (publishAcrossAllCompanies != value)
				{
					SetNonPersistentPropertyValue(PublishAcrossAllCompaniesInfo, ref publishAcrossAllCompanies, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidatePublishAcrossAllCompanies();
						Validation.ValidateLayoutName();
					}
				}
			}
		}
		ZBool publishAcrossAllCompanies;

		public ZPropertyInfo PublishAcrossAllCompaniesInfo
		{
			get { return GetZPropertyInfo(nameof(PublishAcrossAllCompanies)); }
		}

		protected bool PublishAcrossAllCompanies_ReadOnly
		{
			get { return !PublishLayout; }
		}

		#endregion

		#region SaveColumnLayout

		[ReadOnlyMember(nameof(SaveColumnLayout_ReadOnly))]
		public ZBool SaveColumnLayout
		{
			get { return saveColumnLayout; }
			set
			{
				if (saveColumnLayout != value)
				{
					SetNonPersistentPropertyValue(SaveColumnLayoutInfo, ref saveColumnLayout, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAll();
					}
				}
			}
		}

		public ZPropertyInfo SaveColumnLayoutInfo
		{
			get { return GetZPropertyInfo(nameof(SaveColumnLayout)); }
		}

		ZBool saveColumnLayout;

		bool SaveColumnLayout_ReadOnly => IsUserDefinedFilter;

		internal IGridLayoutStorage GetLayoutStorage(bool shouldIgnoreIsPublished)
		{
			return shouldIgnoreIsPublished
				? LayoutSavable.FindLayout(LayoutNameMultilingual.GetUnresolvedString())
				: LayoutSavable.FindLayout(LayoutNameMultilingual.GetUnresolvedString(), PublishLayout);
		}

		#endregion

		#region SaveGridColourLayout

		[ReadOnlyMember(nameof(SaveGridColourLayout_ReadOnly))]
		public ZBool SaveGridColourLayout
		{
			get { return saveGridColourLayout; }
			set
			{
				if (saveGridColourLayout != value)
				{
					SetNonPersistentPropertyValue(SaveGridColourLayoutInfo, ref saveGridColourLayout, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateAll();
					}
				}
			}
		}

		public ZPropertyInfo SaveGridColourLayoutInfo
		{
			get { return GetZPropertyInfo(nameof(SaveGridColourLayout)); }
		}

		bool SaveGridColourLayout_ReadOnly => IsUserDefinedFilter;

		ZBool saveGridColourLayout;

		#endregion

		#region LayoutIsSystemDefined

		public bool LayoutIsSystemDefined
		{
			get { return LayoutIsSystemDefinedCore; }
		}

		protected virtual bool LayoutIsSystemDefinedCore
		{
			get
			{
				var result = false;
				if (!LayoutNameMultilingual.IsEmpty)
				{
					var layoutStorage = GetLayoutStorage(shouldIgnoreIsPublished: false);
					result = layoutStorage != null && layoutStorage.IsSystemDefined;
				}
				return result;
			}
		}

		#endregion

		#region IsUserDefinedFilter

		public ZBool IsUserDefinedFilter
		{
			get { return isUserDefinedFilter; }
			set
			{
				if (isUserDefinedFilter != value)
				{
					SetNonPersistentPropertyValue(IsUserDefinedFilterInfo, ref isUserDefinedFilter, value);

					if (value)
					{
						SaveColumnLayout = false;
						SaveGridColourLayout = false;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}

		ZBool isUserDefinedFilter;

		public ZPropertyInfo IsUserDefinedFilterInfo => GetZPropertyInfo(nameof(IsUserDefinedFilter));

		#endregion

		#region Validation

		public SaveLayoutBizOValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual SaveLayoutBizOValidation GetNewValidation()
		{
			return new SaveLayoutBizOValidation(this);
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Fields required for LinkedTranslatable to StmModuleFilter

		public ZString S9_ModuleID
		{
			get { return moduleId; }
			set
			{
				moduleId = value;
				S9_ModuleIDInfo.RefreshBinding();
			}
		}
		ZString moduleId;

		public ZPropertyInfo S9_ModuleIDInfo
		{
			get { return GetZPropertyInfo(nameof(S9_ModuleID)); }
		}

		#endregion
	}

	#region class SaveLayoutBizOValidation

	public class SaveLayoutBizOValidation : ZValidation
	{
		public SaveLayoutBizOValidation(SaveLayoutBizO parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		#region LayoutName

		public void ValidateLayoutName()
		{
			ValidateCalculatedProperty(Parent.LayoutNameInfo);
		}

		protected virtual void CheckLayoutName()
		{
			MandatoryValidation.CheckEntered(Parent.LayoutNameInfo);
			var helper = new ExistingSavedLayoutMessageHelper(Parent);

			if (!Parent.LayoutName.IsEmpty)
			{
				var reasonTheNameNotAcceptable = Parent.LayoutSavable.GetReasonLayoutNameNotAllowed(Parent.LayoutName, Parent.PublishLayout);

				if (!string.IsNullOrEmpty(reasonTheNameNotAcceptable))
				{
					Parent.LayoutNameInfo.AddError(reasonTheNameNotAcceptable);
				}
				else if (Parent.IsPublishAcrossAllCompaniesOrginalValue && helper.LayoutExists)
				{
					if (!EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches)
					{
						var error = Res.GetString("4ACA50F6-D8A3-4E27-9393-4CA5EBCBC470", "You do not have the appropriate security rights to override this layout.");
						Parent.LayoutNameInfo.AddError(error);
					}
				}
				else if (Parent.LayoutName.StartsWith("[") || Parent.LayoutName.EndsWith("]"))
				{
					var error = Res.GetString("81cd814e-01c9-455a-8e1f-6ed88fd7e57c", "The layout name cannot start or end with square brackets.");
					Parent.LayoutNameInfo.AddError(error);
				}
				else if (Parent.LayoutIsSystemDefined)
				{
					var error = Res.GetString("1b52c160-d8cf-4ade-b383-96eaa6f46933", "{0} is a system-defined layout and cannot be overridden. Change the layout name to continue.", Parent.LayoutName);
					Parent.LayoutNameInfo.AddError(error);
				}
				else
				{
					if (helper.LayoutExists)
					{
						if (helper.AllowUserToSaveWithWarning)
						{
							Parent.LayoutNameInfo.AddWarning(helper.ExistingLayoutExistsMessage);
						}
						else
						{
							Parent.LayoutNameInfo.AddError(helper.ExistingLayoutExistsMessage);
						}
					}
				}
			}

			TranslatableDataFieldAttribute.Validate(Parent.LayoutNameInfo);
		}

		#endregion

		#region PublishLayout

		public void ValidatePublishLayout()
		{
			ValidateCalculatedProperty(Parent.PublishLayoutInfo);
		}

		protected virtual void CheckPublishLayout()
		{
			if (Parent.PublishLayout)
			{
				var publishGlobalFilterLayouts = EnvProxy.Instance.Security.PublishGlobalFilterLayouts;

				if (!publishGlobalFilterLayouts.IsAllowed)
				{
					Parent.PublishLayoutInfo.AddError(publishGlobalFilterLayouts.ErrorMessageForNotAllowed);
				}
				else if (Parent.IsUserDefinedFilter && !EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed)
				{
					Parent.PublishLayoutInfo.AddError(Res.GetString("49353982-d9b9-4c91-a4e7-f57c81a9142a", "You do not have permission to publish user-defined filters. You may only save unpublished user-defined filters, which will be accessible only to you."));
				}
				else
				{
					var filterBizo = Parent.LayoutSavable as FilterStripBusinessObject;

					if (filterBizo != null)
					{
						UserDefinedFilterHelper.CheckFilterBizoDoesNotContainNestedNonpublishedUserDefinedFilters(filterBizo,
							trail => Res.GetString("c35264d2-3930-45e0-95a3-0873953a27ca", @"The layout being saved is published, and yet it contains the following unpublished user-defined filter:

{0}

Unpublished filters cannot be included in published layouts.", trail),
							trails => Res.GetString("9a87e2af-2c6d-4b24-80aa-6ec2ec841104", @"The layout being saved is published, and yet it contains the following unpublished user-defined filters:

{0}

Unpublished filters cannot be included in published layouts.", trails), Parent.PublishLayoutInfo);
					}
				}
			}
			else
			{
				var existingLayout = Parent.GetLayoutStorage(shouldIgnoreIsPublished: true);

				if (existingLayout != null && existingLayout.IsPublished)
				{
					Parent.PublishLayoutInfo.AddError(CannotUnpublishAlreadyPublishedLayout);
				}
			}
		}

		public static string CannotUnpublishAlreadyPublishedLayout
		{
			get { return Res.GetString("aae365ed-72ac-4bed-9242-063bd772be8b", "You cannot make this layout unpublished as it has already been published."); }
		}

		#endregion

		#region SaveColumnLayout

		public void ValidateSaveColumnLayout()
		{
			ValidateCalculatedProperty(Parent.SaveColumnLayoutInfo);
		}

		protected virtual void CheckSaveColumnLayout()
		{
			if (Parent.SaveColumnLayout)
			{
				var publishGlobalFilterLayouts = EnvProxy.Instance.Security.PublishGlobalFilterLayouts;
				if (Parent.PublishLayout && !publishGlobalFilterLayouts.IsAllowed)
				{
					Parent.SaveColumnLayoutInfo.AddError(publishGlobalFilterLayouts.ErrorMessageForNotAllowed);
				}
			}
		}

		#endregion

		#region PublishAcrossAllCompanies

		public void ValidatePublishAcrossAllCompanies()
		{
			ValidateCalculatedProperty(Parent.PublishAcrossAllCompaniesInfo);
		}

		public void ValidateIsUserDefinedFilter()
		{
			ValidateCalculatedProperty(Parent.IsUserDefinedFilterInfo);
		}

		protected virtual void CheckPublishAcrossAllCompanies()
		{
			if (Parent.PublishAcrossAllCompanies)
			{
				var publishGlobalFilterLayouts = EnvProxy.Instance.Security.PublishGlobalFilterLayouts;
				if (!publishGlobalFilterLayouts.IsAllowedForAllBranches)
				{
					Parent.PublishAcrossAllCompaniesInfo.AddError(publishGlobalFilterLayouts.ErrorMessageForNotAllowed);
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "It's okay, just checking to see if it's empty.")]
		protected virtual void CheckIsUserDefinedFilter()
		{
			if (Parent.IsUserDefinedFilter)
			{
				CheckLayoutSavableSupportsUserDefinedFilters();

				if (!string.IsNullOrEmpty(Parent.LayoutName))
				{
					CheckUserDefinedFilterSelfReference();
				}
			}
		}

		void CheckLayoutSavableSupportsUserDefinedFilters()
		{
			if (Parent.LayoutSavable is FilterStripBusinessObject filterBizo && !filterBizo.SupportsUserDefinedFilters)
			{
				Parent.IsUserDefinedFilterInfo.AddError(Res.GetString("0773ef18-30b0-4246-8b0a-15681ec41ecd", "User-Defined Filters are not supported for this module."));
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Not localising, but using LayoutName as an identifier")]
		void CheckUserDefinedFilterSelfReference()
		{
			var thisFilterName = Parent.LayoutNameMultilingual;

			if (Parent.LayoutSavable is FilterStripBusinessObject filterBizo && UserDefinedFilterHelper.FindFilterNestedDownHierarchy(filterBizo, Parent.LayoutName, thisFilterName, true, out var trail))
			{
				var message = trail.Count == 2
					? Res.GetString("791f42ef-ed90-4c37-9112-00f455b4c3b0", "A user-defined filter strip cannot contain a filter strip that references itself. Please remove the [{0}] filter strip before saving.", thisFilterName)
					: Res.GetString("aaef4238-9574-48cd-afb3-19947bcb162d", @"A user-defined filter strip cannot contain a filter strip that references itself. The [{0}] filter strip contains a reference to the filter being saved:
{1}", UserDefinedFilterHelper.GetDirectlyUsedFilterWhichContainsNestedTargetFilter(trail), UserDefinedFilterHelper.GetFullTrailString(trail));

				Parent.IsUserDefinedFilterInfo.AddError(message);
			}
		}

		#endregion

		public override void ValidateAll()
		{
			ValidateLayoutName();
			ValidatePublishLayout();
			ValidateIsUserDefinedFilter();
			ValidateSaveColumnLayout();
		}

		public override Type AutoValidationType
		{
			get { return typeof(SaveLayoutBizO); }
		}

		protected readonly SaveLayoutBizO Parent;
	}

	#endregion
}
