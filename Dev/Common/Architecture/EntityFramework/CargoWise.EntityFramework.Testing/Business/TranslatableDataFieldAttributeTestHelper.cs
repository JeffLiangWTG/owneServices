using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Cache.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.EntityFramework
{
	static class TranslatableDataFieldAttributeTestHelper
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Test code only")]
		[SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Test code only")]
		public static void TestProperty(Type type, string property, BusinessObjectFactory factory, Func<BusinessObjectFactory, BusinessObject> getNewBusinessObject)
		{
			Assertion.CombineAssertions(delegate
			{
				TranslatableDataFieldAttribute attribute;
				var linked = (LinkedTranslatableDataFieldAttribute)type.GetProperty(property).GetCustomAttributes(typeof(LinkedTranslatableDataFieldAttribute), true).SingleOrDefault();
				if (linked != null)
				{
					attribute = linked.attribute;
				}
				else
				{
					var attributes = type.GetProperty(property).GetCustomAttributes(typeof(TranslatableDataFieldAttribute), true);
					Assertion.AssertEquals("Expected one TranslatableDataFieldAttribute on " + property, 1, attributes.Length);
					attribute = attributes[0] as TranslatableDataFieldAttribute;
				}
				Assertion.AssertNotNull("Expected one TranslatableDataFieldAttribute on " + property, attribute);
				Assertion.AssertNotNull("Set the Type property on the TranslatableDataFieldAttribute for " + property, attribute.Type);
				Assertion.Assert("Description should contain both table and column descriptions, but is '" + attribute.Description + "'", attribute.Description.Trim() == attribute.Description);

				Assertion.AssertEquals(0, attribute.GetRuntimeCaptions().Except(attribute.GetCompileTimeSystemCaptions()).Count());

				ZPropertyInfo propertyInfo;
				int maxLength;

				if (linked == null)
				{
					string rootKeyPrefix = attribute.RootKeyPrefixWithSeperator;
					var list = attribute.GetSystemDefinedParentObjectsForTest(factory, type);
					var res = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
					var allKeys = new HashSet<string>();

					using (Res.HoldLanguageInstances())
					{
						foreach (var item in list)
						{
							ZString caption = (ZString)item[property];
							if (!caption.IsEmpty)
							{
								var multilingual = (MultilingualString)item[property + "Multilingual"];
								Assertion.AssertEquals(caption, (string)multilingual);
								Assertion.AssertType(typeof(ResourceString), multilingual);

								string resKey = ((ResourceString)multilingual).ResourceKey;
								Assertion.Assert("resource string key should start with " + rootKeyPrefix, resKey.StartsWith(rootKeyPrefix, StringComparison.Ordinal));
								var data = res.Get(resKey);
								Assertion.AssertNotNull($@"res.Get(""{resKey}"")", data);
								if (data.Caption != ZString.Empty)
								{
									Assertion.AssertNotNull("Expected data for resource string with key '" + resKey + "', caption '" + caption + "'", data);
									Assertion.AssertEquals(caption.Trim(), (ZString)data.Caption.Trim());
								}
								else
								{
									Assertion.AssertNotNull("Expected data for resource string with key '" + resKey + "', caption '" + caption + "'", data);
									Assertion.AssertEquals(caption.Trim(), (ZString)data.FullDescription.Trim());
								}
								allKeys.Add(resKey);

								maxLength = attribute.MaxLength;
								if (maxLength > -1)
								{
									foreach (var language in DataFile.GetAvailableLanguages())
									{
										if (language != Res.DefaultLanguage)
										{
											string translation = multilingual.ToString(language);
											if (translation.Length > maxLength)
											{
												var message = string.Format(CultureInfo.InvariantCulture, "Resource string with key {0} in {1} has length {2}, but {3} has MaxLength of {4}: {5}",
													((ResourceString)multilingual).ResourceKey, language, translation.Length, property, maxLength, translation);
												Assertion.Assert(message, false);
												ResourceStringContentTestTracker.LogFailure(language, ((ResourceString)multilingual).ResourceKey, message, caption, translation);
											}
										}
									}
								}
							}
						}
					}

					foreach (string key in res.AllKeys)
					{
						if (key.StartsWith(rootKeyPrefix, StringComparison.Ordinal))
						{
							Assertion.Assert("There is a resource string with key " + key + " which is not a system defined entry of " + property, allKeys.Contains(key));
						}
					}
				}

				var newItem = getNewBusinessObject(factory);
				propertyInfo = (ZPropertyInfo)newItem[property + "Info"];
				maxLength = propertyInfo.MaxLength;
				if (newItem.GetProperties().Find(property + "Multilingual", true).HasSetter())
				{
					Assertion.AssertEquals("Max length of " + property + "Multilingual should be set to the max length of " + property, maxLength, newItem.GetZPropertyInfo(property + "Multilingual").MaxLength);
				}
				newItem[property] = "Bad {0}";
				TestCaseWithFactory.AssertHasError("Call TranslatableDataFieldAttribute.Validate() in validation of " + property, propertyInfo, "Formatting error");
				newItem[property] = "{fail}";
				TestCaseWithFactory.AssertHasError("Call TranslatableDataFieldAttribute.Validate() in validation of " + property, propertyInfo, "Formatting error");
				newItem[property] = "Zee Test Item";
				TestCaseWithFactory.AssertNoErrors(propertyInfo);

				if (linked == null)
				{
					if (BusinessObjectFactory.HasTableName(type))
					{
						newItem.Factory.Save();

						Assertion.Assert("GetRuntimeCaptions() on " + property + " should include newly added " + type.Name, attribute.GetRuntimeCaptions().Select(r => r.ToString()).Contains("Zee Test Item"));
						Assertion.Assert("GetRuntimeCaptions() with context on " + property + " should include newly added " + type.Name, attribute.GetRuntimeCaptions(context: newItem).Select(r => r.ToString()).Contains("Zee Test Item"));

						newItem.Delete();
						newItem.Factory.Save();
					}

					Assertion.AssertContainsExactElementsInAnyOrder("Duplicates returned by GetCompileTimeSystemCaptions()", attribute.GetCompileTimeSystemCaptions().Distinct(new ResourceString.ResourceKeyEqualityComparer()), attribute.GetCompileTimeSystemCaptions());
					Assertion.AssertContainsExactElementsInAnyOrder("Duplicates returned by GetRuntimeCaptions()", attribute.GetRuntimeCaptions().Distinct(new ResourceString.ResourceKeyEqualityComparer()), attribute.GetRuntimeCaptions());
				}
			});
		}
	}
}
