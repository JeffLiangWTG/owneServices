using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public class NonPersistentDepartureContainerPivotCollection : EU.NCTS.Business.NonPersistentDepartureContainerPivotCollection
	{
		public NonPersistentDepartureContainerPivotCollection(NctsDepartureCargoDesc line) : base(line)
		{
		}
		protected new NctsDepartureCargoDesc line => (NctsDepartureCargoDesc)base.line;

		public new NonPersistentDepartureContainerPivot this[int index] => (NonPersistentDepartureContainerPivot)Elements[index];

		public new NonPersistentDepartureContainerPivot AddNew() => (NonPersistentDepartureContainerPivot)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentDepartureContainerPivot(line);
		}
	}
}
