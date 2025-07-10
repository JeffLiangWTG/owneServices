using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DummyThatThrowsOnAProperty : DummyBusinessObject
	{
		public DummyThatThrowsOnAProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override ZString HumanReadableNameCore => $"Dummy({Z0_Code})";

		public override ZString Z0_VarCharMax
		{
			get
			{
				throw new InvalidOperationException("Nyat nyat nyat");
			}
		}
	}
}
