using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ReportOrderRegistryItemEditor))]
	public class ReportOrderRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ReportOrderRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ReportOrderControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ReportOrderControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ReportOrderRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			ReportOrderCollection collection = new ReportOrderCollection();

			ReportOrder reportOrder = collection.AddNew();
			reportOrder.Language = reportOrder.LanguageList[0].Code;
			reportOrder.AccountsOrderBeginsWith = reportOrder.AccountOrderTypeList[0].Code;

			BusinessObjectFactory factory = new BusinessObjectFactory();

			AccGLAccountDescriptor accGLAccountDescriptor1 = factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accGLAccountDescriptor1.AJ_LocalAccountNumber = "1000.00.00";
			accGLAccountDescriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			accGLAccountDescriptor1.AJ_Language = reportOrder.LanguageList[0].Code;
			AccGLAccountDescriptor accGLAccountDescriptor2 = factory.NewWithValidTestData<AccGLAccountDescriptor>();
			accGLAccountDescriptor2.AJ_LocalAccountNumber = "2000.00.00";
			accGLAccountDescriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.Header;
			accGLAccountDescriptor2.AJ_Language = reportOrder.LanguageList[0].Code;

			factory.Save();

			reportOrder.GLAccountSecondReportStartsFrom = accGLAccountDescriptor2.PK;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
