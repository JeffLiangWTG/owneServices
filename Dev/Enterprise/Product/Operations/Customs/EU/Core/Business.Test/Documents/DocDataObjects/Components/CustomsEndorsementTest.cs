using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(CustomsEndorsement))]
	class CustomsEndorsementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Should be exception when dataProvider parameter is null", () => new CustomsEndorsement(customsEndorsement: null, factory: null, maxLengthInfo: null));
		}

		[ExpectNoExceptions]
		public void TestAllFieldsAreAlterable()
		{
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.Form));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.FormNo));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZDate.Empty, nameof(customsEndorsement.OfDate));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.CustomsOffice));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.IssuingCountry));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.Place));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZDate.Empty, nameof(customsEndorsement.ReferenceDate));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.EntryNumber));
			DocDataObjectTestUtility.AssertDataObjectPropertyIsAlterable(customsEndorsement, ZString.Empty, nameof(customsEndorsement.EUR1Pg1Box11TextEntryNumber));
		}

		[ExpectNoExceptions]
		public void TestForm()
		{
			dataProviderMock.Setup(x => x.Form).Returns("form");
			NUnit.Framework.Assert.That(customsEndorsement.Form, NUnit.Framework.Is.EqualTo("FORM").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.Form));
		}

		[ExpectNoExceptions]
		public void TestFormNo()
		{
			dataProviderMock.Setup(x => x.FormNo).Returns("formno");
			NUnit.Framework.Assert.That(customsEndorsement.FormNo, NUnit.Framework.Is.EqualTo("FORMNO").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.FormNo));
		}

		[ExpectNoExceptions]
		public void TestOfDate()
		{
			dataProviderMock.Setup(x => x.Date).Returns(new ZDate(2022, 01, 01));
			NUnit.Framework.Assert.That(customsEndorsement.OfDate, NUnit.Framework.Is.EqualTo(new ZDate(2022, 01, 01)), nameof(customsEndorsement.OfDate));
		}

		[ExpectNoExceptions]
		public void TestReferenceDate()
		{
			dataProviderMock.Setup(x => x.ReferenceDate).Returns(new ZDate(2022, 01, 01));
			NUnit.Framework.Assert.That(customsEndorsement.ReferenceDate, NUnit.Framework.Is.EqualTo(new ZDate(2022, 01, 01)), nameof(customsEndorsement.ReferenceDate));
		}

		[ExpectNoExceptions]
		public void TestCustomsOffice()
		{
			dataProviderMock.Setup(x => x.CustomsOffice).Returns("cusoff");
			NUnit.Framework.Assert.That(customsEndorsement.CustomsOffice, NUnit.Framework.Is.EqualTo("CUSOFF").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.CustomsOffice));
		}

		[ExpectNoExceptions]
		public void TestIssuingCountry()
		{
			dataProviderMock.Setup(x => x.IssuingCountry).Returns("isscou");
			NUnit.Framework.Assert.That(customsEndorsement.IssuingCountry, NUnit.Framework.Is.EqualTo("ISSCOU").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.IssuingCountry));
		}

		[ExpectNoExceptions]
		public void TestPlace()
		{
			dataProviderMock.Setup(x => x.Place).Returns("place");
			NUnit.Framework.Assert.That(customsEndorsement.Place, NUnit.Framework.Is.EqualTo("PLACE").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.Place));
		}

		[ExpectNoExceptions]
		public void TestEntryNumber()
		{
			dataProviderMock.Setup(x => x.EntryNumber).Returns("entryNumber");
			NUnit.Framework.Assert.That(customsEndorsement.EntryNumber, NUnit.Framework.Is.EqualTo("entryNumber").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.EntryNumber));
		}

		[ExpectNoExceptions]
		public void TestEUR1Pg1Box11TextEntryNumber()
		{
			dataProviderMock.Setup(x => x.EUR1Pg1Box11TextEntryNumber).Returns("No. of");
			NUnit.Framework.Assert.That(customsEndorsement.EUR1Pg1Box11TextEntryNumber, NUnit.Framework.Is.EqualTo("No. of").Using(CustomComparers.TypeComparison), nameof(customsEndorsement.EUR1Pg1Box11TextEntryNumber));
		}

		[ExpectNoExceptions]
		public void TestShowEntryNumber()
		{
			dataProviderMock.Setup(x => x.ShowEntryNumber).Returns(false);
			NUnit.Framework.Assert.That(customsEndorsement.ShowEntryNumber, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), nameof(customsEndorsement.ShowEntryNumber));
		}

		protected override BusinessObject GetNewBusinessObject() => new CustomsEndorsement(new Mock<ICustomsEndorsement>().Object, factory: null, maxLengthInfo: null);

		protected override void SetUp()
		{
			base.SetUp();
			dataProviderMock = new Mock<ICustomsEndorsement>();
			customsEndorsement = new CustomsEndorsement(dataProviderMock.Object, factory: null, maxLengthInfo: null);
		}

		Mock<ICustomsEndorsement> dataProviderMock;
		CustomsEndorsement customsEndorsement;
	}
}
