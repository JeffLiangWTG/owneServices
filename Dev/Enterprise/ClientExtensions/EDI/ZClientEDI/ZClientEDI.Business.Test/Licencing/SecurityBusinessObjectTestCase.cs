using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public abstract class SecurityBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected void AssertPropertyInfosReadOnly(BusinessObject bO, bool shouldBeReadOnly, string[] propertyNamesToExcept)
		{
			PropertyInfo[] infos = bO.GetType().GetProperties();
			foreach (PropertyInfo property in infos)
			{
				if (property.PropertyType == typeof(ZPropertyInfo))
				{
					ZPropertyInfo info = (ZPropertyInfo)property.GetValue(bO, null);
					if (info.HasSetter)
					{
						bool tempReadOnly = shouldBeReadOnly;
						if (Array.IndexOf(propertyNamesToExcept, info.Name) > -1)
						{
							tempReadOnly = !shouldBeReadOnly;
						}
						string message = "The property " + property.Name + " should " + (!shouldBeReadOnly ? "NOT " : "") + " be read only";
						AssertEquals(message, tempReadOnly, info.ReadOnly);
					}
				}
			}
		}
	}
}
