using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[TestedType(typeof(FieldModule))]
	internal sealed class FieldModuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSelectedFieldName()
		{
			AssertEquals("", Module.SelectedFieldName);
			Module.SetInfoChain(new PropertyInfo[] { GetInfo<DummyBusinessObjectWithDocumentSupport>("Collection") });
			AssertEquals("Collection...", Module.SelectedFieldName);
			Module.SetInfoChain(null);
			AssertEquals("", Module.SelectedFieldName);
			Module.SetInfoChain(new PropertyInfo[] { GetInfo<DummyBusinessObjectWithDocumentSupport>("Collection"), GetInfo<DummyBusinessObjectWithDocumentSupport>("Z0_Code"), });
			AssertEquals("Collection.Z0_Code", Module.SelectedFieldName);
		}

		public void TestSetProperty()
		{
			var container = new CustomPropertyContainer();
			container.AddCustomProperty("name", "value");
			Module.SetProperty(container["name"]);
			AssertEquals("name", Module.SelectedFieldName);
			AssertEquals("name", Module.SelectedCaption);
			AssertEquals("name", Module.SelectedDescription);
		}

		public void TestNoCaptionAndDescription()
		{
			PropertyInfo[] infoChain = new PropertyInfo[] { GetInfo<DummyBusinessObjectWithDocumentSupport>("Collection"), GetInfo<DummyBusinessObjectWithDocumentSupport>("Z0_AnotherNumber"), };
			Module.SetInfoChain(infoChain);
			AssertEquals("", Module.SelectedCaption);
			AssertEquals("", Module.SelectedDescription);
		}

		#region Implementation
		PropertyInfo GetInfo<T>(string name)
		{
			PropertyInfo info = ReflectionHelper.GetPropertyInfo(typeof(T), name) ?? throw new ArgumentException(string.Format("The property {0} does not exist on {1}.", name, typeof(T).FullName));

			return info;
		}

		FieldModule Module
		{
			get
			{
				if (module == null)
				{
					module = new FieldModule();
				}

				return module;
			}
		}

		FieldModule module;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FieldModule();
		}
		#endregion
	}
}
