using System.Linq;
using System.Reflection;
using Enterprise.Accounting.ElectronicMessaging.Serbia;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Serbia
{
	class SerbiaEInvoiceAPICommandListTest : TestCase
	{
		public void TestCommandListConsistency()
		{
			var commandList = new SerbiaEInvoiceAPICommandList();
			var commandListCodes = commandList.GetAllCodes();

			var commandListConstants = typeof(SerbiaEInvoiceAPICommandList.Codes)
				.GetFields(BindingFlags.Public | BindingFlags.Static)
				.Where(x => x.IsLiteral && x.FieldType == typeof(string))
				.Select(x => (string)x.GetValue(null))
				.ToArray();

			// Ensure the command list and command constants stay in sync
			AssertSequencesEqual("Command list and constants should be equal", commandListConstants, commandListCodes);

			// Ensure each command code has a description
			var hasAnyEmptyDescription = commandListCodes
				.Select(code => commandList.GetDescriptionFromCode(code))
				.Any(desc => string.IsNullOrEmpty(desc));
			Assert("Command list should have no empty descriptions", !hasAnyEmptyDescription);
		}
	}
}
