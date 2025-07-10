using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackResource : AutoStmTranslationFeedbackResource
	{
		public StmTranslationFeedbackResource(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{ }

		[RelatedBusinessObject("Parent")]
		public override ZGuid XQ_XT
		{
			get { return base.XQ_XT; }
			set { base.XQ_XT = value; }
		}

		public StmTranslationFeedback Parent
		{
			get
			{
				if (parent == null || parent.PK != XQ_XT)
				{
					parent = Factory.Load<StmTranslationFeedback>(XQ_XT);
				}
				return parent;
			}
			set
			{
				parent = value;
				XQ_XT = parent.PK;
			}
		}
		StmTranslationFeedback parent;

		[List("Lookups.MatchTypes")]
		public override ZString XQ_MatchType
		{
			get { return base.XQ_MatchType; }
			set { base.XQ_MatchType = value; }
		}

		[List("Lookups.ResourceStringLevels")]
		[ReadOnly(true)]
		public override ZString XQ_ResourceStringLevel
		{
			get { return base.XQ_ResourceStringLevel; }
			set { base.XQ_ResourceStringLevel = value; }
		}

		public HelpDataString SourceData
		{
			get
			{
				if (sourceData == null)
				{
					sourceData = ResourceStringsFactory.Lookup(Res.DefaultLanguage, this.XQ_ResourceStringKey);
				}
				return sourceData;
			}
			set
			{
				sourceData = value;
			}
		}
		HelpDataString sourceData;

		public HelpDataString TargetData
		{
			get
			{
				if (targetData == null && Parent != null)
				{
					targetData = ResourceStringsFactory.Lookup(Parent.XT_Language, this.XQ_ResourceStringKey);
				}
				return targetData;
			}
			set
			{
				targetData = value;
			}
		}
		HelpDataString targetData;

		public ZString ContextClassName
		{
			get { return SourceData != null ? SourceData.HD_ContextClassName : ZString.Empty; }
		}

		public ZString ContextName
		{
			get
			{
				return ContextClassName.IsEmpty && XQ_ResourceStringKey.Contains('|') ?
					XQ_ResourceStringKey.Substring(0, XQ_ResourceStringKey.IndexOf('|')) :
					ContextClassName.SubstringSafe(ContextClassName.LastIndexOf('.') + 1);
			}
		}

		public ZString Module
		{
			get { return TranslationFileModuleMapping.Instance.Lookup(ContextClassName).ModuleName; }
		}

		public ZBool Update
		{
			get
			{
				return update;
			}
			set
			{
				SetNonPersistentPropertyValue(UpdateInfo, ref update, value);
			}
		}
		ZBool update;

		public ZPropertyInfo UpdateInfo
		{
			get
			{
				return this.GetZPropertyInfo(nameof(Update));
			}
		}

		public override bool IsSavedByFactory
		{
			get { return Update && base.IsSavedByFactory && Parent != null && (Parent.IsSavedByFactory || Parent.IsInDatabase); }
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsInDatabase; }
			set { base.ReadOnly = value; }
		}

		const int ExcelSheetNameMaxLength = 31;
		public int MaxLength
		{
			get
			{
				if (!maxLength.HasValue)
				{
					maxLength = int.MaxValue;
					var d = XQ_ResourceStringKey.IndexOf('$');
					if (d > -1)
					{
						var columnName = XQ_ResourceStringKey.Substring(0, d);
						var attribute = TranslatableDataFieldAttribute.GetAttributeForColumn(columnName);
						if (attribute != null)
						{
							maxLength = attribute.MaxLength;
						}
					}
					else if (XQ_ResourceStringKey.Contains('|')
						&& XQ_ResourceStringKey.Substring(0, XQ_ResourceStringKey.IndexOf('|')).Trim().EqualsIgnoringCase("ReportName"))
					{
						maxLength = ExcelSheetNameMaxLength;
					}
				}
				return maxLength.Value;
			}
		}
		int? maxLength;
	}
}
