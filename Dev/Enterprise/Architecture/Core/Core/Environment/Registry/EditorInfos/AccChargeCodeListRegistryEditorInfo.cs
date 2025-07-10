using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class AccChargeCodeListRegistryEditorInfo : RegistryEditorInfo
	{
		public AccChargeCodeListRegistryEditorInfo(RegistryFindBoxFilter filter)
		{
			Filter = filter;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(StringRegistryDataType); }
		}

		public readonly RegistryFindBoxFilter Filter;
	}
}
