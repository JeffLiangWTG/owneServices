using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class DummyWithSettableCodeDescriptionPairList : DummyWithCodeDescriptionPairList
	{
		public DummyWithSettableCodeDescriptionPairList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ReadOnlyCodeDescriptionPairList DummyList
		{
			get { return fDummyList; }
		}

		public void SetDummyList(ReadOnlyCodeDescriptionPairList list)
		{
			fDummyList = list;
		}
	}
}
