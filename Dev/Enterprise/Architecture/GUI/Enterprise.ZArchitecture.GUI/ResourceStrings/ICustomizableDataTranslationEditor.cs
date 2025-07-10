using CargoWise.ResourceStrings.Cache;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.ZArchitecture.GUI
{
	public interface ICustomizableDataTranslationEditor
	{
		void EditTranslations(ZTranslatableTextControl control);
		void EditTranslations(CustomizableDataResourceStrings customizable, ResourceString initialValue, object context);
	}
}
