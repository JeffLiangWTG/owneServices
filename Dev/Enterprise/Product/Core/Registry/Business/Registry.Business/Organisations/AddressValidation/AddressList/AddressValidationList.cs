using System.Collections;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public abstract class AddressValidationList<T> : CodeDescriptionPairList
	{
		protected void BuildList()
		{
			foreach (T item in GetList())
			{
				var itemToString = GetItemToString(item);
				AddPair(itemToString, itemToString);
			}

			AddPair(Constants.AVSRegistryConstants.AddressTypes.All, Constants.AVSRegistryConstants.ControllerNames.All);
		}

		protected abstract IEnumerable GetList();

		protected abstract string GetItemToString(T item);
	}
}
