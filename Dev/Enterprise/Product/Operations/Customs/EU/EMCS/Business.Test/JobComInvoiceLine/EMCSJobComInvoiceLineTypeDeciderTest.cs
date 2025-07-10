using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSJobComInvoiceLineTypeDeciderTest : BaseJobComInvoiceLineTypeDeciderTest
	{
		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad() => new Dictionary<ZGuid, Type>
		{
			{ Core.Constants.CountryGuids.Latvia, ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceLine>() }
		};

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests() => new Dictionary<string, Type>
		{
			{ Core.Constants.CountryCodes.Latvia, ObjectFactory.GetType<Integration.Customs.EUEMCS.IJobComInvoiceLine>() }
		};

		protected override Type BaseTypeDecidedType => typeof(EMCSJobComInvoiceLine);

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			return declaration.InvoiceHeader.JobComInvoiceLines.AddNew();
		}
	}
}
