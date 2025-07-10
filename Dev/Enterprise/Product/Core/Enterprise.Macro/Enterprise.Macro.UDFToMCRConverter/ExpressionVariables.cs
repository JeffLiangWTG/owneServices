using System;
using System.Collections.Generic;

namespace Enterprise.Macro.UDFToMCRConverter
{
	class ExpressionVariables
	{
		public ExpressionVariables() { }
		public ExpressionVariables(Type rootType)
		{
			RootType = rootType;
		}
		public ExpressionVariables(Type rootType, Dictionary<Type, string> variableTypeNames)
		{
			RootType = rootType;
			VariableTypeNames = variableTypeNames;
		}
		public Dictionary<Type, string> VariableTypeNames { get; } = new();

		string envVariableName;
		public string EnvVariableName
		{
			get
			{
				if (envVariableName == null)
				{
					if(!VariableTypeNames.TryGetValue(typeof(MasterFiles.Business.Macros.Environment), out envVariableName))
					{
						throw new InvalidOperationException("Environment variable not found");
					}
				}
				return envVariableName;
			}
		}

		public Type RootType { get; }

		public IEnumerable<Type> GetAllTypes()
		{
			return new List<Type>(VariableTypeNames.Keys) { RootType };
		}
	}
}
