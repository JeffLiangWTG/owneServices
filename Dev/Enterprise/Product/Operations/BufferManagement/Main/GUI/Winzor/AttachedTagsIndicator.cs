using System;
using System.Text;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AttachedTagsIndicator
	{
		public override string ExtraStyleString => base.ExtraStyleString + GenerateTagStyleString();

		string GenerateTagStyleString()
		{
			var generatedGradient = new StringBuilder();
			if (Tags.Count > 0)
			{
				const int SmallBorderWidth = 1;
				const int LargeBorderWidth = 3;
				var displacement = GetDisplacement(Tags.Count);
				var totalDistance = displacement * Tags.Count;
				var eachPercent = Math.Ceiling((double)displacement / totalDistance * 100);
				var isLarge = false;
				if (Parent is TaskCardControl taskCardControl)
				{
					isLarge = taskCardControl.CardContent.BorderStyle.IsLarge;
				}
				var height = Height - ControlDpiScalingHelper.ScaleToCurrentDpiX(isLarge ? 2 * LargeBorderWidth : 2 * SmallBorderWidth);
				generatedGradient.Append(Tags[0].Name).Append(" ").Append(eachPercent).Append("%,");
				for (int i = 1; i < Tags.Count; i++)
				{
					generatedGradient.Append(Tags[i].Name).Append(" 0.1%,");
					generatedGradient.Append(Tags[i].Name).Append(" ").Append(eachPercent * (i + 1)).Append("%,");
				}

				if (Tags.Count == 1)
				{
					generatedGradient.Append(Tags[0].Name).Append(" 0.1%,");
				}

				generatedGradient.Length--;
				generatedGradient.Append(");");
				if (orientation == BMBoardSectionOrientation.Horizontal)
				{
					return $"width: {totalDistance}px;"
							+ (NoResString)"margin-left:0px;"
							+ (NoResString)"box-sizing: border-box;"
							+ $"height: {height}px;"
							+ $"background:linear-gradient(to right,{generatedGradient}";
				}

				return $"width: {Width}px;"
						+ (NoResString)"margin-bottom:-1px;"
						+ (NoResString)"box-sizing: border-box;"
						+ $"height: {totalDistance}px;"
						+ $"background:linear-gradient(to bottom,{generatedGradient}";
			}

			return string.Empty;
		}
	}
}
