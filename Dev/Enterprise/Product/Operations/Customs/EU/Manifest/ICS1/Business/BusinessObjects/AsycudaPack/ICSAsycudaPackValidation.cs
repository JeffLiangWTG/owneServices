using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class ICSAsycudaPackValidation : AsycudaPackValidation
	{
		public ICSAsycudaPackValidation(AsycudaPack parent)
			: base(parent)
		{
		}

		protected override bool IsWeightRequired()
		{
			var bill = Parent.Bill;
			var header = bill != null && bill.ABL_GrossWeight == 0 ? bill.Header : null;
			return header != null &&
				header.SpecificCircumstanceIndicator != SpecificCircumstanceList.Codes.E &&
				header.Consol == null;
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			var packUQ = Parent.APA_PackUQ;
			if (Parent.APA_MarksAndNumbers.IsEmpty && packUQ != Core.Constants.PkgUnit.BulkBag && packUQ != Core.Constants.PkgUnit.BreakBulk)
			{
				var header = Parent.Bill?.Header;
				if (header != null && header.SpecificCircumstanceIndicator.IsEmpty && header.Consol == null)
				{
					Parent.APA_MarksAndNumbersInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.APA_MarksAndNumbersInfo.HumanReadableName));
				}
			}
		}
	}
}
