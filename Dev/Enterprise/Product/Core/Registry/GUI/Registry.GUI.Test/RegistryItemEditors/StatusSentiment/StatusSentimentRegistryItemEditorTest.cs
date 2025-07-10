using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Test
{
	[TestedType(typeof(StatusSentimentRegistryItemEditor))]
	public class StatusSentimentRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new StatusSentimentRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((StatusSentimentControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(StatusSentimentControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new StatusSentimentRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection1 = new StatusSentimentCollection();
			var statusSentiment1InCollection1 = collection1.AddNew();
			statusSentiment1InCollection1.Code = "BKD";
			statusSentiment1InCollection1.Description = "Booked";
			statusSentiment1InCollection1.Sentiment = StatusSentiment.SentimentTypes.Success;
			statusSentiment1InCollection1.Variant = StatusSentiment.VariantTypes.Fill;

			var collection2 = new StatusSentimentCollection();
			var statusSentiment2InCollection2 = collection2.AddNew();
			statusSentiment2InCollection2.Code = "INC";
			statusSentiment2InCollection2.Description = "Incomplete";
			statusSentiment2InCollection2.Sentiment = StatusSentiment.SentimentTypes.Info;
			statusSentiment2InCollection2.Variant = StatusSentiment.VariantTypes.Outline;

			return new[] { collection1, collection2 };
		}

		#endregion

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}
	}
}
