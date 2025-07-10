using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SuperDummyWithListChild : SuperDummyBusinessObject
	{
		public SuperDummyWithListChild(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DummyWithCodeDescriptionPairListCollection fDummiesWithList;
		public DummyWithCodeDescriptionPairListCollection DummiesWithList
		{
			get
			{
				if (fDummiesWithList == null)
				{
					fDummiesWithList = new DummyWithCodeDescriptionPairListCollection(Factory);
				}

				return fDummiesWithList;
			}
		}
	}
}
