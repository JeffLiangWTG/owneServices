using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public sealed class PreviousDocumentProvider : IPreviousDocument
	{
		public static PreviousDocumentProvider NewOrNull(PreviousDocument previousDocument) => previousDocument == null ? null : new PreviousDocumentProvider(previousDocument);

		PreviousDocumentProvider(PreviousDocument previousDocument)
		{
			this.previousDocument = previousDocument;
		}
		readonly PreviousDocument previousDocument;

		public string FullType => previousDocument.CSI_Code;

		public string Type => previousDocument.CSI_Code.Left(4);

		public string Qualifier => previousDocument.CSI_Code.SubstringSafe(4, 3).ValueOrNullIfEmpty();

		public string ReferenceNumber => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Reference) ? (string)previousDocument.CSI_ReferenceNumber : null;

		public int GoodsItemNumber => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.ItemNumber) ? (int)previousDocument.CSI_ItemNumber : 0;

		public string MeasurementUnitAndQualifier => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.MeasurementUnit) ? (string)previousDocument.CSI_UnitOfQuantity : null;

		public decimal Quantity => previousDocument.CSI_Quantity;

		public string Complement => MapProperty(UniversalReferenceConstants.RefCusCodeListAttributes.Name.Complement) ? (string)previousDocument.CSI_Description : null;

		bool MapProperty(string attributeName)
		{
			bool result;
			if (previousDocument.Declaration?.IsExport ?? false)
			{
				result = RefCusCode?.HasAttribute(attributeName) ?? false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		ZZRefCusCodeListCombined RefCusCode => CachedValueHelper.GetValue(ref refCusCode, () => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(previousDocument.Factory, previousDocument.CSI_Code, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfExportDirection, CargoWise.Types.ZDateTime.Today));
		CachedValue<ZZRefCusCodeListCombined> refCusCode;
	}
}
