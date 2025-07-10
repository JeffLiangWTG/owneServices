using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentReferenceNumberProvider
{
	public PreviousDocumentReferenceNumberProvider(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}
	readonly EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument;

	public ZString Procedure => previousDocument.CSI_Procedure;

	public ZString ReferenceNumber => previousDocument.CSI_ReferenceNumber;

	public ZPropertyInfo ReferenceNumberInfo => previousDocument.CSI_ReferenceNumberInfo;

	public ZBool IsReferenceWithCin => !IsCIM && !IsManualA3 && ReferenceNumber.Length > 1;

	public ZString ReferenceNumberCin => IsReferenceWithCin ? ReferenceNumber.Right(1) : ZString.Empty;

	public ZString ReferenceNumberWithoutCin => IsReferenceWithCin ? ReferenceNumber.Left(ReferenceNumber.Length - 1) : ReferenceNumber;

	public ZBool IsCIM => Procedure == PreviousDocumentProcedureList.Codes.LetteraDiVetturaFerroviariaModCim;

	public ZString DocumentType => previousDocument.CSI_Code;

	#region Implementation

	public ZBool IsManualA3 => Procedure == PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3 && previousDocument.CSI_Status == ManualSeries;

	const string ManualSeries = "M";

	#endregion
}
