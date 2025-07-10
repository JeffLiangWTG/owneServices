using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(RecipientAddress))]
	sealed class RecipientAddressTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new RecipientAddress();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should replace <recipient address>", ValueProviderToTest.IsResponsibleForReplacing("<recipient address>", Passes.FirstPass));
			Assert("Should replace <RecipientAddress>", ValueProviderToTest.IsResponsibleForReplacing("<RecipientAddress>", Passes.FirstPass));
			Assert("Should not replace", !ValueProviderToTest.IsResponsibleForReplacing("Mia Wallace", Passes.FirstPass));

			Assert("Should replace <RecipientAddress(AddressOnly)>", ValueProviderToTest.IsResponsibleForReplacing("<RecipientAddress(AddressOnly)>", Passes.FirstPass));
			Assert("Should replace < recipientaddress ( addressonly ) >", ValueProviderToTest.IsResponsibleForReplacing("< recipientaddress ( addressonly ) >", Passes.FirstPass));
			Assert("Should not replace < recipientaddress ( addressonly2 ) >", !ValueProviderToTest.IsResponsibleForReplacing("< recipientaddress ( addressonly2 ) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			var fred = new DocDeliveryContact(Factory);
			fred.Name = "Fred";
			fred.CompanyName = "Foo Corp";
			fred.Address1 = "Addr1";
			fred.Address2 = "Addr2";
			fred.City = "Sydney";
			fred.PostCode = "2000";
			fred.State = "NSW";
			fred.UNLOCO = (RefUNLOCO)Factory.Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = fred;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));

			Report.TypeOfContact = ContactType.Consignor;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.NotifyParty;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.Payables;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));
			Report.TypeOfContact = ContactType.TransportServices;
			AssertEquals("Foo Corp\nAddr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("", Report));

			AssertEquals("Addr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("<RecipientAddress(AddressOnly)>", Report));
			AssertEquals("Addr1\nAddr2\nSydney NSW 2000".ToUpper(), ValueProviderToTest.GetReplacement("< recipientaddress ( addressonly ) >", Report));
		}

		public void TestReplacementWhenNull()
		{
			PrepareRenderer();
			AssertEquals("", ValueProviderToTest.GetReplacement("", Report));
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>() { typeof(RecipientAddress).GetField("factory", BindingFlags.Instance | BindingFlags.NonPublic) };
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.TypeOfContact = ContactType.Consignee;
			var fred = new DocDeliveryContact(Factory);
			fred.Name = "Fred";
			fred.CompanyName = "WiseTech Global";
			fred.Address1 = "Address line 1";
			fred.Address2 = "Address line 2";
			fred.City = "Sydney";
			fred.PostCode = "2000";
			fred.State = "NSW";
			fred.UNLOCO = (RefUNLOCO)Factory.Load(typeof(RefUNLOCO), new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))[0];
			((IReportForUnitTesting)Report).DeliveryContact = fred;
		}
	}
}
