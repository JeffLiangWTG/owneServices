using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomCssEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(WebCustomCssRegistryDataType); }
		}
	}
}
