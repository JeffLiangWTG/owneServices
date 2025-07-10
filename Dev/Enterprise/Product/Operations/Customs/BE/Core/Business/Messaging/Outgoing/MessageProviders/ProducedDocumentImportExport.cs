using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class ProducedDocumentImportExport : Document, ITProducedDocumentImportExport
{
	public ProducedDocumentImportExport(CusSupportingInfo supportingInfo) : base(supportingInfo)
	{
		Argument.NotNull(supportingInfo, CusSupportingInfo.Schema.TableName);
		this.supportingInfo = supportingInfo;
		DocumentQuantity = new DocumentQuantity(supportingInfo);
		ArchiveInformation = new ArchiveInformation(supportingInfo);
	}

	readonly CusSupportingInfo supportingInfo;

	public ITArchiveInformation ArchiveInformation { get; set; }
	public ZString AuthorisationHolderCategory { get => ZString.Empty; }
	public ZString AuthorisationHolderID { get => ZString.Empty; }
	public ZString ComplementaryInformation { get => supportingInfo.CSI_AdditionalDescription; }
	public ITDocumentQuantity DocumentQuantity { get; set; }
	public DateTime ProducedDocumentsInformationDate { get => supportingInfo.CSI_DateOfIssue.ToDateTime(); }
	public ZBool ProducedDocumentsInformationDateSpecified { get => !supportingInfo.CSI_DateOfIssue.IsEmpty; }
	public ZString ProducedDocumentsValidationOffice { get => supportingInfo.CSI_CustomsOffice; }
}
