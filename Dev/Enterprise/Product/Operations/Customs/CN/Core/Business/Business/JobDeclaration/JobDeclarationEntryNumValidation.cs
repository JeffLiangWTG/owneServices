using System.Text.RegularExpressions;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CN.Business
{
	public class JobDeclarationEntryNumValidation : CusEntryNumValidation
	{
		public JobDeclarationEntryNumValidation(CusEntryNumber parent) : base(parent)
		{
		}

		internal IValidationModeProvider ValidationModeProvider => Parent.Parent as JobDeclaration;

		protected override void CheckCE_EntryNum()
		{
			base.CheckCE_EntryNum();

			switch (Parent.CE_EntryType)
			{
				case AdditionalReferenceNumberTypes.Codes.DTDPreNumber:
					CheckCE_EntryNumForDTD();
					break;
				case AdditionalReferenceNumberTypes.Codes.GoodsCarriedListNo:
					CheckCE_EntryNumForGCL();
					break;
			}
		}

		void CheckCE_EntryNumForDTD()
		{
			if (!Regex.IsMatch(Parent.CE_EntryNum, @"^[a-zA-Z0-9]{16}$"))
			{
				Parent.CE_EntryNumInfo.AddNotification(Res.GetString("42A6A32E-E68A-4E64-842F-1A3D5860DD4D", "DTD number should be 16 alphanumeric"), ValidationModeProvider);
			}
		}

		void CheckCE_EntryNumForGCL()
		{
			if (!Regex.IsMatch(Parent.CE_EntryNum, @"^[a-zA-Z0-9]{13}$"))
			{
				Parent.CE_EntryNumInfo.AddNotification(Res.GetString("7D047672-01C3-454B-BCBC-3D2565CA211E", "GCL number should be 13 alphanumeric"), ValidationModeProvider);
			}
		}
	}
}
