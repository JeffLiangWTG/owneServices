using System;
using Enterprise.Integration;

namespace Enterprise.Registry.Business
{
	public class GlowUseIndexingForModuleListEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(string[]);
	}
}
