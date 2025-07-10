using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class BusinessObjectValidationTestCase : TestCaseWithFactory
	{
		public static void AssertMandatoryValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			AssertErrorStartsWith(propertyInfo, isExpectingError, "Please enter ", "PropertyInfo " + propertyInfo.Name + " should have a mandatory validation error.");
		}

		public static void AssertListValidationInvalidCodeError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			AssertErrorStartsWith(propertyInfo, isExpectingError, "Enter a valid ", "PropertyInfo " + propertyInfo.Name + " should have a list validation error.");
		}

		public static void AssertListValidationInvalidCodeMessageError(ZPropertyInfo propertyInfo, bool isExpectingMessageError)
		{
			AssertHasMessageError(propertyInfo, isExpectingMessageError, ListValidation.InvalidCodeMessageError.ToString(), "PropertyInfo " + propertyInfo.Name + " should have a list validation message error.");
		}

		public static void AssertListValidationInvalidPKError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			AssertErrorStartsWith(propertyInfo, isExpectingError, "Enter a valid ", "PropertyInfo " + propertyInfo.Name + " should have a list validation error.");
		}

		public static void AssertPropertyIsUniqueInCollectionValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			AssertErrorContains(propertyInfo, isExpectingError, "has been duplicated and must be unique.", "PropertyInfo " + propertyInfo.Name + " should have a unique property in collection validation error.");
		}

		public static void AssertCheckNotNegativeValidationError(ZPropertyInfo propertyInfo, bool isExpectingError)
		{
			AssertErrorContains(propertyInfo, isExpectingError, "cannot be negative", "PropertyInfo " + propertyInfo.Name + " should have a negative value validation error.");
		}

		static void AssertErrorStartsWith(ZPropertyInfo propertyInfo, bool isExpectingError, string errorMessage, string failureMessage)
		{
			AssertError(propertyInfo, isExpectingError, errorMessage, failureMessage, true);
		}

		static void AssertErrorContains(ZPropertyInfo propertyInfo, bool isExpectingError, string errorMessage, string failureMessage)
		{
			AssertError(propertyInfo, isExpectingError, errorMessage, failureMessage, false);
		}

		static void AssertError(ZPropertyInfo propertyInfo, bool isExpectingError, string errorMessage, string failureMessage, bool onlyCheckBeginning)
		{
			bool errorMessageFound = false;
			foreach (INotification error in propertyInfo.GetErrors())
			{
				if (FindErrorMessageSegment(error.Message, errorMessage, onlyCheckBeginning))
				{
					errorMessageFound = true;
					break;
				}
			}

			AssertEquals(failureMessage, isExpectingError, errorMessageFound);
		}

		static bool FindErrorMessageSegment(string fullErrorMessage, string errorMessageSegment, bool onlyCheckBeginning)
		{
			return (onlyCheckBeginning)
				? fullErrorMessage.StartsWith(errorMessageSegment)
				: fullErrorMessage.IndexOf(errorMessageSegment) > -1;
		}

		static void AssertHasMessageError(ZPropertyInfo propertyInfo, bool isExpectingMessageError, string errorMessage, string failureMessage)
		{
			AssertMessageError(propertyInfo, isExpectingMessageError, errorMessage, failureMessage, false);
		}

		static void AssertMessageError(ZPropertyInfo propertyInfo, bool isExpectingMessageError, string errorMessage, string failureMessage, bool onlyCheckBeginning)
		{
			bool messageErrorFound = false;
			foreach (INotification error in propertyInfo.GetMessageErrors())
			{
				if (FindErrorMessageSegment(error.Message, errorMessage, onlyCheckBeginning))
				{
					messageErrorFound = true;
					break;
				}
			}

			AssertEquals(failureMessage, isExpectingMessageError, messageErrorFound);
		}

		#region AssertEntityValidation

		protected static ValidatorAsserter<TEntity> AssertEntityValidation<TEntity>(TEntity entity)
		{
			return new ValidatorAsserter<TEntity>(entity);
		}

		protected class ValidatorAsserter<TEntity>
		{
			internal ValidatorAsserter(TEntity entity)
			{
				Entity = entity;
			}

			TEntity Entity { get; }
			List<(object entity, PropertyInfo property, IValueMatcher @is)> ValueMatchers { get; } = new ();
			List<Action> ValidateMethods { get; } = new ();

			public ValidatorAsserter<TEntity> WhenProperty<TProp>(Expression<Func<TEntity, TProp>> propertySelector, ValueMatcher<TProp> @is) where TProp : IZType
			{
				var entityType = typeof(TEntity);
				if (propertySelector is LambdaExpression { Body: MemberExpression
					{
						Expression: var memberExpression,
						Member: { MemberType: MemberTypes.Property } memberInfo
					} })
				{
					// Match x => x.MyProperty
					if (memberExpression.NodeType == ExpressionType.Parameter)
					{
						var member = entityType.GetMember(memberInfo.Name).FirstOrDefault() ?? memberInfo;
						ValueMatchers.Add((Entity, member as PropertyInfo, @is));
						return this;
					}
					// Match x => x.IntermediaryEntity.MyProperty
					if (memberExpression is MemberExpression { NodeType: ExpressionType.MemberAccess, Member: { MemberType: MemberTypes.Property } intermediaryMemberInfo })
					{
						var intermediaryMember = entityType.GetMember(intermediaryMemberInfo.Name).FirstOrDefault() ?? intermediaryMemberInfo;
						var intermediaryEntity = (intermediaryMember as PropertyInfo)?.GetValue(Entity);
						var member = entityType.GetMember(memberInfo.Name).FirstOrDefault() ?? memberInfo;
						ValueMatchers.Add((intermediaryEntity, member as PropertyInfo, @is));
						return this;
					}
				}
				HtmlFail($"Could not find property {typeof(TProp)} within {entityType} with provided propertySelector<br />" +
					$"  Expected:  x => x.MyProperty OR x => x.IntermediaryEntity.MyProperty<br />" +
					$"  But found: {propertySelector}");
				return this;
			}

			public ValidatorAsserter<TEntity> WhenValidating(Action validateMethod)
			{
				ValidateMethods.Add(validateMethod);
				return this;
			}

			public void ShouldCheckThat(Func<TEntity, ZPropertyInfo> propertyInfoSelector, ValidationMatcher validationMatcher)
			{
				var propertyInfo = propertyInfoSelector(Entity);
				foreach (var permutation in GetAllValuePermutations())
				{
					var assertMessage = string.Join(", ", permutation.Select(x => $"{x.property.Name} is {x.value}"));
					CombineAssertions(assertMessage, () =>
					{
						foreach (var (entity, property, value) in permutation)
						{
							property.SetValue(entity, value);
						}
						ValidateMethods.ForEach(x => x?.Invoke());
						validationMatcher.AssertActualValue(propertyInfo);
					});
				}
			}

			IEnumerable<ICollection<(object entity, PropertyInfo property, object value)>> GetAllValuePermutations()
			{
				if (ValueMatchers.Count == 0)
				{
					return new[] { Array.Empty<(object entity, PropertyInfo property, object value)>() };
				}
				var valueMatcherEnumerator = ValueMatchers.GetEnumerator();
				var permutations = Enumerable.Empty<ICollection<(object entity, PropertyInfo property, object value)>>();
				var first = true;
				while (true)
				{
					if (valueMatcherEnumerator.MoveNext() && valueMatcherEnumerator.Current is var (entity, member, @is))
					{
						var permutations2 = first switch
						{
							true => @is.PossibleValuesUntyped.Select(value => new List<(object entity, PropertyInfo property, object value)> { (entity, member, value) }),
							false => permutations.SelectMany(permutation =>
							{
								return @is.PossibleValuesUntyped.Select(value =>
								{
									permutation.Add((entity, member, value));
									return permutation;
								});
							}),
						};
						permutations = permutations2;
						first = false;
						continue;
					}
					return permutations;
				}
			}
		}

		interface IValueMatcher
		{
			object[] PossibleValuesUntyped { get; }
		}

		protected class ValueMatcher<T> : IValueMatcher where T : IZType
		{
			internal ValueMatcher(params T[] possibleValues)
			{
				PossibleValues = possibleValues;
			}

			internal T[] PossibleValues { get; }

			object[] IValueMatcher.PossibleValuesUntyped => PossibleValues.Cast<object>().ToArray();
		}

		protected class ValueMatcherWithInvalidValue<T> : ValueMatcher<T> where T : IZType
		{
			internal ValueMatcherWithInvalidValue(T invalidValue, params T[] validValues) : base(validValues)
			{
				InvalidValue = invalidValue;
			}

			internal T InvalidValue { get; }
		}

		protected static class Is
		{
			public static ValueMatcher<T> EqualTo<T>(T value) where T : IZType => new (value);
			public static ValueMatcher<ZDecimal> EqualTo(decimal value) => new (value);
			public static ValueMatcher<ZString> EqualTo(string value) => new (value);
			public static ValueMatcher<T> EqualToAnyOf<T>(params T[] values) where T : IZType => new (values);
			public static ValueMatcher<ZDecimal> EqualToAnyOf(params decimal[] values) => new (values.Select(x => (ZDecimal)x).ToArray());
			public static ValueMatcher<ZString> EqualToAnyOf(params string[] values) => new (values.Select(x => (ZString)x).ToArray());

			public static ValueMatcherWithInvalidValue<ZDecimal> DecimalBelowZero => new (0m, 1m);
		}

		protected class ValidationMatcher
		{
			internal ValidationMatcher(Action<ZPropertyInfo> validationAsserter)
			{
				ValidationAsserter = validationAsserter;
			}

			Action<ZPropertyInfo> ValidationAsserter { get; }

			internal void AssertActualValue(ZPropertyInfo propertyInfo)
			{
				ValidationAsserter(propertyInfo);
			}
		}

		protected static class Has
		{
			public static ValidationMatcher MandatoryValueCannotBeNegative => MessageErrorContaining(MandatoryValidation.ValueCannotBeNegative);
			public static ValidationMatcher MandatoryValueCannotBeZero => MessageErrorContaining(MandatoryValidation.ValueCannotBeZero);
			public static ValidationMatcher MandatoryYouHaveNotEntered => MessageErrorContaining(MandatoryValidation.YouHaveNotEntered);

			#region Message Error

			public static ValidationMatcher NoMessageErrorContaining(string message) => new (propertyInfo =>
			{
				AssertNoMessageErrorContaining(propertyInfo, message);
			});

			public static ValidationMatcher MessageErrorContaining(string message = null) => new (propertyInfo =>
			{
				AssertHasMessageErrorContaining(propertyInfo, message);
			});

			public static ValidationMatcher MessageErrorIfInvalidCode(ZString invalidCode, ZString validCode, string message = null) => MessageErrorIfInvalidValueCore(invalidCode, validCode, message);

			public static ValidationMatcher MessageErrorIfValue<TProp>(ValueMatcherWithInvalidValue<TProp> @is, string message = null) where TProp : IZType => new (propertyInfo =>
			{
				message ??= ListValidation.InvalidCodeError;
				var invalidValue = @is.InvalidValue;
				@is.PossibleValues
					.Select(validValue => MessageErrorIfInvalidValueCore(invalidValue, validValue, message))
					.ForEach(validationMatcher => validationMatcher.AssertActualValue(propertyInfo));
			});

			static ValidationMatcher MessageErrorIfInvalidValueCore(IZType invalidValue, IZType validValue, string message = null) => new (propertyInfo =>
			{
				message ??= ListValidation.InvalidCodeError;
				propertyInfo.Value = propertyInfo.DefaultValue;
				propertyInfo.Value = invalidValue;
				AssertHasMessageErrorContaining(propertyInfo, message);
				propertyInfo.Value = validValue;
				AssertNoMessageErrorContaining(propertyInfo, message);
			});

			#endregion

			#region Error

			public static ValidationMatcher NoErrorContaining(string message) => new (propertyInfo =>
			{
				AssertNoErrorContaining(propertyInfo, message);
			});

			public static ValidationMatcher ErrorContaining(string message) => new (propertyInfo =>
			{
				AssertHasErrorContaining(propertyInfo, message);
			});

			public static ValidationMatcher ErrorIfInvalidCode(ZString invalidCode, ZString validCode, string message = null) => ErrorIfInvalidValueCore(invalidCode, validCode, message);

			public static ValidationMatcher ErrorIfValue<TProp>(ValueMatcherWithInvalidValue<TProp> @is, string message = null) where TProp : IZType => new (propertyInfo =>
			{
				message ??= ListValidation.InvalidCodeError;
				var invalidValue = @is.InvalidValue;
				@is.PossibleValues
					.Select(validValue => ErrorIfInvalidValueCore(invalidValue, validValue, message))
					.ForEach(validationMatcher => validationMatcher.AssertActualValue(propertyInfo));
			});

			static ValidationMatcher ErrorIfInvalidValueCore(IZType invalidValue, IZType validValue, string message = null) => new (propertyInfo =>
			{
				message ??= ListValidation.InvalidCodeError;
				propertyInfo.Value = propertyInfo.DefaultValue;
				propertyInfo.Value = invalidValue;
				AssertHasErrorContaining(propertyInfo, message);
				propertyInfo.Value = validValue;
				AssertNoErrorContaining(propertyInfo, message);
			});

			#endregion
		}

		#endregion
	}
}
