using System;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ExportClassificationController))]
	sealed class ExportClassificationControllerTest : Customs.Module.Testing.ExportClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(ExportClassificationController);

		protected override Type GetBusinessObjectType() => typeof(Classification);

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
