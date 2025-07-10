using System;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class CompositeCollectionTest : BusinessObjectCollection<BusinessObject>, ICompositeCollection
	{
		public CompositeCollectionTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region ICompositeCollection Members

		public Type TypeOfElementFromPK(ZGuid pK)
		{
			return typeof(DummyBusinessObject);
		}

		public Type TypeOfElementFromCode(ZString code)
		{
			if (code == "TEA")
			{
				return typeof(DummyChildBusinessObject);
			}
			else
			{
				return typeof(DummyBusinessObject);
			}
		}

		public int MaxLength
		{
			get
			{
				return DummyBusinessObject.Schema.Z0_Code.Length;
			}
		}

		#endregion
	}
}
