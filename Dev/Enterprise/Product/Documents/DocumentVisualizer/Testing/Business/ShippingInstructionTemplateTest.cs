using System.Collections.Generic;
using System.Globalization;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ShippingInstructionTemplateTest : TestCase
	{
		public void TestChargesHandler_Prepaid()
		{
			AssertChargesHandler(true, false, "[{\"Category\":{\"Code\":\"OPC\", \"Description\":\"Origin Port Charge\"}, \"PaymentMethod\":{\"Code\":\"PPD\", \"Description\":\"Prepaid\"}}]");
		}

		public void TestChargesHandler_Collect()
		{
			AssertChargesHandler(false, true, "[{\"Category\":{\"Code\":\"OPC\", \"Description\":\"Origin Port Charge\"}, \"PaymentMethod\":{\"Code\":\"CCX\", \"Description\":\"Collect\"}}]");
		}

		public void TestChargesHandler_None()
		{
			AssertChargesHandler(false, false, "[]");
		}

		public void TestChargesHandler_PrepaidAndCollect()
		{
			AssertChargesHandler(true, true, "[]");
		}

		void AssertChargesHandler(bool isPrepaid, bool isCollect, string expected)
		{
			var handler = string.Format(CultureInfo.InvariantCulture,
				"{{ Category = @originPortChargeCode, IsPrepaid = {0}, IsCollect = {1} }}.Eval(@chargeHandler)",
				isPrepaid.ToString().ToLower(), isCollect.ToString().ToLower());

			var macro =
@"def consol = @data;
def freightChargeCode = ""FRT"";
def originPortChargeCode = ""OPC"";
def originHaulageChargeCode = ""OHC"";
def destinationPortChargeCode = ""DPC"";
def destinationHaulageChargeCode = ""DHC"";

def chargePrepaidCode = ""PPD"";
def chargeCollectCode = ""CCX"";

def chargeCategoryDescriptions = { FRT = ""Freight Charge"", OPC = ""Origin Port Charge"", OHC = ""Origin Haulage Charge"", DPC = ""Destination Port Charge"", DHC = ""Destination Haulage Charge"" };
def chargePaymentMethodsDescriptions = { PPD = ""Prepaid"", CCX = ""Collect"" };

def chargeHandler =
{
   def params = @data;
   def charge = @consol.PaymentHandlingInstructionCollection.First({Category.Code == @params[""Category""]});
   @consol.PaymentHandlingInstructionCollection.Remove(@charge);
   def method = (if (@params[""IsPrepaid""] + @params[""IsCollect""]) == 1 then (if @params[""IsPrepaid""] then @chargePrepaidCode else @chargeCollectCode) else none);
   def paymentHandlingInstruction = (if @method != none then { Category = { Code = @params[""Category""], Description = @chargeCategoryDescriptions[""<@params[""Category""]>""] }, PaymentMethod = { Code = @method, Description = @chargePaymentMethodsDescriptions[""<@method>""] }} else none);
   (if @paymentHandlingInstruction != none then @consol.PaymentHandlingInstructionCollection.Create(@paymentHandlingInstruction) else none);
};
" + handler;

			var expr = macro.With<StandardLibrary>().CreateExpression();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var data = shipment.MakeDynamic();

			using (var scope = new MacroScope(data))
			{
				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());

				var name = nameof(shipment.PaymentHandlingInstructionCollection);

				var paymentHandlingInstruction = data.GetDynamicProperty(name);

				var elements = (IEnumerable<object>)((IMacroMapValueProvider)paymentHandlingInstruction).ToMacroMapValue();

				AssertMultilineASCIIEquals(name, expected, elements.ToJSON());
			}
		}
	}
}