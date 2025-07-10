using System;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	public abstract class DocBaseWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMakeSureAllIZTypePropertiesCanBeAccessedWithNoExceptions()
		{
			CheckAllPropertiesOfAGivenTypeCanBeAccessedWithoutException(typeof(IZType));
		}

		[ExpectNoExceptions]
		public void TestMakeSureAllDocBaseWrapperTypePropertiesCanBeAccessedWithNoExceptions()
		{
			CheckAllPropertiesOfAGivenTypeCanBeAccessedWithoutException(typeof(DocBaseWrapper));
		}

		[ExpectNoExceptions]
		public void TestMakeSureAllDocBaseWrapperCollectionTypePropertiesCanBeAccessedWithNoExceptions()
		{
			CheckAllPropertiesOfAGivenTypeCanBeAccessedWithoutException(typeof(DocBaseWrapperCollection));
		}

		public void TestMakeSureAllDocBaseWrapperHasPublicStaticNew()
		{
			const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod;
			var wrapperType = Wrapper.GetType();
			if (wrapperType.GetCustomAttribute<DocumentWrapper.AllowNoStaticNew>(inherit: true) is not null)
			{
				AssertNotNull("Skip this test since wrapper is allowed to not have 'public static New()'.");
				return;
			}
			var methodInfos = wrapperType
				.GetMethods(bindingFlags)
				.Where(methodInfo => methodInfo.Name is "New")
				.ToArray();
			AssertNotEquals("Please implement 'public static New()', this is needed by DocumentWrapperFactory.", 0, methodInfos.Length);
			methodInfos = methodInfos.Where(x => typeof(DocumentWrapper).IsAssignableFrom(x.ReturnType)).ToArray();
			AssertNotEquals("Please implement 'public static New()' with return type 'wrapper', this is needed by DocumentWrapperFactory.", 0, methodInfos.Length);
			var paramInfos = methodInfos
				.Select(x => x.GetParameters())
				.Where(paramInfos => paramInfos.Length is 2)
				.ToArray();
			AssertNotEquals("Please implement 'public static New()' with two parameters, this is needed by DocumentWrapperFactory.", 0, paramInfos.Length);
			paramInfos = paramInfos.Where(x => typeof(BusinessObject).IsAssignableFrom(x[0].ParameterType)).ToArray();
			AssertNotEquals("Please implement 'public static New()' with first parameter 'bizo', this is needed by DocumentWrapperFactory.", 0, paramInfos.Length);
			paramInfos = paramInfos.Where(x => typeof(BusinessObjectFactory).IsAssignableFrom(x[1].ParameterType)).ToArray();
			AssertNotEquals("Please implement 'public static New()' with second parameters 'factory', this is needed by DocumentWrapperFactory.", 0, paramInfos.Length);
		}

		#region Implementation
		void CheckAllPropertiesOfAGivenTypeCanBeAccessedWithoutException(Type typeOfPropertiesToCheck)
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			foreach (PropertyInfo propertyInfo in Wrapper.GetType().GetProperties())
			{
				if (typeOfPropertiesToCheck.IsAssignableFrom(propertyInfo.PropertyType))
				{
					try
					{
						propertyInfo.GetValue(Wrapper, null);
					}
					catch (Exception e)
					{
						ErrorReporter.ReportOnce(
							"Exception Ocurred Accessing " + typeOfPropertiesToCheck.Name + " Property: " + propertyInfo.Name + " (" + propertyInfo.PropertyType.Name + ")",
							(e is TargetInvocationException ? e.InnerException : e));
					}
				}
			}
		}

		protected DocBaseWrapper Wrapper
		{
			get
			{
				if (fWrapper == null)
				{
					fWrapper = GetNewDocumentWrapper();
				}
				return fWrapper;
			}
		}
		DocBaseWrapper fWrapper;

		protected abstract DocBaseWrapper GetNewDocumentWrapper();
		#endregion
	}
}
