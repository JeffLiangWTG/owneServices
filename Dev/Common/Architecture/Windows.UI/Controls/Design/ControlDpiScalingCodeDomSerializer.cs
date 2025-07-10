using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Design
{
	/*
	!!! READ THIS BEFORE CONSIDERING MODIFYING THE SERIALIZER !!!

	If you're here because you need to change something related to DPI Scaling, please avoid changing the serializer as much as humanly possible. 
	It is both error prone and difficult, and it currently [seemingly] works. For the following cases you may do this:

	1. I want to change the serializer in a way that doesnt effect/have anything to do with scaling.
		Then dont modify the scaler. Make your own serializer, if you want scaling to still take effect subclass this (or have this subclass your class).

	2. My property isnt being serialized correctly
		Does it parent class use the ControlDpiScalingCodeDomSerializer (Yes for any ZControl, or KControl)
			Yes -> Is it always scaled the same way? (ie. always ScaleX, always ScaleY or always Unscaled)
				Yes -> Do you have access to the property's declaration?
					Yes -> Add the DpiStateAttribute to the property's declaration
					No -> Modify ExplicitDpiStates.cs, add your property when its relevant

				No -> Have the relevant class implement "IChooseHowMyPropertiesScale" and provide the correct value for serialization

			No -> Does it inherit from Control or Component?
				Yes -> Add the attribute to the class (see our KComponents for examples) [DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))], then for each of its properties go through Step #2
				No -> Consider writing a custom serializer, otherwise modify this

	3. I need to modify the way a whole class/tree is serialized
		Can it be done using the IChooseHowMyPropertiesScale interface (see KSplitContainer)?
			Yes -> Implement that and go ahead
			No -> SUBCLASS this scaler and apply it for your special case

	Hopefully that should cover most cases
	*/

	public class ControlDpiScalingCodeDomSerializer : ControlCodeDomSerializer
	{
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			var result = base.Serialize(manager, value);

			DpiScalingSerializerHelper.ScaleCodeStatements(manager, value, result as CodeStatementCollection);

			return result;
		}
	}

	public class ComponentDpiScalingCodeDomSerializer : ComponentCodeDomSerializer
	{
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			var result = base.Serialize(manager, value);

			DpiScalingSerializerHelper.ScaleCodeStatements(manager, value, result as CodeStatementCollection);

			return result;
		}
	}

	enum TableLayoutStyle
	{
		RowStyle,
		ColumnStyle,
	}

	static class DpiScalingSerializerHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Specific message to serializer")]
		public static void ScaleCodeStatements(IDesignerSerializationManager manager, object value, CodeStatementCollection statements)
		{
			if (statements == null)
			{
				return;
			}

			// Only select assignment statements whose left hand side is one of the scaled properties
			foreach (var assignStatement in statements.OfType<CodeAssignStatement>())
			{
				ScaleAssignStatement(assignStatement, value, ControlDpiScalingHelper.UnscaleFromCurrentDpiX, ControlDpiScalingHelper.UnscaleFromCurrentDpiY);
			}

			foreach (var rowStyleConstructor in RetrieveTableLayoutStyleConstructors(statements, TableLayoutStyle.RowStyle))
			{
				ScaleTableLayoutStyleConstructor(rowStyleConstructor, ControlDpiScalingHelper.UnscaleFromCurrentDpiY, ScaleYFunction);
			}

			foreach (var columnStyleConstructor in RetrieveTableLayoutStyleConstructors(statements, TableLayoutStyle.ColumnStyle))
			{
				ScaleTableLayoutStyleConstructor(columnStyleConstructor, ControlDpiScalingHelper.UnscaleFromCurrentDpiX, ScaleXFunction);
			}

			// Wrap containers that have children in SuspendLayout/ResumeLayout
			if (value is Control valueAsControl && valueAsControl.Controls.Count > 0)
			{
				var executePendingLayout = false;

				CodeExpression layoutTarget = null;
				if (value == ((IDesignerHost)manager.GetService(typeof(IDesignerHost))).RootComponent)
				{
					layoutTarget = new CodeThisReferenceExpression();
				}
				else
				{
					var name = manager.GetName(valueAsControl);
					if (name != null)
					{
						layoutTarget = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), name);
						executePendingLayout = valueAsControl is UserControl;
					}
				}

				if (layoutTarget != null)
				{
					var suspendLayoutCall = new CodeExpressionStatement(new CodeMethodInvokeExpression(layoutTarget, "SuspendLayout"));
					var resumeLayoutCall = new CodeExpressionStatement(new CodeMethodInvokeExpression(layoutTarget, "ResumeLayout", new CodePrimitiveExpression(executePendingLayout)));
					var performLayoutCall = new CodeExpressionStatement(new CodeMethodInvokeExpression(layoutTarget, "PerformLayout"));

					suspendLayoutCall.UserData["statement-ordering"] = "begin";
					resumeLayoutCall.UserData["statement-ordering"] = "end";
					performLayoutCall.UserData["statement-ordering"] = "end";

					TryAddMethodInvokeExpressionStatement(statements, suspendLayoutCall);
					TryAddMethodInvokeExpressionStatement(statements, resumeLayoutCall);
					TryAddMethodInvokeExpressionStatement(statements, performLayoutCall);
				}
			}
		}

		/// <summary>
		/// Do not add duplicate method call statements.
		/// </summary>
		/// <param name="statements">Serialized statements collection.</param>
		/// <param name="codeExpressionStatement">New method call statement.</param>
		/// <remarks>
		/// Due to issues with base ripped ControlCodeDomSerializer, it did not add Suspend/Resume/PerformLayout for most of controls.
		/// Fixed ControlCodeDomSerializer adds such method calls, but not for all controls
		/// (e.g. it does not add them for ZGrid and some user controls like ModuleButtonGrid, ZGuidFindBox.)
		/// </remarks>
		static void TryAddMethodInvokeExpressionStatement(CodeStatementCollection statements, CodeExpressionStatement codeExpressionStatement)
		{
			var methodExpression = (CodeMethodInvokeExpression)codeExpressionStatement.Expression;
			if (!statements.OfType<CodeExpressionStatement>().Any(s => AreMethodExpressionsEqualIgnoreParameters(s.Expression as CodeMethodInvokeExpression, methodExpression)))
			{
				statements.Add(codeExpressionStatement);
			}
		}

		static bool AreMethodExpressionsEqualIgnoreParameters(CodeMethodInvokeExpression x, CodeMethodInvokeExpression y)
		{
			if (x != null && y != null &&
				x.Method.MethodName.Equals(y.Method.MethodName) &&
				AreTargetObjectsEqual(x.Method.TargetObject, y.Method.TargetObject))
			{
				return true;
			}
			return false;
		}

		static bool AreTargetObjectsEqual(CodeExpression x, CodeExpression y)
		{
			if (x is CodeThisReferenceExpression && y is CodeThisReferenceExpression)
			{
				return true;
			}

			if (x is CodeFieldReferenceExpression xField && y is CodeFieldReferenceExpression yField &&
				xField.FieldName.Equals(yField.FieldName) && AreTargetObjectsEqual(xField.TargetObject, yField.TargetObject))
			{
				return true;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		static IEnumerable<CodeObjectCreateExpression> RetrieveTableLayoutStyleConstructors(CodeStatementCollection statements, TableLayoutStyle style)
		{
			return from CodeStatement statement in statements
				   let expressionStatement = statement as CodeExpressionStatement
				   where expressionStatement != null
				   let methodInvoke = expressionStatement.Expression as CodeMethodInvokeExpression
				   where methodInvoke != null && methodInvoke.Method.MethodName == "Add" && methodInvoke.Parameters.Count > 0
				   let targetObject = methodInvoke.Method.TargetObject as CodePropertyReferenceExpression
				   where targetObject != null && targetObject.PropertyName == $"{style}s"
				   let methodParameter = methodInvoke.Parameters[0] as CodeObjectCreateExpression
				   where methodParameter != null && methodParameter.CreateType.BaseType == $"System.Windows.Forms.{style}"
				   select methodParameter;
		}

		#region Scaling functions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		static void ScaleTableLayoutStyleConstructor(CodeObjectCreateExpression tableLayoutConstructor, Func<int, int> scaleFunction, string scaleFunctionName)
		{
			if (tableLayoutConstructor.Parameters.Count < 2)
			{
				return;
			}

			var unitTypeParameter = tableLayoutConstructor.Parameters[0] as CodeFieldReferenceExpression;
			var lengthParameter = tableLayoutConstructor.Parameters[1] as CodePrimitiveExpression;
			if (lengthParameter != null && unitTypeParameter != null && unitTypeParameter.FieldName == "Absolute")
			{
				var length = (float)lengthParameter.Value;
				var scaledHeight = new CodeMethodInvokeExpression(
					new CodeTypeReferenceExpression(ScalerName),
					scaleFunctionName,
					new[] { new CodePrimitiveExpression(scaleFunction((int)length)) });

				tableLayoutConstructor.Parameters[1] = scaledHeight;
			}
		}

		static void ScaleAssignStatement(CodeAssignStatement assignStatement, object value, ScaleFunc scaleX, ScaleFunc scaleY)
		{
			if (assignStatement.Right is CodeObjectCreateExpression && GetScaledState(assignStatement.Left, value) == DpiState.ScaledVariant)
			{
				assignStatement.Right = ScaleCyclicConstructorCall((CodeObjectCreateExpression)assignStatement.Right, scaleX, scaleY);
			}
			else if (assignStatement.Right is CodePrimitiveExpression)
			{
				assignStatement.Right = ScalePrimitiveExpression(assignStatement.Left, (CodePrimitiveExpression)assignStatement.Right, value, scaleX, scaleY);
			}
		}

		static CodeExpression ScalePrimitiveExpression(CodeExpression lhs, CodePrimitiveExpression rhs, object value, ScaleFunc scaleX, ScaleFunc scaleY)
		{
			string functionName;
			ScaleFunc scale;

			switch (GetScaledState(lhs, value))
			{
				case DpiState.ScaleX:
					functionName = ScaleXFunction;
					scale = scaleX;
					break;
				case DpiState.ScaleY:
					functionName = ScaleYFunction;
					scale = scaleY;
					break;
				default:
					return rhs; //Nothing to scale
			}

			return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(ScalerName), functionName, new[] { new CodePrimitiveExpression(scale((int)rhs.Value)) });
		}

		static CodeMethodInvokeExpression ScaleCyclicConstructorCall(CodeObjectCreateExpression expr, ScaleFunc scaleX, ScaleFunc scaleY)
		{
			var methodCallParameters = expr.Parameters.Cast<CodePrimitiveExpression>()
				.Select(param => (int)param.Value)
				.Select((value, index) => index % 2 == 0 ? scaleX(value) : scaleY(value)) // Cyclic methods follow the pattern (x1, y1, x2, y2), eg new Point(x, y), new Padding(x1, y1, x2, y2)
				.Select(scaledValue => new CodePrimitiveExpression(scaledValue))
				.Append(new CodePrimitiveExpression(true));

			return new CodeMethodInvokeExpression(
				new CodeTypeReferenceExpression(ScalerName),
				"NewScaled" + expr.CreateType.BaseType.Split('.').Last(),
				methodCallParameters.ToArray());
		}

		static DpiState GetScaledState(CodeExpression expr, object value)
		{
			var member = GetMemberInfo(expr, value);
			if (member != null)
			{
				var chooseMyself = value as IChooseHowMyPropertiesScale;
				if (chooseMyself != null)
				{
					var result = chooseMyself.GetDpiState(member);
					if (result != DpiState.Unknown)
					{
						return result;
					}
				}

				var customState = member.GetCustomAttribute<DpiStateAttribute>(inherit: true);
				if (customState != null)
				{
					return customState.State;
				}

				return ExplicitDpiStates.GetState(member.DeclaringType.FullName, member.Name);
			}

			return DpiState.Unknown;
		}

		static MemberInfo GetMemberInfo(CodeExpression expr, object value)
		{
			var propExpr = expr as CodePropertyReferenceExpression;
			if (propExpr != null)
			{
				return GetMemberInfo(propExpr.PropertyName, true, value.GetType());
			}

			var fieldExpr = expr as CodeFieldReferenceExpression;
			if (fieldExpr != null)
			{
				return GetMemberInfo(fieldExpr.FieldName, false, value.GetType());
			}

			return null;
		}

		static MemberInfo GetMemberInfo(string name, bool isProperty, Type type)
		{
			var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly | (isProperty ? BindingFlags.GetProperty : BindingFlags.GetField);
			for (var t = type; t != null; t = t.BaseType)
			{
				var member = t.GetMember(name, bindingFlags).SingleOrDefault();
				if (member != null)
				{
					return member;
				}
			}

			return null;
		}

		const string ScalerName = "CargoWise.Windows.UI.ControlDpiScalingHelper";
		const string ScaleYFunction = "ScaleToCurrentDpiY";
		const string ScaleXFunction = "ScaleToCurrentDpiX";

		#endregion

		delegate int ScaleFunc(int value);
	}
}
