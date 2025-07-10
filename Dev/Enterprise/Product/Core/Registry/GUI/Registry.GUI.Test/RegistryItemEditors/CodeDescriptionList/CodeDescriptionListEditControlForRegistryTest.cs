using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class CodeDescriptionListEditControlForRegistryTest : TransactionedTestCase
	{
		static int staticCounter = 1;

		int keyStrokesCounter;

		public void TestSaveAndLoadLayout()
		{
			CodeDescriptionPairListRegistryItem registryItem1 = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM" + staticCounter++, null, null, null, 3, RegistryStorageFlags.System);
			CodeDescriptionPairListRegistryItem registryItem2 = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM" + staticCounter++, null, null, null, 3, RegistryStorageFlags.System);

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem1, false, true, CharacterCasing.Normal, CharacterCasing.Normal, "Code", "Description", 3))
			{
				//To make this control bound
				control.FieldValue = new CodeDescriptionPairList().ToXMLByteArray();

				AssertEquals("Precondition: Column 0 should be the Description column", "Description", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.MappingName);
				Assert("Precondition: Column 0 width should not be 100", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width != 100);

				control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width = 100;
				control.CodeDescriptionGridForTest.Columns.HasLayoutChanged = true;
			}

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem2, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "Code", "Description", 3))
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", control.CodeDescriptionGridForTest.Columns[1].ColumnStyle.MappingName);
				Assert("Precondition: Column 0 width should not be 200", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width != 200);
				Assert("Precondition: Column 1 width should not be 300", control.CodeDescriptionGridForTest.Columns[1].ColumnStyle.Width != 300);

				//To make this control bound
				control.FieldValue = new CodeDescriptionPairList().ToXMLByteArray();

				control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width = 200;
				control.CodeDescriptionGridForTest.Columns[1].ColumnStyle.Width = 300;
				control.CodeDescriptionGridForTest.Columns.HasLayoutChanged = true;
			}

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem1, false, true, CharacterCasing.Normal, CharacterCasing.Normal, "Code", "Description", 3))
			{
				control.FieldValue = new CodeDescriptionPairList().ToXMLByteArray();
				AssertEquals("Column 0 should be the Description column", "Description", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Column 0 width should be 100", 100, control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width);
			}

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem2, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "Code", "Description", 3))
			{
				control.FieldValue = new CodeDescriptionPairList().ToXMLByteArray();
				AssertEquals("Column 0 should be the Code column", "Code", control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Column 1 should be the Description column", "Description", control.CodeDescriptionGridForTest.Columns[1].ColumnStyle.MappingName);
				AssertEquals("Column 0 width should be 200", 200, control.CodeDescriptionGridForTest.Columns[0].ColumnStyle.Width);
				AssertEquals("Column 1 width should be 300", 300, control.CodeDescriptionGridForTest.Columns[1].ColumnStyle.Width);
			}
		}

		public void TestCodeColumnMaxLength()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "x", "Description", 5))
			{
				AssertEquals("Code column max length", 5, ((ZTextBoxColumnStyle)control.CodeDescriptionGridForTest.Columns["Code"].ColumnStyle).TextBox.MaxLength);
				AssertEquals("Description column max length", 32767, ((ZTextBoxColumnStyle)control.CodeDescriptionGridForTest.Columns["Description"].ColumnStyle).TextBox.MaxLength);
			}

			using (CodeDescriptionListEditControlForRegistry control = new CodeDescriptionListEditControlForRegistry(registryItem, false, true, CharacterCasing.Normal, CharacterCasing.Normal, "y", "Description", 3))
			{
				AssertEquals("There should only be one column.", 1, control.CodeDescriptionGridForTest.Columns.Count);
				AssertEquals("Description column max length", 32767, ((ZTextBoxColumnStyle)control.CodeDescriptionGridForTest.Columns["Description"].ColumnStyle).TextBox.MaxLength);
			}
		}

		public void TestProcessCmdKey()
		{
			keyStrokesCounter = 0;
			var registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			using (var control = new CodeDescriptionListEditControlForRegistry(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "x", "Description", 5))
			{
				control.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(HandleHasChangesChanged);
				AssertEquals("Precondition", 0, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Y);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.ShiftKey | Keys.Shift));
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.ControlKey | Keys.Control));
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.Menu | Keys.Alt));
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)(Keys.Shift | Keys.Tab));
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Tab);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.LWin);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.RWin);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Up);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Down);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Left);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Right);
				AssertEquals(1, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.K);
				AssertEquals(2, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.NumPad1);
				AssertEquals(3, keyStrokesCounter);

				KeySender.SendKeyDownToProcessCmdKey(control, (int)Keys.Back);
				AssertEquals(4, keyStrokesCounter);

				void HandleHasChangesChanged(object sender, HasChangesChangedEventArgs e)
				{
					keyStrokesCounter++;
					AssertEquals(control, e.ObjectThatWasChanged);
				}
			}
		}
	}
}
