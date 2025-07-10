using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MailManager.Business
{
	class MailItemTemplateFindBoxListProvider : FindBoxListProvider
	{
		public MailItemTemplateFindBoxListProvider(IActiveBusinessObjectCollection list)
			: base(list)
		{
		}

		protected override void AddCodeStartsWithFilter(ZQuery query, string code)
		{
			if (!string.IsNullOrWhiteSpace(code))
			{
				query.AddToFilter(GetTemplateQuery(code));
			}
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
		{
			return GetTemplateCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return GetTemplateCode(code);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
		{
			return GetTemplateCode(code);
		}

		IEnumerable<BusinessObject> GetTemplateCode(string code)
		{
			return List.Factory.Load<MailItemTemplate>(GetTemplateQuery(code));
		}

		ZQuery GetTemplateQuery(ZString code)
		{
			ZQuery query = new ZQuery();

			ZString category = ZString.Empty;
			ZString name = ZString.Empty;

			if (code.Length >= MailDBItemTemplateSchema.MIT_Category.MaxLength)
			{
				ZString[] values = code.Split('_');
				if (values.Length > 0)
				{
					category = values[0];
				}

				if (values.Length > 1)
				{
					name = values[1];
				}
			}

			if (!category.IsEmpty)
			{
				query.AddToFilter(MailDBItemTemplateSchema.MIT_Category, category);
			}

			if (!name.IsEmpty)
			{
				query.AddToFilter(MailDBItemTemplateSchema.MIT_Name, name);
			}

			if (category.IsEmpty && name.IsEmpty)
			{
				query.IsNoResultQuery = true;
			}

			return query;
		}
	}
}
