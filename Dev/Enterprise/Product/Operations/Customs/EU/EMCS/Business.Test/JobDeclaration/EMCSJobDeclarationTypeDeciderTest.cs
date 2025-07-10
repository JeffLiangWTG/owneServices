using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	public class EMCSJobDeclarationTypeDeciderTest : CountrySpecificTypeDeciderTest
	{
		public void TestDataCanBeLoadedCorrectly()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();

			var invoiceGroup = declaration.TopGroupInvoice;
			var invoiceSubGroup = invoiceGroup.JobComInvoiceGroupHeaders.AddNew();
			var invoiceOfSubGroup = invoiceSubGroup.JobComInvoiceHeaders.AddNew();
			var invoiceLineOfSubGroup = (EMCSJobComInvoiceLine)invoiceOfSubGroup.JobComInvoiceLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var invoiceLineOfSubGroupOutturn = invoiceLineOfSubGroup.Outturn;
			var invoiceLineOutturn = invoiceLine.Outturn;

			var container = declaration.CusContainers.AddNew();

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertCorrectTypeIsLoaded<EMCSJobDeclaration>(declaration.PK, typeof(BaseJobDeclaration));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceGroupHeader>(invoiceGroup.PK, typeof(BaseJobComInvoiceGroupHeader));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceGroupHeader>(invoiceSubGroup.PK, typeof(BaseJobComInvoiceGroupHeader));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceHeader>(invoiceOfSubGroup.PK, typeof(BaseJobComInvoiceHeader));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceHeader>(invoice.PK, typeof(BaseJobComInvoiceHeader));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceLine>(invoiceLineOfSubGroup.PK, typeof(BaseJobComInvoiceLine));
				AssertCorrectTypeIsLoaded<EMCSJobComInvoiceLine>(invoiceLine.PK, typeof(BaseJobComInvoiceLine));
				AssertCorrectTypeIsLoaded<EMCSCusContainer>(container.PK, typeof(BaseCusContainer));
				AssertCorrectTypeIsLoaded<EMCSInvoiceLineCusOutturn>(invoiceLineOfSubGroupOutturn.PK, typeof(CusOutturn));
				AssertCorrectTypeIsLoaded<EMCSInvoiceLineCusOutturn>(invoiceLineOutturn.PK, typeof(CusOutturn));
			});
		}

		void AssertCorrectTypeIsLoaded<T>(ZGuid pk, params Type[] types) where T : BusinessObject
		{
			var expectedType = typeof(T);

			foreach (var type in types.Union(new[] { expectedType }))
			{
				var factory = new BusinessObjectFactory();
				var bizObj = factory.Load(type, pk);

				AssertType("Loading using " + type.FullName, expectedType, bizObj);
				AssertNoExceptionThrown(bizObj.LoadChildEditableObjects);
			}
		}

		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode) => SetCountryCode(countryCode);

		protected override BusinessObject GetNewBusinessObjectForLoadTest() => Factory.New<EMCSJobDeclaration>();

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad() => new Dictionary<ZGuid, Type>
		{
			{ Core.Constants.CountryGuids.Germany, ObjectFactory.GetType<Integration.Customs.DEEMCS.IEMCSJobDeclaration>() },
			{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobDeclaration>() },
			{ Core.Constants.CountryGuids.Ireland, ObjectFactory.GetType<Integration.Customs.IEEMCS.IEMCSJobDeclaration>() },
			{ Core.Constants.CountryGuids.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GBEMCS.IEMCSJobDeclaration>() }
		};

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests() => new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.Germany, ObjectFactory.GetType<Integration.Customs.DEEMCS.IEMCSJobDeclaration>() },
			{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobDeclaration>() },
			{ Core.Constants.CountryCodes.Ireland, ObjectFactory.GetType<Integration.Customs.IEEMCS.IEMCSJobDeclaration>() },
			{ Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GBEMCS.IEMCSJobDeclaration>() }
		};

		protected override Type BaseTypeDecidedType => typeof(EMCSJobDeclaration);
	}
}
