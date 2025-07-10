using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public interface ITagMenuTreeViewModel
	{
		ResourceString Name { get; }
		TagActionType ActionType { get; }

		IEnumerable<MenuItemDescriptor<TagDefinition>> GetValidDefinitionMenuItems();
		IEnumerable<MenuItemDescriptor<TagMagnitude>> GetValidMagnitudeMenuItems(TagDefinition definition);
		void ReloadAllMagnitudes();
	}
}
