using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.SDF
{
	/// <summary>
	/// Collection to ctore the SDF Wrapper objects
	/// </summary>
	public class StmSystemDefinedFieldWrapperCollection : NonPersistentBusinessObjectCollection<StmSystemDefinedFieldWrapper>
	{
		public StmSystemDefinedFieldWrapperCollection()
			: base(null)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("StmSystemDefinedFieldWrapperCollection does not support CreateNonPersistentBusinessObject() method.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
