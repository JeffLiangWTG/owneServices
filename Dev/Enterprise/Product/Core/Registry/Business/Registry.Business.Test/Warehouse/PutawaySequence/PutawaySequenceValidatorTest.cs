using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Registry.Business.Testing
{
	sealed class PutawaySequenceValidatorTest : TestCaseWithFactory
	{
		public void TestGetLocationSortOrderInfos()
		{
			ZPropertyInfo[] propertyInfos = Validator.GetLocationSortOrderPropertyInfos();
			AssertEquals("GetLocationSortOrderInfos().Length", 3, propertyInfos.Length);
			AssertEquals("GetLocationSortOrderInfos()[0]", Validator.consumer.ColumnInfo, propertyInfos[0]);
			AssertEquals("GetLocationSortOrderInfos()[1]", Validator.consumer.LevelInfo, propertyInfos[1]);
			AssertEquals("GetLocationSortOrderInfos()[2]", Validator.consumer.RowInfo, propertyInfos[2]);
		}

		public void TestGetPutawayAlgorithmSequenceInfos()
		{
			ZPropertyInfo[] propertyInfos = Validator.GetPutawayAlgorithmSequencePropertyInfos();
			AssertEquals("GetPickAlgorithmSequenceInfos().Length", 4, propertyInfos.Length);
			AssertEquals("GetPickAlgorithmSequenceInfos()[0]", Validator.consumer.ClientAreaInfo, propertyInfos[0]);
			AssertEquals("GetPickAlgorithmSequenceInfos()[1]", Validator.consumer.LocationInfo, propertyInfos[1]);
			AssertEquals("GetPickAlgorithmSequenceInfos()[2]", Validator.consumer.PickFaceInfo, propertyInfos[2]);
			AssertEquals("GetPickAlgorithmSequenceInfos()[3]", Validator.consumer.ProductAreaInfo, propertyInfos[3]);
		}

		public void TestValidateLocationSortOrder()
		{
			TestValidateSingleLocationSortOrder(Validator.consumer.ColumnInfo);
			TestValidateSingleLocationSortOrder(Validator.consumer.LevelInfo);
			TestValidateSingleLocationSortOrder(Validator.consumer.RowInfo);
		}

		public void TestValidatePickingAlgorithmSequence()
		{
			//Assert("implement with work item WI00004439 - CM", true);
			TestValidateSingleNonLocationPutawayAlgorithmSequence(Validator.consumer.ClientAreaInfo);
			TestValidateSingleNonLocationPutawayAlgorithmSequence(Validator.consumer.PickFaceInfo);
			TestValidateSingleNonLocationPutawayAlgorithmSequence(Validator.consumer.ProductAreaInfo);

			ZPropertyInfo[] relatedPropertyInfos = Validator.GetPutawayAlgorithmSequencePropertyInfos();
			TestEnabledValues(Validator.consumer.LocationInfo, relatedPropertyInfos, 1, 4);
			TestOutOfRange(Validator.consumer.LocationInfo, 1, 4);
			TestUniqueness(Validator.consumer.LocationInfo, relatedPropertyInfos);
		}

		#region Implementation

		PutawaySequenceValidator validator;

		PutawaySequenceValidator Validator
		{
			get
			{
				if (validator == null)
				{
					validator = new PutawaySequenceValidator(new PutawaySequence());
				}
				return validator;
			}
		}

		void ResetValues(ZPropertyInfo[] propertyInfos)
		{
			foreach (ZPropertyInfo propertyInfo in propertyInfos)
			{
				propertyInfo.Value = ZByte.Zero;
			}
		}

		void TestDisabledValues(ZPropertyInfo propertyInfo, string warningMessage, params ZByte[] disabledValues)
		{
			foreach (ZByte disabledValue in disabledValues)
			{
				propertyInfo.Value = disabledValue;
				AssertHasWarning(propertyInfo, warningMessage);
			}
		}

		void TestEnabledValues(ZPropertyInfo propertyInfo, ZPropertyInfo[] relatedPropertyInfos, ZByte minEnabledValue, ZByte maxEnabledValue)
		{
			ResetValues(relatedPropertyInfos);
			for (ZByte i = minEnabledValue; i <= maxEnabledValue; i++)
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
			foreach (ZPropertyInfo otherPropertyInfo in relatedPropertyInfos)
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

		void TestValidateSingleLocationSortOrder(ZPropertyInfo propertyInfo)
		{
			ZPropertyInfo[] relatedPropertyInfos = Validator.GetLocationSortOrderPropertyInfos();
			TestEnabledValues(propertyInfo, relatedPropertyInfos, 1, 3);
			TestOutOfRange(propertyInfo, 0, 3);
			TestUniqueness(propertyInfo, relatedPropertyInfos);
			TestDisabledValues(propertyInfo, "This sort order is disabled because it has a value of zero.", ZByte.Zero);
		}

		void TestValidateSingleNonLocationPutawayAlgorithmSequence(ZPropertyInfo propertyInfo)
		{
			ZPropertyInfo[] relatedPropertyInfos = Validator.GetPutawayAlgorithmSequencePropertyInfos();

			TestEnabledValues(propertyInfo, relatedPropertyInfos, 1, 4);
			TestOutOfRange(propertyInfo, 0, 4);

			ResetValues(relatedPropertyInfos);
			Validator.consumer.LocationInfo.Value = (ZByte)2;
			TestDisabledValues(propertyInfo, "This algorithm is disabled because it has a value of zero.", 0);
			TestDisabledValues(propertyInfo,
				string.Format("This algorithm is disabled because it has a value higher than the {0}.", Validator.consumer.LocationInfo.HumanReadableName),
				3);
			propertyInfo.Value = (ZByte)1;
			AssertNoWarnings(propertyInfo);

			TestUniqueness(propertyInfo, relatedPropertyInfos);
		}

		#endregion
	}
}
