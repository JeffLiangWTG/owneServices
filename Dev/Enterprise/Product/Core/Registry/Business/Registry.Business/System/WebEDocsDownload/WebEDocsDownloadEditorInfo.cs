using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WebEDocsDownloadEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(WebEDocsDownloadRegistryDataType); }
		}
	}
}
