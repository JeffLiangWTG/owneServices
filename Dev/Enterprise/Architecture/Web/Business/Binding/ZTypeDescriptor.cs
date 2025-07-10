using System;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.Web.Business
{
	/// <summary>
	/// A TypeDescriptor that allows PropertyName to be a path.
	/// </summary>
	public static class ZTypeDescriptor
	{
		public static PropertyDescriptor GetProperties(object container, string fullPropertyName, bool matchCase)
		{
			char[] propertySeparator = new char[] { '.' };
			char[] indexPropertyStartChars = new char[] { '[', '(' };
			char[] indexPropertyEndChars = new char[] { ']', ')' };

			PropertyDescriptor propDescription = TypeDescriptor.GetProperties(container).Find(fullPropertyName, matchCase);

			string[] propertyTokens = fullPropertyName.Trim().Split(propertySeparator);
			object local = container;

			for (int i = 0; i < propertyTokens.Length && local != null; i++)
			{
				string propertyName = propertyTokens[i];
				propDescription = TypeDescriptor.GetProperties(local).Find(propertyName, true);
				if (propDescription == null)
				{
					throw new ArgumentException("Property not found",propertyName);
				}
				local = propDescription.GetValue(local);
			}
			return (propDescription);
		}
	}
}
