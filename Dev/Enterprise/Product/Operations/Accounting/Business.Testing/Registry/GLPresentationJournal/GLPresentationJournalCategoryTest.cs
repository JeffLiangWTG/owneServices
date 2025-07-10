using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLPresentationJournalCategory))]
	class GLPresentationJournalCategoryTest : CodeDescriptionBoolWithExtraBoolTest
	{
		public void TestCode_ReadOnly()
		{
			BizObj.Code = "ABC";
			AssertEquals(true, BizObj.CanDelete);
			AssertEquals(false, BizObj.CodeInfo.ReadOnly);

			BizObj.Code = "AAA";
			AssertEquals("Cannot delete because it is already used", false, BizObj.CanDelete);
			AssertEquals("Should not be changed as it is used", true, BizObj.CodeInfo.ReadOnly);
		}

		public void TestIsActive()
		{
			BizObj.Bool = true;
			AssertEquals("Bool", ZBool.True, BizObj.Bool);
			AssertEquals("Bool.ReadOnly", false, BizObj.BoolInfo.ReadOnly);

			BizObj.Bool = false;
			AssertEquals("Bool", ZBool.False, BizObj.Bool);
			AssertEquals("Bool.ReadOnly", false, BizObj.BoolInfo.ReadOnly);

			BizObj.Bool = true;
			BizObj.Code = "aaa";
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals(false, BizObj.BoolInfo.ReadOnly);
			AssertEquals(false, BizObj.Bool2Info.ReadOnly);

			BizObj.Bool2 = true;
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals(true, BizObj.BoolInfo.ReadOnly);
			AssertEquals(true, BizObj.Bool2Info.ReadOnly);

			BizObj.Bool = false;
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals(false, BizObj.BoolInfo.ReadOnly);
			AssertEquals(true, BizObj.Bool2Info.ReadOnly);
		}

		public void TestUseForElimination()
		{
			BizObj.Bool2 = true;
			AssertEquals("Bool2", ZBool.True, BizObj.Bool2);

			BizObj.Bool2 = false;
			AssertEquals("Bool2", ZBool.False, BizObj.Bool2);

			BizObj.Code = "aAa";
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals(false, BizObj.BoolInfo.ReadOnly);
			AssertEquals(false, BizObj.Bool2Info.ReadOnly);

			BizObj.Bool2 = true;
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals(true, BizObj.BoolInfo.ReadOnly);
			AssertEquals(true, BizObj.Bool2Info.ReadOnly);

			BizObj.Bool2Info.AddError("some errors here");
			Assert("Expect elimination editable when error detected", !BizObj.Bool2Info.ReadOnly);
		}

		public void TestUseForClosing()
		{
			BizObj.Bool3 = true;
			AssertEquals("Bool3", ZBool.True, BizObj.Bool3);

			BizObj.Bool3 = false;
			AssertEquals("Bool3", ZBool.False, BizObj.Bool3);
		}

		public void TestUseForOpening()
		{
			BizObj.Bool4 = true;
			AssertEquals("Bool4", ZBool.True, BizObj.Bool4);

			BizObj.Bool4 = false;
			AssertEquals("Bool4", ZBool.False, BizObj.Bool4);
		}

		public new void TestCanDelete()
		{
			BizObj.Code = "";
			Assert(BizObj.CanDelete);

			BizObj.Code = "ABC";
			Assert(BizObj.CanDelete);

			BizObj.Code = "AaA";
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("Cannot delete Category already used in aggregation. Make inactive instead.", BizObj.ReasonForNotAbleToDelete);

			BizObj.Code = "ABC";
			Assert(BizObj.CanDelete);

			BizObj.Bool2 = true;
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("Cannot delete Elimination Category.", BizObj.ReasonForNotAbleToDelete);

			BizObj.Code = "AaA";
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("Cannot delete Elimination Category.", BizObj.ReasonForNotAbleToDelete);

			BizObj.Code = "ABC";
			BizObj.Bool2 = false;
			AssertEquals(true, BizObj.CanDelete);

			var bizObj1 = BizObj.ParentCollection.AddNew();
			bizObj1.Code = "PPP";
			var bizObj2 = BizObj.ParentCollection.AddNew();
			bizObj2.Code = "QQQ";

			Assert(bizObj1.CanDelete);
			Assert(bizObj2.CanDelete);
			bizObj1.ParentCode = BizObj.Code;
			bizObj2.ParentCode = BizObj.Code;
			AssertEquals(false, BizObj.CanDelete);
			AssertEquals("Code 'ABC' is the parent of 'PPP','QQQ' and cannot be deleted. ", BizObj.ReasonForNotAbleToDelete);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			AssertEquals("Code", 3, clone.CodeMaxLength);
			AssertEquals("Code", "ABC", clone.Code);
			AssertEquals("Description", "ABC Category Description", clone.Description);
			AssertEquals("Bool", true, ((GLPresentationJournalCategory)clone).Bool);
			AssertEquals("Bool2", true, ((GLPresentationJournalCategory)clone).Bool2);
			AssertEquals("Bool3", false, ((GLPresentationJournalCategory)clone).Bool3);
			AssertEquals("Bool4", false, ((GLPresentationJournalCategory)clone).Bool4);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new GLPresentationJournalCategory();

			result.Code = "ABC";
			result.Description = (NoResString)"ABC Category Description";
			result.Bool = true;
			result.Bool2 = true;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var collection = new GLPresentationJournalCategoryCollection();
			var element = collection.AddNew();
			element.Code = "TST";
			return element;
		}

		protected new GLPresentationJournalCategory BizObj
		{
			get { return (GLPresentationJournalCategory)base.BizObj; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			Creator = new TestObjectCreator(Factory);
			Creator.CreateAccGLAggregate(10m, 0, Creator.GLHeader1.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentDepartment.PK, "AAA");
			Creator.CreateAccGLAggregate(20m, 0, Creator.GLHeader1.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentDepartment.PK, "Bbb");
			Factory.Save();
		}

		TestObjectCreator Creator;
	}
}
