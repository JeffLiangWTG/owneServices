using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinableCodeDescriptionBoolWithExtraBool))]
	sealed class SystemDefinableCodeDescriptionBoolWithExtraBoolTest : CodeDescriptionBoolWithExtraBoolTest
	{
		public new void TestCode()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			collection.SetDefaultCode("x", false);
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();
			element1.Code = "y";
			AssertEquals("ParentCollection.DefaultCode", "x", collection.DefaultCode);
			element2.Bool = true;
			element2.Code = "z";
			AssertEquals("ParentCollection.DefaultCode", "z", collection.DefaultCode);
		}

		public new void TestBool()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();

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

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 35, BizObj.DescriptionInfo.MaxLength);
		}

		#region Implementation

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("Code", 55, clone.CodeMaxLength);
			AssertEquals("Code", "BLAHB", clone.Code);
			AssertEquals("Description", "Desc", clone.Description);
			Assert("SystemDefined", ((SystemDefinableCodeDescriptionBoolWithExtraBool)clone).SystemDefined);
			AssertEquals("Bool", false, ((SystemDefinableCodeDescriptionBoolWithExtraBool)clone).Bool);
			AssertEquals("Bool2", true, ((SystemDefinableCodeDescriptionBoolWithExtraBool)clone).Bool2);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new SystemDefinableCodeDescriptionBoolWithExtraBool();
			result.Bool = false;
			result.Bool2 = true;
			result.SystemDefined = true;
			result.CodeMaxLength = 55;
			result.Code = "BLAHB";
			result.Description = (Enterprise.ZArchitecture.Core.NoResString)"Desc";
			return result;
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			if (isClone)
			{
				AssertEquals("SystemDefined", ((SystemDefinableCodeDescriptionBoolWithExtraBool)originalBusinessObject).SystemDefined, ((SystemDefinableCodeDescriptionBoolWithExtraBool)newBusinessObject).SystemDefined);
				AssertEquals("Bool2", ((SystemDefinableCodeDescriptionBoolWithExtraBool)originalBusinessObject).Bool2, ((SystemDefinableCodeDescriptionBoolWithExtraBool)newBusinessObject).Bool2);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			var element = collection.AddNew();
			element.Code = "TST";
			return element;
		}

		new SystemDefinableCodeDescriptionBoolWithExtraBool BizObj
		{
			get { return (SystemDefinableCodeDescriptionBoolWithExtraBool)base.BizObj; }
		}

		#endregion
	}
}
