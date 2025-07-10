using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using NUnit.Framework;

namespace Enterprise.Billing.Integration.Test
{
	class BillingTransactionTest : TestCase
	{
		public void TestToString()
		{
			var transaction = CreateWithAllPropertiesSet();
			var str = transaction.ToString();
			CombineAssertions(() =>
			{
				foreach (var property in BillingTransactionProperties)
				{
					Assert(property.Name + " property is shown in ToString().", str.Contains(property.Name + ": " + property.GetValue(transaction)));
				}
			});
		}

		public void TestEqualityAndHashCode()
		{
			var t1 = CreateWithAllPropertiesSet();
			var t2 = CreateWithAllPropertiesSet();
			var comparer = BillingTransaction.BillingTransactionComparer;
			Assert("Transactions with equal properties are equal (comparer)", comparer.Equals(t1, t2));
			Assert("Transactions with equal properties are equal (override)", t1.Equals(t2));
			AssertEquals("Transactions with equal properties must have equal hash codes (comparer)", comparer.GetHashCode(t1), comparer.GetHashCode(t2));
			AssertEquals("Transactions with equal properties must have equal hash codes (override)", t1.GetHashCode(), t2.GetHashCode());
			CombineAssertions(() =>
			{
				foreach (var property in BillingTransactionProperties)
				{
					var initialValue = property.GetValue(t2);
					property.SetValue(t2, ChangeValue(initialValue));
					Assert(property.Name + " values are different, transactions must be not equal (comparer).", !comparer.Equals(t1, t2));
					Assert(property.Name + " values are different, transactions must be not equal (override).", !t1.Equals(t2));
					AssertNotEquals("Transactions with different " + property.Name + " values should have different hash codes (comparer)", comparer.GetHashCode(t1), comparer.GetHashCode(t2));
					AssertNotEquals("Transactions with different " + property.Name + " values should have different hash codes (override)", t1.GetHashCode(), t2.GetHashCode());
					property.SetValue(t2, initialValue);
				}
			});
		}

		static BillingTransaction CreateWithAllPropertiesSet()
		{
			var result = new BillingTransaction();
			foreach (var property in BillingTransactionProperties)
			{
				var propertyName = property.Name;
				var propertyTypeName = property.PropertyType.Name;
				switch (propertyTypeName)
				{
					case "String":
						property.SetValue(result, propertyName);
						break;
					case "Int32":
						property.SetValue(result, propertyName.Length);
						break;
					case "DateTime":
						property.SetValue(result, new DateTime(2020, 1, 1));
						break;
					default:
						throw new Exception(string.Format("Unexpected property type [{0}]. Add correspondent case and populate data.", propertyTypeName));
				}
			}
			return result;
		}

		static IEnumerable<PropertyInfo> BillingTransactionProperties
		{
			get { return typeof(BillingTransaction).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => !Attribute.IsDefined(prop, typeof(XmlIgnoreAttribute))); }
		}

		static object ChangeValue(object obj)
		{
			var propertyTypeName = obj.GetType().Name;
			switch (propertyTypeName)
			{
				case "String":
					return (string)obj + "_";
				case "Int32":
					return (int)obj + 1;
				case "DateTime":
					return ((DateTime)obj).AddDays(1);
				default:
					throw new Exception(string.Format("Unexpected property type [{0}]. Add correspondent case and return changed value.", propertyTypeName));
			}
		}
	}
}
