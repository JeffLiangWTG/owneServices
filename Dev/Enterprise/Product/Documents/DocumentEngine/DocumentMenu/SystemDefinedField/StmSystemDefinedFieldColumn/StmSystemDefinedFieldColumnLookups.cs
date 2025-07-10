using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldColumnLookups : StmSystemDefinedFieldLookups
	{
		public StmSystemDefinedFieldColumnLookups(StmSystemDefinedFieldColumn parent) : base(parent)
		{
		}

		protected override CodeDescriptionPairList GetNewTypes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(TypeCodes.Text, Res.GetString("49880334-4456-486c-98ad-160ab606d438", "Text"));
			result.AddPair(TypeCodes.MultiLineText, Res.GetString("fd620bc4-2441-4d56-a8f8-47136a988bb2", "Multi-Line Text"));
			result.AddPair(TypeCodes.DateTime, Res.GetString("bc6e309f-daf8-4544-8e1f-236237912d24", "Date Time"));
			result.AddPair(TypeCodes.DateOnly, Res.GetString("5d7d7423-e70b-420d-9a6d-64b264154039", "Date Only"));
			result.AddPair(TypeCodes.Decimal, Res.GetString("61587172-50cd-4467-acf4-27efdf78250f", "Decimal"));
			result.AddPair(TypeCodes.Integer, Res.GetString("9dd7ea7f-1846-4eaa-aab9-b0f2dc8fbe04", "Integer"));
			result.AddPair(TypeCodes.Boolean, Res.GetString("3a454077-2e5a-444f-a681-5298a9ca6e5f", "Boolean"));

			return result;
		}

		protected override CodeDescriptionPairList GetNewDefaults()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			if (Parent.ParentCollection != null)
			{
				StmSystemDefinedFieldColumnCollection collection = Parent.ParentCollection as StmSystemDefinedFieldColumnCollection;

				if (collection != null)
				{
					result = collection.ParentField.Lookups.Defaults;
				}
			}

			return result;
		}
	}
}
