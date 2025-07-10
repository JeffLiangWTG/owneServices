using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	class NctsDepartureCargoDescPhase4TemporaryStorageRegisterTransactionDataProvider(NctsDepartureCargoDesc nctsDepartureCargoDesc) : ITemporaryStorageRegisterTransactionDataProvider
	{
		readonly NctsDepartureCargoDesc nctsDepartureCargoDesc = Argument.NotNull(nctsDepartureCargoDesc, nameof(nctsDepartureCargoDesc));

		public IEnumerable<TemporaryStorageRegisterTransactionData> GetTemporaryStorageRegisterTransactionData()
		{
			var previousISTDocument = nctsDepartureCargoDesc.PreviousDocuments.Cast<NctsPreviousDocument>().FirstOrDefault(x => x.CSI_Code == PreviousDocumentCodeList.Codes._337);

			if (previousISTDocument != null)
			{
				yield return new TemporaryStorageRegisterTransactionData(Factory)
				{
					PreviousRegisterHeader = GetPreviousISTHeader(previousISTDocument),
					CustomsReferenceNumber = Header.MovementReferenceNumber,
					InternalReferenceNumber = Header.BH_JobReference,
					GrossMass = nctsDepartureCargoDesc.GrossMassInKilograms,
					PackageQuantity = (ZInt)nctsDepartureCargoDesc.Packages.Cast<NctsPackage>().Sum(x => x.B5_UnitCount),
					ReferenceType = TempStorageTransactionRefTypeList.Codes.NctsHeader,
					Comments = ZString.Empty,
					RegisterLineNo = previousISTDocument.CSI_LineNo
				};
			}
		}

		CusTempStorage.CusTempStorageRegHeader GetPreviousISTHeader(NctsPreviousDocument previousISTDocument)
		{
			return CusTempStorage.ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, previousISTDocument.CSI_ReferenceNumber, Header.CountryCode)?.RegisterHeader;
		}

		BusinessObjectFactory Factory => nctsDepartureCargoDesc.Factory;

		EU.NCTS.Business.NctsHeader Header => nctsDepartureCargoDesc.Header;
	}
}
