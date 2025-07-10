using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocumentWrapperInterfaceTest : TestCaseWithFactory
	{
		public void TestNoPublicConstructorsInDocWrappers()
		{
			string errorMessage = "";
			SubClassRetriever retriever = new SubClassRetriever(GetType().Assembly, typeof(DocumentWrapper));
			retriever.ExcludedAttributes = new[] { typeof(DocumentWrapper.AllowPublicConstructor) };
			retriever.ExcludedTypesAndTheirDescendants = new Type[] { typeof(GenericWrappers.Base.GenericWrapper) };
			retriever.IncludeAbstractClasses = false;
			retriever.IncludeTestClasses = false;
			foreach (Type type in retriever.Retrieve())
			{
				int publicInstanceConstructorCount = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public).Length;
				if (publicInstanceConstructorCount > 0)
				{
					errorMessage += String.Format("Type {0} has {1} public constructor(s), expected none\n", type, publicInstanceConstructorCount);
				}
			}
			AssertEquals("All DocumentWrappers should not have Public Constructors.\r\nAdd the [DocumentWrapper.AllowPublicConstructor] to bypass this", "", errorMessage);
		}

		public void TestStaticNewMethodInAllDocWrappers()
		{
			StringBuilder errorMessage = new StringBuilder();

			SubClassRetriever retriever = new SubClassRetriever(GetType().Assembly, typeof(DocumentWrapper));
			retriever.ExcludedAttributes = new[] { typeof(DocumentWrapper.AllowNoStaticNew) };
			retriever.ExcludedTypesAndTheirDescendants = new Type[] { typeof(GenericWrappers.Base.GenericWrapper) };
			retriever.IncludeAbstractClasses = false;
			retriever.IncludeTestClasses = false;
			foreach (Type type in retriever.Retrieve())
			{
				MethodInfo[] publicStaticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
				bool foundMatchingMethod = false;
				foreach (MethodInfo method in publicStaticMethods)
				{
					if (method.Name == "New")
					{
						ParameterInfo[] parameters = method.GetParameters();
						if (parameters.Length == 2 && typeof(BusinessObjectFactory).IsAssignableFrom(parameters[1].ParameterType))
						{
							foundMatchingMethod = true;
							break;
						}
					}
				}

				if (!foundMatchingMethod)
				{
					errorMessage.Append(type + System.Environment.NewLine);
				}
			}

			if (errorMessage.Length > 0)
			{
				errorMessage.Insert(0, "All DocumentWrappers should have a static New method." + System.Environment.NewLine + "The structure for this method is New(AppropriateSubclassOfBusinessObject, BusinessObjectFactory FactoryToWrap)");
				Fail(errorMessage.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		#region TestImagesDoNotCacheDisposedObjects

		public void TestImagesDoNotCacheDisposedObjects()
		{
			StringBuilder report = new StringBuilder();

			TestImagesDoNotCacheDisposedObjects(typeof(BusinessObjectBaseTestCase), "GetNewBusinessObject", report);
			TestImagesDoNotCacheDisposedObjects(typeof(DocBaseWrapperTest), "GetNewDocumentWrapper", report);

			Assert("Following Image properties may cache disposed image object\r\n" + report.ToString(), report.Length == 0);
		}

		public void TestImagesDoNotCacheDisposedObjects(Type baseTestCaseType, string componetGetterMethod, StringBuilder report)
		{
			SubClassRetriever retriever =
				new SubClassRetriever(GetType().Assembly, baseTestCaseType)
				{
					IncludeAbstractClasses = false,
					IncludeTestClasses = true,
					IncludeNestedClasses = true,
					IncludePrivateNestedClasses = true,
					ExcludedTypesAndTheirDescendants = new[] { typeof(ImageWrapperTest) },
				};

			MethodInfo newBizoMethod = baseTestCaseType.GetMethod(componetGetterMethod, BindingFlags.NonPublic | BindingFlags.Instance);

			foreach (Type testCaseType in retriever.Retrieve())
			{
				try
				{
					var testCase = Activator.CreateInstance(testCaseType);
					testCaseType.GetMethod("SetUp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(testCase, null);
					try
					{
						object component = newBizoMethod.Invoke(testCase, null);
						if (component != null)
						{
							AssertComponentImagesDoNotCacheDisposedObjects(component, report);
						}
					}
					finally
					{
						testCaseType.GetMethod("TearDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(testCase, null);
					}
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}

					// Skip problematic test cases
				}
			}
		}

		void AssertComponentImagesDoNotCacheDisposedObjects(object component, StringBuilder report)
		{
			Type componentType = component.GetType();
			List<PropertyInfo> imageProperties = new List<PropertyInfo>();
			List<string> badPropertyNames = new List<string>();

			foreach (PropertyInfo property in componentType.GetProperties())
			{
				if (typeof(Image).IsAssignableFrom(property.PropertyType))
				{
					imageProperties.Add(property);

					if (property.CanWrite)
					{
						badPropertyNames.AddRange(CheckPropertiesCachingDisposableImages(component, property, new[] { property }).Where(name => !badPropertyNames.Contains(name)));
					}
				}
			}

			Type currentType = componentType;
			while (currentType != typeof(object))
			{
				foreach (FieldInfo field in currentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
				{
					if (typeof(Image).IsAssignableFrom(field.FieldType) && !field.IsInitOnly)
					{
						badPropertyNames.AddRange(CheckPropertiesCachingDisposableImages(component, field, imageProperties).Where(name => !badPropertyNames.Contains(name)));
					}
				}
				currentType = currentType.BaseType;
			}

			DisposeJobHeaderIfComponentIsDocBaseWrapperWithJobHeader(component);

			if (badPropertyNames.Count != 0)
			{
				report.AppendLine("\r\n" + componentType.FullName + ":\r\n\t" + string.Join("\t\r\n", badPropertyNames.ToArray()));
			}
		}

		void DisposeJobHeaderIfComponentIsDocBaseWrapperWithJobHeader(object component)
		{
			if (component is DocBaseWrapperWithJobHeader docBaseWrapperWithJobHeader)
			{
				docBaseWrapperWithJobHeader.JobHeader?.JobHeader?.Dispose();
			}
		}

		IEnumerable<string> CheckPropertiesCachingDisposableImages(object component, MemberInfo setterMember, IEnumerable<PropertyInfo> candidateProperties)
		{
			Image disposableImage = new Bitmap(1, 1);
			SetImageOnComponentMember(component, setterMember, disposableImage);

			foreach (PropertyInfo property in candidateProperties)
			{
				object propertyImage = null;
				try
				{
					propertyImage = property.GetValue(component, null);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
				}

				if (ReferenceEquals(disposableImage, propertyImage))
				{
					disposableImage.Dispose();
					if (ReferenceEquals(disposableImage, property.GetValue(component, null)))
					{
						yield return property.Name;
					}

					disposableImage = new Bitmap(1, 1);
					SetImageOnComponentMember(component, setterMember, disposableImage);
				}
			}

			SetImageOnComponentMember(component, setterMember, null);
		}

		void SetImageOnComponentMember(object component, MemberInfo member, Image image)
		{
			FieldInfo field = member as FieldInfo;
			PropertyInfo property = member as PropertyInfo;

			if (field != null)
			{
				field.SetValue(component, image);
			}
			else if (property != null)
			{
				property.SetValue(component, image, null);
			}
		}

		#endregion
	}
}
