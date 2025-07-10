using System;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	[TestedType(typeof(CusClassificationModule))]
	sealed class CusClassificationModuleTest : Customs.Module.Testing.SingleTariffClassificationModuleAbstractTest<CusClassificationModule>
	{
		public override Type ModuleToBashType => typeof(CusClassificationModule);

		protected override string CountryCode => Core.Constants.CountryCodes.Botswana;
	}
}
