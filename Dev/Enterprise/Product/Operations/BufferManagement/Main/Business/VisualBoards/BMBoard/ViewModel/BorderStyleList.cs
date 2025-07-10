using System;
using System.Collections.Generic;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BorderStyleList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Its a border style")]
		public BorderStyleList()
			: base("These are border styles")
		{
			foreach (var item in GetButtonBorderStyles())
			{
				AddPair(item);
			}
		}

		static IEnumerable<string> GetButtonBorderStyles()
		{
			foreach (VisualBoardButtonBorderStyle value in Enum.GetValues(typeof(VisualBoardButtonBorderStyle)))
			{
				if (value == VisualBoardButtonBorderStyle.None)
				{
					yield return new SizedButtonBorderStyle(value, null).ToString();
				}
				else
				{
					yield return new SizedButtonBorderStyle(value, false).ToString();
					yield return new SizedButtonBorderStyle(value, true).ToString();
				}
			}
		}
	}
}
