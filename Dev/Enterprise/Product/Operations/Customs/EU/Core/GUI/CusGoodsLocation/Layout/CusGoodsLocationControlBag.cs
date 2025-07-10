using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class CusGoodsLocationControlBag : ControlBag
	{
		public CusGoodsLocationControlBag()
		{
			QualifierDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.QualifierDropEdit));
			TypeDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.TypeDropEdit));
			AdditionalIdentifierTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.AdditionalIdentifierTextBox));
			PostcodeTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.PostcodeTextBox));
			CountryCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.CountryCodeFindBox));
			UnlocoCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.UnlocoCodeFindBox));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.CustomsOfficeCodeFindBox));
			GeoLocationLatitudeTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.GeoLocationLatitudeTextBox));
			GeoLocationLongitudeTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.GeoLocationLongitudeTextBox));
			OrganisationFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.OrganisationFindBox));
			EoriNumberTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.EoriNumberTextBox));
			AuthorizationCodeFindBox = RegisterControl(nameof(CusGoodsLocationUserControl.AuthorizationCodeFindBox));
			AuthorizationDropEdit = RegisterControl(nameof(CusGoodsLocationUserControl.AuthorizationDropEdit));
			StreetAndNumberTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.StreetAndNumberTextBox));
			StreetAndNumberWithAddressValidationUserControl = RegisterControl(nameof(CusGoodsLocationUserControl.StreetAndNumberWithAddressValidationUserControl));
			CityTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.CityTextBox));
			ContactTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.ContactTextBox));
			PhoneTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.PhoneTextBox));
			EmailTextBox = RegisterControl(nameof(CusGoodsLocationUserControl.EmailTextBox));
		}

		public static CusGoodsLocationControlBag Instance => instance ?? (instance = new CusGoodsLocationControlBag());

		[ThreadStatic]
		static CusGoodsLocationControlBag instance;

		public ControlReference QualifierDropEdit { get; }

		public ControlReference TypeDropEdit { get; }

		public ControlReference AdditionalIdentifierTextBox { get; }

		public ControlReference PostcodeTextBox { get; }

		public ControlReference CountryCodeFindBox { get; }

		public ControlReference UnlocoCodeFindBox { get; }

		public ControlReference CustomsOfficeCodeFindBox { get; }

		public ControlReference GeoLocationLatitudeTextBox { get; }

		public ControlReference GeoLocationLongitudeTextBox { get; }

		public ControlReference OrganisationFindBox { get; }

		public ControlReference EoriNumberTextBox { get; }

		public ControlReference AuthorizationCodeFindBox { get; }

		public ControlReference AuthorizationDropEdit { get; }

		public ControlReference StreetAndNumberTextBox { get; }

		public ControlReference StreetAndNumberWithAddressValidationUserControl { get; }

		public ControlReference CityTextBox { get; }

		public ControlReference ContactTextBox { get; }

		public ControlReference PhoneTextBox { get; }

		public ControlReference EmailTextBox { get; }

		protected override Control CreateTemplate() => new CusGoodsLocationUserControl();
	}
}
