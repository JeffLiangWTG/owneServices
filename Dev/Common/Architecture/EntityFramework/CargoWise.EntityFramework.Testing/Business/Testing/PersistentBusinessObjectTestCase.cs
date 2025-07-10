using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	public abstract class PersistentBusinessObjectTestCase : BusinessObjectBaseTestCase
	{
		[DeveloperOnlyTest]
		public virtual void TestFetchForLoad()
		{
			var bizObj = GetBusinessObjectForFetchForLoad();
			bizObj.Factory.Save();
			var newFactory = NewFactory();
			GetBusinessObjectInNewFactory(bizObj, newFactory);

			var fetchedTables = new ZStringBuilder(newFactory.GetAllFetchHintedTableNames()).ToStringWithDelimiterBetweenAppends(",");
			AssertEquals(@"** DEVELOPER-ONLY TEST **
Fetch hints that are located in FetchForLoad should always be consumed prior to the completed construction of the business object. 
Any fetch hint not following this pattern should be moved to either FetchForValidate or FetchForView or even a manual fetch hint in a particular business scenario.", string.Empty, fetchedTables);
		}

		protected virtual BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Factory.NewWithValidTestData(GetExpectedBusinessObjectType(), TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
		}

		protected virtual BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			return newFactory.Load(bizObj.GetType(), bizObj.PK);
		}

		public void TestNoPropertiesHaveOverridenPropertiesOverridingTheSetterButNotTheGetter()
		{
			List<PropertyInfo> propertyInfosWithProblems = new List<PropertyInfo>();
			Type boType = GetExpectedBusinessObjectType();
			foreach (PropertyInfo propertyInfo in boType.GetProperties())
			{
				if (propertyInfo.DeclaringType == boType && (typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) || typeof(IBusiness).IsAssignableFrom(propertyInfo.PropertyType)))
				{
					MethodInfo getMethod = propertyInfo.GetGetMethod();
					if (getMethod == null)
					{
						Type baseType = boType.BaseType;
						if (baseType != null)
						{
							PropertyInfo basePropertyInfo = baseType.GetProperty(propertyInfo.Name);
							if (basePropertyInfo != null)
							{
								MethodInfo baseGetMethod = basePropertyInfo.GetGetMethod();
								if (baseGetMethod != null)
								{
									propertyInfosWithProblems.Add(propertyInfo);
								}
							}
						}
					}
				}
			}
			if (propertyInfosWithProblems.Count == 0)
			{
				Assert("Everything is Waaaay cool.", true);
			}
			else
			{
				ZStringBuilder problems = new ZStringBuilder();
				foreach (PropertyInfo propertyInfo in propertyInfosWithProblems)
				{
					problems.Append("override " + propertyInfo.PropertyType.Name + " " + propertyInfo.Name);
				}
				Fail("In: " + boType.FullName + "\r\n"
						+ "The following overriden properties override the setter without implementing the getter on properties where the base DOES implement a getter:\r\n\r\n"
						+ problems.ToStringWithNewLineBetweenAppends()
						+ "\r\n\r\nThis causes problems when using a watch to debug these properties, or when trying to reflect out properties on the object.\r\n");
			}
		}

		public void TestConcurrencyPolicyDoesntChangeOnPropertyInfoGet()
		{
			var bizObj = GetNewBusinessObject();
			foreach (var column in bizObj.Row.Table.Columns.Cast<DataColumn>())
			{
				var concurrencyPolicyBefore = ConcurrencyInfo.Get(bizObj.Row, column);
				var prop = GetExpectedBusinessObjectType().GetProperty(column.ColumnName + "Info", BindingFlags.Public | BindingFlags.Instance);
				if (prop == null)
				{
					continue;
				}
				_ = prop.GetValue(bizObj);
				var concurrencyPolicyAfter = ConcurrencyInfo.Get(bizObj.Row, column);
				AssertEquals(column.ColumnName, concurrencyPolicyBefore, concurrencyPolicyAfter);
			}
		}

		public void TestDontReusePropertyInfosFromOtherBusinessObjects()
		{
			BusinessObject bizObj = GetNewBusinessObject();
			Dictionary<PropertyInfo, Exception> infosThrowingExceptions = new Dictionary<PropertyInfo, Exception>();
			Dictionary<PropertyInfo, ZPropertyInfo> infosWithBrokenNames = new Dictionary<PropertyInfo, ZPropertyInfo>();
			Dictionary<PropertyInfo, ZPropertyInfo> infosFromOtherBizOs = new Dictionary<PropertyInfo, ZPropertyInfo>();

			foreach (PropertyInfo property in bizObj.GetType().GetProperties())
			{
				if (typeof(ZPropertyInfo).IsAssignableFrom(property.PropertyType))
				{
					ZPropertyInfo info;

					try
					{
						info = (ZPropertyInfo)property.GetValue(bizObj, Array.Empty<object>());
					}
					catch (TargetInvocationException ex)
					{
						infosThrowingExceptions.Add(property, ex.InnerException);
						continue;
					}

					if (info != null)
					{
						if (!object.ReferenceEquals(bizObj, info.BizObj))
						{
							infosFromOtherBizOs.Add(property, info);
						}
						else
						{
							bool propertyExists;

							try
							{
								propertyExists = (bizObj.GetType().GetProperty(info.Name) != null);
							}
							catch (AmbiguousMatchException)
							{
								propertyExists = true;
							}

							if (!propertyExists)
							{
								infosWithBrokenNames.Add(property, info);
							}
						}
					}
				}
			}

			StringBuilder builder = new StringBuilder();

			if (infosThrowingExceptions.Count > 0)
			{
				builder.Append("<br/><b>The following property info(s) threw exceptions.</b><br/>");

				foreach (KeyValuePair<PropertyInfo, Exception> pair in infosThrowingExceptions)
				{
					builder.AppendFormat("{0} - {1}<br/>", Html(pair.Key.Name), Html(pair.Value.Message));
				}
			}

			if (infosFromOtherBizOs.Count > 0)
			{
				builder.Append("<br/><b>The following property info(s) belong to other business objects, you should use a ZWrappedPropertyInfo or create a new ZPropertyInfo.</b><br/>");

				foreach (KeyValuePair<PropertyInfo, ZPropertyInfo> pair in infosFromOtherBizOs)
				{
					builder.AppendFormat("{0} - {1} ({2})<br>", Html(pair.Key.Name), Html(pair.Value.BizObj.GetType().FullName), Html(pair.Value.Name));
				}
			}

			if (infosWithBrokenNames.Count > 0)
			{
				builder.Append("<br/><b>The following property info(s) have names that do not refer to a valid property on the BusinessObject. Note, it may be that the properties in question are not public.</b><br/>");

				foreach (KeyValuePair<PropertyInfo, ZPropertyInfo> pair in infosWithBrokenNames)
				{
					builder.AppendFormat("{0} - {1}<br/>", Html(pair.Key.Name), Html(pair.Value.Name));
				}
			}

			HtmlAssert(builder.ToString(), builder.Length == 0);
		}

		public void TestWrappedPropertyInfoBinding()
		{
			BusinessObject bO = GetNewBusinessObject();
			StringBuilder wrappedPropertyFailedInfos = new StringBuilder();

			foreach (PropertyDescriptor property in bO.GetProperties())
			{
				if (typeof(ZWrappedPropertyInfo).IsAssignableFrom(property.PropertyType))
				{
					ZWrappedPropertyInfo wrappedPropertyInfo = (ZWrappedPropertyInfo)property.GetValue(bO);
					if (wrappedPropertyInfo != null && wrappedPropertyInfo.HasSetter)
					{
						bO.isRefreshed_Debug = false;
						wrappedPropertyInfo.InnerInfo.RefreshBinding();
						if (!bO.isRefreshed_Debug)
						{
							wrappedPropertyFailedInfos.Append(string.Format("{0} which is bound to {1}.{2} needs 'RegisterListChangedCalledRefreshBinding({1});' to be added to the instantiation of {1} and unhook it ('UnRegisterListChangedCalledRefreshBinding({1});') were appropriate.",
								System.Environment.NewLine + wrappedPropertyInfo.Name, wrappedPropertyInfo.InnerInfo.BizObj.GetType().FullName, wrappedPropertyInfo.InnerInfo.Name));
						}
					}
				}
			}
			Assert("The following WrappedPropertyInfos are not properly written and will cause refreshing issue for any GUI bound to it:" + wrappedPropertyFailedInfos.ToString(), wrappedPropertyFailedInfos.Length == 0);
		}

		public void TestChildEditableAttribute_AppliedCorrectlyOnProperties()
		{
			var expectedChildEditable = new List<PropertyDescriptor>();
			var expectedNotChildEditable = new List<PropertyDescriptor>();

			foreach (PropertyDescriptor property in ZCustomTypeDescriptor.GetProperties(BusinessObject.GetType()))
			{
				if (!property.GetAttributesAllowMultiple(typeof(BusinessObjectTestExclude)).Any() &&
					!property.GetAttributesAllowMultiple(typeof(ChildEditableTestExclude)).Any())
				{
					bool expectedHasAttribute = false;

					if (typeof(IBusinessObjectCollection).IsAssignableFrom(property.PropertyType))
					{
						IBusiness childEditable = property.GetValue(BusinessObject) as IBusiness;

						IList<IBusiness> children = ((IBusiness)BusinessObject).Children;

						expectedHasAttribute = children.Contains(childEditable) || ContainsActiveBusinessObjectCollectionIndex(children, childEditable as IActiveBusinessObjectCollection);
					}

					bool actualHasAttribute = property.GetAttributesAllowMultiple(typeof(ChildEditableAttribute)).Any();

					if (expectedHasAttribute && !actualHasAttribute)
					{
						expectedChildEditable.Add(property);
					}
					else if (!expectedHasAttribute && actualHasAttribute)
					{
						expectedNotChildEditable.Add(property);
					}
				}
			}

			ReportChildEditableAttributeFailures(expectedChildEditable, expectedNotChildEditable);
		}

		public void TestChildEditableAttribute_AppliedOnlyToIBusiness()
		{
			var expectedNotChildEditable = new List<PropertyDescriptor>();

			foreach (PropertyDescriptor property in ZCustomTypeDescriptor.GetProperties(BusinessObject.GetType()))
			{
				if (!typeof(IBusiness).IsAssignableFrom(property.PropertyType) &&
					!typeof(ICollection).IsAssignableFrom(property.PropertyType) &&
					property.GetAttributesAllowMultiple(typeof(ChildEditableAttribute)).Any())
				{
					expectedNotChildEditable.Add(property);
				}
			}

			ReportChildEditableAttributeFailures(Array.Empty<PropertyDescriptor>(), expectedNotChildEditable);
		}

		bool ContainsActiveBusinessObjectCollectionIndex(IEnumerable<IBusiness> items, IActiveBusinessObjectCollection collection)
		{
			if (collection != null)
			{
				foreach (IBusiness item in items)
				{
					IActiveBusinessObjectCollection currentCollection = item as IActiveBusinessObjectCollection;
					if (currentCollection != null)
					{
						if (currentCollection.Index == collection.Index)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		void ReportChildEditableAttributeFailures(ICollection<PropertyDescriptor> expectedChildEditable, ICollection<PropertyDescriptor> expectedNotChildEditable)
		{
			if (expectedChildEditable.Count > 0 || expectedNotChildEditable.Count > 0)
			{
				string message = "";
				if (expectedChildEditable.Count > 0)
				{
					message += "The following properties must have ChildEditableAttribute applied:\r\n";
					foreach (PropertyDescriptor property in expectedChildEditable)
					{
						message += property.Name + "\r\n";
					}
				}
				message += "\r\n";

				if (expectedNotChildEditable.Count > 0)
				{
					message += "The following properties must not have ChildEditableAttribute applied:\r\n";
					foreach (PropertyDescriptor property in expectedNotChildEditable)
					{
						message += property.Name + "\r\n";
					}
				}
				Fail(message);
			}
			else
			{
				Assert("No failures reported", true);
			}
		}

		[ExpectNoExceptions()]
		public void TestLightValidation()
		{
			if (GetNewBusinessObject().LightValidationEnabled) // check GetNewBusinessObject as may not support GetNewBusinessObjectForDeleteTest
			{
				TestLightValidation(GetNewBusinessObjectForDefaultLightValidationTest());
			}
			// to test different configurations (eg, Import vs Export in customs), make a new test, setup your objects and then call TestLightValidation(myObj)
		}

		protected void TestLightValidation(BusinessObject bizObjToTest)
		{
			if (bizObjToTest.LightValidationEnabled)
			{
				GetNewLightValidationTester(bizObjToTest).Test();
			}
			Assert("All good", true);
		}

		protected virtual LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new LightValidationTester(bizObjToTest);
		}

		protected virtual BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		void AssertCloneAuditProperties(string auditProp1, string auditProp2)
		{
			if (!string.IsNullOrEmpty(BusinessObject.TableName))
			{
				ReleaseFactory();
				string property1 = BizObjTablePrefix + auditProp1;
				string property2 = BizObjTablePrefix + auditProp2;
				BusinessObject bizObj = GetNewBusinessObjectForDeleteTest(new BusinessObjectFactory());
				if (bizObj.ZPropertyInfoHash.ContainsKey(property1) && bizObj.ZPropertyInfoHash.ContainsKey(property2) && bizObj.SupportsClone() && bizObj.IsSavedByFactory)
				{
					bizObj.Factory.Save();

					AssertEquals(property1 + ".IsEmpty", false, ((IZType)bizObj[property1]).IsEmpty);
					AssertEquals(property2 + ".IsEmpty", false, ((IZType)bizObj[property2]).IsEmpty);
					BusinessObject clone = bizObj.Clone();
					AssertEquals(property1 + ".IsEmpty", true, ((IZType)clone[property1]).IsEmpty);
					AssertEquals(property2 + ".IsEmpty", true, ((IZType)clone[property2]).IsEmpty);
				}
				else
				{
					Assert(true);
				}
			}
			else
			{
				Assert(true);
			}
			ErrorReporter.Clear();
		}

		string BizObjTablePrefix
		{
			get { return BusinessObjectFactory.GetTableCodeFromType(GetExpectedBusinessObjectType()) + "_"; }
		}

		public virtual void TestCloneAuditProperties()
		{
			AssertCloneAuditProperties("SystemCreateUser", "SystemCreateTimeUtc");
		}

		public virtual void TestCloneAuditContextProperties()
		{
			AssertCloneAuditProperties("SystemCreateBranch", "SystemCreateDepartment");
		}

		public virtual void TestReadonlyDuplication()
		{
			List<PropertyInfo> duplicates = new List<PropertyInfo>();
			Type type = GetExpectedBusinessObjectType();

			foreach (PropertyInfo propertyInfo in type.GetProperties())
			{
				if (HasDuplicateReadonlySpecification(type, propertyInfo))
				{
					duplicates.Add(propertyInfo);
				}
			}

			if (duplicates.Count == 0)
			{
				Assert("No duplicate readonly specifications found.", true);
			}
			else
			{
				ZStringBuilder problems = new ZStringBuilder();
				foreach (PropertyInfo propertyInfo in duplicates)
				{
					problems.Append(propertyInfo.Name);
				}

				Fail("In: " + type.FullName + "\r\n"
						+ "The following properties have both ReadOnly attribute applied and _ReadOnly property specified:\r\n\r\n"
						+ problems.ToStringWithNewLineBetweenAppends()
						+ "\r\n\r\nThis causes problems with bound UI controls. Leave only one way to specify whether property is read only.\r\n");
			}
		}

		bool HasDuplicateReadonlySpecification(Type type, PropertyInfo propertyInfo)
		{
			bool hasAttribute = propertyInfo.GetCustomAttributes(typeof(ReadOnlyAttribute), true).Length > 0;
			bool hasProperty = type.GetProperty(propertyInfo.Name + "_ReadOnly", BindingFlags.Instance | BindingFlags.Public) != null;
			return hasAttribute && hasProperty;
		}

		#region Code/Description property lookups fit in their fields

		[ExpectNoExceptions]
		[StressTest]
		public void TestCodeDescriptionPropertyLookupsDoNotExceedMaximumLength()
		{
			var typeOfObject = BusinessObject.GetType();
			var failureMessageBuilder = new ZStringBuilder();
			const BindingFlags propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance;
			var properties = typeOfObject.GetProperties(propertyBindingFlags).Where(x => !x.Name.EndsWith("Info"));

			foreach (var property in properties)
			{
				// Filter non-ZString properties.
				if (property.PropertyType != typeof(ZString))
				{
					continue;
				}
				// Filter unlimited properties and get the max length requirement.
				var propertyInfoProperty = typeOfObject.GetProperty(property.Name + "Info", propertyBindingFlags);
				if (propertyInfoProperty == null)
				{
					continue;
				}
				var propertyInfo = propertyInfoProperty.GetValue(BusinessObject) as ZPropertyInfo;
				if (propertyInfo == null || !propertyInfo.SupportsMaxLength || propertyInfo.MaxLength <= 0)
				{
					continue;
				}
				var propertyMaxLength = propertyInfo.MaxLength;
				// Filter the properties that do no have lookups.
				var lookupsAttribute = property.GetCustomAttribute<ListAttribute>(true);
				if (lookupsAttribute == null)
				{
					continue;
				}
				// Filter lookups that are not code description pairs.
				ICodeDescriptionPairList list;
				try
				{
					var propertyListDataSource = MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor);
					list = propertyListDataSource as ICodeDescriptionPairList;
					if (list == null)
					{
						continue;
					}
				}
				catch (NullReferenceException)
				{
					continue;
				}
				// Put the invalid code into the failure message.
				foreach (var code in list.Cast<ICodeDescription>().Select(cd => cd.Code).Where(c => c.Length > propertyMaxLength))
				{
					var propertyName = string.Format("{0}.{1}", typeOfObject.FullName, property.Name);
					// Exceptional case of code conversion
					if (typeOfObject.Name == "ClientInvoiceDelivery" && code == "<ALL>" && lookupsAttribute.ListDataSourceMember == "Lookups.ServerCodes")
					{
						continue;
					}
					// Exceptional case of code splitting
					if (propertyName.StartsWith("Enterprise.Customs.SG.V4.Business.JobComInvoiceLine.SG_CertOriginCriterion"))
					{
						var splittedCodes = code.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var splittedCode in splittedCodes)
						{
							if (splittedCode.Length > propertyMaxLength)
							{
								failureMessageBuilder.Append(String.Format("The maximum length {0} of the property {1} is exceeded by the splitted code '{2}' of '{3}' from [List(\"{4}\")]", propertyMaxLength, propertyName, splittedCode, code, lookupsAttribute.ListDataSourceMember));
							}
						}
						continue;
					}
					failureMessageBuilder.Append(String.Format("The maximum length {0} of the property {1} is exceeded by code '{2}' from [List(\"{3}\")]", propertyMaxLength, propertyName, code, lookupsAttribute.ListDataSourceMember));
				}
			}

			if (!failureMessageBuilder.IsEmpty)
			{
				Fail(failureMessageBuilder.ToStringWithNewLineBetweenAppends());
			}
		}

		#endregion

		#region Code Property Attribute is ZString

		[ExpectNoExceptions]
		public void TestCodePropertiesAreZStrings()
		{
			string propertyName = "";
			try
			{
				propertyName = CodePropertyAttribute.CodePropertyNameFromType(BusinessObject.GetType());
			}
			catch (NoCodePropertyException)
			{
				return;
			}

			if (!(BusinessObject[propertyName] is ZString))
			{
				string message =
					"The CodeProperty Attribute <b>{0}</b> is not a ZString. " +
					"If there is no unique ZString in your business object, create a calculated property which returns your unique code, for example: " +
					"<pre>" +
					"<code>" +
					"	<font color=\"Blue\">public</font> ZString UniqueCode " + NewLine +
					"	{{" + NewLine;

				if (BusinessObject[propertyName] is ZGuid)
				{
					message += "		<font color=\"Blue\">get</font>  {{ <font color=\"Blue\">return</font> RelatedBizO.XX_SomeZStringField; }} " + NewLine;
				}
				else
				{
					message += "		<font color=\"Blue\">get</font>  {{ <font color=\"Blue\">return</font> {0}.ToString(); }} " + NewLine;
				}

				message += "	}}" +
					"</code>" +
					"</pre>" + NewLine +
					"Add this new property to your schema:" + NewLine +
					"<pre>" +
					"<code>" +
					"	<font color=\"Blue\">public new abstract class</font> Schema : AutoBizO.Schema " + NewLine +
					"	{{" + NewLine +
					"		<font color=\"Blue\">public const string</font> UniqueCode = \"UniqueCode\";" + NewLine +
					"	}}" +
					"</code>" +
					"</pre>" + NewLine + NewLine;

				HtmlFail(string.Format(message, propertyName));
			}
		}

		#endregion

		#region Description Property Attribute is ZString

		[ExpectNoExceptions]
		public void TestDescriptionPropertiesAreZStrings()
		{
			string propertyName = "";
			try
			{
				propertyName = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(BusinessObject.GetType());
			}
			catch (NoCodePropertyException)
			{
				return;
			}

			if (!(BusinessObject[propertyName] is ZString) && !(BusinessObject[propertyName] is MultilingualString))
			{
				string message =
					"The DescriptionProperty Attribute <b>{0}</b> is not a ZString or MultilingualString. " +
					"If there is no relevant ZString in your business object, create a calculated property which returns your description, for example: " +
					"<pre>" +
					"<code>" +
					"	<font color=\"Blue\">public</font> ZString Description " + NewLine +
					"	{{" + NewLine;

				if (BusinessObject[propertyName] is ZGuid)
				{
					message += "		<font color=\"Blue\">get</font>  {{ <font color=\"Blue\">return</font> RelatedBizO.XX_SomeZStringField; }} " + NewLine;
				}
				else
				{
					message += "		<font color=\"Blue\">get</font>  {{ <font color=\"Blue\">return</font> {0}.ToString(); }} " + NewLine;
				}

				message += "	}}" +
					"</code>" +
					"</pre>" + NewLine +
					"Add this new property to your schema:" + NewLine +
					"<pre>" +
					"<code>" +
					"	<font color=\"Blue\">public new abstract class</font> Schema : AutoBizO.Schema " + NewLine +
					"	{{" + NewLine +
					"		<font color=\"Blue\">public const string</font> Description = \"Description\";" + NewLine +
					"	}}" +
					"</code>" +
					"</pre>" + NewLine + NewLine;

				HtmlFail(string.Format(message, propertyName));
			}
		}

		#endregion

		#region Geography Properties marked with AllowSpatialTypesAttribute

		public void TestAllowSpatialTypesAttribute()
		{
			if (EnableAllowSpatialTypesAttributeTest)
			{
				var bizObj = GetNewBusinessObject();
				BusinessObject constraintTestBizo = null;
				foreach (ZPropertyInfo propertyInfo in bizObj.ZPropertyInfoHash)
				{
					var propertyInfoToTest = (propertyInfo is IWrappedPropertyInfo) ? ((IWrappedPropertyInfo)propertyInfo).InnerInfo : propertyInfo;
					var geographyPropertyInfo = propertyInfoToTest as ZPropertyInfoGeography;
					if (geographyPropertyInfo != null)
					{
						var allowedTypes = geographyPropertyInfo.AllowedSpatialTypes;

						AssertNotNull(string.Format("The ZGeography property {0} should have AllowGeographyTypesAttribute", geographyPropertyInfo.Name), allowedTypes);
						AssertGreaterThanOrEqualTo(string.Format("The ZGeography property {0} has AllowGeographyTypesAttribute but no actual types are set.", geographyPropertyInfo.Name), allowedTypes.Count, 1);

						if (constraintTestBizo == null)
						{
							constraintTestBizo = GetGeographyConstraintTestBusinessObject();
						}

						var allowedSpatialTypesCheckResult = CheckAllowedSpatialTypesOfDatabase(geographyPropertyInfo.Name, constraintTestBizo);
						var allowedSpatialTypes = from spatialType in allowedSpatialTypesCheckResult.Keys
												  where allowedSpatialTypesCheckResult[spatialType]
												  select spatialType;

						AssertContainsExactElementsInAnyOrder(string.Format("The AllowGeographyTypesAttribute of ZGeography property {0} should set with spatial type(s) which match database constraints.", geographyPropertyInfo.Name), new ZGeography.SpatialTypeComparer(), allowedSpatialTypes, allowedTypes);
					}
				}
			}

			Assert(true);
		}

		public virtual bool EnableAllowSpatialTypesAttributeTest => true;

		BusinessObject GetGeographyConstraintTestBusinessObject()
		{
			var factory = new BusinessObjectFactory();
			BusinessObject bizo = GetNewBusinessObjectSafeSaving();
			FailTestIfBizoIsNull(bizo);

			bizo = factory.Load(bizo.GetType(), bizo.PK);
			FailTestIfBizoIsNull(bizo);

			return bizo;

			void FailTestIfBizoIsNull(BusinessObject bizoToTest)
			{
				if (bizoToTest == null)
				{
					Fail(@"Failed creating test business object for saving. Please:
1. Override and implement GetNewBusinessObjectSafeSavingCore() and make sure the bizo is saved in it properly;
2. Override EnableAllowSpatialTypesAttributeTest to return false to disable the test if the bizo is not able to be saved.");
				}
			}
		}

		Dictionary<string, bool> CheckAllowedSpatialTypesOfDatabase(string propertyName, BusinessObject testBizo)
		{
			var result = new Dictionary<string, bool>();
			var propertyInfo = testBizo.GetZPropertyInfo(propertyName) as ZPropertyInfoGeography;

			using (testBizo.GetValidationSuspender())
			{
				foreach (var spatialType in ZGeography.SpatialType.All)
				{
					propertyInfo.Value = GetTestGeographyValue(spatialType);
					try
					{
						testBizo.Factory.Save();
						result.Add(spatialType, true);
					}
					catch (ZSaveException)
					{
						result.Add(spatialType, false);
					}
				}

				propertyInfo.Value = ZGeography.Empty;
			}

			return result;
		}

		ZGeography GetTestGeographyValue(string spatialType)
		{
			switch (spatialType)
			{
				case ZGeography.SpatialType.Point:
					return new ZGeography("POINT (1 1)");
				case ZGeography.SpatialType.LineString:
					return new ZGeography("LINESTRING (1 1, 2 2)");
				case ZGeography.SpatialType.CircularString:
					return new ZGeography("CIRCULARSTRING(1 1, 2 0, 2 0, 1 1, 0 1)");
				case ZGeography.SpatialType.CompoundCurve:
					return new ZGeography("COMPOUNDCURVE(CIRCULARSTRING(1 0, 0 1, -1 0), (-1 0, 2 0))");
				case ZGeography.SpatialType.Polygon:
					return new ZGeography("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))");
				case ZGeography.SpatialType.CurvePolygon:
					return new ZGeography("CURVEPOLYGON(CIRCULARSTRING(1 3, 3 5, 4 7, 7 3, 1 3))");
				case ZGeography.SpatialType.MultiPoint:
					return new ZGeography("MULTIPOINT ((0 0), (1 1))");
				case ZGeography.SpatialType.MultiLineString:
					return new ZGeography("MULTILINESTRING((1 1, 3 5), (-5 3, -8 -2))");
				case ZGeography.SpatialType.MultiPolygon:
					return new ZGeography("MULTIPOLYGON(((0 0, 0 3, 3 3, 3 0, 0 0), (1 1, 1 2, 2 1, 1 1)), ((9 9, 9 10, 10 9, 9 9)))").MakeValid();
				case ZGeography.SpatialType.GeometryCollection:
					return new ZGeography("GEOMETRYCOLLECTION(LINESTRING(1 1, 3 5),POLYGON((-1 -1, 1 -5, -5 5, -5 -1, -1 -1)))").MakeValid();
				default:
					return GetTestGeographyValue(ZGeography.SpatialType.Point);
			}
		}

		public virtual BusinessObject GetNewBusinessObjectSafeSaving()
		{
			try
			{
				BusinessObject bizo = null;
				bizo = Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
				Factory.Save();
				if (!bizo.IsInDatabase)
				{
					return null;
				}

				return bizo;
			}
			catch
			{
				return null;
			}
		}

		#endregion

		#region On Loaded Create / Load Other Objects

		public virtual void TestOnLoadedDoesNotCreateOrLoadOtherObjects()
		{
			BusinessObject businessObject = this.BusinessObject;
			try
			{
				BusinessObjectFactory.StartLogging();
				businessObject.OnLoaded();
				Assert(
					"OnLoaded should not create or load other objects. "
					+ "OnLoaded performed these operations:\r\n\r\n"
					+ BusinessObjectFactory.DebugLog +
					"\r\n\r\nPut a breakpoint in BusinessObjectFactory.LogAction() to find out why this is happening." +
					"\r\n\r\n",
					BusinessObjectFactory.DebugLogCount == 0);
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		#endregion

		#region OnLoaded Change Persistent Values

		public virtual void TestOnLoadedDoesNotChangePersistentValues()
		{
			string message = "";

			var factory = BusinessObject.Factory;
			ResetAllRowStatesAndHasChanges(factory);
			BusinessObject.OnLoaded();

			foreach (BusinessObject bO in ((IBusinessObjectFactoryInternals)BusinessObject.Factory).AllBusinessObjects)
			{
				if (bO.Row != null)
				{
					if (bO.Row.RowState == DataRowState.Modified)
					{
						message += "\r\n- MODIFIED - " + bO.GetType().FullName + " (" + bO.PK + ")";
					}
					else if (bO.Row.RowState == DataRowState.Added)
					{
						message += "\r\n- ADDED - " + bO.GetType().FullName + " (" + bO.PK + ")";
					}
					else if (bO.Row.RowState == DataRowState.Deleted)
					{
						message += "\r\n- DELETED - " + bO.GetType().FullName + " (" + bO.PK + ")";
					}
				}

				if (bO.HasChanges)
				{
					message += "\r\n- HAS CHANGES is TRUE - " + bO.GetType().FullName + " (" + bO.PK + ")";
				}
			}

			Assert("These objects got changed/created by the OnLoaded() in " + BusinessObject.GetType() +
				"\r\n" + message + "\r\n", message.Length == 0);
		}

		#endregion

		#region Set Default Values - Call to Base

		public virtual void TestCallsBaseSetDefaultValues()
		{
			Assert(BusinessObject.GetType().FullName + " should call base.SetDefaultValues() from any overrides of SetDefaultValues().", BusinessObject.SetDefaultValuesCalled);
		}

		#endregion

		#region Save and Delete

		[ExpectNoExceptions]
		public virtual void TestSaveAndDeleteBusinessObject()
		{
			BusinessObjectFactory factory = NewFactory();
			BusinessObject bO;

			if (!IsDeleteSupported())
			{
				bO = GetNewBusinessObject();
				try
				{
					bO.Delete();
					AssertNotNullOrEmpty("Deleting is not supported on " + bO.GetType().FullName + " but no developer error is raised", ErrorReporter.LastMessageReported);
				}
				catch (NotSupportedException) // if it is very bad to have the object deleted, throw not supported exception
				{
					Assert(true);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
			else
			{
				bO = GetNewBusinessObjectForDeleteTest(factory);
				bO.Factory.Save();
				AssertEquals("BO.IsDeleted", false, bO.IsDeleted);
				BusinessObjectFactory separateFactory = NewFactory();

				AssertNotNull("The BizO is saved and should have been persisted", separateFactory.Load(bO.GetType(), bO.PK));

				if (CanPersistedObjectBeDeleted)
				{
					bO.Delete();

					if (bO.TableName != "StmALog")
					{
						try
						{
							bO.Factory.Save();
						}
						catch (ZSaveException ex)
						{
							if (ex.ToString().Contains("conflicted with the REFERENCE constraint"))
							{
								ErrorReporter.ReportOnce("Tables were last saved in the following order:\r\n" + string.Join(",", ZSaver.LastTableSaveOrder));
							}
							throw;
						}
						separateFactory = NewFactory();
						AssertNull("The BizO should have been deleted from DB", separateFactory.Load(bO.GetType(), bO.PK));
					}
				}
				ErrorReporter.Clear();
			}
		}

		protected virtual bool IsDeleteSupported()
		{
			return true;
		}

		protected virtual bool CanPersistedObjectBeDeleted
		{
			get { return true; }
		}

		#endregion

		#region TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		public virtual void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			BusinessObject bO = GetNewBusinessObject();
			var zDateTimeInfos = new List<ZPropertyInfoDateTime>();
			var zDateTimeOffsetInfos = new List<ZPropertyInfoDateTimeOffset>();
			var excludeAttributeType = typeof(SettingInvalidDateOnDateTimePropertyTestExclude);

			foreach (ZPropertyInfo propertyInfo in bO.ZPropertyInfoHash)
			{
				if (propertyInfo.PropertyDescriptor.Attributes[excludeAttributeType] == null)
				{
					if (propertyInfo is ZPropertyInfoDateTime && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
						zDateTimeInfos.Add((ZPropertyInfoDateTime)propertyInfo);
					}
					else if (propertyInfo is ZPropertyInfoDateTimeOffset && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTimeOffset.Empty;
						zDateTimeOffsetInfos.Add((ZPropertyInfoDateTimeOffset)propertyInfo);
					}
				}
			}

			ZStringBuilder errors = new ZStringBuilder();
			foreach (ZPropertyInfoDateTime infoToSet in zDateTimeInfos)
			{
				infoToSet.Value = ZDateTime.Invalid;
				infoToSet.Value = ZDateTime.Empty;
				foreach (ZPropertyInfoDateTime infoToCheck in zDateTimeInfos)
				{
					if (!infoToCheck.Value.IsEmpty && !infoToCheck.Value.IsValid)
					{
						errors.Append("\r\nSetting [" + infoToSet.Name + "] to ZDateTime.Invalid - Invalid Value was passed on to [" + infoToCheck.Name + "].");
						infoToCheck.Value = ZDateTime.Empty;
					}
				}
				foreach (ZPropertyInfoDateTimeOffset infoToCheck in zDateTimeOffsetInfos)
				{
					if (!infoToCheck.Value.IsEmpty && !infoToCheck.Value.IsValid)
					{
						errors.Append("\r\nSetting [" + infoToSet.Name + "] to ZDateTime.Invalid - Invalid Value was passed on to [" + infoToCheck.Name + "].");
						infoToCheck.Value = ZDateTimeOffset.Empty;
					}
				}
			}
			foreach (ZPropertyInfoDateTimeOffset infoToSet in zDateTimeOffsetInfos)
			{
				infoToSet.Value = ZDateTimeOffset.Invalid;
				infoToSet.Value = ZDateTimeOffset.Empty;
				foreach (ZPropertyInfoDateTime infoToCheck in zDateTimeInfos)
				{
					if (!infoToCheck.Value.IsEmpty && !infoToCheck.Value.IsValid)
					{
						errors.Append("\r\nSetting [" + infoToSet.Name + "] to ZDateTimeOffset.Invalid - Invalid Value was passed on to [" + infoToCheck.Name + "].");
						infoToCheck.Value = ZDateTime.Empty;
					}
				}
				foreach (ZPropertyInfoDateTimeOffset infoToCheck in zDateTimeOffsetInfos)
				{
					if (!infoToCheck.Value.IsEmpty && !infoToCheck.Value.IsValid)
					{
						errors.Append("\r\nSetting [" + infoToSet.Name + "] to ZDateTimeOffset.Invalid - Invalid Value was passed on to [" + infoToCheck.Name + "].");
						infoToCheck.Value = ZDateTimeOffset.Empty;
					}
				}
			}
			ZString result = errors.ToString();
			Assert(result, result.IsEmpty);
		}
		#endregion

		#region OrgHeaderDependency

		public void TestOrgHeaderDependency()
		{
			Type bizObjType = GetExpectedBusinessObjectType();
			if (bizObjType.Assembly.FullName.StartsWith("Enterprise.MasterFiles.Business"))
			{
				ArrayList fields = (ArrayList)OrgHeaderFKs(Factory)[BusinessObject.TableName];
				if (fields != null)
				{
					string message = BusinessObject.TableName + " (";
					for (int i = 0; i < fields.Count; i++)
					{
						message += fields[i];
						if (i < fields.Count - 1)
						{
							message += ", ";
						}
						else
						{
							message += ")";
						}
					}

					message += " This table has column(s) which reference OrgHeader - you need to modify MasterFiles.Business.TemporaryOrgRemover to indicate whether these new column(s) are DEPENDENT on OrgHeader (for example, OrgContact.OC_OH) or INDEPENDENT (eg JobShipment.ConsigneePK).";
					Assert(message, IsTableInTestData(BusinessObject.TableName, fields, bizObjType.Assembly));
				}
			}

			Assert(true);
		}

		static Hashtable OrgHeaderFKs(BusinessObjectFactory factory)
		{
			if (fOrgHeaderFKs == null)
			{
				fOrgHeaderFKs = new Hashtable();

				DynamicBusinessObjectCollection queryResults = new DynamicBusinessObjectCollection(factory);
				string sQLText = @"select
	object_name(parent_object_id) TableName,
	c.Name as fColName
from sys.foreign_key_columns fk
join sys.columns c on c.object_id = fk.parent_object_id and c.column_id = fk.parent_column_id
where fk.referenced_object_id = object_id('OrgHeader');";
				queryResults.Load(sQLText);
				foreach (DynamicBusinessObject queryResult in queryResults)
				{
					string tableName = (string)(ZString)queryResult["tablename"];
					string fieldName = (string)(ZString)queryResult["fcolname"];

					ArrayList fields = (ArrayList)fOrgHeaderFKs[tableName];
					if (fields == null)
					{
						fields = new ArrayList();
						fOrgHeaderFKs[tableName] = fields;
					}
					fields.Add(fieldName);
				}
			}

			return fOrgHeaderFKs;
		}

		static Hashtable fOrgHeaderFKs;

		bool IsTableInTestData(string tableName, ArrayList fields, Assembly bizObjAssembly)
		{
			Type removerTestType = bizObjAssembly.GetType("Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover");
			if (removerTestType != null)
			{
				MethodInfo method = removerTestType.GetMethod("IsTableInTestData", BindingFlags.NonPublic | BindingFlags.Static);
				if (method != null)
				{
					return (bool)method.Invoke(null, new object[] { tableName, fields });
				}
			}

			return false;
		}

		#endregion

		#region OnLoaded Causes HasChanges

		public void TestOnLoadedDoesNotSetHasChanges()
		{
			var bizO = GetNewBusinessObject();
			foreach (BusinessObject bo in ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects)
			{
				bo.HasChanges = false;
			}
			((INeedDataSet)Factory).Data.AcceptChanges();

			Assert("PreCondition : HasChanges should be false before running OnLoaded", !bizO.HasChanges);
			bizO.OnLoaded();
			Assert("Calling OnLoaded has set HasChanges to true on: " + bizO.GetType().FullName, !bizO.HasChanges);
		}

		#endregion

		#region Client-Specific Schema Columns Defined

		public void TestClientSpecificTableSchemaDefined()
		{
			if (IsClientSpecificTable)
			{
				string errorMessage = @"
You must override GetTableSchema and return the schema for client-specific table '" + BizObjTableName + @"':

public override ITableSchema GetTableSchema(string TableName)
{
	switch (TableName)
	{
		case " + BizObjTableName + @"Schema.Constants.TableName : return new " + BizObjTableName + @"Schema();
		default : return null;
	}
}
";
				AssertNotNull(errorMessage, ObjectFactory.Get<IClientHookLoader>().ClientHook.GetTableSchema(BizObjTableName));
			}
			else
			{
				Assert("Not client-specific", true);
			}
		}

		string BizObjTableName
		{
			get { return BusinessObjectFactory.GetTableNameFromType(GetExpectedBusinessObjectType()); }
		}

		bool IsClientSpecificTable
		{
			get
			{
				bool result = false;
				try
				{
					if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(GetExpectedBusinessObjectType()))
					{
						result = BizObjTableName.StartsWith("Client");
					}
				}
				catch
				{
				}
				return result;
			}
		}

		#endregion

		#region Properties with DB Hits

		public void TestCallingBusinessPropertiesSecondTimeDoesntCreateDbHits()
		{
			if (IsSuppressedForTestDbHits)
			{
				Assert("This test case is suppressed.", true);
				return;
			}

			var factory = NewFactory();
			var bizo = GetNewBusinessObjectForDeleteTest(factory);

			factory.Save();

			var newFactory = NewFactory();

			var type = GetExpectedBusinessObjectType();
			var newBizo = newFactory.Load(type, bizo.PK);

			if (newBizo == null)
			{
				var message = string.Format("The business object - {0} has not been saved properly.", bizo.HumanReadableName);
				AssertEquals(message, false, bizo.IsSavedByFactory);
			}
			else
			{
				InstallBizoForTestDbHits(newBizo);

				var result = new SortedDictionary<string, IList<string>>();

				var infos = newBizo.ZPropertyInfoHash;

				foreach (ZPropertyInfo info in infos)
				{
					var descriptor = info.PropertyDescriptor;
					if (descriptor != null && descriptor.Attributes[typeof(BusinessObjectTestExclude)] == null)
					{
						var valueFirstTime = info.Value;

						newFactory.ResetDatabaseLoadCount();

						var valueSecondTime = info.Value;

						var hintedTableNames = newFactory.TableSelects
							.Select(c => string.Concat(c.TableName, " - ", c.Value))
							.ToList();

						if (hintedTableNames.Any())
						{
							result.Add(info.Name, hintedTableNames);
						}

						newFactory.ResetDatabaseLoadCount();
					}
				}

				AssertGroupedErrorList("Following properties make extra db hits to listed tables when they were called the second time.", result);
			}
		}

		protected virtual bool IsSuppressedForTestDbHits
		{
			get
			{
				var type = GetExpectedBusinessObjectType();
				var assemblyName = type.FullName;

				var includedNames = new[]
				{
					"Enterprise.Freight", "Enterprise.Rating", "Enterprise.eManifest"
				};

				var excludedNames = new[]
				{
					"Enterprise.Freight.LocalCartage"
				};

				var isIlAssembly = includedNames.All(c => assemblyName.StartsWith(c, true, CultureInfo.InvariantCulture))
					&& excludedNames.Any(c => !assemblyName.StartsWith(c, true, CultureInfo.InvariantCulture));

				return !isIlAssembly;
			}
		}

		protected virtual void InstallBizoForTestDbHits(BusinessObject bizo)
		{
		}

		#endregion

		#region Calculated Properties with DB Hits - Use Fetch Hints
#pragma warning disable CS3016
		[DeveloperOnlyTest(new string[] { "Enterprise.Customs.Business.Testing" })]
		public virtual void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			var result = new SortedDictionary<string, IList<string>>();

			var factory = new BusinessObjectFactory();
			var bizo = GetNewBusinessObjectForDeleteTest(factory);
			if (bizo.Table != null && bizo.GetType().GetCustomAttribute(typeof(BusinessObjectTestExclude)) == null && bizo.FetchStrategy != null)
			{
				var typeToLoad = GetTypeForCalcPropertiesFetchHintTest(bizo.GetType());
				factory.RefreshEnabled = false;
				factory.Save();
				foreach (var info in GetCalcProperties(bizo))
				{
					try
					{
						var differentFactory = new BusinessObjectFactory { RefreshEnabled = false };
						var bizObjInDiffFactory = differentFactory.Load(typeToLoad, bizo.PK);
						var fetchStrategy = bizObjInDiffFactory?.FetchStrategy;
						if (fetchStrategy != null)
						{
							var infoOnBizObjInDiffFactory = bizObjInDiffFactory.ZPropertyInfoHash.GetPropertySafe(info.Name);
							if (infoOnBizObjInDiffFactory != null)
							{
								fetchStrategy.FetchForView(new[] { new TableColumn("", infoOnBizObjInDiffFactory.Name) });
								var fetchedTables = bizObjInDiffFactory.Factory.RowFactory.GetAllFetchHintedTableNames().ToList();
								bizObjInDiffFactory.Factory.DropHints();

								var exceptTables = new List<string> { "GlbCompany", "GlbBranch", "StmALog", DummyBizoSchema.Constants.TableName };
								exceptTables.Add(bizObjInDiffFactory.TableName);
								exceptTables.AddRange(GetAdditionalIgnoreTablesForFetchHintsCheck());

								var loadedTables = bizObjInDiffFactory.Factory.RowFactory.LogLoadTables(() => { var x = infoOnBizObjInDiffFactory.Value; }, exceptTables);
								if (loadedTables.Count > 0)
								{
									var notFetchHintedTables = loadedTables.Except(fetchedTables).ToList();
									if (notFetchHintedTables.Count > 0)
									{
										result.Add(infoOnBizObjInDiffFactory.Name, notFetchHintedTables);
									}
								}
							}
						}

						differentFactory.Save();
					}
					catch (Exception)
					{
						// Ignore any exception and move to next property
					}
				}
			}

			AssertGroupedErrorList("Following properties make extra db hits to listed tables without fetch hints. Add fetch hints in method FetchForView() of bizo's FetchStrategy.", result);
		}
#pragma warning restore CS3016

		protected virtual Type GetTypeForCalcPropertiesFetchHintTest(Type type)
		{
			Type foundType = null;
			var baseType = type;
			while (foundType == null && baseType != null)
			{
				var decider = TypeDecider.GetTypeDeciderFromType(baseType);
				if (decider == null)
				{
					baseType = baseType.BaseType;
				}
				else
				{
					foundType = baseType;
				}
			}
			return foundType ?? type;
		}

		protected virtual IEnumerable<string> GetAdditionalIgnoreTablesForFetchHintsCheck()
		{
			yield break;
		}

		static IEnumerable<ZPropertyInfo> GetCalcProperties(BusinessObject bizo)
		{
			return
				from ZPropertyInfo propertyInfo in bizo.ZPropertyInfoHash
				let propertyDescriptor = propertyInfo.PropertyDescriptor
				where propertyDescriptor.Attributes[typeof(BusinessObjectTestExclude)] == null
				where typeof(IZType).IsAssignableFrom(propertyInfo.PropertyType) && !bizo.Table.Columns.Contains(propertyInfo.Name)
				let componentType = propertyDescriptor.ComponentType
				where componentType == bizo.GetType() || (componentType.IsAbstract && componentType == bizo.GetType().BaseType)
				select propertyInfo;
		}

		#endregion

		#region Implementation

		protected BusinessObject BusinessObject
		{
			get
			{
				if (businessObject == null)
				{
					businessObject = GetNewBusinessObject();
				}
				return businessObject;
			}
		}
		BusinessObject businessObject;

		protected virtual BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BusinessObject result;
			try
			{
				result = factory.NewWithValidTestData(
					 GetExpectedBusinessObjectType(),
					 TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			}
			catch
			{
				result = factory.New(GetExpectedBusinessObjectType());
				result.FillWithValidTestData();
			}

			SetSpeicalValueWhenGetingNewBusinessObject(result);
			return result;
		}

		protected virtual void SetSpeicalValueWhenGetingNewBusinessObject(BusinessObject result)
		{
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			BusinessObject result = Factory.New(GetExpectedBusinessObjectType());
			return result;
		}

		protected void ResetAllRowStatesAndHasChanges(BusinessObjectFactory factory)
		{
			foreach (BusinessObject bO in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				bO.ResetHasChangesForTest();
			}
			((INeedDataSet)factory).Data.AcceptChanges();
			((INeedDataSet)factory).Data.AcceptChanges();
		}

		#endregion
	}
}
