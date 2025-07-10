using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebThemeEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(WebThemeRegistryDataType); }
		}
	}
}
