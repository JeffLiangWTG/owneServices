using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedField : StmSystemDefinedFieldBase
	{
		public StmSystemDefinedField(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override StmSystemDefinedFieldValidation GetNewValidation()
		{
			return new StmSystemDefinedFieldWithOrderAndGridValidation(this);
		}

		public override void Delete()
		{
			FieldColumns.RemoveAndDeleteAll();
			FieldCountries.RemoveAndDeleteAll();
			base.Delete();
		}

		#region ZProperty Overrides

		public override ZString S1_Type
		{
			get { return base.S1_Type; }
			set
			{
				bool isValueChanged = (base.S1_Type != value);
				base.S1_Type = value;

				if (isValueChanged)
				{
					if (IsGrid && !S1_Default.IsEmpty)
					{
						S1_Default = "";
					}

					FieldColumns.SetReadOnlyIncludingChildren(!IsGrid);

					if (!IsGrid && FieldColumns.Count > 0)
					{
						FieldColumns.RemoveAndDeleteAll();
					}
				}
			}
		}

		public ZString MultiColumnStyleType
		{
			get
			{
				switch (S1_Type)
				{
					case StmSystemDefinedFieldLookups.TypeCodes.DateOnly:
						return nameof(FieldType.Date);

					case StmSystemDefinedFieldLookups.TypeCodes.DateTime:
						return nameof(FieldType.DateTime);

					case StmSystemDefinedFieldLookups.TypeCodes.Decimal:
						return nameof(FieldType.Decimal);

					case StmSystemDefinedFieldLookups.TypeCodes.Integer:
						return nameof(FieldType.Integer);

					case StmSystemDefinedFieldLookups.TypeCodes.MultiLineText:
						return nameof(FieldType.TextMultiLine);

					case StmSystemDefinedFieldLookups.TypeCodes.Boolean:
						return nameof(FieldType.Boolean);
				}

				return nameof(FieldType.Text);
			}
		}

		[TranslatableDataField(Schema.TableName, Schema.S1_Category, DataXmlFilePaths.SystemDefinedField, MaxLength = Schema.S1_CategoryMaxLength, Type = typeof(StmSystemDefinedField), Asmid = ResString.AssemblyId)]
		public override ZString S1_Category
		{
			get => base.S1_Category;
			set => base.S1_Category = value;
		}

		public MultilingualString S1_CategoryMultilingual => GetMultilingual(S1_CategoryInfo);

		[TranslatableDataField(Schema.TableName, Schema.S1_Name, DataXmlFilePaths.SystemDefinedField, MaxLength = Schema.S1_NameMaxLength, Type = typeof(StmSystemDefinedField), Asmid = ResString.AssemblyId)]
		public override ZString S1_Name
		{
			get => base.S1_Name;
			set => base.S1_Name = value;
		}

		public MultilingualString S1_NameMultilingual => GetMultilingual(S1_NameInfo);

		#endregion

		protected bool S1_Default_ReadOnly
		{
			get { return IsGrid; }
		}

		#region Field Columns

		[ChildEditable(true)]
		public StmSystemDefinedFieldColumnCollection FieldColumns
		{
			get
			{
				if (fFieldColumns == null)
				{
					var lFieldColumns = new StmSystemDefinedFieldColumnCollection(this, Factory);
					lFieldColumns.Load();
					fFieldColumns = lFieldColumns;
					fFieldColumns.SetReadOnlyIncludingChildren(!IsGrid);
					fFieldColumns.Sort(StmSystemDefinedFieldSchema.Constants.S1_OrderColumn, ListSortDirection.Ascending);
					RegisterEditableChildObject(fFieldColumns);
				}
				return fFieldColumns;
			}
		}
		StmSystemDefinedFieldColumnCollection fFieldColumns;

		#endregion

		#region Field Countries

		[ChildEditable(true)]
		public StmSystemDefinedFieldCountryCollection FieldCountries
		{
			get
			{
				if (fFieldCountries == null)
				{
					var lFieldCountries = new StmSystemDefinedFieldCountryCollection(this, Factory);
					lFieldCountries.Load();
					fFieldCountries = lFieldCountries;
					RegisterEditableChildObject(fFieldCountries);
				}
				return fFieldCountries;
			}
		}
		StmSystemDefinedFieldCountryCollection fFieldCountries;

		#endregion
	}
}
