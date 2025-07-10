using System;
using System.Collections.Generic;
using CargoWise.Application.Testing;
using NUnit.Framework;

namespace CargoWise.Application.InversionOfControl.Testing
{
	public class ObjectDefinitionTests : TestCase
	{
		public void TestExceptionOnTypeResolutionFailureHasFullInformation()
		{
			var definition = new ObjectDefinition
			{
				Name = "UnknownObject",
				TypeName = "Foo.Bar.UnknownObject, Foo.Bar"
			};

			var exception = AssertExceptionThrown<CannotLoadObjectTypeException>(() =>
			{
				var x = definition.GetType(typeof(ObjectFactory.EmptyType));
			});

			CombineAssertions(() =>
			{
				AssertContains("Message should contain the definition name", definition.Name, exception.Message);
				AssertContains("Message should contain the definition type name", definition.TypeName, exception.Message);
				AssertNotNull("The InnerException should be set", exception.InnerException);
			});
		}

		public void TestGetType_ShouldCache_LastResolvedType()
		{
			var testImplementationType = typeof(Tuple<object, object>).GetGenericTypeDefinition();
			var definition = new ObjectDefinition
			{
				Name = nameof(ITestInterface),
				TypeName = $"{testImplementationType.FullName}, {testImplementationType.Assembly.GetName().Name}"
			};

			var requestType = typeof(KeyValuePair<int, string>);
			var resultType = definition.GetType(requestType);

			AssertEquals("Should get the correct type", typeof(Tuple<int, string>), resultType);

			AssertNotNull("Should cache last resolved type", definition.LastResolvedTypeCacheForTest);
			AssertEquals($"{definition.TypeName}|Args:{typeof(int).FullName},{typeof(string).FullName}", definition.LastResolvedTypeCacheForTest.Key);
			AssertEquals(typeof(Tuple<int, string>), definition.LastResolvedTypeCacheForTest.ResolvedType);
		}

		public void TestGetType_ShouldUseLastResolvedTypeCache_SameRequestedType()
		{
			var testImplementationType = typeof(TestImplementation);
			var definition = new ObjectDefinition
			{
				Name = nameof(ITestInterface),
				TypeName = $"{testImplementationType.FullName}, {testImplementationType.Assembly.GetName().Name}"
			};

			var cachedRequestType = typeof(ObjectFactory.EmptyType);
			var cachedResolvedType = typeof(ObjectDefinitionTests);

			var cacheKey = ObjectDefinition.GetTypeDefinitionCacheKey(cachedRequestType, definition);
			var resolvedTypeCache = new ObjectDefinition.ResolvedTypeCache(cacheKey, cachedResolvedType);
			definition.LastResolvedTypeCacheForTest = resolvedTypeCache;

			var resultType = definition.GetType(cachedRequestType);

			AssertEquals("Should return cached resolved type", cachedResolvedType, resultType);
		}

		public void TestGetType_ShouldUseLastResolvedTypeCache_DifferentRequestedType()
		{
			var testImplementationType = typeof(TestImplementation);
			var definition = new ObjectDefinition
			{
				Name = nameof(ITestInterface),
				TypeName = $"{testImplementationType.FullName}, {testImplementationType.Assembly.GetName().Name}"
			};

			var cachedRequestType = typeof(ObjectFactory.EmptyType);
			var cachedResolvedType = typeof(ObjectDefinitionTests);

			var cacheKey = ObjectDefinition.GetTypeDefinitionCacheKey(cachedRequestType, definition);
			var resolvedTypeCache = new ObjectDefinition.ResolvedTypeCache(cacheKey, cachedResolvedType);
			definition.LastResolvedTypeCacheForTest = resolvedTypeCache;

			var resultType = definition.GetType(typeof(ITestInterface));

			AssertEquals("Should return cached resolved type", cachedResolvedType, resultType);
		}

		public void TestGetType_ShouldUseLastResolvedTypeCache_DifferentRequestedType_SameGenericArguments()
		{
			var testImplementationType = typeof(Tuple<object, object>).GetGenericTypeDefinition();
			var definition = new ObjectDefinition
			{
				Name = nameof(ITestInterface),
				TypeName = $"{testImplementationType.FullName}, {testImplementationType.Assembly.GetName().Name}"
			};

			var cachedRequestType = typeof(Tuple<int, string>);
			var cachedResolvedType = typeof(ObjectDefinitionTests);

			var cacheKey = ObjectDefinition.GetTypeDefinitionCacheKey(cachedRequestType, definition);
			var resolvedTypeCache = new ObjectDefinition.ResolvedTypeCache(cacheKey, cachedResolvedType);
			definition.LastResolvedTypeCacheForTest = resolvedTypeCache;

			var resultType = definition.GetType(typeof(KeyValuePair<int, string>));

			AssertEquals("Should return cached resolved type", cachedResolvedType, resultType);
		}

		public void TestGetType_ShouldUseLastResolvedTypeCache_DifferentRequestedType_DifferentGenericArguments()
		{
			var testImplementationType = typeof(Tuple<object, object>).GetGenericTypeDefinition();
			var definition = new ObjectDefinition
			{
				Name = nameof(ITestInterface),
				TypeName = $"{testImplementationType.FullName}, {testImplementationType.Assembly.GetName().Name}"
			};

			var cachedRequestType = typeof(Tuple<int, string>);
			var cachedResolvedType = typeof(ObjectDefinitionTests);

			var cacheKey = ObjectDefinition.GetTypeDefinitionCacheKey(cachedRequestType, definition);
			var resolvedTypeCache = new ObjectDefinition.ResolvedTypeCache(cacheKey, cachedResolvedType);
			definition.LastResolvedTypeCacheForTest = resolvedTypeCache;

			var resultType = definition.GetType(typeof(KeyValuePair<int, Guid>));

			AssertEquals("Should ignore cached result for requested type with different generic arguments and resolve correct type again", typeof(Tuple<int, Guid>), resultType);
		}

		public void TestClone()
		{
			var objectConstructorArgumentDefinition = new ObjectConstructorArgumentDefinition()
			{
				Index = 1,
				ObjectName = "ObjectName1",
				ListValues = new[]
				{
					new ObjectListValueDefinition()
					{
						ObjectName = "ObjectName2"
					},
					new ObjectListValueDefinition()
					{
						ObjectName = "ObjectName3"
					}
				}
			};
			var objectPropertyDefinition = new ObjectPropertyDefinition()
			{
				DictionaryValues = new[]
				{
					new ObjectDictionaryValueDefinition()
					{
						Key = new ObjectDictionaryValueKeyDefinition()
						{
							Value = "Key1"
						},
						Value = "Key1Value"
					},
					new ObjectDictionaryValueDefinition()
					{
						Key = new ObjectDictionaryValueKeyDefinition()
						{
							Value = "Key2"
						},
						Value = "Key2Value"
					}
				},
				EnableParallelInit = false,
				IsSubSet = true,
				ListValues = new[]
				{
					new ObjectListValueDefinition()
					{
						ObjectName = "ObjectName7"
					},
					new ObjectListValueDefinition()
					{
						ObjectName = "ObjectName8"
					}
				},
				Name = "Name2",
				ObjectValue = new ObjectDefinition()
				{
					Name = "Name3"
				},
				Value = "Value1"
			};
			var definition = new ObjectDefinition
			{
				Name = "TestName",
				TypeName = "TestTypeName",
				ConstructorArguments = new[]
				{
					objectConstructorArgumentDefinition,
					new ObjectConstructorArgumentDefinition()
					{
						Index = 2,
						ObjectName = "ObjectName4",
						ListValues = new[]
						{
							new ObjectListValueDefinition()
							{
								ObjectName = "ObjectName5"
							},
							new ObjectListValueDefinition()
							{
								ObjectName = "ObjectName6"
							}
						}
					}
				},
				FactoryMethodName = "FactoryMethodName",
				IsSingleton = true,
				PropertyDefinitions = new[]
				{
					objectPropertyDefinition,
					new ObjectPropertyDefinition()
					{
						DictionaryValues = new[]
						{
							new ObjectDictionaryValueDefinition()
							{
								Key = new ObjectDictionaryValueKeyDefinition()
								{
									Value = "Key3"
								},
								Value = "Key3Value"
							},
							new ObjectDictionaryValueDefinition()
							{
								Key = new ObjectDictionaryValueKeyDefinition()
								{
									Value = "Key4"
								},
								Value = "Key4Value"
							}
						},
						EnableParallelInit = true,
						IsSubSet = false,
						ListValues = new[]
						{
							new ObjectListValueDefinition()
							{
								ObjectName = "ObjectName9"
							},
							new ObjectListValueDefinition()
							{
								ObjectName = "ObjectName10"
							}
						},
						Name = "Name5",
						ObjectValue = new ObjectDefinition()
						{
							Name = "Name6"
						},
						Value = "Value2"
					}
				},
				Singleton = new ObjectDefinition()
				{
					Name = "Name7"
				}
			};

			var clonedDefinition = definition.Clone();
			AssertEquals("Not same", false, object.ReferenceEquals(clonedDefinition, definition));
			AssertEquals("Name", "TestName", clonedDefinition.Name);
			AssertEquals("TypeName", "TestTypeName", clonedDefinition.TypeName);
			AssertEquals("FactoryMethodName", "FactoryMethodName", clonedDefinition.FactoryMethodName);
			AssertEquals("IsSingleton", true, clonedDefinition.IsSingleton);
			AssertEquals("clonedDefinition.Singleton.Name", "Name7", ((ObjectDefinition)clonedDefinition.Singleton).Name);

			AssertEquals("clonedDefinition.ConstructorArguments.Length", 2, clonedDefinition.ConstructorArguments.Length);
			var clonedObjectConstructorArgumentDefinition = clonedDefinition.ConstructorArguments[0];
			Assert("clonedDefinition.ConstructorArguments[0] should be cloned", !object.ReferenceEquals(objectConstructorArgumentDefinition, clonedObjectConstructorArgumentDefinition));
			AssertEquals("clonedObjectConstructorArgumentDefinition.Index", 1, clonedObjectConstructorArgumentDefinition.Index);
			AssertEquals("clonedObjectConstructorArgumentDefinition.ObjectName", "ObjectName1", clonedObjectConstructorArgumentDefinition.ObjectName);
			AssertEquals("clonedObjectConstructorArgumentDefinition.ListValues.Length", 2, clonedObjectConstructorArgumentDefinition.ListValues.Length);
			AssertEquals("clonedObjectConstructorArgumentDefinition.[0].ObjectName", "ObjectName2", clonedObjectConstructorArgumentDefinition.ListValues[0].ObjectName);
			AssertEquals("clonedObjectConstructorArgumentDefinition.[1].ObjectName", "ObjectName3", clonedObjectConstructorArgumentDefinition.ListValues[1].ObjectName);
			AssertEquals("clonedDefinition.ConstructorArguments[1].Index", 2, clonedDefinition.ConstructorArguments[1].Index);
			AssertEquals("clonedDefinition.ConstructorArguments[1].ObjectName", "ObjectName4", clonedDefinition.ConstructorArguments[1].ObjectName);
			AssertEquals("clonedDefinition.ConstructorArguments[1].ListValues.Length", 2, clonedDefinition.ConstructorArguments[1].ListValues.Length);
			AssertEquals("clonedDefinition.ConstructorArguments[1].[0].ObjectName", "ObjectName5", clonedDefinition.ConstructorArguments[1].ListValues[0].ObjectName);
			AssertEquals("clonedDefinition.ConstructorArguments[1].[1].ObjectName", "ObjectName6", clonedDefinition.ConstructorArguments[1].ListValues[1].ObjectName);

			AssertEquals("clonedDefinition.PropertyDefinitions.Length", 2, clonedDefinition.PropertyDefinitions.Length);
			var clonedObjectPropertyDefinition = clonedDefinition.PropertyDefinitions[0];
			Assert("clonedDefinition.PropertyDefinitions[0] should be cloned", !object.ReferenceEquals(objectPropertyDefinition, clonedObjectPropertyDefinition));
			AssertEquals("clonedObjectPropertyDefinition.EnableParallelInit", false, clonedObjectPropertyDefinition.EnableParallelInit);
			AssertEquals("clonedObjectPropertyDefinition.IsSubSet", true, clonedObjectPropertyDefinition.IsSubSet);
			AssertEquals("clonedObjectPropertyDefinition.Name", "Name2", clonedObjectPropertyDefinition.Name);
			AssertEquals("clonedObjectPropertyDefinition.ObjectValue.Name", "Name3", clonedObjectPropertyDefinition.ObjectValue.Name);
			AssertEquals("clonedObjectPropertyDefinition.Value", "Value1", clonedObjectPropertyDefinition.Value);

			AssertEquals("clonedObjectPropertyDefinition.DictionaryValues.Length", 2, clonedObjectPropertyDefinition.DictionaryValues.Length);
			AssertEquals("clonedObjectPropertyDefinition.DictionaryValues[0].Key.Value", "Key1", clonedObjectPropertyDefinition.DictionaryValues[0].Key.Value);
			AssertEquals("clonedObjectPropertyDefinition.DictionaryValues[0].Value", "Key1Value", clonedObjectPropertyDefinition.DictionaryValues[0].Value);
			AssertEquals("clonedObjectPropertyDefinition.DictionaryValues[1].Key.Value", "Key2", clonedObjectPropertyDefinition.DictionaryValues[1].Key.Value);
			AssertEquals("clonedObjectPropertyDefinition.DictionaryValues[1].Value", "Key2Value", clonedObjectPropertyDefinition.DictionaryValues[1].Value);

			AssertEquals("clonedObjectPropertyDefinition.ListValues.Length", 2, clonedObjectPropertyDefinition.ListValues.Length);
			AssertEquals("clonedObjectPropertyDefinition.ListValues[0].ObjectName", "ObjectName7", clonedObjectPropertyDefinition.ListValues[0].ObjectName);
			AssertEquals("clonedObjectPropertyDefinition.ListValues[1].ObjectName", "ObjectName8", clonedObjectPropertyDefinition.ListValues[1].ObjectName);

			AssertEquals("clonedDefinition.PropertyDefinitions[1].EnableParallelInit", true, clonedDefinition.PropertyDefinitions[1].EnableParallelInit);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].IsSubSet", false, clonedDefinition.PropertyDefinitions[1].IsSubSet);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].Name", "Name5", clonedDefinition.PropertyDefinitions[1].Name);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].ObjectValue.Name", "Name6", clonedDefinition.PropertyDefinitions[1].ObjectValue.Name);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].Value", "Value2", clonedDefinition.PropertyDefinitions[1].Value);

			AssertEquals("clonedDefinition.PropertyDefinitions[1].DictionaryValues.Length", 2, clonedDefinition.PropertyDefinitions[1].DictionaryValues.Length);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].DictionaryValues[0].Key.Value", "Key3", clonedDefinition.PropertyDefinitions[1].DictionaryValues[0].Key.Value);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].DictionaryValues[0].Value", "Key3Value", clonedDefinition.PropertyDefinitions[1].DictionaryValues[0].Value);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].DictionaryValues[1].Key.Value", "Key4", clonedDefinition.PropertyDefinitions[1].DictionaryValues[1].Key.Value);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].DictionaryValues[1].Value", "Key4Value", clonedDefinition.PropertyDefinitions[1].DictionaryValues[1].Value);

			AssertEquals("clonedDefinition.PropertyDefinitions[1].ListValues.Length", 2, clonedDefinition.PropertyDefinitions[1].ListValues.Length);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].ListValues[0].ObjectName", "ObjectName9", clonedDefinition.PropertyDefinitions[1].ListValues[0].ObjectName);
			AssertEquals("clonedDefinition.PropertyDefinitions[1].ListValues[1].ObjectName", "ObjectName10", clonedDefinition.PropertyDefinitions[1].ListValues[1].ObjectName);
		}

		public void TestGetTypeDefinitionCacheKey()
		{
			var definition = new ObjectDefinition
			{
				Name = "TestName",
				TypeName = "TestTypeName"
			};

			var key = ObjectDefinition.GetTypeDefinitionCacheKey(typeof(object), definition);
			AssertEquals("TestTypeName", key);

			key = ObjectDefinition.GetTypeDefinitionCacheKey(typeof(string), definition);
			AssertEquals("TestTypeName", key);

			key = ObjectDefinition.GetTypeDefinitionCacheKey(typeof(Tuple<int, string>), definition);
			AssertEquals($"TestTypeName|Args:{typeof(int).FullName},{typeof(string).FullName}", key);

			key = ObjectDefinition.GetTypeDefinitionCacheKey(typeof(Tuple<object, Guid>), definition);
			AssertEquals($"TestTypeName|Args:{typeof(object).FullName},{typeof(Guid).FullName}", key);

			key = ObjectDefinition.GetTypeDefinitionCacheKey(typeof(KeyValuePair<object, Guid>), definition);
			AssertEquals($"TestTypeName|Args:{typeof(object).FullName},{typeof(Guid).FullName}", key);
		}
	}
}
