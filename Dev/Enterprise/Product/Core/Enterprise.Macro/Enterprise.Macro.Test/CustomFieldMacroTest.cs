using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Macro.Test
{
	sealed class CustomFieldMacroTest : TestCaseWithFactory
	{
		public void TestGetCustomFieldMacro()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_StringField");
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART1_ComboField");
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART2_ComboField");
			customPropertyCollection.Add(typeof(ZInt), "ZZZ_IntField");
			customPropertyCollection.Add(typeof(ZBool), "ZZZ_BoolField");

			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_StringField"] = "text",
				["ZZZ_PART1_ComboField"] = "aaa",
				["ZZZ_PART2_ComboField"] = "bbb",
				["ZZZ_IntField"] = 123,
				["ZZZ_BoolField"] = true
			};

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var expr = "GetCustomField(\"String Field\")".With(Context).CreateExpression();
			var res = expr.Evaluate(mock.Object);

			AssertEquals("returned string field value", "text", res);

			expr = "GetCustomField(\"Int Field\")".With(Context).CreateExpression();
			res = expr.Evaluate(mock.Object);

			AssertEquals("returned int field value", 123, res);

			expr = "GetCustomField(\"Bool Field\")".With(Context).CreateExpression();
			res = expr.Evaluate(mock.Object);

			Assert("returned boolean field value", (ZBool)res);

			expr = "GetCustomField(\"Combo Field\")".With(Context).CreateExpression();
			res = expr.Evaluate(mock.Object);

			AssertEquals("returned the first part of combo box field value", "aaa", res);
		}

		public void TestErrorThrownWhenNoSuchCustomField()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();

			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection);

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var expr = "GetCustomField(\"String Field\")".With(Context).CreateExpression();
			Assert("No Error when initialize Macro Expression", !expr.HasErrors());
			var res = expr.Evaluate(mock.Object);
			Assert("Error thrown for Custom Field 'String Field' does not exist", expr.HasErrors());
			AssertEquals("Evaluated result is an error message","The Custom Field with the name 'String Field' is not defined.", expr.ToFormatString());
		}

		public void TestGetCustomFieldMacroOnComboBox()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART1_ComboField");
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART2_ComboField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_PART1_ComboField"] = "aaa",
				["ZZZ_PART2_ComboField"] = "bbb"
			};

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var expr = "GetCustomField(\"Combo Field\", 2)".With(Context).CreateExpression();
			var res = expr.Evaluate(mock.Object);

			AssertEquals("returned the second part of combo box field value", "bbb", res);
		}

		public void TestGetCustomFieldMacroInvalidArgument()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART1_ComboField");
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART2_ComboField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_PART1_ComboField"] = "aaa",
				["ZZZ_PART2_ComboField"] = "bbb"
			};

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var expr = "GetCustomField(\"Combo Field\", 1)".With(Context).CreateExpression();
			Assert("No Error when initialize Macro Expression", !expr.HasErrors());
			var res = expr.Evaluate(mock.Object);
			Assert("Error thrown for invalid 2nd argument", expr.HasErrors());
			AssertEquals("Evaluated result is an error message", "Field identifier can only be 2.", expr.ToFormatString());
		}

		public void TestErrorThrownWhenNoSuchComboBoxCustomField()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var customPropertyCollection = GetCustomPropertyCollection();
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART1_ComboField");
			customPropertyCollection.Add(typeof(ZString), "ZZZ_PART2_ComboField");
			customPropertyCollection.Add(typeof(ZInt), "ZZZ_IntField");
			var customBusinessObj = new CustomBusinessObject(dummy, customPropertyCollection)
			{
				["ZZZ_PART1_ComboField"] = "aaa",
				["ZZZ_PART2_ComboField"] = "bbb",
				["ZZZ_IntField"] = 123,
			};

			var mock = new Mock<ICustomFieldProvider>();
			mock.Setup(p => p.GetCustomBusinessObject(It.IsAny<bool>())).Returns(customBusinessObj);

			var expr = "GetCustomField(\"String Field\", 2)".With(Context).CreateExpression();
			Assert("No Error when initialize Macro Expression", !expr.HasErrors());
			var res = expr.Evaluate(mock.Object);
			Assert("Error thrown for Custom Field 'String Field' does not exist", expr.HasErrors());
			AssertEquals("Evaluated result is an error message", "The Custom Field with the name 'String Field' is not defined.", expr.ToFormatString());

			var expr2 = "GetCustomField(\"Int Field\", 2)".With(Context).CreateExpression();
			Assert("No Error when initialize Macro Expression", !expr2.HasErrors());
			var res2 = expr2.Evaluate(mock.Object);
			Assert("Error thrown for incorrect syntax", expr2.HasErrors());
			AssertEquals("Evaluated result is an error message", "The Custom Field with the name 'Int Field' is not a Combo Box Custom Field.", expr2.ToFormatString());
		}

		IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[]
					{
						new CargoWiseOneStandardLibrary()
					}
					.CreateContext();
				}

				return context;
			}
		}
		IMacroEvaluationContext context;

		#region Implementation

		CustomPropertyCollectionImpl GetCustomPropertyCollection()
		{
			var values = new Dictionary<string, object>();

			return new CustomPropertyCollectionImpl(
				propertyName => values.TryGetValue(propertyName, out var value) ? value : null,
				(propertyName, value) =>
				{
					values[propertyName] = value;
					return true;
				});
		}

		#endregion
	}
}
