using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class DummyWithLookups : DummyBusinessObject
	{
		public DummyWithLookups(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List(nameof(Lookups) + "." + nameof(DummyDependentLookups.PairList))]
		public override ZString Z0_Code { get => base.Z0_Code; set => base.Z0_Code = value; }

		[List("Lookups.Dependents")]
		public override ZGuid Z0_Guid
		{
			get
			{
				return base.Z0_Guid;
			}
			set
			{
				base.Z0_Guid = value;
			}
		}

		public DummyDependentLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new DummyDependentLookups(this);
				}
				return lookups;
			}
		}

		DummyDependentLookups lookups;
	}
}
