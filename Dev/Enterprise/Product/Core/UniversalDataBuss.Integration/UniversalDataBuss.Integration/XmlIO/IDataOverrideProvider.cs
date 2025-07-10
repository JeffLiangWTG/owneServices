using System.Collections.Generic;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IDataOverrideProvider
	{
		/// <summary>
		/// returns IDataObject state; Default, Added, Removed
		/// </summary>
		DataObjectState GetDataObjectState(IDataObject dataObject);

		/// <summary>
		/// returns overriden and additional property values
		/// </summary>
		IEnumerable<IPropertyOverride> GetPropertyOverrides(IDataObject dataObject);
	}
}
