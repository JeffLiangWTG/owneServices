using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ARLStatementOfAccountController))]
	sealed class ARLStatementOfAccountControllerTest : StatementControllerTest
	{
		public override void TestCorrectForm()
		{
			var statement = Factory.New<CusStatementHeader>();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				foreach (var type in new CARMStatementOfAccountStatementTypeList().GetAllCodes())
				{
					statement.B2_StatementType = CARMStatementOfAccountStatementTypeList.GetShortCode(type);
					using (var form = Controller.ShowFormForNewEntity(statement))
					{
						AssertEquals("CARM SOA Form Type", typeof(CARMSOAStatementForm), form.GetType());
					}
				}
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				foreach (var type in new CARMStatementOfAccountStatementTypeList().GetAllCodes())
				{
					statement.B2_StatementType = CARMStatementOfAccountStatementTypeList.GetShortCode(type);
					using (var form = Controller.ShowFormForNewEntity(statement))
					{
						AssertEquals("Normal Statement Form Type", typeof(StatementForm), form.GetType());
					}
				}
			}
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CustomsStatement;

		public override Type ControllerToBashType => typeof(ARLStatementOfAccountController);
	}
}
