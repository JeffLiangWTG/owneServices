using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportLineWrapper : ExportLineCommonWrapper, IDUAExportLine
	{
		public DUAExportLineWrapper(CusEntryLine cusEntryLine) : base(cusEntryLine)
		{
			randomLine = entryLine.RandomLine;
			previousDocuments = randomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
		}
		protected readonly JobComInvoiceLine randomLine;
		readonly PreviousDocumentCollection previousDocuments;

		public ZString GoodsCustomsProcedureCategory1 => entryLine.Tariff + entryLine.SupplementaryCode1 + entryLine.SupplementaryCode2;

		public ZString GoodsCustomsProcedureCategory2 => randomLine.JI_FormattedProcedure.SubstringSafe(0, 4).InsertSafe(2, ".");

		public ZString GoodsCustomsProcedureCategory3
		{
			get
			{
				if (goodsCustomsProcedureCategory3 == null)
				{
					goodsCustomsProcedureCategory3 = new CachedProperty<ZString>(entryLine.Factory, () =>
					{
						var concessionCodes = GetConcessionCodeFromProcedure();

						foreach (var additionalCode in GetAdditionalProcedureCodes())
						{
							concessionCodes += additionalCode.CY_Code.Right(3);
						}
						return concessionCodes;
					});
				}
				return goodsCustomsProcedureCategory3.Value;
			}
		}
		CachedProperty<ZString> goodsCustomsProcedureCategory3;

		protected virtual ZString GetConcessionCodeFromProcedure() => randomLine.JI_FormattedProcedure.Length > 4 ? randomLine.JI_FormattedProcedure.SubstringSafe(4) : ZString.Empty;

		protected virtual IEnumerable<EU.Business.AdditionalProcedureCode> GetAdditionalProcedureCodes() => randomLine.AdditionalProcedureCodes.Cast<EU.Business.AdditionalProcedureCode>();

		public ZString GoodsCustomsProcedureCategory4 => randomLine.JI_AdditionalSupplements.Replace(",", "");

		public ZString GoodsDescription => randomLine.JI_Description;

		public IDUAExportSpecialConditions SpecialConditions => specialConditions ?? (specialConditions = new DUAExportSpecialConditionsWrapper(entryLine));
		DUAExportSpecialConditionsWrapper specialConditions;

		public ZString CountryOfOrigin => entryLine.CountryOfOriginCode;

		public ZString StateOfOrigin => randomLine.JI_StateOrRegionOfOrigin;

		public ZDecimal SupplementaryUnitsNumber => entryLine.SupplementaryQuantity;

		public ZString SupplementaryUnitsQualifier => entryLine.SupplementaryUQ.ConvertCargoWiseToES(entryLine.Factory);

		public ZString DangerousGoodsCode => randomLine.UNDGs.FirstItemForBinding[0].SubstanceCode;

		public IExternalPackagesInfoCommon ExternalPackages => externalPackages ?? (externalPackages = new DUAExportExternalPackagesInfoWrapper(entryLine));
		ExternalPackagesInfoCommonWrapper externalPackages;

		public IInternalPackagesInfoCommon InternalPackages => internalPackages ?? (internalPackages = new DUAExportInternalPackagesInfoWrapper(entryLine));
		DUAExportInternalPackagesInfoWrapper internalPackages;

		public IVehiclePackagesInfoCommon VehiclePackages => vehiclePackages ?? (vehiclePackages = new DUAExportVehiclePackagesInfoWrapper(entryLine));
		DUAExportVehiclePackagesInfoWrapper vehiclePackages;

		public ZString DocumentReferenceNumber => previousDocuments != null && previousDocuments.Count > 0 ? PreviousDocumentHelper.GetSUMReferenceNumberToSend(previousDocuments[0]) : ZString.Empty;

		public ZString DocumentTypeCode => previousDocuments != null && previousDocuments.Count > 0 ? previousDocuments[0].CSI_SubType : ZString.Empty;

		public IReadOnlyCollection<IDUAExportDocuments> Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new List<DUAExportDocumentWrapper>();

					documents.AddRange(entryLine.SupportingDocuments.Cast<SupportingDocument>().Select(doc => new DUAExportDocumentWrapper(doc)));
					documents.AddRange(entryLine.Header.SupportingDocuments.Cast<SupportingDocument>().Select(doc => new DUAExportDocumentWrapper(doc)));
				}
				return documents;
			}
		}
		List<DUAExportDocumentWrapper> documents;
	}
}
