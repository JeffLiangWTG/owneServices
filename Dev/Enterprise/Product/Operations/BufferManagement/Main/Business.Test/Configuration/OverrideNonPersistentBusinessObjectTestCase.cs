using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class OverrideNonPersistentBusinessObjectTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestInheritedOverrideFields()
		{
			BusinessObject bizO = GetNewBusinessObject();

			AssertNotEquals(bizO.GetType().BaseType.Name, "Object");

			TestInheritedOverrideFields(bizO);
		}

		void TestInheritedOverrideFields(BusinessObject bizO)
		{
			foreach (ZPropertyInfo info in bizO.ZPropertyInfoHash)
			{
				using (info.BizObj.GetValidationSuspender())
				{
					try
					{
						TestInheritedOverrideField(info);
					}
					finally
					{
						if (ColumnsToClearValueAfterTested.Contains(info.Name))
						{
							info.ClearValue();
						}
					}
				}
			}
		}

		void TestInheritedOverrideField(ZPropertyInfo info)
		{
			var bizoType = info.BizObj.GetType();
			var baseProperty = bizoType.BaseType.GetProperty(info.Name);
			var onThisProperty = bizoType.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Any(pi => pi.Name == info.Name);
			if (info.HasSetter &&
				onThisProperty)
			{
				AssertNotNull(string.Format("Expect Property {0} to be inherited from base class.", info.Name), baseProperty);
				Assert(string.Format("Expect Property {0} to be virtual in base class.", info.Name), baseProperty.GetGetMethod().IsVirtual);
				AssertNotNull(string.Format("Expect Property {0} to have read only attribute.", info.Name), info.ReadOnly);
			}
		}
	}
}
