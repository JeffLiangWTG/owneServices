using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZAddressControlTest : ZControlBaseTestCase<ZAddressControl>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			using (ZAddressControl control = new ZAddressControl())
			{
				AssertNotNull(control);
			}
		}

		#endregion

		#region TestFetchHintsGenerated

		[ExpectNoExceptions]
		public void TestAddFetchHint_BusinessObjectRowDeleted()
		{
			using (var control = new ZAddressControl())
			{
				var bizO = DummyBusinessObject.New(Factory);
				control.BindToAddress = "Z0_PK";
				bizO.Delete();
				((IFetchHintGenerator)control).AddFetchHint(bizO, string.Empty);
			}
		}

		public void TestFetchHintsGenerated()
		{
			using (ZAddressControl control = new ZAddressControl())
			{
				DummyBusinessObject bizO = DummyBusinessObject.New(Factory);
				control.BindToAddress = "Z0_PK";
				AssertEquals(0, Factory.ActiveFetchHintsForTable(OrgAddressSchema.Constants.TableName));
				((IFetchHintGenerator)control).AddFetchHint(bizO, "");
				AssertEquals(1, Factory.ActiveFetchHintsForTable(OrgAddressSchema.Constants.TableName));
			}
		}

		#endregion

		#region TestReadOnlyWhenNoCurrent

		public void TestReadOnlyWhenNoCurrent()
		{
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AddressControl.Focus();

				AssertNotNull("ZAddressControl should be instantiated.", testForm.AddressControl);
				AssertEquals("ReadOnly value when No Current", true, testForm.AddressControl.ReadOnly);

				DummyWithAddy.Dummies.AddNew();
				AssertEquals("ReadOnly value when Current", false, testForm.AddressControl.ReadOnly);

				DummyWithAddy.Dummies.RemoveAll();
				AssertEquals("ReadOnly value when No Current", true, testForm.AddressControl.ReadOnly);
			}
		}

		#endregion

		#region TestSettingViaBusinessLayer

		public void TestSettingViaBusinessLayer()
		{
			DummyWithAddy.Dummies.AddNew();
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				DummyWithAddy.Dummies[0].Z0_Guid = new ZGuid("131A8DEE-603D-47FA-98E9-2E69BDBBCCF5");
				AssertEquals("Label Test", "MIDWAY METALS" + System.Environment.NewLine + "YATALA, QLD 4207", testForm.AddressControl.AddressTextBox.Text);
			}
		}

		#endregion

		#region TestSetDataBinding

		public void TestSetDataBinding()
		{
			DummyWithAddy.Dummies.AddNew();
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				testForm.AddressControl.SetDataBinding(null, "");
				testForm.AddressControl.SetDataBinding(DummyWithAddy, "Dummies.Z0_Guid");
				AssertEquals("Dummies.Z0_Guid", testForm.AddressControl.BindToAddress);
			}
		}

		#endregion

		#region TestShowAddressDropEdit

		public void TestShowAddressDropEdit()
		{
			DummyWithAddy.Dummies.AddNew();
			using (var testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				var addressControl = testForm.AddressControl;
				AssertEquals(true, addressControl.ShowAddressDropEdit);
				AssertEquals(true, addressControl.AddressDropEdit.Visible);
				AssertEquals(Math.Max(addressControl.OrganisationFindBox.Height, addressControl.AddressDropEdit.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(ZAddressControl.DropEditToAddressGap + ZAddressControl.AddressTextHeight), testForm.AddressControl.Height);
				AssertEquals(addressControl.OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ZAddressControl.FindBoxToDropEditGap) + addressControl.AddressDropEdit.Width + 2 * (addressControl.DropEditToStatusButtonGap + addressControl.AddressStatusButton.Width), testForm.AddressControl.Width);

				testForm.AddressControl.ShowAddressDropEdit = false;
				AssertEquals(false, testForm.AddressControl.ShowAddressDropEdit);
				AssertEquals(false, testForm.AddressControl.AddressDropEdit.Visible);
				AssertEquals(Math.Max(addressControl.OrganisationFindBox.Height, addressControl.AddressDropEdit.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(ZAddressControl.DropEditToAddressGap + ZAddressControl.AddressTextHeight), testForm.AddressControl.Height);
				AssertEquals(addressControl.OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ZAddressControl.FindBoxToDropEditGap) + addressControl.AddressDropEdit.Width + 2 * (addressControl.DropEditToStatusButtonGap + addressControl.AddressStatusButton.Width), testForm.AddressControl.Width);

				// Stacked controls
				testForm.AddressControl.StackControls = true;
				AssertEquals(false, testForm.AddressControl.ShowAddressDropEdit);
				AssertEquals(false, testForm.AddressControl.AddressDropEdit.Visible);
				AssertEquals(Math.Max(addressControl.OrganisationFindBox.Height, addressControl.AddressDropEdit.Height) + ControlDpiScalingHelper.ScaleToCurrentDpiY(ZAddressControl.DropEditToAddressGap + ZAddressControl.AddressTextHeight), testForm.AddressControl.Height);
				AssertEquals(addressControl.OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ZAddressControl.FindBoxToDropEditGap) + addressControl.AddressDropEdit.Width + 2 * (addressControl.DropEditToStatusButtonGap + addressControl.AddressStatusButton.Width), testForm.AddressControl.Width);

				testForm.AddressControl.ShowAddressDropEdit = true;
				AssertEquals(true, testForm.AddressControl.ShowAddressDropEdit);
				AssertEquals(true, testForm.AddressControl.AddressDropEdit.Visible);
				AssertEquals(addressControl.OrganisationFindBox.Height + addressControl.AddressDropEdit.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(ZAddressControl.FindBoxToDropEditGap + ZAddressControl.DropEditToAddressGap + ZAddressControl.AddressTextHeight), testForm.AddressControl.Height);
				AssertEquals(addressControl.OrganisationFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ZAddressControl.FindBoxToDropEditGap) + addressControl.AddressDropEdit.Width + 2 * (addressControl.DropEditToStatusButtonGap + addressControl.AddressStatusButton.Width), testForm.AddressControl.Width);
			}
		}

		#endregion

		#region TestChangingDataSource

		public void TestChangingDataSource()
		{
			DummyWithZAddress dummy1 = DummyWithAddy.Dummies.AddNew();
			DummyWithZAddress dummy2 = DummyWithAddy.Dummies.AddNew();

			dummy1.Z0_Guid = new ZGuid("131A8DEE-603D-47FA-98E9-2E69BDBBCCF5");
			dummy2.Z0_Guid = new ZGuid("B681B902-685A-4AFC-85E2-C312C0B29F4B");

			string dummy1Address = "MIDWAY METALS\r\nYATALA, QLD 4207";
			string dummy2Address = "DEMO ORGANISATION\r\n DEMOVILLE NSW 2000";

			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();

				AssertEquals("AddressTextBox.Text", dummy1Address, testForm.AddressControl.AddressTextBox.Text);
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", true, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", false, dummy2.Z0_Guid_ZAddress.IsOrgVisible);

				testForm.Grid.ListManager.Position = 1;
				AssertEquals("AddressTextBox.Text", dummy2Address, testForm.AddressControl.AddressTextBox.Text);
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", true, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", true, dummy2.Z0_Guid_ZAddress.IsOrgVisible);

				testForm.AddressControl.ShowOrganisation = false;
				testForm.AddressControl.SetDataBinding(null, "");
				testForm.AddressControl.SetDataBinding(DummyWithAddy, testForm.AddressControl.BindTo);

				AssertEquals("AddressTextBox.Text", dummy2Address, testForm.AddressControl.AddressTextBox.Text);
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", true, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", false, dummy2.Z0_Guid_ZAddress.IsOrgVisible);

				testForm.Grid.ListManager.Position = 0;
				AssertEquals("AddressTextBox.Text", dummy1Address, testForm.AddressControl.AddressTextBox.Text);
				AssertEquals("dummy1.Z0_Guid_ZAddress.IsOrgVisible", false, dummy1.Z0_Guid_ZAddress.IsOrgVisible);
				AssertEquals("dummy2.Z0_Guid_ZAddress.IsOrgVisible", false, dummy2.Z0_Guid_ZAddress.IsOrgVisible);
			}
		}

		#endregion

		#region TestValidation

		public void TestValidation()
		{
			DummyWithConditionalSetter dummy = Factory.New<DummyWithConditionalSetter>();
			DummyWithAddy.Dummies.Add(dummy);
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				ZAddressFindBox orgFindBox = (ZAddressFindBox)testForm.AddressControl.Controls.Find("OrganisationFindBox", true)[0];
				Control codeBox = orgFindBox.Controls.Find("CodeBox", true)[0];

				orgFindBox.Focus();
				codeBox.Text = "ZZZ";
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", ZGuid.Invalid, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertHasError(dummy.Z0_GuidInfo, "Enter a valid Organization.");

				orgFindBox.Focus();
				codeBox.Text = "";
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", ZGuid.Empty, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertNoErrors(dummy.Z0_GuidInfo);

				orgFindBox.Focus();
				BusinessObject org = (BusinessObject)Factory.LoadTop1<IOrgHeader>(new ZQuery());
				codeBox.Text = org[OrgHeaderSchema.Constants.OH_Code].ToString();
				testForm.Grid.Focus();
				AssertEquals("dummy.Z0_Guid_ZAddress.OrgPK", org.PK, dummy.Z0_Guid_ZAddress.OrgPK);
				TestCaseWithFactory.AssertNoErrors(dummy.Z0_GuidInfo);
			}
		}

		#endregion

		#region TestDataSourceType

		public void TestDataSourceType()
		{
			using (ZAddressControl control = new ZAddressControl())
			{
				AssertEquals(typeof(ZGuid), control.DataSourceType);
			}
		}

		#endregion

		#region TestShowOrganisationName

		public void TestShowOrganisationName()
		{
			DummyWithAddy.Dummies.AddNew();
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				AssertEquals(false, testForm.AddressControl.ShowOrganisationName);
				AssertEquals(false, testForm.AddressControl.OrganisationFindBox.ShowDescriptionBox);
				AssertEquals(29, testForm.AddressControl.AddressDropEdit.PreBoundMaxLength);

				testForm.AddressControl.ShowOrganisationName = true;
				AssertEquals(true, testForm.AddressControl.ShowOrganisationName);
				AssertEquals(true, testForm.AddressControl.OrganisationFindBox.ShowDescriptionBox);
				AssertEquals(18, testForm.AddressControl.AddressDropEdit.PreBoundMaxLength);

				testForm.AddressControl.StackControls = true;
				AssertEquals(true, testForm.AddressControl.ShowOrganisationName);
				AssertEquals(true, testForm.AddressControl.OrganisationFindBox.ShowDescriptionBox);
				AssertEquals(29, testForm.AddressControl.AddressDropEdit.PreBoundMaxLength);
			}
		}

		public void TestSetOrganisationBoxLength()
		{
			DummyWithAddy.Dummies.AddNew();
			using (TestZAddressControlForm testForm = new TestZAddressControlForm(DummyWithAddy))
			{
				testForm.Show();
				testForm.AddressControl.ShowOrganisationName = true;
				AssertEquals(0, testForm.AddressControl.OrganisationFindBox.PreBoundMaxLength);
				testForm.AddressControl.SetOrganisationFindBoxPreBoundMaxLength(4);
				AssertEquals(4, testForm.AddressControl.OrganisationFindBox.PreBoundMaxLength);
			}
		}

		#endregion

		#region DummyWithConditionalSetter

		class DummyWithConditionalSetter : DummyWithZAddress
		{
			public DummyWithConditionalSetter(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZGuid Z0_Guid
			{
				get { return base.Z0_Guid; }
				set
				{
					if (base.Z0_Guid != value)
					{
						base.Z0_Guid = value;
					}
				}
			}
		}

		#endregion

		#region TestZAddressControlForm

		internal class TestZAddressControlForm : ZForm
		{
			ZAddressControl addressControl;
			ZGrid grid;

			public TestZAddressControlForm(DummyWithZAddress businessEntity)
				: base(businessEntity)
			{
			}

			public ZAddressControl AddressControl
			{
				get { return addressControl; }
			}

			public ZGrid Grid
			{
				get { return grid; }
			}

			#region Windows Form Designer generated code

			protected sealed override void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				this.addressControl = new ZAddressControl();
				this.grid = new ZGrid();
				((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
				this.SuspendLayout();
				//
				// MainStatusBar
				//
				this.MainStatusBar.Location = new System.Drawing.Point(0, 91);
				this.MainStatusBar.Name = "MainStatusBar";
				this.MainStatusBar.Size = new System.Drawing.Size(420, 24);
				//
				// MessageStatusBarPanel
				//
				this.MessageStatusBarPanel.Width = 204;
				//
				// ErrorStatusBarPanel
				//
				this.ErrorStatusBarPanel.Width = 205;
				//
				// AddressControl
				//
				this.addressControl.BindToAddress = "Dummies.Z0_Guid";
				this.addressControl.BindToOrgList = "Dummies.Organisations";
				this.addressControl.Location = new System.Drawing.Point(20, 16);
				this.addressControl.Name = "AddressControl";
				this.addressControl.PopupCaption = "";
				this.addressControl.Size = new System.Drawing.Size(383, 61);
				this.addressControl.TabIndex = 2;
				//
				// Grid
				//
				this.grid.BindTo = "Dummies";
				zTextBoxColumnStyleInfo1.Caption = "Text";
				zTextBoxColumnStyleInfo1.ColumnName = "Z0_VarCharMax";
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.grid.Name = "grid";
				//
				// TestZAddressControlForm
				//

				this.ClientSize = new System.Drawing.Size(420, 115);
				this.Controls.Add(this.addressControl);
				this.Controls.Add(this.grid);
				this.Name = "TestZAddressControlForm";
				this.Text = "TestZAddressControlForm";
				this.Controls.SetChildIndex(this.MainStatusBar, 0);
				this.Controls.SetChildIndex(this.addressControl, 0);
				((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
				((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
				this.ResumeLayout(false);
			}

			#endregion
		}

		#endregion

		public void TestReadonlyWhenControlIsReadOnly()
		{
			var testHeader = Factory.New<IOrgHeader>();

			using (var form = new ZForm(testHeader))
			using (var testControl = new ZAddressControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var readOnlyToggleControl = testControl as IReadOnlyToggleControl;
				AssertNotNull("ZDocAddressControlTestClass should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, testControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, testControl.ReadOnly);
				AssertEquals("AddressStatusButton.ReadOnly", true, testControl.AddressStatusButton_Exposed.ReadOnly);
			}
		}

		#region Implementation

		public class ZAddressControlForTest : ZAddressControl
		{
			public ZButton AddressStatusButton_Exposed
			{
				get { return this.AddressStatusButton; }
			}
		}

		protected override string[] BindablePropertyNames
		{
			get { return new string[] { "ShowOrganisationForBinding" }; }
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "Text"; }
		}

		protected override bool UsesControlDataBindings
		{
			get { return false; }
		}

		DummyWithZAddress DummyWithAddy
		{
			get { return dummyWithAddy ?? (dummyWithAddy = Factory.New<DummyWithZAddress>()); }
		}
		DummyWithZAddress dummyWithAddy;

		#endregion
	}
}
