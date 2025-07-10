using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentEngine
{
	public interface IReportDocumenter
	{
		ZString Useage { get; }
		ZString Explanation { get; }
		List<string> SupportedProperties { get; }
		bool SupportLookup { get; }
		List<ValueProviderDocumenter> ValueProviderDocumenters { get; set; }
		ZString DefaultOptions { get; set; }
	}
}
