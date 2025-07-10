using System;
using System.Windows.Forms;
using Enterprise.Customs.CN.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNDeclarationDeadlineWarningThresholdEditor))]
	class CNDeclarationDeadlineWarningThresholdEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CNDeclarationDeadlineWarningThresholdEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CNDeclarationDeadlineWarningThresholdUserControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType() => typeof(CNDeclarationDeadlineWarningThresholdUserControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CNDeclarationDeadlineWarningThresholdRegistryItem("", null, null, null, RegistryStorageFlags.System, CNDeclarationDeadlineWarningThresholdCollection.GetDefault());
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CNDeclarationDeadlineWarningThresholdCollection();
			var deadlineWarning = collection.AddNew();
			deadlineWarning.TransportMode = "ALL";
			deadlineWarning.FirstLevelThreshold = 0;
			deadlineWarning.FirstLevelWarningColor = "255000000";
			deadlineWarning.SecondLevelThreshold = 3;
			deadlineWarning.SecondLevelWarningColor = "255160122";
			deadlineWarning.ThirdLevelThreshold = 7;
			deadlineWarning.ThirdLevelWarningColor = "255255224";
			deadlineWarning.DelayedWarningColor = "";
			Factory.Save();
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
