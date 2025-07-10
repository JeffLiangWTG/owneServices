using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.Movement))]
	sealed class MovementTest : ValueObjectTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.Movement value = null;
			value = new Xsd.MovementCollection().AddNew();
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestIsSpecified()
		{
			Xsd.Movement movement = new Xsd.Movement();
			AssertEquals("Movement should not be specified by default", false, movement.IsSpecified);

			movement.EstimatedDateTime = ZDateTime.Empty;
			movement.ActualDateTime = ZDateTime.Now;
			AssertEquals("Movement should be specified if there is an estimated date", true, movement.IsSpecified);

			movement.EstimatedDateTime = ZDateTime.Now;
			movement.ActualDateTime = ZDateTime.Empty;
			AssertEquals("Movement should be specified if there is an actual date", true, movement.IsSpecified);

			movement.Port = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			movement.EstimatedDateTime = ZDateTime.Empty;
			movement.ActualDateTime = ZDateTime.Empty;
			AssertEquals("Movement should be specified if there is a port", true, movement.IsSpecified);

			movement.IsSpecified = false;
			AssertEquals("IsSpecified=false when explicitly set to false", false, movement.IsSpecified);
		}
	}
}
