using System;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ImportClassificationController))]
	sealed class ImportClassificationControllerTest : Customs.Module.Testing.ImportClassificationControllerTest
	{
		public override Type ControllerToBashType => typeof(ImportClassificationController);

		protected override Type GetBusinessObjectType() => typeof(Classification);

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}
