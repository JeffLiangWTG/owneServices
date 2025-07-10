using System.Linq;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class UNDGDataItemValidation : MasterFiles.Business.UNDGDataItemValidation
	{
		public UNDGDataItemValidation(UNDGDataItem parent)
			: base(parent)
		{
		}

		public new UNDGDataItem Parent => (UNDGDataItem)base.Parent;

		protected override void CheckDI_DG()
		{
			base.CheckDI_DG();

			var substance = Parent.UNDGSubstance;
			if (substance != null)
			{
				Parent.DI_DGInfo.AddMessageErrorIfContainsInvalidNACCSCharacter(substance.DG_ClassInfo, substance.DG_Class);

				var bill = Parent.Bill;
				if (bill != null && (bill.Substance?.PK == substance.PK || bill.UNDGs.Any(c => c.DI_DG == substance.PK && c.PK != Parent.PK)))
				{
					Parent.DI_DGInfo.AddMessageError(ValidationConstants.Bill.DuplicatedUNDG(substance));
				}
			}
		}

		protected override bool UNDGSubstanceIsRequired => false;
	}
}
