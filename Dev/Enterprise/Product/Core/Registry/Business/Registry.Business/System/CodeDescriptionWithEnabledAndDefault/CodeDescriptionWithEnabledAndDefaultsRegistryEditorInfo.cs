using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.CodeDescriptionWithEnabledAndDefaultsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CodeDescriptionWithEnabledAndDefaultsRegistryEditorInfo : IRegistryEditorInfo
	{
		public MultilingualString CodeColumnCaption;
		public MultilingualString DescriptionColumnCaption;

		public Type BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionWithEnabledAndDefaultCollection); }
		}
	}
}
