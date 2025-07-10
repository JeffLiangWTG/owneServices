using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DocumentPreviewFormLayoutRegistryItem))]
	sealed class DocumentPreviewFormLayoutRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<DocumentPreviewFormLayout>
	{
		public void TestStorageAndOptions()
		{
			AssertEquals("RegistryItem should apply to the Company level only.", RegistryStorageFlags.Company, RegistryItem.Storage);
			AssertEquals("RegistryItem should be hidden.", RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue, RegistryItem.Options);
		}

		public void TestUserValue()
		{
			AssertEquals("IsSetByCurrentUser", false, RegistryItem.IsSetByCurrentUser);

			RegistryItem.UserValue = new DocumentPreviewFormLayout();
			AssertEquals("IsSetByCurrentUser", true, RegistryItem.IsSetByCurrentUser);

			RegistryItem.UserValue = new DocumentPreviewFormLayout(true, 987, 654);

			Type glbStaffType = ObjectFactory.GetType<IGlbStaff>();
			BusinessObject glbStaff = Factory.NewWithValidTestData(glbStaffType);
			glbStaff[GlbStaffSchema.Constants.GS_LoginName] = new ZString("BobTheBuilder");
			Factory.Save();

			string loginNameBeforeSwitch = Env.CurrentUser.LoginName;

			using (CurrentUserChanger.SwitchToNewUserTemporarily("BobTheBuilder"))
			{
				AssertEquals("Env.CurrentUser.LoginName", "BobTheBuilder", Env.CurrentUser.LoginName);
				AssertEquals("IsSetByCurrentUser", false, RegistryItem.IsSetByCurrentUser);

				RegistryItem.UserValue = new DocumentPreviewFormLayout(true, 123, 456);
				AssertEquals("UserValue.IsNotSetByUser", true, RegistryItem.IsSetByCurrentUser);
				AssertEquals("UserValue.IsThumbnailPanelVisible", true, RegistryItem.UserValue.IsThumbnailPanelVisible);
				AssertEquals("UserValue.ThumbnailPanelPixelWidth", 123, RegistryItem.UserValue.ThumbnailPanelPixelWidth);
				AssertEquals("UserValue.ZoomValue", 456, RegistryItem.UserValue.ZoomValue);
			}

			AssertEquals("Env.CurrentUser.LoginName", loginNameBeforeSwitch, Env.CurrentUser.LoginName);
			AssertEquals("UserValue.IsNotSetByUser", true, RegistryItem.IsSetByCurrentUser);
			AssertEquals("UserValue.IsThumbnailPanelVisible", true, RegistryItem.UserValue.IsThumbnailPanelVisible);
			AssertEquals("UserValue.ThumbnailPanelPixelWidth", 987, RegistryItem.UserValue.ThumbnailPanelPixelWidth);
			AssertEquals("UserValue.ZoomValue", 654, RegistryItem.UserValue.ZoomValue);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			RegistryItem = new DocumentPreviewFormLayoutRegistryItem("");
		}

		protected override StronglyTypedRegistryItem<DocumentPreviewFormLayout, DocumentPreviewFormLayout> GetNewRegistryItem()
		{
			return new DocumentPreviewFormLayoutRegistryItem("");
		}

		protected override FallbackLevel Fallback
		{
			get { return new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		DocumentPreviewFormLayoutRegistryItem RegistryItem;

		#endregion
	}
}
