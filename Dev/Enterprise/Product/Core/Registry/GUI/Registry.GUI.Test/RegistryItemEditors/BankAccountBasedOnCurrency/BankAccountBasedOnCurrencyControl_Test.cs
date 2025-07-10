using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(BankAccountBasedOnCurrencyControl))]
	sealed class BankAccountBasedOnCurrencyControl_Test : Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BankAccountBasedOnCurrencyCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((BankAccountBasedOnCurrencyControl)control).BankAccountBasedOnCurrencyGrid.ReadOnly;
		}
	}
}
