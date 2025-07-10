using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.Core.Testing
{
	abstract class EnumConverterTestCase<T, U> : EnumConverterTestCase<T, U, ZString>
		where T : EnumConverter<U>, new()
		where U : struct
	{
		public void TestRejectsInvalidCodeGracefully()
		{
			var converter = new T();

			AssertEquals("[$$%%^^##$$#$] should have failed to get a value.", null, converter.ToEnumValue("$$%%^^##$$#$"));
			AssertEquals("[] should have failed to get a value.", null, converter.ToEnumValue(""));
			AssertEquals("[null] should have failed to get a value.", null, converter.ToEnumValue(null));
		}
	}

	[TestsSubclassesOf(typeof(EnumConverter<>))]
	abstract class EnumConverterTestCase<T, U, V> : TestCaseWithFactory
		where T : EnumConverter<U, V>, new()
		where U : struct
	{
		public void TestConvertsAllValuesBothWaysTheSameWay()
		{
			var contents = new List<KeyValuePair<V, U>>();
			var converter = new T();
			foreach (U value in Enum.GetValues(typeof(U)))
			{
				V code = converter.FromEnumValue(value);
				contents.Add(new KeyValuePair<V, U>(code, value));
			}

			foreach (var item in contents)
			{
				AssertEquals("[" + item.Key + "] got wrong value.", item.Value, converter.ToEnumValue(item.Key));
			}
		}

		public void TestAllPossibleEnterpriseValuesAreMapped()
		{
			var allPossibleEnterpriseValues = GetAllPossibleEnterpriseValues();
			var valuesWithoutMapping = new List<V>();
			var converter = new T();

			if (allPossibleEnterpriseValues != null)
			{
				var enterpriseValuesToMapAgainst = allPossibleEnterpriseValues.Except(GetEnterpriseValuesExcludedFromMapping());
				foreach (V enterpriseValue in enterpriseValuesToMapAgainst)
				{
					if (converter.ToEnumValue(enterpriseValue) == null)
					{
						valuesWithoutMapping.Add(enterpriseValue);
					}
				}

				AssertEquals(string.Format("Not all Enterprise codes are mapped. These codes need to be mapped (or exclude them in your GetEnterpriseValuesExcludedFromMapping method): {0}"
					, string.Join(",", valuesWithoutMapping)), 0, valuesWithoutMapping.Count);
			}
			else
			{
				Assert("No values to map against Enterprise codes.", true);
			}
		}

		public void TestAllUniversalXmlEnumValuesAreMappedAgainstEnterprise()
		{
			Type objType = typeof(U);
			if (objType.Namespace.Contains("Accounting")) //We want to run this test only for Accounting types!
			{
				var allPossibleEnterpriseValues = GetAllPossibleEnterpriseValues();
				var valuesWithoutMapping = new List<V>();
				var converter = new T();

				if (allPossibleEnterpriseValues != null)
				{
					foreach (U value in Enum.GetValues(typeof(U)))
					{
						V code = converter.FromEnumValue(value);

						if (!allPossibleEnterpriseValues.ToList().Contains(code))
						{
							valuesWithoutMapping.Add(code);
						}
					}

					AssertEquals(string.Format("There are codes in Universal XML which are not in Enterprise. These codes need to be removed from Universal XML enum: {0}"
						, string.Join(",", valuesWithoutMapping)), 0, valuesWithoutMapping.Count);
				}
				else
				{
					Assert("No values to map against Enterprise codes.", true);
				}
			}
			else
			{
				Assert("For other modules other than accounting it could be a valid case that Enum values would be in Universal XML but not in enterpise! For clarification ask Ben!", true);
			}
		}

		protected abstract IEnumerable<V> GetAllPossibleEnterpriseValues();
		protected virtual IEnumerable<V> GetEnterpriseValuesExcludedFromMapping()
		{
			return Array.Empty<V>();
		}
	}
}
