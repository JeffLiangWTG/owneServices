using System.Linq;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class InformationOnNonExitedExportAlternativeEvidenceValidation : AlternativeEvidenceValidation
	{
		public InformationOnNonExitedExportAlternativeEvidenceValidation(AlternativeEvidence parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			if (Parent.IsTranportDocumentRequired)
			{
				if (!Parent.AdditionalInfos.Any(i => i.IsATransportDocument))
				{
					Parent.CY_CodeInfo.AddMessageError(Res.GetString("89A1320E-031E-4C8F-A39A-2FB3E0D3D1A5", "At least one Transport Document is required"));
				}
			}
			else if (Parent.AdditionalInfos.Any(i => i.IsATransportDocument))
			{
				Parent.CY_CodeInfo.AddMessageError(Res.GetString("E8531C50-8EBC-4C7F-ACD4-1A2EC430515E", "Transport Document is not allowed for this selection"));
			}
		}
	}
}
