using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControl_EntryDetailsTest : TestCaseWithFactory
{
	public void TestGetNewEntryDetailsPanelLayout()
	{
		using (var control = new MessageUserControlForTest())
		{
			AssertType<EntryDetailsLayout>(control.GetNewEntryDetailsPanelLayoutExposed());
		}
	}

	public void TestDynamicLayoutApplied()
	{
		using (var control = new MessageUserControlForTest())
		{
			AssertEquals("DynamicLayoutApplied", true, control.DynamicLayoutAppliedExposed);
		}
	}

	public void TestEntryDetailsControlsForUcc6Export()
		=> AssertControlsInEntryDetailsTab(ucc6ExportControlNames, isUcc6: true, messageType: EUJobMessageTypeList.Codes.Export);

	public void TestEntryDetailsControlForImport()
		=> AssertControlsInEntryDetailsTab(ucc6ExportControlNames, isUcc6: false, messageType: EUJobMessageTypeList.Codes.Import);

	public void TestControlWidthInTotalsSection()
	{
		declaration.CustomsEntryHeaders.AddNew();
		var controls = new[]
		{
			"NoPacksCalcEdit",
			"AmountCalcEdit",
			"InvoiceAmountCalcEdit",
			"FreightAdjustmentCalcEdit",
			"DutyCalcEdit",
			"VatCalcEdit",
			"EntryLinesCountCalcEdit"
		};

		AssertControlWidthIsSame(controls);
	}

	public void TestControlWidthInStatusSection()
	{
		declaration.CustomsEntryHeaders.AddNew();
		var controls = new[]
		{
			"MessageStatusUserControl",
			"EntryStatusDropEdit",
			"ControlChannelDropEdit",
			"MessageTypeDropEdit",
		};

		AssertControlWidthIsSame(controls);
	}

	public void TestControlWidthInCustomsSection()
	{
		declaration.CustomsEntryHeaders.AddNew();
		var controls = new[]
		{
			"RegistrationNumberTextBox",
			"CustomsOfficeTextBox",
			"SubmittedDateDateEdit",
			"MRNTextBox",
			"ReleaseCodeTextBox",
			"ReleaseDateDateEdit"
		};

		AssertControlWidthIsSame(controls);
	}

	public void TestEntryDetailsControlsReadOnly()
	{
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = "MRN";
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = "ENTRY_NUM";
		cusEntryNumber.CE_IssueDate = new ZDateTime(2021, 04, 28);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertReadOnlyControls("UCC6, EXP");

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertReadOnlyControls("UCC6, IMP");
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertReadOnlyControls("Non-UCC6, EXP");

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertReadOnlyControls("Non-UCC6, MISC");
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	void AssertControlWidthIsSame(IEnumerable<string> controlNames)
	{
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			var entryDetailsControl = GetEntryDetailsControl(control);
			var controlNameWithWidths = GetControlWidths(entryDetailsControl, controlNames).ToArray();
			var controlNamesAndWidthMessage = string.Join(", ", controlNameWithWidths.Select((c) => $"{c.Item1}={c.Item2}"));

			CombineAssertions(() =>
			{
				AssertEquals($"Control Width Should be same for -> {controlNamesAndWidthMessage}", true, controlNameWithWidths.Select(c => c.Item2).AllSame());
			});
		}

		IEnumerable<(string, int)> GetControlWidths(EU.GUI.EntryDetailsUserControl entryDetailsControl, IEnumerable<string> controls)
		{
			foreach (var controlName in controls)
			{
				var childControls = entryDetailsControl.FindAll<Control>(c => c.Name == controlName.Trim());
				foreach (var childControl in childControls)
				{
					yield return (childControl.Name, childControl.Width);
				}
			}
		}
	}

	void AssertReadOnlyControls(string assertionGroupMessage)
	{
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			var entryDetailsControl = GetEntryDetailsControl(control);
			var allUserInputControls = GetAllUserInputControlsWithReadOnlyValue(entryDetailsControl);

			CombineAssertions(assertionGroupMessage, () =>
			{
				foreach (var (userInputControl, isReadOnly) in allUserInputControls)
				{
					AssertEquals($"{userInputControl.Name} should be ReadOnly", true, isReadOnly);
				}
			});
		}

		IReadOnlyCollection<(Control, bool)> GetAllUserInputControlsWithReadOnlyValue(EU.GUI.EntryDetailsUserControl entryDetailsUserControl)
		{
			return entryDetailsUserControl.FindAll<Control>(c => c.GetType().In(userInputControlTypes))
				.Select(c => (c, IsReadOnly(c)))
				.ToArray();

			bool IsReadOnly(Control c)
			{
				switch (c)
				{
					case ZDateEdit dropEdit:
						return dropEdit.ReadOnly;
					case ZTextBox textBox:
						return textBox.ReadOnly;
					case ZDropEdit dropEdit:
						return dropEdit.ReadOnly;
					default:
						throw new NotSupportedException($"{c.GetType()} is not yet supported");
				}
			}
		}
	}

	void AssertControlsInEntryDetailsTab(IReadOnlyCollection<string> controlNames, bool isUcc6, string messageType)
	{
		declaration.JE_MessageType = messageType;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUcc6))
		using (var form = new ZForm(declaration))
		using (var control = new MessageUserControlForTest())
		{
			form.Controls.Add(control);
			form.Show();

			var entryDetailsControl = GetEntryDetailsControl(control);

			CombineAssertions(() =>
			{
				foreach (var controlName in controlNames)
				{
					AssertControl(controlName, entryDetailsControl);
				}
			});
		}

		void AssertControl(string controlName, EU.GUI.EntryDetailsUserControl detailsControl)
		{
			var childControl = detailsControl.FindSingleOrDefault<Control>(controlName.Trim());
			AssertNotNull($"Control - {controlName}", childControl);
		}
	}

	static EU.GUI.EntryDetailsUserControl GetEntryDetailsControl(MessageUserControlForTest control)
	{
		var tabControl = control.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
		tabControl.SelectedTab = control.FindSingle<ZTabPage>("NewEntryDetailsTabPage");

		var entryDetailsControl = control.FindSingle<EU.GUI.EntryDetailsUserControl>("EntryDetailsUserControl");
		AssertNotNull("EntryDetailsUserControl", entryDetailsControl);
		return entryDetailsControl;
	}

	JobDeclaration declaration;
	readonly ImmutableArray<Type> userInputControlTypes = new[]
	{
		typeof(ZTextBox),
		typeof(ZDateEdit),
		typeof(ZDropEdit),
	}.ToImmutableArray();

	readonly IReadOnlyCollection<string> ucc6ExportControlNames = new[]
	{
		"EntryTypeTextBox",
		"ReferenceNumberTextBox",
		"IssueDateDateEdit",
		"IncotermTextBox",
		"GrossWeightUserControl",
		"NetWeightUserControl",
		"CustomsQuantityUserControl",
		"InvoiceAmountCalcEdit",
		"InvoiceCurrencyTextBox",
		"FreightAdjustmentCalcEdit",
		"MessageStatusTextBox",
		"MessageStatusDescriptionTextBox",
		"ControlChannelDropEdit",
		"MessageTypeDropEdit",
		"RegistrationNumberTextBox",
		"CustomsOfficeTextBox",
		"ReleaseCodeTextBox",
		"A93Grid",
		"ExitDateDateEdit",
		"ExitOfficeTextBox",
		"ExitOfficeDescriptionTextBox",
		"ExitStatusTextBox",
		"ExitStatusDescriptionTextBox",
		"ReferenceLabel",
		"TotalsLabel",
		"StatusLabel",
		"CustomsLabel",
		"A93Label",
		"ExitLabel",
	};
}
