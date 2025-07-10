using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(UsersAuthorizedToReopenClosedPeriodsRegistryItemEditor))]
	public class UsersAuthorizedToReopenClosedPeriodsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new UsersAuthorizedToReopenClosedPeriodsRegistryItem("", null, null, null, RegistryStorageFlags.System, true);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new UsersAuthorizedToReopenClosedPeriodsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(UsersAuthorizedToReopenClosedPeriodsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			UsersAuthorizedToReopenClosedPeriodsCollection collection = new UsersAuthorizedToReopenClosedPeriodsCollection();

			UsersAuthorizedToReopenClosedPeriods usersAuthorizedToReopenClosedPeriods = collection.AddNew();
			usersAuthorizedToReopenClosedPeriods.StaffPK = usersAuthorizedToReopenClosedPeriods.GlobalStaffList[0].PK;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			RegistryZUserControl control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		#endregion
	}
}
