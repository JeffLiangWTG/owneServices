using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ScreeningMethodCollection : CusSupportingInfoCollection<ScreeningMethod>
	{
		public ScreeningMethodCollection(BusinessObject parent)
			: base(parent, Common.EU.CusSupportingInfoTypeList.Codes.ScreeningMethod)
		{
			MaxCountValidationEnable(9);
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			var firstScreeningMethod = this.FirstOrDefault();
			if (Count == 1)
			{
				firstScreeningMethod.AddRowMessageError(ErrorMessage);
			}
			else if (firstScreeningMethod != null && firstScreeningMethod.RowNotifications.Any(n => n.Message == ErrorMessage))
			{
				firstScreeningMethod.RemoveRowMessageError(ErrorMessage);
			}
		}

		string ErrorMessage => Res.GetString("76F517F9-D9A1-4E05-A5AB-0749CAD430FE", "Please add at lest 2 Screen Methods");
	}
}
