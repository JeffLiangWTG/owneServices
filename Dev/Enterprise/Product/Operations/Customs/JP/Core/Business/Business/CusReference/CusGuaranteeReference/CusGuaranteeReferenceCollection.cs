using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class CusGuaranteeReferenceCollection : CusReferenceCollection<CusGuaranteeReference>
	{
		public CusGuaranteeReferenceCollection(CusEntryInstruction parent) : base(parent, GuaranteeCusCodeDataTypeList.Codes.GRN)
		{
			MaxCountValidationEnable(MaxRowCount, Res.GetString("CDA2895B-5A99-45A7-A68C-4EF60C2BC1F4", "You are only allowed a maximum of 2 guarantees here."));
		}

		public static int MaxRowCount => 2;

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxRowCount;
	}
}
