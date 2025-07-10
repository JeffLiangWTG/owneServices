using System;
using System.Drawing;
using System.Globalization;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Business
{
	public enum AcceptabilityStatusPolarity
	{
		Middle = 0,
		High,
		Low,
	}

	public static class ComponentAcceptabilityStatusExtensions
	{
		public static string GetHeadlineText(this ComponentAcceptabilityStatus status)
		{
			return Res.GetString("54c35cd0-b829-4188-8c01-05b88c384074", "Status: {0}", status.GetStatusText());
		}

		public static string GetStatusText(this ComponentAcceptabilityStatus status)
		{
			switch (status)
			{
				case ComponentAcceptabilityStatus.HighRisk:
					return Res.GetString("ef01f621-6226-4bf1-a4a5-6e5233dc5e40", "High Risk");

				case ComponentAcceptabilityStatus.Caution:
					return Res.GetString("6e6561e7-d996-4a48-a9df-6b767aaadaac", "Caution");

				case ComponentAcceptabilityStatus.Good:
					return Res.GetString("8038c2e4-a2f5-4722-b528-402e22cc7468", "Good");

				case ComponentAcceptabilityStatus.Excellent:
					return Res.GetString("3976e95f-6a95-490d-aeec-b02de5675205", "Excellent");

				case ComponentAcceptabilityStatus.None:
				case ComponentAcceptabilityStatus.Timeout:
					return Res.GetString("b2d3a39c-a9cb-4af9-99f9-b619f8956c3f", "None");

				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Status {0} is not supported", status), nameof(status));
			}
		}

		public static Color GetBackgroundColor(this ComponentAcceptabilityStatus status)
		{
			switch (status)
			{
				case ComponentAcceptabilityStatus.Excellent:
					return BMConstants.ExcellentBoardColor;

				case ComponentAcceptabilityStatus.Good:
					return BMConstants.GoodBoardColor;

				case ComponentAcceptabilityStatus.Caution:
					return BMConstants.CautionBoardColor;

				case ComponentAcceptabilityStatus.HighRisk:
					return BMConstants.HighRiskBoardColor;

				default:
					return Color.Transparent;
			}
		}

		public static Color GetForegroundColor(this ComponentAcceptabilityStatus status)
		{
			switch (status)
			{
				case ComponentAcceptabilityStatus.Excellent:
				case ComponentAcceptabilityStatus.Caution:
				case ComponentAcceptabilityStatus.None:
					return Color.Black;

				case ComponentAcceptabilityStatus.Good:
				case ComponentAcceptabilityStatus.HighRisk:
					return Color.White;

				default:
					return Color.Transparent;
			}
		}
	}
}
