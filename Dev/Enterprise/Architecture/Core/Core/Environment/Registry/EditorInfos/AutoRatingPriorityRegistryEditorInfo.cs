using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class AutoRatingPriorityRegistryEditorInfo : RegistryEditorInfo
	{
		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}
	}
}
