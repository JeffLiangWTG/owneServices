using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class AdditionalInfoValidation : EU.ExitControl.Business.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();
			var parent = Parent;
			var itemNumber = parent.CSI_ItemNumber;
			var info = parent.CSI_ItemNumberInfo;
			if (itemNumber > 0)
			{
					var master = parent.Parent;
					if (master is CusExitReportItem reportItem ? reportItem.AdditionalInfosItemNumberDictionary.TryGetValue(itemNumber, out var reportItemAdditionalInfoCount) && reportItemAdditionalInfoCount > 1
						: (master is CusExitReport report && report.AdditionalInfosItemNumberDictionary.TryGetValue(itemNumber, out var reportAdditionalInfoCount) && reportAdditionalInfoCount > 1))
					{
						info.AddError(Res.GetString("{49B796D1-92C8-4969-B6F1-41B19AFCCC6A}", "{0} ({1}) should not be duplicated", info.Description, itemNumber));
					}
			}
			else
			{
				MandatoryValidation.CheckNotZero(info);
			}
		}
	}
}
