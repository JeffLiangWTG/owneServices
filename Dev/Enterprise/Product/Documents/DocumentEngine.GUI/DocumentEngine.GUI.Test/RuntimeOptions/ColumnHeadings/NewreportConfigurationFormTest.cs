using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(NewReportConfigurationForm))]
	sealed class NewReportConfigurationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			LookupField linkedField = new LookupField(Factory);
			linkedField.DisplayName = "Test";
			linkedField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
			ColumnConfigurationsManager manager = new ColumnConfigurationsManager(ZGuid.NewZGuid(), false);
			manager.SaveToFilterField = "Test";
			manager.AddLinkedField(linkedField);
			return new NewReportConfigurationForm(new NewConfiguration(manager));
		}
	}
}
