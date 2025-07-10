using System.Linq;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class SealNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public SealNumberValidation(SealNumber parent) : base(parent)
		{
		}

		protected new SealNumber Parent => (SealNumber)base.Parent;

		CusEntryInstruction ParentEntryInstruction => Parent?.Parent as CusEntryInstruction;

		protected override void CheckCY_Code() { }

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var sourceData = Parent.CY_Data;
			var targetInfo = Parent.CY_DataInfo;

			if (sourceData.IsEmpty)
			{
				targetInfo.AddMessageError(Res.GetString("09780F18-84AC-4878-BEDA-C07623EDBC52", "Seal number can not be empty."));
			}
			else if (ParentEntryInstruction?.Seals?.OfType<SealNumber>()?.Any(s => s.CY_Data == sourceData && s.PK != Parent.PK) ?? false)
			{
				targetInfo.AddMessageError(Res.GetString("C27B4606-8749-4708-84F7-633F27BA17AC", "Seal number with specified name already exists."));
			}
		}
	}
}
