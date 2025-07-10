using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADPreviousDocumentWrapper : IPreviousDocument
{
	public SADPreviousDocumentWrapper(IMergedPreviousDocument previousDocument)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}
	readonly IMergedPreviousDocument previousDocument;

	public ZString DocType => previousDocument.Type;

	public ZString Category => previousDocument.Category;

	public ZString Mrn => IsMrnRegister() ? previousDocument.Mrn : ZString.Empty;

	public ZString ComplementOfInformation => ZString.Empty;

	public ZString Register => previousDocument.Register;

	public ZString ReferenceNumber => !IsMrnRegister() ? previousDocument.ReferenceNumber : ZString.Empty;

	public ZString ReferenceCIN => !IsMrnRegister() ? previousDocument.ReferenceNumberCin : ZString.Empty;

	public ZDate Date => previousDocument.Date;

	public ZString Series => previousDocument.Series;

	public ZString CustomsOffice
	{
		get
		{
			const int maximumFieldLength = 8;
			var customsOffice = previousDocument.CustomsOffice;
			return customsOffice.IsEmpty ? ZString.Empty : SADWrapperHelper.RemoveIsoCode(customsOffice).PadRight(maximumFieldLength, ' ');
		}
	}

	public ZInt? ItemNumber
	{
		get
		{
			var itemNumber = previousDocument.ItemNumber;

			return !itemNumber.IsEmpty ? itemNumber : null;
		}
	}
	#region Implementation

	ZBool IsMrnRegister() => Register == PreviousDocumentProcedureList.Codes.DichiarazioneMeccanizzataDiTransito;

	#endregion
}
