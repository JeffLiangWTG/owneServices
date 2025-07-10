using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageAdditionalInfoValidation : EU.Business.CusTempStorage.TemporaryStorageAdditionalInfoValidation
	{
		public TemporaryStorageAdditionalInfoValidation(EU.Business.CusTempStorage.TemporaryStorageAdditionalInfo parent)
				: base(parent)
		{
		}

		public new TemporaryStorageAdditionalInfo Parent => (TemporaryStorageAdditionalInfo)base.Parent;

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var parent = Parent;
			if (parent.CSI_Code.EqualsIgnoringCase(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture) && parent.IsAnAdditionalReference)
			{
				var referenceNumber = parent.CSI_ReferenceNumber;
				if (!referenceNumber.IsEmpty)
				{
					if (!ZDateTime.TryParseExact(referenceNumber, out var referenceDateTime, Constants.DateTimeFormat.AdditionalReferenceDateTime))
					{
						parent.CSI_ReferenceNumberInfo.AddMessageError(Res.GetString("4676547F-F94F-41E5-8C69-E0DB5960FCB5", "[BR20319] 1D23 must be in the format of {0}.", Constants.DateTimeFormat.AdditionalReferenceDateTime));
					}
				}
			}
		}
	}
}
