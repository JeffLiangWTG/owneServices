using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BankAccountBasedOnCurrencyCollection))]
	sealed class BankAccountBasedOnCurrencyCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BankAccountBasedOnCurrencyCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BankAccountBasedOnCurrencyCollection GetCollectionToTest()
		{
			return new BankAccountBasedOnCurrencyCollection(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BankAccountBasedOnCurrency();
		}

		#endregion
	}
}
