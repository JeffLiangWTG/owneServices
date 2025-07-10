using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusDV1DetailCollection : DependentBusinessObjectCollection<CusDV1Detail, JobDeclaration>
	{
		public CusDV1DetailCollection(JobDeclaration parent) : base(parent)
		{
		}

		public new JobDeclaration Master => base.Master;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var dv1Detail = (CusDV1Detail)child;
			Master.DV1DetailLineNumberGenerator.RecalculateWhenAdded(dv1Detail);
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			RecalculateAllLineNumbers();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			RecalculateAllLineNumbers();
		}

		internal void RecalculateAllLineNumbers()
		{
			Master.DV1DetailLineNumberGenerator.ReCalculateAll();
		}
	}
}
