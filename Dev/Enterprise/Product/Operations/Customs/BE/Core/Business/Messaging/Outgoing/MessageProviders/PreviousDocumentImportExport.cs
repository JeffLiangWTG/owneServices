using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class PreviousDocumentImportExport : Document, ITPreviousDocumentImportExport
{
	public PreviousDocumentImportExport(CusSupportingInfo supportingInfo) : base(supportingInfo)
	{
		Argument.NotNull(supportingInfo, CusSupportingInfo.Schema.TableName);
		this.supportingInfo = supportingInfo;
	}

	readonly CusSupportingInfo supportingInfo;

	// TODO all the following properties have to be confirmed as there may be mismatches
	public ZString PreviousDocumentArt { get => ZString.Empty; }
	public ZString PreviousDocumentBillOfLoading { get => supportingInfo.CSI_ReferenceNumber2; }
	public ZString PreviousDocumentCategory { get => supportingInfo.CSI_SubType; }
	public DateTime PreviousDocumentDate { get => supportingInfo.CSI_DateOfIssue.ToDateTime(); }
	public ZBool PreviousDocumentDateSpecified { get => !supportingInfo.CSI_DateOfIssue.IsEmpty; }
	public ZString PreviousDocumentItem { get => supportingInfo.CSI_LineNo.ToString(); }
	public ZString PreviousDocumentLoc { get => supportingInfo.CSI_CustomsOffice; }
}
