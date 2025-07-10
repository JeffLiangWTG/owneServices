using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class DummyWithMultilingualCodePairList : DummyBusinessObject
	{
		public DummyWithMultilingualCodePairList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ReadOnlyCodeDescriptionPairList fDummyList;

		public virtual ReadOnlyCodeDescriptionPairList DummyList
		{
			get;
			set;
		}
	}
}
