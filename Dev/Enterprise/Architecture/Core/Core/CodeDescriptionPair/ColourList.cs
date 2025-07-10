using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// A list containing the names of colors available in the System.Windows.Forms.Color class.
	/// The Code property of elements is the human-readable name, and the description is the property name as it is listed in the Color class.
	/// Ideally this should be bound to a drop-down list just displaying the Code property.
	/// </summary>
	public class ColorList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For unit tests only")]
		public ColorList()
			: base("Built from WinForms Color struct")
		{
			foreach (var item in GetKnownColors())
			{
				AddPair(item.HumanReadableName, item.ColorName);
			}
		}

		public static IEnumerable<(string HumanReadableName, string ColorName)> GetKnownColors()
		{
			var colorNames = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
				.Where(p => p.PropertyType == typeof(Color))
				.Select(p => p.Name)
				.OrderBy(n => n);

			foreach (var color in colorNames)
			{
				yield return (NameFromColorName(color), color);
			}
		}

		static readonly Regex PascalCaseRegex = new Regex("([A-Z]+[a-z]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public static Color ColorFromName(string name)
		{
			return Color.FromName(name.Replace(" ", ""));
		}

		public static string NameFromColor(Color color)
		{
			return NameFromColorName(color.Name);
		}

		static string NameFromColorName(string colorName)
		{
			return PascalCaseRegex.Replace(colorName, m => m.Value + " ").Trim();
		}
	}
}
