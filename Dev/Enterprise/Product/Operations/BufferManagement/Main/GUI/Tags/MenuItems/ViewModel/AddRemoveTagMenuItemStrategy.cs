using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	class AddRemoveTagMenuItemStrategy
	{
		internal IEnumerable<MenuItem> GetAddAndRemoveTagMenuItems(ZFilterGridModule module, Type bizoType)
		{
			var getTagables = new GetTagables((f, isForRemovingTag) => GetSelectedBusinessObjects(f, module, isForRemovingTag));
			var factory = module.GridCollection.Factory;
			var definitions = new Lazy<TagDefinitionCache>(() => factory.GetCachedValue(nameof(TagDefinitionCache), () => TagProvider.GetAllTagDefinitions(factory)), isThreadSafe: true);
			var tagScopeType = GetTypeForTagScope(bizoType);

			var addTag = new TagMenuItemMenuTree(new AddTagMenuItemViewModel(getTagables, tagScopeType, definitions, module.GridCollection.Factory)) { Name = TagilatorMenuItemProvider.AddTagMenuItemName };
			var removeTag = new TagMenuItemMenuTree(new RemoveTagMenuItemViewModel(getTagables, definitions, module.GridCollection.Factory)) { Name = TagilatorMenuItemProvider.RemoveTagMenuItemName };

			return new[] { addTag, removeTag };
		}

		protected virtual IEnumerable<ITagable> GetSelectedBusinessObjects(BusinessObjectFactory factory, ZFilterGridModule module, bool isForRemovingTag)
		{
			return module.GetSelectedBusinessObjects().Cast<ITagable>();
		}

		protected virtual Type GetTypeForTagScope(Type proposedType)
		{
			return proposedType;
		}
	}
}
