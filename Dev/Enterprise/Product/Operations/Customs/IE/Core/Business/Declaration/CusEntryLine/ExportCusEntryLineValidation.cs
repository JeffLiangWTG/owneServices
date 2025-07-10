using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportCusEntryLineValidation : CusEntryLineValidation
	{
		public ExportCusEntryLineValidation(CusEntryLine parent) : base(parent)
		{
		}

		public new CusEntryLine Parent => (CusEntryLine)base.Parent;

		protected override void CheckCL_StatisticalValue()
		{
			base.CheckCL_StatisticalValue();

			if (Parent.CL_StatisticalValue.IsEmpty && Parent.Header?.EntryInstruction is CusEntryInstruction instruction && instruction.StatisticalValueRequired)
			{
				Parent.CL_StatisticalValueInfo.AddMessageError(Res.GetString(
					"0F88228B-940E-4408-B38F-B8B0EC73F9DE",
					@"Statistical Value must be greater than 0. 
At a minimum, Invoice Line Price must be entered. 
It is suggested that other fields like Incoterms and then various charges should be entered in order for system to arrive at the correct Statistical Value."
				));
			}
		}
	}
}
