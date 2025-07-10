using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing.CusGuarantee
{
	class CusGuaranteeHeaderTypeDeciderTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestTypeOfBizOForFR()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "FR guarantee should be of Customs.FR.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForDE()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.DE.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "DE guarantee should be of Customs.DE.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForBE()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.BE.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "BE guarantee should be of Customs.BE.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForGB()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "GB guarantee should be of Customs.EU.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForIT()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.IT.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "IT guarantee should be of Customs.IT.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForTR()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Constants.CountryCodes.Turkey;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.TR.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "TR guarantee should be of Customs.TR.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForES()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "ES guarantee should be of Customs.ES.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForNL()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.NL.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "NL guarantee should be of Customs.NL.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForPL()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "PL guarantee should be of Customs.PL.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestTypeOfBizOForIE()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
			Factory.Save();

			var loadedGuaranteeHeader = NewFactory().Load<CusGuaranteeHeader>(guaranteeHeader.PK);
			var expectedType = ObjectFactory.GetType<Integration.Customs.IE.ICusGuaranteeHeader>();

			NUnit.Framework.Assert.That(loadedGuaranteeHeader, NUnit.Framework.Is.TypeOf(expectedType), "IE guarantee should be of Customs.IE.Business.CusGuaranteeHeader type");
		}

		[ExpectNoExceptions]
		public void TestGetTypeForNew()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>()), "New GB guarantee should be of EU GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>()), "New LV guarantee should be of EU GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeHeader>()), "New FR guarantee should be of FR GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.NL.ICusGuaranteeHeader>()), "New NL guarantee should be of NL GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeHeader>()), "New PL guarantee should be of PL GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.IT.ICusGuaranteeHeader>()), "New IT guarantee should be of IT GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForNew(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeHeader>()), "New ES guarantee should be of ES GuaranteeHeader type");
			}
		}

		[ExpectNoExceptions]
		public void TestGetTypeForBinding()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedKingdom))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>()), "New GB guarantee should be of EU GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Latvia))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.EU.ICusGuaranteeHeader>()), "New LV guarantee should be of EU GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.FR.ICusGuaranteeHeader>()), "New FR guarantee should be of FR GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.NL.ICusGuaranteeHeader>()), "New NL guarantee should be of NL GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Poland))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.PL.ICusGuaranteeHeader>()), "New PL guarantee should be of PL GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.IT.ICusGuaranteeHeader>()), "New IT guarantee should be of IT GuaranteeHeader type");
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				NUnit.Framework.Assert.That(new CusGuaranteeHeaderTypeDecider().GetTypeForBinding(), NUnit.Framework.Is.EqualTo(ObjectFactory.GetType<Integration.Customs.ES.ICusGuaranteeHeader>()), "New ES guarantee should be of ES GuaranteeHeader type");
			}
		}
	}
}
