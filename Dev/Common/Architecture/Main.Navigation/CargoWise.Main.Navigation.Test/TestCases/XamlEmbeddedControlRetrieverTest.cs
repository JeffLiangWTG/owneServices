using System.Collections.Generic;
using System.Windows;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.WPF.Test
{
	class XamlEmbeddedControlRetrieverTest : TestCase
	{
		[RequiresSTA]
		public void TestRetrievedMultilingualControlCount()
		{
			var window = new Window();
			var userControl = new UserControlForTesting();
			userControl.DataContext = new DummyBindingObjectForTest();
			window.Content = userControl;

			window.Show();

			List<string> multilingualControls = new List<string>();
			var retriever = new XamlEmbeddedControlRetriever();
			retriever.OnControlProcessing += (sender, e) =>
				{
					if (typeof(MultilingualTextLabel).IsAssignableFrom(sender.GetType()))
					{
						multilingualControls.Add(e.ControlFullName);
					}
				};
			retriever.Retrieve(userControl);
			var allMultilingualControls = new List<string>(multilingualControls);

			multilingualControls = new List<string>();
			retriever.ProcessElementDecider = processElementDecider;
			retriever.Retrieve(userControl);

			window.Close();

			AssertContainsExactElementsInExactOrder
			(
				"All MultilingualTextLabel controls should be detected.",
				new[] {
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel`2",
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel`3", // nonTranslatableMultilingualXamlString
					"UserControlForTesting.Border.ContentPresenter.Grid.ItemsControl.StackPanel.Border.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.ItemsControl.StackPanel.ItemsControl.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.ContentPresenter.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.StackPanel.MultilingualTextLabel"
				},
				allMultilingualControls
			);

			AssertContainsExactElementsInExactOrder
			(
				"All MultilingualTextLabel controls except one should be detected.",
				new[] {
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel`2",
					"UserControlForTesting.Border.ContentPresenter.Grid.MultilingualTextLabel`3", // nonTranslatableMultilingualXamlString
					"UserControlForTesting.Border.ContentPresenter.Grid.ItemsControl.StackPanel.Border.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.ItemsControl.StackPanel.ItemsControl.MultilingualTextLabel",
					"UserControlForTesting.Border.ContentPresenter.Grid.ContentPresenter.MultilingualTextLabel"
				},
				multilingualControls
			);
		}

		bool processElementDecider(DependencyObject control)
		{
			var element = control as FrameworkElement;
			return element == null || element.Name != "stackPanelToSkip";
		}
	}

	class DummyBindingObjectForTest
	{
		public MultilingualString MultilingualText
		{ get { return (NoResString)"Test binding attribute"; } }

		public string Name
		{ get { return "TestName"; } }

		public bool IsTranslatable
		{ get { return true; } }

		public List<DummyBindingObjectForTest> Items
		{
			get
			{
				var items = new List<DummyBindingObjectForTest>();
				items.Add(new DummyBindingObjectForTest());
				return items;
			}
		}
	}
}
