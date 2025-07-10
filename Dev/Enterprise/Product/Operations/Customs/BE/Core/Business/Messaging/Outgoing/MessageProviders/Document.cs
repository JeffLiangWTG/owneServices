using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

public class Document : ITDocument
{
	public Document(CusSupportingInfo supportingInfo)
	{
		Argument.NotNull(supportingInfo, CusSupportingInfo.Schema.TableName);
		this.supportingInfo = supportingInfo;
	}

	readonly CusSupportingInfo supportingInfo;

	public ZString DocumentReference { get => supportingInfo.CSI_ReferenceNumber; }
	public ZString DocumentType { get => supportingInfo.CSI_Code; }
}
