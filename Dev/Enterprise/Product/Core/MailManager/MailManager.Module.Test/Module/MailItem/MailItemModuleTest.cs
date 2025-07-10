using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.Module.Testing
{
	[TestedType(typeof(MailItemModule))]
	public class MailItemModuleTest : ZModuleBasherTest
	{
		public MailItemModuleTest() : base()
		{
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.MailItem;
		}

		public void TestCheckpoints()
		{
			using (MailItemModule module = new MailItemModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.Emails, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			}
		}

		public void TestRestrictions()
		{
			using (MailItemModule module = new MailItemModule())
			{
				AssertEquals("AllowEdit", false, module.AllowEdit);
				AssertEquals("AllowDelete", false, module.AllowDelete);
				// See TestMenuForNewItemOnlyVisibleToCargoWise() for AllowNew
			}
		}

		#region Properties
		public void TestGetNewFilterControl()
		{
			using (MailItemModuleForTest module = new MailItemModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is MailItemFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (MailItemModuleForTest module = new MailItemModuleForTest())
			{
				IBusinessObjectCollection collection = module.NewGridCollection;
				Assert("Invalid type", collection is MailItemCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (MailItemModuleForTest module = new MailItemModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is MailItemFilterBusinessObject);
			}
		}

		#endregion
		public void TestShowOnlySTDMailItems()
		{
			MailItem item1 = GetNewMailItem();
			item1.MI_Application = "STD";
			item1.MI_Subject = "STD Item";
			MailItem item2 = GetNewMailItem();
			item2.MI_Application = "CSV";
			item2.MI_Subject = "CSV Item";
			Factory.Save();
			using (MailItemModulePublicHelper module = new MailItemModulePublicHelper())
			{
				module.PublicPerformSearch();
				ZQuery query = new ZQuery(MailDBItemsSchema.MI_Application, "STD");
				query.AddToFilter(MailDBItemsSchema.MI_Subject, "STD Item");
				Assert("Should find records with MI_Application='STD'", module.PublicGridCollection().Find(query).Length != 0);
				query = new ZQuery(MailDBItemsSchema.MI_Application, "CSV");
				query.AddToFilter(MailDBItemsSchema.MI_Subject, "CSV Item");
				Assert("Should find records with MI_Application='CSV'", module.PublicGridCollection().Find(query).Length != 0);
			}
		}

		public void TestResetStatusTo()
		{
			MailItem item1 = GetNewMailItem();
			MailItem item2 = GetNewMailItem();
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (MailItemModuleTestHelper module = new MailItemModuleTestHelper())
			{
				module.ResetStatusTo(Array.Empty<BusinessObject>(), MailStatus.Queued);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please select one or more Emails to reset the Status.", UnitTestUserNotification.Instance.LastMessage.Text);
				BusinessObject[] selectedItems = new BusinessObject[] { item1, item2 };
				MailStatusAssignerTestHelper assigner = (MailStatusAssignerTestHelper)module.GetMailStatusAssigner(selectedItems, MailStatus.Queued);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				assigner.TestHasDBChanged = true;
				module.ResetStatusTo(selectedItems, MailStatus.Queued);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("One or more of the selected Emails has been changed. Please refresh the grid and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				assigner.TestHasDBChanged = false;
				assigner.TestErrors = "ERROR";
				module.ResetStatusTo(selectedItems, MailStatus.Queued);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Errors were encountered trying to reset the Status of Mail Item(s)." + System.Environment.NewLine + "ERROR", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				assigner.TestErrors = "";
				module.ResetStatusTo(selectedItems, MailStatus.Queued);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("No Emails changed Status to " + MailStatus.Queued + " from 2 selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				assigner.TestItemsAffected = 1;
				module.ResetStatusTo(selectedItems, MailStatus.Queued);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Reset Status to " + MailStatus.Queued + " for 1 Email(s) from 2 selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
				assigner.ThrowException = true;
				assigner.ExceptionMessage = "Some Exception";
				module.ResetStatusTo(selectedItems, MailStatus.Queued);
				AssertNotNull(ErrorReporter.LastExceptionReported);
				AssertEquals("Some Exception", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestSendCopyTo()
		{
			MailItem item1 = GetNewMailItem();
			MailItem item2 = GetNewMailItem();
			Factory.Save();
			BusinessObject[] selectedItems = new BusinessObject[] { item1, item2 };
			using (MailItemModuleNoDialog module = new MailItemModuleNoDialog())
			{
				module.SendCopyTo(Array.Empty<BusinessObject>());
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please select one or more Emails to send a copy.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.SendCopyTo(selectedItems);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error - MailAddressToSendCopyTo: Please enter a value.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (MailItemModuleTestHelper module = new MailItemModuleTestHelper())
			{
				MailItemCopySenderTestHelper copySender = (MailItemCopySenderTestHelper)module.GetMailItemCopySender(selectedItems);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				copySender.MailAddressToSendCopyTo = "av@edi.com";
				copySender.TestErrors = "";
				module.SendCopyTo(selectedItems);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Copies of 2 Email(s) were successfully sent to the av@edi.com address.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				copySender.TestErrors = "ERROR";
				module.SendCopyTo(selectedItems);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Errors were encountered trying to send copy of Email(s) to the av@edi.com address." + System.Environment.NewLine + "ERROR", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				copySender.TestErrors = "";
				ErrorReporter.Clear();
				copySender.ThrowException = true;
				copySender.ExceptionMessage = "Some Exception";
				module.SendCopyTo(selectedItems);
				AssertNotNull(ErrorReporter.LastExceptionReported);
				AssertEquals("Some Exception", ErrorReporter.LastExceptionReported.Message);
				ErrorReporter.Clear();
			}
		}

		public void TestMenuForNewItemOnlyVisibleToCargoWise()
		{
			using (var module = new MailItemModuleForTest())
			{
				GlbStaff.CurrentUser.GS_LoginName = "GeekyDev";
				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				AssertEquals("Module allows 'Make New' for devs", true, module.AllowNew);
				GlbStaff.CurrentUser.GS_LoginName = "Joe.Public";
				GlbStaff.CurrentUser.GS_IsDeveloper = false;
				AssertEquals("Module allows 'Make New' for ordinary user", false, module.AllowNew);
				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
				AssertEquals("Module allows 'Make New' for support user", true, module.AllowNew);
			}
		}

		#region Implementation
		class MailItemModuleNoDialog : MailItemModule
		{
			public override bool AssignMailAddressToSendCopy(MailItemCopySender copySender)
			{
				return true;
			}
		}

		class MailItemModuleTestHelper : MailItemModuleNoDialog
		{
			public override MailStatusAssigner GetMailStatusAssigner(BusinessObject[] selectedElements, string newStatus)
			{
				if (AssignerTestHelper == null)
				{
					AssignerTestHelper = new MailStatusAssignerTestHelper(selectedElements, newStatus);
				}

				return AssignerTestHelper;
			}

			public MailStatusAssignerTestHelper AssignerTestHelper;
			public override MailItemCopySender GetMailItemCopySender(BusinessObject[] selectedElements)
			{
				if (CopySenderTestHelper == null)
				{
					CopySenderTestHelper = new MailItemCopySenderTestHelper(Array.ConvertAll(selectedElements, el => (MailItem)el));
				}

				return CopySenderTestHelper;
			}

			public MailItemCopySenderTestHelper CopySenderTestHelper;
		}

		class MailStatusAssignerTestHelper : MailStatusAssigner
		{
			public MailStatusAssignerTestHelper(BusinessObject[] selectedElements, string newStatus) : base(selectedElements, newStatus)
			{
				TestErrors = "";
				TestHasDBChanged = false;
				TestItemsAffected = 0;
				ThrowException = false;
				ExceptionMessage = "";
			}

			public override string Errors
			{
				get
				{
					return TestErrors;
				}
			}

			public string TestErrors;
			public override bool HasDBChanged
			{
				get
				{
					return TestHasDBChanged;
				}
			}

			public bool TestHasDBChanged;
			public override int ItemsAffected
			{
				get
				{
					return TestItemsAffected;
				}
			}

			public int TestItemsAffected;
			public bool ThrowException;
			public string ExceptionMessage;
			public override void Assign()
			{
				if (ThrowException)
				{
					throw new Exception(ExceptionMessage);
				}
			}
		}

		class MailItemCopySenderTestHelper : MailItemCopySender
		{
			public MailItemCopySenderTestHelper(MailItem[] items) : base(items)
			{
				TestErrors = "";
				ThrowException = false;
				ExceptionMessage = "";
			}

			public override ZString Errors
			{
				get
				{
					return TestErrors;
				}
			}

			public ZString TestErrors;
			public bool ThrowException;
			public string ExceptionMessage;
			public override void SendCopyTo()
			{
				if (ThrowException)
				{
					throw new Exception(ExceptionMessage);
				}
			}
		}

		protected MailItem GetNewMailItem()
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Direction = MailDirection.Transmit;
			item.MI_Status = MailStatus.Sent;
			item.MI_SendDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			return item;
		}

		class MailItemModulePublicHelper : MailItemModule
		{
			public void PublicPerformSearch()
			{
				base.PerformSearch();
			}

			public BusinessObjectCollection PublicGridCollection()
			{
				return (BusinessObjectCollection)base.GridCollection;
			}
		}

		#region MailItemModuleForTest
		public class MailItemModuleForTest : MailItemModule
		{
			public MailItemModuleForTest()
			{
			}

			public IFilterControl NewFilterControl
			{
				get
				{
					return GetNewFilterControl();
				}
			}

			public IBusinessObjectCollection NewGridCollection
			{
				get
				{
					return GetNewGridCollection();
				}
			}

			public FilterBusinessObject NewFilterBusinessObject
			{
				get
				{
					return GetNewFilterBusinessObject();
				}
			}
		}
		#endregion
		#endregion
	}
}
