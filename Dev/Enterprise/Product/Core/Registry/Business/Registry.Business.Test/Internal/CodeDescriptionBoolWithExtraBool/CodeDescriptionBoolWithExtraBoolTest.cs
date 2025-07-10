using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionBoolWithExtraBool))]
	public class CodeDescriptionBoolWithExtraBoolTest : CodeDescriptionBoolTest
	{
		public void TestCode()
		{
			var collection = new CodeDescriptionBoolWithExtraBoolCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();
			element1.Code = "y";
			element2.Code = "z";
			AssertEquals("element1.Code", "y", element1.Code);
			AssertEquals("element2.Code", "z", element2.Code);
		}

		public void TestBool()
		{
			var collection = new CodeDescriptionBoolWithExtraBoolCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();

			element1.Bool = false;
			element2.Bool = true;
			AssertEquals("element1.Bool", false, element1.Bool);
			AssertEquals("element2.Bool", true, element2.Bool);
		}

		public void TestBool2()
		{
			var collection = new CodeDescriptionBoolWithExtraBoolCollection();
			var element1 = collection.AddNew();
			var element2 = collection.AddNew();

			element1.Bool2 = false;
			element2.Bool2 = true;
			AssertEquals("element1.Bool2", false, element1.Bool2);
			AssertEquals("element2.Bool2", true, element2.Bool2);
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		#region Implementation

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("Code", 55, clone.CodeMaxLength);
			AssertEquals("Code", "BLAHB", clone.Code);
			AssertEquals("Description", "Desc", clone.Description);
			AssertEquals("Bool", false, ((CodeDescriptionBoolWithExtraBool)clone).Bool);
			AssertEquals("Bool2", true, ((CodeDescriptionBoolWithExtraBool)clone).Bool2);
			AssertEquals("SystemDefined", true, ((CodeDescriptionBoolWithExtraBool)clone).SystemDefined);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			CodeDescriptionBoolWithExtraBool result = new CodeDescriptionBoolWithExtraBool();
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
				AssertEquals("SystemDefined", ((CodeDescriptionBoolWithExtraBool)originalBusinessObject).SystemDefined, ((CodeDescriptionBoolWithExtraBool)newBusinessObject).SystemDefined);
				AssertEquals("Bool2", ((CodeDescriptionBoolWithExtraBool)originalBusinessObject).Bool2, ((CodeDescriptionBoolWithExtraBool)newBusinessObject).Bool2);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new CodeDescriptionBoolWithExtraBoolCollection();
			var element = collection.AddNew();
			element.Code = "TST";
			return element;
		}

		new CodeDescriptionBoolWithExtraBool BizObj
		{
			get { return (CodeDescriptionBoolWithExtraBool)base.BizObj; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		#endregion
	}
}
