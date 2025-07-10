using CargoWise.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSEmailSubjectSuffixProvider
	{
		static string LrnCaption => Res.GetString("A8F84B66-E1F4-488F-B2E6-5055925A10B7", "LRN:");
		static string RefCaption => Res.GetString("5A0ECE96-EA1E-4EEA-9CFE-B557F98CFFB6", "Ref.:");

		readonly string suffix;

		public NCTSEmailSubjectSuffixProvider(NctsCommonMovementHeader movementHeader)
		{
			Argument.NotNull(movementHeader, nameof(movementHeader));
			var caption = movementHeader.IsPhase5Departure ? LrnCaption : RefCaption;
			suffix = caption + " " + movementHeader.BM_PaperlessInbondNum;
		}

		public void SetEmailSubjectSuffix(EmailDef email)
		{
			email.Subject = email.Subject + " " + suffix;
		}
	}
}
