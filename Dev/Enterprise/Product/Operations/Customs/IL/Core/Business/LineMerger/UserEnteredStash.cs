using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class UserEnteredStash
	{
		List<object> tempList;

		public void Stash(IUserEnteredStashSource userEnteredStashSource)
		{
			tempList = new List<object>();
			foreach (var property in userEnteredStashSource.StashedPropertiesAndConditions)
			{
				var temp = property.Key.Value;
				tempList.Add(temp);
			}
		}

		public void Apply(IUserEnteredStashSource userEnteredStashSource)
		{
			for (int i = 0; i < tempList.Count; i++)
			{
				var propertyAndCondition = userEnteredStashSource.StashedPropertiesAndConditions.ElementAtOrDefault(i);
				if (propertyAndCondition.Key != null && propertyAndCondition.Value)
				{
					var value = (IZType)tempList.ElementAt(i);
					if (!value.IsEmpty)
					{
						propertyAndCondition.Key.Value = value;
					}
				}
			}
		}
	}
}
