using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Testing
{
	public abstract class NotificationInGridTestCase : ResourceStringTestCase
	{
		protected abstract string GetColumnName();
		protected abstract ZGridColumnInfo GetInfo();

		public abstract void TestLocationAndSizeWithAndWithoutNotifications();

		protected virtual string ExpectedDataKey
		{
			get { return "DummyBizo|" + PropertyInfo.Name; }
		}

		protected ZPropertyInfo PropertyInfo
		{
			get { return child.ZPropertyInfoHash[GetColumnName()]; }
		}

		protected ZPropertyInfo OtherPropertyInfo
		{
			get { return child.ZPropertyInfoHash[DummyBusinessObject.Schema.Z0_VarCharMax]; }
		}

#if !WINZOR
		public void TestShowsBalloonWhenMouseOverNotification()
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			form.Show();
			UserIdleWorker.Flush();

			PropertyInfo.AddError("err");
			PropertyInfo.AddError("err2");
			PropertyInfo.AddMessageError("mess err");
			PropertyInfo.AddWarning("warn");
			PropertyInfo.AddWarning("warn2");
			UserIdleWorker.Flush();

			button.Focus();
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("No balloon should show", descriptor);

			var onIconPosition = ControlDpiScalingHelper.NewScaledPoint(38, 22);
			var offIconPosition = ControlDpiScalingHelper.NewScaledPoint(100, 100);

			form.Grid.MousePosition = onIconPosition;
			form.Grid.FireMouseMove();
			form.Grid.FireMouseHover();

			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Balloon showing", descriptor);
			AssertEquals("Control", form.Grid, descriptor.AnchorControl);
			AssertEquals("caption", descriptor.Caption);
			AssertEquals("fulldescription", descriptor.Description);
			AssertEquals("notifications", 2, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications", 2, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications", 1, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));
			AssertEquals("HideWhenMouseOverBalloon", true, descriptor.HideWhenMouseOverBalloon);

			form.Grid.MousePosition = offIconPosition;
			form.Grid.FireMouseMove();
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("No balloon should show", descriptor);
		}

		public void TestBalloonCaptionFromPropertyHumanReadableName()
		{
			form.Grid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			form.Show();
			form.Grid.SetColumnCaption(GetColumnName(), "Column Header");
			UserIdleWorker.Flush();

			PropertyInfo.AddError("err");
			UserIdleWorker.Flush();

			var onIconPosition = ControlDpiScalingHelper.NewScaledPoint(38, 22);
			form.Grid.MousePosition = onIconPosition;
			form.Grid.FireMouseMove();
			form.Grid.FireMouseHover();

			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Balloon showing", descriptor);
			AssertEquals("Control", form.Grid, descriptor.AnchorControl);
			AssertEquals("Column Header", descriptor.Caption);
			AssertEquals("fulldescription", descriptor.Description);
		}

		public void TestShowsBalloonWhenMouseOver_QuickViewCard()
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			form.Show();
			UserIdleWorker.Flush();

			child.QuickViewCard_Override = "Here's my calling card";

			button.Focus();
			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("No balloon should show", descriptor);

			var onIconPosition = ControlDpiScalingHelper.NewScaledPoint(38, 22);
			var offIconPosition = ControlDpiScalingHelper.NewScaledPoint(100, 100);

			form.Grid.MousePosition = onIconPosition;
			form.Grid.FireMouseMove();
			form.Grid.FireMouseHover();

			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Balloon showing", descriptor);
			AssertEquals("Control", form.Grid, descriptor.AnchorControl);
			AssertEquals(child.HumanReadableName, descriptor.Caption);
			AssertEquals("Here's my calling card", descriptor.Description);
			AssertEquals("HideWhenMouseOverBalloon", true, descriptor.HideWhenMouseOverBalloon);
		}
#endif

		public void TestShowsBalloonWhenEnterControlInTrainingMode()
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = true;
			form.Show();
			UserIdleWorker.Flush();

			PropertyInfo.AddError("err");
			PropertyInfo.AddMessageError("mess err");
			PropertyInfo.AddWarning("warn");
			PropertyInfo.AddWarning("warn2");

			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();

			var descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNotNull("Balloon showing", descriptor);

#if WINZOR
			var columnStyle = form.Grid.TableStyles[0].GridColumnStyles[form.Grid.CurrentCell.ColumnNumber] as ZTextBoxColumnStyle;
			AssertEquals("Control", columnStyle.EditControl, descriptor.AnchorControl);
#else
			AssertEquals("Control", form.Grid, descriptor.AnchorControl);
#endif
			AssertEquals("caption", descriptor.Caption);
			AssertEquals("fulldescription", descriptor.Description);
			AssertEquals("notifications", 1, GetEnumerableCount(descriptor.Notifications.GetErrors()));
			AssertEquals("notifications", 2, GetEnumerableCount(descriptor.Notifications.GetWarnings()));
			AssertEquals("notifications", 1, GetEnumerableCount(descriptor.Notifications.GetMessageErrors()));
			AssertEquals("HideWhenMouseOverBalloon", false, descriptor.HideWhenMouseOverBalloon);

			OtherPropertyInfo.AddWarning("one warning");
			form.Grid.CurrentCell = new DataGridCell(0, 1);
			UserIdleWorker.Flush();

			button.Focus();
			descriptor = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor;
			AssertNull("Balloon not showing", descriptor);
		}

		public void TestDoesNotShowBalloonWhenEnterControlInNonTrainingMode()
		{
			EnvProxy.Instance.Registry.TraningModeEnabled = false;
			form.Show();
			UserIdleWorker.Flush();
			PropertyInfo.AddError("An error has occurred");

			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();

			AssertNull("Balloon should not be showing", Balloon.Instance.BalloonWindowExposedForTesting.Descriptor);
		}

		public void TestUpdatesStatusBar()
		{
			form.Show();
			UserIdleWorker.Flush();
			button.Focus();

			AssertEquals("Status bar text", "", form.StatusBarTextForTesting);

			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "fulldescription", form.StatusBarTextForTesting);

			button.Focus();
			AssertEquals("Status bar text", "", form.StatusBarTextForTesting);
			PropertyInfo.AddWarning("warn");
			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "warn", form.StatusBarTextForTesting);

			button.Focus();
			PropertyInfo.AddWarning("warn");
			PropertyInfo.AddMessageError("mess err");
			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "mess err", form.StatusBarTextForTesting);

			button.Focus();
			PropertyInfo.AddWarning("warn");
			PropertyInfo.AddMessageError("mess err");
			PropertyInfo.AddError("err");
			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "err", form.StatusBarTextForTesting);

			button.Focus();
			PropertyInfo.ClearAllNotifications();
			form.Grid.Focus();
			form.Grid.CurrentCell = new DataGridCell(0, 0);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "fulldescription", form.StatusBarTextForTesting);

			button.Focus();
			form.Grid.Focus();
			ExpectedDescription = "otherdesc";
			form.Grid.CurrentCell = new DataGridCell(0, 1);
			UserIdleWorker.Flush();
			AssertEquals("Status bar text", "otherdesc", form.StatusBarTextForTesting);
		}

		protected void CheckSizeAndLocation(int columnWidth, Point expectedLocation, Size expectedSize)
		{
			using (var form = new TestForm(Dummy))
			{
				form.Grid.ColumnStyles.Clear();
				var info = GetInfo();
				info.ColumnName = GetColumnName();
				info.IsMandatory = true;
				form.Grid.ColumnStyles.Add(info);
				form.Grid.RefreshTableStyles();
				form.Grid.SetDataBinding(form.BusinessEntity, "");
				UserIdleWorker.Flush();

				var style = (ZTextBoxColumnStyle)form.Grid.Columns[0].ColumnStyle;
				style.Width = columnWidth;
				form.Show();
				form.Grid.Focus();
				form.Grid.CurrentCell = new DataGridCell(0, 0);
				UserIdleWorker.Flush();

				var control = style.EditControl;
				AssertSizeAndLocation(control, expectedLocation, expectedSize);
			}
		}

		protected virtual void AssertSizeAndLocation(Control control, Point expectedLocation, Size expectedSize)
		{
#if !WINZOR
			AssertEquals("LocationX", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedLocation.X), control.Location.X, control.Location.X * 0.1);
			AssertEquals("LocationY", ControlDpiScalingHelper.ScaleToCurrentDpiY(expectedLocation.Y), control.Location.Y, control.Location.Y * 0.1);
#endif
			AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(expectedSize.Width), control.Width, control.Width * 0.1);
			AssertEquals("Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(expectedSize.Height), control.Height, control.Height * 0.1);
		}

		protected TestForm form;
		Button button;
		protected DummyChildBusinessObject child;

		protected override void SetUp()
		{
			base.SetUp();
			child = Dummy.Collection.AddNew();
			validationCheckerSuspend = child.SuspendValidationTesting();

			form = new TestForm(Dummy);
			button = new Button();

			form.Controls.Add(button);

			form.Grid.ColumnStyles.Clear();
			var info = GetInfo();
			info.ColumnName = GetColumnName();
			info.IsMandatory = true;

			ZGridColumnInfo otherInfo = new ZTextBoxColumnStyleInfo();
			otherInfo.ColumnName = DummyBusinessObject.Schema.Z0_VarCharMax;
			otherInfo.IsMandatory = true;

			form.Grid.ColumnStyles.Add(info);
			form.Grid.ColumnStyles.Add(otherInfo);
			form.Grid.RefreshTableStyles();
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
			validationCheckerSuspend.Dispose();
			Balloon.Instance.Hide();
		}

		IDisposable validationCheckerSuspend;

		protected class TestForm : GUI.Testing.ZTestForm
		{
			public TestForm(DummyBusinessObject dummy)
				: base(dummy)
			{
			}

			internal new ZGridNotificationsTestCase.DummyZGrid Grid
			{
				get
				{
					return (ZGridNotificationsTestCase.DummyZGrid)base.Grid;
				}
			}

			protected override ZGrid GetNewGrid()
			{
				return new ZGridNotificationsTestCase.DummyZGrid();
			}
		}
	}

	public abstract class ResourceStringTestCase : TestCaseWithDummy
	{
		protected IMockResourceStringCache Cache;
		protected string ExpectedDescription;

		protected new DummyEnterpriseBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
		}
		DummyEnterpriseBusinessObject dummy;

		protected override void SetUp()
		{
			base.SetUp();
			Cache = Res.UseMockData();
			Cache.SetResourceGetter((s) => new ResourceStringData("key", "shortCaption", "mediumCaption", "caption", ExpectedDescription ?? "fulldescription"));
		}

		protected override void TearDown()
		{
			ExpectedDescription = null;
			Cache.Dispose();
			base.TearDown();
		}

		protected int GetEnumerableCount(IEnumerable enumerable)
		{
			var i = 0;
			foreach (var e in enumerable)
			{
				i++;
			}

			return i;
		}
	}

	class ZGridColumnNotificationProviderTest : TestCaseWithFactory
	{
		public void TestOnEnterEditControl_WhenNoZPropertyInfo_ShouldDisplayResourceStringFullDescription()
		{
			var bizo = Factory.New<DummyParentBusinessObject>();
			var child = bizo.DisCollection.AddNew();

			using (var form = new ZForm(bizo))
			using (var grid = new ZGrid())
			{
				grid.Columns.AddTextColumn("SomePropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("AnotherPropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("YetAnotherPropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("PropertyWithZPropertyInfo", 80);

				form.Controls.Add(grid);
				grid.SetDataBinding(bizo, "DisCollection");
				form.Show();

				form.ValidateAll(ValidationType.Full);

				grid.CurrentCell = new DataGridCell(0, 0);
				AssertEquals("CationBot was here.", form.StatusBarTextForTesting);

				grid.CurrentCell = new DataGridCell(0, 1);
				AssertEquals("CaptionBot was also here.", form.StatusBarTextForTesting);

				grid.CurrentCell = new DataGridCell(0, 2);
				AssertEquals(string.Empty, form.StatusBarTextForTesting);

				grid.CurrentCell = new DataGridCell(0, 3);
				AssertEquals("Dat error.", form.StatusBarTextForTesting);
			}
		}

		public void TestOnHoverWhenRowNumIsGreaterThanListCount()
		{
			var bizo = Factory.New<DummyParentBusinessObject>();
			var child = bizo.DisCollection.AddNew();

			using (var form = new ZForm(bizo))
			using (var grid = new ZGridNotificationsTestCase.DummyZGrid() { Width = 300, Height = 350 })
			{
				grid.Columns.AddTextColumn("SomePropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("AnotherPropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("YetAnotherPropertyWithoutZPropertyInfo", 80);
				grid.Columns.AddTextColumn("PropertyWithZPropertyInfo", 80);

				form.Controls.Add(grid);
				grid.SetDataBinding(bizo, "DisCollection");
				form.Show();
				try
				{
					var position = grid.GetCellBounds(1, 1).Location;
					AssertEquals(DataGrid.HitTestType.Cell, grid.HitTest(position).Type);

					grid.CurrentCell = new DataGridCell(1, 1);
					grid.Focus();
					grid.MousePosition = position;

					grid.FireMouseMove();
					grid.FireMouseHover();

					AssertNull(string.Empty, Balloon.Instance.BalloonWindowExposedForTesting.Descriptor);
				}
				finally
				{
					Balloon.Instance.Hide();
				}
			}
		}

		#region Dummy Bizos

		class DummyParentBusinessObject : DummyBusinessObject
		{
			public DummyParentBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ChildEditable]
			public ActiveBusinessObjectCollection<DummyBizoWithPropertyWithoutZPropertyInfo> DisCollection
			{
				get
				{
					if (collection == null)
					{
						collection = new ActiveBusinessObjectCollection<DummyBizoWithPropertyWithoutZPropertyInfo>(Factory);
						RegisterEditableChildObject(collection);
					}

					return collection;
				}
			}

			ActiveBusinessObjectCollection<DummyBizoWithPropertyWithoutZPropertyInfo> collection;
		}

		class DummyBizoWithPropertyWithoutZPropertyInfo : DummyBusinessObject, IObsoleteValidation
		{
			public DummyBizoWithPropertyWithoutZPropertyInfo(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[ResourceStringData("DummyBizoWithPropertyWithoutZPropertyInfo.SomePropertyWithoutZPropertyInfo", Caption = "A really interesting cation", FullDescription = "CationBot was here.")]
			public ZString SomePropertyWithoutZPropertyInfo
			{
				get { return "Booooo"; }
			}

			[ResourceStringData("DummyBizoWithPropertyWithoutZPropertyInfo.AnotherPropertyWithoutZPropertyInfo", Caption = "Another really interesting cation", FullDescription = "CaptionBot was also here.")]
			public ZString AnotherPropertyWithoutZPropertyInfo
			{
				get { return "Booooo"; }
			}

			[ResourceStringData("DummyBizoWithPropertyWithoutZPropertyInfo.YetAnotherPropertyWithoutZPropertyInfo", Caption = "Yet another really interesting cation")]
			public ZString YetAnotherPropertyWithoutZPropertyInfo
			{
				get { return "Booooo"; }
			}

			[ResourceStringData("DummyBizoWithPropertyWithoutZPropertyInfo.YetAnotherPropertyWithoutZPropertyInfo", Caption = "Yet another really interesting cation", FullDescription = "I have an error so should not see me")]
			public ZString PropertyWithZPropertyInfo
			{
				get { return "Booooo"; }
			}

			public ZPropertyInfo PropertyWithZPropertyInfoInfo
			{
				get { return GetZPropertyInfo(nameof(PropertyWithZPropertyInfo)); }
			}

			protected override DummyBizoValidation GetNewValidation()
			{
				return new OverriddenValidation(this);
			}

			class OverriddenValidation : DummyBizoValidation
			{
				internal OverriddenValidation(DummyBizoWithPropertyWithoutZPropertyInfo parent)
					: base(parent)
				{
					this.parent = parent;
				}

				readonly DummyBizoWithPropertyWithoutZPropertyInfo parent;

				public override void ValidateAll()
				{
					base.ValidateAll();
					ValidatePropertyWithZPropertyInfo();
				}

				public void ValidatePropertyWithZPropertyInfo()
				{
					ValidateCalculatedProperty(parent.PropertyWithZPropertyInfoInfo);
				}

				protected void CheckPropertyWithZPropertyInfo()
				{
					parent.PropertyWithZPropertyInfoInfo.AddError("Dat error.");
				}
			}
		}

		#endregion
	}
}
