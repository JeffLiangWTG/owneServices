using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ParentCodeDescriptionBool))]
	class TestParentCodeDescriptionBool : RegistryBusinessObjectTestCaseBase
	{
		public void TestSetChildList()
		{
			CodeDescriptionBoolCollection newChildList = new CodeDescriptionBoolCollection();
			BizObj.SetChildList(newChildList);
			AssertEquals("ChildList", newChildList, BizObj.ChildList);

			BizObj.fChildList = null;
			Assert("ChildList should be reset.", BizObj.ChildList != newChildList);
			BizObj.SetChildList(newChildList);
			AssertEquals("ChildList", newChildList, BizObj.ChildList);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ParentCodeDescriptionBool result = new ParentCodeDescriptionBool();

			result.Code = "ABC";
			result.Description = (NoResString)"ABC Description";

			CodeDescriptionBool child1 = result.ChildList.AddNew();
			CodeDescriptionBool child2 = result.ChildList.AddNew();

			child1.Code = "C1";
			child1.Description = (NoResString)"Description 1";

			child2.Code = "C2";
			child2.Description = (NoResString)"Description 2";

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void CheckAllPropertiesAreEqual(RegistryBusinessObjectTemplate originalBusinessObject, RegistryBusinessObjectTemplate newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			ParentCodeDescriptionBool originalParent = (ParentCodeDescriptionBool)originalBusinessObject;
			ParentCodeDescriptionBool newParent = (ParentCodeDescriptionBool)newBusinessObject;

			AssertEquals("ChildList.Count", originalParent.ChildList.Count, newParent.ChildList.Count);

			for (int i = 0; i < originalParent.ChildList.Count; i++)
			{
				AssertEquals("ChildList[" + i + "].Code", originalParent.ChildList[i].Code, newParent.ChildList[i].Code);
				AssertEquals("ChildList[" + i + "].Description", originalParent.ChildList[i].Description, newParent.ChildList[i].Description);
			}
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new ParentCodeDescriptionBool BizObj
		{
			get { return (ParentCodeDescriptionBool)base.BizObj; }
		}

		#endregion
	}
}
