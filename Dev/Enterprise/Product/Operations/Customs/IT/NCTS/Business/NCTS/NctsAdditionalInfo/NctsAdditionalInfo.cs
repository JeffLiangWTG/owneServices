using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsAdditionalInfo : EU.NCTS.Business.NctsAdditionalInfo
{
	public NctsAdditionalInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema
	{
		public new const int CSI_CodeMaxLength = 5;
	}

	[MaxLength(Schema.CSI_CodeMaxLength)]
	public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }
}
