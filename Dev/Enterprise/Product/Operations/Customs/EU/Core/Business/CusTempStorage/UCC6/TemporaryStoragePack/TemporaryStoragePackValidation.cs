using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePackValidation : AsycudaPackValidation
	{
		public TemporaryStoragePackValidation(TemporaryStoragePack parent) : base(parent)
		{
		}

		protected new TemporaryStoragePack Parent => (TemporaryStoragePack)base.Parent;

		protected override void CheckAPA_PackUQ()
		{
			base.CheckAPA_PackUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.APA_PackUQInfo);
		}

		protected override void CheckAPA_PackQty()
		{
			base.CheckAPA_PackQty();

			CheckPackQtyIsMandatoryOrForbiddenBasedOnPackUnit();
		}

		protected override void CheckAPA_MarksAndNumbers()
		{
			base.CheckAPA_MarksAndNumbers();

			if (Parent.Bill?.Header is TemporaryStorageHeader header && !header.IsTransfer)
			{
				CheckMarksAndNumbersIsMandatoryBasedOnPackUnit();
			}
		}

		#region Implementation

		void CheckPackQtyIsMandatoryOrForbiddenBasedOnPackUnit()
		{
			var targetPtyInfo = Parent.APA_PackQtyInfo;
			var isPackQtyEmpty = Parent.APA_PackQty.IsEmpty;
			var isBulkOrBreakBulk = IsBulkOrBreakBulk;

			if (!isPackQtyEmpty && isBulkOrBreakBulk)
			{
				targetPtyInfo.AddMessageError(Res.GetString("9B739B6C-7127-4F84-970B-6C47AF426724", "{0} must be zero for the selected {1}.", targetPtyInfo.HumanReadableName, Parent.APA_PackUQInfo.HumanReadableName));
			}
			else if (isPackQtyEmpty && !isBulkOrBreakBulk)
			{
				targetPtyInfo.AddMessageError(Res.GetString("24AE2F1C-08B4-4D31-A8B8-93E073D3D7E2", "{0} cannot be zero for the selected {1}.", targetPtyInfo.HumanReadableName, Parent.APA_PackUQInfo.HumanReadableName));
			}
		}

		void CheckMarksAndNumbersIsMandatoryBasedOnPackUnit()
		{
			if (!IsBulkOrBreakBulk)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.APA_MarksAndNumbersInfo);
			}
		}

		bool IsBulkOrBreakBulk => Parent.IsBulk || Parent.IsBreakBulk;

		#endregion
	}
}
