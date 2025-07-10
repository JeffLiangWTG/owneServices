using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class AccTaxRateListRegistryEditorInfo : RegistryEditorInfo
	{
		public AccTaxRateListRegistryEditorInfo(RegistryFindBoxFilter filter)
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
