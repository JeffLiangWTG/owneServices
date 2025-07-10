using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	public class CodeDescriptionListEditControlForRegistryExTest : TransactionedTestCase
	{
		[DeveloperOnlyTest]
		public void TestPaste()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			string clipText = @"Country Code,Region
AD,EU1
AE,MA1
AF,MA2";
			SafeClipboard.SetText(clipText);
			using (CodeDescriptionListEditControlForRegistryExForTest control = new CodeDescriptionListEditControlForRegistryExForTest(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "x", "Description", 5))
			{
				control.PasteMenuItem_ClickForTest(null, EventArgs.Empty);
				var list = new ReadOnlyCodeDescriptionPairList(control.FieldValue);
				AssertEquals("list count", 3, list.Count);
				AssertEquals("Code", "AD", list[0].Code);
				AssertEquals("Region", "EU1", list[0].Description);
				AssertEquals("Code", "AE", list[1].Code);
				AssertEquals("Region", "MA1", list[1].Description);
				AssertEquals("Code", "AF", list[2].Code);
				AssertEquals("Region", "MA2", list[2].Description);
			}

			SafeClipboard.SetText(clipText.Replace(',', '\t'));
			using (CodeDescriptionListEditControlForRegistryExForTest control = new CodeDescriptionListEditControlForRegistryExForTest(registryItem, true, true, CharacterCasing.Normal, CharacterCasing.Normal, "x", "Description", 5))
			{
				control.PasteMenuItem_ClickForTest(null, EventArgs.Empty);
				var list = new ReadOnlyCodeDescriptionPairList(control.FieldValue);
				AssertEquals("list count", 3, list.Count);
				AssertEquals("Code", "AD", list[0].Code);
				AssertEquals("Region", "EU1", list[0].Description);
				AssertEquals("Code", "AE", list[1].Code);
				AssertEquals("Region", "MA1", list[1].Description);
				AssertEquals("Code", "AF", list[2].Code);
				AssertEquals("Region", "MA2", list[2].Description);
				SafeClipboard.SetText(clipText.Replace("MA2", "NEW"));
				control.PasteMenuItem_ClickForTest(null, EventArgs.Empty);
				list = new ReadOnlyCodeDescriptionPairList(control.FieldValue);
				AssertEquals("list count", 3, list.Count);
				AssertEquals("Code", "AD", list[0].Code);
				AssertEquals("Region", "EU1", list[0].Description);
				AssertEquals("Code", "AE", list[1].Code);
				AssertEquals("Region", "MA1", list[1].Description);
				AssertEquals("Code", "AF", list[2].Code);
				AssertEquals("Region", "NEW", list[2].Description);
				SafeClipboard.SetText("ZZ,YYY,IGNORE,ALSO,THIS\nXX,\nX,code too short");
				control.PasteMenuItem_ClickForTest(null, EventArgs.Empty);
				list = new ReadOnlyCodeDescriptionPairList(control.FieldValue);
				AssertEquals("list count", 4, list.Count);
				AssertEquals("Code", "AD", list[0].Code);
				AssertEquals("Region", "EU1", list[0].Description);
				AssertEquals("Code", "AE", list[1].Code);
				AssertEquals("Region", "MA1", list[1].Description);
				AssertEquals("Code", "AF", list[2].Code);
				AssertEquals("Region", "NEW", list[2].Description);
				AssertEquals("Code", "ZZ", list[3].Code);
				AssertEquals("Region", "YYY", list[3].Description);
			}
		}

		class CodeDescriptionListEditControlForRegistryExForTest : CodeDescriptionListEditControlForRegistryEx
		{
			public CodeDescriptionListEditControlForRegistryExForTest(IRegistryItem registryItem, bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption, string descriptionColumnCaption, int codeColumnMaxLength) : base(registryItem, showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, descriptionColumnCaption, codeColumnMaxLength)
			{
			}

			internal void PasteMenuItem_ClickForTest(object sender, EventArgs e)
			{
				PasteMenuItem_Click(sender, e);
			}

			internal ZGrid CodeDescriptionGridForTest
			{
				get
				{
					return base.CodeDescriptionGrid;
				}
			}

			internal IRegistryItem RegistryItemForTest
			{
				get
				{
					return registryItem;
				}
			}
		}
	}
}
