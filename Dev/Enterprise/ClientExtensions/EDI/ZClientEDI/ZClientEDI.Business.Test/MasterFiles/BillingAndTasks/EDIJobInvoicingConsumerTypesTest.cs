using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIJobInvoicingConsumerTypesTest : TestCaseWithFactory
	{
		public void TestUniqueCodes()
		{
			string errors = "";
			List<string> codes = new List<string>();
			foreach (JobInvoicingConsumerType type in EDIJobInvoicingConsumerTypes.New())
			{
				if (codes.Contains(type.Code))
				{
					errors += "Code [" + type.Code + "] is not unique in the EDIJobInvoicingConsumerTypes List.\r\n";
				}
				codes.Add(type.Code);
			}

			if (errors.Length > 0)
			{
				Fail("Please update the code in the C# file\r\n\r\n" + errors);
			}
			else
			{
				Assert("All OK", true);
			}
		}

		public void TestDistanceCalculationCheckpoint()
		{
			foreach (JobInvoicingConsumerType type in EDIJobInvoicingConsumerTypes.New())
			{
				if (type is EDIJobInvoicingConsumerType)
				{
					AssertEquals(Env.Security.None, type.DistanceCalculationCheckpoint);
				}
			}
		}
	}
}