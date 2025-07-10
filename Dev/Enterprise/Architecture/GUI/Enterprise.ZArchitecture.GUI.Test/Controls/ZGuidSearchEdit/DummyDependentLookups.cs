using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class DummyDependentLookups : ZLookups
	{
		public DummyDependentLookups(DummyWithLookups parent)
			: base(parent)
		{
		}

		public DummyDependentWithCodeBusinessObjectCollection Dependents
		{
			get
			{
				if (dependents == null)
				{
					dependents = new DummyDependentWithCodeBusinessObjectCollection((DummyWithLookups)Parent, Factory);
				}
				return dependents;
			}
		}

		DummyDependentWithCodeBusinessObjectCollection dependents;

		public ActiveBusinessObjectCollection<DummyDependentWithCodeBusinessObject> ActiveDependents
		{
			get
			{
				if (activeDependents == null)
				{
					activeDependents = new ActiveBusinessObjectCollection<DummyDependentWithCodeBusinessObject>(Factory, (DummyWithLookups)Parent);
				}
				return activeDependents;
			}
		}

		ActiveBusinessObjectCollection<DummyDependentWithCodeBusinessObject> activeDependents;
	}
}
