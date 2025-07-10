using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class DocumentQuantity : ITDocumentQuantity
{
	public DocumentQuantity(CusSupportingInfo supportingInfo)
	{
		Argument.NotNull(supportingInfo, CusSupportingInfo.Schema.TableName);
		this.supportingInfo = supportingInfo;
	}

	readonly CusSupportingInfo supportingInfo;

	public ZDecimal Quantity { get => supportingInfo.CSI_Quantity; }
	public ZString QuantityCode { get => supportingInfo.CSI_UnitOfQuantity; }
}
