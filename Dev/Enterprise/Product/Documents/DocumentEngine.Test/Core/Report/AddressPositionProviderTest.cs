using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using ZConstants = Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class AddressPositionProviderTest : TestCaseWithFactory
	{
		public void TestGetAddressPosition()
		{
			var addressPositionProvider = new AddressPositionProvider();

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Australia);
			AssertEquals("GetAddressPosition Australia", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Cameroon);
			AssertEquals("GetAddressPosition Cameroon", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.CoteDivoire);
			AssertEquals("GetAddressPosition CoteDivoire", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Mozambique);
			AssertEquals("GetAddressPosition Mozambique", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Senegal);
			AssertEquals("GetAddressPosition Senegal", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.DominicanRepublic);
			AssertEquals("GetAddressPosition DominicanRepublic", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Yemen);
			AssertEquals("GetAddressPosition Yemen", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Somalia);
			AssertEquals("GetAddressPosition Somalia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Italy);
			AssertEquals("GetAddressPosition Italy", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.NewZealand);
			AssertEquals("GetAddressPosition NewZealand", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Belgium);
			AssertEquals("GetAddressPosition Belgium", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.France);
			AssertEquals("GetAddressPosition France", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Spain);
			AssertEquals("GetAddressPosition Spain", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Greece);
			AssertEquals("GetAddressPosition Greece", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Luxembourg);
			AssertEquals("GetAddressPosition Luxembourg", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Denmark);
			AssertEquals("GetAddressPosition Denmark", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Israel);
			AssertEquals("GetAddressPosition Israel", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Romania);
			AssertEquals("GetAddressPosition Romania", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Finland);
			AssertEquals("GetAddressPosition Finland", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Jordan);
			AssertEquals("GetAddressPosition Jordan", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Macau);
			AssertEquals("GetAddressPosition Macau", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Bahrain);
			AssertEquals("GetAddressPosition Bahrain", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Lebanon);
			AssertEquals("GetAddressPosition Lebanon", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Algeria);
			AssertEquals("GetAddressPosition Algeria", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.PuertoRico);
			AssertEquals("GetAddressPosition PuertoRico", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Malawi);
			AssertEquals("GetAddressPosition Malawi", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Niger);
			AssertEquals("GetAddressPosition Niger", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Palau);
			AssertEquals("GetAddressPosition Palau", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Cuba);
			AssertEquals("GetAddressPosition Cuba", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Belarus);
			AssertEquals("GetAddressPosition Belarus", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Kiribati);
			AssertEquals("GetAddressPosition Belarus", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Haiti);
			AssertEquals("GetAddressPosition Haiti", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Curacao);
			AssertEquals("GetAddressPosition Curacao", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Jamaica);
			AssertEquals("GetAddressPosition Jamaica", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.TrinidadAndTobago);
			AssertEquals("GetAddressPosition TrinidadAndTobago", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Togo);
			AssertEquals("GetAddressPosition Togo", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Macedonia);
			AssertEquals("GetAddressPosition Macedonia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Croatia);
			AssertEquals("GetAddressPosition Croatia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Barbados);
			AssertEquals("GetAddressPosition Barbados", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Serbia);
			AssertEquals("GetAddressPosition Serbia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Kosovo);
			AssertEquals("GetAddressPosition Kosovo", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Iran);
			AssertEquals("GetAddressPosition Iran", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.AmericanSamoa);
			AssertEquals("GetAddressPosition American Samoa", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Georgia);
			AssertEquals("GetAddressPosition Georgia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.NewCaledonia);
			AssertEquals("GetAddressPosition New Caledonia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Chad);
			AssertEquals("GetAddressPosition Chad", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.LaoPeoplesDemocraticRepublic);
			AssertEquals("GetAddressPosition LaoPeoplesDemocraticRepublic", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.BosniaAndHerzegovina);
			AssertEquals("GetAddressPosition BosniaAndHerzegovina", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.StPierreEtMiquelon);
			AssertEquals("GetAddressPosition Chad", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());
		}

		public void TestOverrideOverrides()
		{
			var addressPositionProvider = new AddressPositionProvider();

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Australia);

			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AddressPositionList.Codes.Right);
			AssertEquals("GetAddressPosition Australia", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());
			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AddressPositionList.Codes.Left);
			AssertEquals("GetAddressPosition Australia", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());
			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals("GetAddressPosition Australia", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Italy);
			AssertEquals("GetAddressPosition Italy", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AddressPositionList.Codes.Right);
			AssertEquals("GetAddressPosition Italy", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());
			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AddressPositionList.Codes.Left);
			AssertEquals("GetAddressPosition Italy", AddressPositionList.Codes.Left, addressPositionProvider.GetAddressPosition());
			DocumentsDataRegistry.Instance.AddressPosition.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			AssertEquals("GetAddressPosition Italy", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());

			GlbCompany.CurrentCompany.SetCountry(ZConstants.CountryCodes.Nepal);
			AssertEquals("GetAddressPosition Nepal", AddressPositionList.Codes.Right, addressPositionProvider.GetAddressPosition());
		}
	}
}
