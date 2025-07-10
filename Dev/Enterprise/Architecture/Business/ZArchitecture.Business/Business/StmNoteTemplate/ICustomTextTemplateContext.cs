using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomTextTemplateContext
	{
		string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo);
		BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo);
	}

	public interface ICustomTextTemplateAlternateContexts
	{
		IReadOnlyCollection<string> GetAlternateTextTemplateContextIDs(object dataSource, KBindingMemberInfo bindingMemberInfo);
	}

	public interface ICustomTextTemplateFallbackContext
	{
		string GetFallbackTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo);
	}
}
