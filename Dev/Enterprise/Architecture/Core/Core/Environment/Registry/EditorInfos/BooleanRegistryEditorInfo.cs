using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class BooleanRegistryEditorInfo : RegistryEditorInfo
	{
		public BooleanRegistryEditorInfo()
		{
		}

		public BooleanRegistryEditorInfo(MultilingualString caption)
		{
			this.caption = caption;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BooleanRegistryDataType); }
		}

		public string Caption
		{
			get { return caption ?? ""; }
		}

		readonly MultilingualString caption;
	}
}
