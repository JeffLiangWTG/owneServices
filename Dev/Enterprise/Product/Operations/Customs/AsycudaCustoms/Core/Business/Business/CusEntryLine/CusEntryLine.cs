using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class CusEntryLine : Customs.Business.CusEntryLine, Integration.Customs.AsycudaCustoms.ICusEntryLine
	{
		public CusEntryLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("0786680c-9c83-4920-b5a3-7939d5330a82", Caption = "Customs Value")]
		public override ZDecimal CL_CustomsValue
		{
			get => base.CL_CustomsValue;
			set => base.CL_CustomsValue = value;
		}

		[ResourceStringData("baeaea0f-2ef4-468a-9e77-0016eccd6a2a", Caption = "VAT/GST Value")]
		public override ZDecimal CL_ValueForVAT
		{
			get => base.CL_ValueForVAT;
			set => base.CL_ValueForVAT = value;
		}
	}
}
