using CargoWise.EntityFramework;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ProperCaseExcludeWordValidation : AutoProperCaseExcludeWordValidation
	{
		public ProperCaseExcludeWordValidation(AutoProperCaseExcludeWord parent)
			: base(parent)
		{
		}

		protected override void CheckWord()
		{
			base.CheckWord();
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.WordInfo);
			if (Parent.Word.Contains(' '))
			{
				Parent.WordInfo.AddError(Res.GetString("9c6b9cad-dca2-4e08-bc1a-bcb59e75360a", "Word can not contain spaces"));
			}
		}

		#region Implementation

		public new ProperCaseExcludeWord Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ProperCaseExcludeWord)base.Parent; }
		}

		#endregion
	}
}
