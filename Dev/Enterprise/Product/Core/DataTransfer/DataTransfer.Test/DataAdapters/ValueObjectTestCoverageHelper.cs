using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ValueObjectTestCoverageHelper
	{
		public ValueObjectTestCoverageHelper(Type valueObjectType)
			: this(valueObjectType, Array.Empty<string>())
		{
		}

		public ValueObjectTestCoverageHelper(Type valueObjectType, string[] xmlNodesToExclude)
		{
			PopulatePropertyPaths(new ValueObjectPropertyNavigator(valueObjectType), xmlNodesToExclude);
		}

		public void NotifyCovered(IValueObject value)
		{
			NotifyCovered(value, new ValueObjectPropertyNavigator(value.GetType()));
		}

		internal string[] UncoveredPaths
		{
			get { return uncoveredPaths.ToArray(); }
		}

		public string UncoveredPathsAsString
		{
			get
			{
				string result = "";
				foreach (string uncoveredPath in uncoveredPaths)
				{
					if (!string.IsNullOrEmpty(result))
					{
						result += "\r\n";
					}

					result += uncoveredPath;
				}
				return result;
			}
		}

		#region Implementation

		readonly List<string> uncoveredPaths = new List<string>();

		void PopulatePropertyPaths(ValueObjectPropertyNavigator currentPath, string[] xmlNodesToExclude)
		{
			if (!currentPath.IsRecurringInPath())
			{
				foreach (ValueObjectPropertyNavigator nextProperty in currentPath.GetNextPropertiesInPath())
				{
					if (typeof(IValueObject).IsAssignableFrom(nextProperty.PropertyType))
					{
						PopulatePropertyPaths(nextProperty, xmlNodesToExclude);
					}
					else if (nextProperty.CanValueBeEmpty())
					{
						bool isExcluded = false;
						foreach (string xmlNodeToExclude in xmlNodesToExclude)
						{
							if (nextProperty.PathAsString == xmlNodeToExclude || nextProperty.PathAsString.StartsWith(xmlNodeToExclude + "/"))
							{
								isExcluded = true;
							}
						}
						if (!isExcluded)
						{
							uncoveredPaths.Add(nextProperty.PathAsString);
						}
					}
				}
			}
		}

		void NotifyCovered(IValueObject valueObject, ValueObjectPropertyNavigator currentPath)
		{
			if (valueObject.IsSpecified)
			{
				foreach (ValueObjectPropertyNavigator nextProperty in currentPath.GetNextPropertiesInPath())
				{
					object next = nextProperty.GetValue(valueObject);
					if (next is IList && !(next is Array))
					{
						foreach (IValueObject itemValue in (IList)next)
						{
							NotifyCovered(itemValue, nextProperty);
						}
					}
					else if (next is IValueObject)
					{
						NotifyCovered((IValueObject)next, nextProperty);
					}
					else if (nextProperty.HasValue(valueObject))
					{
						uncoveredPaths.Remove(nextProperty.PathAsString);
					}
				}
			}
		}

		#endregion
	}
}
