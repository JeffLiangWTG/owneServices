using System.Data;
using CargoWise.EntityFramework;
using UniversalReferenceConstants = Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AlternativeEvidence : EU.ExitControl.Business.AlternativeEvidence
	{
		public AlternativeEvidence(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusExitReport ExitReport => (CusExitReport)base.ExitReport;
		public bool IsInformationOnNonExitedExport => ExitReport?.IsInformationOnNonExitedExport ?? false;
		public bool IsTranportDocumentRequired
		{
			get
			{
				var result = false;
				switch (CY_Code.ToUpperInvariant())
				{
					case UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DeliveryNoteSignedByConsigneeOutsideCustomsTerritory:
					case UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DeliveryNote:
					case UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.DocumentSignedByOperatorTakingGoodsOutOfUnion:
					case UniversalReferenceConstants.ExitReportAlternativeEvidenceTypes.Codes.OperatorsRecordsOfGoodsSuppliedToShipsAircraftOffshore:
						result = true;
						break;
				}
				return result;
			}
		}

		public new AlternativeEvidenceValidation Validation => (AlternativeEvidenceValidation)base.Validation;
		protected override Customs.Business.CusCodeDataValidation GetNewValidation() => IsInformationOnNonExitedExport ? new InformationOnNonExitedExportAlternativeEvidenceValidation(this) : new AlternativeEvidenceValidation(this);

		protected override EU.ExitControl.Business.IAdditionalInfoCollection<EU.ExitControl.Business.AdditionalInfo> CreateNewAdditionalInfoCollection() => new EU.ExitControl.Business.AdditionalInfoCollection<AdditionalInfo>(this);
	}
}
