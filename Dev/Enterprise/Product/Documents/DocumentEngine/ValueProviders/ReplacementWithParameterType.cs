namespace Enterprise.DocumentEngine
{
	internal class ReplacementWithParameterType
	{
		internal ReplacementWithParameterType(object macroValue, string parameterTypeName)
		{
			MacroValue = macroValue;
			ParameterTypeName = parameterTypeName;
		}
		internal object MacroValue;
		internal string ParameterTypeName;
	}
}