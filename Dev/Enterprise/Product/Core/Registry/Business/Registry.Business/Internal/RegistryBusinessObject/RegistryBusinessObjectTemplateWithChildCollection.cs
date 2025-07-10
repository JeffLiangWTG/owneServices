using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class RegistryBusinessObjectTemplateWithChildCollection : RegistryBusinessObjectTemplate
	{
		protected RegistryBusinessObjectTemplateWithChildCollection()
		{
		}

		protected RegistryBusinessObjectTemplateWithChildCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected RegistryBusinessObjectTemplateWithChildCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected RegistryBusinessObjectTemplateWithChildCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected abstract IEnumerable<RegistryBusinessObjectCollectionTemplate> ChildCollections { get; }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			foreach (var collection in ChildCollections)
			{
				ValidateCollection(collection);
			}
		}

		void ValidateCollection(RegistryBusinessObjectCollectionTemplate collection)
		{
			collection.RunPreSaveValidation();
			collection.RunPreSaveValidationOnElements();

			if (collection.HasValidationErrors())
			{
				ThrowValidationExceptionToPreventSave();
			}
		}
	}
}
