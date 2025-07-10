using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		protected override ZForm GetForm(BusinessObject bizO) => new JobDeclarationForm((JobDeclaration)bizO);

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
		}
		IDisposable localCountryCustomsInterface;

		protected override void TearDown()
		{
			base.TearDown();
			localCountryCustomsInterface?.Dispose();
		}
	}
}
