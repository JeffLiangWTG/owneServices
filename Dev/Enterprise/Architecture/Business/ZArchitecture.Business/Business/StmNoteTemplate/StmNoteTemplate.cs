using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteTemplate : AutoStmNoteTemplate
	{
		public StmNoteTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public ZBool IsPublished
		{
			get { return S8_GS_NKStaff.IsEmpty; }
			set
			{
				if (!value)
				{
					IsAllCompanies = false;
				}
				SetPropertyValue(S8_GS_NKStaffInfo, value ? ZString.Empty : (ZString)EnvProxy.Instance.CurrentUser.Initials);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsPublished();
				}
				IsPublishedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPublishedInfo
		{
			get { return GetZPropertyInfo(nameof(IsPublished)); }
		}

		public ZBool IsAllCompanies
		{
			get { return S8_GC.IsEmpty; }
			set
			{
				SetPropertyValue(S8_GCInfo, value ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsAllCompanies();
				}
				IsAllCompaniesInfo.RefreshBinding();
			}
		}

		protected bool IsAllCompanies_ReadOnly
		{
			get { return !IsPublished; }
		}

		public ZPropertyInfo IsAllCompaniesInfo
		{
			get { return GetZPropertyInfo(nameof(IsAllCompanies)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			S8_GS_NKStaff = EnvProxy.Instance.CurrentUser.Initials;
			S8_GC = EnvProxy.Instance.CurrentCompany.PK;
		}

		public virtual ZString TemplateText
		{
			get { return S8_TemplateText; }
			set { S8_TemplateText = value; }
		}

		public StmNoteTemplateSecurityProvider SecurityProvider
		{
			get
			{
				if (securityProvider == null)
				{
					securityProvider = new StmNoteTemplateSecurityProvider();
				}
				return securityProvider;
			}
		}
		StmNoteTemplateSecurityProvider securityProvider;
	}
}
