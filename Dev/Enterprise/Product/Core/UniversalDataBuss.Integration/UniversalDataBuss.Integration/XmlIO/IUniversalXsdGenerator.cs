using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalXsdGenerator
	{
		string GetXsdOutput(Type outermostType);
		string GetXsdOutput(Assembly assemblyContainingTypes);
		string GetCommonSchemaXsdOutput();
		void SaveXsdFile(string content, string unmappedFileName);
	}
}
