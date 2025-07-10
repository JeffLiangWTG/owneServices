using System;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAMiscOptionsUserControl))]
	sealed class MiscOptionsUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(CAMiscOptionsUserControl);

		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = org1.PK;
			return declaration;
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}
	}
}
#endif
