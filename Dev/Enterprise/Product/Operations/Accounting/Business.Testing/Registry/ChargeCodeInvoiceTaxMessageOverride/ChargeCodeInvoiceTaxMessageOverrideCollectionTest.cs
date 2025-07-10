using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeInvoiceTaxMessageOverrideCollection))]
	public class ChargeCodeInvoiceTaxMessageOverrideCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeCodeInvoiceTaxMessageOverrideCollection>
	{
		#region Implementation

		protected override ChargeCodeInvoiceTaxMessageOverrideCollection GetCollectionToTest()
		{
			return new ChargeCodeInvoiceTaxMessageOverrideCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeCodeInvoiceTaxMessageOverride();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new ChargeCodeInvoiceTaxMessageOverrideCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
