using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientNameAndAddressFollowingByContactName))]
	sealed class RecipientNameAndAddressFollowingByContactNameTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientNameAndAddressFollowingByContactName();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient name and address following by contact name>", ValueProviderToTest.IsResponsibleForReplacing("<recipient name and address following by contact name>", Passes.FirstPass));
			Assert("Should replace <Recipientnameandaddressfollowingbycontactname>", ValueProviderToTest.IsResponsibleForReplacing("<Recipientnameandaddressfollowingbycontactname>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Mia Wallace", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();

			DocDeliveryContact fred = new DocDeliveryContact(new BusinessObjectFactory());
			fred.Name = "Fred";
			fred.CompanyName = "Foo Corp";
			fred.Address1 = "Addr1";
			fred.Address2 = "Addr2";
			fred.City = "Sydney";
			fred.PostCode = "2000";
			fred.State = "NSW";
			fred.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).MostOfficialContact = fred;

			DocDeliveryContact bob = new DocDeliveryContact(new BusinessObjectFactory());
			bob.Name = "Bob";
			bob.CompanyName = "Boo Corp";
			bob.Address1 = "Addr1";
			bob.Address2 = "Addr2";
			bob.City = "Sydney";
			bob.PostCode = "2000";
			bob.State = "NSW";
			bob.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = bob;

			Report.TypeOfContact = ContactType.Consignee;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.Consignor;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.NotifyParty;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.Payables;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.TransportServices;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
		}

		public void TestReplacementWhenNull()
		{
			// When original contact is null, the delivery contact should be displayed
			PrepareRenderer();
			var bob = new DocDeliveryContact(new BusinessObjectFactory());
			bob.Name = "Bob";
			bob.CompanyName = "Boo Corp";
			bob.Address1 = "Addr1";
			bob.Address2 = "Addr2";
			bob.City = "Sydney";
			bob.PostCode = "2000";
			bob.State = "NSW";
			bob.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = bob;

			Report.TypeOfContact = ContactType.Consignee;
			AssertEquals("Boo Corp\nAddr1\nAddr2\nSydney NSW 2000\nATTENTION: Bob".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>() { typeof(RecipientNameAndAddressFollowingByContactName).GetField("factory", BindingFlags.Instance | BindingFlags.NonPublic) };
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			var bob = new DocDeliveryContact(new BusinessObjectFactory());
			bob.Name = "John Doe";
			bob.CompanyName = "WiseTech Global";
			bob.Address1 = "Address line 1";
			bob.Address2 = "Address line 2";
			bob.City = "Sydney";
			bob.PostCode = "2000";
			bob.State = "NSW";
			bob.UNLOCO = (RefUNLOCO)new BusinessObjectFactory().Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = bob;
			Report.TypeOfContact = ContactType.Consignee;
		}
	}
}
