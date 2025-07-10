using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(AmendmentGridContextMenuItemComponent))]
sealed class ExportAmendmentGridContextMenuItemComponentTest : AmendmentGridContextMenuItemComponentAbstractTest
{
	public void TestSetAsAmendmentMenuItemVisible_WithEmptyEntryStatus()
	{
		AssertMenuItemVisible(ZString.Empty, false);
	}

	public void TestSetAsAmendmentMenuItemVisible_UCC6_WithREGEntryStatus()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertMenuItemVisible("REG", true);
		}
	}

	public void TestSetAsAmendmentMenuItemVisible_UCC6_WithECCEntryStatus()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertMenuItemVisible("ECC", true);
		}
	}

	public void TestSetAsAmendmentMenuItemVisible_UCC6_WithUCLEntryStatus()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertMenuItemVisible("UCL", true);
		}
	}

	public void TestSetAsAmendmentMenuItemVisible_UCC6_WithAMDEntryStatus()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertMenuItemVisible("AMD", true);
		}
	}

	public void TestSetAsAmendmentMenuItemVisible_UCC6_WithEXIEntryStatus()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			AssertMenuItemVisible("EXI", true);
		}
	}

	public void TestSetAsAmendmentMenuItemVisible_NoUcc6_WithREGEntryStatus()
	{
		AssertMenuItemVisible("REG", false);
	}

	public void TestSetAsAmendmentMenuItemVisible_NoUcc6_WithECCEntryStatus()
	{
		AssertMenuItemVisible("ECC", false);
	}

	public void TestSetAsAmendmentMenuItemVisible_NoUcc6_WithUCLEntryStatus()
	{
		AssertMenuItemVisible("UCL", false);
	}

	public void TestSetAsAmendmentMenuItemVisible_NoUCC6_WithAMDEntryStatus()
	{
		AssertMenuItemVisible("AMD", false);
	}

	public void TestSetAsAmendmentMenuItemVisible_NoUCC6_WithEXIEntryStatus()
	{
		AssertMenuItemVisible("EXI", false);
	}

	protected override ZString GetMessageType() => "EXP";
}
