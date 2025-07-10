using System;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	public class CommonElementStrategy : ElementStrategy
	{
		public CommonElementStrategy(string key, string name, string description
			, NumberCustomisationElementCategories categories, int maxGeneratedLength
			, Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType)
			: this(key, name, description, categories, maxGeneratedLength, null, overrideRegExForDataType) { }

		public CommonElementStrategy(string key, string name, string description
			, NumberCustomisationElementCategories categories, Func<int> getMaxGeneratedLengthFunc
			, Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType)
			: this(key, name, description, categories, 0, getMaxGeneratedLengthFunc, overrideRegExForDataType) { }

		CommonElementStrategy(string key, string name, string description, NumberCustomisationElementCategories categories
			, int maxGeneratedLength
			, Func<int> getMaxGeneratedLengthFunc
			, Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType)
		{
			this.key = key;
			this.name = name;
			this.description = description;
			this.maxGeneratedLength = maxGeneratedLength;
			this.getMaxGeneratedLengthFunc = getMaxGeneratedLengthFunc;
			this.categories = categories;
			this.overrideRegExForDataType = @overrideRegExForDataType;
		}

		public override string Key
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return key; }
		}
		public override string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return name; }
		}
		public override string Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return description; }
		}

		public override NumberCustomisationElementCategories Categories
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return categories; }
		}

		public override int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent)
		{
			return getMaxGeneratedLengthFunc != null ? getMaxGeneratedLengthFunc.Invoke() : maxGeneratedLength;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string key;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string name;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string description;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int maxGeneratedLength;
		readonly Func<int> getMaxGeneratedLengthFunc;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly NumberCustomisationElementCategories categories;
		public override string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent)
			=> overrideRegExForDataType?.Invoke(parent) ?? string.Empty;

		Func<BillOfLadingNumberCustomisationElement, string> overrideRegExForDataType { get; }
	}
}
