using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin.Testing
{
	class CustomsEndorsementWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When declaration parameter is null", () => new CustomsEndorsementWrapper(declaration: null));
		}

		[ExpectNoExceptions]
		public void TestForm()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Form, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Form");
		}

		[ExpectNoExceptions]
		public void TestFormNo()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.FormNo, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "FormNo");
		}

		[ExpectNoExceptions]
		public void TestDate()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Date, NUnit.Framework.Is.EqualTo(ZDate.Empty), "Date");
		}

		[ExpectNoExceptions]
		public void TestReferenceDate()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.ReferenceDate, NUnit.Framework.Is.EqualTo(ZDate.Empty), "ReferenceDate");
		}

		[ExpectNoExceptions]
		public void TestCustomsOffice()
		{
			SetUpCustomsOffice();

			declaration.JE_CustomsOffice = "";
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "When JE_CustomsOffice is empty, CustomsOffice");

			declaration.JE_CustomsOffice = "ABCDEFGH";
			wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "When JE_CustomsOffice does not exist, CustomsOffice");

			declaration.JE_CustomsOffice = "IT000000";
			wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.CustomsOffice, NUnit.Framework.Is.EqualTo("Campodoro").Using(CustomComparers.TypeComparison), "When JE_CustomsOffice exists, CustomsOffice");
		}

		[ExpectNoExceptions]
		public void TestIssuingCountry()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.IssuingCountry, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "IssuingCountry");
		}

		[ExpectNoExceptions]
		public void TestPlace()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.Place, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "Place");
		}

		[ExpectNoExceptions]
		public void TestEntryNumber()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "no entry => EntryNumber empty");

			declaration.CustomsEntryHeaders.AddNew();
			wrapper = new CustomsEndorsementWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.EntryNumber, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "entry with no entrynumber => EntryNumber empty");

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.EntryNumber = "entrynum";

			wrapper = new CustomsEndorsementWrapper(declaration);
			NUnit.Framework.Assert.That(wrapper.EntryNumber, NUnit.Framework.Is.EqualTo("entrynum").Using(CustomComparers.TypeComparison), "entry with an entrynumber => EntryNumber ok");
		}

		[ExpectNoExceptions]
		public void TestEUR1Pg1Box11TextEntryNumber()
		{
			var wrapper = GetNewWrapper();
			NUnit.Framework.Assert.That(wrapper.EUR1Pg1Box11TextEntryNumber, NUnit.Framework.Is.EqualTo("No. of").Using(CustomComparers.TypeComparison), "No. of");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;

		ICustomsEndorsement GetNewWrapper() => new CustomsEndorsementWrapper(declaration);

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT000000", "Campodoro", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestShowEntryNumberFromCustomsEndorsement()
		{
			var declaration = Factory.New<JobDeclaration>();
			IEURCertificateOfOrigin dataProvider = new EURCertificateOfOriginWrapper(declaration);
			NUnit.Framework.Assert.That(!dataProvider.CustomsEndorsement.ShowEntryNumber, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "ShowEntryNumber should be false in EU");
		}
	}
}
