using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider(NctsDepartureCargoDesc nctsDepartureCargoDesc) : ITemporaryStorageRegisterTransactionDataProvider
{
	readonly NctsDepartureCargoDesc nctsDepartureCargoDesc = Argument.NotNull(nctsDepartureCargoDesc, nameof(nctsDepartureCargoDesc));

	public IEnumerable<TemporaryStorageRegisterTransactionData> GetTemporaryStorageRegisterTransactionData()
	{
		var temporaryStorageDeclarationPreviousDocuments = nctsDepartureCargoDesc.PreviousDocuments.Cast<NctsPreviousDocument>().Where(x => x.CSI_Code == NCTS5PreviousDocumentTypeList.Codes.N337);

		var regHeaderAndPreviousDocuments = temporaryStorageDeclarationPreviousDocuments
			.Select(previousDocument => (TempStorageRegHeader: GetTempStorageRegHeader(previousDocument), PreviousDocument: previousDocument))
			.Where(x => x.TempStorageRegHeader != null);

		return CreateTemporaryStorageRegisterTransactionDataCollection(regHeaderAndPreviousDocuments);
	}

	protected virtual IEnumerable<TemporaryStorageRegisterTransactionData> CreateTemporaryStorageRegisterTransactionDataCollection(IEnumerable<(CusTempStorageRegHeader TempStorageRegHeader, NctsPreviousDocument PreviousDocument)> regHeaderAndPreviousDocuments)
	{
		foreach (var regHeaderAndPreviousDocument in regHeaderAndPreviousDocuments)
		{
			var previousDocument = regHeaderAndPreviousDocument.PreviousDocument;
			yield return new TemporaryStorageRegisterTransactionData(Factory)
			{
				PreviousRegisterHeader = regHeaderAndPreviousDocument.TempStorageRegHeader,
				RegisterLineNo = previousDocument.CSI_ItemNumber,
				GrossMass = -previousDocument.CSI_Quantity,
				PackageQuantity = -previousDocument.CSI_PackQty,
				CustomsReferenceNumber = Header.JobNumber,
				ReferenceType = NCTS5ReferenceType,
				InternalReferenceNumber = Header.MovementReferenceNumber,
				InternalReferenceType = NCTS5InternalReferenceType,
			};
		}
	}

	protected virtual CusTempStorageRegHeader GetTempStorageRegHeader(NctsPreviousDocument previousDocument)
	{
		var headers = Factory.Load<CusTempStorageRegHeader>(new ZQuery(CusTempStorageRegHeaderSchema.SRH_PreviousReference, previousDocument.CSI_ReferenceNumber));
		return headers.FirstOrDefault(header => header.CusTempStorageRegLines.Cast<CusTempStorageRegLine>().Any(line => line.SRL_LineNumber == previousDocument.CSI_ItemNumber));
	}

	protected BusinessObjectFactory Factory => nctsDepartureCargoDesc.Factory;

	protected NctsHeader Header => nctsDepartureCargoDesc.Header;

	const string NCTS5ReferenceType = "DEC";
	const string NCTS5InternalReferenceType = "MRN";
}
