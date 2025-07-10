using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var charge = entry.Charges.AddNew();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().Charges.AddNew();

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { CusEntryHeaderCharges.Schema.C1_Source };
		}
	}
}
