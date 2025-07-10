using System;

using CargoWise.EntityFramework;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Enterprise.ZArchitecture.Dynamic
{
	public class DlrProxy : IDlrProxy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public TDelegate CreateLambda<TDelegate>(string expression, params string[] parameters)
		{
			string lambdaExpr = "lambda " + String.Join(", ", parameters) + ": " + expression;
			ScriptSource code = Engine.CreateScriptSourceFromString(lambdaExpr, SourceCodeKind.Expression);

			return code.Execute<TDelegate>(Scope);
		}

		public object RunScript(string code)
		{
			return Engine.CreateScriptSourceFromString(code, SourceCodeKind.Statements).Execute(Scope);
		}

		public void SetVariable(string name, object value)
		{
			Scope.SetVariable(name, value);
		}

		public bool IsDlrException(Exception ex)
		{
			var fullname = ex.GetType().FullName;
			return
				fullname.StartsWith("IronPython.") ||
				fullname.StartsWith("Microsoft.Scripting.") ||
				ex.Source.Equals("IronPython");
		}

		#region Engine / Scope

		ScriptEngine Engine
		{
			get
			{
				if (scriptEngine == null)
				{
					scriptEngine = Python.CreateEngine();
				}

				return scriptEngine;
			}
		}
		ScriptEngine scriptEngine;

		ScriptScope Scope
		{
			get
			{
				if (scriptScope == null)
				{
					scriptScope = Engine.CreateScope();
					RunScript(helpers);
				}

				return scriptScope;
			}
		}
		ScriptScope scriptScope;

		#endregion

		#region Helpers
		#region SuppressResourceStringsCheckRegion

		const string helpers = @"
import clr
from System import *

def iif(a, b, c):
	return b if a else c

def left(str, length):
	return str[:length]

def right(str, length):
	return str[len(str)-length:]

def mid(str, start, length=-1):
	if length >= 0:
		return str[start:start+length]
	else:
		return str[start:]

def numberFormat(num, decimalPlaces):
	return num.ToString(String.Format(""F{0}"", decimalPlaces))
";

		#endregion
		#endregion
	}
}
