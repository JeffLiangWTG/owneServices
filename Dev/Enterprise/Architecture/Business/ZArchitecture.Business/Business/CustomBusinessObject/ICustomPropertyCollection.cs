using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business
{
	public interface ICustomPropertyCollection : IEnumerable<ICustomProperty>
	{
		ICustomProperty GetCustomProperty(string identifier);

		string[] GetIdentifiers();

		IEnumerable<ICustomProperty> GetProperties();

		void AddCustomProperties(IEnumerable<ICustomProperty> properties);
	}
}
