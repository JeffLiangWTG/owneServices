using System;
using System.Collections.Generic;

namespace Enterprise.Customs.KR.Business
{
	public class MultiPurposeResponseSupporterProvider
	{
		public IGOVCBRR20Supporter GetSupporterForR20(string messageType)
		{
			return (IGOVCBRR20Supporter)GetASupporter(messageType, typeof(IGOVCBRR20Supporter));
		}

		public IGOVCBRR99Supporter GetSupporterForR99(string messageType)
		{
			return (IGOVCBRR99Supporter)GetASupporter(messageType, typeof(IGOVCBRR99Supporter));
		}

		public IGOVCBRR38Supporter GetSupporterForR38(string messageType)
		{
			return (IGOVCBRR38Supporter)GetASupporter(messageType, typeof(IGOVCBRR38Supporter));
		}

		object GetASupporter(string messageType, Type typeofSupporter)
		{
			object result = null;
			var typeList = new List<Type>();
			foreach (Type type in this.GetType().Assembly.GetTypes())
			{
				if (typeofSupporter.IsAssignableFrom(type) && !type.IsAbstract)
				{
					object[] supportMessageTypeAttributes = type.GetCustomAttributes(typeof(MultiPurposeResponseSupportMessageTypeAttribute), false);
					if (supportMessageTypeAttributes.Length > 0)
					{
						var attribute = (MultiPurposeResponseSupportMessageTypeAttribute)supportMessageTypeAttributes[0];
						if (attribute.DoesSupport(messageType))
						{
							typeList.Add(type);
						}
					}
				}
			}
			if (typeList.Count == 1)
			{
				result = Activator.CreateInstance(typeList[0]);
			}
			else if (typeList.Count > 1)
			{
				throw new Exception($"More than one supporter is related to the message type {messageType}");
			}
			return result;
		}
	}
}
