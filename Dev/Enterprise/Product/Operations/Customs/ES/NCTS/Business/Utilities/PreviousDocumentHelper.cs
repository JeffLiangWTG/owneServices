using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public static class PreviousDocumentHelper
	{
		const int NctsDepartureReferenceLineNoLength = 3;

		public static ZString GetReferenceNumberToSendNctsDeparture(NctsPreviousDocument doc)
		{
			var referenceNumber = doc.CSI_ReferenceNumber;
			var docType = doc.CSI_Code;
			var docClass = doc.CSI_SubType;

			referenceNumber = docClass.Equals(EU.Business.PreviousDocumentClassList.Codes.PreviousDocument) ? docType + referenceNumber : (string)referenceNumber;

			if (NctDepartureAAEDocumentTypes.Contains(doc.CSI_Code) && doc.CSI_LineNo > 0)
			{
				referenceNumber += doc.CSI_LineNo.ToString().PadLeft(NctsDepartureReferenceLineNoLength, '0');
			}

			return referenceNumber;
		}

		public static readonly ImmutableHashSet<ZString> NctDepartureAAEDocumentTypes = new ZString[] { NctsPreviousDocumentTypeCodeList.Codes.DuaDocument, NctsPreviousDocumentTypeCodeList.Codes.DvdDocument, NctsPreviousDocumentTypeCodeList.Codes.IdaDocument, NctsPreviousDocumentTypeCodeList.Codes.RunDocument, NctsPreviousDocumentTypeCodeList.Codes.TrsDocument }.ToImmutableHashSet();
	}
}
