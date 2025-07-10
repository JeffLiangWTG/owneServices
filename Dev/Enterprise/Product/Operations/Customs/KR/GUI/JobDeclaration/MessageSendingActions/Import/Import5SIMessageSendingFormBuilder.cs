using System.Linq;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public class Import5SIMessageSendingFormBuilder : ImportOriginalMessageSendingFormBuilder
	{
		public override ZUserControl GetUserControl() => null;

		public override ResourceStringData GetUserControlGroupBoxCaption() => Res.GetData("841A37B5-A557-482B-ABBA-A81998C47274", "Mail Items");

		protected override ZTextBoxColumnStyleInfo[] GetAdditionalColumnStyles()
		{
			var result = base.GetAdditionalColumnStyles().ToList();
			result.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(MailItemIDsMessageSendingObject.DeclarationCustomsOffice),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(MailItemIDsMessageSendingObject.DeclarationCustomsDivision),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				}
			});

			return result.ToArray();
		}
	}
}
