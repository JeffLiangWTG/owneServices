using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class SaveLayoutBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SaveLayoutBusinessObject(FilterStripBusinessObject filterStripBizO, WebUser user)
		{
			if (filterStripBizO != null)
			{
				FilterStripBizO = filterStripBizO;
				User = user;

				using (SuspendSettingHasChanges())
				{
					StmModuleFilter layout = filterStripBizO.LastUsedLayout;
					if (layout != null)
					{
						LayoutName = layout.S9_FilterNameMultilingual.GetUnresolvedString();
						IsPublished = layout.S9_IsPublished && CanPublishLayouts;
						IsPublishedForCompany = layout.IsPublishedForCompanyInWeb && (CanPublishCompanyLayouts);
						IsSavingColumns = layout.S9_SaveColumnLayout;
					}
				}

				FilterStripBizO.RunPreSaveValidation();
				if (FilterStripBizO.HasErrors)
				{
					this.AddRowError(Res.GetString("6f29b58d-6b0a-423f-beea-e58750da9e78", "Please fix filter error before saving layout."));
				}
			}
		}

		public readonly FilterStripBusinessObject FilterStripBizO;
		protected readonly WebUser User;

		public bool CanPublishLayouts
		{
			get
			{
				OrgContactWebUser user = User as OrgContactWebUser;
				return user != null && user.CanPublishLayouts;
			}
		}

		public bool CanPublishCompanyLayouts
		{
			get
			{
				OrgContactWebUser user = User as OrgContactWebUser;
				return user != null && user.CanPublishCompanyLayouts;
			}
		}

		#region LayoutName

		[MaxLength(StmModuleFilter.Schema.S9_FilterNameMaxLength)]
		public ZString LayoutName
		{
			get { return fLayoutName; }
			set
			{
				SetNonPersistentPropertyValue(LayoutNameInfo, ref fLayoutName, value);
				LayoutNameInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateLayoutName();
				}
			}
		}

		public ZPropertyInfo LayoutNameInfo
		{
			get { return GetZPropertyInfo(nameof(LayoutName)); }
		}

		ZString fLayoutName;

		void ValidateLayoutName()
		{
			LayoutNameInfo.ClearAllNotifications();

			if (FilterStripBizO != null)
			{
				StmModuleFilter layout = this.FilterStripBizO.FindLayout(LayoutName, IsPublished);
				if (LayoutName.StartsWith("[") || LayoutName.EndsWith("]"))
				{
					LayoutNameInfo.AddError(Res.GetString("a66502b9-1829-4dc0-a7bd-f90f632c5146", "The layout name cannot start or end with square brackets."));
				}
				if (layout != null && layout.IsPublishedForCompanyInWeb && !CanPublishCompanyLayouts)
				{
					LayoutNameInfo.AddError(Res.GetString("B1E3CED4-81FD-4426-A5B8-3D4F6B5E3A44", "Company Layout cannot be overridden.\r\nPlease enter a different name to save Layout."));
				}
				if (layout != null && layout.S9_IsSystem)
				{
					LayoutNameInfo.AddError(Res.GetString("a83bad19-66c8-4e16-b708-b7d3e342ce00", "System Layout cannot be overridden.\r\nPlease enter a different name to save Layout."));
				}
			}
		}

		#endregion

		#region IsPublished

		public ZBool IsPublished
		{
			get { return fIsPublished; }
			set
			{
				SetNonPersistentPropertyValue(IsPublishedInfo, ref fIsPublished, value);
				IsPublishedInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateIsPublished();
				}
			}
		}

		public ZPropertyInfo IsPublishedInfo
		{
			get { return GetZPropertyInfo(nameof(IsPublished)); }
		}

		ZBool fIsPublished;

		void ValidateIsPublished()
		{
			IsPublishedInfo.ClearAllNotifications();

			if (IsPublished && !CanPublishLayouts)
			{
				IsPublishedInfo.AddError(Res.GetString("498b4020-5106-42ec-88a5-d7c7b1457e44", "You do not have permissions to save or override Published Layout."));
			}
		}

		#endregion

		#region IsPublishedForCompany

		public ZBool IsPublishedForCompany
		{
			get { return fIsPublishedForCompany; }
			set
			{
				SetNonPersistentPropertyValue(IsPublishedForCompanyInfo, ref fIsPublishedForCompany, value);
				IsPublishedForCompanyInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateIsPublishedForCompany();
				}
				if (value)
				{
					IsPublished = value; // Company layout must be published
				}
			}
		}

		public ZPropertyInfo IsPublishedForCompanyInfo
		{
			get { return GetZPropertyInfo(nameof(IsPublishedForCompany)); }
		}

		ZBool fIsPublishedForCompany;

		void ValidateIsPublishedForCompany()
		{
			IsPublishedForCompanyInfo.ClearAllNotifications();
			if (IsPublishedForCompany && !CanPublishCompanyLayouts)
			{
				IsPublishedForCompanyInfo.AddError(Res.GetString("6b0097a3-c248-4341-a76d-27f822a0dc87", "You do not have permissions to save or override Company Layout."));
			}
		}

		#endregion

		#region IsSavingColumns

		public ZBool IsSavingColumns
		{
			get { return fIsSavingColumns; }
			set { SetNonPersistentPropertyValue(IsSavingColumnsInfo, ref fIsSavingColumns, value); }
		}

		public ZPropertyInfo IsSavingColumnsInfo
		{
			get { return GetZPropertyInfo(nameof(IsSavingColumns)); }
		}

		ZBool fIsSavingColumns;

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLayoutName();
			ValidateIsPublished();

			if (FilterStripBizO != null)
			{
				FilterStripBizO.RunPreSaveValidation();
				if (FilterStripBizO.HasErrors)
				{
					AddRowError(FilterStripBizErrorMessage);
				}
			}
		}

		public static string FilterStripBizErrorMessage
		{
			get { return Res.GetString("b9e6153d-b743-424d-91e7-e165d068c47d", "There are errors on the filters. Please fix the error before saving."); }
		}
	}
}
