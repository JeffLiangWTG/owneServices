#if !WINZOR
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CargoWise.Main.Navigation.WPF;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test
{
	sealed class NextHomeResourcesTest : TestCase
	{
		NextHomeResources nextHomeResources;
		protected override void SetUp()
		{
			nextHomeResources = new NextHomeResources();
			nextHomeResources.InitializeComponent();
			base.SetUp();
		}
		[RequiresSTA]
		public void TestRecentMessagesRowTemplateSenderCompanyShouldNotOverlapTimestamp()
		{
			Window window = new();
			try
			{
				var template = GetResources<DataTemplate>("RecentMessagesRowTemplate");
				ListBox listBox = new();
				listBox.ItemTemplate = template;
				window.Content = listBox;
				listBox.ItemsSource = new List<RecentMessage>()
				{
					new RecentMessage()
					{
						SenderCompanyName = "TestCompanyName",
						SenderName = "TestSender",
						PostedTimeAgo = "One Hour Ago",
						JobCode = "TestJobCode",
					}
				};
				window.Show();
				var listBoxItem = FindFirstVisualChild<ListBoxItem>(listBox);
				var grid = FindFirstVisualChild<Grid>(listBoxItem);
				AssertNotNull("Grid not found", grid);
				var secondChildGrid = grid.Children[1] as Grid;
				AssertNotNull("Second child grid not found", secondChildGrid);
				var senderCompanyNameLabel = secondChildGrid.Children[0] as MultilingualTextLabel;
				var postedTimeAgoLabel = secondChildGrid.Children[1] as MultilingualTextLabel;
				AssertNotNull("Posted time ago text block not found", postedTimeAgoLabel);
				AssertNotNull("Sender company name text block not found", senderCompanyNameLabel);
				Assert(Grid.GetColumn(senderCompanyNameLabel) == 0);
				Assert(Grid.GetColumn(postedTimeAgoLabel) == 1);
				var senderCompanyTextBlock = FindFirstVisualChild<TextBlock>(senderCompanyNameLabel);
				Assert(senderCompanyTextBlock.TextTrimming == TextTrimming.CharacterEllipsis);
				Assert(senderCompanyTextBlock.TextWrapping == TextWrapping.NoWrap);
			}
			finally
			{
				window.Close();
			}
		}

		static T FindFirstVisualChild<T>(DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
			{
				return default;
			}

			for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T result)
				{
					return result;
				}
				T foundChild = FindFirstVisualChild<T>(child);
				if (foundChild != null)
				{
					return foundChild;
				}
			}

			return null;
		}

		T GetResources<T>(string resourceKey)
		{
			var obj = nextHomeResources[resourceKey];
			AssertNotNull($"Resource '{resourceKey}' not found", obj);
			Assert($"Resource type does not match,Expected:{typeof(T).FullName}, Actual:{obj.GetType().FullName}", obj is T);
			return (T)obj;
		}
	}
}
#endif
