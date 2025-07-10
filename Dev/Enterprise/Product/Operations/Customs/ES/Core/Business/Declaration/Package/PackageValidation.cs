using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class PackageValidation : EU.Business.Declaration.PackageValidation
	{
		public PackageValidation(AutoCusDecHouseContainerPack parent) : base(parent)
		{
		}

		new Package Parent => (Package)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidatePackageIsAssignedToInvoiceLine();
		}

		void ValidatePackageIsAssignedToInvoiceLine()
		{
			if (Parent.InvoiceLinePivotCollection.Count == 0)
			{
				Parent.AddRowMessageError(Res.GetString("34B94A26-325A-4347-A383-16DEC7B85184", "Package Line is not assigned to an Invoice Line."));
			}
		}

		protected override void CheckCW_MarksAndNos()
		{
			const int maxLengthMarksAndNosForProvisionalPeriod = 42;
			base.CheckCW_MarksAndNos();
			var isTransitionPeriod = Parent.Declaration?.IsTransitionPeriodAES30 ?? false;
			if (isTransitionPeriod && Parent.CW_MarksAndNos.Length > maxLengthMarksAndNosForProvisionalPeriod)
			{
				Parent.CW_MarksAndNosInfo.AddMessageError(Res.GetString("6239D835-5F49-42D9-9547-D4B942FDED2E", "In provisional period, Marks length could not be greater than 42."));
			}
		}
	}
}
