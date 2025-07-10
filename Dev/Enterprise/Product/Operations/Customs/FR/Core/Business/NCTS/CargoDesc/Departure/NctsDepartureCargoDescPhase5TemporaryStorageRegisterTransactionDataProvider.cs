using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	class NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider : EU.NCTS.Business.NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider
	{
		public NctsDepartureCargoDescPhase5TemporaryStorageRegisterTransactionDataProvider(NctsDepartureCargoDesc nctsDepartureCargoDesc) : base(nctsDepartureCargoDesc)
		{
		}

		protected override CusTempStorageRegHeader GetTempStorageRegHeader(EU.NCTS.Business.NctsPreviousDocument previousDocument)
		{
			var registerHeader = CusTempStorage.ComplementaryJobISTFinder.FindFromReferenceNumber(Factory, previousDocument.CSI_ReferenceNumber, Header.CountryCode)?.RegisterHeader;
			var cusTempStorageRegLine = registerHeader?.CusTempStorageRegLines?.FirstOrDefault(x => x.SRL_LineNumber == previousDocument.CSI_ItemNumber && x.SRL_PackageType == previousDocument.CSI_UnitOfQuantity2);
			registerHeader = cusTempStorageRegLine != null ? registerHeader : null;
			return registerHeader;
		}

		protected override IEnumerable<TemporaryStorageRegisterTransactionData> CreateTemporaryStorageRegisterTransactionDataCollection(IEnumerable<(CusTempStorageRegHeader TempStorageRegHeader, EU.NCTS.Business.NctsPreviousDocument PreviousDocument)> regHeaderAndPreviousDocuments)
		{
			var groups = regHeaderAndPreviousDocuments
				.GroupBy(x => (PreviousRegisterHeader: x.TempStorageRegHeader, RegisterLineNo: x.PreviousDocument.CSI_ItemNumber));

			foreach (var group in groups)
			{
				yield return new TemporaryStorageRegisterTransactionData(Factory)
				{
					PreviousRegisterHeader = group.Key.PreviousRegisterHeader,
					CustomsReferenceNumber = Header.MovementReferenceNumber,
					InternalReferenceNumber = Header.MovementHeader.BM_PaperlessInbondNum,
					GrossMass = group.Sum(g => g.PreviousDocument.CSI_Quantity),
					PackageQuantity = group.Sum(g => (ZInt)g.PreviousDocument.CSI_Quantity2),
					ReferenceType = TempStorageTransactionRefTypeList.Codes.NctsHeader,
					Comments = ZString.Empty,
					RegisterLineNo = group.Key.RegisterLineNo
				};
			}
		}
	}
}
