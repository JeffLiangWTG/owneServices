using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	internal class DummyWithSetableHumanReadableName : DummyBusinessObject
	{
		public DummyWithSetableHumanReadableName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string NameOverride { get; set; }

		protected override ZString HumanReadableNameCore
		{
			get { return NameOverride ?? base.HumanReadableNameCore; }
		}
	}
}
