namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class InwardsCreationCMRTest : InwardsCreationPreCMRTest
	{
		protected override void SetEntryStatusToPaid(CusEntryHeader entry)
		{
			entry.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;
		}

		protected override void SetCMRMode(JobDeclaration dec)
		{
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
		}
	}
}
