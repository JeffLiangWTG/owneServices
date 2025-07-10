using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class AddressSuburbTest<TBusinessObject, TValueObject> : TestCaseWithFactory
		where TBusinessObject : BusinessObject
		where TValueObject : Xsd.Consol
	{
		public void TestPopulateAddressDetailsWithValidSuburb()
		{
			ZString addressLine2 = "AddressLine2";
			ZString cityOrSuburb = "CityOrSuburb";

			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "AddressLine2", Address2Property.Value);
			AssertEquals("CityOrSuburb", "CityOrSuburb", SuburbProperty.Value);
		}

		public void TestPopulateAddressDetailsWithOversizeSuburb()
		{
			ZString addressLine2 = "BANKSMEADOW NSW 2019";
			ZString cityOrSuburb = "P.O. BOX 477 MASCOT NSW 2020 Australia planet Earth SL";

			NotificationBuffer notifier = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifier);
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "BANKSMEADOW NSW 2019 P.O.", Address2Property.Value);
			AssertEquals("CityOrSuburb", "BOX 477 MASCOT NSW 2020 Australia planet Earth SL", SuburbProperty.Value);
			AssertEquals("Should not have any notifications", 0, notifier.Events.Length);
		}

		public void TestPopulateAddressDetailsWithOversizeSingleWordSuburb()
		{
			ZString addressLine2 = "1 Bangkok Place";
			ZString cityOrSuburb = "Krungthepmahanakonbowornratanakosinblahblahblahblahblah";

			NotificationBuffer notifier = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifier);
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "1 Bangkok Place Krung", Address2Property.Value);
			AssertEquals("CityOrSuburb", "thepmahanakonbowornratanakosinblahblahblahblahblah", SuburbProperty.Value);
			AssertEquals("Should not have any notifications", 0, notifier.Events.Length);
		}

		public void TestPopulateAddressDetailsWithOversizeSuburbAndAddress()
		{
			ZString addressLine2 = "Level 3, Siam House, 1 Bangkok Place, Bangkok";
			ZString cityOrSuburb = "Krungthepmahanakonbowornratanakosinamurabillabonga blah";

			NotificationBuffer notifier = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifier);
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "Level 3, Siam House, 1 Bangkok Place, Bangkok", Address2Property.Value);
			AssertEquals("CityOrSuburb", "Krungthepmahanakonbowornratanakosinamurabillabonga", SuburbProperty.Value);
			AssertEquals("Should have a notification", 1, notifier.Events.Length);
			AssertEquals("Warning Message", "Warning: Maximum length of this field has been exceeded (value=Krungthepmahanakonbowornratanakosinamurabillabonga blah)",
				notifier.Events[0].Message);
		}

		public void TestPopulateAddressDetailsWithOversizeSuburbNoFindableSpace()
		{
			ZString addressLine2 = "Level 3, Siam centre, 1 Bangkok Place, Bangkok";
			ZString cityOrSuburb = "Krungthepmahanakonbowornratanakosinamurabillabongabla";

			NotificationBuffer notifier = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifier);
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "Level 3, Siam centre, 1 Bangkok Place, Bangkok Kru", Address2Property.Value);
			AssertEquals("CityOrSuburb", "ngthepmahanakonbowornratanakosinamurabillabongabla", SuburbProperty.Value);
			AssertEquals("Should have a notification", 0, notifier.Events.Length);
		}

		public void TestPopulateAddressDetailsWithOversizeSuburbAndAddress2()
		{
			ZString addressLine2 = "Level 13, Siam centre, 1 Bangkok Place, Bangkok";
			ZString cityOrSuburb = "Krungthepmahanakonbowornratanakosinamurabillabonga blah";

			NotificationBuffer notifier = new NotificationBuffer();
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, notifier);
			PopulateAddressAndSuburb(GetNewCargoDataAdapter(), Address2Property, SuburbProperty, addressLine2, cityOrSuburb, importContext);

			AssertEquals("AddressLine2", "Level 13, Siam centre, 1 Bangkok Place, Bangkok", Address2Property.Value);
			AssertEquals("CityOrSuburb", "Krungthepmahanakonbowornratanakosinamurabillabonga", SuburbProperty.Value);
			AssertEquals("Should have a notification", 1, notifier.Events.Length);
			AssertEquals("Warning Message", "Warning: Maximum length of this field has been exceeded (value=Krungthepmahanakonbowornratanakosinamurabillabonga blah)",
				notifier.Events[0].Message);
		}

		BusinessObject addressBizO;
		protected BusinessObject AddressBusinessObject => addressBizO ?? (addressBizO = Factory.New(AddressBusinessObjectType));

		protected void PopulateAddressAndSuburb(BaseCargoXmlDataAdapter<TBusinessObject, TValueObject> adapter, ZPropertyInfo address2Property, ZPropertyInfo suburbProperty,
			ZString address2, ZString suburb, ValueObjectImportContext context)
		{
			adapter.PopulateAddress2SuburbDetails(address2Property, suburbProperty, address2, suburb, context);
		}

		protected abstract Type AddressBusinessObjectType { get; }

		protected abstract ZPropertyInfo Address2Property { get; }

		protected abstract ZPropertyInfo SuburbProperty { get; }

		protected abstract BaseCargoXmlDataAdapter<TBusinessObject, TValueObject> GetNewCargoDataAdapter();
	}
}
