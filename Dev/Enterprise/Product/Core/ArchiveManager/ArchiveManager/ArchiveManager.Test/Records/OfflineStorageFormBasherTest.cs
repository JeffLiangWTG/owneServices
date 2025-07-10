using System.Windows.Forms;
using Enterprise.ArchiveManager.Business.Records;
using Enterprise.ArchiveManager.GUI.Records;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Records
{
	[TestedType(typeof(OfflineStorageForm))]
	class OfflineStorageFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var offlineStorage = new OfflineStorage();
			var form = new OfflineStorageForm(offlineStorage);
			MissingResourceStringChecker.ExcludeFromTest(form.confirmationLabel);
			return form;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
			=> control.Name == "progressTextBox";
	}
}
