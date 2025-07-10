using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TabPageNotificationsExposerTest : TestCaseWithFactory
	{
		public void TestExposeTabPageNotifications_ForNotBoundInnerTabControl()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			AssertEquals("No TabPage image initially", -1, Form.TabPage3.ImageIndex);
			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage3.ImageIndex);
			dependent.ZD1_Number = 0;
			AssertEquals("No TabPage image once the error is fixed", -1, Form.TabPage3.ImageIndex);
		}

		public void TestExposeTabPageNotifications()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			AssertEquals("No TabPage image initially", -1, Form.TabPage2.ImageIndex);
			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage2.ImageIndex);
			dependent.ZD1_Number = 0;
			AssertEquals("No TabPage image once the error is fixed", -1, Form.TabPage2.ImageIndex);
		}

		public void TestExposeTabPageNotifications_ForWrappedProperty()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;
			Form.InnerControl.innerTextBox.SetBindingMember("Dependents.ZD1_WrappedNumberProperty");

			AssertEquals("No TabPage image initially", -1, Form.TabPage3.ImageIndex);
			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage3.ImageIndex);
			dependent.ZD1_Number = 0;
			AssertEquals("No TabPage image once the error is fixed", -1, Form.TabPage3.ImageIndex);
		}

		public void TestExposeTabPageNotifications_OnIdle()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			AssertEquals("No TabPage image initially", -1, Form.TabPage2.ImageIndex);
			var initialUserIdleWorkerWorkItems = UserIdleWorker.QueuedWorkItemCount;
			TabPageNotificationsExposer.ExposeTabPageNotificationsOnIdle(Form, Dummy);
			AssertEquals("No error initially", -1, Form.TabPage2.ImageIndex);
			AssertNotEquals("Lots", initialUserIdleWorkerWorkItems, UserIdleWorker.QueuedWorkItemCount);
			UserIdleWorker.Flush();
			AssertEquals("Error once notifications are exposed and UserIdleWorker run", Icons.GetImageIndex(IconTypes.Error), Form.TabPage2.ImageIndex);
		}

		[ExpectNoExceptions]
		public void TestExposeTabPageNotifications_DisposeDuringBind()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			Form.TabPage3.Binding += delegate
			{ Form.TabPage3.Dispose(); };
			TabPageNotificationsExposer.ExposeTabPageNotificationsOnIdle(Form, Dummy);
			UserIdleWorker.Flush();

			Assert(Form.TabPage3.IsDisposed);
		}

		[ExpectNoExceptions]
		public void TestExposeTabPageNotifications_DisposeAfterBind()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			Form.Dispose();
			Form.txtNumber.ForceBindingIncludingParents();

			UserIdleWorker.Flush();

			Assert(Form.TabPage3.IsDisposed);
		}

		public void TestExposeTabPageNotifications_ForGrid()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Code = "error";

			AssertEquals("No TabPage image initially", -1, Form.TabPage2.ImageIndex);
			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage2.ImageIndex);
			dependent.ZD1_Code = "";
			AssertEquals("No TabPage image once the error is fixed", -1, Form.TabPage2.ImageIndex);
		}

		public void TestExposeTabPageNotifications_ForGridWithPropertyNotInColumns()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			AssertEquals("No TabPage image initially", -1, Form.TabPage2.ImageIndex);

			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("No TabPage image for no errors", -1, Form.TabPage2.ImageIndex);

			dependent.ZD1_NumberInfo.AdditionalValidation += () => dependent.ZD1_NumberInfo.AddError("error");
			dependent.Validation.ValidateAll();
			AssertEquals("No TabPage image until exposed", -1, Form.TabPage2.ImageIndex);

			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage2.ImageIndex);
		}

		public void TestExposeTabPageNotifications_ChildControlIsDynamic()
		{
			Form.Show();

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			CombineAssertions(() =>
			{
				AssertEquals("No TabPage image initially", -1, Form.TabPage4.ImageIndex);
				TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
				AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage4.ImageIndex);
			});
		}

		public void TestExposeTabPageNotifications_DynamicControl()
		{
			var dynamicCreationUserControl = new ZDynamicControlCreationUserControl();
			dynamicCreationUserControl.UserControlType = typeof(UserControlForTestTabPage);
			var count = 0;
			dynamicCreationUserControl.HostedControlCreated += (s, e) => { count++; };
			Form.Controls.Add(dynamicCreationUserControl);

			var dependent = Dummy.Dependents.AddNew();
			dependent.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;

			AssertNull("Initially", dynamicCreationUserControl.HostedControl);
			TabPageNotificationsExposer.ExposeTabPageNotifications(dynamicCreationUserControl, Dummy);
			AssertEquals("ForceCreateHostedControl is called", 1, count);
		}

		#region TestExposeTabPageNotificationsForEDocsPlubgin

		public void TestExposeTabPageNotifications_EDocsPlugIn()
		{
			using (var plugIn = new eDocsPlugIn(Form.BusinessEntity))
			{
				var eDocsTab = new EDocsZTabPagePlugIn(plugIn);
				Form.TabPage1.ParentTabControl.TabPages.Add(eDocsTab);

				Form.Show();

				((BusinessObject)Form.BusinessEntity).RunPreSaveValidation();

				Assert("PlugIn is not active initially", !eDocsTab.PlugIn.IsActive);
				AssertEquals("No TabPage image initially", -1, eDocsTab.ImageIndex);

				TabPageNotificationsExposer.ExposeTabPageNotifications(Form, new[] { "XX_Abc" }, false);

				Assert("PlugIn is not active yet", !eDocsTab.PlugIn.IsActive);
				AssertEquals("No TabPage image yet", -1, eDocsTab.ImageIndex);

				TabPageNotificationsExposer.ExposeTabPageNotifications(Form, new[] { "EQ_Abc" }, false);

				Assert("PlugIn is active now", eDocsTab.PlugIn.IsActive);
				// Next part is difficult to reproduce in test conditions with all validation and binding
				//AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), eDocsTab.ImageIndex);
			}
		}

		public void TestExposeTabPageNotifications_UseCellNotificationSuspender()
		{
			var dependent1 = Dummy.Dependents.AddNew();
			dependent1.ZD1_Code = "error";
			dependent1.ZD1_Number = DummyDependantBusinessObject.ZD1_Number_WhenInError;
			Form.Show();
			AssertEquals(0, form.Grid.UpdateGridNotificationTypeCountForTesting);

			AssertEquals("No TabPage image initially", -1, Form.TabPage2.ImageIndex);
			TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Dummy);
			AssertEquals("Error once notifications are exposed", Icons.GetImageIndex(IconTypes.Error), Form.TabPage2.ImageIndex);
			AssertEquals(1, Form.Grid.UpdateGridNotificationTypeCountForTesting);

			dependent1.ZD1_Code = "";
			dependent1.ZD1_Number = 2;
			AssertEquals("No TabPage image once the error is fixed", -1, Form.TabPage2.ImageIndex);
			AssertEquals("2 extra for each fields", 5, Form.Grid.UpdateGridNotificationTypeCountForTesting);
		}

		class EDocsZTabPagePlugIn : ZTabPagePlugIn
		{
			public EDocsZTabPagePlugIn(eDocsPlugIn plugIn) : base(plugIn) { }
		}

		class eDocsPlugIn : ZPlugIn
		{
			public eDocsPlugIn(IBusiness hostBusinessEntity) : base(hostBusinessEntity) { }

			public override string Name
			{
				get { return "eDocsPlugIn"; }
			}

			protected internal override ZBool HasUserControl
			{
				get { return true; }
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return null; }
			}

			protected internal override Control GetNewUserControl()
			{
				return new eDocsControl(HostBusinessEntity);
			}
		}

		class Dummy1 : DummyBusinessObject
		{
			public Dummy1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public ZString EQ_Abc { get; set; }

			public ZPropertyInfo EQ_AbcInfo
			{
				get { return GetZPropertyInfo(nameof(EQ_Abc)); }
			}
		}

		class eDocsControl : ZUserControl
		{
			public eDocsControl(IBusiness hostBusinessEntity)
			{
				HostBusinessEntity = (BusinessObject)hostBusinessEntity;

				var textBox = new ZTextBox();
				BindingSource.SetBindingMember(textBox, "EQ_Abc");
				Controls.Add(textBox);
			}

			readonly BusinessObject HostBusinessEntity;

			public override void SetDataBinding(object dataSource, string dataMember)
			{
				if (dataSource != null)
				{
					var dummy1 = HostBusinessEntity.Factory.New<Dummy1>();
					HostBusinessEntity.RegisterEditableChildObject(dummy1);
					base.SetDataBinding(dummy1, dataMember);
				}
				else
				{
					base.SetDataBinding(dataSource, dataMember);
				}
			}
		}

		#endregion

		#region Implementation

		FormWithTabPageNotifications Form
		{
			get
			{
				if (form == null)
				{
					form = new FormWithTabPageNotifications(Dummy);
				}
				return form;
			}
		}
		FormWithTabPageNotifications form;

		DummyWithDependentsBusinessObject Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithDependentsBusinessObject>();
				}
				return dummy;
			}
		}
		DummyWithDependentsBusinessObject dummy;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
