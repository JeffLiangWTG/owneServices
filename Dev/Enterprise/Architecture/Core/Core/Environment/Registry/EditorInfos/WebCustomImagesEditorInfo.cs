using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomImagesEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(WebCustomImagesRegistryDataType); }
		}
	}
}
