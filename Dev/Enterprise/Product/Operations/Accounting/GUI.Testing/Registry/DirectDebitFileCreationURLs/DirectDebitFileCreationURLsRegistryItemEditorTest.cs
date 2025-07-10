using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(DirectDebitFileCreationURLsRegistryItemEditor))]
	public class DirectDebitFileCreationURLsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DirectDebitFileCreationURLRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DirectDebitFileCreationURLsRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DirectDebitFileCreationURLsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { Settings };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		DirectDebitFileCreationURLCollection settings;
		DirectDebitFileCreationURLCollection Settings
		{
			get
			{
				if (settings == null)
				{
					BusinessObject bankAcct = Factory.NewWithValidTestData<AccBankAccount>();
					bankAcct[AccBankAccountSchema.AB_AllowAutoDDR] = true;
					bankAcct[AccBankAccountSchema.AB_AutoDDRFormat] = "NAB";
					bankAcct[AccBankAccountSchema.AB_IsActive] = true;

					Factory.Save();
					settings = new DirectDebitFileCreationURLCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
					DirectDebitFileCreationURL setting = settings.AddNew();
					setting.BankAccountPK = bankAcct.PK;
					setting.BankWebsite = "xxx";
				}
				return settings;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			RegistryZUserControl control = (RegistryZUserControl)editorPane;
			return !control.ReadOnly;
		}

		#endregion
	}
}
