using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Business.Testing
{
	class StmMenuItemBaseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSU_SignBy()
		{
			var documents = DocumentsDataRegistry.Instance.DocumentsAllowedForSigning.Value;
			documents.DocumentsAllowedForSigningItems.Add(new DocumentsAllowedForSigningItem("Jerry Test Document", DocumentSigningRegistryConstants.DocumentAllowedForSigningType.Yes));
			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.DocumentsAllowedForSigning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, documents))
			{
				var user = Factory.NewWithValidTestData<GlbStaff>();
				Factory.Save();

				using (Env.SetTemporaryUserContext(user.GS_LoginName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertEquals(false, Env.CurrentUser.IsSupportUser);

					MenuItem.SU_MenuName = "Jerry Test Document";
					MenuItem.SU_IsSystemDefined = true;
					MenuItem.SU_SignBy = "DOS";
					AssertNoErrors(MenuItem.SU_SignByInfo);

					MenuItem.SU_IsSystemDefined = false;
					MenuItem.SU_SignBy = "DOS";
					AssertHasError(MenuItem.SU_SignByInfo, "You cannot use the DOS service task for this document type.");

					MenuItem.SU_MenuName = "Jerry Test Other Document";
					MenuItem.SU_SignBy = "DOS";
					AssertHasError(MenuItem.SU_SignByInfo, "You cannot use the DOS service task for this document type.");
				}

				using (Env.SetTemporaryUserContext("CWSupport", Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					AssertEquals(true, Env.CurrentUser.IsSupportUser);

					MenuItem.SU_MenuName = "Jerry Test Document";
					MenuItem.SU_IsSystemDefined = true;
					MenuItem.SU_SignBy = "DOS";
					AssertNoErrors(MenuItem.SU_SignByInfo);

					MenuItem.SU_IsSystemDefined = false;
					MenuItem.SU_SignBy = "DOS";
					AssertNoErrors(MenuItem.SU_SignByInfo);

					MenuItem.SU_MenuName = "Jerry Test Other Document";
					MenuItem.SU_SignBy = "DOS";
					AssertNoErrors(MenuItem.SU_SignByInfo);
				}
			}
		}

		public void TestValidateSU_DocumentDirection()
		{
			MenuItem.SU_DocumentDirection = "123";
			Assert("Error expected when numeric entry", MenuItem.SU_DocumentDirectionInfo.HasErrors());

			MenuItem.SU_DocumentDirection = "NOT";
			Assert("Error expected when entry isn't from the document direction list", MenuItem.SU_DocumentDirectionInfo.HasErrors());
		}

		public void TestValidateSU_DraftOption()
		{
			MenuItem.SU_DraftOption = "NOT";
			Assert("Error expected when entry isn't from the draft option list", MenuItem.SU_DraftOptionInfo.HasErrors());
		}

		public void TestValidateSU_AddressCategory()
		{
			MenuItem.SU_AddressCategory = "#A#"; //Definitely it is not in the list
			Assert("Error expected when invalid entry", MenuItem.SU_AddressCategoryInfo.HasErrors());

			MenuItem.SU_AddressCategory = new OrgAddressCategory()[0].Code; //Valid Item from the list
			MenuItem.SU_AddressCategory = ""; //Then clear it
			MenuItem.SU_PreventAutoDelivery = false;
			Assert("Error expected when Null entry with no PreventAutoDelivery", MenuItem.SU_AddressCategoryInfo.HasErrors());

			#region If SU_PreventAutoDelivery is True, no matter it is updated after SU_AddressCategory or before
			MenuItem.SU_AddressCategory = "";
			MenuItem.SU_PreventAutoDelivery = true;
			Assert("Error not expected when Null entry with PreventAutoDelivery", !MenuItem.SU_AddressCategoryInfo.HasErrors());

			MenuItem.SU_PreventAutoDelivery = true;
			MenuItem.SU_AddressCategory = "";
			Assert("Error not expected when Null entry with PreventAutoDelivery", !MenuItem.SU_AddressCategoryInfo.HasErrors());
			#endregion
		}

		public void TestValidateSU_MenuType()
		{
			MenuItem.SU_MenuType = "";
			AssertHasError(MenuItem.SU_MenuTypeInfo, "Please enter a Menu Type.");

			MenuItem.SU_MenuType = "ZZZ";
			AssertHasError(MenuItem.SU_MenuTypeInfo, "Enter a valid Menu Type.");

			MenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			AssertNoErrors(MenuItem.SU_MenuTypeInfo);

			MenuItem.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			AssertNoErrors(MenuItem.SU_MenuTypeInfo);
		}

		public void TestValidateSU_MenuName()
		{
			MenuItem.SU_MenuName = "";
			AssertHasError(MenuItem.SU_MenuNameInfo, "Please enter a Menu Name.");
			MenuItem.SU_MenuName = "Something";
			AssertNoError(MenuItem.SU_MenuNameInfo, "Please enter a Menu Name.");
		}

		public void TestValidateSU_Hint()
		{
			MenuItem.SU_BusinessContext = "FakeContext";
			MenuItem.SU_IsSystemDefined = true;
			MenuItem.SU_MenuName = "Name";
			MenuItem.SU_MenuPath = "Path";
			MenuItem.SU_FilterList = "Filter1";
			MenuItem.SU_Hint = "";
			AssertNoErrors(MenuItem.SU_HintInfo);

			var anotherMenuItem = Factory.New<StmMenuItemBase>();
			anotherMenuItem.SU_BusinessContext = MenuItem.SU_BusinessContext;
			anotherMenuItem.SU_IsSystemDefined = MenuItem.SU_IsSystemDefined;
			anotherMenuItem.SU_MenuName = MenuItem.SU_MenuName;
			anotherMenuItem.SU_MenuPath = MenuItem.SU_MenuPath;
			anotherMenuItem.SU_FilterList = "Filter2";
			anotherMenuItem.SU_Hint = MenuItem.SU_Hint;
			AssertNoErrors(MenuItem.SU_HintInfo);
			AssertHasError(anotherMenuItem.SU_HintInfo, "At least one other document has the same name and menu path. Please enter a unique description.");

			MenuItem.SU_Hint = "Hint";
			anotherMenuItem.SU_Hint = "Hint2";
			AssertNoErrors(MenuItem.SU_HintInfo);
			AssertNoErrors(anotherMenuItem.SU_HintInfo);

			anotherMenuItem.SU_Hint = MenuItem.SU_Hint;
			AssertNoErrors(MenuItem.SU_HintInfo);
			AssertHasError(anotherMenuItem.SU_HintInfo, "At least one other document has the same name and menu path. Please enter a unique description.");

			var itemWithAnotherContext = Factory.New<StmMenuItemBase>();
			itemWithAnotherContext.SU_BusinessContext = "AnotherContext";
			itemWithAnotherContext.SU_IsSystemDefined = MenuItem.SU_IsSystemDefined;
			itemWithAnotherContext.SU_MenuName = MenuItem.SU_MenuName;
			itemWithAnotherContext.SU_MenuPath = MenuItem.SU_MenuPath;
			itemWithAnotherContext.SU_FilterList = MenuItem.SU_FilterList;
			itemWithAnotherContext.SU_Hint = MenuItem.SU_Hint;
			AssertNoErrors("Context is different. No error expected.", itemWithAnotherContext.SU_HintInfo);

			var nonSystemItem = Factory.New<StmMenuItemBase>();
			nonSystemItem.SU_BusinessContext = MenuItem.SU_BusinessContext;
			nonSystemItem.SU_IsSystemDefined = false;
			nonSystemItem.SU_MenuName = MenuItem.SU_MenuName;
			nonSystemItem.SU_MenuPath = MenuItem.SU_MenuPath;
			nonSystemItem.SU_FilterList = MenuItem.SU_FilterList;
			nonSystemItem.SU_Hint = MenuItem.SU_Hint;
			AssertHasError(nonSystemItem.SU_HintInfo, "At least one other document has the same name and menu path. Please enter a unique description.");
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			MenuItem = Factory.New<StmMenuItemBase>();
		}

		StmMenuItemBase MenuItem;
		#endregion
	}
}
