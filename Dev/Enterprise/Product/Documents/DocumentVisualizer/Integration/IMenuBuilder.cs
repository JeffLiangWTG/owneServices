using System.Collections.Generic;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface IMenuBuilder
	{
		IReadOnlyCollection<IMenuItemDescriptor> Build(object documentDataSource, IMenuItemBuilder menuItemBuilder, IResourceAccessor res, IMessageInstructions messageInstructions);
	}
}
