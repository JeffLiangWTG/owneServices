using System;
using System.Linq;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace GlowIndexQueryService.Business
{
	public class SearchField
	{
		public SearchField(string fieldName, string description, Type dataType, bool uiHidden, bool isUtcTime, int scale, SearchFieldRuleLookup ruleLookup) : this(fieldName, description, dataType, uiHidden, isUtcTime, scale)
		{
			_ruleLookup = ruleLookup;
		}

		public SearchField(string fieldName, string description, Type dataType, bool uiHidden, bool isUtcTime, int scale, SearchFieldEntityLookup entityLookup) : this(fieldName, description, dataType, uiHidden, isUtcTime, scale)
		{
			_entityLookup = entityLookup;
		}

		public SearchField(string fieldName, string description, Type dataType, bool uiHidden = false, bool isUtcTime = false, int scale = 0)
		{
			FieldName = fieldName;
			Description = description;
			UIHidden = uiHidden;
			DataType = dataType;
			IsUtcTime = isUtcTime;
			Scale = scale;
		}

		public static SearchField Create(string fieldName)
		{
			return Create(fieldName, "", typeof(string));
		}

		public static SearchField Create(string fieldName, string description)
		{
			return Create(fieldName, description, typeof(string));
		}

		public static SearchField Create(string fieldName, string description, Type dataType)
		{
			return new SearchField(fieldName, description, dataType);
		}

		public string FieldName { get; }
		public string Description { get; }
		public bool UIHidden { get; }
		public Type DataType { get; }

		public bool IsUtcTime { get; }
		public int Scale { get; }

		protected SearchFieldRuleLookup _ruleLookup;
		protected SearchFieldEntityLookup _entityLookup;

		public SearchFieldRuleLookup RuleLookup => _ruleLookup;
		public SearchFieldEntityLookup EntityLookup => _entityLookup;

		public bool HasListDelegate => RuleLookup != null;
		public Func<CodeDescriptionPairList> GetListDelegate => RuleLookup.GetList;

		public bool HasLookup => EntityLookup != null && !string.IsNullOrWhiteSpace(EntityLookup.TableName);

		public bool IsActiveStatusField => FieldName.Equals("ISACTIVE", StringComparison.OrdinalIgnoreCase);
		public bool IsCancelledStatusField => FieldName.Equals("ISCANCELLED", StringComparison.OrdinalIgnoreCase);
		public bool IsAuditField => AuditLogFields.Contains(FieldName, StringComparer.OrdinalIgnoreCase);

		[ThreadSafe]
		static readonly string[] AuditLogFields = { "CREATEUSER", "CREATETIME", "LASTEDITUSER", "LASTEDITTIME" };
	}
}
