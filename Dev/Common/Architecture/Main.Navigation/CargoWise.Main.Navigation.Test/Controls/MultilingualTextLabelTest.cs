using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.WPF.Test
{
	[TestedType(typeof(MultilingualTextLabel))]
	[SuppressDataContextCheck]
	class MultilingualTextLabelBasherTest : WPFControlBasherTest
	{
		protected override Control GetControlToBashCore()
		{
			var control = new MultilingualTextLabel();
			control.IsTranslatable = false;

			return control;
		}

		[RequiresSTA]
		public void TestTranslationFeedbackManager()
		{
			var multilingualTextLabelWithText = new MultilingualTextLabel { Content = "blah blah" };
			var multilingualTextLabelWithMultilingualBinding = new MultilingualTextLabel { Content = new TextBlock() { Text = "Text" }, MultilingualText = (NoResString)"TestString" };
			var multilingualTextLabelWithContentControl = new MultilingualTextLabel { Content = new Button() { Content = "TestButton" } };

			performActionsOnControls(multilingualTextLabelWithText);
			performActionsOnControls(multilingualTextLabelWithMultilingualBinding);
			performActionsOnControls(multilingualTextLabelWithContentControl);
		}

		[RequiresSTA]
		public void TestMultilingualTextLabelWithText()
		{
			var window = new Window();
			var uri = new Uri("/CargoWise.Main.Navigation;component/WPF/MultilingualTextLabelResources.xaml", UriKind.Relative);
			var dictionary = new ResourceDictionary() { Source = uri };

			window.Resources.MergedDictionaries.Add(dictionary);

			var multilingualTextLabelWithMultilingualBinding = new MultilingualTextLabel { MultilingualText = (NoResString)"Test__MultilingualText" };

			window.Content = multilingualTextLabelWithMultilingualBinding;
			window.Show();
			var child = FindVisualChildren<ContentPresenter>(multilingualTextLabelWithMultilingualBinding).Single();
			Assert(!child.RecognizesAccessKey);
			window.Close();
		}

		IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		[RequiresSTA]
		public void TestMultilingualTextLabelDisplayText()
		{
			var window = new Window();

			var bind = new Binding();
			bind.Source = new DummyBindingObjectForTest();
			bind.Path = new PropertyPath("Name");

			var textBlock = new TextBlock();
			textBlock.SetBinding(TextBlock.TextProperty, bind);

			var multilingualTextLabelWithMultilingualBinding = new MultilingualTextLabel { Content = textBlock, MultilingualText = (NoResString)"Test_MultilingualText" };

			window.Content = multilingualTextLabelWithMultilingualBinding;
			window.Show();
			AssertEquals("TestName", ((TextBlock)multilingualTextLabelWithMultilingualBinding.Content).Text);
			AssertNotEquals("TestMultilingualText", ((TextBlock)multilingualTextLabelWithMultilingualBinding.Content).Text);
			window.Close();
		}

		void performActionsOnControls(MultilingualTextLabel multilingualTextLabel)
		{
			using (multilingualTextLabel.TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			{
				Assert(!multilingualTextLabel.InTranslationFeedBackMode);

				var mouseEventArgs = new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount) { RoutedEvent = ButtonBase.ClickEvent };

				multilingualTextLabel.GetType()
					.InvokeMember("OnMouseEnter",
						BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null,
						multilingualTextLabel, new object[] { mouseEventArgs });
				Assert(multilingualTextLabel.InTranslationFeedBackMode);

				var mouseButtonEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = ButtonBase.ClickEvent };
				Assert(!mouseButtonEventArgs.Handled);

				multilingualTextLabel.GetType()
					.InvokeMember("OnPreviewMouseLeftButtonDown",
						BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null,
						multilingualTextLabel, new object[] { mouseButtonEventArgs });
				Assert(mouseButtonEventArgs.Handled);
			}
		}
	}
}
