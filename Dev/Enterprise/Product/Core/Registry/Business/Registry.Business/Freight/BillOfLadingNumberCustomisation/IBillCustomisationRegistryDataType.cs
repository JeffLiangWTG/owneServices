using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public interface IBillCustomisationRegistryDataType
	{
		string FountainPrefix { get; set; }
		MultilingualString GeneratedNumberName { get; set; }
		MultilingualString SequenceNumberName { get; set; }
		int MaxLength { get; set; }
		NumberCustomisationElementCategories Categories { get; set; }
		bool AllowNonAlphanumericCharacters { get; set; }
		bool EnableMacroInsertion { get; set; }
		Type MacroType { get; set; }
	}
}
