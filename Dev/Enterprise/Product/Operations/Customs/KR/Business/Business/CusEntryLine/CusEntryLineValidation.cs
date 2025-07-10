using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryLineValidation : AutoKRCusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}

		public CusEntryLine EntryLine
		{
			get { return Parent; }
		}

		protected new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}

		protected override void CheckCL_CustomsValue()
		{
			base.CheckCL_CustomsValue();

			var transactionType = Parent.Declaration?.JE_ExportGoodsType ?? CargoWise.Types.ZString.Empty;
			var isExport = Parent.Declaration?.IsExport ?? CargoWise.Types.ZBool.False;

			if (isExport && transactionType == TransactionTypeCodeList.Codes._82)
			{
				if (Parent.CL_CustomsValue != 0)
				{
					Parent.CL_CustomsValueInfo.AddMessageError(Res.GetString("4E6E3CF9-163E-4A89-A522-578F512E2684", "If Transaction Type is ‘82’, then Customs Value needs to be ‘0’."));
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.CL_CustomsValueInfo);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.CL_CustomsValueInfo);
			}
		}
	}
}
