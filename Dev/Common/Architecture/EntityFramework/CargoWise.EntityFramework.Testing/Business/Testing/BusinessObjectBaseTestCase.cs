using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace CargoWise.EntityFramework.Testing
{
	[TestsSubclassesOf(
		typeof(BusinessObject),
		typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute),
		new Type[] { typeof(IDocumentWrapper), typeof(NonPersistentBusinessObject) },
		ExcludeClientDlls = true)]
	public abstract class BusinessObjectBaseTestCase : TestCaseWithFactory
	{
		protected internal BusinessObjectBaseTestCase()
		{
		}

		#region Creation

		public void TestCreateNewBusinessObject()
		{
			AssertNotNull(GetNewBusinessObject());
		}

		#endregion

		#region Correct Type

		public void TestBusinessObjectIsExpectedTypeOrSubClass()
		{
			Type actual = GetNewBusinessObject().GetType();
			Type expected = GetExpectedBusinessObjectType();
			bool actualIsExpected = actual == expected;
			bool actualIsSubclassOfExpected = actual.IsSubclassOf(expected);
			var message = new StringBuilder("Business Object class - actual is expected or subclass thereof.<br />")
				.Append($"  Actual           = {actual}<br />")
				.Append($"  Expected         = {expected}<br />")
				.Append($"  ActualIsExpected = {actualIsExpected}<br />")
				.Append($"  ActualIsSubclass = {actualIsSubclassOfExpected}<br />");
			HtmlAssertEquals(message.ToString(), true, actualIsExpected || actualIsSubclassOfExpected);
		}

		#endregion

		#region Properties - Related Business Objects

		public void TestRelatedBusinessObjects()
		{
			SortedDictionary<string, IList<string>> result = new SortedDictionary<string, IList<string>>();

			List<string> errors = new List<string>();

			BusinessObject bizObj = GetNewBusinessObject();

			foreach (ZPropertyInfo propertyInfo in bizObj.ZPropertyInfoHash)
			{
				string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(propertyInfo.PropertyDescriptor);
				if (string.IsNullOrEmpty(relatedBizObjName) || IsRelatedBusinessObjectTestExcluded(propertyInfo))
				{
					continue;
				}

				PropertyDescriptor relatedDescriptor = TypeDescriptor.GetProperties(bizObj)[relatedBizObjName];
				if (relatedDescriptor == null)
				{
					errors.Add(string.Format("Cant find the referenced property '{0}'.", relatedBizObjName));
				}
				else if (!typeof(BusinessObject).IsAssignableFrom(relatedDescriptor.PropertyType))
				{
					errors.Add(string.Format("The referenced property '{0}' does not return a BusinessObject.", relatedBizObjName));
				}
				else
				{
					try
					{
						Factory.GetNull(relatedDescriptor.PropertyType);
					}
					catch (Exception ex)
					{
						errors.Add("Exception thrown while attempting to create a null BusinessObject:\r\n" + ex.ToString());
					}
				}

				if (errors.Count > 0)
				{
					result.Add(propertyInfo.Name, errors);
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("These properties have problems with their RelatedBusinessObjectAttribute.", result);
		}

		bool IsRelatedBusinessObjectTestExcluded(ZPropertyInfo propertyInfo)
		{
			return propertyInfo.PropertyDescriptor.Attributes[typeof(RelatedBusinessObjectTestExclude)] != null;
		}

		#endregion

		#region Properties - Null Reference Exceptions

		[ExpectNoExceptions]
		[SnailTest]
		public void TestNoPropertiesThrowNullReferenceException()
		{
			if (GetType().Namespace.StartsWith("Enterprise.ZArchitecture"))
			{
				StringBuilder propertiesWithNullReference = new StringBuilder();
				BusinessObject bizO = GetNewBusinessObject();
				foreach (PropertyInfo propertyInfo in bizO.GetType().GetProperties())
				{
					try
					{
						bool isAnIndexer = propertyInfo.PropertyType.FullName.IndexOf('[') > -1;
						if (!isAnIndexer)
						{
							propertyInfo.GetValue(bizO, Array.Empty<object>());
						}
					}
					catch (Exception e)
					{
						if (e.InnerException is NullReferenceException)
						{
							propertiesWithNullReference.Append(propertyInfo.ToString() + System.Environment.NewLine);
						}
					}
					finally
					{
						ErrorReporter.Clear();
					}
				}
				if (propertiesWithNullReference.Length > 0)
				{
					Fail("The following properties threw null reference exceptions" + System.Environment.NewLine + System.Environment.NewLine + propertiesWithNullReference.ToString());
				}
			}
		}

		public void TestDocumentSupportableNotNull()
		{
			BusinessObject bizo = GetNewBusinessObject();
			Type iDocumentSupportableType = Type.GetType("Enterprise.DocumentEngineCore.DocumentSupport.IDocumentSupportable, Enterprise.DocumentEngineCore", true);
			if (iDocumentSupportableType.IsAssignableFrom(bizo.GetType()))
			{
				object documentSupporter = iDocumentSupportableType.InvokeMember("get_DocumentSupporter", BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, bizo, null);
				AssertNotNull("DocumentSupporter should not be null", documentSupporter);
			}
			else
			{
				AssertionCount++; // Not applicable - no need to assert anything.
			}
		}

		#endregion

		#region Array Properties - Return Null

		public void TestArrayPropertiesDoNotReturnNull()
		{
			var bizO = GetNewBusinessObject();
			var results = new StringBuilder();

			foreach (KPropertyDescriptor property in bizO.GetProperties())
			{
				var propertyName = Regex.Replace(property.PropertyType.FullName, @"\[\[.*Version=.*Culture=.*PublicKeyToken=.*\]\]", "", RegexOptions.IgnoreCase);

				if (propertyName.IndexOf('[') > -1 && !propertyName.StartsWith("System.Nullable"))
				{
					//TODO: remove this block once Henry changes interface for documents
					if (propertyName != "Enterprise.Core.Constants+BusinessContext[]")
					{
						if (property.Attributes[typeof(BusinessObjectTestExclude)] == null)
						{
							try
							{
								var returnsNull = property.GetValue(bizO) == null;

								if (returnsNull)
								{
									results.Append(property.Name + " << NULL" + System.Environment.NewLine);
								}
							}
							catch (Exception e)
							{
								results.Append(property.Name + " << Exception = " + e + System.Environment.NewLine);
							}
						}
					}
				}
			}

			if (results.Length > 0)
			{
				Fail("These properties return null or throw exceptions" +
					System.Environment.NewLine +
					System.Environment.NewLine +
					results.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Collection Properties - Return Null

		public void TestCollectionPropertiesDoNotReturnNull()
		{
			BusinessObject bizO = GetNewBusinessObject();
			StringBuilder results = new StringBuilder();
			foreach (KPropertyDescriptor property in bizO.GetProperties())
			{
				if (typeof(IBusinessObjectCollection).IsAssignableFrom(property.PropertyType) &&
					property.PropertyType != typeof(IBusinessObjectCollection) &&
					property.PropertyType != typeof(BusinessObjectCollection) &&
					property.Attributes[typeof(BusinessObjectTestExclude)] == null)
				{
					try
					{
						bool returnsNull = property.GetValue(bizO) == null;
						// this test doesn't care if properties throw silent exceptions, only that they don't return null
						ErrorReporter.Clear();
						if (returnsNull)
						{
							results.Append(property.Name + " << NULL" + System.Environment.NewLine);
						}
					}
					catch (Exception e)
					{
						if (!(e is NotSupportedException) && !(e is InvalidOperationException) && !(e.InnerException is NotSupportedException) && !(e.InnerException is InvalidOperationException))
						{
							ZString message = e.InnerException != null ? GetExceptionInfo(e.InnerException) : GetExceptionInfo(e);
							results.Append(property.Name + " << Exception = " + message + System.Environment.NewLine);
						}
					}
				}
			}

			if (results.Length > 0)
			{
				Fail("These properties return null or throw exceptions" +
					System.Environment.NewLine +
					System.Environment.NewLine +
					results.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		protected string GetExceptionInfo(Exception e)
		{
			return e.Message + "\r\n" + e.StackTrace;
		}

		#endregion

		#region ZQuery Properties - Return Null

		public void TestZQueryPropertiesDoNotReturnNull()
		{
			BusinessObject bizO = GetNewBusinessObject();
			StringBuilder results = new StringBuilder();
			foreach (KPropertyDescriptor property in bizO.GetProperties())
			{
				if (property.PropertyType == typeof(ZQuery) || property.PropertyType.IsSubclassOf(typeof(ZQuery)))
				{
					try
					{
						bool returnsNull = property.GetValue(bizO) == null;
						if (returnsNull)
						{
							results.Append(property.Name + " << NULL" + System.Environment.NewLine);
						}
					}
					catch (Exception e)
					{
						results.Append(property.Name + " << Exception = " + e.Message + System.Environment.NewLine);
					}
				}
			}

			if (results.Length > 0)
			{
				Fail("These properties return null or throw exceptions" + System.Environment.NewLine + System.Environment.NewLine + results);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestSettingValueCallsRefreshBinding
		public virtual void TestSettingValueCallsRefreshBinding()
		{
			BusinessObject bo = GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			TestSettingValueCallsRefreshBindingCore(bo);
		}

		protected void TestSettingValueCallsRefreshBindingCore(BusinessObject bo)
		{
			List<ZPropertyInfo> propertyInfosWithProblems = new List<ZPropertyInfo>();
			Type boType = bo.GetType();
			Type zPropertyInfoType = typeof(ZPropertyInfo);
			Type sQLComparisonOperatorType = typeof(SQLComparisonOperator);
			using (bo.GetValidationSuspender())
			{
				foreach (PropertyInfo infoProperty in boType.GetProperties())
				{
					if (zPropertyInfoType.IsAssignableFrom(infoProperty.PropertyType))
					{
						ZPropertyInfo info = (ZPropertyInfo)infoProperty.GetValue(bo, null);
						if (info != null && info.PropertyType != sQLComparisonOperatorType && info.PropertyDescriptor.Attributes[typeof(BusinessObjectTestExclude)] == null)
						{
							string propertyName = infoProperty.Name.Substring(0, infoProperty.Name.Length - 4);
							PropertyInfo property = null;
							try
							{
								property = boType.GetProperty(propertyName, info.PropertyType);
							}
							catch (AmbiguousMatchException)
							{
								// do nothing
							}
							if (property != null && HasPropertySetter(property))
							{
								if (IsPartOfClassOrInheritedFromAnAbstractClass(infoProperty, boType) || IsPartOfClassOrInheritedFromAnAbstractClass(property, boType))
								{
									bool isSettingSupported = true;
									IZType originalValue = info.Value;
									try
									{
										SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
										info.ValueChanged -= new EventHandler(info_ValueChanged);
										info.ValueChanged += new EventHandler(info_ValueChanged);
										info_ValueChangedCalled = false;
										ErrorReporter.Clear();
										if (!TestWithCachedValue(info))
										{
											ZPropertyInfoTestHelper.SetValue(info);
										}
										if (!info_ValueChangedCalled)
										{
											propertyInfosWithProblems.Add(info);
										}
									}
									catch (TargetInvocationException e)
									{
										if (IsNotSupportedOrReadOnlyException(e))
										{
											isSettingSupported = false;
											// this property should be ignored
										}
										else
										{
											throw;
										}
									}
									finally
									{
										if (isSettingSupported)
										{
											bool hadError = !string.IsNullOrEmpty(ErrorReporter.LastMessageReported) || ErrorReporter.LastExceptionReported != null || !string.IsNullOrEmpty(ErrorReporter.LastKeyReported);
											if (!hadError)
											{
												info.Value = originalValue;
												// just in case setting back to originalvalue has caused a error
												ErrorReporter.Clear();
											}
										}
										info.ValueChanged -= new EventHandler(info_ValueChanged);
									}
								}
							}
						}
					}
				}
				ZStringBuilder problems = new ZStringBuilder();
				foreach (ZPropertyInfo propertyInfo in propertyInfosWithProblems)
				{
					problems.Append(propertyInfo.PropertyType.Name + " " + propertyInfo.Name);
				}
				if (problems.IsEmpty)
				{
					Assert("All sweet", true);
				}
				else
				{
					Fail("In: " + boType.FullName + "\r\n"
							+ "The following properties have a setter that does not call refreshbinding:\r\n\r\n"
							+ problems.ToStringWithNewLineBetweenAppends()
							+ "\r\n\r\nThis causes problems when using on a form or hooking event to ZPropertyInfo.ValueChanged.\r\n"
							+ "Solution:\r\n"
							+ "- Use ZWrappedPropertyInfo if the property is setting another persistent property.\r\n"
							+ "- Use SetNonPersistentPropertyValue to set non persistent values; eg. SetNonPersistencePropertyValue<ZString>(HelloWorldInfo, ref helloWorld, value);\r\n"
							+ "- Use SetPropertyValue to set persistent values.\r\n"
							+ "- Call RefreshBinding in the setter; eg. HelloWorldInfo.RefreshBinding();");
				}
			}
		}

		protected virtual void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
		}

		bool IsNotSupportedOrReadOnlyException(Exception e)
		{
			Exception innerException = e.InnerException;
			while (innerException != null)
			{
				if (innerException is NotSupportedException || innerException is System.Data.ReadOnlyException)
				{
					return true;
				}
				innerException = innerException.InnerException;
			}
			return false;
		}

		bool TestWithCachedValue(ZPropertyInfo info)
		{
			if (CachedValueForSettingValueCallsRefreshBindingTest.ContainsKey(info.Name))
			{
				info.Value = CachedValueForSettingValueCallsRefreshBindingTest[info.Name];
				return true;
			}
			return false;
		}

		Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTest
		{
			get { return cachedValueForSettingValueCallsRefreshBindingTest ?? (cachedValueForSettingValueCallsRefreshBindingTest = CachedValueForSettingValueCallsRefreshBindingTestCore); }
		}
		Dictionary<string, IZType> cachedValueForSettingValueCallsRefreshBindingTest;

		protected virtual Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get { return new Dictionary<string, IZType>(); }
		}

		bool HasPropertySetter(PropertyInfo property)
		{
			bool result = false;
			if (property.CanWrite)
			{
				MethodInfo setMethodInfo = property.GetSetMethod();
				if (setMethodInfo != null)
				{
					byte[] body = property.GetSetMethod().GetMethodBody().GetILAsByteArray();
					result = body.Length > 2;
				}
			}
			return result;
		}

		bool IsPartOfClassOrInheritedFromAnAbstractClass(PropertyInfo info, Type boType)
		{
			bool result = true;
			if (info.DeclaringType != boType)
			{
				Type checkType = boType.BaseType;
				while (checkType != typeof(BusinessObject))
				{
					if (!checkType.IsAbstract)
					{
						result = false;
						break;
					}
					if (info.DeclaringType == checkType)
					{
						break;
					}
					checkType = checkType.BaseType;
				}
			}
			return result;
		}

		bool info_ValueChangedCalled;
		void info_ValueChanged(object sender, EventArgs e)
		{
			info_ValueChangedCalled = true;
		}

		protected virtual BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			return GetNewBusinessObject();
		}
		#endregion

		#region Business Object Fields

		[ExpectNoExceptions]
		public virtual void TestBizObjectFields()
		{
			// Iterate through all public properties of the Business Object
			BusinessObject bizO = GetNewBusinessObject();
			TestBizObjectFieldsCore(bizO);
		}

		protected void TestBizObjectFieldsCore(BusinessObject bizO)
		{
			foreach (ZPropertyInfo info in bizO.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				TestBizObjectField(info);
				using (info.BizObj.GetValidationSuspender())
				{
					try
					{
						TestBizObjectField(info);
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

		protected virtual List<string> ColumnsToClearValueAfterTested
		{
			get { return new List<string>(); }
		}

		protected virtual void TestBizObjectField(ZPropertyInfo info)
		{
			BusinessObject bizO = info.BizObj;
			if (info.HasSetter)
			{
				bool isExcluded = (info.PropertyDescriptor.Attributes[typeof(BusinessObjectTestExclude)] != null);

				if (!isExcluded)
				{
					IZType value = GetValueFromBusinessObject(bizO, info.Name);
					if (value is ZBlob)
					{
						AssertZBlobDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZBool)
					{
						AssertZBoolDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZByte)
					{
						AssertZByteDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZDateTime)
					{
						AssertZDateTimeDefaultBehaviour(bizO, info);
					}
					else if (value is ZDecimal)
					{
						AssertZDecimalDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZGuid)
					{
						AssertZGuidDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZInt)
					{
						AssertZIntDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZShort)
					{
						AssertZShortDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZString)
					{
						AssertZStringDefaultBehaviour(bizO, info.Name);
					}
					else if (value is ZTime)
					{
						AssertZTimeDefaultBehaviour(bizO, info.Name);
					}
					else
					{
						return;
					}

					SetValueToBusinessObject(bizO, info.Name, value);

					TestBizObjectTranslatableDataField(info);
				}
			}
		}

		protected virtual void TestBizObjectTranslatableDataField(ZPropertyInfo info)
		{
			var translatableDataFieldAttribute = info.PropertyDescriptor.Attributes[typeof(TranslatableDataFieldAttribute)] as TranslatableDataFieldAttribute;
			var linkedTranslatableDataFieldAttribute = info.PropertyDescriptor.Attributes[typeof(LinkedTranslatableDataFieldAttribute)] as LinkedTranslatableDataFieldAttribute;
			if (translatableDataFieldAttribute != null || (linkedTranslatableDataFieldAttribute != null && !linkedTranslatableDataFieldAttribute.skipValidation))
			{
				TranslatableDataFieldAttributeTestHelper.TestProperty(info.BizObj.GetType(), info.Name, NewFactory(), GetNewBusinessObjectForTranslatableFieldTest);
			}
		}

		#endregion

		#region PropertyInfos have corresponding Properties

		[ExpectNoExceptions]
		public void TestPropertyInfosHaveCorrespondingProperties()
		{
			PropertyDescriptorCollection properties = ZCustomTypeDescriptor.GetProperties(ExpectedBusinessObjectType);
			foreach (KPropertyDescriptor property in properties)
			{
				if (property.PropertyType.IsAssignableFrom(typeof(ZPropertyInfo)) &&
					(typeof(IZType).IsAssignableFrom(property.PropertyType)))
				{
					ZPropertyInfo propertyInfo = (ZPropertyInfo)property.GetValue(CachedBusinessObject);
					AssertNotNull("Property info not found for " + property.Name, propertyInfo);
					AssertNotNull(
						"Exists property for ZPropertyInfo property '" + property.Name + "'",
						properties[propertyInfo.Name]);
				}
			}
		}

		[ExpectNoExceptions]
		public virtual void TestPropertyInfoShouldGetRelevantPropertyValue()
		{
			var bizo = GetNewBusinessObject();
			if (bizo != null)
			{
				var propertyInfos = bizo.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping)
					.Cast<ZPropertyInfo>()
					.Where(p => p.PropertyType != typeof(SQLComparisonOperator) && !typeof(IZType).IsAssignableFrom(p.PropertyType));

				if (propertyInfos.Any())
				{
					Fail($"ZPropertyInfo(s) should be linked to IZType properties.\r\nClass:{ExpectedBusinessObjectType.FullName},\r\nProperties:{string.Join(", ", propertyInfos.Select(p => p.Name))}");
				}
			}
		}

		#endregion

		#region Validation Object access does not blow up

		public void TestValidationObjectAccessDoesNotBlowUp()
		{
			BusinessObject bizO = GetNewBusinessObject();

			if (bizO is IObsoleteValidation)
			{
				Assert("Not applicable to obsolete validation classes", true);
			}
			else
			{
				Assert(bizO.ValidationInternal is ZValidation || bizO.ValidationInternal == null);
			}
		}

		#endregion

		#region Validation Object is not cached

		public void TestValidationObjectIsNotCached()
		{
			BusinessObject bizO = GetNewBusinessObject();

			if (!(bizO is IObsoleteValidation) && bizO.ValidationInternal != null)
			{
				Assert(
					"BusinessObject: <" + bizO.GetType().FullName + ">" + System.Environment.NewLine +
					"Validation objects are lightweight and should not be cached. Caching the object is potentially dangerous - " +
					"a class you inherit from may return different validation objects depending on its business logic.",
					bizO.ValidationInternal != bizO.ValidationInternal);
			}
			else
			{
				AssertionCount++;
			}
		}

		#endregion

		#region Popular Static Methods
		[ExpectNoExceptions]
		public void TestStaticNewMathodWithFactoryAsParameterReturnsCorrectType()
		{
			BusinessObject bizO = GetNewBusinessObject();
			Type bizOType = bizO.GetType();
			MethodInfo mInfo = bizOType.GetMethod("New", new Type[] { typeof(BusinessObjectFactory) });
			if (mInfo != null)
			{
				object result = mInfo.Invoke(bizO, new object[] { Factory });
				AssertNotNull(result);
				AssertEquals("New return type", bizOType, result.GetType());
			}
		}
		#endregion

		#region Properties - DecimalPlacesAttribute only on ZDecimal

		public void TestDecimalPlacesAttributeOnZDecimalOnly()
		{
			StringBuilder report = null;

			BusinessObject bizObj = GetNewBusinessObject();
			foreach (ZPropertyInfo propertyInfo in bizObj.ZPropertyInfoHash)
			{
				if (propertyInfo.PropertyDescriptor.GetAttributeFromMostSpecificComponentType(typeof(DecimalPlacesAttribute)) != null &&
					propertyInfo.PropertyType != typeof(ZDecimal))
				{
					if (report == null)
					{
						report = new StringBuilder();
						report.AppendLine("These properties have [DecimalPlacesAttribute()] applied, which can be used only with ZDecimal properties:").AppendLine();
					}
					report.AppendLine(propertyInfo.PropertyType.Name + " " + propertyInfo.Name);
				}
			}

			if (report != null)
			{
				Fail(report.ToString());
			}
			else
			{
				Assert("Test passed ok, and it is not empty", true);
			}
		}

		#endregion

		#region XmlColumnPropertyAttribute

		public void TestXmlColumnPropertyAttribute_ForZTypeProperties_ShouldCallSetXmlColumnPropertyValue()
		{
			var bizo = GetNewBusinessObject();

			CombineAssertions(string.Format("Properties marked with {0} attribute should call SetXmlColumnPropertyValue in their setter so it ends up serialised against the bizo's XML column.", nameof(XmlColumnPropertyAttribute)), () =>
			{
				foreach (var property in BusinessObjectXmlHelper.GetXmlColumnStrategies(bizo))
				{
					var value = GetNonDefaultValue(property.ColumnType);

					if (value != null)
					{
						var propertyInfo = bizo.ZPropertyInfoHash[property.Name];

						SetPropertyInfoValue_ForXmlColumnPropertyAttributeTest(propertyInfo, value);

						IZType setValue;
						bizo.XmlColumnDictionary.Values.TryGetValue(property.Name, out setValue);

						AssertEquals(property.Name, value, setValue);
					}
				}
			});

			Assert("It's OK if there are no XML columns to check", true);
		}

		protected virtual void SetPropertyInfoValue_ForXmlColumnPropertyAttributeTest(ZPropertyInfo propertyInfo, IZType value)
		{
			propertyInfo.Value = value;
		}

		public void TestXmlColumnPropertyAttribute_ForZTypeProperties_ShouldCallGetXmlColumnPropertyValue()
		{
			var bizo = GetNewBusinessObject();

			CombineAssertions(string.Format("Properties marked with {0} attribute should call GetXmlColumnPropertyValue in their getter so it uses deserialised values from the bizo's XML column.", nameof(XmlColumnPropertyAttribute)), () =>
			{
				foreach (var property in BusinessObjectXmlHelper.GetXmlColumnStrategies(bizo))
				{
					var value = GetNonDefaultValue(property.ColumnType);

					if (value != null)
					{
						bizo.XmlColumnDictionary.Values[property.Name] = value;
						AssertEquals(property.Name, value, property.GetValue(bizo));
					}
				}
			});

			Assert("It's OK if there are no XML columns to check", true);
		}

		public void TestXmlColumnPropertyAttribute_ShouldNotBeDefinedOnReadOnlyProperties()
		{
			var bizo = GetNewBusinessObject();
			if (!bizo.XmlSerialisedColumns.Any())
			{
				Assert("Nothing to test", true);
			}
			else
			{
				var properties = bizo.XmlSerialisedColumns.SelectMany(c => BusinessObjectXmlHelper.GetXmlColumnProperties(bizo, bizo, c)).ToArray();
				if (properties.Length == 0)
				{
					Fail("Xml column serialisation is being run but no columns are serialised - don't override XmlSerialisedColumns in this case!");
				}
				else
				{
					CombineAssertions(() =>
					{
						foreach (var (property, attribute) in properties.Where(p => typeof(IZType).IsAssignableFrom(p.PropertyInfo.PropertyType)))
						{
							var setMethod = property.GetSetMethod();

							var message = string.Format("Property [{0}].[{1}] should have a public setter for XML deserialisation", property.DeclaringType.FullName, property.Name);
							AssertNotNull(message, setMethod);
						}
					});

					Assert("There might just be collection properties - we're cool with that.", true);
				}
			}
		}

		public void TestXmlColumnMembers_DoNotRequireATransformRightNow()
		{
			var bizo = GetNewBusinessObject();
			var xmlMembers = BusinessObjectXmlHelper.GetXmlColumnStrategies(bizo).Select(m => m.Name).OrderBy(s => s).ToArray();

			var property = typeof(BusinessObjectBaseTestCase).GetProperty("XmlMemberNames", BindingFlags.Instance | BindingFlags.NonPublic);
			var message = string.Format(@"
Business objects which use XmlColumnProperty attributes must override {0}.{1} supplying names for every property or field using the attribute.
{1} must exactly match the names of properties and fields. Any differences could indicate a member has been renamed or removed. Any renamed members will require a transformation, otherwise existing serialised values will be lost. See XmlColumnElementRenameTransformation.
If this is a new property, please add to {1} to protect it from later renames.
", property.DeclaringType.Name, property.Name);

			AssertContainsExactElementsInAnyOrder(message, XmlMemberNames.OrderBy(s => s), xmlMembers);
		}

		public void TestXmlProperties_ShouldNotBeMandatory()
		{
			var bizo = GetNewBusinessObject();

			if (!bizo.XmlSerialisedColumns.Any())
			{
				Assert("Nothing to test", true);
			}
			else
			{
				bizo.RunPreSaveValidation();

				foreach (var column in bizo.XmlSerialisedColumns)
				{
					var propertyInfo = bizo.ZPropertyInfoHash.GetPropertySafe(column.XmlColumnName);

					if (propertyInfo != null)
					{
						propertyInfo.Value = ZString.Empty;

						AssertNoErrors($"There should be no validation preventing empty values in the XML column {column.XmlColumnName}, since we store an empty string if there are only default values on properties that are serialised onto this column", propertyInfo);
					}
					else
					{
						Assert("There wasn't a ZPropertyInfo for this business object, so it must descent from NonPersistentBusinessObject. It's likely there's some custom serialisation going on here.", bizo is NonPersistentBusinessObject);
					}
				}
			}
		}

		protected virtual IEnumerable<string> XmlMemberNames
		{
			get { yield break; }
		}

		static IZType GetNonDefaultValue(Type type)
		{
			if (type == typeof(ZBool))
			{
				return ZBool.True;
			}
			else if (type == typeof(ZByte))
			{
				return new ZByte(1);
			}
			else if (type == typeof(ZBlob))
			{
				return new ZBlob(new byte[] { 1, 2, 3 });
			}
			else if (type == typeof(ZInt))
			{
				return new ZInt(1);
			}
			else if (type == typeof(ZString))
			{
				return new ZString("one");
			}
			else if (type == typeof(ZDate))
			{
				return ZDate.BrettsBirthday;
			}
			else if (type == typeof(ZDateTime))
			{
				return ZDateTime.BrettsBirthday;
			}
			else if (type == typeof(ZDateTimeOffset))
			{
				return new ZDateTimeOffset(ZDateTime.BrettsBirthday, TimeSpan.FromHours(10));
			}
			else if (type == typeof(ZGeography))
			{
				return new ZGeography("-122,49.3");
			}
			else if (type == typeof(ZGuid))
			{
				return ZGuid.NewZGuid();
			}
			else if (type == typeof(ZShort))
			{
				return new ZShort(1);
			}
			else if (type == typeof(ZDecimal))
			{
				return new ZDecimal(1);
			}
			else if (type == typeof(ZTime))
			{
				return new ZTime(12, 34);
			}
			else if (typeof(IZType).IsAssignableFrom(type))
			{
				throw new InvalidOperationException("ZType " + type.Name + " is not supported in this test. Consider adding it above.");
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Lookups

		[ExpectNoExceptions]
		[DeveloperOnlyTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1080:DoNotUseBusinessObjectCollectionIsAssignableFrom", Justification = "Testing")]
		public void TestLookupsIsFlyweight()
		{
			var bizObj = GetNewBusinessObject();
			var boType = bizObj.GetType();
			var lookupsProperty = GetProperty(boType, "Lookups");

			if (lookupsProperty != null)
			{
				var boLookups = lookupsProperty.GetValue(bizObj, null);
				var bf = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
				var boLookupsMembers = boLookups
					.GetType()
					.GetFields(bf)
					.Where(x =>
						!typeof(BusinessObjectCollection).IsAssignableFrom(x.FieldType)
						&& !x.FieldType.IsSubclassOf(typeof(BusinessObject))
						&& x.FieldType != typeof(BusinessObjectFactory)
						&& !typeof(IActiveBusinessObjectCollection).IsAssignableFrom(x.FieldType)
						).Select(x => x.Name);
				var zLookupsMembers = typeof(ZLookups).GetFields(bf).Select(x => x.Name);
				var difference = boLookupsMembers.Except(zLookupsMembers).ToArray();

				Assert("THIS TEST DOES NOT YET FAIL ON DAT.\r\nIf you have a false positive please see Brett Shearer.\r\nSubclasses of zLookups must have no new instance variables.\r\n" + boLookups.GetType().FullName + " defines the following fields : " + String.Join(", ", difference), !difference.Any());
			}
		}

		static PropertyInfo GetProperty(Type componentType, string propertyName)
		{
			PropertyInfo result = null;
			try
			{
				result = componentType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
			}
			catch (AmbiguousMatchException)
			{
				while (result == null && componentType != null)
				{
					result = componentType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
					componentType = componentType.BaseType;
				}
			}

			return result;
		}

		#endregion

		#region Class implement interface with SupportGridColour attribute

		public void TestClassImplementInterfaceWithSupportGridColourAttribute()
		{
			var bizo = GetNewBusinessObject();

			// get all interfaces of the bizo that has the SupportGridColourAttribute
			ICollection<string> ZPropertiesImplementedExplicitly(Type i)
				=> i.GetProperties()
					.Where(p => typeof(IZType).IsAssignableFrom(p.PropertyType) && !bizo.TryGetPropertyValueByName(p.Name, out _))
					.Select(p => p.Name)
					.ToList();

			var violations = bizo.GetType()
				.GetInterfaces()
				.Where(i => Attribute.IsDefined(i, typeof(SupportGridColourAttribute)))
				.Select(i => (ClassName: i.Name, Properties: ZPropertiesImplementedExplicitly(i)))
				.Where(kvp => kvp.Properties.Count > 0)
				.OrderBy(kvp => kvp.ClassName)
				.ToList();

			var message = new StringBuilder();
			message.AppendLine($"Business object class that implements interface with attribute '{nameof(SupportGridColourAttribute)}' must implement all its {nameof(IZType)} property publicly, eg. public ZString Foo.");
			message.AppendLine($"Please correct '{bizo.GetType().Name}' class as it implements the following interface without implementing all of the property publicly.");
			foreach (var violation in violations)
			{
				message.AppendLine($"Interface: {violation.ClassName}");
				message.AppendLine($"Property: {string.Join(", ", violation.Properties)}");
			}

			Assert(message.ToString(), !violations.Any());
		}

		#endregion

		#region Sparse columns should not be set default value in base

		public void TestSparseColumnsAreEmptyAfterSetDefaultValues()
		{
			var message = ZString.Empty;
			var bizo = GetNewBusinessObject();

			if (!(bizo is NonPersistentBusinessObject))
			{
				message = GenericTestHelper.AssertSparseColumnsAreEmptyAfterSetDefaultValues(TestConnection, bizo, GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues());
			}

			if (message.IsEmpty)
			{
				Assert(true);
			}
			else
			{
				Assert(message, false);
			}
		}

		public virtual List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>();
		}

		#endregion

		#region Sensitive Columns and Password Attributes

		public void TestSensitiveColumnsAndPasswordAttributesMatch()
		{
			var message = ZString.Empty;
			var bizo = GetNewBusinessObject();
			if (!(bizo is NonPersistentBusinessObject))
			{
				var sensitiveColumns = SensitiveColumnsCache.Instance.SensitiveColumns;

				var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(bizo.TableName);
				if (schema != null)
				{
					foreach (var column in schema.All)
					{
						var isSensitive = sensitiveColumns.Contains(column.Name);
						var hasPasswordAttribute = false;
						var propertyInfo = ExpectedBusinessObjectType.GetProperty(column.Name, BindingFlags.Instance | BindingFlags.Public);
						if (propertyInfo != null)
						{
							hasPasswordAttribute = propertyInfo.GetCustomAttribute<PasswordAttribute>(true) != null;
						}
						if (isSensitive && !hasPasswordAttribute)
						{
							message += string.Format("{0} is sensitive, but is missing [Password].", column.Name) + Environment.NewLine;
						}
						else if (!isSensitive && hasPasswordAttribute)
						{
							message += string.Format("{0} has [Password], but isn't marked as sensitive in the database.", column.Name) + Environment.NewLine;
						}
					}
				}
			}

			if (message.IsEmpty)
			{
				Assert(true);
			}
			else
			{
				Assert(message, false);
			}
		}

		#endregion

		#region Implementation

		public Type ExpectedBusinessObjectType
		{
			get { return GetExpectedBusinessObjectType(); }
		}

		protected virtual IZType GetValueFromBusinessObject(BusinessObject bizO, string name)
		{
			return (IZType)bizO[name];
		}

		protected virtual void SetValueToBusinessObject(BusinessObject bizO, string name, IZType value)
		{
			bizO[name] = value;
		}

		protected Type GetExpectedBusinessObjectType() =>
			TestedTypeHelper.GetTestedType(GetType());

		protected abstract BusinessObject GetNewBusinessObject();

		protected virtual BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var type = GetExpectedBusinessObjectType();
			if (BusinessObjectFactory.HasTableName(type))
			{
				return factory.NewWithValidTestData(type);
			}
			else
			{
				return (BusinessObject)Activator.CreateInstance(type);
			}
		}

		protected string NewLine
		{
			get { return System.Environment.NewLine; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			fCachedBusinessObject = null;
		}

		protected BusinessObject CachedBusinessObject
		{
			get
			{
				if (fCachedBusinessObject == null)
				{
					fCachedBusinessObject = GetNewBusinessObject();
				}

				return fCachedBusinessObject;
			}
		}

		/// <summary>
		/// Find the most specific concrete ancestor of startingType or scopeCap, whichever is more specific.
		/// A type is considered concrete if it is not abstract.
		/// </summary>
		/// <param name="startingType">The starting type, this type will not be concidered unless startingType == scopeCap, but it's ancestors will always be concidered.</param>
		/// <param name="scopeCap">The least specific type we are interested in, if this type is reached without finding a concrete ancestor then this is returned.</param>
		/// <returns>The most specific concrete ancestor of startingType or scopeCap if no concrete ancestors were found before reaching scopeCap.</returns>
		public static Type NearestConcreteAncestor(Type startingType, Type scopeCap)
		{
			if (startingType == null)
			{
				throw new ArgumentNullException(nameof(startingType));
			}

			if (scopeCap == null)
			{
				scopeCap = typeof(object);
			}
			else if (!scopeCap.IsAssignableFrom(startingType))
			{
				throw new InvalidOperationException(string.Format("startingType ({0}) must inherit from scopeCap ({1})", startingType.FullName, scopeCap.FullName));
			}

			for (Type t = startingType.BaseType; scopeCap.IsAssignableFrom(t); t = t.BaseType)
			{
				if (!t.IsAbstract)
				{
					return t;
				}
			}

			return scopeCap;
		}

		#region Assert ZType Default Behaviours

		protected void AssertZBlobDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = ZBlob.FromAscii(" ");
			owner[propertyName] = ZBlob.Empty;
		}

		protected void AssertZBoolDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = ZBool.True;
			AssertEquals("Valid Bool on " + propertyName, ZBool.True, owner[propertyName]);
			owner[propertyName] = ZBool.False;
			AssertEquals("Valid Bool on " + propertyName, ZBool.False, owner[propertyName]);
		}

		protected void AssertZByteDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			ZByte testByte = 2;
			owner[propertyName] = testByte;
			AssertEquals("Valid Byte on " + propertyName, testByte, (ZByte)owner[propertyName]);
			testByte = 3;
			owner[propertyName] = testByte;
			testByte = 4;
			owner[propertyName] = testByte;
			testByte = 0;
			owner[propertyName] = testByte;
			testByte = 0;
			owner[propertyName] = testByte;
		}

		protected void AssertZDateTimeDefaultBehaviour(BusinessObject owner, ZPropertyInfo info)
		{
			var propertyName = info.Name;
			var indexForMessage = 0;

			var attributeTypeToTestValuesMap = new List<(Type AttributeType, Func<string, (ZDateTime AssignValue, ZDateTime AssertValue)[]> GetValuesFunc)>
				{
					(typeof(ZDateTimeDurationValueExclude1900Attribute), GetValidZDateTimeDurationValueExclude1900Values),
					(typeof(ZDateTimeDurationValueCalculatedFromMinutesAttribute), GetValidZDateTimeDurationValueCalculatedFromMinutesValues),
					(typeof(ZDateTimeDurationValueCalculatedFromSecondsAttribute), GetValidZDateTimeDurationValueCalculatedFromSecondsValues),
					(typeof(ZDateTimeDurationCalculatedOnEmptyAttribute), GetValidZDateTimeDurationCalculatedOnEmptyValues),
					(typeof(ZDateTimeOffsetValueNegatableAttribute), GetValidZDateTimeOffsetValueNegativeableValues),
					(typeof(ZDateTimeDurationValueAttribute), GetValidZDateTimeDurationValues),
				};

			var testValues = attributeTypeToTestValuesMap
				.Where(kvp => info.PropertyDescriptor.Attributes[kvp.AttributeType] != null)
				.Select(kvp => kvp.GetValuesFunc(propertyName))
				.FirstOrDefault() ?? GetValidZDateTimes(propertyName);

			foreach (var dateTime in testValues)
			{
				owner[propertyName] = dateTime.AssignValue;
				AssertEquals("Testing ZDateTime " + indexForMessage++ + " for property: " + propertyName, dateTime.AssertValue, (ZDateTime)owner[propertyName]);
			}
		}

		public virtual (ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimes(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(2003, 11, 21), new ZDateTime(2003, 11, 21)),
				(ZDateTime.Invalid, ZDateTime.Invalid),
				(ZDateTime.MaxSmallDateTime, ZDateTime.MaxSmallDateTime),
				(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
			};
		}

		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeDurationValues(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(1900, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 15, 12, 0, 0)),
				(new ZDateTime(1900, 12, 31, 23, 59, 00), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 00)),

				(new ZDateTime(1901, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(2003, 11, 21, 8, 30, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21, 8, 30, 0)),
				(new ZDateTime(1999, 7, 4, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 7, 4, 12, 0, 0)),
				(new ZDateTime(2050, 12, 25, 18, 45, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 25, 18, 45, 0)),

				(ZDateTime.Invalid, ZDateTime.Invalid),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(DateTime.MaxValue.AddYears(ZDateTime.DefaultDurationEpoch.Year - DateTime.MaxValue.Year))),
				(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
				(ZDateTime.Empty, ZDateTime.Empty),
			};
		}

		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeDurationValueExclude1900Values(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.Empty),
				(new ZDateTime(1900, 1, 2, 0, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 1, 2, 0, 0, 0)),
				(new ZDateTime(1900, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 15, 12, 0, 0)),
				(new ZDateTime(1900, 12, 31, 23, 59, 00), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 00)),

				(new ZDateTime(2000, 1, 1, 0, 0, 0), ZDateTime.Empty),
				(new ZDateTime(2003, 11, 21, 8, 30, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21, 8, 30, 0)),
				(new ZDateTime(1999, 7, 4, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 7, 4, 12, 0, 0)),
				(new ZDateTime(2050, 12, 25, 18, 45, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 25, 18, 45, 0)),

				(ZDateTime.Invalid, ZDateTime.Invalid),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(DateTime.MaxValue.AddYears(ZDateTime.DefaultDurationEpoch.Year - DateTime.MaxValue.Year))),
				(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
				(ZDateTime.Empty, ZDateTime.Empty),
			};
		}

		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeDurationValueCalculatedFromMinutesValues(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(1900, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 15, 12, 0, 0)),
				(new ZDateTime(1900, 12, 31, 23, 59, 00), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 00)),

				(new ZDateTime(1901, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(2003, 11, 21, 8, 30, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21, 8, 30, 0)),
				(new ZDateTime(1999, 7, 4, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 7, 4, 12, 0, 0)),
				(new ZDateTime(2050, 12, 25, 18, 45, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 25, 18, 45, 0)),

				(ZDateTime.Invalid, ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(new DateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 0, 0))),
				(new ZDateTime(DateTime.MinValue), ZDateTime.DefaultDurationEpoch),
				(ZDateTime.Empty, ZDateTime.DefaultDurationEpoch),
			};
		}
		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeDurationValueCalculatedFromSecondsValues(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(1900, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 15, 12, 0, 0)),
				(new ZDateTime(1900, 12, 31, 23, 59, 59), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 59)),

				(new ZDateTime(1901, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(2003, 11, 21, 8, 30, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21, 8, 30, 0)),
				(new ZDateTime(1999, 7, 4, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 7, 4, 12, 0, 0)),
				(new ZDateTime(2050, 12, 25, 18, 45, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 25, 18, 45, 0)),

				(ZDateTime.Invalid, ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(new DateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 59, 000))),
				(new ZDateTime(DateTime.MinValue), ZDateTime.DefaultDurationEpoch),
				(ZDateTime.Empty, ZDateTime.DefaultDurationEpoch),
			};
		}

		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeDurationCalculatedOnEmptyValues(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(1900, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 6, 15, 12, 0, 0)),
				(new ZDateTime(1900, 12, 31, 23, 59, 00), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 31, 23, 59, 00)),

				(new ZDateTime(1901, 1, 1, 0, 0, 0), ZDateTime.DefaultDurationEpoch),
				(new ZDateTime(2003, 11, 21, 8, 30, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 11, 21, 8, 30, 0)),
				(new ZDateTime(1999, 7, 4, 12, 0, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 7, 4, 12, 0, 0)),
				(new ZDateTime(2050, 12, 25, 18, 45, 0), new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 12, 25, 18, 45, 0)),

				(ZDateTime.Invalid, ZDateTime.Invalid),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(DateTime.MaxValue.AddYears(ZDateTime.DefaultDurationEpoch.Year - DateTime.MaxValue.Year))),
				(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
				(ZDateTime.Empty, new ZDateTime(ZDateTime.DefaultDurationEpoch.Year, 1, 1, 23, 59, 59)),
			};
		}

		(ZDateTime AssignValue, ZDateTime AssertValue)[] GetValidZDateTimeOffsetValueNegativeableValues(string propertyName)
		{
			return new (ZDateTime AssignValue, ZDateTime AssertValue)[]
			{
				(new ZDateTime(2000, 1, 1, 0, 0, 0), ZDateTime.DefaultNegatableDurationEpoch),
				(new ZDateTime(2000, 5, 31, 0, 0, 0), new ZDateTime(ZDateTime.DefaultNegatableDurationEpoch.Year, 5, 31, 0, 0, 0)),
				(new ZDateTime(1999, 6, 15, 12, 0, 0), new ZDateTime(ZDateTime.DefaultNegatableDurationEpoch.Year - 1, 6, 15, 12, 0, 0)),
				(new ZDateTime(1999, 12, 31, 23, 59, 00), new ZDateTime(ZDateTime.DefaultNegatableDurationEpoch.Year - 1, 12, 31, 23, 59, 00)),

				(new ZDateTime(1900, 1, 1, 0, 0, 0), ZDateTime.DefaultNegatableDurationEpoch),
				(new ZDateTime(2001, 1, 1, 0, 0, 0), ZDateTime.DefaultNegatableDurationEpoch),

				(ZDateTime.Invalid, ZDateTime.Invalid),
				(new ZDateTime(DateTime.MaxValue), new ZDateTime(DateTime.MaxValue.AddYears(ZDateTime.DefaultNegatableDurationEpoch.Year - DateTime.MaxValue.Year - 1))),
				(new ZDateTime(DateTime.MinValue), new ZDateTime(DateTime.MinValue)),
				(ZDateTime.Empty, ZDateTime.Empty),
			};
		}

		protected void AssertZDateTimeOffsetDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			ZDateTimeOffset validDateTimeOffset = new ZDateTimeOffset(2003, 11, 21, 1, 2, 3, TimeSpan.FromHours(11));
			owner[propertyName] = validDateTimeOffset;
			AssertEquals("Valid ZDateTimeOffset on " + propertyName, validDateTimeOffset, (ZDateTimeOffset)owner[propertyName]);

			ZDateTimeOffset invalidDateTimeOffset = ZDateTimeOffset.Invalid;
			owner[propertyName] = invalidDateTimeOffset;
			AssertEquals("Valid ZDateTimeOffset on " + propertyName, invalidDateTimeOffset, (ZDateTimeOffset)owner[propertyName]);

			ZDateTimeOffset minDateTimeOffset = new ZDateTimeOffset(DateTime.MinValue, TimeSpan.FromHours(0));
			owner[propertyName] = minDateTimeOffset;
			AssertEquals("Valid ZDateTime on " + propertyName, minDateTimeOffset, (ZDateTimeOffset)owner[propertyName]);
			owner[propertyName] = new ZDateTimeOffset(2003, 11, 21, 1, 2, 3, TimeSpan.FromHours(11));
			owner[propertyName] = new ZDateTimeOffset(2003, 11, 22, 4, 5, 6, TimeSpan.FromHours(11));
			owner[propertyName] = ZDateTimeOffset.Empty;
			owner[propertyName] = ZDateTimeOffset.Empty;
		}

		protected void AssertZDecimalDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = new ZDecimal(1m);

			// TODO: A few bizos fail this, so we can't have this test until they are resolved:
			//AssertEquals("Valid ZDecimal on " + PropertyName, 1m, (ZDecimal)Owner[PropertyName]);

			owner[propertyName] = new ZDecimal(2m);
			owner[propertyName] = new ZDecimal(0m);
			owner[propertyName] = new ZDecimal(0m);
		}

		protected void AssertZGeographyDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = new ZGeography("-121 48");
			AssertEquals(new ZGeography("-121 48"), owner[propertyName]);
			owner[propertyName] = ZGeography.Empty;
			AssertEquals(ZGeography.Empty, owner[propertyName]);
			owner[propertyName] = ZGeography.Invalid;
			AssertEquals(ZGeography.Invalid, owner[propertyName]);
			owner[propertyName] = new ZGeography("-121 44");
			AssertEquals(new ZGeography("-121 44"), owner[propertyName]);
		}

		protected void AssertZGuidDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			ZGuid originalValue = (ZGuid)owner[propertyName];
			try
			{
				owner[propertyName] = ZGuid.NewZGuid();
				owner[propertyName] = ZGuid.NewZGuid();
				owner[propertyName] = ZGuid.Empty;
				owner[propertyName] = ZGuid.Empty;
				owner[propertyName] = ZGuid.Invalid;
				owner[propertyName] = ZGuid.Invalid;
			}
			finally
			{
				owner[propertyName] = originalValue;
			}
		}

		protected void AssertZIntDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = new ZInt(1);
			AssertEquals("Valid ZInt on " + propertyName, 1, (ZInt)owner[propertyName]);
			owner[propertyName] = new ZInt(2);
			owner[propertyName] = new ZInt(0);
			owner[propertyName] = new ZInt(0);
		}

		protected void AssertZShortDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			owner[propertyName] = (short)1;
			AssertEquals("Valid ZShort on " + propertyName, (short)1, (ZShort)owner[propertyName]);
			owner[propertyName] = (short)2;
			owner[propertyName] = (short)0;
			owner[propertyName] = (short)0;
		}

		protected virtual bool CanTestMaxLength(int maxStringLength)
		{
			return maxStringLength > 0 && maxStringLength < MaximumLengthOfMultiByteTextFieldInSQL2000;
		}

		protected void AssertZStringDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			ErrorReporter.Clear();
			ZPropertyInfo property = owner.ZPropertyInfoHash[propertyName];
			if (property.PropertyDescriptor.Attributes[typeof(BusinessObjectMaxLengthTestExcludeAttribute)] == null)
			{
				int maxStringLength = property.MaxLength;

				bool canTestExceedMaxLength = true;
				if (!CanTestMaxLength(maxStringLength))
				{
					maxStringLength = 1024;
					canTestExceedMaxLength = false;
				}

				ZString maxLengthString = null;
				try
				{
					maxLengthString = ZString.Replicate('A', maxStringLength);
				}
				catch (OutOfMemoryException e)
				{
					Fail("Creating string for ZProperty " + propertyName + " with MaxLength " + maxStringLength + ":" + NewLine + e);
				}

				bool readOnlyException = false;
				try
				{
					owner[propertyName] = maxLengthString;
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException != null && ex.InnerException is System.Data.ReadOnlyException)
					{
						readOnlyException = true;
						// for bizos generated from views
					}
					else
					{
						throw;
					}
				}

				if (!readOnlyException)
				{
					if (maxStringLength > 0)
					{
						ZString testResult = (ZString)owner[propertyName];

						AssertEquals("Failed to set Value for Property " + propertyName, maxLengthString, testResult);

						if (canTestExceedMaxLength && CanTestMaxLength(maxStringLength))
						{
							ZString overMaxLengthString = maxLengthString + 'A';

							AssertEquals("No errors reported before", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
							try
							{
								owner[propertyName] = overMaxLengthString;
								owner[propertyName] = overMaxLengthString;
							}
							catch (TargetInvocationException ex1)
							{
								Exception currentException = ex1;
								while (currentException is TargetInvocationException)
								{
									currentException = currentException.InnerException;
								}
								if (currentException is MaxLengthExceededException)
								{
									ErrorReporter.Clear();
								}
								else
								{
									throw currentException;
								}
							}

							testResult = (ZString)owner[propertyName];

							string message = string.Format(
								"We were able to set the property <b>{0}</b> to a value that exceeds the MaxLength of {1}." + NewLine + NewLine +
								"If this is a calculated property, your set method should test for MaxLength, for example:" + NewLine + NewLine +
								"<pre>" +
								"<code>" +
								"	public ZString {0}" + NewLine +
								"	{{" + NewLine +
								"		get {{ return f{0}; }}" + NewLine +
								"		set" + NewLine +
								"		{{" + NewLine +
								"			if (f{0} != value)" + NewLine +
								"			{{" + NewLine +
								"				<b>CheckMaximumLength({0}Info, <font color=\"Blue\">value</font>);</b> <font color=\"Green\">// you are likely missing this line!</font>" + NewLine +
								"				f{0} = value;" + NewLine +
								"			}}" + NewLine +
								"		}}" + NewLine +
								"	}}" +
								"</code>" +
								"</pre>",
								propertyName, maxStringLength.ToString());

							HtmlAssert(message, testResult.Length <= maxStringLength);
							HtmlAssertEquals(message, maxLengthString, testResult);
						}
					}

					owner[propertyName] = ZString.Empty;

					if (property.PropertyDescriptor.Attributes[typeof(BusinessObjectEmptyStringTestExcludeAttribute)] == null && maxStringLength > 0)
					{
						AssertEquals("Failed to set Value for Property " + propertyName + " for Empty String", ZString.Empty, (ZString)owner[propertyName]);
					}
				}
			}

			// Checking to see exception is not thrown for duplicate error
			try
			{
				owner[propertyName] = "A";
				owner[propertyName] = "B";
			}
			catch (TargetInvocationException ex)
			{
				if (ex.InnerException != null && ex.InnerException is System.Data.ReadOnlyException)
				{
					// for bizos generated from views
				}
				else
				{
					throw;
				}
			}
		}

		protected void AssertZTimeDefaultBehaviour(BusinessObject owner, string propertyName)
		{
			var indexForMessage = 0;
			foreach (var time in GetValidZTimes(propertyName))
			{
				owner[propertyName] = time.Value;
				if (time.ShouldAssert)
				{
					AssertEquals("Testing ZTime " + indexForMessage++ + " for property: " + propertyName, time.Value, (ZTime)owner[propertyName]);
				}
			}
		}
		public virtual (ZTime Value, bool ShouldAssert)[] GetValidZTimes(string propertyName)
		{
			return new (ZTime Value, bool ShouldAssert)[]
			{
				(ZTime.Invalid, true),
				(new ZTime(DateTime.MinValue), false),
				(new ZTime(12, 34), false),
				(new ZTime(12, 34), false),
				(ZTime.Empty, false),
				(new ZTime(), false)
			};
		}

		protected const int MaximumLengthOfMultiByteTextFieldInSQL2000 = 1073741823;
		protected StringCollection fExcludedFields;
		BusinessObject fCachedBusinessObject;

		#endregion

		#region AssertEquals for ZDecimal

		/// <summary>
		/// Asserts the Expected ZDecimal and Actual ZDecimal within a specified tolerance
		/// </summary>
		/// <param name="errorMessage">Error Message to display</param>
		/// <param name="expected">Expected Value</param>
		/// <param name="actual">Actual Value</param>
		/// <param name="tolerance">the Acceptable Tolerance</param>
		public static void AssertZDecimalEquals(string errorMessage, ZDecimal expected, ZDecimal actual, ZDecimal tolerance)
		{
			ZDecimal difference = expected - actual;
			errorMessage += "\nExpected: " + expected + " but was: " + actual;
			Assert(errorMessage, Math.Abs(difference) < tolerance);
		}

		/// <summary>
		/// Asserts the Expected ZDecimal and Actual ZDecimal within the default tolerance
		/// </summary>
		/// <param name="errorMessage">Error Message to display</param>
		/// <param name="expected">Expected Value</param>
		/// <param name="actual">Actual Value</param>
		public static void AssertZDecimalEquals(string errorMessage, ZDecimal expected, ZDecimal actual)
		{
			AssertZDecimalEquals(errorMessage, expected, actual, MinimumCareResult);
		}

		protected static ZDecimal MinimumCareResult = 0.00001m;

		#endregion

		#region Reflection Utility Methods

		protected IList GetBusinessEvents()
		{
			ArrayList result = new ArrayList();
			Type bizType = CachedBusinessObject.GetType();

			while (bizType != null)
			{
				result.AddRange(bizType.GetEvents());
				bizType = bizType.BaseType;
			}

			return result;
		}

		protected bool HasBusinessEvent(string name)
		{
			bool result = false;

			foreach (EventInfo @event in GetBusinessEvents())
			{
				if (@event.Name == name)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		#endregion

		#endregion
	}
}
