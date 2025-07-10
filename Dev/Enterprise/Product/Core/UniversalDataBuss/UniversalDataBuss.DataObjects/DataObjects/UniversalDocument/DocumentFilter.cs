using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), RootElement("Filter")]
	public class DocumentFilter : IDataObject
	{
		[Mandatory]
		public DocumentFilterType? Type { get; set; }

		[MaxLength(1024), Mandatory]
		public ZString? Value { get; set; }
	}

	public enum DocumentFilterType
	{
		FileName,
		DocumentID,
		DocumentType,
		IsPublished,
		SaveDateUTCFrom,
		SaveDateUTCTo,
		CompanyCode,
		BranchCode,
		RelatedEDoc,
		DepartmentCode
	}
}
