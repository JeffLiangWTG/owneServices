using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DynamicDataTest : DynamicDataTestCase
	{
		public void TestEqualWithNotDynamicObject()
		{
			const string macro = "@data.ContainerCollection.Select({Seal}).First() == \"SealNumber\"";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						Seal = "SealNumber"
					}
				});

			AssertMacroExprResult(shipment, macro, true);
		}

		public void TestDistinctWithNotDynamicObject()
		{
			const string macro = "@data.ContainerCollection.Select({Seal}).Distinct()";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						Seal = "SealNumber"
					},
					new Container
					{
						Link = 2,
						Seal = "SealNumber"
					}
				});
			var data = shipment.MakeDynamic();
			var expr = macro.With<StandardLibrary>()
				.And<MetaDataLibrary>().CreateExpression();

			var result = expr.Evaluate(data) as IEnumerable;
			AssertNotNull(result);

			var collection = result.Cast<object>();
			AssertEquals(1, collection.Count());
			AssertEquals("SealNumber", ((IDynamicData)collection.First()).Value);
		}

		public void TestContainsWithNotDynamicObject()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						Seal = "SealNumber"
					}
				});

			var macro = "@data.ContainerCollection.Select({Seal}).Contains(\"SealNumber\")";
			AssertMacroExprResult(shipment, macro, true);

			macro = "@data.ContainerCollection.Select({Seal}).Contains(\"TestWrongNumber\")";
			AssertMacroExprResult(shipment, macro, false);
		}

		public void TestMacroContainsWithValueCollection()
		{
			AssertMacroExprResult("FCL", "[\"FCL\", \"LCL\", \"GRP\"].Contains(@data)", true);
		}

		public void TestGetPropertyValue()
		{
			const string macro = "\"<Shipper.Name>\"";

			var consol = new Consol();

			var shipper = new Organization();
			shipper.Name = "Maersk";

			consol.Shipper = shipper;

			AssertMacroExprResult(consol, macro, "Maersk");
		}

		public void TestGetValueProperty()
		{
			const string macro = "\"<Value>\"";
			var registrationNumber = new RegistrationNumber { Value = "TEST" };

			AssertMacroExprResult(registrationNumber, macro, "TEST");
		}

		public void TestSetPropertyValue_ZCodeMappedZString()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				OrganizationCode = "xxx"
			};

			var data = address.MakeDynamic(null, null, new UXmlTypeConverter());
			var orgCode = data.GetDynamicProperty(nameof(address.OrganizationCode));

			orgCode.SetValue("aaa");

			AssertEquals("OrganizationCode value", "aaa", orgCode.ToString());
		}

		public void TestSetPropertyValue_EmptyStringOnNullableString()
		{
			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Contact = "Michelle Dobyne"
			};

			var dynamicAddress = address.MakeDynamic(null, null, new UXmlTypeConverter());
			var dynamicContact = dynamicAddress.GetDynamicProperty("Contact");

			dynamicContact.SetValue(string.Empty);

			AssertEquals("Value of the Contact",
				new ZString?(string.Empty), dynamicContact.Value);
		}

		public void TestIsOverridden_WithZTypes()
		{
			const string macro = "\"<Z0_Description>\"";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "XXX";

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			expr.Evaluate(data);

			AssertEquals("IsOverridden", false, data.IsOverriddenIncludingChildren);

			data.Properties[DummyBusinessObject.Schema.Z0_Description].SetValue(new ZString("XXX"));

			AssertEquals("set the same value should set the override to true", false, data.IsOverriddenIncludingChildren);
		}

		public void TestIsOverridden_WithZTypes_SetConvertible()
		{
			const string macro = "\"<Z0_Description>\"";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "XXX";

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			expr.Evaluate(data);

			AssertEquals("IsOverridden", false, data.IsOverriddenIncludingChildren);

			data.Properties[DummyBusinessObject.Schema.Z0_Description].SetValue("XXX");

			AssertEquals("set the same value should set the override to true", false, data.IsOverriddenIncludingChildren);
		}

		public void TestCallClosureWithMacroMapParameter()
		{
			const string macro =
				@"def prop2 = false;
def customField = @data.CustomField(""CustomField"", false);
def handler =
{
""Prop1; property call: [<@data.Prop1>, <typeof(@data.Prop1)>], indexer: [<@data[""Prop1""]>, <typeof(@data[""Prop1""])>]
Prop2; property call: [<@data.Prop2>, <typeof(@data.Prop2)>], indexer: [<@data[""Prop2""]>, <typeof(@data[""Prop2""])>]
"";
};
{ Prop1 = ""AAA"", Prop2 = @customField }.Eval(@handler);";

			using (var scope = new MacroScope(new object().MakeDynamic()))
			{
				var expr = macro
					.With<StandardLibrary>()
					.And<DataLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("result",
					@"Prop1; property call: [AAA, System.String], indexer: [AAA, System.String]
Prop2; property call: [N, CargoWise.Types.ZBool], indexer: [N, CargoWise.Types.ZBool]",
					Convert.ToString(result));
			}
		}

		public void TestDynamicDataToMap()
		{
			var contact = new Contact
			{
				Number = 123,
				Name = "Roger",
				Car = new Car
				{
					Make = "Porshia",
					Year = 1999
				}
			};

			var data = contact.MakeDynamic();

			AssertMultilineASCIIEquals("map", "{}", data.ToMap().ToJSON());

			using (var scope = new MacroScope(data))
			{
				var expr = "\"<Number>, <Name>, <Car.Make>, <Car.Year>\""
					.With<StandardLibrary>()
					.And<DataLibrary>()
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertMultilineASCIIEquals("result", "123, Roger, Porshia, 1999", Convert.ToString(result));
				AssertMultilineASCIIEquals("map",
					@"{""Number"":123, ""Name"":""Roger"", ""Car"":{""Make"":""Porshia"", ""Year"":1999}}",
					data.ToMap().ToJSON());
			}
		}

		#region Macro Operator Overloading

		public void TestZTypePropertyComparison_ZInt()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Number + " > 2";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(macro, false, result);
		}

		public void TestZTypePropertiesComparison_ZInt()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Number + " > " + DummyBusinessObject.Schema.Z0_Number;

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(macro, false, result);
		}

		public void TestZTypePropertyComparison_ZString()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Description + " != \"\"";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "";

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(DummyBusinessObject.Schema.Z0_Description + " == \"\"", false, result);
		}

		public void TestZTypePropertiesComparison_ZString()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Description + " != " + DummyBusinessObject.Schema.Z0_Description;

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(macro, false, result);
		}

		public void TestZTypePropertyComparison_ZDecimal()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Decimal + " > 2.2";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Decimal = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(DummyBusinessObject.Schema.Z0_Decimal + " > 2", false, result);
		}

		public void TestZTypePropertyComparison_ZDecimal_ZInt()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Decimal + " > 2.2";

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Decimal = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(DummyBusinessObject.Schema.Z0_Decimal + " > 2", false, result);
		}

		public void TestZTypePropertiesComparison_ZDecimal()
		{
			const string macro = DummyBusinessObject.Schema.Z0_Decimal + " > " + DummyBusinessObject.Schema.Z0_Decimal;

			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Decimal = 1;

			var data = dummy.MakeDynamic();

			var expr = macro.CreateExpression();

			var result = expr.Evaluate(data);

			AssertMultilineASCIIEquals("no errors", "",
				string.Join("\r\n", expr.Errors.Select(err => err.Message)));

			AssertEquals(macro, false, result);
		}

		#endregion

		#region MetaData

		public void TestSetValueMacro_DynamicData()
		{
			var contact = new
			{
				Name = new ZString("John")
			};

			var data = contact.MakeDynamic();

			const string macro = "@data.Name.SetValue(CustomField(\"CustomField\", \"Bob\"))";

			using (var scope = new MacroScope(data))
			{
				var expr = macro
					.With<DocumentLibrary>()
					.And<DataLibrary>().CreateExpression();

				expr.Evaluate(scope);

				AssertMultilineASCIIEquals("no errors", "", expr.ToFormatString());
				AssertEquals("Name", "Bob", data.GetDynamicProperty(nameof(contact.Name)).Value);
			}
		}

		#endregion

		#region Implementation

		void AssertMacroExprResult(object obj, string macro, object expectedResult, string message = "")
		{
			var data = obj.MakeDynamic();
			var expr = macro.With<StandardLibrary>()
				.And<MetaDataLibrary>().CreateExpression();
			var result = expr.Evaluate(data);

			AssertEquals(message, expectedResult, result);
		}

		string ToString(IDynamicData dynamicData, int indent = 0)
		{
			var builder = new StringBuilder();

			builder.AppendLine(dynamicData.Type.Name);

			builder.AppendLine(ToString(dynamicData.Properties, indent + 1));

			return builder.ToString();
		}

		string ToString(IEnumerable<KeyValuePair<string, IDynamicData>> properties, int indent)
		{
			var builder = new StringBuilder();

			foreach (var property in properties)
			{
				builder.Append(new string(' ', indent));

				if (indent > 0)
				{
					builder.Append("|-");
				}

				builder.Append(property.Key);
				builder.Append(" ");
				builder.AppendLine(ToString(property.Value, indent + 1));
			}

			return builder.ToString();
		}

		#endregion
	}
}