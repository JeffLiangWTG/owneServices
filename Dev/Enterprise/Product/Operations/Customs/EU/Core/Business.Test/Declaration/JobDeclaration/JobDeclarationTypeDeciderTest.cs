using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobDeclarationTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBindingForNonEUCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var typeDecider = new JobDeclarationTypeDecider();
				var expectedType = typeof(JobDeclaration);
				AssertEquals(expectedType, typeDecider.GetTypeForBinding());
			}
		}

		public void TestGetTypeForNewForNonEUCountry()
		{
			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns(Core.Constants.CountryCodes.Australia);

			var typeDecider = new JobDeclarationTypeDecider();
			var expectedType = typeof(JobDeclaration);
			AssertEquals(expectedType, typeDecider.GetTypeForNew(typeDeciderContextMock.Object));
		}

		public void TestTypeDecider_UnitedKingdom()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.GB.IJobDeclaration>(Core.Constants.CountryCodes.UnitedKingdom);
		}

		public void TestTypeDecider_Germany()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.DE.IJobDeclaration>(Core.Constants.CountryCodes.Germany);
		}

		public void TestTypeDecider_Italy()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.IT.IJobDeclaration>(Core.Constants.CountryCodes.Italy);
		}

		public void TestTypeDecider_France()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.FR.IJobDeclaration>(Core.Constants.CountryCodes.France);
		}
		public void TestTypeDecider_Spain()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.ES.IJobDeclaration>(Core.Constants.CountryCodes.Spain);
		}

		public void TestTypeDecider_Poland()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.PL.IJobDeclaration>(Core.Constants.CountryCodes.Poland);
		}

		public void TestTypeDecider_Ireland()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.IE.IJobDeclaration>(Core.Constants.CountryCodes.Ireland);
		}

		public void TestTypeDecider_Belgium()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.BE.IJobDeclaration>(Core.Constants.CountryCodes.Belgium);
		}

		public void TestTypeDecider_Turkey()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.TR.IJobDeclaration>(Core.Constants.CountryCodes.Turkey);
		}

		public void TestTypeDecider_Netherlands()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.NL.IJobDeclaration>(Core.Constants.CountryCodes.Netherlands);
		}

		public void TestTypeDecider_Sweden()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.SE.IJobDeclaration>(Core.Constants.CountryCodes.Sweden);
		}

		public void TestTypeDecider_Denmark()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.DK.IJobDeclaration>(Core.Constants.CountryCodes.Denmark);
		}

		public void TestTypeDecider_Finland()
		{
			AssertNoExceptionWhenLoading<Integration.Customs.FI.IJobDeclaration>(Core.Constants.CountryCodes.Finland);
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.JobDeclaration", jobDeclaration.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			jobDeclaration = Factory.New<JobDeclaration>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.Declaration.JobDeclaration", jobDeclaration.GetType().FullName);
		}

		void AssertNoExceptionWhenLoading<T>(string country)
			where T : class
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var jobDeclaration = Factory.New<JobDeclaration>();
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertNoExceptionThrown($"The reload as {jobDeclaration.CountryCode}.CEH of an object previously loaded and cached as EU.CEH did not explode", () =>
				{
					newFactory.Load<JobDeclaration>(jobDeclaration.PK);
					newFactory.Load<T>(jobDeclaration.PK);
				});
			}
		}
	}
}
