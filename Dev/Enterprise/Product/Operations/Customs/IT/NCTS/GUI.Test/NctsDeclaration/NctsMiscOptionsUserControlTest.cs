using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsMiscOptionsUserControlTest : TestCaseWithFactory
{
	public void TestNodeDropEdit()
	{
		using (var control = new NctsMiscOptionsUserControl())
		{
			var nodeDropEdit = control.FindSingleOrDefault<ZDropEdit>("NodeDropEdit");
			AssertNotNull(nodeDropEdit);
			Assert("Visible", nodeDropEdit.Visible);
			AssertEquals("PreBoundMaxLength", 3, nodeDropEdit.PreBoundMaxLength);
		}
	}

	public void TestSubscriberDropEdit()
	{
		using (var control = new NctsMiscOptionsUserControl())
		{
			var subscriberDropEdit = control.FindSingleOrDefault<ZDropEdit>("SubscriberDropEdit");
			AssertNotNull(subscriberDropEdit);
			Assert("Visible", subscriberDropEdit.Visible);
			AssertEquals("PreBoundMaxLength", 3, subscriberDropEdit.PreBoundMaxLength);
		}
	}

	[RequiresSTA]
	public void TestUseElectronicFolderCheckBoxVisibile()
	{
		AssertUseElectronicFolderCheckBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Departure, "Expected visible", expectedVisible: true);
	}

	public void TestUseElectronicFolderCheckBoxNotVisibile()
	{
		AssertUseElectronicFolderCheckBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Arrival, "Expected not visible", expectedVisible: false);
	}

	public void TestDeferralGroupBoxVisible()
	{
		AssertDeferralGroupBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Departure, "Expected visible", expectedVisible: true);
	}

	[RequiresSTA]
	public void TestDeferralGroupBoxNotVisible()
	{
		AssertDeferralGroupBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Arrival, "Expected not visible", expectedVisible: false);
	}

	[RequiresSTA]
	public void TestWarehouseGroupBoxVisible()
	{
		AssertWarehouseGroupBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Departure, "Expected visible", expectedVisible: true);
	}

	public void TestWarehouseGroupBoxNotVisible()
	{
		AssertWarehouseGroupBoxVisibility(EU.NCTS.Business.NctsMovementType.Codes.Arrival, "Expected not visible", expectedVisible: false);
	}

	public void TestParticipantDropEdit()
	{
		using (var control = new NctsMiscOptionsUserControl())
		{
			var participantDropEdit = control.FindSingleOrDefault<ZDropEdit>("ParticipantDropEdit");
			AssertNotNull(participantDropEdit);
			Assert("Visible", participantDropEdit.Visible);
			AssertEquals("PreBoundMaxLength", 3, participantDropEdit.PreBoundMaxLength);
		}
	}

	[RequiresSTA]
	public void TestParticipantDropEditVisible()
	{
		AssertParticipantDropEditVisibility(EU.NCTS.Business.NctsMovementType.Codes.Departure, "Expected visible", expectedVisible: true);
	}

	public void TestParticipantDropEditNotVisible()
	{
		AssertParticipantDropEditVisibility(EU.NCTS.Business.NctsMovementType.Codes.Arrival, "Expected not visible", expectedVisible: false);
	}

	[RequiresSTA]
	public void TestWarehouseAddressControl()
	{
		var header = Factory.NewDepartureNctsHeader();

		using (var form = new ZForm(header))
		using (var control = new NctsMiscOptionsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var warehouseAddressControl = control.FindSingle<ZAddressControl>("WarehouseAddressControl");
			AssertEquals("BindToOrgList", "MovementHeader.ITLookups.BondedWarehouseCollection", warehouseAddressControl.BindToOrgList);
		}
	}

	#region Implementation

	void AssertUseElectronicFolderCheckBoxVisibility(ZString movementType, ZString assertionMessage, bool expectedVisible)
	{
		AssertChildControlVisibility<ZCheckBox>(movementType, assertionMessage, "UseElectronicFolderCheckBox", expectedVisible);
	}

	void AssertDeferralGroupBoxVisibility(ZString movementType, ZString assertionMessage, bool expectedVisible)
	{
		AssertChildControlVisibility<ZGroupBox>(movementType, assertionMessage, "DeferralGroupBox", expectedVisible);
	}

	void AssertWarehouseGroupBoxVisibility(ZString movementType, ZString assertionMessage, bool expectedVisible)
	{
		AssertChildControlVisibility<ZGroupBox>(movementType, assertionMessage, "WarehouseGroupBox", expectedVisible);
	}

	void AssertParticipantDropEditVisibility(ZString movementType, ZString assertionMessage, bool expectedVisible)
	{
		AssertChildControlVisibility<ZDropEdit>(movementType, assertionMessage, "ParticipantDropEdit", expectedVisible);
	}

	void AssertChildControlVisibility<TControl>(ZString movementType, ZString assertionMessage, ZString controlName, bool expectedVisible)
		where TControl : Control
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(movementType);

		using (var form = new ZForm(header))
		using (var miscOptionsUserControl = new NctsMiscOptionsUserControl())
		{
			form.Controls.Add(miscOptionsUserControl);
			form.Show();
			var controlToAssert = miscOptionsUserControl.FindSingleOrDefault<TControl>(controlName);
			AssertNotNull("Control not found", controlToAssert);
			AssertEquals(assertionMessage, expectedVisible, controlToAssert.Visible);
		}
	}

	#endregion
}
