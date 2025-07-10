using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DbHealthWarningRegistryElement))]
	sealed class DbHealthWarningRegistryElementTest : RegistryBusinessObjectTemplateTestCase<DbHealthWarningRegistryElement>
	{
		public void TestConstruction()
		{
			var testElement = new DbHealthWarningRegistryElement("Test Source", "Test Type", "Test Description", ZBool.True, "X", ZDateTime.BrettsBirthday);
			AssertEquals("Warning Source", "Test Source", testElement.Source);
			AssertEquals("Warning Type", "Test Type", testElement.WarningType);
			AssertEquals("Warning Description", "Test Description", testElement.Description);
			AssertEquals("Warning IsAcknowledgeable", ZBool.True, testElement.IsAcknowledgeable);
			AssertEquals("Warning IsAcknowledged", ZBool.True, testElement.IsAcknowledged);
			AssertEquals("Warning AcknowledgedBy", "X", testElement.AcknowledgedBy);
			AssertEquals("Warning AcknowledgedDate", ZDateTime.BrettsBirthday, testElement.AcknowledgedDate);
			AssertEquals("Warning AcknowledgedByUser", "NonOperational", testElement.AcknowledgedByUser);

			testElement = new DbHealthWarningRegistryElement("Test Source", "Test Type", "Test Description", ZBool.False);
			AssertEquals("Warning Source", "Test Source", testElement.Source);
			AssertEquals("Warning Type", "Test Type", testElement.WarningType);
			AssertEquals("Warning Description", "Test Description", testElement.Description);
			AssertEquals("Warning IsAcknowledgeable", ZBool.False, testElement.IsAcknowledgeable);
			AssertEquals("Warning IsAcknowledged", ZBool.False, testElement.IsAcknowledged);
			AssertEquals("Warning AcknowledgedBy", "", testElement.AcknowledgedBy);
			AssertEquals("Warning AcknowledgedDate", ZDateTime.Empty, testElement.AcknowledgedDate);
			AssertEquals("Warning AcknowledgedByUser", "", testElement.AcknowledgedByUser);

			AssertExceptionThrown(
				"Attempt to set acknowledgement fields in an non-acknowledgeable warning => should throw an exception",
				typeof(InvalidOperationException), () => testElement.SetAcknowledgementInfo("", ZDateTime.Empty));

			// Attempt to set IsAcknowledged in an non-acknowledgeable warning => should do NOTHING
			testElement.IsAcknowledged = ZBool.True;
			AssertEquals("Warning IsAcknowledged", ZBool.False, testElement.IsAcknowledged);

			testElement = new DbHealthWarningRegistryElement("", "SQL Server Version", "", ZBool.True, "", ZDateTime.Today);
			AssertEquals("Warning IsAcknowledgeable", ZBool.True, testElement.IsAcknowledgeable);
			AssertEquals("Warning IsAcknowledged", ZBool.False, testElement.IsAcknowledged);

			testElement.SetAcknowledgementInfo("SomeUser", ZDateTime.Today);
			AssertEquals("Warning IsAcknowledged", ZBool.True, testElement.IsAcknowledged);
			AssertEquals("Warning AcknowledgedByUser", "SomeUser", testElement.AcknowledgedByUser);

			testElement.SetAcknowledgementInfo("SomeUser", ZDateTime.Empty);
			AssertEquals("Warning IsAcknowledged", ZBool.False, testElement.IsAcknowledged);

			testElement.IsAcknowledged = ZBool.True;
			AssertEquals("Warning IsAcknowledged", ZBool.True, testElement.IsAcknowledged);
			AssertEquals("Warning AcknowledgedBy", Environment.Env.CurrentUser.Initials, testElement.AcknowledgedBy);
			AssertEquals("Warning AcknowledgedDate set?", true, testElement.AcknowledgedDate > ZDateTime.Now.AddHours(-1));
			AssertEquals("Warning AcknowledgedByUser", Environment.Env.CurrentUser.FullName, testElement.AcknowledgedByUser);
		}

		#region Implementation

		protected override DbHealthWarningRegistryElement GetBusinessObjectToClone()
		{
			DbHealthWarningRegistryElement result = new DbHealthWarningRegistryElement();
			return result;
		}

		protected override DbHealthWarningRegistryElement GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
