using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecPermitDetailDataProvider : IEdecGoodsItemPermitDetail
{
	public static EdecPermitDetailDataProvider New(PermitItemDetail permitItemDetail) => permitItemDetail == null ? null : new EdecPermitDetailDataProvider(permitItemDetail);

	EdecPermitDetailDataProvider(PermitItemDetail permitItemDetail)
	{
		this.permitItemDetail = Argument.NotNull(permitItemDetail, nameof(permitItemDetail));
	}
	readonly PermitItemDetail permitItemDetail;

	public string Key => permitItemDetail.CY_Code.IsEmpty ? "0" : permitItemDetail.CY_Code.ToString();

	public string Type => permitItemDetail.CY_Data;
}
