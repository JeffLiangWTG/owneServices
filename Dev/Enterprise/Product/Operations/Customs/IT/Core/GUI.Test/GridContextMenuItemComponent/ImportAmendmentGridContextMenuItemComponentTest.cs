using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(AmendmentGridContextMenuItemComponent))]
sealed class ImportAmendmentGridContextMenuItemComponentTest : AmendmentGridContextMenuItemComponentAbstractTest
{
	public void TestSetAsAmendmentMenuItemVisible_WithEmptyEntryStatus()
	{
		AssertMenuItemVisible(ZString.Empty, false);
	}

	public void TestSetAsAmendmentMenuItemVisible_WithREGEntryStatus()
	{
		AssertMenuItemVisible("REG", true);
	}

	public void TestSetAsAmendmentMenuItemVisible_WithICCEntryStatus()
	{
		AssertMenuItemVisible("ICC", true);
	}

	public void TestSetAsAmendmentMenuItemVisible_WithUCLEntryStatus()
	{
		AssertMenuItemVisible("UCL", true);
	}

	public void TestSetAsAmendmentMenuItemVisible_WithAMDEntryStatus()
	{
		AssertMenuItemVisible("AMD", true);
	}

	protected override ZString GetMessageType() => "IMP";
}
