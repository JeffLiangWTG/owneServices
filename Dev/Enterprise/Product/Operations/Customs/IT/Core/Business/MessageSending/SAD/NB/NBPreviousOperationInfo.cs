using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBPreviousOperationInfo : IPreviousOperationInfo
{
	public NBPreviousOperationInfo(GroupedPreviousDocument groupedPreviousDocument)
	{
		this.groupedPreviousDocument = Argument.NotNull(groupedPreviousDocument, nameof(groupedPreviousDocument));
	}

	readonly GroupedPreviousDocument groupedPreviousDocument;

	public IPreviousAdministrativeReference PreviousAllibrament =>
		new NBPreviousAdministrativeReferenceWrapper(
				groupedPreviousDocument.SummaryDeclarationDocumentRegister,
				groupedPreviousDocument.SummaryDeclarationDocumentReferenceNumber,
				groupedPreviousDocument.SummaryDeclarationDocumentReferenceCIN,
				groupedPreviousDocument.SummaryDeclarationDocumentDate.Date,
				groupedPreviousDocument.SummaryDeclarationDocumentSeries,
				groupedPreviousDocument.SummaryDeclarationDocumentCustomsOffice,
				groupedPreviousDocument.SummaryDeclarationDocumentItemNumber
			);

	public ZString MRN => groupedPreviousDocument.SummaryDeclarationDocumentMRN;

	public IPreviousAdministrativeReference PreviousProcedure =>
		new NBPreviousAdministrativeReferenceWrapper(
			groupedPreviousDocument.PreviousProcedureDocumentRegister,
			groupedPreviousDocument.PreviousProcedureDocumentReferenceNumber,
			groupedPreviousDocument.PreviousProcedureDocumentReferenceCIN,
			groupedPreviousDocument.PreviousProcedureDocumentDate.Date,
			groupedPreviousDocument.PreviousProcedureDocumentSeries,
			groupedPreviousDocument.PreviousProcedureDocumentCustomsOffice,
			groupedPreviousDocument.PreviousProcedureDocumentItemNumber
		);

	public ZInt? NumberOfPackages => groupedPreviousDocument.PackageQuantity;

	public ZDecimal? GrossMass => groupedPreviousDocument.GrossMass;

	public ZString CombinedNomenclature => groupedPreviousDocument.Tariff;

	public ZDecimal? NetMass => groupedPreviousDocument.NetMass.GetValueOrNullIfZero();

	public ZDecimal? SupplementaryUnit => groupedPreviousDocument.SupplementaryQuantity.GetValueOrNullIfZero();
}
