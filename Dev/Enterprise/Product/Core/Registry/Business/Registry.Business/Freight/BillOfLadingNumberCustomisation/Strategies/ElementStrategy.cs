using Enterprise.ZArchitecture;

namespace Enterprise.Registry.Business.BillCustomisationStrategies
{
	[System.Diagnostics.DebuggerDisplay("{Key}")]
	public abstract class ElementStrategy : IElementStrategy
	{
		public abstract string Key { get; }
		public abstract string Name { get; }
		public abstract int CalcMaxGeneratedLength(BillOfLadingNumberCustomisationElement parent);

		public virtual string Description
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return ""; }
		}

		public virtual bool Force
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		public virtual bool ReadOnly
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}

		public virtual byte DefaultOrder
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return 0; }
		}

		public virtual NumberCustomisationElementCategories Categories
		{
			get { return NumberCustomisationElementCategories.Standard; }
		}

		public virtual bool UseDetail
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return false; }
		}
		public virtual string DefaultDetail
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return ""; }
		}
		public virtual int DetailMaxLength
		{
			get { return 0; }
		}
		public virtual FieldType DetailFieldType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FieldType.Text; }
		}

		public virtual BillOfLadingNumberCustomisationElementValidation GetValidation(BillOfLadingNumberCustomisationElement parent)
		{
			return new BillOfLadingNumberCustomisationElementValidation(parent);
		}

		public virtual bool OrderReadOnly => false;

		public virtual bool IncludeReadOnly => false;

		public abstract string GetRegExForDataType(BillOfLadingNumberCustomisationElement parent);
	}
}
