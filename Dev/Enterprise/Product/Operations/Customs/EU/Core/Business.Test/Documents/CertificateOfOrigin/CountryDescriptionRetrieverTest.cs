using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class CountryDescriptionRetrieverTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected when parameter factory is null", () => new CountryDescriptionRetriever("IT", null));
		}

		[ExpectNoExceptions]
		public void TestRetrieveCountryDescription()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var retriever = new CountryDescriptionRetriever("LV", entryHeader.Factory);
			NUnit.Framework.Assert.That(retriever.RetrieveCountryDescription(), NUnit.Framework.Is.EqualTo("Latvia").Using(CustomComparers.TypeComparison), "Latvia is expected");
		}

		[ExpectNoExceptions]
		public void TestRetrieveCountryDescriptionFromEmpyCode()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var retriever = new CountryDescriptionRetriever("", entryHeader.Factory);
			NUnit.Framework.Assert.That(retriever.RetrieveCountryDescription(), NUnit.Framework.Is.EqualTo(ZString.Empty), "When countryCode is empty, empty string is expected");
		}
	}
}
