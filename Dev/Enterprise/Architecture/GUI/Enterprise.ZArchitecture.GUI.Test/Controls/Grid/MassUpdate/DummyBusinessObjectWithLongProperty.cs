using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping.Testing;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class DummyBusinessObjectWithLongProperty : DummyBusinessObjectWithForeignKey
	{
		public DummyBusinessObjectWithLongProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString __WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW__prop__ZString
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo __WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW__prop__ZStringInfo => GetZPropertyInfo(nameof(__WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW__prop__ZString));
	}
}
