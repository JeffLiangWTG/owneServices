using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Macros;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Macro.UDFToMCRConverter.Test
{
	class UDFToMCRConverterTest : TestCaseWithFactory
	{
		public void TestWithShipment()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consol = Factory.New<IForwardingConsol>();
			consol.AddShipment(shipment);
			shipment.JS_UniqueConsignRef = "S0001000";

			TestAll(ShipmentTestCases, (IBusiness)shipment);
		}

		public void TestAll(IEnumerable<Tuple<string, string>> testCases, IBusiness business)
		{
			CombineAssertions(() =>
			{
				foreach (var testData in testCases)
				{
					TestConversion(business, testData.Item1, testData.Item2);
				}
			});
		}

		public void TestWithDummyWithWorkflow()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Number = 10;
			dummy.Z0_Date = ZDateTime.Now;
			dummy.Z0_Description = "This is a test decription";
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Task 1";

			TestAll(ValueProviderTestCases, dummy);
			TestAll(ComplexTestCases, dummy);
		}

		public void TestWithShipmentAndConsol()
		{
			var shipment = Factory.New<IForwardingShipment>();
			var consol = Factory.New<IForwardingConsol>();
			consol.AddShipment(shipment);
			shipment.JS_UniqueConsignRef = "S0001000";
			shipment.JS_PackingMode = "FCL";
			consol.JK_ConsolMode = "AAA";
			shipment.JS_GoodsDescription = "AAA";

			var macroContext = new AntlrMacroContextForTest
			{
				Parent = shipment,
				ParentType = shipment.GetType(),
				Libraries = Libraries.ToArray()
			};
			macroContext.Variables.Add("env", (new MasterFiles.Business.Macros.Environment(), typeof(MasterFiles.Business.Macros.Environment)));
			macroContext.Variables.Add("Consol", (consol, consol.GetType()));

			CombineAssertions(() =>
			{
				foreach (var testData in MultipleObjectTestCases)
				{
					TestConversion(macroContext, testData.Item1, testData.Item2);
				}
			});
		}

		public void TestConversion(IBusiness bizo, string udf, string mcr)
		{
			var macroContext = new AntlrMacroContextForTest
			{
				Parent = bizo,
				ParentType = bizo.GetType(),
				Libraries = Libraries.ToArray()
			};
			macroContext.Variables.Add("env", (new MasterFiles.Business.Macros.Environment(), typeof(MasterFiles.Business.Macros.Environment)));
			TestConversion(macroContext, udf, mcr);
		}
		public void TestConversion(IAntlrMacroContext macroContext, string udf, string mcr, bool? result = null)
		{
			try
			{
				AssertConvertion(macroContext, udf, mcr, result);
			}
			catch (Exception ex)
			{
				throw new Exception($"Conversion failed for: {udf}", ex);
			}
		}

		static void AssertConvertion(IAntlrMacroContext macroContext, string udf, string mcr, bool? result)
		{
			var converter = new UDFToMCRConverter();
			AssertEquals(mcr.Replace(" ", string.Empty), converter.Convert(udf, macroContext).Replace(" ", string.Empty));

			var mcrResult = GetMCRMacroResult(mcr, macroContext.Scope);
			var expression = ObjectFactory.Get<ITextMacroProcessor>().Replace(udf, GetBusinessObjects(macroContext.Scope), useJs: false);

			if (mcrResult is bool)
			{
				var udfResult = EvaluateBooleanExpression(expression);
				AssertEquals($"UDF and MCR should evaluate to the same result. UDF: {udf} - MCR: {mcr}", udfResult, mcrResult);
				if (result.HasValue)
				{
					AssertEquals($"Macro should evaluate to {result}. MCR: {mcr}", result.Value, mcrResult);
				}
			}
			else
			{
				AssertEquals($"UDF and MCR should evaluate to the same result. UDF: {udf} - MCR: {mcr}", expression, mcrResult.ToString());
			}
		}

		static IBusiness[] GetBusinessObjects(IMacroScope scope)
		{
			List<IBusiness> businesses = new List<IBusiness>();
			businesses.Add((IBusiness)scope.Data);
			foreach (var v in scope)
			{
				if(v.Value is IBusiness b)
				{
					businesses.Add(b);
				}
			}
			return businesses.ToArray();
		}

		#region Run Macro Helpers
		public static bool EvaluateBooleanExpression(string expression)
		{
			var boolResult = expression.EvaluateDocEngineExpressionNoJS();
			if (boolResult.IsLeft)
			{
				return expression == "Y" || (expression != "N" && bool.Parse(expression));
			}
			else
			{
				return boolResult.Right;
			}
		}

		static object GetMCRMacroResult(string macro, IMacroScope scope)
		{
			var context = new MacroEvaluationContext();

			foreach (var library in Libraries)
			{
				context.Import(library);
			}

			IMacroExpression macroExpr = new MacroExpression(macro, context, null);

			if (macroExpr.Handler == null)
			{
				var errors = string.Join(", ", macroExpr.Errors.Select(err => err.Message));

				Fail(string.Concat("Macro could not be compiled. The following errors have occurred: ", errors));
			}

			//scope.SetVariable("env", new MasterFiles.Business.Macros.Environment());

			var result = macroExpr.Evaluate(scope);

			var notifications = macroExpr.Errors
				.Select(err => err.Message)
				.ToArray();
			if (notifications.Any())
			{
				Fail(string.Join("\n\r", notifications));
			}
			return result;
		}

		protected static IEnumerable<IMacroLibrary> Libraries
		{
			get { yield return new StandardLibrary(); }
		}
		#endregion Run Macro Helpers

		#region Test Data
		public static IEnumerable<Tuple<string, string>> ValueProviderTestCases
		{
			get
			{
				yield return new Tuple<string, string>("<Contains(\"string\", \"subString\")>", "Contains(\"string\", \"subString\", false)");
				yield return new Tuple<string, string>("<Contains(\"<Z0_Description>\", \"A\")>", "Contains(Z0_Description, \"A\", false)");
				yield return new Tuple<string, string>("<Contains(\"<Z0_Description>\", \"<CompanyCode>\")>", "Contains(Z0_Description, @env.Company.Code, false)");
				yield return new Tuple<string, string>("<Substring(\"<Z0_Description>\", 2)>", "Z0_Description.Substring(2)");
				yield return new Tuple<string, string>("<Substring(\"<Z0_Description>\", 0, 3)>", "Z0_Description.Substring(0, 3)");
				//yield return new Tuple<string, string>("<Left(\"<Z0_Description>\", 0, 3)>", "Substring(Z0_Description, 0, 3)");

				//yield return new Tuple<string, string>("<ChangeCase(\"<Z0_Description>\", \"U\")>", "ToUpper(Z0_Description)");
				//yield return new Tuple<string, string>("<ChangeCase(\"<Z0_Description>\", \"u\")>", "ToUpper(Z0_Description)");
				//yield return new Tuple<string, string>("<ChangeCase(\"<Z0_Description>\", \"L\")>", "ToLower(Z0_Description)");
				//yield return new Tuple<string, string>("<ChangeCase(\"<Z0_Description>\", \"l\")>", "ToLower(Z0_Description)");

				//yield return new Tuple<string, string>("<Add(Z0_Number, \"5\")>", "(Z0_Number + 5)");
				//yield return new Tuple<string, string>("<Add(Z0_Number, \"5\", \"2\")>", "(Z0_Number + 5 + 2)");
				//yield return new Tuple<string, string>("<Multiply(\"<Add(\"<Z0_Number>\" ,\"2\")>\", \"3\")>", "((Z0_Number + 2) * 3)");
				//yield return new Tuple<string, string>("<Multiply(\"<Z0_Number>\" ,\"2\", \"3\")>", "(Z0_Number * 2 * 3)");
				//yield return new Tuple<string, string>("<Divide(\"<Z0_Number>\" ,\"2\", \"3\")>", "(Z0_Number / 2 / 3)");
				//yield return new Tuple<string, string>("<Subtract(\"<Z0_Number>\" ,\"2\", \"3\")>", "(Z0_Number - 2 - 3)");

				//yield return new Tuple<string, string>("<If(\"<Z0_Number>\" == \"2\", \"3\", \"0\")>", "If(Z0_Number == 2, 3, 0)");
				//yield return new Tuple<string, string>("<If(\"<Z0_Number>\" == \"2\", \"<Z0_Description>\", \"none\")>", "If(Z0_Number == 2, Z0_Description, \"none\")");

				//yield return new Tuple<string, string>("<NOW>", "Now");

				//yield return new Tuple<string, string>("<DataDiff(\"<Z0_Date>\",\"2023-02-25\", \"DAYS\")>", "");
				//yield return new Tuple<string, string>("<UTCTimeOffset>", "");
				//yield return new Tuple<string, string>("<ConvertUTCDateTimeToLocal(\"<Z0_Date\")>", "ConvertUTCDateTimeToLocal(Z0_Date)");
				//yield return new Tuple<string, string>("<DateTimeAsString(\"<Z0_Date\")>, '{DateFormatString}' [,Calendar:{CalendarInfo}] [,{ConvertFromUtcToLocal}])>", "");

				//yield return new Tuple<string, string>("<AddDurationToDate(\"<Z0_Date\", \"DAYS\", \"2\")>)>", "AddDurationToDate(Z0_Date,\"DAYS\", 2)>");

				//yield return new Tuple<string, string>("<HasEvent(\"Z00\")>", "HasEvent(\"Z00\")");

				//yield return new Tuple<string, string>("<GetEventReferenceValue()>", "");

				//yield return new Tuple<string, string>("<GetEventLastDateTime>", "");
				//yield return new Tuple<string, string>("<GetUserEmailWhoRaisedEvent>", "");
				//yield return new Tuple<string, string>("<GetCustomField>", "");
				//yield return new Tuple<string, string>("<GetCustomFieldCodeDescription>", "");
				//yield return new Tuple<string, string>("<GetCustomFieldCodeDescriptionWithType>", "");
				//yield return new Tuple<string, string>("<GetCustomFieldWithType>", "");

				//yield return new Tuple<string, string>("<BranchCode>", "@env.Branch.Code");
				//yield return new Tuple<string, string>("<BranchProxy>", "@env.Branch.Organization.PK");
				//yield return new Tuple<string, string>("<BranchProxyCode>", "@env.Branch.Organization.Code");
				//yield return new Tuple<string, string>("<CompanyCode>", "@env.Company.Code");
				//yield return new Tuple<string, string>("<CompanyCountry>", "@env.Company.Country.Name");

				//yield return new Tuple<string, string>("<CompanyCountryCode>", "@env.Company.Country.Code");
				//yield return new Tuple<string, string>("<CompanyCurrencyCode>", "");

				//yield return new Tuple<string, string>("<CompanyName>", "@env.Company.Name");
				//yield return new Tuple<string, string>("<CompanyPK>", "");
				//yield return new Tuple<string, string>("<CompanyProxyCode>", "@env.Company.Organization.Code");
				//yield return new Tuple<string, string>("<CurrentBranch>", "@env.Branch.PK");
				//yield return new Tuple<string, string>("<CurrentCompany>", "@env.Company.PK");
				//yield return new Tuple<string, string>("<CurrentDepartment>", "@env.Department.PK");
				//yield return new Tuple<string, string>("<LoginCode>", "@env.CurrentUser.Code");
				//yield return new Tuple<string, string>("<LoginName>", "@env.CurrentUser.LoginName"); //TODO
				//yield return new Tuple<string, string>("<<LoginFullName>", "@env.CurrentUser.Name");
				//yield return new Tuple<string, string>("<LoginTitle>", "@env.CurrentUser.Title"); //TODO
				//yield return new Tuple<string, string>("<LoginStaffCertificateNumber>", "");
				//yield return new Tuple<string, string>("<Absolute(\"Z0_Number\")>", "Absolute(Z0_Number)");
			}
		}

		public static IEnumerable<Tuple<string, string>> ComplexTestCases
		{
			get
			{
				yield return new Tuple<string, string>(
					@"<WorkflowItems.Find(""{IsTask}"" == ""Y"").P9_Description>",
					"WorkflowItems.First({IsTask == true}).P9_Description");
				yield return new Tuple<string, string>(
					@"""<WorkflowItems.Find(""{Company.GC_Code}""==""<CompanyCode>""&&""{P9_ShareTasksForAllCompanies}""==""Y"").P9_ShareTasksForAllCompanies>"" == ""Y""",
					"WorkflowItems.First({Company.GC_Code==@env.Company.Code && P9_ShareTasksForAllCompanies==true}).P9_ShareTasksForAllCompanies == true");
			}
		}

		public static IEnumerable<Tuple<string, string>> ShipmentTestCases
		{
			get
			{
				yield return new Tuple<string, string>(
					"\"<Contains(\"<Substring(\"<ServiceLevel.RS_Description>\",4)>\",\"Door\")>\"==\"Y\" || \"<Consignee.RequiredDocuments.Find('{EQ_DocType}'=='CAD').EQ_DocCategory>\" == \"SCL\"",
					"Contains(ServiceLevel.RS_Description.Substring(4), \"Door\", false) == true || Consignee.RequiredDocuments.First({EQ_DocType == 'CAD'}).EQ_DocCategory == \"SCL\"");
				yield return new Tuple<string, string>(
					@"""<Consols.JK_ConsolMode>"" !=""FCL"" && ""<WorkflowItems.Find(""{IsTask}"" == ""Y"" && ""{P9_Description}"" == ""Send: Booking Confirmation"").P9_Status>"" !=""CLS"" && ""<Job.LocalCharges.OH_Code>"" != ""UNMATCHED""",
					@"Consols.First().JK_ConsolMode !=""FCL"" && WorkflowItems.First({IsTask == true && P9_Description =="" Send: Booking Confirmation""}).P9_Status !=""CLS"" && Job.LocalCharges.OH_Code != ""UNMATCHED""");

				//yield return new Tuple<string, string>("<HasEvent(\"<Consignee>\",\"Z00\")>", "Consignee.HasEvent(\"Z00\")");
			}
		}

		public static IEnumerable<Tuple<string, string, bool>> MultipleObjectTestCases
		{
			get
			{
				yield return new Tuple<string, string, bool>("\"<JS_PackingMode>\" == \"<JK_ConsolMode>\"", "JS_PackingMode == @Consol.JK_ConsolMode", false);
				yield return new Tuple<string, string, bool>("\"<JS_GoodsDescription>\" == \"<JK_ConsolMode>\"", "JS_GoodsDescription == @Consol.JK_ConsolMode", true);
			}
		}
		#endregion Test Data

		class AntlrMacroContextForTest : IAntlrMacroContext
		{
			public object Parent { get; set; }

			public Type ParentType { get; set; }

			public IMacroLibrary[] Libraries { get; set; }

			public Dictionary<string, (object, Type)> Variables { get; } = new Dictionary<string, (object, Type)>();

			IMacroScope scope;

			public IMacroScope Scope
			{
				get
				{
					if (scope == null)
					{
						scope = new MacroScope(Parent);
						if (Variables != null)
						{
							foreach (var entry in Variables)
							{
								scope.SetVariable(entry.Key, entry.Value.Item1);
							}
						}
					}
					return scope;
				}
			}

			public Func<string, string> ErrorMessageExtender { get; }

			public void Dispose()
			{
				throw new NotImplementedException();
			}
		}
	}
}
