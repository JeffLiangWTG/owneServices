#if DEBUG
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class CompileTimeCheckBindingMemberCodeDomSerializerTests : TestCase
	{
		public void TestGenerateCodeForNullBindingMember()
		{
			CodePrimitiveExpression expr = (CodePrimitiveExpression)SerializeExpr(null);
			AssertNull(expr.Value);
		}

		public void TestGenerateCodeForEmptyBindingMember()
		{
			CodePrimitiveExpression expr = (CodePrimitiveExpression)SerializeExpr("");
			AssertNull(expr.Value);
		}

		public void TestGenerateCodeForDotBindingMember()
		{
			CodeCastExpression castToSource = (CodeCastExpression)SerializeExpr(".");
			AssertEquals(typeof(TestDataSource).FullName, castToSource.TargetType.BaseType);
		}

		public void TestGenerateCodeForNormalProperty()
		{
			CodePropertyReferenceExpression rootExpr = (CodePropertyReferenceExpression)SerializeExpr("PK");
			AssertEquals("PK", rootExpr.PropertyName);

			CodeCastExpression castToSource = (CodeCastExpression)rootExpr.TargetObject;
			AssertEquals(typeof(TestDataSource).FullName, castToSource.TargetType.BaseType);
		}

		public void TestGenerateCodeWhenControlPropertyTypeNull()
		{
			// expect no null ref exception
			CodePropertyReferenceExpression rootExpr = (CodePropertyReferenceExpression)SerializeExpr(null, "PK");
			AssertEquals("PK", rootExpr.PropertyName);
		}

		public void TestGenerateCodeForChildRelation()
		{
			CodePropertyReferenceExpression rootExpr =
				(CodePropertyReferenceExpression)SerializeExpr("ChildRelation.Prop");
			AssertEquals("Prop", rootExpr.PropertyName);

			CodeCastExpression castExpr = (CodeCastExpression)rootExpr.TargetObject;
			AssertEquals(typeof(TestListEntity).FullName, castExpr.TargetType.BaseType);

			CodePropertyReferenceExpression syncrootProperty =
				(CodePropertyReferenceExpression)castExpr.Expression;
			AssertEquals("SyncRoot", syncrootProperty.PropertyName);

			CodeCastExpression listCastExpr = (CodeCastExpression)syncrootProperty.TargetObject;
			AssertEquals(typeof(IList).FullName, listCastExpr.TargetType.BaseType);

			CodePropertyReferenceExpression listProperty =
				(CodePropertyReferenceExpression)listCastExpr.Expression;
			AssertEquals("ChildRelation", listProperty.PropertyName);

			CodeCastExpression castToSource = (CodeCastExpression)listProperty.TargetObject;
			AssertEquals(typeof(TestDataSource).FullName, castToSource.TargetType.BaseType);
		}

		public void TestGenerateCodeForChildRelationUsingWrappedProperties()
		{
			CodePropertyReferenceExpression expression =
				(CodePropertyReferenceExpression)SerializeExpr("Self+ChildRelation.Prop.SecondProp");
			string expressionString = Generate(expression);
			AssertEquals(
				"((CargoWise.ComponentModel.Testing.CompileTimeCheckBindingMemberCodeDomSerializerTests.TestListEntity)(((System.Collections.IList)(((CargoWise.ComponentModel.Testing.CompileTimeCheckBindingMemberCodeDomSerializerTests.TestDataSource)(null)).Self.ChildRelation)).SyncRoot)).Prop.SecondProp",
				expressionString);
		}

		public void TestGenerateCodeForListRelation()
		{
			CodePropertyReferenceExpression rootExpr =
				(CodePropertyReferenceExpression)SerializeExpr("List.Prop");
			AssertEquals("Prop", rootExpr.PropertyName);

			CodeCastExpression castExpr = (CodeCastExpression)rootExpr.TargetObject;
			AssertEquals(typeof(TestListEntity).FullName, castExpr.TargetType.BaseType);

			CodePropertyReferenceExpression syncrootProperty =
				(CodePropertyReferenceExpression)castExpr.Expression;
			AssertEquals("SyncRoot", syncrootProperty.PropertyName);

			CodeCastExpression listCastExpr = (CodeCastExpression)syncrootProperty.TargetObject;
			AssertEquals(typeof(IList).FullName, listCastExpr.TargetType.BaseType);

			CodePropertyReferenceExpression listProperty =
				(CodePropertyReferenceExpression)listCastExpr.Expression;
			AssertEquals("List", listProperty.PropertyName);

			CodeCastExpression castToSource = (CodeCastExpression)listProperty.TargetObject;
			AssertEquals(typeof(TestDataSource).FullName, castToSource.TargetType.BaseType);
		}

		public void TestGenerateCodeForIndexedCollection()
		{
			CodePropertyReferenceExpression rootExpr =
				(CodePropertyReferenceExpression)SerializeExpr("IndexedList.Prop");
			AssertEquals("Prop", rootExpr.PropertyName);

			CodeCastExpression castExpr = (CodeCastExpression)rootExpr.TargetObject;
			AssertEquals(typeof(TestListEntity).FullName, castExpr.TargetType.BaseType);

			CodePropertyReferenceExpression syncrootProperty =
				(CodePropertyReferenceExpression)castExpr.Expression;
			AssertEquals("SyncRoot", syncrootProperty.PropertyName);

			CodeCastExpression listCastExpr = (CodeCastExpression)syncrootProperty.TargetObject;
			AssertEquals(typeof(IList).FullName, listCastExpr.TargetType.BaseType);

			CodePropertyReferenceExpression listProperty =
				(CodePropertyReferenceExpression)listCastExpr.Expression;
			AssertEquals("IndexedList", listProperty.PropertyName);

			CodeCastExpression castToSource = (CodeCastExpression)listProperty.TargetObject;
			AssertEquals(typeof(TestDataSource).FullName, castToSource.TargetType.BaseType);
		}

		public void TestGenerateCodeForCollectionAsTopLevelBindTo()
		{
			CompileTimeCheckBindingMember bindToObject =
				new CompileTimeCheckBindingMember(typeof(TestDataSourceCollection),
				typeof(TestDataSource), ".");
			CompileTimeCheckBindingMemberCodeDomSerializer serializer = new CompileTimeCheckBindingMemberCodeDomSerializer();

			CodeExpressionStatement exprStatement = (CodeExpressionStatement)serializer.Serialize(null, bindToObject);
			CodeCastExpression castToCtrlValueType = (CodeCastExpression)((CodeMethodInvokeExpression)exprStatement.Expression).Parameters[0];

			CodeCastExpression castToEntityExpr =
				(CodeCastExpression)castToCtrlValueType.Expression;
			AssertEquals(typeof(TestDataSource).FullName, castToEntityExpr.TargetType.BaseType);

			CodePrimitiveExpression initialNull = (CodePrimitiveExpression)castToEntityExpr.Expression;
			AssertNull("Should start with null", initialNull.Value);
		}

		public void TestGenerateCodeForEmpty()
		{
			CompileTimeCheckBindingMemberCodeDomSerializer serializer = new CompileTimeCheckBindingMemberCodeDomSerializer();
			CodeExpressionStatement statement = (CodeExpressionStatement)serializer.Serialize(null, CompileTimeCheckBindingMember.Empty);
			AssertEquals(null, statement);
		}

		#region Test Data Components

		public class TestDataSourceCollection : ArrayList
		{
			public new TestDataSource this[int i]
			{ get { return null; } }
		}

		public abstract class TestDataSource
		{
			public abstract int PK { get; }
			public abstract TestListEntityCollection ChildRelation { get; }
			public TestDataSource Self { get { return this; } }
			public abstract TestListEntityCollection List { get; }
			public TestEntityCollection IndexedList
			{ get { return new TestEntityCollection(); } }
		}

		public abstract class TestListEntity : Component
		{
			public abstract int PK { get; }

			public abstract int FK { get; }

			public abstract string Prop { get; }
		}

		public class TestListEntityCollection : ArrayList
		{
			public new TestListEntity this[int i]
			{ get { return null; } }
		}

		public class TestEntityCollection : ArrayList
		{
			public new TestListEntity this[int i]
			{ get { return null; } }
		}

		#endregion

		#region Implementation

		static string Generate(CodeExpression expression)
		{
			System.IO.StringWriter writer = new System.IO.StringWriter();
			new Microsoft.CSharp.CSharpCodeProvider().GenerateCodeFromExpression(expression, writer, new System.CodeDom.Compiler.CodeGeneratorOptions());
			return writer.GetStringBuilder().ToString();
		}

		CompileTimeCheckBindingMemberCodeDomSerializer Serializer
		{
			get
			{
				if (serializer == null)
				{
					serializer = new CompileTimeCheckBindingMemberCodeDomSerializer();
				}
				return serializer;
			}
		}
		CompileTimeCheckBindingMemberCodeDomSerializer serializer;

		CodeExpression SerializeExpr(string bindto)
		{ return SerializeExpr(typeof(string), bindto); }

		CodeExpression SerializeExpr(Type controlPropertyType, string bindingMember)
		{
			CompileTimeCheckBindingMember bindingMemberCheck = new CompileTimeCheckBindingMember(
				typeof(TestDataSource), controlPropertyType, bindingMember);
			CodeExpressionStatement exprStatement = (CodeExpressionStatement)Serializer.Serialize(null, bindingMemberCheck);
			CodeCastExpression castToCtrlValueType =
				(CodeCastExpression)((CodeMethodInvokeExpression)exprStatement.Expression).Parameters[0];
			return castToCtrlValueType.Expression;
		}

		#endregion
	}
}
#endif
