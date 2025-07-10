using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CusGoodsLocationControlBag))]
	sealed class CusGoodsLocationControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CusGoodsLocationControlBag.QualifierDropEdit);
				yield return nameof(CusGoodsLocationControlBag.TypeDropEdit);
				yield return nameof(CusGoodsLocationControlBag.AdditionalIdentifierTextBox);
				yield return nameof(CusGoodsLocationControlBag.PostcodeTextBox);
				yield return nameof(CusGoodsLocationControlBag.CountryCodeFindBox);
				yield return nameof(CusGoodsLocationControlBag.UnlocoCodeFindBox);
				yield return nameof(CusGoodsLocationControlBag.CustomsOfficeCodeFindBox);
				yield return nameof(CusGoodsLocationControlBag.GeoLocationLatitudeTextBox);
				yield return nameof(CusGoodsLocationControlBag.GeoLocationLongitudeTextBox);
				yield return nameof(CusGoodsLocationControlBag.OrganisationFindBox);
				yield return nameof(CusGoodsLocationControlBag.EoriNumberTextBox);
				yield return nameof(CusGoodsLocationControlBag.AuthorizationCodeFindBox);
				yield return nameof(CusGoodsLocationControlBag.AuthorizationDropEdit);
				yield return nameof(CusGoodsLocationControlBag.StreetAndNumberTextBox);
				yield return nameof(CusGoodsLocationControlBag.StreetAndNumberWithAddressValidationUserControl);
				yield return nameof(CusGoodsLocationControlBag.CityTextBox);
				yield return nameof(CusGoodsLocationControlBag.ContactTextBox);
				yield return nameof(CusGoodsLocationControlBag.PhoneTextBox);
				yield return nameof(CusGoodsLocationControlBag.EmailTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CusGoodsLocationControlBag.Instance;
	}
}
