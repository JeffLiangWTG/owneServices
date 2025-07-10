using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinableCodeDescriptionBool))]
	sealed class SystemDefinableCodeDescriptionBoolTest : CodeDescriptionBoolTest
	{
		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("Code", 55, clone.CodeMaxLength);
			AssertEquals("Code", "BLAHB", clone.Code);
			AssertEquals("Description", "Description :)", clone.Description);
			Assert("SystemDefined", ((SystemDefinableCodeDescriptionBool)clone).SystemDefined);
		}

		public void TestCode()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			collection.SetDefaultCode("x", false);
			SystemDefinableCodeDescriptionBool element1 = collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = collection.AddNew();
			element1.Code = "y";
			AssertEquals("ParentCollection.DefaultCode", "x", collection.DefaultCode);
			element2.Bool = true;
			element2.Code = "z";
			AssertEquals("ParentCollection.DefaultCode", "z", collection.DefaultCode);
		}

		public void TestBool()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element1 = collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = collection.AddNew();

			element1.Bool = true;
			element2.Code = "x";
			element2.Bool = true;
			AssertEquals("element1.Bool", false, element1.Bool);
			AssertEquals("element2.Bool", true, element2.Bool);
			AssertEquals("ParentCollection.DefaultCode", "x", collection.DefaultCode);
			AssertEquals("ParentCollection.DefaultElement", element2, collection.DefaultElement);

			element2.Bool = false;
			AssertEquals("element2.Bool", false, element2.Bool);
			AssertEquals("ParentCollection.DefaultCode", "", collection.DefaultCode);
			AssertNull("ParentCollection.DefaultElement", collection.DefaultElement);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			SystemDefinableCodeDescriptionBool result = (SystemDefinableCodeDescriptionBool)base.GetBusinessObjectToClone();
			result.Bool = false;
			result.SystemDefined = true;
			result.CodeMaxLength = 55;
			result.Code = "BLAHB";
			result.Description = (NoResString)"Description :)";
			return result;
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			if (isClone)
			{
				AssertEquals("SystemDefined", ((SystemDefinableCodeDescriptionBool)originalBusinessObject).SystemDefined, ((SystemDefinableCodeDescriptionBool)newBusinessObject).SystemDefined);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element = collection.AddNew();
			element.Code = "TST";
			element.Description = (NoResString)"Test Element";
			return element;
		}

		#endregion
	}
}
