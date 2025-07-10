using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Notifications.Testing
{
	sealed class AdornmentLayoutTest : TestCase
	{
		public void TestGetBackroundAdornmentTargetsReturnsSourceByDefault()
		{
			var layout = new AdornmentLayout<TextBox>();

			var source = new TextBox();
			var target = GetFirstOne(layout.GetBackroundAdornmentTargets(source));

			Assert(target == source);
		}

		public void TestGetGetIconAdornmentTargetsReturnsSourceByDefault()
		{
			var layout = new AdornmentLayout<TextBox>();

			var source = new TextBox();
			var iconLayout = (IconLayout)GetFirstOne(layout.GetIconAdornmentTargets(source));

			Assert(iconLayout.Target == source);
			Assert(iconLayout.Align == IconAlignment.Default);
		}

		static TElement GetFirstOne<TElement>(IEnumerable<TElement> elements)
		{
			foreach (var element in elements)
			{
				return element;
			}

			throw new Exception("Empty");
		}
	}
}
