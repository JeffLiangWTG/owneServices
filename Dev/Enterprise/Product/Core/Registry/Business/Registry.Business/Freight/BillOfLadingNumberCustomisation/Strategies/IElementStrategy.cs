using Enterprise.ZArchitecture;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	public interface IElementStrategy
	{
		string Key { get; }
		string Name { get; }
		string Description { get; }
		bool Force { get; }
		byte DefaultOrder { get; }
		bool ReadOnly { get; }

		bool UseDetail { get; }
		string DefaultDetail { get; }
		int DetailMaxLength { get; }
		FieldType DetailFieldType { get; }

		int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent);
		BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent);

		NumberCustomisationElementCategories Categories { get; }

		bool OrderReadOnly { get; }

		bool IncludeReadOnly { get; }

		string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent);
	}
}
