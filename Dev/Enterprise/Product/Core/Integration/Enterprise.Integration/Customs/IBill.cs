using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBill
		{
			ZGuid PK { get; }
			ZString CU_AddInfo { get; set; }
			ZString CU_BillNum { get; set; }
			ZString CU_BillType { get; set; }
			ZGuid CU_CU_ParentBill { get; set; }
			ZBool CU_GUIPresentationRecord { get; set; }
			ZDateTime CU_IssueDate { get; set; }
			ZGuid CU_JE { get; set; }
			ZString CU_MessageStatus { get; set; }
			ZDecimal CU_NoOfPacks { get; set; }
			ZString CU_PackType { get; set; }
		}
	}
}
