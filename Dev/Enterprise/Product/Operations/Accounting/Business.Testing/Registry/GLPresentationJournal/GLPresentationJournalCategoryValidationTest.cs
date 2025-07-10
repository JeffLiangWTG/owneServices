using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class GLPresentationJournalCategoryValidationTest : TestCaseWithFactory
	{
		public void TestRunPreSaveValidation()
		{
			BizObj.Code = "ABC";
			BizObj.Bool2 = true;
			BizObj.Bool3 = true;
			BizObj.Bool4 = true;

			var anotherBizObj = BizObjCollection.AddNew();
			anotherBizObj.Code = "abc";
			anotherBizObj.Bool2 = true;
			anotherBizObj.Bool3 = true;
			anotherBizObj.Bool4 = true;

			BizObj.ClearAllNotifications();
			AssertNoErrors("Precondition: There should be no errors.", BizObj);

			BizObj.RunPreSaveValidation();
			AssertHasErrors(BizObj.CodeInfo);
			AssertHasErrors(BizObj.DescriptionInfo);
			AssertHasErrors(BizObj.Bool2Info);
			AssertHasErrors(BizObj.Bool3Info);
			AssertHasErrors(BizObj.Bool4Info);
		}

		public void TestCode()
		{
			BizObj.Code = "";
			BizObj.ValidateCode();
			AssertHasErrors(BizObj.CodeInfo);

			BizObj.Code = "ABC";
			AssertNoErrors(BizObj.CodeInfo);
		}

		public void TestParentCode()
		{
			var otherBizObj1 = BizObjCollection.AddNew();
			var otherBizObj2 = BizObjCollection.AddNew();

			BizObj.Code = "MMM";
			otherBizObj1.Code = "NNN";
			otherBizObj2.Code = "PPP";

			otherBizObj1.ParentCode = "ZZZ";
			AssertNoErrors(BizObj.ParentCodeInfo);
			Assert(otherBizObj1.ParentCodeInfo.HasError("Code 'ZZZ' is not valid"));
			AssertNoErrors(otherBizObj2.ParentCodeInfo);

			otherBizObj1.ParentCode = "MMM";
			otherBizObj2.ParentCode = "NNN";
			AssertNoErrors(BizObj.ParentCodeInfo);
			AssertNoErrors(otherBizObj1.ParentCodeInfo);
			Assert(otherBizObj2.ParentCodeInfo.HasError("Code 'NNN' is the child of 'MMM' and cannot be setup as parent of 'PPP'."));
		}

		public void TestDescription()
		{
			BizObj.Description = (NoResString)"";
			BizObj.ValidateDescription();
			AssertHasError(BizObj.DescriptionInfo, "Please enter a Description.");

			BizObj.Description = (NoResString)"Description";
			AssertNoErrors(BizObj.DescriptionInfo);
		}

		public void TestClosing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				BizObjCollection = new GLPresentationJournalCategoryCollection(fallbackLevel);
				var bizObj = BizObjCollection.AddNew();
				bizObj.Code = "AU1";
				bizObj.Bool = true;
				bizObj.Bool3 = true;
				AssertNoErrors(bizObj.Bool3Info);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				BizObjCollection = new GLPresentationJournalCategoryCollection(fallbackLevel);
				var chinaBizObj = BizObjCollection.AddNew();
				chinaBizObj.Code = "CHN";
				chinaBizObj.Bool = true;
				chinaBizObj.Bool2 = false;
				chinaBizObj.Bool3 = true;
				AssertNoErrors(chinaBizObj.Bool3Info);

				var anotherBizObj = BizObjCollection.AddNew();
				anotherBizObj.Code = "abc";
				anotherBizObj.Bool = true;
				anotherBizObj.Bool2 = true;
				anotherBizObj.Bool4 = true;
				anotherBizObj.Bool3 = true;
				AssertHasErrors(anotherBizObj.Bool3Info);
				AssertHasErrorContaining(anotherBizObj.Bool3Info, "Only one Category can be used for Closing");
				AssertHasErrorContaining(anotherBizObj.Bool3Info, "Category used for Elimination can not be used for Closing");
				AssertHasErrorContaining(anotherBizObj.Bool3Info, "Category used for Opening can not be used for Closing");
			}
		}

		public void TestOpening()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				BizObjCollection = new GLPresentationJournalCategoryCollection(fallbackLevel);
				var bizObj = BizObjCollection.AddNew();
				bizObj.Code = "AU1";
				bizObj.Bool = true;
				bizObj.Bool4 = true;
				AssertNoErrors(bizObj.Bool4Info);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
				BizObjCollection = new GLPresentationJournalCategoryCollection(fallbackLevel);
				var chinaBizObj = BizObjCollection.AddNew();
				chinaBizObj.Code = "CHN";
				chinaBizObj.Bool = true;
				chinaBizObj.Bool2 = false;
				chinaBizObj.Bool3 = false;
				chinaBizObj.Bool4 = true;
				AssertNoErrors(chinaBizObj.Bool3Info);

				var anotherBizObj = BizObjCollection.AddNew();
				anotherBizObj.Code = "abc";
				anotherBizObj.Bool = true;
				anotherBizObj.Bool2 = true;
				anotherBizObj.Bool3 = true;
				anotherBizObj.Bool4 = true;
				AssertHasErrors(anotherBizObj.Bool4Info);
				AssertHasErrorContaining(anotherBizObj.Bool4Info, "Only one Category can be used for Opening");
				AssertHasErrorContaining(anotherBizObj.Bool4Info, "Category used for Elimination can not be used for Opening");
				AssertHasErrorContaining(anotherBizObj.Bool4Info, "Category used for Closing can not be used for Opening");
			}
		}

		#region Implementation

		GLPresentationJournalCategory BizObj;
		GLPresentationJournalCategoryCollection BizObjCollection;

		protected override void SetUp()
		{
			BizObjCollection = new GLPresentationJournalCategoryCollection();
			BizObj = BizObjCollection.AddNew();
		}

		#endregion
	}
}