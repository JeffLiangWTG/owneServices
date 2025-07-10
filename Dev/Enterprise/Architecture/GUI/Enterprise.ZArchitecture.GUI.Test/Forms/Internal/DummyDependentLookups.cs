using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class DummyDependentLookups : ZLookups
	{
		public DummyDependentLookups(DummyWithLookups parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList PairList
		{
			get
			{
				if (pairList == null)
				{
					pairList = new CodeDescriptionPairList();
					pairList.AddPair("A", "A DESC");
					pairList.AddPair("B", "B DESC");
					((IAdditionalInformationWithSetter)pairList).SetAdditionalInformation("It is blue");
				}
				return pairList;
			}
		}
		CodeDescriptionPairList pairList;

		public DummyDependentWithCodeBusinessObjectCollection Dependents
		{
			get
			{
				if (dependents == null)
				{
					dependents = new DummyDependentWithCodeBusinessObjectCollection((DummyWithLookups)Parent, Factory);

					var dependent = dependents.AddNew();
					dependent.ZD1_Code = "ONE";

					dependent = dependents.AddNew();
					dependent.ZD1_Code = "TWO";

					dependent = dependents.AddNew();
					dependent.ZD1_Code = "THREE";
				}
				return dependents;
			}
		}

		DummyDependentWithCodeBusinessObjectCollection dependents;
	}
}
