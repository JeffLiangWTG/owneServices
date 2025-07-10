using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class SequenceValidatorTest : TestCaseWithFactory
	{
		#region TestValidate

		public void TestValidate()
		{
			foreach (var info in PropertyInfos)
			{
				TestValidate(info);
			}
		}

		void TestValidate(ZPropertyInfo propertyInfo)
		{
			var relatedPropertyInfos = PropertyInfos;
			TestEnabledValues(propertyInfo, relatedPropertyInfos, 1, (ZByte)PropertyInfos.Length);
			TestOutOfRange(propertyInfo, 0, (ZByte)PropertyInfos.Length);
			TestUniqueness(propertyInfo, relatedPropertyInfos);
			TestDisabledValues(propertyInfo, "This item is disabled because it has a value of zero.", ZByte.Zero);
		}

		void ResetValues(ZPropertyInfo[] propertyInfos)
		{
			foreach (var propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZByte.Zero;
			}
		}

		void TestDisabledValues(ZPropertyInfo propertyInfo, string warningMessage, params ZByte[] disabledValues)
		{
			foreach (var disabledValue in disabledValues)
			{
				propertyInfo.Value = disabledValue;
				AssertHasWarning(propertyInfo, warningMessage);
			}
		}

		void TestEnabledValues(ZPropertyInfo propertyInfo, ZPropertyInfo[] relatedPropertyInfos, ZByte minEnabledValue, ZByte maxEnabledValue)
		{
			ResetValues(relatedPropertyInfos);
			for (var i = minEnabledValue; i <= maxEnabledValue; i++)
			{
				propertyInfo.Value = i;
				AssertNoErrors(propertyInfo);
			}
		}

		void TestOutOfRange(ZPropertyInfo propertyInfo, ZByte minValidValue, ZByte maxValidValue)
		{
			string prefix = Grammar.Instance.IndefiniteArticlePrefix(propertyInfo.HumanReadableName);
			string errorMessage = string.Format("Please enter {0}'{1}' within the range {2} to {3}.", prefix, propertyInfo.HumanReadableName, minValidValue, maxValidValue);

			if (!minValidValue.IsEmpty)
			{
				propertyInfo.Value = (ZByte)(minValidValue - 1);
				AssertHasError(propertyInfo, errorMessage);
			}

			propertyInfo.Value = (ZByte)(maxValidValue + 1);
			AssertHasError(propertyInfo, errorMessage);
		}

		void TestUniqueness(ZPropertyInfo propertyInfo, ZPropertyInfo[] relatedPropertyInfos)
		{
			foreach (var otherPropertyInfo in relatedPropertyInfos)
			{
				if (propertyInfo != otherPropertyInfo)
				{
					ResetValues(relatedPropertyInfos);
					string notUniqueErrorMessage = string.Format("The '{0}' you have entered has the same value as the '{1}'. Please enter a unique value.", propertyInfo.HumanReadableName, otherPropertyInfo.HumanReadableName);

					otherPropertyInfo.Value = (ZByte)1;
					propertyInfo.Value = (ZByte)1;
					AssertHasError(propertyInfo, notUniqueErrorMessage);

					propertyInfo.Value = (ZByte)2;
					AssertNoErrors(propertyInfo);
				}
			}
		}

		#region TestMustHaveAtLeastOneNonZeroValue

		public void TestMustHaveAtLeastOneNonZeroValue()
		{
			var testPropertyInfos = PropertyInfos;
			foreach (var propertyInfo in testPropertyInfos)
			{
				AssertNoErrors(propertyInfo);
			}

			ResetValues(testPropertyInfos);

			var allValuesAreZeroErrorMessage = "There must be at least one non-zero value.";
			foreach (var propertyInfo in testPropertyInfos)
			{
				if (MustHaveAtLeastOneNonZeroValue)
				{
					AssertHasError(propertyInfo, allValuesAreZeroErrorMessage);
				}
				else
				{
					AssertNoErrors(propertyInfo);
				}
			}

			testPropertyInfos[0].Value = (ZByte)1;
			testPropertyInfos[0].BizObj.RunPreSaveValidation();
			foreach (var propertyInfo in testPropertyInfos)
			{
				AssertNoErrors(propertyInfo);
			}
		}

		#endregion

		#endregion

		#region Implementation

		#region PropertyInfos

		ZPropertyInfo[] PropertyInfos => propertyInfos ??= GetPropertyInfos();

		protected abstract ZPropertyInfo[] GetPropertyInfos();

		ZPropertyInfo[] propertyInfos;

		protected abstract bool MustHaveAtLeastOneNonZeroValue { get; }

		#endregion

		#endregion

	}
}
