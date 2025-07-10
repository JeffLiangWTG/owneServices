using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = Enterprise.DocumentVisualizer.Business.Res;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public sealed class DocumentSettings : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Properties

		[CargoWiseOne.ResourceStrings.ResourceStringData("a102fe0a-a9df-4a1a-9c42-88b2e2b48ce5", Caption = "Zoom")]
		public ZInt Zoom
		{
			get { return zoom; }
			set
			{
				if (SetNonPersistentPropertyValue(ZoomInfo, ref zoom, value))
				{
					if (!IsValidationSuspended)
					{
						ValidateZoomInfo();
					}
				}
			}
		}

		ZInt zoom = 100;

		public ZPropertyInfo ZoomInfo => base.GetZPropertyInfo(nameof(Zoom));

		void ValidateZoomInfo()
		{
			ZoomInfo.ClearAllNotifications();

			if (Zoom < 20)
			{
				ZoomInfo.AddError(Res.GetString("1484de98-1987-4f5c-8d74-5e9478a72f7e", "The minimum value of zoom is 20."));
			}
			else if (Zoom > 200)
			{
				ZoomInfo.AddError(Res.GetString("783144f1-ffed-4873-96d9-c524c5110d5c", "The maximum value of zoom is 200."));
			}
		}

		#endregion
	}
}
