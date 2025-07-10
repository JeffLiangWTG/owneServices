using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageBillValidation : EU.Business.CusTempStorage.TemporaryStorageBillValidation
	{
		public TemporaryStorageBillValidation(AutoAsycudaBill parent)
				: base(parent)
		{
		}

		public new TemporaryStorageBill Parent => (TemporaryStorageBill)base.Parent;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			var parent = Parent;

			if (parent.Header is TemporaryStorageHeader header && header.IsUCC6)
			{
				var additionalReferences = parent.AdditionalInfos.Where(addInfo => addInfo.IsAnAdditionalReference);
				if (!additionalReferences.Any(Constants.AdditionalReferenceCodes.IsEstimatedDeparture))
				{
					parent.ABL_BillNumberInfo.AddMessageError(GetEstimatedTimeMissingMessage(Constants.AdditionalReferenceCodes.EstimatedTimeOfDeparture));
				}
			}
		}

		static string GetEstimatedTimeMissingMessage(ZString missingType) => Res.GetString(
			"E4EE0397-AE4A-466E-99B8-436AD4D16E2D",
			"[BR20319] You have not entered a {0} additional reference under the Additional Information grid.",
			missingType);
	}
}
