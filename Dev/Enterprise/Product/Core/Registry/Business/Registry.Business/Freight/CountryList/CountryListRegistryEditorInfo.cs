using System;
using Enterprise.Integration;

namespace Enterprise.Registry.Business
{
	public class CountryListRegistryEditorInfo : IRegistryEditorInfo
	{
		#region IRegistryEditorInfo Members

		Type IRegistryEditorInfo.BaseDataTypeToBeEdited
		{
			get { return typeof(Guid[]); }
		}

		#endregion
	}
}
