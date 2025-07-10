//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmSystemDefinedFieldLookups
//
//    This class should be used for overriding collections in AutoStmSystemDefinedFieldLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldLookups : AutoStmSystemDefinedFieldLookups
	{
		public StmSystemDefinedFieldLookups(AutoStmSystemDefinedField parent)
			: base(parent)
		{
		}

		protected new StmSystemDefinedFieldBase Parent
		{
			get { return (StmSystemDefinedFieldBase)base.Parent; }
		}

		#region Types

		public abstract class TypeCodes
		{
			public const string DateOnly = "DAT";
			public const string DateTime = "DTM";
			public const string Decimal = "DEC";
			public const string Grid = "GRD";
			public const string Integer = "INT";
			public const string Text = "TXT";
			public const string MultiLineText = "MUL";
			public const string Boolean = "BLN";
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				if (fTypes == null)
				{
					fTypes = GetNewTypes();
				}
				return fTypes;
			}
		}

		protected virtual CodeDescriptionPairList GetNewTypes()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(TypeCodes.MultiLineText, Res.GetString("f2f43e86-d3ba-45b6-9cfe-cf65053e273d", "Multi-Line Text"));
			result.AddPair(TypeCodes.Text, Res.GetString("ad2dfc12-b5c4-45f5-94b0-ca02b9f44562", "Text"));
			result.AddPair(TypeCodes.DateTime, Res.GetString("41fae0f6-b7e5-48ed-b3b9-6a7e4697d5f1", "Date Time"));
			result.AddPair(TypeCodes.DateOnly, Res.GetString("069bb230-60dc-4eb7-a7b9-e7d868010d02", "Date Only"));
			result.AddPair(TypeCodes.Decimal, Res.GetString("c5502d8f-6445-4e4b-970a-739630c0a2b6", "Decimal"));
			result.AddPair(TypeCodes.Integer, Res.GetString("a65eb95c-3e17-439d-9398-a75f035fb3f0", "Integer"));
			result.AddPair(TypeCodes.Grid, Res.GetString("86bd9a5d-14a6-4f63-9b84-df01a135bd12", "Grid"));
			result.AddPair(TypeCodes.Boolean, Res.GetString("0b48edf5-1a0d-482d-8c23-906e69b7eed6", "Boolean"));

			return result;
		}

		CodeDescriptionPairList fTypes;

		#endregion

		#region Display/Edit Rules

		public abstract class DisplayEditRuleCodes
		{
			public const string InGrid = "ING";
			public const string PopupScrollBox = "POP";
		}

		public CodeDescriptionPairList DisplayEditRules
		{
			get
			{
				if (fDisplayEditRules == null)
				{
					fDisplayEditRules = new CodeDescriptionPairList();
					fDisplayEditRules.AddPair(DisplayEditRuleCodes.InGrid, Res.GetString("49ae2362-b4fc-488f-bd20-b782515046f4", "In Grid"));
					fDisplayEditRules.AddPair(DisplayEditRuleCodes.PopupScrollBox, Res.GetString("ac807a07-1197-4b9b-b8fa-386e8546967f", "Popup Scroll Box"));
				}

				return fDisplayEditRules;
			}
		}

		CodeDescriptionPairList fDisplayEditRules;

		#endregion

		#region Validations

		public abstract class ValidationCodes
		{
			public const string NoValidation = "NOV";
			public const string RangeValueRequired = "RVL";
			public const string ValueRequired = "VAL";
		}

		public CodeDescriptionPairList Validations
		{
			get
			{
				if (fValidations == null)
				{
					fValidations = new CodeDescriptionPairList();
					fValidations.AddPair(ValidationCodes.NoValidation, Res.GetString("71ade899-1925-48a2-ba92-7377ac209811", "No Validation"));
					fValidations.AddPair(ValidationCodes.ValueRequired, Res.GetString("94b14672-0102-48ae-b52c-2e8dd3c7bec2", "Value Required"));
					fValidations.AddPair(ValidationCodes.RangeValueRequired, Res.GetString("2bb198ba-542d-452c-950e-beb7dfdf0b5b", "Range Value Required"));
				}

				return fValidations;
			}
		}

		CodeDescriptionPairList fValidations;

		#endregion

		#region Defaults

		public CodeDescriptionPairList Defaults
		{
			get
			{
				if (fDefaults == null)
				{
					fDefaults = GetNewDefaults();
				}
				return fDefaults;
			}
		}

		protected virtual CodeDescriptionPairList GetNewDefaults()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			StmSystemDefinedFieldDependentCollection collection = Parent.ParentCollection as StmSystemDefinedFieldDependentCollection;

			if (collection != null)
			{
				BusinessObject baseBusinessObject = collection.DocumentSupportable as BusinessObject;

				if (baseBusinessObject != null)
				{
					foreach (ZPropertyInfo propertyInfo in baseBusinessObject.ZPropertyInfoHash)
					{
						if (propertyInfo.IsPersistent)
						{
							result.AddPair(propertyInfo.Name, propertyInfo.PropertyType.Name);
						}
					}

					result.SortByDescription();
				}
			}

			return result;
		}

		CodeDescriptionPairList fDefaults;

		#endregion
	}
}
