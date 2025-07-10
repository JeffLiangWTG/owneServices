using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.MacroEvaluator.Testing
{
	[TestedType(typeof(MacroEvaluatorManager))]
	sealed class MacroEvaluatorManagerTest : EvaluatorManagerTest
	{
		public void TestShouldUnEscapeAngleBrackets()
		{
			ShipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = @"S123A\<";
			Manager.Macro = "<JobNumber>";
			Manager.Evaluate();
			AssertEquals(@"S123A\<", Manager.Output);

			Manager.Macro = @"<If(""<JobNumber>""!=""TEST"", ""- \>$680 to MML"", """")> | <Substring(""\>\> 1"", 0, 1)>";
			Manager.Evaluate();

			AssertEquals(@"- >$680 to MML | >", Manager.Output);
		}

		public void TestDataContext_List()
		{
			Assert("GenericFreightJob", Manager.DataContext_List.ContainsCode("GenericFreightJob"));
			Assert("Shipment", Manager.DataContext_List.ContainsCode("Shipment"));
			Assert("GenericCommercialInvoice", Manager.DataContext_List.ContainsCode("GenericCommercialInvoice"));
			Assert("Invalid Data Context", !Manager.DataContext_List.ContainsCode("InvalidDataContextForTesting"));
		}

		public void TestEvaluateMacro()
		{
			ShipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S12345";
			ShipmentBO[JobShipmentSchema.JS_RL_NKOrigin] = "DEHAM";

			Manager.Macro = "<Delimit(\"<JobNumber>\", \" ex \", \"<Origin.Location.PortName>\")>";
			Manager.Evaluate();
			AssertEquals("S12345 ex Hamburg", Manager.Output);

			Manager.DataContextCode = "Shipment";
			Manager.Macro = "<ShipmentNumber> - <OriginLoco.CodeAndName>";
			Manager.Evaluate();
			AssertEquals("S12345 - DEHAM - Hamburg", Manager.Output);
		}

		public void TestInvalidDataContext()
		{
			Manager.DataContextCode = nameof(DataContext.ComplianceSeqBook);

			Manager.Macro = "<JobNumber><ABC123>";
			var result = Manager.Evaluate();

			AssertEquals("Document supporter did not create wrappers for 'ComplianceSeqBook' data context", result);
			AssertEquals("Output is blank", "", Manager.Output);
		}

		public void TestDataContextParameterWorksFineForSupportIncident()
		{
			Manager.DataContextCode = "SupportIncident";

			Manager.Macro = "<JobNumber><ABC123>";
			var result = Manager.Evaluate();

			AssertEquals("DataContext is invalid", result);
			AssertEquals("Output is blank", "", Manager.Output);
		}

		public void TestEvaluateMacro_ExcessMaxLengthOfOutput()
		{
			var jobNumberMacro = "<JobNumber>";
			ZString jobNumber = "S1234567890123456789";
			ShipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = jobNumber;
			Manager.Macro = new string('X', Manager.OutputInfo.MaxLength - jobNumberMacro.Length - 1) + '\n' + jobNumberMacro;
			Manager.Evaluate();

			CombineAssertions("Output Truncated", () =>
			{
				AssertEquals(Manager.OutputInfo.MaxLength, Manager.Output.Length);
				Assert(Manager.Output.EndsWith("\r\nS123456789"));
			});
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var dataContext = new DataContextValue(nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob));
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var manager = new MacroEvaluatorManager(Factory, dummy);

			return manager;
		}

		MacroEvaluatorManager Manager
		{
			get
			{
				if (manager == null)
				{
					manager = new MacroEvaluatorManager(Factory, ShipmentBO as IDocumentSupportable);
				}

				return manager;
			}
		}
		MacroEvaluatorManager manager;

		#endregion
	}
}
