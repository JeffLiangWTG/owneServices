using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Res = MailManager.Res;

namespace Enterprise.MailManager.Business
{
	[ModuleID(ModuleId.MailItemTemplate)]
	public class MailItemTemplateCollection : ActiveBusinessObjectCollection<MailItemTemplate>
		, IMailItemTemplateCollection
	{
		/// <summary>
		/// For use with the ObjectFactory
		/// </summary>
		public MailItemTemplateCollection()
			: base(new BusinessObjectFactory())
		{
		}

		public MailItemTemplateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString CategoryCodeToFilter
		{
			get { return categoryCodeToFilter; }
			set
			{
				if (CategoryCodeToFilter != value)
				{
					categoryCodeToFilter = value;
					AdditionalFilter = new ZQuery(MailDBItemTemplateSchema.MIT_Category, categoryCodeToFilter);
					FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Category", "Property", categoryCodeToFilter));
				}
			}
		}
		ZString categoryCodeToFilter;

		protected override object[] GetCollectionState()
		{
			return new object[] { categoryCodeToFilter };
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			var template = (MailItemTemplate)selectedBusinessObject;
			if (!CategoryCodeToFilter.IsEmpty && template.MIT_Category != CategoryCodeToFilter)
			{
				errors.Add(Res.GetString("2753D959-AE51-495E-A650-FB8A125CDC3E", "A Template selected from here must be in Category {0}.", CategoryCodeToFilter));
			}
		}

		public ZString GetTemplateBody(ZGuid pk)
		{
			var result = ZString.Empty;

			foreach (var template in this)
			{
				if (template.PK == pk)
				{
					result = template.MIT_Body;
				}
			}
			return result;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery additionalFilter = base.CreateRelationshipFilter();

			if (!Env.Security.ViewAllEmailTemplates.IsAllowed)
			{
				additionalFilter.AddToFilter(GetQuery(GlbCompany.CurrentCompany.PK, MailDBItemTemplateSchema.MIT_GC_Company));
				additionalFilter.AddToFilter(GetQuery(GlbBranch.CurrentBranch.PK, MailDBItemTemplateSchema.MIT_GB_Branch));
				additionalFilter.AddToFilter(GetQuery(GlbDepartment.CurrentDepartment.PK, MailDBItemTemplateSchema.MIT_GE_Department));
			}

			return additionalFilter;
		}

		public ZGuid GetPKOfFirstValidTemplateForCurrentCompany()
		{
			if (Count > 0)
			{
				var defaultTemplate = CategoryCodeToFilter.IsEmpty ?
					this.FirstOrDefault(t => t.MIT_GC_Company == GlbCompany.CurrentCompany.PK)
					: this.FirstOrDefault(t => t.MIT_GC_Company == GlbCompany.CurrentCompany.PK && t.MIT_Category == CategoryCodeToFilter);

				return defaultTemplate == null ? this.First().PK : defaultTemplate.PK;
			}
			return ZGuid.Empty;
		}

		ZQuery GetQuery(ZGuid value, SchemaGuidColumn column)
		{
			var result = new ZQuery();
			if (value.IsValid)
			{
				result.AddToFilter(column, value);
				result.AddToFilter(JoinCondition.Or, column, null);
			}
			return result;
		}

		public void FilterByCurrentLoginDetails()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Company", "Property", GlbCompany.CurrentCompany.PK));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Branch", "Property", GlbBranch.CurrentBranch.PK));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Department", "Property", GlbDepartment.CurrentDepartment.PK));
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new MailItemTemplateFindBoxListProvider(this); }
		}
	}
}
