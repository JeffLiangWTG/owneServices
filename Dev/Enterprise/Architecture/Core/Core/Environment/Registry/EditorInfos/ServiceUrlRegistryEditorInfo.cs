using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class ServiceUrlRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get
			{
				return typeof(ServiceUrlRegistryDataType);
			}
		}
	}
}
