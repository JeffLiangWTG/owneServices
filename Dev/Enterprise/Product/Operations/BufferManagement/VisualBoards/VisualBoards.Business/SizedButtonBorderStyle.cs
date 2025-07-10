using System;
using System.Globalization;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.VisualBoards.Business
{
	public class SizedButtonBorderStyle
	{
		public SizedButtonBorderStyle(VisualBoardButtonBorderStyle borderStyle, bool? isLarge)
		{
			this.borderStyle = borderStyle;
			this.isLarge = isLarge;
		}

		readonly VisualBoardButtonBorderStyle borderStyle;
		readonly bool? isLarge;

		public VisualBoardButtonBorderStyle BorderStyle
		{
			get { return borderStyle; }
		}

		public bool IsLarge
		{
			get { return isLarge ?? false; }
		}

		public override bool Equals(object obj)
		{
			var other = obj as SizedButtonBorderStyle;
			return other != null
				&& other.BorderStyle == BorderStyle
				&& other.IsLarge == IsLarge;
		}

		public override int GetHashCode()
		{
			return (BorderStyle.ToString() + IsLarge).GetHashCode();
		}

		public override string ToString()
		{
			return isLarge.HasValue ? string.Format(CultureInfo.InvariantCulture, "{0} - {1}", BorderStyle, IsLarge ? Large : Small) : BorderStyle.ToString();
		}

		public static SizedButtonBorderStyle FromCode(string code)
		{
			var values = code.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
			var borderStyle = (VisualBoardButtonBorderStyle)Enum.Parse(typeof(VisualBoardButtonBorderStyle), values[0].Trim());

			return values.Length == 2
				? new SizedButtonBorderStyle(borderStyle, values[1].Trim() == Large)
				: new SizedButtonBorderStyle(borderStyle, null);
		}

		public static SizedButtonBorderStyle Default
		{
			get { return new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, false); }
		}

		public static TagBorderStyle? FromCodeToTagBorderStyle(string code)
		{
			var borderStyle = FromCode(code);

			switch (borderStyle.BorderStyle)
			{
				case VisualBoardButtonBorderStyle.Inset:
				case VisualBoardButtonBorderStyle.Outset:
				case VisualBoardButtonBorderStyle.Solid:
					return TagBorderStyle.Solid;
				case VisualBoardButtonBorderStyle.Dashed:
					return TagBorderStyle.Dashed;
				case VisualBoardButtonBorderStyle.Dotted:
					return TagBorderStyle.Dotted;
				default:
					return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant values")]
		const string Small = "small";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "constant values")]
		const string Large = "large";
	}
}
