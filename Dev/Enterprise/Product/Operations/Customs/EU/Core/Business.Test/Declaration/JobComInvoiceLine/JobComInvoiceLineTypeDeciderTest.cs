using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Moq;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class JobComInvoiceLineTypeDeciderTest : BaseJobComInvoiceLineTypeDeciderTest
	{
		public void TestGetTypeForLoad_EMCS()
		{
			var declaration = (Customs.Business.BaseJobDeclaration)Factory.New<Integration.Customs.EUEMCS.IJobDeclaration>();
			declaration.FillWithValidTestData();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			Factory.Save();

			var row = ((IBusinessObjectInternals)invoiceLine).Row;

			var typeDecider = new JobComInvoiceLineTypeDecider();

			var expectedType = ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceLine>();
			var actualType = typeDecider.GetTypeForLoad(row, NewFactory());

			AssertEquals("Should is EMCSJobComInvoiceLine as its parent header is EMCSJobComInvoiceHeader.", expectedType, actualType);
		}

		public void TestGetTypeForNew_ITypeDeciderContext()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine", jobComInvoiceLine.GetType().FullName);

			var typeDeciderContextMock = new Mock<ITypeDeciderContext>();
			typeDeciderContextMock.Setup(k => k.Country).Returns("DE");
			jobComInvoiceLine = Factory.New<JobComInvoiceLine>(typeDeciderContextMock.Object);
			AssertEquals("Enterprise.Customs.DE.Business.Declaration.JobComInvoiceLine", jobComInvoiceLine.GetType().FullName);
		}

		protected override Type BaseTypeDecidedType => typeof(JobComInvoiceLine);

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			return new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryGuids.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceLine>() }
			};
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			return new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Italy, ObjectFactory.GetType<Integration.Customs.IT.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Spain, ObjectFactory.GetType<Integration.Customs.ES.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Poland, ObjectFactory.GetType<Integration.Customs.PL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Belgium, ObjectFactory.GetType<Integration.Customs.BE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Turkey, ObjectFactory.GetType<Integration.Customs.TR.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Netherlands, ObjectFactory.GetType<Integration.Customs.NL.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Sweden, ObjectFactory.GetType<Integration.Customs.SE.IJobComInvoiceLine>() },
				{ Core.Constants.CountryCodes.Finland, ObjectFactory.GetType<Integration.Customs.FI.IJobComInvoiceLine>() }
			};
		}
	}
}
