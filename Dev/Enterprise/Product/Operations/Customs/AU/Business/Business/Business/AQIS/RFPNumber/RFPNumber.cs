using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPNumber : AutoRFPNumber
	{
		public RFPNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Cloning

		public void Clone(JobComInvoiceLine clonedInvoiceLine)
		{
			RFPNumber clonedRFPNumber = clonedInvoiceLine.RFPNumbers.AddNew();
			using (clonedRFPNumber.SuspendSettingHasChanges())
			using (clonedRFPNumber.GetValidationSuspender())
			{
				clonedRFPNumber.CopyPersistentValuesFrom(this);
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}
