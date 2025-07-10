using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestsSubclassesOf(typeof(GenericWrapper))]
	public abstract class GenericWrapperTest : DocBaseWrapperTest
	{
		public abstract void TestWrapperMappingsEmpty();
		protected abstract ZString ExpectedDefaultFormatting { get; }
		protected abstract GenericWrapper GetSetupWrapperForDefaultFormatting();

		public void TestMakeSureFieldMapIsRightAndAllExposedPropertiesAreImplementedOnTheAppropriateBaseGenericWrapper()
		{
			AssertMultilineASCIIEquals("wrapper.GetFieldMap(false)", ExpectedFieldMap.Trim(), Wrapper.GetFieldMap(false));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CurrentCurrencySymbol = System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
		}
		string CurrentCurrencySymbol;

		protected override void TearDown()
		{
			AssertEquals("Currency symbol has changed during test and not been reset", CurrentCurrencySymbol, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol);
			base.TearDown();
		}

		public void TestDefaultFieldAttributeIsMappedProperlyCausingNoExpectionOnToString()
		{
			DefaultFieldAttribute[] defaultField = (DefaultFieldAttribute[])Wrapper.GetType().GetCustomAttributes(typeof(DefaultFieldAttribute), true);
			string expectedValue = string.Format(DefaultFieldAttribute.DefaultFieldNotImplementedMessage, Wrapper.HumanReadableName);
			if (defaultField.Length == 0)
			{
				AssertEquals(expectedValue, Wrapper.ToString());
			}
			else
			{
				AssertNotEquals(expectedValue, Wrapper.ToString());
			}
		}

		public void TestCheckDefaultFormattingForAllProperties()
		{
			List<string> result = new List<string>();
			GenericWrapper defaultFormattingWrapper = GetSetupWrapperForDefaultFormatting();
			foreach (System.Reflection.PropertyInfo propertyInfo in defaultFormattingWrapper.GetType().GetProperties())
			{
				if (typeof(GenericWrapper).IsAssignableFrom(propertyInfo.PropertyType))
				{
					object propertyValue = propertyInfo.GetValue(defaultFormattingWrapper, null);
					var name = propertyInfo.Name;
					result.Add(name + " : " + (propertyValue != null ? ((ZString)propertyValue.ToString()).Replace("\n", @"\n") : (ZString)" is null"));
				}
			}
			result.Sort();
			AssertMultilineASCIIEquals("Default Fields should be formatted correctly", ExpectedDefaultFormatting.Trim(), new ZStringBuilder(result).ToStringWithNewLineBetweenAppends().Trim());
		}

		#region Implementation
		protected new GenericWrapper Wrapper
		{
			get { return (GenericWrapper)base.Wrapper; }
		}

		protected abstract string ExpectedFieldMap { get; }
		#endregion
	}
}
