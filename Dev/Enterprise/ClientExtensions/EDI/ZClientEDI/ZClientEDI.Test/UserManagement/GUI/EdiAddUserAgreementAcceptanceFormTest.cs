using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(EdiAddUserAgreementAcceptanceForm))]
	public class EdiAddUserAgreementAcceptanceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_INS_EdiUserAgreement ON EdiUserAgreement ; DISABLE TRIGGER TG_UPD_EdiUserAgreement ON EdiUserAgreement");

			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			enterprise.Organisation.OH_Code = "org001";

			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_VariantCode = "CA1";
			agreement.ERA_VariantDescription = "CA1";
			agreement.ERA_VersionNumber = 1;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);

			var acceptanceLog = Factory.New<EdiUserAgreementAcceptanceLog>();
			acceptanceLog.EUL_ERA = agreement.PK;
			acceptanceLog.EUL_AcceptanceTimeUtc = ZDateTime.UtcNow;
			acceptanceLog.EUL_LE = enterprise.PK;

			Factory.Save();
			return new EdiAddUserAgreementAcceptanceForm(acceptanceLog);
		}
	}
}
