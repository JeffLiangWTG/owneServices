
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	[CodeProperty(MailItemTemplate.Schema.TemplateID), DescriptionProperty(AutoMailDBItemTemplate.Schema.MIT_Name)]
	public class MailItemTemplate : AutoMailDBItemTemplate
	{
		public MailItemTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			MIT_Language = Res.CurrentLanguage;
		}

		public new class Schema : AutoMailDBItemTemplate.Schema
		{
			public const string TemplateID = "TemplateID";
			public const string CategoryDescription = "CategoryDescription";
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("E71BF09B-9CB0-401C-8918-B3F167F85DB4", "Email Template"); }
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var result = HumanReadableNameCore;
				if (!TemplateID.IsEmpty)
				{
					result = string.Format("{0} - {1}", result, TemplateID);
				}
				return result;
			}
		}

		[MaxLength(MailItemTemplate.Schema.MIT_CategoryMaxLength + MailItemTemplate.Schema.MIT_NameMaxLength)]
		public virtual ZString TemplateID
		{
			get
			{
				if (!MIT_Category.IsEmpty && !MIT_Name.IsEmpty)
				{
					return string.Format("{0}_{1}", MIT_Category, MIT_Name);
				}
				else if (!MIT_Name.IsEmpty || !MIT_Category.IsEmpty)
				{
					return string.Format("{0}{1}", MIT_Category, MIT_Name);
				}
				return string.Empty;
			}
		}

		public ZPropertyInfo TemplateIDInfo
		{
			get { return GetZPropertyInfo(Schema.TemplateID); }
		}

		[List("Lookups.MailTemplateCategories")]
		public override ZString MIT_Category
		{
			get { return base.MIT_Category; }
			set { base.MIT_Category = value; }
		}

		public ZString CategoryDescription
		{
			get { return Lookups.MailTemplateCategories.GetDescriptionFromCode(MIT_Category); }
		}

		public ZPropertyInfo CategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CategoryDescription); }
		}

		[List("Lookups.Languages")]
		public override ZString MIT_Language
		{
			get { return base.MIT_Language; }
			set { base.MIT_Language = value; }
		}

		public override ZGuid MIT_GC_Company
		{
			get { return base.MIT_GC_Company; }
			set
			{
				if (base.MIT_GC_Company != value)
				{
					base.MIT_GC_Company = value;
					Validation.ValidateMIT_GB_Branch();
				}
			}
		}

		protected sealed override MailDBItemTemplateLookups GetNewLookups()
		{
			return MailDBItemTemplateLookups.New();
		}
	}
}
