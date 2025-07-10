using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class CustomPropertiesControlTest : TestCaseWithFactory
	{
		public void TestRebindWhenMetaDataChanges()
		{
			using (ZForm form = new ZChildForm(GetCustomBusinessObject()))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var customPropertyCollection1 = GetCustomPropertyCollection();
				customPropertyCollection1.Add(typeof(ZString), "ZZZ_String");
				var cusObj1 = new CustomBusinessObject(null, customPropertyCollection1);
				ctrl.SetDataBinding(cusObj1, "");
				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZTextBox), "ZZZ_String");

				var list = new CodeDescriptionPairList();
				list.AddPair("A1", "AAA AAA");
				list.AddPair("B1", "AAA BBB");
				var customPropertyCollection2 = GetCustomPropertyCollection();
				customPropertyCollection2.Add(typeof(ZString), "ZZZ_String", ValidateList, DynamicMetaData.MaxLength(2), DynamicMetaData.ListDataSource(list));
				var cusObj2 = new CustomBusinessObject(null, customPropertyCollection2);
				ctrl.SetDataBinding(cusObj2, "");
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZDropEdit), "ZZZ_String");
			}
		}

		public void TestCustomControlsShouldBeDisposedWhenFormIsClosing()
		{
			var form = new ZChildForm(GetCustomBusinessObject());
			var ctrl = new CustomPropertiesControl();
			form.Controls.Add(ctrl);
			ctrl.Dock = DockStyle.Fill;
			form.Show();

			var customPropertyCollection1 = GetCustomPropertyCollection();
			customPropertyCollection1.Add(typeof(ZString), "ZZZ_String");
			var cusCtl = new CustomBusinessObject(null, customPropertyCollection1);
			ctrl.SetDataBinding(cusCtl, "");
			var customControl = ctrl.Controls["rowLayoutPanel"].Controls[0];
			Assert(!customControl.IsDisposed);

			form.Close();
			Assert(customControl.IsDisposed);
		}

		public void TestCustomControlsShouldBeDisposedFromTheBottomOfTheGrid()
		{
			using (var form = new ZChildForm(GetCustomBusinessObject()))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var customPropertyCollection1 = GetCustomPropertyCollection();
				customPropertyCollection1.Add(typeof(ZString), "ZZZ_String");
				customPropertyCollection1.Add(typeof(ZString), "ZZZ_String2");
				var cusCtl = new CustomBusinessObject(null, customPropertyCollection1);
				ctrl.SetDataBinding(cusCtl, "");
				var panel = ctrl.Controls["rowLayoutPanel"];
				var customControl1 = panel.Controls[0];
				var customControl2 = panel.Controls[1];

				panel.ControlRemoved += (sender, args) =>
				{
					if (panel.Controls.Count == 1)
					{
						AssertEquals("The top control should be removed last", customControl1, panel.Controls[0]);
						AssertEquals("The bottom control should be removed during its disposal", true, customControl2.Disposing);
						AssertEquals(true, panel.IsLayoutSuspended());
					}
				};

				ctrl.SetDataBinding("", "");//This causes the custom controls to be removed
				form.Close();
			}
		}

		public void TestOnLayoutResizesTextboxes()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String1", DynamicMetaData.MaxLength(3));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String2", DynamicMetaData.MaxLength(60));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String3", DynamicMetaData.MaxLength(30000));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String4");
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Width = 300;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				var ctrl1 = (ZTextBox)rowLayoutPanel.Controls[0];
				var ctrl2 = (ZTextBox)rowLayoutPanel.Controls[1];
				var ctrl3 = (ZTextBox)rowLayoutPanel.Controls[2];
				var ctrl4 = (ZTextBox)rowLayoutPanel.Controls[3];
				var width0 = rowLayoutPanel.Controls[0].Width;
				var width1 = rowLayoutPanel.Controls[1].Width;
				var width2 = rowLayoutPanel.Controls[2].Width;
				var width3 = rowLayoutPanel.Controls[3].Width;

				AssertEquals(TextRenderer.MeasureText(new String('Q', 3), ctrl1.Font).Width, ctrl1.MaximumSize.Width);
				AssertEquals(TextRenderer.MeasureText(new String('Q', 60), ctrl2.Font).Width, ctrl2.MaximumSize.Width);
				AssertEquals(0, ctrl3.MaximumSize.Width);
				AssertEquals(0, ctrl4.MaximumSize.Width);

				form.Width = 600;
				form.PerformLayout();
				Application.DoEvents();

				Assert(rowLayoutPanel.Controls[0].Width == width0);
				Assert(rowLayoutPanel.Controls[1].Width > width1);
				Assert(rowLayoutPanel.Controls[2].Width > width2);
				Assert(rowLayoutPanel.Controls[3].Width > width3);
			}
		}

		public void TestTextBoxCharacterCasingIsNormal()
		{
			var cusObj = GetCustomBusinessObject();

			using (ZForm form = new ZForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();

				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];

				int left;
				using (var gr = Graphics.FromHwnd(form.Handle))
				{
					left =
						(int)Math.Ceiling(
								gr.MeasureString("ZZZ_ComboBoxWithoutList", rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().Font)
									.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(CustomPropertiesControl.ControlClearanceOnLeft);
				}

				AssertEquals(7, rowLayoutPanel.Controls.Count);
				AssertControl(rowLayoutPanel.Controls[3], typeof(ZTextBox), "ZZZ_String", left);
				var textBox = (ZTextBox)rowLayoutPanel.Controls[3];
				AssertEquals(textBox.CharacterCasing, CharacterCasing.Normal);
			}
		}

		public void TestRegisterControlToBeBoundOnPreSaveValidation()
		{
			var dynamicObj = GetCustomBusinessObject();
			using (var form = new ZForm(dynamicObj))
			{
				bool controlBindingWasHit = false;

				var control = new CustomPropertiesControl();
				control.AfterFirstBinding += (sender, e) => controlBindingWasHit = true;
				var tabControl = new ZTabControl();
				tabControl.TabPages.Add(new ZTabPage { Name = "Test1" });
				tabControl.TabPages.Add(new ZTabPage { Name = "Test2" });
				var tabPage = tabControl.GetTabPage("Test2");
				tabPage.Controls.Add(control);
				form.Controls.Add(tabControl);

				form.Show();
				AssertEquals("Precondition:", false, controlBindingWasHit);

				form.FireSaveButton();
				AssertEquals(true, controlBindingWasHit);
			}
		}

		public void TestAddControls()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			using (ZForm form = new ZForm(cusObj))
			{
				CustomPropertiesControl ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];

				int left;
				using (var gr = Graphics.FromHwnd(form.Handle))
				{
					left =
						(int)Math.Ceiling(
								gr.MeasureString("ZZZ_ComboBoxWithoutList", rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().Font)
									.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(CustomPropertiesControl.ControlClearanceOnLeft);
				}

				CombineAssertions(() =>
				{
					AssertControl(rowLayoutPanel.Controls[0], typeof(ZDateEdit), "Start Date", left);
					AssertControl(rowLayoutPanel.Controls[1], typeof(ZCheckBox), "ZZZ_Bool", left);
					AssertControl(rowLayoutPanel.Controls[2], typeof(ZCalcEdit), "ZZZ_Decimal", left);
					AssertControl(rowLayoutPanel.Controls[3], typeof(ZTextBox), "ZZZ_String", left);
					AssertControl(rowLayoutPanel.Controls[4], typeof(ZDropEdit), "ZZZ_StringFromDropDown", left);
					AssertComboBoxControl(rowLayoutPanel.Controls[5], "ZZZ_ComboBoxWithList", left, true);
					AssertComboBoxControl(rowLayoutPanel.Controls[6], "ZZZ_ComboBoxWithoutList", left, false);
				});
			}
		}

		public void TestAddControls_InCorrectOrder()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String", ValidateText, DynamicMetaData.MaxLength(3), DynamicMetaData.Position(1));
			customPropertyCollection.Add(typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.DecimalPlaces(3), DynamicMetaData.Position(2));
			customPropertyCollection.Add(typeof(ZDateTime), "ZZZ_DateTime", DynamicMetaData.Description(new SimpleDescription("Start Date")));
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_Bool");
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];

				int left;
				using (var gr = Graphics.FromHwnd(form.Handle))
				{
					left = (int)Math.Ceiling(gr.MeasureString("ZZZ_Decimal", rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().Font).Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(CustomPropertiesControl.ControlClearanceOnLeft);
				}

				AssertControl(rowLayoutPanel.Controls[0], typeof(ZCheckBox), "ZZZ_Bool", left);
				AssertControl(rowLayoutPanel.Controls[1], typeof(ZDateEdit), "Start Date", left);
				AssertControl(rowLayoutPanel.Controls[2], typeof(ZTextBox), "ZZZ_String", left);
				AssertControl(rowLayoutPanel.Controls[3], typeof(ZCalcEdit), "ZZZ_Decimal", left);
			}
		}

		public void TestCheckBoxWidthIsCorrect()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			var captions = new string[] { "boo", "A very long string!", "WWWWWWWWWWWWWWWWWWWWWWWWWWWWW" };
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_Bool", DynamicMetaData.Description(new SimpleDescription(captions[0])));
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_Bool2", DynamicMetaData.Description(new SimpleDescription(captions[1])));
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_Bool3", DynamicMetaData.Description(new SimpleDescription(captions[2])));
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];

				using (Graphics gr = Graphics.FromHwnd(form.Handle))
				{
					for (int i = 0; i < 3; ++i)
					{
						var control = rowLayoutPanel.Controls[i];
						var width = control.Width;
						var render = control.GetExtension<LabelCaptionRenderer>();
						var caption = control.Text;
						var captionLength = (int)Math.Ceiling(gr.MeasureString(caption + render.LabelSeparator, render.Font).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(32));
						Assert(string.Format("checkBox width is wide enough to hold caption, but not arbitrarily large: {0}, {1}", width, captionLength), Math.Abs(width - captionLength) < ControlDpiScalingHelper.ScaleToCurrentDpiX(10));
					}
				}
			}
		}

		// Test when data binding a record of a grid collection
		[ExpectNoExceptions]
		public void TestAddControls_CorrectlyFromDifferentObjsInConsecutiveOrder()
		{
			var cusObj = GetCustomBusinessObject();
			var cusObj2 = GetCustomBusinessObject2();

			using (ZForm form = new ZForm(cusObj))
			{
				CustomPropertiesControl ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];

				int left;
				using (var gr = Graphics.FromHwnd(form.Handle))
				{
					left =
						(int)Math.Ceiling(
								gr.MeasureString("ZZZ_ComboBoxWithoutList", rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().Font)
									.Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(CustomPropertiesControl.ControlClearanceOnLeft);
				}
				AssertEquals(7, rowLayoutPanel.Controls.Count);
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZDateEdit), "Start Date", left);
				AssertControl(rowLayoutPanel.Controls[1], typeof(ZCheckBox), "ZZZ_Bool", left);
				AssertControl(rowLayoutPanel.Controls[2], typeof(ZCalcEdit), "ZZZ_Decimal", left);
				AssertControl(rowLayoutPanel.Controls[3], typeof(ZTextBox), "ZZZ_String", left);
				AssertControl(rowLayoutPanel.Controls[4], typeof(ZDropEdit), "ZZZ_StringFromDropDown", left);
				AssertComboBoxControl(rowLayoutPanel.Controls[5], "ZZZ_ComboBoxWithList", left, true);
				AssertComboBoxControl(rowLayoutPanel.Controls[6], "ZZZ_ComboBoxWithoutList", left, false);

				form.SetDataBinding(cusObj2, "");

				using (Graphics gr = Graphics.FromHwnd(form.Handle))
				{
					left = (int)Math.Ceiling(gr.MeasureString("ZZZ_Decimal", rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().Font).Width) + ControlDpiScalingHelper.ScaleToCurrentDpiX(CustomPropertiesControl.ControlClearanceOnLeft);
				}
				AssertEquals(3, rowLayoutPanel.Controls.Count);
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZCalcEdit), "ZZZ_Decimal", left);
				AssertControl(rowLayoutPanel.Controls[1], typeof(ZTextBox), "ZZZ_String", left);
				AssertControl(rowLayoutPanel.Controls[2], typeof(ZTextBox), "ZZZ_String2", left);
			}
		}

		public void TestAddControls_SpecialCase()
		{
			// In Customer Service Incident CS00408187)
			// A custom string field with name "OE62.Overseas Devan CFS" will be truncated unexpectedly under screen resolution 1920x1080 (No problem in higher resolution )
			// The problem could be reproduced with string ""OE62_Overseas Devan CFS" as i cant use property with .(dot) in unit test
			string problemCap = "OE62_Overseas Devan CFS";

			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), problemCap, ValidateText, DynamicMetaData.MaxLength(3));
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				AssertEquals("Caption is truncated", problemCap, rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().captionMeasurement.Caption);
				Assert("Caption: " + problemCap + " should not be truncated", !rowLayoutPanel.Controls[0].GetExtension<LabelCaptionRenderer>().IsCaptionTruncated);
			}
		}

		[ExpectNoExceptions]
		public void TestSetDataBinding()
		{
			using (CustomPropertiesControl ctrl = new CustomPropertiesControl())
			{
				AssertNoExceptionThrown("DataSource not using the IDynamicBusinessObject interface", () => ctrl.SetDataBinding(Factory.New<DummyBusinessObject>(), ""));
				AssertNoExceptionThrown("DataSource using the IDynamicBusinessObject interface", () => ctrl.SetDataBinding(GetCustomBusinessObject(), ""));
			}
		}

		public void TestBinding()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			cusObj["ZZZ_DateTime"] = new DateTime(2000, 11, 2);
			cusObj["ZZZ_Bool"] = true;
			cusObj["ZZZ_Decimal"] = 3.9;
			cusObj["ZZZ_String"] = "Tst";
			cusObj["ZZZ_StringFromDropDown"] = "B1";

			using (ZForm form = new ZForm(cusObj))
			{
				CustomPropertiesControl ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				ZDateEdit ctrl1 = (ZDateEdit)rowLayoutPanel.Controls[0];
				ZCheckBox ctrl2 = (ZCheckBox)rowLayoutPanel.Controls[1];
				ZCalcEdit ctrl3 = (ZCalcEdit)rowLayoutPanel.Controls[2];
				ZTextBox ctrl4 = (ZTextBox)rowLayoutPanel.Controls[3];
				ZDropEdit ctrl5 = (ZDropEdit)rowLayoutPanel.Controls[4];

				AssertEquals("Tst", ctrl4.Text);
				AssertEquals("3.90", ctrl3.Text);
				AssertEquals("02-NOV-00", ctrl1.Text);
				AssertEquals(true, ctrl2.Checked);
				AssertEquals("B1", ctrl5.Text);
				AssertEquals(3, ctrl5.List.Count);

				ctrl4.Text = "@T@";
				GetBinding(ctrl, cusObj, ctrl4, "Text").WriteValue();
				ctrl3.Text = "2.4";
				GetBinding(ctrl, cusObj, ctrl3, "Text").WriteValue();
				ctrl1.Text = "12-AUG-09";
				GetBinding(ctrl, cusObj, ctrl1, "DateTimeValue").WriteValue();
				ctrl2.Checked = false;
				GetBinding(ctrl, cusObj, ctrl2, "Checked").WriteValue();
				ctrl5.Text = "C1";
				GetBinding(ctrl, cusObj, ctrl5, "Text").WriteValue();
			}

			AssertEquals("@T@", cusObj["ZZZ_String"]);
			AssertEquals(2.4m, cusObj["ZZZ_Decimal"]);
			AssertEquals(new ZDateTime(2009, 8, 12), cusObj["ZZZ_DateTime"]);
			AssertEquals(false, cusObj["ZZZ_Bool"]);
			AssertEquals("C1", cusObj["ZZZ_StringFromDropDown"]);
		}

		public void TestNotifications()
		{
			CustomBusinessObject cusObj = GetCustomBusinessObject();
			cusObj["ZZZ_DateTime"] = new DateTime(2000, 11, 2);
			cusObj["ZZZ_Bool"] = true;
			cusObj["ZZZ_Decimal"] = 3.9m;
			cusObj["ZZZ_String"] = "Tst";
			cusObj["ZZZ_StringFromDropDown"] = "AA";
			cusObj.Validation.ValidateAll();

			using (ZForm form = new ZChildForm(cusObj))
			{
				CustomPropertiesControl ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				ZTextBox ctrl4 = (ZTextBox)rowLayoutPanel.Controls[3];
				ZDropEdit ctrl5 = (ZDropEdit)rowLayoutPanel.Controls[4];

				Assert(!ctrl4.GetExtension<NotificationExtension>().Notifications.HasErrors());
				Assert(ctrl5.GetExtension<NotificationExtension>().Notifications.HasErrors());

				ctrl4.Text = "";
				GetBinding(ctrl, cusObj, ctrl4, "Text").WriteValue();
				ctrl5.Text = "B1";
				GetBinding(ctrl, cusObj, ctrl5, "Text").WriteValue();

				Assert(ctrl4.GetExtension<NotificationExtension>().Notifications.HasErrors());
				Assert(!ctrl5.GetExtension<NotificationExtension>().Notifications.HasErrors());
			}
		}

		public void TestNothingSetupMessageLabel()
		{
			using (ZForm form = new ZChildForm(GetCustomBusinessObject()))
			{
				CustomPropertiesControl ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				ZLabel nothingSetupMessageLabel = (ZLabel)ctrl.Controls["nothingSetupMessageLabel"];

				Assert(rowLayoutPanel.Visible);
				Assert(!nothingSetupMessageLabel.Visible);

				ctrl.NothingSetupMessageLabelText = "NothingSetup Test";
				ctrl.SetDataBinding(null, "");
				Assert(!rowLayoutPanel.Visible);
				Assert(nothingSetupMessageLabel.Visible);
				AssertEquals("NothingSetup Test", nothingSetupMessageLabel.Text);
			}
		}

		public void TestMaxLength()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String1", DynamicMetaData.MaxLength(3));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String2", DynamicMetaData.MaxLength(60));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String3", DynamicMetaData.MaxLength(30000));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String4");
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				var ctrl1 = (ZTextBox)rowLayoutPanel.Controls[0];
				var ctrl2 = (ZTextBox)rowLayoutPanel.Controls[1];
				var ctrl3 = (ZTextBox)rowLayoutPanel.Controls[2];
				var ctrl4 = (ZTextBox)rowLayoutPanel.Controls[3];

				AssertEquals(TextRenderer.MeasureText(new String('Q', 3), ctrl1.Font).Width, ctrl1.MaximumSize.Width);
				AssertEquals(TextRenderer.MeasureText(new String('Q', 60), ctrl2.Font).Width, ctrl2.MaximumSize.Width);
				AssertEquals(0, ctrl3.MaximumSize.Width);
				AssertEquals(0, ctrl4.MaximumSize.Width);
			}
		}

		[ExpectNoExceptions]
		public void TestWithCheckBoxOnly()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_Bool");
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();
			}
		}

		public void TestFixedRows()
		{
			using (CustomPropertiesControl ctrl = new CustomPropertiesControl())
			{
				RowLayoutPanel rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				AssertEquals("FixedRows", true, rowLayoutPanel.FixedRows);
			}
		}

		public void TestCustomPropertyOrder_PositionMetaData()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String1", DynamicMetaData.MaxLength(3), DynamicMetaData.Position(3));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String2", DynamicMetaData.MaxLength(3), DynamicMetaData.Position(3));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String3", DynamicMetaData.MaxLength(3), DynamicMetaData.Position(2));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String4", DynamicMetaData.MaxLength(3), DynamicMetaData.Position(2));
			var cusObj = new CustomBusinessObject(null, customPropertyCollection)
			{
				["ZZZ_String1"] = "1",
				["ZZZ_String2"] = "2",
				["ZZZ_String3"] = "3",
				["ZZZ_String4"] = "4"
			};

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				AssertEquals("3", ((ZTextBox)rowLayoutPanel.Controls[0]).Text);
				AssertEquals("4", ((ZTextBox)rowLayoutPanel.Controls[1]).Text);
				AssertEquals("1", ((ZTextBox)rowLayoutPanel.Controls[2]).Text);
				AssertEquals("2", ((ZTextBox)rowLayoutPanel.Controls[3]).Text);
			}
		}

		public void TestMaximumSizeHeightTextBoxControl()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String1", DynamicMetaData.MaxLength(3));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String2", DynamicMetaData.MaxLength(60));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String3", DynamicMetaData.MaxLength(30000));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_String4");
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Width = 300;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				var ctrl1 = (ZTextBox)rowLayoutPanel.Controls[0];
				var ctrl2 = (ZTextBox)rowLayoutPanel.Controls[1];
				var ctrl3 = (ZTextBox)rowLayoutPanel.Controls[2];
				var ctrl4 = (ZTextBox)rowLayoutPanel.Controls[3];

				form.PerformLayout();
				Application.DoEvents();

				AssertEquals(ctrl1.MaximumSize.Height, 0);
				AssertEquals(ctrl2.MaximumSize.Height, 0);
				AssertEquals(ctrl3.MaximumSize.Height, 0);
				AssertEquals(ctrl4.MaximumSize.Height, 0);
			}
		}

		public void TestComboBoxHeightScales()
		{
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithoutList" + AddOnColumnDataType.PartIdentifier + "1", ValidateList,
				DynamicMetaData.MaxLength(2), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));
			customPropertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithoutList" + AddOnColumnDataType.PartIdentifier + "2", ValidateText,
				DynamicMetaData.MaxLength(3), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));
			var cusObj = new CustomBusinessObject(null, customPropertyCollection);

			using (var form = new ZChildForm(cusObj))
			{
				var ctrl = new CustomPropertiesControl();
				form.Controls.Add(ctrl);
				ctrl.Dock = DockStyle.Fill;
				form.Width = 300;
				form.Show();

				var rowLayoutPanel = (RowLayoutPanel)ctrl.Controls["rowLayoutPanel"];
				var ctrl1 = rowLayoutPanel.Controls[0];
				Assert(ctrl1.Height >= ctrl1.Controls[0].Height);

				ctrl1.Height = ctrl1.Height / 2;

				form.PerformLayout();
				Application.DoEvents();

				Assert(ctrl1.Height >= ctrl1.Controls[0].Height);
			}
		}

		#region Implementation

		Binding GetBinding(CustomPropertiesControl cusCtrl, CustomBusinessObject cusObj, Control ctrl, string propertyName)
		{
			foreach (Binding binding in ctrl.BindingContext[cusObj].Bindings)
			{
				if ((binding.Control == ctrl || (binding.Control != null && binding.Control.Parent == ctrl)) && binding.PropertyName == propertyName)
				{
					return binding;
				}
			}

			return null;
		}

		void AssertControl(Control ctrl, Type type, string caption, int left)
		{
			AssertControl(ctrl, type, caption);
			AssertEquals(left, ctrl.Left);
		}

		void AssertComboBoxControl(Control ctrl, string caption, int left, bool hasList)
		{
			var panel = ctrl as ZUserControl;
			AssertNotNull(panel);
			AssertEquals(caption, panel.GetExtension<LabelCaptionRenderer>().Caption);
			AssertControl(panel.Controls[0], hasList ? typeof(ZDropEdit) : typeof(ZTextBox), null);
			AssertControl(panel.Controls[1], typeof(ZTextBox), null);
			AssertEquals(left, ctrl.Left);
			AssertEquals(0, panel.Controls[0].Left);
			Assert(string.Format("{0}, {1}", panel.Controls[1].Left, panel.Controls[0].Left + panel.Controls[0].Width),
				panel.Controls[1].Left >= panel.Controls[0].Left + panel.Controls[0].Width);
		}

		void AssertControl(Control ctrl, Type type, string caption)
		{
			AssertEquals(type, ctrl.GetType());
			AssertEquals(caption, ctrl.GetExtension<LabelCaptionRenderer>().Caption);
		}

		CustomBusinessObject GetCustomBusinessObject()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA AAA");
			list.AddPair("B1", "AAA BBB");
			list.AddPair("C1", "AAA CCC");

			var propertyCollection = GetCustomPropertyCollection();
			propertyCollection.Add(typeof(ZString), "ZZZ_String", ValidateText, DynamicMetaData.MaxLength(3), DynamicMetaData.Position(3));
			propertyCollection.Add(typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.DecimalPlaces(3), DynamicMetaData.Position(2));
			propertyCollection.Add(typeof(ZDateTime), "ZZZ_DateTime", DynamicMetaData.Description(new SimpleDescription("Start Date")), DynamicMetaData.Position(0));
			propertyCollection.Add(typeof(ZBool), "ZZZ_Bool", DynamicMetaData.Position(1));
			propertyCollection.Add(typeof(ZString), "ZZZ_StringFromDropDown", ValidateList, DynamicMetaData.MaxLength(2), DynamicMetaData.ListDataSource(list), DynamicMetaData.Position(4));
			propertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithList" + AddOnColumnDataType.PartIdentifier + "1", ValidateList,
				DynamicMetaData.MaxLength(2), DynamicMetaData.ListDataSource(list), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));
			propertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithList" + AddOnColumnDataType.PartIdentifier + "2", ValidateText,
				DynamicMetaData.MaxLength(3), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));
			propertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithoutList" + AddOnColumnDataType.PartIdentifier + "1", ValidateText,
				DynamicMetaData.MaxLength(3), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));
			propertyCollection.Add(typeof(ZString), "ZZZ_ComboBoxWithoutList" + AddOnColumnDataType.PartIdentifier + "2", ValidateText,
				DynamicMetaData.MaxLength(3), DynamicMetaData.Position(5), DynamicMetaData.ParentCustomFieldType(AddOnColumnDataType.Codes.ComboBox));

			return new CustomBusinessObject(null, propertyCollection);
		}

		CustomBusinessObject GetCustomBusinessObject2()
		{
			var propertyCollection = GetCustomPropertyCollection();
			propertyCollection.Add(typeof(ZString), "ZZZ_String", ValidateText, DynamicMetaData.MaxLength(3));
			propertyCollection.Add(typeof(ZDecimal), "ZZZ_Decimal", DynamicMetaData.DecimalPlaces(3));
			propertyCollection.Add(typeof(ZString), "ZZZ_String2", ValidateText, DynamicMetaData.MaxLength(3));
			return new CustomBusinessObject(null, propertyCollection);
		}

		CustomPropertyCollectionImpl GetCustomPropertyCollection()
		{
			Dictionary<string, object> values = new Dictionary<string, object>();

			return new CustomPropertyCollectionImpl(
				propertyName =>
				{
					object value;
					return values.TryGetValue(propertyName, out value) ? value : null;
				},
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				});
		}

		void ValidateText(ZPropertyInfo propInfo)
		{
			MandatoryValidation.CheckEntered(propInfo);
		}

		void ValidateList(ZPropertyInfo propInfo)
		{
			ListValidation.ErrorIfInvalidCode(propInfo);
		}

		#endregion
	}
}
