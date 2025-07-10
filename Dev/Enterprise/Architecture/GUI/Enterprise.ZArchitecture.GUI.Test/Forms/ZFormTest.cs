using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Interop;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.DevTools;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using static System.Windows.Forms.Control;
using MethodInvoker = System.Windows.Forms.MethodInvoker;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.GUI.Testing
{
	class ZFormWithoutTransactionTest : TestCase
	{
		public void TestProcessTemplateValidationManager_RejectFieldRules()
		{
			var count = 0;
			var factory = new BusinessObjectFactory();
			var mock = new Mock<IProcessTemplateValidationManager>();
			mock.Setup(x => x.InjectFieldRules(It.IsAny<IBusiness>())).Callback(() =>
			{
				count++;
			});
			using var substitute = ObjectFactory.Substitute(mock.Object);
			using var form = new TestZForm(Mock.Of<IBusiness>(x => x.Factory == factory));
			form.Show();
			Application.DoEvents();
			AssertEquals(1, count);
		}

		public void TestProcessTemplateValidationManager_OnValidateAll()
		{
			var count = 0;
			var factory = new BusinessObjectFactory();
			var mock = new Mock<IProcessTemplateValidationManager>();
			mock.Setup(x => x.ValidateOnValidateAll(It.IsAny<IBusiness>())).Callback(() =>
			{
				count++;
			});
			mock.Setup(x => x.Validate(It.IsAny<ZString>(), It.IsAny<IBusiness>(), It.IsAny<Action>()))
				.Callback((ZString _, IBusiness _, Action originalAction) => { originalAction(); });
			using var substitute = ObjectFactory.Substitute(mock.Object);
			using var form = new TestZForm(Mock.Of<IBusiness>(x => x.Factory == factory));
			form.FireValidateAllForTest();
			AssertEquals(1, count);
		}

		public void TestProcessTemplateValidationManager_OnSave()
		{
			var count = 0;
			var mock = new Mock<IProcessTemplateValidationManager>();
			mock.Setup(x => x.ValidateOnSave(It.IsAny<IBusiness>(), It.IsAny<Action>())).Callback(() =>
			{
				count++;
			});
			using var substitute = ObjectFactory.Substitute(mock.Object);
			using var form = new TestZForm(Mock.Of<IBusiness>());
			form.FireSaveButton();
			AssertEquals(1, count);
		}

		[UseSnapshotProtection]
		public void TestReloadFormWhenDisplayModeIsNewSaved()
		{
			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				var factory = new BusinessObjectFactory();
				var stmServiceTask = factory.New<IStmServiceTask>() as BusinessObject;
				stmServiceTask.FillWithValidTestData();
				factory.Save();

				var controller = ZControllerFactory.Create(ControllerIDs.StmServiceTask);
				using (var form = controller.ShowEditForm(stmServiceTask))
				{
					AssertEquals("the display mode should be NewSaved", ODisplayMode.NewSaved, form.DisplayMode);

					ZFormUtilities.ReloadCurrentForm(form as ZForm);
					var openedForms = Application.OpenForms.OfType<ZForm>();
					AssertEquals(1, openedForms.Count());

					var openedForm = openedForms.First();
					AssertEquals("the display mode should be NewSaved as original.", ODisplayMode.NewSaved,
						openedForm!.DisplayMode);
					openedForm.Close();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestSetVisibleCore_WithClosedForm()
		{
			var testForm = new ZForm();
			testForm.Close();
			((IZForm)testForm).Show();
		}

		public void TestCodeDomSerializer()
		{
			var attr = (DesignerSerializerAttribute)TypeDescriptor.GetAttributes(typeof(ZForm))[typeof(DesignerSerializerAttribute)];
			AssertEquals("Forms should use " + nameof(ControlCodeDomSerializerWithDelayedTabCreate) + " for runtime performance", true, attr.SerializerTypeName.StartsWith(typeof(ControlCodeDomSerializerWithDelayedTabCreate).FullName));
		}

		[UseSnapshotProtection]
		public void TestEditingFormTwiceShowsAccessDialog()
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			factory.Save();

			using (var provider = new SemaphoreProviderWithMockUserIdForTesting(Guid.NewGuid()))
			using (var semaphoreHandle = ((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(bizO.PK.ToString())))
			{
				var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
				using (var form = controller.ShowEditForm(bizO))
				{
					Application.DoEvents();
				}
			}

			Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("These users are currently modifying ZDummyForm or one of its dependent objects:\r\nZDummyForm"));
			Assert(UnitTestUserNotification.Instance.LastMessage.Text.Trim().EndsWith("Your modifications may not be able to be saved if the other user saves first (times shown in your local time zone)."));
		}

		[UseSnapshotProtection]
		public void TestEditingDependantObjectViaTerminalSessionDoesNotShowAccessDialog()
		{
			var provider = new SemaphoreProviderWithCurrentUserForTesting();
			provider.RemoteClientName = "REMOTE_MACHINE_NAME";
			{
				var factory = new BusinessObjectFactory();
				var bizO = factory.New<DummyBusinessObject>();
				var dependent1 = factory.New<DummyBusinessObject>();
				factory.Save();

				using (var semaphoreHandle1 = ((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(dependent1.PK.ToString())))
				{
					var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
					(controller as DummyController).DependentObjects.Add(dependent1, "blabla1");

					using (var form = (ZDummyForm)controller.ShowEditForm(bizO))
					{
						form.IsAnotherUserEditing_CompareUserIdOnly = false;
						form.SemaphoreProvider_Exposed = provider;
						Application.DoEvents();
					}
				}

				AssertNull("No message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		public void TestEditingFormTwiceInSameInstanceShowAccessDialog()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			factory.Save();

			using (var semaphoreHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(new PendingUserActionSemaphore(bizO.PK.ToString())))
			{
				var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
				using (var form = controller.ShowEditForm(bizO))
				{
					Application.DoEvents();
				}
			}

			Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("You are currently modifying ZDummyForm in another form. You may be able to save modifications from only one form."));
		}

		[UseSnapshotProtection]
		public void TestEditingFormFromSemaphoreParentDoesNotShowAccessDialog()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			factory.Save();
			var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
			using (var parentForm = (ZForm)controller.ShowEditForm(bizO))
			{
				using (ZForm childForm = new ZDummyForm(bizO))
				{
					ZFormModaliser.Show(childForm, parentForm);
					Application.DoEvents();
				}
			}

			AssertNull("No message should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[UseSnapshotProtection]
		public void TestEditingFormTwiceShowsAccessDialogForEachDependentObject()
		{
			AssertEditingFormTwiceShowsAccessDialogForEachDependentObject(Guid.NewGuid(), true);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEditingFormTwiceShowsAccessDialogForEachDependentObject(EnvProxy.Instance.CurrentUser.PK, false);
		}

		void AssertEditingFormTwiceShowsAccessDialogForEachDependentObject(Guid user, bool showsMessage)
		{
			var factory = new BusinessObjectFactory();
			var bizO = factory.New<DummyBusinessObject>();
			var dependent1 = factory.New<DummyBusinessObject>();
			var dependent2 = factory.New<DummyBusinessObject>();
			factory.Save();

			using (var provider = new SemaphoreProviderWithMockUserIdForTesting(user))
			using (var semaphoreHandle1 = ((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(dependent1.PK.ToString())))
			using (var semaphoreHandle2 = ((ISemaphoreProvider)provider).CreateSemaphoreHandle(new PendingUserActionSemaphore(dependent2.PK.ToString())))
			{
				var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
				(controller as DummyController).DependentObjects.Add(dependent1, "blabla1");
				(controller as DummyController).DependentObjects.Add(dependent2, "blabla2");
				using (var form = controller.ShowEditForm(bizO))
				{
					Application.DoEvents();
				}
			}

			if (showsMessage)
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("blabla1"));
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("blabla2"));
			}
			else
			{
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestNoDeadLockWhenSavingFromMultiThreads()
		{
			GC.Collect();
			Db.Connection.ExecuteNonQuery("DELETE FROM dbo.DummyBizo");

			const int NUM = 20;
			var index = 0;
			var pkList = new List<ZGuid>();

			for (index = 0; index < NUM; index++)
			{
				var factory = new BusinessObjectFactory();
				var dummy = factory.New<DummyBusinessObject>();
				dummy.Z0_Number = 100;
				factory.Save();
				pkList.Add(dummy.PK);
			}

			var threads = new Thread[NUM];
			var errMsgs = new string[NUM];

			for (index = 0; index < NUM; index++)
			{
				var value = index;
				threads[value] = new Thread(() => ThreadFunc(pkList, value, ref errMsgs[value]));
				threads[value].Start();
			}

			for (index = 0; index < NUM; index++)
			{
				threads[index].Join();
			}
		}

		void ThreadFunc(List<ZGuid> pkList, int index, ref string errMsg)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var nUM = pkList.Count;

				try
				{
					using (var form = new ZForm())
					{
						var factory = new BusinessObjectFactory();
						var dummyList = new List<DummyBusinessObject>();

						for (var i = 0; i < nUM; i++)
						{
							var dummy = factory.Load<DummyBusinessObject>(pkList[i]);
							dummyList.Add(dummy);

							if (dummyList[i].Z0_Number != 100)
							{
								errMsg += "At the beginning, in thread " + index + ", the Z0_Number of dummy object " + i + " = " + dummyList[i].Z0_Number + "\n";
							}
						}

						// timer to control the modification of one object: modify the object when dialogs in all threads are showing
						var timerModify = new System.Windows.Forms.Timer();
						string modificationErrMsg = null;

						timerModify.Tick += delegate
						{
							try
							{
								dummyList[index].Z0_Number = index;
								factory.Save();
							}
							catch (Exception ex)
							{
								modificationErrMsg = "In timerSave.Tick (the thread id = " + Thread.CurrentThread.ManagedThreadId + ") Thread " + index + " caught an exception: " + ex.ToString() + "\n";
							}
						};

						timerModify.Interval = 10000;

						// timer to control disposing the form: after all threads have finished the modification of objects
						var timerDisposeForm = new System.Windows.Forms.Timer();
						string disposeFormErrMsg = null;

						timerDisposeForm.Tick += delegate
						{
							try
							{
								form.Dispose();
							}
							catch (Exception ex)
							{
								disposeFormErrMsg = "In timerClose.Tick (the thread id = " + Thread.CurrentThread.ManagedThreadId + ") Thread " + index + " caught an exception: " + ex.ToString() + "\n";
							}
						};

						timerDisposeForm.Interval = 30000;

						timerModify.Start();
						timerDisposeForm.Start();

						form.ShowDialog();

						timerModify.Dispose();
						timerDisposeForm.Dispose();

						if (modificationErrMsg != null)
						{
							errMsg += modificationErrMsg;
						}

						if (disposeFormErrMsg != null)
						{
							errMsg += disposeFormErrMsg;
						}

						for (var i = 0; i < nUM; i++)
						{
							if (dummyList[i].Z0_Number != i)
							{
								errMsg += "In thread " + index + ", the Z0_Number of dummy object " + i + " = " + dummyList[i].Z0_Number + "\n";
							}
						}
					}
				}
				catch (Exception ex)
				{
					errMsg += "Thread " + index + " caught an exception: " + ex.ToString() + "\n";
				}

				if (!errMsg.IsNullOrEmpty())
				{
					errMsg = "\nIn ThreadFunc, the thread id = " + Thread.CurrentThread.ManagedThreadId + "\n" + errMsg;
				}
			}
		}

		[UseSnapshotProtection]
		public void TestExceptionShouldBeReportedIfSemaphoreDisposedFailedWithException()
		{
			try
			{
				var factory = new BusinessObjectFactory();
				var bizO = factory.New<DummyBusinessObject>();
				var dependent = factory.New<DummyBusinessObject>();
				factory.Save();

				var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
				(controller as DummyController).DependentObjects.Add(dependent, "blabla");

				AssertNoExceptionThrown(() =>
				{
					using (var form = controller.ShowEditForm(bizO))
					{
						Application.DoEvents();
						Db.ConnectionOverrideForTest = Db.NewExtraConnection(Db.Connection.ServerName, Db.Connection.CurrentDatabase, userLogin: "dummy", userPassword: "dummy");
					}
				});

				Assert("Should show correct report", ErrorReporter.LastMessageReported.Contains("Error occurred while deleting semaphore handle from database."));
			}
			finally
			{
				Db.ConnectionOverrideForTest = null;
				ErrorReporter.Clear();
			}
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZForm.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZForm)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZForm).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}
	}

	[DoNotAddToTestTree]
	public class MockTestForTestTargetPlatformAttribute : TestCase
	{
		public void TestZFormLeakage()
		{
			if (!CrossPlatformTest.RunningOnOriginalPlatform)
			{
				var form = new ZForm();
			}

			Assert(true);
		}
	}

	public class ZFormTest : TestCaseWithDummy
	{
#if !WINZOR
		public void TestAccessibleObjectsAreCleanedUpOnDispose()
		{
			ControlAccessibleObject ao = null;
			using (var form = new ZForm(Dummy))
			{
				ao = form.AccessibilityObject as ControlAccessibleObject;
				AssertEquals(form, ao.Owner);
			}
			AssertEquals(null, ao.Owner);
		}
#endif

#if !WINZOR
		public void TestTryToActivateOwnerFormIfIsRemoteAppSessionWhenOwnerFormWasOpenedByZFormModaliser()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: true));
			var parentBizO = Factory.New<DummyBusinessObject>();
			using (var parentForm = new ZForm(parentBizO))
			{
				parentForm.Show();
				Assert("ParentForm should get focused", parentForm.Focused);

				var childBizO = Factory.New<DummyBusinessObject>();
				using (var childForm = new ZForm(childBizO))
				{
					ZFormModaliser.Show(childForm, parentForm);

					Assert("ParentForm should lose focused", !parentForm.Focused);
					AssertEquals("Owner should be ParentForm", parentForm, childForm.Owner);
					AssertEquals("ParentFormInZFormModaliser should be parentForm", parentForm, childForm.ParentFormInZFormModaliser);

					childForm.Close();

					Assert("ParentForm should get focused", parentForm.Focused);
					AssertNull("Owner should be set to null", childForm.Owner);
					AssertEquals("ParentFormInZFormModaliser should be still parentForm", parentForm, childForm.ParentFormInZFormModaliser);
				}

				Assert("Owner form should get focused", parentForm.Focused);
			}
		}
#endif

		public void TestLastManualText()
		{
			var dataSource = Factory.NewWithValidTestData<DummyBusinessObjectWithIsInDatabaseReadOnly>();
			using (var form = new TestZForm(dataSource))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				form.Text = "asdf";
				AssertEquals("asdf", form.Text);
				form.SuspendDrawing();
				AssertEquals("asdf", form.Text);
				form.Dispose();
				AssertEquals("asdf", form.Text);
				form.Text = "fghj";
				AssertEquals("fghj", form.Text);
				form.Text = "";
				AssertEquals("", form.Text);
			}
		}

		public void TestReloadFormMenuItem_ShouldReOpenFormProperly()
		{
			new ZFormUtilitiesTest().AssertReloadFormMenuItem_WorksWithGivenReloadMethod(reloadThisForm);

			void reloadThisForm(ZForm form) => form.ReloadForm();
		}

		public void TestFireSaveButton_ShouldSaveAsIfDisplayModeIsEdit_ShouldNotModifyFormDisplayMode()
		{
			var displayMode = ODisplayMode.Browse;
			using (var form = new ZForm(Dummy))
			{
				form.DisplayMode = displayMode;
				Assert("Our form should not have succesfully saved yet, however...", !form.LastSaveSucceeded);
				form.FireSaveButton();
				Assert("Our form should have succesfully saved, and yet...", form.LastSaveSucceeded);
				AssertEquals("Our form should not have had its DisplayMode modified from " + displayMode + ", and yet it is now " + form.DisplayMode, ODisplayMode.Browse, form.DisplayMode);
			}

			displayMode = ODisplayMode.Delete;
			using (var form = new ZForm(Dummy))
			{
				form.DisplayMode = displayMode;
				Assert("Our form should not have succesfully saved yet, however...", !form.LastSaveSucceeded);
				form.FireSaveButton();
				Assert("Our form should have succesfully saved and not deleted, and yet...", form.LastSaveSucceeded);
				AssertEquals("Our form should not have had its DisplayMode modified from " + displayMode + ", and yet it is now " + form.DisplayMode, ODisplayMode.Browse, form.DisplayMode);
			}

			displayMode = ODisplayMode.Undefined;
			using (var form = new ZForm(Dummy))
			{
				form.DisplayMode = displayMode;
				Assert("Our form should not have succesfully saved yet, however...", !form.LastSaveSucceeded);
				form.FireSaveButton();
				Assert("Our form should have succesfully saved, and yet...", form.LastSaveSucceeded);
				AssertEquals("Our form should not have had its DisplayMode modified from " + displayMode + ", and yet it is now " + form.DisplayMode, ODisplayMode.Browse, form.DisplayMode);
			}

			displayMode = ODisplayMode.Edit;
			using (var form = new ZForm(Dummy))
			{
				form.DisplayMode = displayMode;
				Assert("Our form should not have succesfully saved yet, however...", !form.LastSaveSucceeded);
				form.FireSaveButton();
				Assert("Our form should have succesfully saved, and yet...", form.LastSaveSucceeded);
				AssertEquals("Our form should not have had its DisplayMode modified from " + displayMode + ", and yet it is now " + form.DisplayMode, ODisplayMode.Browse, form.DisplayMode);
			}

			displayMode = ODisplayMode.New;
			using (var form = new ZForm(Dummy))
			{
				form.DisplayMode = displayMode;
				Assert("Our form should not have succesfully saved yet, however...", !form.LastSaveSucceeded);
				form.FireSaveButton();
				Assert("Our form should have succesfully saved, and yet...", form.LastSaveSucceeded);
				AssertEquals("Our form should not have had its DisplayMode modified from " + displayMode + ", and yet it is now " + form.DisplayMode, ODisplayMode.Browse, form.DisplayMode);
			}
		}

		public void TestBindingIsRefreshedAfterAFormIsSaved()
		{
			var dataSource = Factory.NewWithValidTestData<DummyBusinessObjectWithIsInDatabaseReadOnly>();
			using (var form = new TestZForm(dataSource))
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				textBox.SetDataBinding(dataSource, "Z0_Code");

				form.DisplayMode = ODisplayMode.New;
				form.Show();
				AssertEquals("Not read only", false, textBox.ReadOnly);

				dataSource.Z0_Code = "AAA";
				form.PerformApply();
				AssertEquals(true, dataSource.IsInDatabase);
				AssertEquals("Is read only after the datasource was saved", true, textBox.ReadOnly);

				form.Controls.Remove(textBox);
			}
		}

		class DummyBusinessObjectWithIsInDatabaseReadOnly : DummyBusinessObject
		{
			public DummyBusinessObjectWithIsInDatabaseReadOnly(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ReadOnlyMember(nameof(IsInDatabase))]
			public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }
		}

		public void TestRegisterControlToBeBoundOnPreSaveValidation()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizO))
			{
				var control1 = new DummyBindableControl();
				var control2 = new DummyBindableControl();
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new ZTabPage { Name = "Test1" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test2" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test3" });
				var tabPage1 = tabControl.GetTabPage("Test2");
				tabPage1.Controls.Add(control1);
				var tabPage2 = tabControl.GetTabPage("Test3");
				tabPage2.Controls.Add(control2);
				form.Controls.Add(tabControl);

				form.Show();
				AssertEquals("Precondition:", 0, control1.SetDataBindingHitCount);
				AssertEquals("Precondition:", 0, control2.SetDataBindingHitCount);

				form.RegisterControlToBeBoundOnPreSaveValidation(control1);
				form.RegisterControlToBeBoundOnPreSaveValidation(control1); // call Register twice to make sure it doesn't actually add it twice
				form.RegisterControlToBeBoundOnPreSaveValidation(control2);
				form.RegisterControlToBeBoundOnPreSaveValidation(control2); // call Register twice to make sure it doesn't actually add it twice
				control2.FireAfterFirstBinding();

				form.FireSaveButton();

				AssertEquals(1, control1.SetDataBindingHitCount);
				AssertEquals(0, control2.SetDataBindingHitCount);

				form.FireSaveButton();
				AssertEquals(1, control1.SetDataBindingHitCount);
				AssertEquals(0, control2.SetDataBindingHitCount);
			}
		}

		public void TestRegisterControlToBeBoundOnPreSaveValidation_ControlDisposed()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizO))
			{
				var control1 = new DummyBindableControl();
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new ZTabPage { Name = "Test1" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test2" });
				var tabPage2 = tabControl.GetTabPage("Test2");
				tabPage2.Controls.Add(control1);
				form.Controls.Add(tabControl);

				form.Show();

				form.RegisterControlToBeBoundOnPreSaveValidation(control1);
				tabControl.Controls.Remove(tabPage2);
				tabPage2.Dispose();

				AssertNoExceptionThrown("Disposed controls should not be bound on save", () => form.FireSaveButton());
			}
		}

		public void TestDisposeIsFinalizedWhenUnableToDisposeSemaphore()
		{
			var form = new ZForm();
			var mockSemaphoreHandle = new Mock<ISemaphoreHandle>();
			form.pendingUserActionSemaphoreHandle = mockSemaphoreHandle.Object;

			var sqlError = SqlExceptionBuilder.CreateSqlError(0, 0, 0, "srv", "A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - The semaphore timeout period has expired.)", "RemoveSemaphoreHandle", 123);

			mockSemaphoreHandle.Setup(o => o.Dispose()).Throws(SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(sqlError)));
			form.Show();

			try
			{
				form.Dispose();
			}
			catch { } // Something handles the exception and displays a message to the user
			Assert("Form should be disposed!", form.IsDisposed);
		}

		public void TestDisposeActivatesOwner()
		{
			using var form1 = new ZForm();
			using var form2 = new ZForm();
			using var form3 = new ZFormWithRemoteAppSession();

			form1.Show();
			form2.Show();

			form3.Owner = form1;
			form3.IsRemoteAppSessionOverride = true;
			form3.Show();

			Application.DoEvents();

			form2.Activate();
			form3.Close();

			Application.DoEvents();

			Assert("Owner form should get focused", form1.Focused);
		}

		public void TestDisposeOwnerDoesNotCycle()
		{
			using var form1 = new ZFormWithRemoteAppSession();
			using var form2 = new ZFormWithRemoteAppSession();

			form1.IsRemoteAppSessionOverride = true;
			form1.Show();

			form2.Owner = form1;
			form2.IsRemoteAppSessionOverride = true;
			form2.Show();

			Application.DoEvents();

			form1.Close();

			Application.DoEvents();

			Assert("Forms should be closed", !form1.Visible);
			Assert("Forms should be closed", !form2.Visible);
			Assert("Forms should be destroyed", !form1.IsHandleCreated);
			Assert("Forms should be destroyed", !form2.IsHandleCreated);
			Assert("Forms should be disposed", form1.IsDisposed);
			Assert("Forms should be disposed", form2.IsDisposed);
		}

		class ZFormToSimulateFinalizerDispose : ZForm
		{
			protected override bool AllowDisposeError_ForTest => true;

			protected override void Dispose(bool disposing)
			{
				base.Dispose(false);
			}
		}

		public void TestDispose_ReportsErrorWhenDisposingIsFalse()
		{
			ErrorReporter.Clear();

			var form = new ZFormToSimulateFinalizerDispose();
			DisposableLeakListener.Instance.Clear();
			form.Dispose();
			AssertEquals("Error is reported", "Form Enterprise.Core.GUI.Testing.ZFormTest+ZFormToSimulateFinalizerDispose disposed by finalizer", ErrorReporter.LastExceptionReported?.Message);

			ErrorReporter.Clear();
		}

		public void TestHidingFormDoesNotMakeItTopMost()
		{
			using var form = new ZFormWithTopMostCheck();

			form.IsRemoteAppSessionOverride = true;
			form.Show();
			Application.DoEvents();

			form.WasSetTopMost = false;
			form.Visible = false;
			Application.DoEvents();

			Assert("Should not be set TopMost during closing", !form.WasSetTopMost);
		}

		public void TestDisposeDoesNotSetVisible()
		{
			using var form = new ZFormWithSetVisibleCoreCheck();

			form.IsRemoteAppSessionOverride = true;
			form.Show();
			Application.DoEvents();

			form.Close();
			Application.DoEvents();

			Assert("Should not be set visible while being disposed", !form.WasSetVisibleWhileDisposing);
			Assert("Should not be set visible after being disposed", !form.WasSetVisibleWhileDisposed);
		}

		public void TestNoRecursiveDispose()
		{
			using var form = new ZFormWithRecursiveDispose();
			form.Show();
			form.Close();

			Assert("Form should have been disposed", form.IsDisposed);
			Assert("Disposing should have finished", !form.IsDisposing);
		}

		class ZFormWithRecursiveDispose : ZForm
		{
			protected override void UnbindControls()
			{
				if (IsDisposing)
				{
					// Recursive call
					Dispose();
				}
			}
		}

		class ZFormWithRemoteAppSession : ZForm
		{
			public bool IsRemoteAppSessionOverride { get; set; }

#if !WINZOR
			protected override bool IsRemoteAppSession => IsRemoteAppSessionOverride;
#endif
		}

		class ZFormWithTopMostCheck : ZFormWithRemoteAppSession
		{
			public bool WasSetTopMost { get; set; }

			protected override void SetTopMost(bool value)
			{
				WasSetTopMost |= value;

				base.SetTopMost(value);
			}
		}

		class ZFormWithSetVisibleCoreCheck : ZFormWithRemoteAppSession
		{
			public bool WasSetVisibleWhileDisposing { get; private set; }
			public bool WasSetVisibleWhileDisposed { get; private set; }

			protected override void SetVisibleCore(bool value)
			{
				if (value)
				{
					WasSetVisibleWhileDisposing |= IsDisposing;
					WasSetVisibleWhileDisposed |= IsDisposed;
				}

				base.SetVisibleCore(value);
			}
		}

		class DummyBindableControl : Control, IBoundOnPreSaveValidation, IDataBoundControl
		{
			public DummyBindableControl()
			{
				this.SetBindingMember(".");
			}

			public string DataMember
			{
				get { return Member; }
			}

			public object DataSource
			{
				get { return Data; }
			}

			public Type DataSourceType
			{
				get { return typeof(DummyBusinessObject); }
			}

			public void SetDataBinding(object dataSource, string dataMember)
			{
				SetDataBindingHitCount++;
				Data = dataSource;
				Member = dataMember;

				FireAfterFirstBinding();
			}

			object Data;
			string Member;

			public int SetDataBindingHitCount { get; private set; }

			public void FireAfterFirstBinding()
			{
				if (AfterFirstBinding != null)
				{
					AfterFirstBinding(this, EventArgs.Empty);
				}
			}

			public event EventHandler AfterFirstBinding;
		}

		public void TestRestoreOriginalIsRootPropertyValueOnBoundBusinessObject_NotMarkedAsTopLevel()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.IsTopLevel = false;

			using (var form = new ZForm(dummy))
			{
				form.Show();

				AssertEquals("dummy should be marked as IsTopLevel", true, dummy.IsTopLevel);
			}

			AssertEquals("dummy should not be marked as IsTopLevel", false, dummy.IsTopLevel);
		}

		public void TestRestoreOriginalIsRootPropertyValueOnBoundBusinessObject_MarkedAsTopLevel()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.IsTopLevel = true;

			using (var form = new ZForm(dummy))
			{
				form.Show();

				AssertEquals("dummy should be marked as IsTopLevel", true, dummy.IsTopLevel);
			}

			AssertEquals("dummy should remain marked as IsTopLevel", true, dummy.IsTopLevel);
		}

		public void UserEventReference()
		{
			using (var form = new ZForm() { Text = "Blaticus" })
			{
				AssertEquals("Caption = \"Blaticus\"", UserEventDiagnosticReferenceAttribute.Render(form));
			}
		}

		public void TestResetFormSizeToDefaultMenuItem()
		{
			var resetMenuItem = GetMenuItem(BoundForm.ActionsMenuItem.MenuItems, "Reset Form Size and Layout to Default");
			AssertNotNull("Menu Item Not found", resetMenuItem);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			resetMenuItem.PerformClick();
			AssertEquals("After you close and re-open this form, default form layout will be restored.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDeleteCallsDeleteCore()
		{
			BoundForm.Delete();
			AssertEquals("DeleteCoreCallCount", 1, BoundForm.DeleteCoreCallCount);
		}

		public void TestBusinessEntityForHasChanges()
		{
			using (ZTestForm testForm = new HasChangesTestForm(Dummy))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				Dummy.Z0_Description = "Test!!";
				AssertEquals("DisplayMode", ODisplayMode.New, testForm.DisplayMode);

				Dummy.Collection.AddNew().Z0_Description = "Hi";
				AssertEquals("DisplayMode", ODisplayMode.Edit, testForm.DisplayMode);
			}
		}

		[ExpectNoExceptions()]
		public void TestFireSaveButtonDoesNotThrowException()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			Factory.RefreshEnabled = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);

			dummy1.Z0_Decimal = 5;
			dummy2.Z0_Decimal = 10;

			using (var testForm = new ZForm(dummy2))
			{
				testForm.Show();
				Factory.Save(); // make original version in DB not match original version in form's factory

				try
				{
					testForm.FireSaveButton();
				}
				catch (Exception e)
				{
					Fail("\r\nFireSaveButton should have its own try/catch. Failed with: \r\n\r\n" + e.ToString());
				}
			}
			ExceptionReporterTestListener.Instance.Clear(); // stop exception reporter whinging that a concurrency exception was sent silently
		}

		public void TestSaveButtonIsFocusedWhenFired()
		{
			// Arrange
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			var dependent1 = dummy.Dependents.AddNew();
			dependent1.ZD1_Code = "ABC";
			var dependent2 = dummy.Dependents.AddNew();
			dependent2.ZD1_Code = "ABC";
			Factory.Save();
			using (var form = new ZForm(dummy))
			using (var toolstrip = new ZToolStrip())
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				form.Controls.Add(toolstrip);
				toolstrip.Items.Add(saveAndCloseButton);
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.Show();
				Assert(!toolstrip.Focused);
				// Act
				form.FireSaveButton(saveAndCloseButton);
				Application.DoEvents();
				// Assert
				Assert(toolstrip.Focused);
			}
		}

		public void TestCannotPerformOperationOnRowExceptionThrown()
		{
			var table = new DataTable("Test Table");
			table.PrimaryKey = new DataColumn[] { table.Columns.Add("Test Column") };
			var row = table.Rows.Add(1);
			row.AcceptChanges();
			row.Delete();

			var mockFactory = new Mock<BusinessObjectFactory>() { CallBase = true };
			var factory = mockFactory.Object;
			var zRowNotInTable = new ZSaveException(new ZRowNotInTableException(new RowNotInTableException(), row, Db.Connection), factory);

			mockFactory.Protected().Setup("SaveInTransactionCore").Throws(zRowNotInTable);

			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();

			using (var form = new ZDummyForm(dummy))
			{
				form.FireSaveButton();
			}

			var friendlyMessage = "A local data row is in an unexpected state and cannot be saved. Please close the current form and retry the operation. If the problem continues, please contact your system administrator";
			var lastException = ErrorReporter.LastExceptionReported as ZSaveException;

			AssertNotNull("A ZSaveException was reported", lastException);
			AssertEquals("Friendly error message should have been displayed to user instead of error report", friendlyMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Error message should have table information", "Tablename: Test Table", lastException.Message);
			AssertContains("Error message should have row information", "Test Column = 1", lastException.ExtraDebugInfo);
			ErrorReporter.Clear();
		}

		public void TestSetupPostingAndHasChanges()
		{
			BoundForm.DisplayMode = ODisplayMode.New;
			BoundForm.Show();

			Dummy.Z0_Description = "Test!!";
			AssertEquals("DisplayMode", ODisplayMode.Edit, BoundForm.DisplayMode);
			Dummy.Factory.Save();
			AssertEquals("DisplayMode", ODisplayMode.New, BoundForm.DisplayMode);
		}

		public void TestDisableNewAction()
		{
			using (var testForm = new TestWinFormForSaving(Dummy))
			{
				testForm.Show();

				testForm.DisplayMode = ODisplayMode.New;
				Assert("Doesn't have new button", !testForm.HasNewButton());

				testForm.ActuallyDoTheSave = true;
				Dummy.Z0_Number = 34;
				testForm.FireSaveButton();
				Assert("Does have new button", testForm.HasNewButton());
				AssertEquals("In Browse display mode", ODisplayMode.Browse, testForm.DisplayMode);

				testForm.DisableNewAction();
				Assert("Doesn't have new button", !testForm.HasNewButton());
				AssertEquals("In NewSaved display mode", ODisplayMode.NewSaved, testForm.DisplayMode);
			}
		}

		public void TestDisableNewActionWithReadonly()
		{
			using (var testForm = new TestWinFormForSaving(Dummy))
			{
				testForm.Show();

				testForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert("Doesn't have new button", !testForm.HasNewButton());
				AssertEquals("In ReadOnly display mode", ODisplayMode.ReadOnly, testForm.DisplayMode);

				testForm.DisableNewAction();
				Assert("Still doesn't have new button", !testForm.HasNewButton());
				AssertEquals("In ReadOnly display mode", ODisplayMode.ReadOnly, testForm.DisplayMode);
			}
		}

		public void TestBoundaryFormSize()
		{
			// This test expects that the machine the test is being run on is running at
			// atleast the Enterprise minimum spec for screen resolution (1024 x 768)
			var errorMessage = "Machine must be running at Enterprise Minimum Specification for Screen Resolution (1024 x 768), but was (" + CachedScreenInfo.Instance.PrimaryScreenInfo.Width + ", " + CachedScreenInfo.Instance.PrimaryScreenInfo.Height + ")";
			Assert(errorMessage, CachedScreenInfo.Instance.PrimaryScreenInfo.Height >= 700);
			Assert(errorMessage, CachedScreenInfo.Instance.PrimaryScreenInfo.Width >= 1024);

			Dummy.Z0_Code = "TTT";
			Dummy.Factory.Save();
			Dummy.ReadOnly = true;

			using (var testForm = new TestSizeForm(Dummy))
			{
				testForm.Show();
				AssertEquals("Form Size", new Size(1016, 691), testForm.ClientSize);
			}
		}

		public void TestMultipleClicksOnSavingButton()
		{
			// BeginInvoke is used to push multiple click messages onto the message loop,
			// then App.DoEvents pushes them all through together to create the situation where
			// one save click occurs within another save click.

			using (var testForm = new TestWinFormForSaving(Dummy))
			{
				testForm.AllowToSimulateAnotherButtonClickInside_ForTestOnly = true;
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.Show();
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveButton.PerformClick));
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveButton.PerformClick));
				Application.DoEvents();
				AssertEquals("Saving should only be run once when clicking on the SaveButton twice", 1, testForm.ValidateAndSaveCallCount);

				testForm.ValidateAndSaveCallCount = 0;
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveAndCloseButton.PerformClick));
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveAndCloseButton.PerformClick));
				Application.DoEvents();
				AssertEquals("Saving should only be run once when clicking on the SaveAndCloseButton twice", 1, testForm.ValidateAndSaveCallCount);
			}

			using (var testForm = new TestWinFormForSaving(Dummy))
			{
				testForm.AllowToSimulateAnotherButtonClickInside_ForTestOnly = true;
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveAndCloseButton.PerformClick));
				testForm.BeginInvoke(new MethodInvoker(testForm.SaveAndCloseButton.PerformClick));
				Application.DoEvents();
				AssertEquals("Saving should only be run once when clicking on the Delete button twice", 1, testForm.DeleteCallCount);
			}
		}

		public void TestSaveAndCloseWithChangesInOnSaving()
		{
			var dummyWithSaving = Factory.New<DummyBusinessObjectWithChangeInSaving>();
			using (var testForm = new ZForm(dummyWithSaving))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Edit;

				testForm.Show();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickCloseButtonToCloseFormOnBusinessEntityDeletedByConcurrentSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Factory.Save();

			var dummyInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);

			using (var testForm = new ZForm(dummyInOtherFactory))
			using (var cancelButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, null, cancelButton, null);
				testForm.Show();

				dummy.Delete();
				dummy.Factory.Save();

				dummyInOtherFactory.Z0_Number = 234;
				Assert("Precondition", dummyInOtherFactory.HasChanges);

				Assert("Form's business entity is not touched yet", !dummyInOtherFactory.IsDeleted);
				Assert("Form should be still visible", testForm.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				testForm.CommandButtonCancel.PerformClick();
				Application.DoEvents();

				Assert("Form's business entity should be marked as deleted during concurrency resolve", dummyInOtherFactory.IsDeleted);
				Assert("Form should be closed after concurrency resolve", !testForm.Visible);
				AssertEquals(string.Format("Because this {0} has been deleted while you were editing it, this form will be closed.", dummyInOtherFactory.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSuspendCellNotificationWhenClickApplyButton()
		{
			var dummyWithSaving = Factory.New<DummyBusinessObjectWithChangeInSaving>();
			using (var testForm = new ZTestForm(dummyWithSaving))
			{
				testForm.Grid.IsTestingUpdateNonCellNotifications = true;
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.Show();

				var child1 = dummyWithSaving.Collection.AddNew();
				var child2 = dummyWithSaving.Collection.AddNew();
				using (child1.SuspendValidationTesting())
				{
					child1.Z0_DateInfo.AddMessageError("Message Error");
				}

				testForm.Grid.UpdateNonCellNotificationsCountForTesting = 0;
				testForm.Grid.UpdateGridNotificationTypeCountForTesting = 0;

				testForm.CommandButtonApply.PerformClick();

				AssertEquals("UpdateNonCellNotifications should be called 11 times", 11, testForm.Grid.UpdateNonCellNotificationsCountForTesting);
				AssertEquals("Grid Notification Type should be updated only once", 1, testForm.Grid.UpdateGridNotificationTypeCountForTesting);
			}
		}

		public void TestSuspendCellNotificationWhenClickPostButton()
		{
			var dummyWithSaving = Factory.New<DummyBusinessObjectWithChangeInSaving>();
			using (var testForm = new ZTestForm(dummyWithSaving))
			{
				testForm.Grid.IsTestingUpdateNonCellNotifications = true;
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.Show();

				var child1 = dummyWithSaving.Collection.AddNew();
				var child2 = dummyWithSaving.Collection.AddNew();
				using (child1.SuspendValidationTesting())
				{
					child1.Z0_DateInfo.AddMessageError("Message Error");
				}

				testForm.Grid.UpdateNonCellNotificationsCountForTesting = 0;
				testForm.Grid.UpdateGridNotificationTypeCountForTesting = 0;

				testForm.CommandButtonPost.PerformClick();

				AssertEquals("UpdateNonCellNotifications should be called 11 times", 11, testForm.Grid.UpdateNonCellNotificationsCountForTesting);
				AssertEquals("Grid Notification Type should be updated only once", 1, testForm.Grid.UpdateGridNotificationTypeCountForTesting);
			}
		}

		public void TestSuspendCellNotificationWhenValidation()
		{
			var dummyWithSaving = Factory.New<DummyBusinessObjectWithChangeInSaving>();
			using (var testForm = new ZTestForm(dummyWithSaving))
			{
				testForm.Grid.IsTestingUpdateNonCellNotifications = true;
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.AllowToSimulateAnotherButtonClickInside_ForTestOnly = true;
				testForm.Show();

				dummyWithSaving.Collection.AddNew().Z0_Date = new ZDateTime(1900, 1, 1);
				dummyWithSaving.Collection.AddNew().Z0_Date = new ZDateTime(1900, 1, 1);

				testForm.Grid.UpdateNonCellNotificationsCountForTesting = 0;
				testForm.Grid.UpdateGridNotificationTypeCountForTesting = 0;

				testForm.PerformValidation();

				AssertEquals("UpdateNonCellNotifications should be called 2 times", 2, testForm.Grid.UpdateNonCellNotificationsCountForTesting);
				AssertEquals("Grid Notification Type should be updated only once", 1, testForm.Grid.UpdateGridNotificationTypeCountForTesting);
			}
		}

		public void TestDeactiveObjectWithValidationErrors_ApplicableIfControlIDISKnown()
		{
			var dummy = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			dummy.IsCancelled = false;
			Factory.Save();

			using (var testForm = new ZFormForTestCancelling(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.Text = "Form TEST";
				testForm.ControllerID = DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;
				testForm.DisplayMode = ODisplayMode.Delete;

				var objToDeactivate = testForm.BusinessEntity as BusinessObject;
				objToDeactivate.AddRowError("Test");

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"This record is in use by other records in the system. Would you like to mark this record as Inactive?"
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				var reloadedForm = new ZFormUtilitiesTest().GetFormCreatedByReloading(testForm);
				var dummyInReloadedForm = (DummyCancellableWhichCanNotBeDeleted)reloadedForm.BusinessEntity;

				AssertEquals("Old form is disposed.", true, testForm.IsDisposed);
				AssertEquals("DisplayMode of new form is Edit.", ODisplayMode.Edit, reloadedForm.DisplayMode);
				AssertEquals("Object of new form is deactivated.", true, dummyInReloadedForm.IsCancelled);
				AssertEquals("Collection of BizO should be same as without Rollback", 1, dummyInReloadedForm.Collection.Count);
				reloadedForm.FireSaveButton();
				reloadedForm.Dispose();
			}

			var dummyInNewFactory = new BusinessObjectFactory().Load<DummyCancellableWhichCanNotBeDeleted>(dummy.PK);
			AssertEquals("Save successfully", true, dummyInNewFactory.IsCancelled);
		}

		[ExpectNoExceptions]
		public void TestDeleteObjectWithConcurrencyIssues()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			using (var form = new ZForm(dummy1))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.ControllerID = DummyControllerIDs.Dummy;
				form.DisplayMode = ODisplayMode.Delete;

				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				form.CommandButtonPost.PerformClick();
				Application.DoEvents();

				var firstFormCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				var concurrencyResolverMessage = UnitTestUserNotification.Instance.PreviousMessages[1].Text;
				AssertStartsWith("The concurrency resolver dialog should be shown.", "While you have been working with this form, another user has made changes.", concurrencyResolverMessage);
				AssertStartsWith("The form reload dialog should be shown.", "This form has been modified by another user. It will now be reloaded.", UnitTestUserNotification.Instance.LastMessage.Text);
				firstFormCreatedByReloading.Dispose();
			}
		}

		public void TestReCancelBizOAfterConcurrencyIssues()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanBeCancelled>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			dummy1.IsCancelled = true;

			using (var form = new ZForm(dummy1))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.ControllerID = DummyControllerIDs.DummyControllerWithCancellableBizO;
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				form.CommandButtonPost.PerformClick();
				Application.DoEvents();

				var firstFormCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				AssertStartsWith("The concurrency resolver dialog should be shown.", "This form has been modified by another user. It will now be reloaded.", UnitTestUserNotification.Instance.LastMessage.Text);
				firstFormCreatedByReloading.Dispose();
			}
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackAndRebindEnabledFormNotDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, true, true, false);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackAndRebindEnabledFormDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, true, true, true);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackEnabledRebindDisabledFormNotDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, true, false, false);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackEnabledRebindDisabledFormDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, true, false, true);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackDisabledRebindEnabledFormNotDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, false, true, false);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackDisabledRebindEnabledFormDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, false, true, true);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackAndRebindDisabledFormNotDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, false, false, false);
		}

		public void TestDeleteObjectWithConcurrencyIssues_RollbackAndRebindDisabledFormDisabled()
		{
			var dummy1 = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			Factory.RefreshEnabled = false;
			dummy1.Z0_Number = 10;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var dummy2 = factory2.Load<DummyBusinessObject>(dummy1.PK);
			dummy2.Z0_Number = 9;
			factory2.Save();

			AssertDeleteObjectWithConcurrencyIssues(dummy1, false, false, true);
		}

		void AssertDeleteObjectWithConcurrencyIssues(DummyCancellableWhichCanNotBeDeleted dummy, bool rollback, bool rebind, bool disableForm)
		{
			using (var form = new ZForm(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.ControllerID = DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;
				form.DisplayMode = ODisplayMode.Delete;
				dummy.RollbackAfterDeleteError = rollback;
				dummy.RebindAfterDeleteError = rebind;
				dummy.DisableFormOnDeleteConcurrencyError = disableForm;
				Assert("Dummy is not deleted yet", !dummy.IsDeleted);

				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				form.CommandButtonPost.PerformClick();
				Application.DoEvents();

				if (rollback && rebind)
				{
					var secondLastNotification = UnitTestUserNotification.Instance.PreviousMessages[1].Text;
					AssertStartsWith("The concurrency resolver dialog should be shown.", "While you have been working with this form, another user has made changes.", secondLastNotification);
					AssertStartsWith("The form reload dialog should be shown.", "This form has been modified by another user. It will now be reloaded.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (var firstFormCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(form))
					{
						AssertRollbackRebind(firstFormCreatedByReloading);
					}
				}
				else
				{
					AssertStartsWith("The concurrency resolver dialog should be shown.", "While you have been working with this form, another user has made changes.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertRollbackRebind(form);
				}

				void AssertRollbackRebind(ZForm testForm)
				{
					if (rollback)
					{
						Assert("Dummy deletion was rolled back", !dummy.IsDeleted);
						if (rebind)
						{
							Assert("Form is not disposed", !testForm.IsDisposed);
							AssertEquals("Form Disabled", disableForm, testForm.DisplayMode == ODisplayMode.ReadOnly);

							var boundBizo = (DummyCancellableWhichCanNotBeDeleted)testForm.BusinessEntity;
							AssertNotEquals("BizO should be reloaded", dummy, boundBizo);
							AssertNotEquals("New Factory", dummy.Factory._Instance, boundBizo.Factory._Instance);
							Assert("Not deleted", !boundBizo.IsDeleted);
						}
						else
						{
							Assert("Form should be disposed", testForm.IsDisposed);
						}
					}
					else
					{
						Assert("Dummy deletion was not rolled back", dummy.IsDeleted);
						Assert("Form is not disposed", !testForm.IsDisposed);

						AssertEquals("Form Disabled", disableForm, testForm.DisplayMode == ODisplayMode.ReadOnly);
						var boundBizo = (DummyCancellableWhichCanNotBeDeleted)testForm.BusinessEntity;
						AssertEquals("BizO should be same as without Rollback the Rebind flag value does not matter", dummy, boundBizo);
					}
				}
			}
		}

		public void TestCloseFormOnBusinessEntityDeleted()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Factory.Save();

			using (var testForm = new ZForm(new BusinessObjectFactory().Load<DummyBusinessObject>(dummy.PK)))
			{
				testForm.Show();
				UnitTestUserNotification.Instance.AddOKAnswer();

				dummy.Delete();
				Assert("Should be visible yet", testForm.Visible);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				dummy.Factory.Save();
				Application.DoEvents();

				Assert("Should be closed after data refresh", !testForm.Visible);
				AssertEquals("This record has been deleted in other form and cannot be used further. This form will be closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCloseFormOnBusinessEntityDeletedByConcurrentSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Factory.Save();

			var dummyInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);

			using (var testForm = new ZForm(dummyInOtherFactory))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);
				testForm.Show();
				dummyInOtherFactory.Z0_Number = 123;
				Assert("Precondition", dummyInOtherFactory.HasChanges);

				dummy.Delete();
				dummy.Factory.Save();
				Application.DoEvents();

				Assert("Form's business entity is not touched yet", !dummyInOtherFactory.IsDeleted);
				Assert("Form should be still visible", testForm.Visible);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.AddOKAnswer();
				UnitTestUserNotification.Instance.AddOKAnswer();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				Assert("Form's business entity should be marked as deleted during concurrency resolve", dummyInOtherFactory.IsDeleted);
				Assert("Form should be closed after concurrency resolve", !testForm.Visible);
				AssertEquals(string.Format("Because this {0} has been deleted while you were editing it, this form will be closed.", dummyInOtherFactory.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Cancelling

		public void TestGetICancellable()
		{
			var dummyBusiness = Factory.New<DummyBusinessObject>();
			var dummyCancellable = Factory.New<DummyCancellableWhichCanBeCancelled>();

			using (var testForm = new ZForm(dummyCancellable))
			{
				AssertNull(testForm.GetICancellable(dummyBusiness));
				AssertEquals(dummyCancellable, testForm.GetICancellable(dummyCancellable));
			}
		}

		public void TestGetICancellable_TemplateRecord()
		{
			var dummyBusiness = Factory.New<DummyBusinessObject>();
			var dummyTemplateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var dummyTemplateRecord = Factory.New<DummyTemplateRecord>();

			dummyTemplateRecordProvider.TemplateRecord = dummyTemplateRecord;

			using (var testForm = new ZForm(dummyTemplateRecordProvider))
			{
				AssertNull(testForm.GetICancellable(dummyBusiness));
				AssertEquals(
					"Should return template record, not the provider",
					dummyTemplateRecord,
					testForm.GetICancellable(dummyTemplateRecordProvider)
				);
			}

			dummyTemplateRecordProvider.TemplateRecord = null;

			using (var testForm = new ZForm(dummyTemplateRecordProvider))
			{
				AssertEquals(
					"Should return provider because no template record",
					dummyTemplateRecordProvider,
					testForm.GetICancellable(dummyTemplateRecordProvider)
				);
			}
		}

		public void TestSavingCancellableWhichCanNotBeCancelled()
		{
			var dummyCancellableWhichCanNotBeCancelled = Factory.New<DummyCancellableWhichCanNotBeCancelled>();
			using (var testForm = new ZForm(dummyCancellableWhichCanNotBeCancelled))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;
				dummyCancellableWhichCanNotBeCancelled.IsCancelled = true; // it's being set in ZController - so we do it manually here

				testForm.Show();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				AssertEquals("This Dummy BizO can't be cancelled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSavingCancellableWhichCanBeCancelled()
		{
			var dummyCancellableWhichCanBeCancelled = Factory.New<DummyCancellableWhichCanBeCancelled>();
			using (var testForm = new ZForm(dummyCancellableWhichCanBeCancelled))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSavingCancellableWhichCanNotBeDeletedBecauseOfFKViolation()
		{
			var dummyCancellableWhichCanNotBeDeleted = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			dummyCancellableWhichCanNotBeDeleted.IsCancelled = false;
			Factory.Save();

			using (var testForm = new ZFormForTestCancelling(dummyCancellableWhichCanNotBeDeleted))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);
				testForm.ControllerID = DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;
				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"This record is in use by other records in the system. Would you like to mark this record as Inactive?"
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();
				AssertEquals("This record is in use by other records in the system. Would you like to mark this record as Inactive?", UnitTestUserNotification.Instance.LastMessage.Text);

				var reloadedForm = new ZFormUtilitiesTest().GetFormCreatedByReloading(testForm);
				var dummyInReloadedForm = (DummyCancellableWhichCanNotBeDeleted)reloadedForm.BusinessEntity;

				AssertEquals("Old form is disposed.", true, testForm.IsDisposed);
				AssertEquals("DisplayMode of new form is Edit.", ODisplayMode.Edit, reloadedForm.DisplayMode);
				AssertEquals("Object of new form is deactivated.", true, dummyInReloadedForm.IsCancelled);
				AssertEquals("Collection of BizO should be same as without Rollback", 1, dummyInReloadedForm.Collection.Count);
				reloadedForm.FireSaveButton();
				reloadedForm.Dispose();
			}

			var dummyInNewFactory = new BusinessObjectFactory().Load<DummyCancellableWhichCanNotBeDeleted>(dummyCancellableWhichCanNotBeDeleted.PK);
			AssertEquals("Save successfully", true, dummyInNewFactory.IsCancelled);
		}

		public void TestSavingNonCancellableWhichCanNotBeDeletedBecauseOfFKViolation()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var testForm = new ZFormForTestCancelling(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK); //"This record is in use by other records in the system and cannot be deleted."
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();
				AssertEquals("This record is in use by other records in the system and cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSavingNonCancellableWhichCanNotBeDeletedBecauseOfFKViolationWithHyperlink()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			using (var testForm = new ZFormForTestHyperlink(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();

				AssertEquals(typeof(HyperlinkAlertForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var hyperlinkAlertBusinessObject = (HyperlinkAlertBusinessObject)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertNotNull(hyperlinkAlertBusinessObject);
				var topMessageLabel = hyperlinkAlertBusinessObject.MessageLabel;
				var textBox = hyperlinkAlertBusinessObject.LongMessageText;

				AssertEquals("This record is in use by one or more record(s) with the following descriptions, and thus cannot be deleted. Please click View Details below to see full list.", topMessageLabel);
				AssertEquals(@"This record is in use by one or more record(s) of the module Cheque Books with the following descriptions, and thus cannot be deleted.
TestAccChequeBook
TestAccChequeBook
TestAccChequeBook
TestAccChequeBook", textBox);
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestSavingCancellableWithBusinessEntityForValidationOverride()
		{
			var dummyCancellableWhichCanNotBeCancelled = Factory.New<DummyCancellableWhichCanNotBeCancelled>();
			using (var testForm = new ZFormForTestBusinessEntityForValidation(dummyCancellableWhichCanNotBeCancelled))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			var dummyBO = Factory.New<DummyBusinessObject>();
			using (var testForm = new ZFormForTestBusinessEntityForValidation(dummyCancellableWhichCanNotBeCancelled))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.BusinessEntityForValidationForTest = dummyBO;
				testForm.Show();
				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("You are about to delete this record"));
			}
		}

		public void TestQueryForSavingOnCancelInModeNew()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.New;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Close();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Would you like to save the changes?"));
			}
		}

		public void TestOnClosingWhenCancelled()
		{
			using (var form = new ZFormWithOnClosingExposed(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.New;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var e = new CancelEventArgs(true);
				form.OnClosing_Exposed(e);
				Assert(e.Cancel);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.OnClosing_Exposed(e);
				Assert(!e.Cancel);
			}

			using (var form = new ZFormWithOnClosingExposed(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
				form.DisplayMode = ODisplayMode.New;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var e = new CancelEventArgs(false);
				form.OnClosing_Exposed(e);
				Assert(e.Cancel);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.OnClosing_Exposed(e);
				Assert(!e.Cancel);
			}
		}

		class ZFormWithOnClosingExposed : ZForm
		{
			public ZFormWithOnClosingExposed(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public void OnClosing_Exposed(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}

		class ZFormForTestCancelling : ZForm
		{
			public ZFormForTestCancelling(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			protected override void DeleteCore()
			{
				base.DeleteCore();
				throw new Exception("FK violation exception", new Exception("The DELETE statement conflicted with the REFERENCE constraint"));
			}
		}

		class ZFormForTestHyperlink : ZForm
		{
			public ZFormForTestHyperlink(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			protected internal override void HandleApplyPostingButtonClickUnsafe(bool closeOnSave)
			{
				throw new Exception("FK violation exception", new Exception("The DELETE statement conflicted with the REFERENCE constraint"));
			}

			protected override string GetMessageForCannotDeleteRecordInUseException(Exception ex)
			{
				return @"This record is in use by one or more record(s) of the module Cheque Books with the following descriptions, and thus cannot be deleted.
TestAccChequeBook
TestAccChequeBook
TestAccChequeBook
TestAccChequeBook";
			}
		}

		public class ZFormForTestBusinessEntityForValidation : ZForm
		{
			public ZFormForTestBusinessEntityForValidation(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			protected internal override IBusiness BusinessEntityForValidation
			{
				get
				{
					return BusinessEntityForValidationForTest ?? base.BusinessEntityForValidation;
				}
			}
			public IBusiness BusinessEntityForValidationForTest { private get; set; }
		}

		[PreventDelete(true)]
		public class DummyCancellableWhichCanNotBeCancelled : DummyBusinessObject, ICancellable
		{
			public DummyCancellableWhichCanNotBeCancelled(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICancellable Members

			public string CanCancel()
			{
				return "This Dummy BizO can't be cancelled.";
			}

			public string CanReactivate()
			{
				return null;
			}

			public bool IsCancelled
			{
				get
				{
					return isCancelled;
				}
				set
				{
					isCancelled = value;
				}
			}
			bool isCancelled;

			public bool IsCancelledHasChanged
			{
				get { return false; }
			}

			#endregion
		}

		[PreventDelete(true)]
		public class DummyCancellableWhichCanNotBeReactivated : DummyBusinessObject, ICancellable
		{
			public DummyCancellableWhichCanNotBeReactivated(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICancellable Members

			public string CanCancel()
			{
				return null;
			}

			public string CanReactivate()
			{
				return "This Dummy BizO can't be reactivated.";
			}

			public bool IsCancelled
			{
				get
				{
					return isCancelled;
				}
				set
				{
					isCancelled = value;
				}
			}
			bool isCancelled;

			public bool IsCancelledHasChanged
			{
				get { return false; }
			}

			#endregion
		}

		[PreventDelete(true)]
		public class DummyCancellableWhichCanBeCancelled : DummyBusinessObject, ICancellable
		{
			public DummyCancellableWhichCanBeCancelled(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ICancellable Members

			public string CanCancel()
			{
				return null;
			}

			public string CanReactivate()
			{
				return null;
			}

			public bool IsCancelled
			{
				get
				{
					return Z0_Bool;
				}
				set
				{
					Z0_Bool = value;
				}
			}

			public bool IsCancelledHasChanged
			{
				get { return false; }
			}

			#endregion
		}

		#endregion

		class DummyBusinessObjectWithChangeInSaving : DummyBusinessObject
		{
			public DummyBusinessObjectWithChangeInSaving(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable]
			public new DummyBusinessObjectActiveCollection Collection
			{
				get
				{
					if (dataCollection == null)
					{
						dataCollection = new DummyBusinessObjectActiveCollection(this);
						RegisterEditableChildObject(dataCollection);
					}
					return dataCollection;
				}
			}
			public DummyBusinessObjectActiveCollection dataCollection;

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				this.Z0_AnotherNumber = 99;
				Collection.Cast<DummyBusinessObject>().ForEach(c =>
				{
					c.Z0_Date = new ZDateTime(1900, 1, 1);
					c.Z0_AnotherDate = new ZDateTime(1900, 1, 1);
				});

				if (Collection.Count > 0)
				{
					Collection[0].Delete();
				}

				base.OnFactorySavingBeforeTransactionCore();
			}
		}

		class DummyBusinessObjectActiveCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
		{
			public DummyBusinessObjectActiveCollection(DummyBusinessObject master)
				: base(master.Factory, master, new ZQuery(), DummyBizoSchema.Z0_Guid)
			{
			}
		}

		public void TestSettingForBorderStyleSetsMaximizeBoxValue()
		{
			Form.FormBorderStyle = FormBorderStyle.Fixed3D;
			Assert("Form.MaximizeBox", !Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.Sizable;
			Assert("Form.MaximizeBox", Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.FixedDialog;
			Assert("Form.MaximizeBox", !Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Assert("Form.MaximizeBox", Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.FixedSingle;
			Assert("Form.MaximizeBox", !Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.SizableToolWindow;
			Assert("Form.MaximizeBox", Form.MaximizeBox);

			Form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
			Assert("Form.MaximizeBox", !Form.MaximizeBox);
		}

		public void TestTabbingBackThroughControls()
		{
			var dummy1 = Dummy.Collection.AddNew();
			dummy1.Z0_Code = "111";
			dummy1.Z0_Description = "ONE";
			dummy1.Z0_NVarCharMax = "1";
			dummy1.Z0_VarCharMax = "11111";
			var dummy2 = Dummy.Collection.AddNew();
			dummy2.Z0_Code = "222";
			dummy2.Z0_Description = "TWO";
			dummy2.Z0_NVarCharMax = "2";
			dummy2.Z0_VarCharMax = "22222";
			var dummy3 = Dummy.Collection.AddNew();
			dummy3.Z0_Code = "333";
			dummy3.Z0_NVarCharMax = "3";
			dummy3.Z0_Description = "THREE";
			dummy3.Z0_VarCharMax = "33333";

			using (var testForm = new ZForm(Dummy))
			{
				testForm.Size = new Size(500, 500);

				var textBox = new TestTextBox { TabStop = true, TabIndex = 0 };
				var testGrid = new OWinFormTestGrid();

				testForm.Controls.Add(textBox);
				testForm.Controls.Add(testGrid);

				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });
				testGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_VarCharMax });

				testGrid.SetDataBinding(Dummy, "Collection", Dummy.Collection.GetType().Name);

				testGrid.TabStop = true;
				testGrid.TabIndex = 1;
				testGrid.Size = new Size(500, 400);
				testGrid.Location = new Point(0, 30);

				testForm.Show();

				testGrid.Focus();
				testGrid.CurrentCell = new DataGridCell(0, 0);

				testGrid.LastFocusedColumn.TextBox.Visible = true;
				KeySender.SendKeyPress(testGrid.LastFocusedColumn.TextBox, testGrid.LastFocusedColumn.TextBox.Handle, Keys.A);
				testGrid.LastFocusedColumn.TextBox.Visible = true;
				testGrid.SendCmdKey(new Message(), Keys.Tab | Keys.Shift);
				Assert("TextBox should be focused", textBox.Focused);
			}
		}

		public void TestGetControlsRecursively()
		{
			using (var testForm = new ZForm())
			{
				var testTabControl = new ZTabControl { Parent = testForm };
				var page1 = new ZTabPage(); // This is legacy architecture that will be removed
				testTabControl.TabPages.Add(page1);
				var page2 = new ZTabPage(); // This is legacy architecture that will be removed
				testTabControl.TabPages.Add(page2);

				new ZGroupBox { Parent = testForm };
				var groupBox2 = new ZGroupBox { Parent = page1 };
				new ZPanel { Parent = testForm };
				new ZPanel { Parent = page2 };
				new ZLabel { Parent = testForm };
				new TestTextBox { Parent = testForm };
				new ZLabel { Parent = page1 };
				new TestTextBox { Parent = page1 };
				new ZLabel { Parent = groupBox2 };

				Label[] labels = ZTestFormUtilities.GetControlsRecursively<ZLabel>(testForm);
				AssertEquals(3, labels.Length);

				var textBoxes = ZTestFormUtilities.GetControlsRecursively<TestTextBox>(testForm);
				AssertEquals(2, textBoxes.Length);

				var controls = ZTestFormUtilities.GetControlsRecursively<Control>(testForm);
				AssertEquals(13, controls.Length);
			}
		}

		public void TestActionsMenuItemAlwaysEnabled()
		{
			using (var testForm = new TestZForm())
			{
				AssertEquals("Actions Menu Item should be enabled", true, testForm.ExposedActionMenuItem.Enabled);
			}
		}

#if !WINZOR
		//Test for winzor: TestTriggerResizeCompleteOnBrowserSizeChangedAsyncEventDispatcher()
		//https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FWinzor%2FEnterprise.Winzor.Architecture.Test%2FZFormTest.cs&version=GBmaster&line=17&lineEnd=17&lineStartColumn=20&lineEndColumn=85&lineStyle=plain&_a=contents
		public void TestResizeComplete()
		{
			var resizeCompleteEventsTriggeredCount = 0;

			using (var testForm = new TestZForm())
			{
				testForm.ResizeComplete += delegate
				{ resizeCompleteEventsTriggeredCount++; };
				testForm.Show();
				AssertEquals("No ResizeComplete events yet", 0, resizeCompleteEventsTriggeredCount);

				UnsafeNativeMethods.PostMessage(new HandleRef(testForm, testForm.Handle), WindowsMessage.WM_DISPLAYCHANGE, IntPtr.Zero, IntPtr.Zero);
				Application.DoEvents(); // give message you just posted a chance to be processed.

				AssertEquals("ResizeComplete events", 1, resizeCompleteEventsTriggeredCount);
			}
		}
#endif

		[ExpectNoExceptions]
		public void TestIsPostOnly_NoExceptionThrownWhenFileMenuItemIsNull()
		{
			using (var testForm = new ZChildForm())
			{
				AssertNull("Precondition", ((IFileMenuItemsProvider)testForm).FileMenuItem);
				testForm.IsPostOnly = true;
			}
		}

		public void TestFactoryChanged()
		{
			using (var testForm = new TestZForm())
			{
				var factoryChangeCount = 0;
				testForm.OnFactoryChangedForTest();
				AssertEquals("Not listening", 0, factoryChangeCount);

				testForm.FactoryChanged += delegate
				{ factoryChangeCount++; };
				testForm.OnFactoryChangedForTest();
				AssertEquals("Raised once", 1, factoryChangeCount);
			}
		}

		public void TestRegisterHotkeys()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestForm(dummy))
			{
				KeySender.SendKeyDownToProcessCmdKey(form, Keys.Control | Keys.Shift | Keys.D);
				var forms = ZApplication.GetOpenForms().Where(f => f is DeveloperDiagnosticsForm);
				AssertEquals("Show Developer Diagnostics Form", 1, forms.Count());
				forms.First().Close();

				KeySender.SendKeyDownToProcessCmdKey(form, Keys.Control | Keys.Alt | Keys.D);
				forms = ZApplication.GetOpenForms().Where(f => f is DeveloperDiagnosticsForm);
				AssertEquals("Show Developer Diagnostics Form", 1, forms.Count());
				forms.First().Close();

				KeySender.SendKeyDownToProcessCmdKey(form, Keys.Control | Keys.Shift | Keys.R);
				Assert("Show Control Information Form", ZFormModaliser.LastFormShownDialogForTest is DebugControlInfoForm);
				ZFormModaliser.LastFormShownDialogForTest = null;

				dummy.Z0_Code = "JSW";
				KeySender.SendKeyDownToProcessCmdKey(form, Keys.Control | Keys.Enter);
				Assert("Click Apply Button", !dummy.HasChanges);

				dummy.Z0_Code = "Wen";
				form.CommandButtonApply.Enabled = false;
				KeySender.SendKeyDownToProcessCmdKey(form, Keys.Control | Keys.Enter);
				Assert("Click Post Button", !dummy.HasChanges);
				Assert("Form is Closed", form.IsDisposed);
			}
		}

		#region TestEnableScanning

		public void TestEnableScanning()
		{
			using (var form = new TestZForm())
			{
				AssertNull("Precondition", form.Scanner);

				var isBarcodeInvoked = false;
				var barcodes = new BarcodeManager();
				barcodes.AddBarcode("a", () => isBarcodeInvoked = true);

				form.EnableScanning(barcodes, () => true);
				AssertEquals("Precondition", false, isBarcodeInvoked);

				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Barcode scan should have registered.", true, isBarcodeInvoked);

				var scanner = form.Scanner;
				AssertNotNull(scanner);
				AssertExceptionThrown(typeof(NotSupportedException), () => form.EnableScanning(new BarcodeManager(), () => false));
				AssertEquals("Scanner should not be overwritten when invoking EnableScanning() twice.", scanner, form.Scanner);
			}
		}

		public void TestEnableScanning_WhenReadOnly()
		{
			using (var form = new TestZForm())
			{
				var isBarcodeInvoked = false;
				var barcodes = new BarcodeManager();
				barcodes.AddBarcode("a", () => isBarcodeInvoked = true);

				form.EnableScanning(barcodes, () => true);
				AssertEquals("Precondition", false, isBarcodeInvoked);

				form.DisplayMode = ODisplayMode.ReadOnly;
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Barcode scan should *not* have registered because the form DisplayMode is ReadOnly.", false, isBarcodeInvoked);

				form.DisplayMode = ODisplayMode.Delete;
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Barcode scan should *not* have registered because the form DisplayMode is Delete.", false, isBarcodeInvoked);

				form.DisplayMode = ODisplayMode.Edit;
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals("Barcode scan should have registered because the form DisplayMode is Edit.", true, isBarcodeInvoked);
			}
		}

		public void TestEnableScanning_ShowScanningMessage()
		{
			using (var form = new TestZForm())
			{
				var panel = new ZPanel();
				form.Controls.Add(panel);
				form.EnableScanning(new BarcodeManager(), () => true, panel);

				form.Show();

				// ensure we have added the error control to the consumer's panel
				var messageControl = panel.Controls[0] as ScanMessageUserControl;
				AssertNotNull(messageControl);
				AssertEquals(false, messageControl.Visible);

				// show an error
				form.ShowScanningMessage("error", NotificationTypes.Error);
				AssertEquals(true, messageControl.Visible);

				// ensure a subsequent scan clears the error
				SendKeys(form, ScanningManager.ScannerPreAndPostamble_ForTesting, Keys.A, ScanningManager.ScannerPreAndPostamble_ForTesting);
				AssertEquals(false, messageControl.Visible);

				// attempt to show an empty error
				form.ShowScanningMessage("", NotificationTypes.None);
				AssertEquals(false, messageControl.Visible);
			}
		}

		public void TestEnableScanning_HideScanningMessage()
		{
			using (var form = new TestZForm())
			{
				var panel = new ZPanel();
				form.Controls.Add(panel);
				form.EnableScanning(new BarcodeManager(), () => true, panel);

				form.Show();
				var messageControl = (ScanMessageUserControl)panel.Controls[0];

				// show an error
				form.ShowScanningMessage("error", NotificationTypes.Error);
				AssertEquals("Precondition", true, messageControl.Visible);

				// hide the error
				form.HideScanningMessage();
				AssertEquals(false, messageControl.Visible);
			}
		}

		void SendKeys(Control control, params Keys[] keys)
		{
			foreach (var key in keys)
			{
				KeySender.SendKeyDown(control, control.Handle, key);
			}
		}

		#endregion

		#region Display Mode

		public void TestOriginalDisplayMode()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var testForm = new TestZFormWithSaveButton(dummy))
			using (var postingButtons = new ZPostingButtonsUserControl())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, postingButtons);
				testForm.DisplayMode = ODisplayMode.New;

				testForm.Show();
				Application.DoEvents();

				AssertEquals(ODisplayMode.Undefined, testForm.OriginalDisplayMode);

				dummy.Z0_Code = "XXX";
				AssertEquals(ODisplayMode.New, testForm.OriginalDisplayMode);

				testForm.HandleSaveButton();
				AssertEquals(ODisplayMode.Browse, testForm.OriginalDisplayMode);
			}
		}

		public class TestZFormWithSaveButton : ZForm
		{
			public TestZFormWithSaveButton(object dataSource) : base(dataSource) { }

			public void HandleSaveButton()
			{
				HandleSaveWhileClosing(new CancelEventArgs());
			}
		}

		public void TestSaveButtonUpdateShouldBeDoneAfterSaveIsFinished()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			var dependent1 = dummy.Dependents.AddNew();
			dependent1.ZD1_Code = "ABC";
			var dependent2 = dummy.Dependents.AddNew();
			dependent2.ZD1_Code = "ABC";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var mock = new Mock<DummyWithDependentsBusinessObject>(newFactory, new RowFactory(newFactory).LoadFromPK(DummyWithDependentsBusinessObject.Schema.TableName, dummy.PK)) { CallBase = true };
			dummy = mock.Object;
			var dependent1Mock = new Mock<DummyDependantBusinessObject>(newFactory, new RowFactory(newFactory).LoadFromPK(DummyDependantBusinessObject.Schema.TableName, dependent1.PK)) { CallBase = true };
			dependent1 = dependent1Mock.Object;
			var validation1Mock = new Mock<DummyDependentBizoValidation>(dependent1Mock.Object);
			dependent1Mock.Protected().Setup<DummyDependentBizoValidation>("GetNewValidation").Returns(validation1Mock.Object);
			validation1Mock.Protected().Setup("CheckZD1_Number").Callback(() => dependent1.ZD1_NumberInfo.AddMessageError("Some error"));
			validation1Mock.Protected().Setup("CheckZD1_Code").Callback(() => dependent1.ZD1_CodeInfo.AddMessageError("Some error"));

			var dependent2Mock = new Mock<DummyDependantBusinessObject>(newFactory, new RowFactory(newFactory).LoadFromPK(DummyDependantBusinessObject.Schema.TableName, dependent2.PK)) { CallBase = true };
			dependent2 = dependent2Mock.Object;
			var validation2Mock = new Mock<DummyDependentBizoValidation>(dependent2Mock.Object);
			dependent2Mock.Protected().Setup<DummyDependentBizoValidation>("GetNewValidation").Returns(validation2Mock.Object);
			validation2Mock.Protected().Setup("CheckZD1_Number").Callback(() => dependent2.ZD1_NumberInfo.AddMessageError("Some error"));
			validation2Mock.Protected().Setup("CheckZD1_Code").Callback(() => dependent2.ZD1_CodeInfo.AddMessageError("Some error"));

			var collection = dummy.ActiveDependents;
			dummy.RegisterEditableChildObject(collection);

			using (var form = new ZForm(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.Show();
				var postingCount = 0;
				mock.SetupGet(o => o.HasChanges).Callback(() =>
				{
					var stackLong = new System.Diagnostics.StackTrace().ToString();
					if (stackLong.Contains(".UpdateSaveButtonsBasedOnHasChanges"))
					{
						postingCount++;
					}
				}).Returns(true);

				form.FireSaveButton();
				Application.DoEvents();
				AssertEquals("Accessing HasChanges can be very expensive; UpdateSaveButtonsBasedOnHasChanges should be called last", 1, postingCount);
			}
			mock.VerifyAll();
		}

		#endregion

#if !WINZOR

		public void TestSavingLeavesFormDisabledIfItShowedModalisedChildForm()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var form = new TestZFormShowModalFormOnSaving(dummy))
			using (ZForm childForm = new TestForm())
			{
				form.PreSaveModalForm = childForm;
				form.Show();

				var nativeWindow = ONativeWindow.FromHandle(form.Handle);
				AssertEquals("Form is originally enabled", false, nativeWindow.GetWindowStyle(NativeMethods.WindowStyles.WS_DISABLED));

				form.FireSaveButton();
				Application.DoEvents();
				AssertEquals("Form has a modaaaly shown child form", childForm, ZFormModaliser.GetActiveChildFormForParentForm(form));
				AssertEquals("Form should be still disabled", true, nativeWindow.GetWindowStyle(NativeMethods.WindowStyles.WS_DISABLED));

				childForm.Close();
				AssertNull("No child form", ZFormModaliser.GetActiveChildFormForParentForm(form));
				AssertEquals("Now form should be enabled", false, nativeWindow.GetWindowStyle(NativeMethods.WindowStyles.WS_DISABLED));
			}
		}

		public void TestDeactiveObjectWithValidationErrors_ReloadFormFailed()
		{
			var dummy = Factory.New<DummyCancellableWhichCanNotBeDeleted>();
			dummy.IsCancelled = false;
			Factory.Save();

			using (var testForm = new ZFormForTestCancelling(dummy))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, saveAndCloseButton, null, null);

				testForm.Text = "Form TEST";
				testForm.ControllerID = DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted;
				testForm.DisplayMode = ODisplayMode.Delete;

				var objToDeactivate = testForm.BusinessEntity as BusinessObject;
				objToDeactivate.AddRowError("Test");

				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"This record is in use by other records in the system. Would you like to mark this record as Inactive?"

				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var dummy2 = factory.Load<DummyCancellableWhichCanNotBeDeleted>(dummy.PK);
				dummy2.Delete();
				factory.Save();

				testForm.CommandButtonPost.PerformClick();
				Application.DoEvents();
				AssertEquals("Old form is disposed.", true, testForm.IsDisposed);
			}
		}

#endif

		[ExpectNoExceptions]
		public void TestAcceptButtonShouldNotThrowNullReferenceException()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			{
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				textBox.Focus();
				KeySender.PostKeyDown(textBox, Keys.Enter);
				Application.DoEvents();

				AssertNull(form.AcceptButton);
			}
		}

		public class TestZFormShowModalFormOnSaving : ZForm
		{
			public TestZFormShowModalFormOnSaving(object dataSource) : base(dataSource) { }

			public void HandleSaveButton()
			{
				HandleSaveWhileClosing(new CancelEventArgs());
			}

			public ZForm PreSaveModalForm;

			protected internal override ContinueWithSave ShowPreSaveDialogs()
			{
				if (PreSaveModalForm != null)
				{
					ZFormModaliser.Show(PreSaveModalForm, this);
					return ContinueWithSave.No;
				}
				else
				{
					return base.ShowPreSaveDialogs();
				}
			}
		}

		#region Tab Skipping ReadOnly Fields

#if !WINZOR

		public void TestProcessTabKey()
		{
			Form.Show();
			Form.Controls.Add(TextBox1);
			Form.Controls.Add(TextBox2);
			Form.Controls.Add(TextBox3);
			Application.DoEvents();

			TextBox1.TabIndex = 1;
			TextBox2.TabIndex = 2;
			TextBox3.TabIndex = 3;

			TextBox1.Focus();
			TextBox2.ReadOnly = true;
			Form.ProcessTabKey(true);
			AssertEquals("TextBox2 is read only, thus should be skipped (forwards)", true, TextBox3.Focused);

			Form.ProcessTabKey(false);
			AssertEquals("TextBox2 is read only, thus should be skipped (backwards)", true, TextBox1.Focused);
			AssertEquals(true, TextBox1.Focused);
		}

		ZTextBox TextBox1
		{
			get { return textBox1 ?? (textBox1 = new ZTextBox()); }
		}
		ZTextBox textBox1;

		ZTextBox TextBox2
		{
			get { return textBox2 ?? (textBox2 = new ZTextBox()); }
		}
		ZTextBox textBox2;

		ZTextBox TextBox3
		{
			get { return textBox3 ?? (textBox3 = new ZTextBox()); }
		}
		ZTextBox textBox3;

#endif

		#endregion

		#region FormCaption

		public void TestFormCaptionNewToEdit()
		{
			BoundForm.Show();
			AssertEquals("Form Text", "New ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "New ZDummyForm", BoundForm.FormHeading);

			Dummy.Z0_Code = "TTT";
			AssertEquals("Form Text", "New ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "New ZDummyForm", BoundForm.FormHeading);

			BoundForm.SaveUserControl.SaveButton.PerformClick();
			AssertEquals("Form Text", "Edit ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "Edit ZDummyForm", BoundForm.FormHeading);
		}

		public void TestFormCaptionDelete()
		{
			Dummy.Z0_Code = "TTT";
			Dummy.Factory.Save();

			BoundForm.DisplayMode = ODisplayMode.Delete;
			BoundForm.Show();
			AssertEquals("Form Text", "Delete ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "Delete ZDummyForm", BoundForm.FormHeading);
		}

		public void TestFormCaptionEdit()
		{
			Dummy.Z0_Code = "TTT";
			Dummy.Factory.Save();

			BoundForm.Show();
			AssertEquals("Form Text", "Edit ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "Edit ZDummyForm", BoundForm.FormHeading);
		}

		public void TestFormCaptionView()
		{
			Dummy.Z0_Code = "TTT";
			Dummy.Factory.Save();
			Dummy.ReadOnly = true;

			BoundForm.DisplayMode = ODisplayMode.ReadOnly;
			BoundForm.Show();
			AssertEquals("Form Text", "View ZDummyForm", BoundForm.Text);
			AssertEquals("Form Heading", "View ZDummyForm", BoundForm.FormHeading);
		}

		public void TestFormCaptionDeactivate()
		{
			var cancellable = Factory.New<DummyCancellable>();
			cancellable.Factory.Save();
			cancellable.IsCancelled = true;
			using (var dummyForm = new ZDummyForm(cancellable))
			{
				dummyForm.DisplayMode = ODisplayMode.Delete;
				dummyForm.Show();
				AssertEquals("Form Text", "Deactivate ZDummyForm", dummyForm.Text);
				AssertEquals("Form Heading", "Deactivate ZDummyForm", dummyForm.FormHeading);
			}
			cancellable.IsCancelled = false;
			using (var dummyForm = new ZDummyForm(cancellable))
			{
				dummyForm.DisplayMode = ODisplayMode.Delete;
				dummyForm.Show();
				AssertEquals("Form Text", "Activate ZDummyForm", dummyForm.Text);
				AssertEquals("Form Heading", "Activate ZDummyForm", dummyForm.FormHeading);
			}
		}

		public void TestText()
		{
			Form.Text = "Text";
			AssertEquals("Text", Form.Text);
		}

		public void TestShouldSerializeText()
		{
			Form.Show();
			Application.DoEvents();

			AssertEquals("False by default", false, TypeDescriptor.GetProperties(Form)["Text"].ShouldSerializeValue(Form));
			Form.Text = "NewValue";
			AssertEquals("True when set explicitly", true, TypeDescriptor.GetProperties(Form)["Text"].ShouldSerializeValue(Form));
			Form.Text = "";
			AssertEquals("False when empty", false, TypeDescriptor.GetProperties(Form)["Text"].ShouldSerializeValue(Form));
		}

#if !WINZOR
		public void TestTopMostShouldNotBeOverwrittenWhenTrue()
		{
			using (var form = new ZForm())
			{
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				form.TopMost = true;
				form.Show();
				Assert("Top most should still be true.", form.TopMost);
			}
		}

		public void TestTopMostShouldNotBeTrueWhenZFormExplodes()
		{
			using (var explodingForm = new SilentlyExplodingZForm())
			{
				explodingForm.TopMost = false;
				ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
				try
				{
					explodingForm.Show();
				}
				catch (Exception ex)
				{
					if (ex.Message != "Boom.")
					{
						throw;
					}
				}
				Assert("Top most should still be false.", !explodingForm.TopMost);
				ErrorReporter.Clear();
			}
		}
#endif

		#endregion

		#region ActionDataMenuItem

		public void TestAllowActionDataMenuItem()
		{
			using (var testForm = new ZForm())
			{
				AssertEquals("Form's AllowActionDataMenuItem property should return false by default", false, testForm.AllowActionDataMenuItem);
			}
		}

		public void TestActionDataMenuNotAddedByDefault()
		{
			using (var testForm = new ZTestForm())
			{
				var dataMenuFound = IsMenuItemInCollection(testForm.ActionsMenuItem.MenuItems, "&Data");
				AssertEquals("Data menu item shouldn't be in the MenuItems collection by default", false, dataMenuFound);
			}

			ActionDataMenuSubclassForTesting.Initialise();

			using (var testForm = new ZTestForm())
			{
				var dataMenuFound = IsMenuItemInCollection(testForm.ActionsMenuItem.MenuItems, "&Data");
				AssertEquals("Still shouldn't find the data menu item - it hasn't been allowed", false, dataMenuFound);
			}
		}

		public void TestActionDataMenuAddedIfFormRequests()
		{
			using (var testForm = new ZTestFormForActionMenuItem(Dummy))
			{
				testForm.ActionsMenuItem.OnPopup(EventArgs.Empty);
				var dataMenuFound = IsMenuItemInCollection(testForm.ActionsMenuItem.MenuItems, "&Data");
				AssertEquals("Data menu item shouldn't be in the MenuItems collection by default", false, dataMenuFound);
			}

			ActionDataMenuSubclassForTesting.Initialise();

			using (var testForm = new ZTestFormForActionMenuItem(Dummy))
			{
				testForm.ActionsMenuItem.OnPopup(EventArgs.Empty);
				var dataMenuFound = IsMenuItemInCollection(testForm.ActionsMenuItem.MenuItems, "&Data");
				AssertEquals("Should have found the Data menu now that the Actions Menu subclass was initialised.", true, dataMenuFound);
			}
		}

		class ZTestFormForActionMenuItem : ZTestForm
		{
			public ZTestFormForActionMenuItem(IBusiness businessEntity) : base(businessEntity) { }

			protected internal override bool AllowActionDataMenuItem
			{
				get { return true; }
			}
		}

		static bool IsMenuItemInCollection(Menu.MenuItemCollection menuItems, string menuItemText)
		{
			return GetMenuItem(menuItems, menuItemText) != null;
		}

		static MenuItem GetMenuItem(Menu.MenuItemCollection menuItems, string menuItemText)
		{
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == menuItemText)
				{
					return item;
				}
			}
			return null;
		}

		public void TestActionDataMenuBusinessEntityIsFormsBusinessEntity()
		{
			ActionDataMenuSubclassForTesting.Initialise(); // hook up menu item and make it appear in form's menu item collection

			using (var testForm = new ZTestFormForActionMenuItem(Dummy))
			{
				var menu = (ActionDataMenuSubclassForTesting)GetMenuItem(testForm.ActionsMenuItem.MenuItems, "&Data");
				AssertNotNull("Action menu item should be in the menu item collection", menu);
				AssertEquals("ActionDataMenuItem's BusinessEntity should be the dummy", Dummy, menu.BusinessEntityForTesting);
			}
		}

		#endregion

		#region Test Classes

		class TestForm : ZForm
		{
#if !WINZOR

			public new bool ProcessTabKey(bool forward)
			{
				return base.ProcessTabKey(forward);
			}

#endif
		}

		class HasChangesTestForm : ZTestForm
		{
			public HasChangesTestForm(DummyBusinessObject bO) : base(bO) { }

			public override IBusiness BusinessEntityForHasChanges
			{
				get { return ((DummyBusinessObject)BusinessEntity).Collection; }
			}
		}

		class TestWinFormForSaving : ZForm
		{
			public ZToolStripButton SaveButton;
			public ZToolStripButton SaveAndCloseButton;
			public ZToolStripButton CancelBtn;

			public TestWinFormForSaving(IBusiness businessEntity)
				: base(businessEntity)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, SaveAndCloseButton, CancelBtn, SaveButton);
			}

			// buttons randomly change caption to "new" - see OWinform code
			public bool HasNewButton()
			{
				return (SaveButton.Visible && SaveButton.Text.IndexOf("New") != -1) ||
					(SaveAndCloseButton.Visible && SaveAndCloseButton.Text.IndexOf("New") != -1) ||
					(CancelBtn.Visible && CancelBtn.Text.IndexOf("New") != -1);
			}

			public int ValidateAndSaveCallCount;
			public bool ActuallyDoTheSave;
			protected internal override ContinueWithSave ValidateAndSave()
			{
				ValidateAndSaveCallCount++;
				return ActuallyDoTheSave ? base.ValidateAndSave() : ContinueWithSave.No;
			}

			public int DeleteCallCount;
			protected override ContinueWithDelete ShowPreDeleteDialogs()
			{
				DeleteCallCount++;
				return ContinueWithDelete.No;
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				var toolStrip = new ZToolStrip();
				SaveButton = new ZToolStripButton();
				SaveAndCloseButton = new ZToolStripButton();
				CancelBtn = new ZToolStripButton();
				SaveButton.Width = 50;
				SaveAndCloseButton.Width = 50;
				CancelBtn.Width = 50;
				SaveButton.Height = 23;
				SaveAndCloseButton.Height = 23;
				CancelBtn.Height = 23;

				toolStrip.Items.AddRange(new[] { SaveButton, SaveAndCloseButton, CancelBtn });
				Controls.Add(toolStrip);
			}
		}

		class TestSizeForm : ZForm
		{
			public TestSizeForm(IBusiness businessEntity) : base() { }

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				ClientSize = new Size(1016, 691);
			}
		}

		class SilentlyExplodingZForm : ZForm
		{
			public SilentlyExplodingZForm()
				: base()
			{
			}

			protected override void ThrowExceptionForTestIfNeeded()
			{
				throw new Exception("Boom.");
			}
		}

		protected class TestClientHook : ClientHook
		{
			#region ClientHook Members

			public override string ClientDisplayName
			{
				get { return null; }
			}

			string fHelpWebPage = "http://www.cargowise.com/";

			public void SetHelpWebPage(string value)
			{
				fHelpWebPage = value;
			}

			public override string HelpWebPage
			{
				get { return fHelpWebPage; }
			}

			public override ITypeDeciderDictionary ClientTypeDeciders
			{
				get { return null; }
			}

			protected override void UninitialiseCore()
			{
			}

			protected override void InitialiseCore()
			{
			}

			protected override ControllerOverrides GetControllerOverrides() => null;

			protected override ModuleOverrides GetModuleOverrides() => null;

			public override Clients Client
			{
				get { return new Clients(); }
			}

			public override IExtensionObjects DbSchemaExtensionObjects => null;

			#endregion
		}

		protected class OWinFormTestGrid : ZGrid
		{
			public void SendCmdKey(Message m, Keys keyData)
			{
				ProcessCmdKey(ref m, keyData);
			}
		}

		#endregion

		#region Implementation

		DataRow Row;
#pragma warning disable CW1108 // Do Not Use DataSet
		DataSet Data; // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet

		TestForm Form
		{
			get { return form ?? (form = new TestForm()); }
		}
		TestForm form;

		ZDummyForm BoundForm
		{
			get
			{
				if (boundForm == null)
				{
					boundForm = new ZDummyForm(Dummy);
				}
				return boundForm;
			}
		}
		ZDummyForm boundForm;

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();

			var table = new DataTable("TestTable");
			table.Columns.Add(new DataColumn("TestColumn"));
			table.Columns.Add(new DataColumn("TestColumn2"));

			var gridTable = new DataTable("TestGridTable");
			gridTable.Columns.Add(new DataColumn("TestTextGridColumn"));
#pragma warning disable CW1108 // Do Not Use DataSet
			Data = new DataSet(); // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet
			Data.Tables.Add(table);
			Data.Tables.Add(gridTable);

			Row = table.NewRow();
			table.Rows.Add(Row);

			gridTable.Rows.Add(gridTable.NewRow());
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (form != null)
			{
				form.Dispose();
			}
			if (boundForm != null)
			{
				boundForm.Dispose();
			}
#if !WINZOR
			if (textBox1 != null)
			{
				textBox1.Dispose();
			}
			if (textBox2 != null)
			{
				textBox2.Dispose();
			}
			if (textBox3 != null)
			{
				textBox3.Dispose();
			}
#endif
			UserIdleWorker.Flush();
		}

		#endregion

		static void InputToTextBox(ZTextBox textBox, string input)
		{
			var keysConverter = new KeysConverter();
			textBox.Focus();

			foreach (var ch in input)
			{
				var key = (Keys)(byte)char.ToUpper(ch);
				KeySender.PostKeyDown(textBox, key);
				Application.DoEvents();
			}

			KeySender.PostKeyDown(textBox, Keys.Enter);
			Application.DoEvents();
		}

		public static void SetFilterStrip(Form form, int filterIndex, string filterDescription, string comparisonOperator, string value)
		{
			var filterStrip = (ZFilterStrip)(form.Controls.Find("ZFilterStrip", true)[filterIndex]);
			InputToTextBox(filterStrip.FilterDescriptionDropEdit.CodeBox, filterDescription);

			var currentFilterControls = filterStrip.CurrentFilterControls;
			InputToTextBox(((ZDropEdit)currentFilterControls[0]).CodeBox, comparisonOperator);
			InputToTextBox((ZTextBox)(currentFilterControls[1]), value);
		}

		public class TestZFormHandleSaveException : ZForm
		{
			public void HandleSaveException_Exposed(Exception e) => HandleSaveException(e);
		}

		public void TestHandleSaveException()
		{
			using (var testForm = new TestZFormHandleSaveException())
			{
				testForm.HandleSaveException_Exposed(SqlExceptionBuilder.CreateSqlException(1, "fail"));
				AssertEquals("Error occurred trying to save to the database. Please try to Save again or contact your systems administrator to check the database for errors. fail",
					UnitTestUserNotification.Instance.LastMessage.Text);

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.S3))
				{
					testForm.HandleSaveException_Exposed(SqlExceptionBuilder.CreateSqlException(1, "Invalid object name 'OdysseyDat_SD001.dbo.StorageDocs'."));
					AssertEquals("Error occurred trying to save to the database. eDocs Storage registry setting is set to S3 compatible storage and it is incorrectly configured. Contact your system administrator.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EDocsStorageProviders.Code.DB))
				{
					testForm.HandleSaveException_Exposed(SqlExceptionBuilder.CreateSqlException(1, "Invalid object name 'OdysseyDat_SD001.dbo.StorageDocs'."));
					AssertEquals("Error occurred trying to save to the database. Please try to Save again or contact your systems administrator to check the database for errors. Invalid object name 'OdysseyDat_SD001.dbo.StorageDocs'.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleSaveEmptyEDocException()
		{
			using (var testForm = new TestZFormHandleSaveException())
			{
				var emptyContentException = new EmptyContentEDocsException();
				testForm.HandleSaveException_Exposed(emptyContentException);
				AssertEquals("Error occurred trying to save to the database. Error: 'The eDoc content cannot be empty.'. Please try to Save again. If error repeats, please reopen form and try again.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleSaveException_LoopbackLinkedServerDoesNotExist_FriendlyMessage()
		{
			using (var testForm = new TestZFormHandleSaveException())
			{
				var sqlException = SqlExceptionBuilder.CreateSqlException(7202, "Could not find server 'LOOPBACK' in sys.servers. Verify that the correct server name was specified.");
				testForm.HandleSaveException_Exposed(sqlException);
				AssertEquals("The system cannot perform this operation as a crucial server configuration object is missing. The 'ConfigureServer' procedure has not been run on this server. Please contact your system administrator.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public class TestZFormHandleSaveButton_ExceptionHandle_Exception : ZForm
		{
			public TestZFormHandleSaveButton_ExceptionHandle_Exception(object dataSource) : base(dataSource) { }

			public void HandleSaveButton() => HandleSaveWhileClosing(new CancelEventArgs());

			//mock to throw inner exception
			protected override void HandleSaveException(Exception ex)
			{
				base.HandleSaveException(ex);
				ThrowMockException();
			}

			void ThrowMockException()
			{
				throw new Exception("Throw inner exception", new ApplicationException("Attempted to return a parent type when a sub type was requested.(AllowMultipleBusinessObjectsAroundOneRow=true, SingleObjectAroundARow=true)"));
			}
		}

		public void TestHandleSaveButton_ExceptionHandle_ExceptionKey()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Factory.Save();

			var dummyInOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyBusinessObject>(dummy.PK);

			using (var testForm = new TestZFormHandleSaveButton_ExceptionHandle_Exception(dummyInOtherFactory))
			using (var cancelButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(testForm, null, cancelButton, null);
				testForm.Show();

				dummy.Delete();
				dummy.Factory.Save();

				dummyInOtherFactory.Z0_Number = 234;
				Assert("Precondition", dummyInOtherFactory.HasChanges);

				Assert("Form's business entity is not touched yet", !dummyInOtherFactory.IsDeleted);
				Assert("Form should be still visible", testForm.Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				testForm.HandleSaveButton();
				AssertEquals("Exception Message", "Attempted to return a parent type when a sub type was requested.(AllowMultipleBusinessObjectsAroundOneRow=true, SingleObjectAroundARow=true)", ExceptionReporterTestListener.Instance[0].InnerException.Message);
				AssertStartsWith("Exception Key", "HandleSaveButton_ExceptionHandle_Exception|Attempted to return a parent type when a sub type was requested.(AllowMultipleBusinessObjectsAroundOneRow=true, SingleObjectAroundARow=true)|ThrowMockException|HandleSaveButton_Exception|", ExceptionReporterTestListener.Instance.GetExceptionKey(0));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

#if !WINZOR
		[ExpectNoExceptions]
		public void TestGetOwnerInDispose_WhenFinalizedByGC_ShouldNotThrowException()
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			using var form = new TestZFormForFinalizedByGC() { Name = "TestZFormForFinalizedByGC" };
			form.TrackDisposedAccess = false;

			var propertyStoreField = typeof(Control).GetField("propertyStore", BindingFlags.NonPublic | BindingFlags.Instance);
			var oldPropertyStoreFieldValue = propertyStoreField.GetValue(form);
			propertyStoreField.SetValue(form, null);

			var propertiesField = typeof(Control).GetProperty("Properties", BindingFlags.NonPublic | BindingFlags.Instance);
			AssertNull("Form.Properties", propertiesField.GetValue(form));

			AssertNoExceptionThrown(() => form.DisposeByGC());

			propertyStoreField.SetValue(form, oldPropertyStoreFieldValue);
			var isDisposedField = typeof(KForm).GetField("isDisposed", BindingFlags.NonPublic | BindingFlags.Instance);
			isDisposedField.SetValue(form, false);
		}

		class TestZFormForFinalizedByGC : ZForm
		{
			public void DisposeByGC()
			{
				base.Dispose(false);
			}
		}
#endif
	}

	public class ZFormNonTransactionedTest : NonTransactionedTestCase
	{
		#region Running in Background

		public
#if WINZOR
		async
#endif
			void TestShouldNotErrorReportWhenRunningInBackground()
		{
			SystemDataRegistry.Instance.ErrorReportingFormsRunningInBackground.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ErrorReporter.Clear();
			Exception threadException = null;
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
#if WINZOR
			await DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(Action);
#else
			var thread = DefaultAsyncStrategy.Get().RunInAnotherWinformsThreadAsync(Action);
			thread.Join();
#endif

			AssertNull(threadException);
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			void Action()
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						var form = new ZForm();
						form.Show();
						Application.DoEvents();
						form.Dispose();
					}
				}
				catch (Exception ex)
				{
					threadException = ex;
				}
			}
		}
		#endregion
	}

	#region Support Classes

	internal class TestTextBox : ZTextBox, IDynamicToolTip
	{
		#region IDynamicToolTip Members

		public event QueryToolTipEventHandler QueryToolTip;

		public void GetToolTip(ToolTipInfo info)
		{
			if (QueryToolTip != null)
			{
				QueryToolTip(this, info);
			}
		}

		#endregion
	}

	internal class TestZForm : ZForm
	{
		public TestZForm(object datasource)
			: base(datasource)
		{
			Setup();
		}

		public TestZForm()
		{
			Setup();
		}

		protected internal override bool ShouldRememberPositionAndSize => false;

		void Setup()
		{
			TextBox1 = new TestTextBox { Name = "TestColumnBoundTextBox", CaptionResourceString = Res.GetData("1A904F16-BB54-4100-B005-6D361C8F1F99", "Test Caption") };
			TextBox2 = new TestTextBox { Name = "TestColumn2BoundTextBox", CaptionResourceString = Res.GetData("1A904F16-BB54-4100-B005-6D361C8F1F99", "Test Caption") };
			CancelBoundButton = new Button();
			PostBoundButton = new Button();
			ApplyBoundButton = new Button();

			PostBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 224);
			CancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 224);
			ApplyBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 224);

			Controls.AddRange(new Control[] { TextBox1, TextBox2, PostBoundButton, CancelBoundButton, ApplyBoundButton });
		}

		internal TestTextBox TextBox1;
		internal TestTextBox TextBox2;
		public Button CancelBoundButton;
		public Button PostBoundButton;
		public Button ApplyBoundButton;

		public string MessageStatusBarText
		{
			get { return MessageStatusBarPanel.Text; }
		}

		public string ErrorStatusBarText
		{
			get { return ErrorStatusBarPanel.Text; }
		}

		public string FileSaveAndCloseMenuItemText
		{
			get
			{
				var fileSaveAndCloseMenuItem = FileMenuItem.MenuItems.FindByName(ZFormMenuStrategy.FileSaveAndCloseMenuItemName);
				return fileSaveAndCloseMenuItem != null ? fileSaveAndCloseMenuItem.Text : string.Empty;
			}
		}

		public new MenuItem EditMenuItem
		{
			get { return base.EditMenuItem; }
		}

		public new MenuItem HelpClientSpecificMenuItem
		{
			get { return base.HelpClientSpecificMenuItem; }
		}

		public MenuItem ExposedActionMenuItem
		{
			get { return ActionsMenuItem; }
		}

		protected override DialogResult ShowConfirmationForDelete()
		{
			return DialogResult.Yes;
		}

		public void OnFactoryChangedForTest()
		{
			OnFactoryChanged();
		}

		public void PerformApply()
		{
			base.OnApplyButtonClick(this.ApplyBoundButton, EventArgs.Empty);
		}
	}

	#endregion

	class Test : TestCaseWithDummy
	{
		public void TestIsResizableByTabPageAllowed()
		{
			var formMock = new Mock<ZForm>() { CallBase = true };
			using (var testForm = formMock.Object)
			{
				formMock.SetupGet(o => o.IsResizableByTabPageAllowed).Returns(false);
				AssertEquals("IsResizableByTabPageAllowed", false, testForm.IsResizableByTabPageAllowed);

				formMock.SetupGet(o => o.IsResizableByTabPageAllowed).Returns(true);
				AssertEquals("IsResizableByTabPageAllowed", true, testForm.IsResizableByTabPageAllowed);
				formMock.VerifyAll();
			}
		}

		#region Save and Validate

#if !WINZOR
		class BusinessobjectFactoryWithFormValidationAndSaveNoSqlException : BusinessObjectFactory
		{
			public override BusinessObject[] Load(Type bizOType, ZQuery sqlFilter)
			{
				if (sqlFilter.ParameterisedText.LiteralTextSql.Contains("and Z0_PK =", StringComparison.InvariantCultureIgnoreCase))
				{
					throw SqlExceptionBuilder.CreateSqlException(8623, "");
				}
				else
				{
					return base.Load(bizOType, sqlFilter);
				}
			}
		}

		public void TestValidateAndSave_NoSqlException()
		{
			var factory = new BusinessobjectFactoryWithFormValidationAndSaveNoSqlException();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = "Default";
			dummy.Z0_Code = "ABCDE";
			factory.Save();

			var collection = new DummyBusinessObjectCollection(new BusinessobjectFactoryWithFormValidationAndSaveNoSqlException());
			collection.IsManagedForDataRefresh = true;
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_Code, dummy.Z0_Code);
			collection.Load(query);

			using (var testForm = new ZForm(dummy))
			{
				dummy.Z0_Description = "A";
				AssertNoExceptionThrown(() => testForm.ValidateAndSave());
				AssertEquals(collection[0].Z0_Description, "A");
			}
		}
#endif

		public void TestShowMessageIfTransactionHasBeenRolledBackInTheServer()
		{
			void RollbackTransaction()
			{
				throw new TransactionException("Transaction has been rolled back in the server (application transaction count pending reset).", OdysseyDataErrorType.TransactionRolledBack);
			}
			using (var parentForm = new ZDummyForm(Dummy))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				parentForm.Show();
				Dummy.OnSavingHook = () => RollbackTransaction();
				parentForm.FireSaveButton();

				AssertEquals("There was transaction related error on DB server. Please try to Save again. If error repeats, please reopen form and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExceptionReportOnSave()
		{
			void ExceptionThrowerA()
			{
				throw new Exception("From A");
			}

			void ExceptionThrowerB()
			{
				throw new Exception("From B");
			}

			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			ExceptionReporter.SuppressGui();

			try
			{
				using (var parentForm = new ZDummyForm(Dummy))
				{
					parentForm.Show();

					Dummy.OnSavingHook = () => ExceptionThrowerA();
					parentForm.FireSaveButton();

					var result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
					var document = XDocument.Parse(result);
					var key = document.XPathSelectElement("//Key");
					AssertEquals("Report count", 1, ExceptionReporter.Instance.TotalReportCount);
					AssertContains("Key contains orginator", "ExceptionThrowerA", key.ToString());

					new BaseExceptionReporter().Enable();
					ExceptionReporter.Instance.TestingDoReportException.Value = true;
					ExceptionReporter.SuppressGui();

					Dummy.OnSavingHook = () => ExceptionThrowerB();
					parentForm.FireSaveButton();

					result = (string)Db.Connection.ExecuteScalar("SELECT QER_ReportXml FROM dbo.StmErrorReport ORDER BY QER_SystemCreateTimeUtc DESC");
					document = XDocument.Parse(result);
					key = document.XPathSelectElement("//Key");
					AssertEquals("Report count", 1, ExceptionReporter.Instance.TotalReportCount);
					AssertContains("Key contains orginator", "ExceptionThrowerB", key.ToString());
				}
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
				ExceptionReporter.Instance.TotalReportCount = 0;
			}
		}

		public void TestLightValidationOnSave()
		{
			EnvProxy.Instance.Registry.LightValidationEnabled = true;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyDependantBusinessObject>();
			Dummy.RegisterEditableChildObject(dummy2);
			Dummy.RegisterEditableChildObject(dummy3);
			using (var parentForm = new ZDummyForm(Dummy))
			{
				parentForm.Show();
				AssertEquals(0, Dummy.RunPreSaveValidationCount);
				AssertEquals(0, dummy2.RunPreSaveValidationCount);
				AssertEquals(0, dummy3.RunPreSaveValidationCount);

				parentForm.FireSaveButton();
				AssertEquals(1, Dummy.RunPreSaveValidationCount);
				AssertEquals(1, dummy2.RunPreSaveValidationCount);
				AssertEquals(1, dummy3.RunPreSaveValidationCount);

				dummy2.Z0_Description = "teapot";
				parentForm.FireSaveButton();
				AssertEquals("Dummy does not need to be validated due to light validation", 1, Dummy.RunPreSaveValidationCount);
				AssertEquals("Dummy2 has been changed but is still valid so does not need to run validation", 1, dummy2.RunPreSaveValidationCount);
				AssertEquals("Dummy3 does not have light validation so should be validated every time", 2, dummy3.RunPreSaveValidationCount);
			}
		}

		public void TestFullValidationOnValidateMenu()
		{
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyDependantBusinessObject>();
			Dummy.RegisterEditableChildObject(dummy2);
			Dummy.RegisterEditableChildObject(dummy3);
			using (var parentForm = new ZDummyForm(Dummy))
			{
				parentForm.Show();
				AssertEquals(0, Dummy.RunPreSaveValidationCount);
				AssertEquals(0, dummy2.RunPreSaveValidationCount);
				AssertEquals(0, dummy3.RunPreSaveValidationCount);

				parentForm.FireSaveButton();
				AssertEquals(1, Dummy.RunPreSaveValidationCount);
				AssertEquals(1, dummy2.RunPreSaveValidationCount);
				AssertEquals(1, dummy3.RunPreSaveValidationCount);

				parentForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.ValidateMenuItemName].PerformClick();
				AssertEquals(2, Dummy.RunPreSaveValidationCount);
				AssertEquals(2, dummy2.RunPreSaveValidationCount);
				AssertEquals(2, dummy3.RunPreSaveValidationCount);
			}
		}

		public void TestValidateWithProgressBox_ProgressReporter()
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = true;
			try
			{
				var dummy = Factory.New<DummyWithValidateWithProgressReporter>();

				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(dummy))
				{
					testForm.Show();
					testForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.ValidateMenuItemName].PerformClick();
					AssertEquals("Thread1 log",
@"1: SaveProgressMediator constructed
1: Request show form with status: 'Loading validation code...' percentComplete: 5
1: Request update status: 'Updating plug-ins...' percentComplete: 8
1: Request update status: 'Validating data...' percentComplete: 15
1: Request update status: 'Processing 1/3' percentComplete: 18
1: Request update status: 'Processing 2/3' percentComplete: 18
1: Request update status: 'Processing 3/3' percentComplete: 18
1: Request update status: 'Calculating errors, warnings and message errors...' percentComplete: 50",
						logger.LogThread1.ToString());

					Application.DoEvents();
					if (System.Windows.Forms.Form.ActiveForm != null)
					{
						AssertEquals("Form is active", testForm.GetType(), System.Windows.Forms.Form.ActiveForm.GetType());
					}
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		class DummyWithValidateWithProgressReporter : DummyBusinessObject, IProgressReporterValidation
		{
			public DummyWithValidateWithProgressReporter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IDisposable IProgressReporterValidation.ReportProgress(Action<string> progress)
			{
				this.progress = progress;
				return new DisposableAction(() => this.progress = null);
			}
			Action<string> progress;

			protected override void RunPreSaveValidationCore()
			{
				progress?.Invoke("Processing 1/3");
				progress?.Invoke("Processing 2/3");
				progress?.Invoke("Processing 3/3");
			}
		}

		public void TestValidateWithProgressBox()
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = true;
			try
			{
				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(Dummy))
				{
					testForm.Show();
					testForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.ValidateMenuItemName].PerformClick();
					AssertEquals("Thread1 log",
@"1: SaveProgressMediator constructed
1: Request show form with status: 'Loading validation code...' percentComplete: 5
1: Request update status: 'Updating plug-ins...' percentComplete: 8
1: Request update status: 'Validating data...' percentComplete: 15
1: Request update status: 'Calculating errors, warnings and message errors...' percentComplete: 50",
						logger.LogThread1.ToString());

					Application.DoEvents();
					if (System.Windows.Forms.Form.ActiveForm != null)
					{
						AssertEquals("Form is active", testForm.GetType(), System.Windows.Forms.Form.ActiveForm.GetType());
					}
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		public void TestSaveToCompletionWithProgressBox()
		{
			AssertSaveToCompletionWithProgressBox(Dummy);
		}

		public void TestSaveToCompletionWithProgressBox_TemplateRecord()
		{
			var templateRecord = Factory.New<DummyTemplateRecord>();
			AssertSaveToCompletionWithProgressBox(templateRecord);
		}

		void AssertSaveToCompletionWithProgressBox(BusinessObject bizObj)
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = true;
			try
			{
				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(bizObj))
				{
					testForm.Show();
					testForm.FireSaveButton();
					AssertEquals("Thread1 log",
@"1: SaveProgressMediator constructed
1: Request show form with status: 'Loading validation code...' percentComplete: 5
1: Request update status: 'Updating plug-ins...' percentComplete: 8
1: Request update status: 'Validating data...' percentComplete: 15
1: Request update status: 'Calculating errors, warnings and message errors...' percentComplete: 50
1: Request update status: 'Checking data...' percentComplete: 60
1: Request update status: 'Saving to the database...' percentComplete: 80
1: Request update status: 'Saved successfully.' percentComplete: 100",
						logger.LogThread1.ToString());

					Application.DoEvents();
					if (System.Windows.Forms.Form.ActiveForm != null)
					{
						AssertEquals("Form is active", testForm.GetType(), System.Windows.Forms.Form.ActiveForm.GetType());
					}
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		public void TestSaveWithErrorsWithProgressBox()
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = true;
			try
			{
				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(Dummy))
				{
					testForm.Show();
					Dummy.Z0_Description = "Bad";
					testForm.FireSaveButton();
					AssertEquals("Thread1 log",
@"1: SaveProgressMediator constructed
1: Request show form with status: 'Loading validation code...' percentComplete: 5
1: Request update status: 'Updating plug-ins...' percentComplete: 8
1: Request update status: 'Validating data...' percentComplete: 15
1: Request update status: 'Calculating errors, warnings and message errors...' percentComplete: 50
1: Request update status: 'Checking data...' percentComplete: 60",
						logger.LogThread1.ToString());
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		public void TestSaveWithErrorsWithFactoryValidationSuspended()
		{
			using (Dummy.GetValidationSuspender())
			{
				Dummy.Z0_Description = "Bad";
				Dummy.Factory.Save();

				var dummy2 = new BusinessObjectFactory().Load<DummyBusinessObject>(Dummy.PK);
				dummy2.Factory.SuspendValidation();
				using (var testForm = new ZTestForm(dummy2))
				{
					testForm.Show();
					testForm.FireSaveButton();
					AssertEquals(0, dummy2.GetErrors().Count());
				}

				var dummy3 = new BusinessObjectFactory().Load<DummyBusinessObject>(Dummy.PK);
				using (var testForm = new ZTestForm(dummy3))
				{
					testForm.Show();
					testForm.FireSaveButton();
					AssertEquals(1, dummy3.GetErrors().Count());
				}
			}
		}

		public void TestSaveWhenProgressBoxIsDisabled()
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = false;
			try
			{
				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(Dummy))
				{
					testForm.Show();
					testForm.FireSaveButton();
					AssertEquals("", logger.LogThread1.ToString());
					AssertEquals("", logger.LogThread2.ToString());
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		public void TestValidateWhenProgressBoxIsDisabled()
		{
			var oldVal = EnvProxy.Instance.Registry.ShowSaveProgressBox;
			EnvProxy.Instance.Registry.ShowSaveProgressBox = false;
			try
			{
				using (var logger = new SaveProgressMediator.Logger())
				using (var testForm = new ZTestForm(Dummy))
				{
					testForm.Show();
					testForm.Menu.MenuItems[ZFormMenuStrategy.FileMenuItemName].MenuItems[ZFormMenuStrategy.ValidateMenuItemName].PerformClick();
					AssertEquals("", logger.LogThread1.ToString());
					AssertEquals("", logger.LogThread2.ToString());
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowSaveProgressBox = oldVal;
			}
		}

		public void TestValidateCreatesAndExecutesFetchHints()
		{
			Factory.AddFetchHint(StmNoteSchema.ST_Description, (ZString)"Hello");
			var mockDummy = new Mock<DummyBusinessObject>(Factory, new RowFactory(Factory).New(DummyBusinessObject.Schema.TableName)) { CallBase = true };
			using (var testForm = new ZForm(mockDummy.Object))
			{
				var mockFetchStrategy = new Mock<IBusinessObjectFetchStrategy>();
				mockDummy.Protected().Setup<IBusinessObjectFetchStrategy>("GetFetchStrategy").Returns(mockFetchStrategy.Object);
				testForm.ValidateAll(ValidationType.Light);
				mockFetchStrategy.Verify(o => o.FetchForValidate(), Times.Once());
				var hits = Factory.TableSelects;
				AssertEquals(1, hits.Length);
				AssertEquals("StmNote", hits[0].TableName);
				AssertEquals(1, hits[0].Value);
			}
		}

		[ExpectNoExceptions]
		public void TestSaveInternal_HandleNotCreated()
		{
			var testForm = new ZForm(Factory.New(typeof(DummyBusinessObject)));
			testForm.Hide();
			AssertEquals("Handle discarded for the test", false, testForm.IsHandleCreated);
			testForm.SaveInternal();
			testForm.Dispose();
		}

		[ExpectNoExceptions]
		public void TestPreSaveDialogsDoesNotRequireTheFormToHaveABusinessEntity()
		{
			using (var testForm = new ZForm())
			{
				testForm.ShowPreSaveDialogs();
			}
		}

		[ExpectNoExceptions]
		public void TestShowPreSaveDialogsCallsBusinessObject()
		{
			var mock = new Mock<IBusiness>();
			using (var testForm = new ZForm(mock.Object))
			{
				mock.SetupGet(o => o.CanContinueWithSave).Returns(true).Verifiable();
				testForm.ShowPreSaveDialogs();
				mock.Verify();
			}
		}

		public void TestShowPreSaveDialogsTranslatesBusinessObjectReturnValue()
		{
			var mock = new Mock<IBusiness>();
			using (var testForm = new ZForm(mock.Object))
			{
				mock.SetupGet(o => o.CanContinueWithSave).Returns(true);
				AssertEquals(ContinueWithSave.Yes, testForm.ShowPreSaveDialogs());
				mock.SetupGet(o => o.CanContinueWithSave).Returns(false);
				AssertEquals(ContinueWithSave.No, testForm.ShowPreSaveDialogs());
			}
		}

		public void TestValidateAndSave_RunsPreSaveValidation()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			var dummyDependent = dummy.Dependents.AddNew();
			using (dummyDependent.GetValidationSuspender())
			{
				dummyDependent.ZD1_Code = "error";
			}

			using (var testForm = new ZFormWithBoundGridOnTabPage(dummy))
			{
				testForm.Show();
				Application.DoEvents();

				AssertNoErrors("No errors initially for the test", dummyDependent.ZD1_CodeInfo);
				testForm.ValidateAndSave();
				AssertHasErrors("Errors should exist now that we have called ValidateAll()", dummyDependent.ZD1_CodeInfo);
			}
		}

		public void TestValidateAndSave_DataBindingIsNull()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			var dummyDependent = dummy.Dependents.AddNew();

			using (var testForm = new ZFormWithBoundGridOnTabPage(dummy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.SetDataBinding(null, "");

				AssertNull(testForm.BusinessEntity);
				AssertNull(testForm.BusinessEntityForValidation);
				AssertNull(testForm.DataSource);

				testForm.ValidateAndSave();
				AssertEquals("", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestValidateAll_RunsPreSaveValidation()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			var dummyDependent = dummy.Dependents.AddNew();
			using (dummyDependent.GetValidationSuspender())
			{
				dummyDependent.ZD1_Code = "error";
			}

			using (var testFomr = new ZFormWithBoundGridOnTabPage(dummy))
			{
				testFomr.Show();
				Application.DoEvents();

				AssertNoErrors("No errors initially for the test", dummyDependent.ZD1_CodeInfo);
				testFomr.ValidateAll(ValidationType.Light);
				AssertHasErrors("Errors should exist now that we have called ValidateAll()", dummyDependent.ZD1_CodeInfo);
			}
		}

		public void TestValidateAll_CallsSynchronisePlugInsOnSave()
		{
			var dummy = Factory.New<DummyWithDependentsBusinessObject>();
			using (var testForm = new ZForm(dummy))
			{
				testForm.PlugIns.Add(DummyControllerIDs.Dummy1);
				testForm.Show();
				Application.DoEvents();

				var plugIn = (DummyPlugIn1)testForm.PlugIns.Instances[0];
				plugIn.Setup();

				AssertEquals("OnSaving should not be called initially for the test", 0, plugIn.OnSavingCount);
				testForm.ValidateAll(ValidationType.Light);
				AssertEquals("OnSaving should be called in ValidateAll()", 1, plugIn.OnSavingCount);
			}
		}

		[ExpectNoExceptions]
		public void TestValidateAll_WhenFormIsDisposed()
		{
			using (var testForm = new ZForm(null))
			{
				testForm.Dispose();
				testForm.ValidateAll(ValidationType.Light);
			}
		}

		#endregion

		public void TestIHaveTooltipsForMigration()
		{
			using (var control = new ZForm())
			{
				IHaveTooltipsForMigration tooltips = control;
				var someControl = new Control();
				tooltips.ToolTipForMigration.SetToolTip(someControl, "blah");
				AssertEquals("blah", tooltips.ToolTipForMigration.GetToolTip(someControl));
				AssertEquals(control.FormToolTip, tooltips.ToolTipForMigration);
			}
		}

		#region Copying to Clipboard

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCopyLinkToClipboard()
		{
			Form.Show();
			Form.ControllerID = DummyControllerIDs.Dummy;
			Application.DoEvents();

			Form.ActionsMenuItem.MenuItems.FindByText("Copy Hyperlink to Clipboard").PerformClick();
			Thread.Sleep(50);

			AssertEquals("Text", Dummy.HumanReadableName, SafeClipboard.GetData(DataFormats.Text));
			AssertContains("<html><body><!--StartFragment--><a href=\"" + ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK) + "\">" + Dummy.HumanReadableName + "</a><!--EndFragment--></body></html>", SafeClipboard.GetData(DataFormats.Html).ToString());

			var rtf = (string)SafeClipboard.GetData(DataFormats.Rtf);
			AssertEquals("Rtf link", true, rtf.Contains(ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK)));
			AssertEquals("Rtf caption", true, rtf.Contains(Dummy.HumanReadableName));
		}
#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1089:DoNotUseAssemblyGetEntryAssemblyAnalyzer", Justification = "Baseline")]
		public void TestCreateShortcutsOnDesktop()
		{
			Form.Show();
			Form.ControllerID = DummyControllerIDs.Dummy;
			Application.DoEvents();

			var shortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), Dummy.HumanReadableName + " (2)" + ".url");
			if (File.Exists(shortcutFile))
			{
				File.Delete(shortcutFile);
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Form.ActionsMenuItem.MenuItems.FindByText("Create Desktop Shortcut").PerformClick();
			AssertEquals("A shortcut shouldn't be created unless the user clicks OK", false, File.Exists(shortcutFile));

			var existingShortcutFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), Dummy.HumanReadableName + ".url");
			File.WriteAllText(existingShortcutFile, "");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			try
			{
				Form.ActionsMenuItem.MenuItems.FindByText("Create Desktop Shortcut").PerformClick();
				AssertEquals("A shortcut file should be created", true, File.Exists(shortcutFile));
				AssertEquals("Shortcut file content",
@"[InternetShortcut]
URL=" + ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, Dummy.PK) + @"
IconIndex=0
IconFile=" + new Uri(Assembly.GetEntryAssembly().Location).LocalPath + @"
", File.ReadAllText(shortcutFile));
			}
			finally
			{
				File.Delete(existingShortcutFile);
				File.Delete(shortcutFile);
			}
		}
#endif
		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestCopyFormToClipboard()
		{
			Form.Show();
			Application.DoEvents();

			var menuItem = Form.ActionsMenuItem.MenuItems.FindByText("Copy Form to Clipboard");
			var action = new Action(() => menuItem.PerformClick());
			action.Invoke();
			Thread.Sleep(50);

			var data = ClipboardTestHelper.RetryIfCopyOrCutFailed<object>(action, DataFormats.Bitmap);
			AssertEquals("Bitmap copied to clipboard", true, data != null);
		}

		#endregion

		#region Previous & Next Button Support

		public void TestHasNoPreviousNextButtonsIfNotBroughtUpByController()
		{
			using (var testForm = new ZForm(Dummy))
			{
				Assert("No control as not brought up by controller", !HasPreviousNextUserControl(testForm));
			}
		}

		public void TestAutoAddPreviousNextButtonsDefault()
		{
			var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
			try
			{
				controller.ShowNewForm();
				Assert("Should have previous next", HasPreviousNextUserControl((ZForm)controller.LastShownForm));
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}
			}
		}

		public void TestAutoAddPreviousNextButtonsFalse()
		{
			var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
			try
			{
				ZDummyForm.OverrideDefaultAddPreviousNextValue = true;
				ZDummyForm.OverridenAddPreviousNextValue = false;
				controller.ShowNewForm();
				Assert("Should not have previous next", !HasPreviousNextUserControl((ZForm)controller.LastShownForm));
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}

				ZDummyForm.ResetPreviousNextOverrideValues();
			}
		}

		public void TestAutoAddPreviousNextButtonsTrue()
		{
			var controller = ZControllerFactory.Create(DummyControllerIDs.Dummy);
			try
			{
				ZDummyForm.OverrideDefaultAddPreviousNextValue = true;
				ZDummyForm.OverridenAddPreviousNextValue = true;
				controller.ShowNewForm();
				Assert("Should have previous next", HasPreviousNextUserControl((ZForm)controller.LastShownForm));
			}
			finally
			{
				if (controller.LastShownForm != null)
				{
					controller.LastShownForm.Dispose();
				}

				ZDummyForm.ResetPreviousNextOverrideValues();
			}
		}

		static bool HasPreviousNextUserControl(ZForm testForm)
		{
			foreach (Control control in testForm.Controls)
			{
				if (control is ZPreviousNextControl)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		public void TestGetMaoduleId()
		{
			Form.Show();
			Form.ControllerID = DummyControllerIDs.Dummy;
			AssertEquals("Dummy", Form.GetModule().ID.Name);
		}

		public void TestGetModuleId_OnSecondAttempt()
		{
			Form.Show();
			Form.GetModule();

			Form.ControllerID = DummyControllerIDs.Dummy;
			var name = Form.GetModule();
			AssertEquals("Dummy", name.ID.Name);
		}

		public void TestForceClose()
		{
			using (var testForm = new ZForm(Dummy))
			{
				testForm.Show();
				Assert("Open", testForm.Visible);
				Dummy.HasChanges = true;
				testForm.ForceClose();
				Assert("Closed", !testForm.Visible);
			}
		}

		public void TestHandleDragOver()
		{
			using (var testForm = new ZForm())
			{
				var dragOverCalled = false;
				testForm.DragOver += delegate
				{ dragOverCalled = true; };
				testForm.HandleDragOver(new DragEventArgs(null, 0, 0, 0, DragDropEffects.None, DragDropEffects.None));
				Assert("Form's OnDragOver event should have been fired", dragOverCalled);
			}
		}

		public void TestHandleDragDrop()
		{
			using (var testForm = new ZForm())
			{
				var dragDropCalled = false;
				testForm.DragDrop += delegate
				{ dragDropCalled = true; };
				testForm.HandleDragDrop(new DragEventArgs(null, 0, 0, 0, DragDropEffects.None, DragDropEffects.None));
				Assert("Form's OnDragDrop event should have been fired", dragDropCalled);
			}
		}

		public void TestPasteData()
		{
			using (var testForm = new ZForm())
			{
				var dataObjectPastedCalled = false;
				testForm.DataObjectPasted += delegate
				{ dataObjectPastedCalled = true; };
				using (var data = ZDataObject.FromData("hello"))
				{
					ZFormPaster.PasteData(testForm, data);
					Assert("Form's OnPaste event should have been fired", dataObjectPastedCalled);
				}
			}
		}

		public void TestRememberPositionAndSize()
		{
			using (var testForm = new ZForm { Name = "TestForm1" })
			{
				testForm.ForceRememberPositionAndSize = true;
				testForm.Show();
				testForm.Width = 400;
				testForm.Close();
			}

			using (var testForm = new ZForm { Name = "TestForm1" })
			{
				Assert(EnterpriseFormLookStrategy.FormIsSaved(testForm));
				testForm.ForceRememberPositionAndSize = true;
				testForm.Show();
				AssertEquals(400, testForm.Width);
			}

			using (var testForm = new Testing.TestZForm { Name = "TestForm2" })
			{
				testForm.ForceRememberPositionAndSize = true;
				testForm.Show();
				testForm.Width = 400;
				testForm.Close();
			}

			using (var testForm = new Testing.TestZForm { Name = "TestForm2" })
			{
				Assert(!EnterpriseFormLookStrategy.FormIsSaved(testForm));
				testForm.ForceRememberPositionAndSize = true;
				testForm.Show();
				AssertNotEquals(400, testForm.Width);
			}
		}

		public void TestPositionAndSizeNotSavedAfterDatabaseUpgradedExceptionHasBeenThrown()
		{
			using (Db.Connection.SetDatabaseUpgradedExceptionHasBeenThrown_ForTest())
			{
				using (var testForm = new ZForm { Name = "TestForm1" })
				{
					testForm.ForceRememberPositionAndSize = true;
					testForm.Show();
					testForm.Width = 400;
					testForm.Close();
				}

				using (var testForm = new ZForm { Name = "TestForm1" })
				{
					Assert(!EnterpriseFormLookStrategy.FormIsSaved(testForm));
					testForm.ForceRememberPositionAndSize = true;
					testForm.Show();
					AssertNotEquals(400, testForm.Width);
				}
			}
		}

		#region Test Classes

		class TestZForm : ZForm
		{
			public TestZForm(IBusiness businessEntity)
				: base(businessEntity)
			{
			}

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			public void SetFormCaption(string value)
			{
				this.formCaption = value;
			}

			public override string FormCaption
			{
				get { return formCaption ?? base.FormCaption; }
			}
			string formCaption;
		}

		#endregion

		#region Implementation

		TestZForm Form
		{
			get { return form ?? (form = new TestZForm(Dummy)); }
		}
		TestZForm form;

		protected override void TearDown()
		{
			base.TearDown();
			UserIdleWorker.Flush();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion

		#region SetUp/TearDown

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}

		#endregion
	}
}
