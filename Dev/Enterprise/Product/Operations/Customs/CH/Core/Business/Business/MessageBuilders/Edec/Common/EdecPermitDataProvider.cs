using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecPermitDataProvider : IEdecGoodsItemPermit
{
	public static EdecPermitDataProvider New(Permit permit) => permit == null ? null : new EdecPermitDataProvider(permit);

	EdecPermitDataProvider(Permit permit)
	{
		this.permit = Argument.NotNull(permit, nameof(permit));
	}
	readonly Permit permit;

	public string PermitType => permit.CSI_Code.IsEmpty ? "0" : permit.CSI_Code.ToString();

	public string PermitAuthority => permit.CSI_IssuerType.IsEmpty ? "0" : permit.CSI_IssuerType.ToString();

	public string PermitNumber => permit.CSI_ReferenceNumber;

	public DateTime? IssueDate => permit.CSI_DateOfIssue.IsValid ? permit.CSI_DateOfIssue.ToDateTime() : null;

	public string AdditionalInformation => permit.CSI_Description.IsEmpty ? null : permit.CSI_Description.ToString();

	public IEnumerable<IEdecGoodsItemPermitDetail> PermitItemDetails => permitItemDetails ?? (permitItemDetails = permit.PermitItemDetails.Cast<PermitItemDetail>().Select(permitItemDetail => EdecPermitDetailDataProvider.New(permitItemDetail)).ToArray());
	IEnumerable<IEdecGoodsItemPermitDetail> permitItemDetails;
}
