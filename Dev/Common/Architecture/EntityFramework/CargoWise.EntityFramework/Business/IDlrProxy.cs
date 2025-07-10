using System;

namespace CargoWise.EntityFramework
{
	public interface IDlrProxy
	{
		TDelegate CreateLambda<TDelegate>(string expression, params string[] parameters);
		object RunScript(string code);
		void SetVariable(string name, object value);
		bool IsDlrException(Exception ex);
	}
}
