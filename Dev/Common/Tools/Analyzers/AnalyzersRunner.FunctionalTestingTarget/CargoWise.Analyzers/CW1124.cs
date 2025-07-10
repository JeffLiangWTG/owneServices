// CW1124:Do Not Use Cached Property Analyzer

using CargoWise.EntityFramework;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1124
	{
		readonly BusinessObjectFactory Factory = new BusinessObjectFactory();

		public bool BillShouldBeLocked
		{
			get
			{
				if (billShouldBeLocked == null)
				{
					billShouldBeLocked = new CachedProperty<bool>(Factory, GetBillShouldBeLocked);
				}
				return billShouldBeLocked.Value;
			}
		}
		CachedProperty<bool> billShouldBeLocked;

		protected virtual bool GetBillShouldBeLocked() => false;
	}
}
