
namespace Enterprise.Customs.FR.Business.CusStatement
{
	public class CusStatementEntryValidation : Customs.Business.CusStatementLineValidation
	{
		public CusStatementEntryValidation(CusStatementEntry parent) : base(parent)
		{
		}

		protected override void CheckB3_EntryType()
		{
			base.CheckB3_EntryType();
			if (Parent.B3_EntryType != Parent.StatementHeader.B2_BranchDesignation)
			{
				Parent.B3_EntryTypeInfo.AddMessageError(InconsistentEntryType);
			}
		}

		public static string InconsistentEntryType => Res.GetString("98F3D0A4-8266-485E-BDBB-97B573AD90EC", "The entry type is inconsistent in regard to its header.");

		protected new CusStatementEntry Parent => base.Parent as CusStatementEntry;
	}
}
