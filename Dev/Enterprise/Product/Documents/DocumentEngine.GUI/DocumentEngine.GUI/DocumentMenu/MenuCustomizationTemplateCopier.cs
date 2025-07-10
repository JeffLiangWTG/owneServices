using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.GUI
{
	class MenuCustomizationTemplateCopier
	{
		internal MenuCustomizationTemplateCopier(MenuCustomisation menuCustomisation)
		{
			this.menuCustomisation = menuCustomisation;
		}

		readonly MenuCustomisation menuCustomisation;

		internal StmTemplateBase CopyTemplate(StmTemplateBase template)
		{
			StmTemplateBase result = null;

			if (template != null)
			{
				if (!template.SO_TemplateRestriction.IsEmpty)
				{
					Globals.Message.ShowInformation(Res.GetString("9ce9025a-bf4d-49ed-b833-ed45bb80345b", "The selected template is restricted and cannot be copied for the following reasons:\r\n{0}", template.SO_TemplateRestriction), Res.GetString("6d9e76a1-6896-4d33-950e-e6141f891ab3", "Error"));
				}
				else
				{
					result = menuCustomisation.CopyTemplate(template);
				}
			}

			return result;
		}
	}
}
