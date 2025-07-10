#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class BrowsableChecker
	{
		public static void CheckBrowsableProperties(Type componentType, params string[] propertyNames)
		{
			CheckBrowsableProperties(componentType, BrowsableAttribute.No, propertyNames);
		}

		public static void CheckBrowsableProperties(Type componentType, Attribute additionalBrowsableNoAttribute, params string[] propertyNames)
		{
			string[] property_names = propertyNames;

			List<string> expected_browsable = new List<string>();
			List<string> expected_not_browsable = new List<string>();

			foreach (string name in property_names)
			{
				PropertyDescriptor property = TypeDescriptor.GetProperties(componentType)[name];
				Assertion.AssertNotNull("Could not find property " + name, property);
				if (property.Attributes.Contains(BrowsableAttribute.No) ||
					property.Attributes.Contains(additionalBrowsableNoAttribute))
				{
					expected_browsable.Add(name);
				}
			}
			foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(componentType))
			{
				if (!((IList)propertyNames).Contains(prop.Name) &&
					!prop.Attributes.Contains(BrowsableAttribute.No) &&
					!prop.Attributes.Contains(additionalBrowsableNoAttribute))
				{
					expected_not_browsable.Add(prop.Name);
				}
			}
			if (expected_browsable.Count > 0 ||
				expected_not_browsable.Count > 0)
			{
				string message = "\n";
				foreach (string name in expected_browsable)
				{
					message += "Expected property " + name + " to be browsable\n";
				}
				foreach (string name in expected_not_browsable)
				{
					message += "Expected property " + name + " to NOT be browsable\n";
				}
				throw new InvalidOperationException(message);
			}
		}
	}
}
#endif
