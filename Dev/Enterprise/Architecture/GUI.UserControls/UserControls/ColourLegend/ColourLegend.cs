using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormsLocalizedTest]
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public class ColourLegend : FlowLayoutPanel
	{
		public ColourLegend()
		{
			InitializeAvailableColors();
		}

		public override bool AutoScroll
		{
			get { return true; }
			set { base.AutoScroll = value; }
		}

		public int AvailableColorsCount { get; private set; }

		public Color GetColour(ZGuid pk, ZString text)
		{
			ColourLegendElement element;

			if (lookup.TryGetValue(pk, out element))
			{
				element.Text = text;
			}
			else
			{
				element = new ColourLegendElement();
				element.Text = text;

				if (availableColours.Count > 0)
				{
					element.Colour = availableColours.Pop();
				}

				lookup.Add(pk, element);
				Controls.Add(element);
				element.TabIndex = 0;
			}

			return element.Colour;
		}

		public void Release(ZGuid pk)
		{
			ColourLegendElement element;
			if (lookup.TryGetValue(pk, out element))
			{
				Controls.Remove(element);
				lookup.Remove(pk);

				if (!element.Colour.IsSystemColor)
				{
					availableColours.Push(element.Colour);
				}

				element.Dispose();
			}
		}

		#region Implementation

		readonly Stack<Color> availableColours = new Stack<Color>();
		readonly Dictionary<ZGuid, ColourLegendElement> lookup = new Dictionary<ZGuid, ColourLegendElement>();

		static IEnumerable<Color> PreferedColours
		{
			get
			{
				yield return Color.Yellow;
				yield return Color.Bisque;
				yield return Color.PaleTurquoise;
			}
		}

		static IEnumerable<Color> SkipColours
		{
			get
			{
				yield return Color.White;
				yield return Color.Black;
			}
		}

		void InitializeAvailableColors()
		{
			foreach (KnownColor knownColor in Enum.GetValues(typeof(KnownColor)))
			{
				Color color = Color.FromKnownColor(knownColor);
				if (color.IsNamedColor && !color.IsSystemColor && !PreferedColours.Contains(color) && !SkipColours.Contains(color) && !availableColours.Contains(color))
				{
					availableColours.Push(color);
				}
			}
			foreach (Color preferedColour in PreferedColours)
			{
				if (!availableColours.Contains(preferedColour))
				{
					availableColours.Push(preferedColour);
				}
			}
			AvailableColorsCount = availableColours.Count;
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, 30, specified);
		}

		#endregion
	}
}
