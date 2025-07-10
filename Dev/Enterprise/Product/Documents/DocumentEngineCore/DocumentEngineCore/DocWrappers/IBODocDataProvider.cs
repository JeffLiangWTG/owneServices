using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	/// <summary>
	/// Provides data from a Business Object to the DocumentEngine for rendering Documents.
	/// </summary>
	public interface IBODocDataProvider
	{
		DocWrapperCopyInfo AdditionalCopyInfo { get; }
		string[] ImageNamesToRemove { get; }
		BusinessObject BusinessObjectToLogAgainst { get; }
		void SetDocWrapperContext(Dictionary<string, object> constants);
		BusinessObject ParentBusinessObject { get; }
		string ToString();
		ZString GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue);
		IZType GetCustomField(string fieldName, string fieldType = null);
		string GetCustomFieldCodeDescription(string fieldName, string fieldType = null);
		ZDateTime GetEventLastDateTime(string eventCode);
	}

	public interface IBODocDataProviderWithBOForPrintJob : IBODocDataProvider
	{
		BusinessObject BusinessObjectForPrintJob { get; }
	}
}
