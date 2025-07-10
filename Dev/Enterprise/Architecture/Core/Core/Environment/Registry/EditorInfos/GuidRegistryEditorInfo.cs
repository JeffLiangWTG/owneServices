using System;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class GuidRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(GuidRegistryDataType); }
		}
	}
}
