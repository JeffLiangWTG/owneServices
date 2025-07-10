using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.UniversalCopy.Business
{
	[CodeProperty("FilterCode")]
	[DescriptionProperty("FilterNameWithoutHotkeys")]
	public class UniversalCopyTemplate : StmModuleFilter, IUniversalCopyTemplate
	{
		public UniversalCopyTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void PrepareForSave()
		{
			if (!IsDeleted && !IsDeleting)
			{
				AssignHotKeysToAllNameParts();
				SyncNameFromTemplateTree();

				using (var stream = new MemoryStream())
				{
					CopyTemplateTree.CopyTemplateNode.GetCompactCopy().Serialize(stream);
					S9_FilterData = stream.ToArray();
				}
			}
		}

		public bool IsExtended { get; set; }

		#region AssignHotKeysToAllNameParts

		internal IEnumerable<UniversalCopyTemplate> LoadAllTemplateForCurrentModule()
		{
			ZQuery query = new ZQuery(StmModuleFilterSchema.S9_ModuleID, S9_ModuleID);
			query.AddToFilter(StmModuleFilterSchema.S9_GC, S9_GC);
			if (!S9_IsPublished)
			{
				// Todo: Add more accurate check for cases when this is published/unpublished template and there are published/unpublished templates from this/other users/companies
				query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, S9_RelatedEntityID);
			}
			query.AddToFilter(StmModuleFilterSchema.PK, SQLComparisonOperator.NotEqual, PK);

			return Factory.Load<UniversalCopyTemplate>(query);
		}

		void AssignHotKeysToAllNameParts()
		{
			string[] nameParts = CopyTemplateTree.ConfigurationName.ToString().Split('\\');
			var allTemplates = LoadAllTemplateForCurrentModule();
			string prefix = string.Empty;

			for (int i = 0; i < nameParts.Length; i++)
			{
				string currentPart = nameParts[i];
				string currentPartWithoutHotkey = currentPart.Replace("&", "");

				if (!string.IsNullOrWhiteSpace(currentPart) && !currentPart.Contains('&'))
				{
					char availableChar = GetHotkeyInExistingTemplateNamePartInThePath(prefix, currentPartWithoutHotkey, allTemplates);
					if (availableChar == 0)
					{
						availableChar = FindAvailableChar(prefix, currentPart, 'A', 'Z', allTemplates);
					}
					if (availableChar == 0)
					{
						availableChar = FindAvailableChar(prefix, currentPart, '0', '9', allTemplates);
					}

					if (availableChar != (char)0)
					{
						var charIndex = currentPart.IndexOf(availableChar.ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase);
						if (charIndex >= 0)
						{
							nameParts[i] = currentPart.Insert(charIndex, "&");
						}
					}
				}

				if (i < (nameParts.Length - 1))
				{
					prefix += nameParts[i] + @"\";
				}
			}

			CopyTemplateTree.ConfigurationName = string.Join(@"\", nameParts);
		}

		char GetHotkeyInExistingTemplateNamePartInThePath(string prefix, string namePartWithoutHotkey, IEnumerable<UniversalCopyTemplate> allTemplates)
		{
			var existingTemplateNamePart = GetExistingTemplateNamePartInThePath(prefix, namePartWithoutHotkey, allTemplates);
			if (!string.IsNullOrWhiteSpace(existingTemplateNamePart))
			{
				var hotkeyIndex = existingTemplateNamePart.IndexOf('&');
				if (hotkeyIndex >= 0 & hotkeyIndex < (existingTemplateNamePart.Length - 1))
				{
					return existingTemplateNamePart[hotkeyIndex + 1];
				}
			}

			return (char)0;
		}

		string GetExistingTemplateNamePartInThePath(string prefix, string namePartWithoutHotkey, IEnumerable<UniversalCopyTemplate> allTemplates)
		{
			return
				allTemplates
					.Where(template => template.CopyTemplateTree.ConfigurationName.ToString().StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
					.Select(
						template =>
						{
							var templateNamePartEndIndex = template.CopyTemplateTree.ConfigurationName.ToString().IndexOf('\\', prefix.Length);
							if (templateNamePartEndIndex < 0)
							{
								templateNamePartEndIndex = template.CopyTemplateTree.ConfigurationName.Length;
							}
							var templateNamePartLenght = templateNamePartEndIndex - prefix.Length;
							return template.CopyTemplateTree.ConfigurationName.ToString().Substring(prefix.Length, templateNamePartLenght);
						})
					.FirstOrDefault(templateNextPartName => templateNextPartName.Replace("&", "").Equals(namePartWithoutHotkey, StringComparison.InvariantCultureIgnoreCase));
		}

		char FindAvailableChar(string prefix, string name, char from, char to, IEnumerable<UniversalCopyTemplate> allTemplates)
		{
			System.Diagnostics.Debug.Assert(from <= to, (NoResString)"Cannot iterate from bigger to lower values.");

			for (char c = from; c <= to; c++)
			{
				if (name.IndexOf(c.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase) >= 0 &&
					FindTemplateWithHotKey(prefix, "&" + c, allTemplates) == null)
				{
					return c;
				}
			}

			return (char)0;
		}

		internal UniversalCopyTemplate FindTemplateWithHotKey(string prefix, string hotKey, IEnumerable<UniversalCopyTemplate> allTemplates)
		{
			return
				allTemplates
					.Where(template => template.CopyTemplateTree.ConfigurationName.ToString().StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
					.FirstOrDefault(
						template =>
						{
							var hotkeyIndex = template.CopyTemplateTree.ConfigurationName.ToString().IndexOf(hotKey, prefix.Length, StringComparison.InvariantCultureIgnoreCase);
							var nextPartIndex = template.CopyTemplateTree.ConfigurationName.ToString().IndexOf('\\', prefix.Length);
							return hotkeyIndex >= prefix.Length && (nextPartIndex < 0 || hotkeyIndex < nextPartIndex);
						});
		}

		public MultilingualString FilterNameWithoutHotkeys
		{
			get { return S9_FilterNameMultilingual.Replace("&", ""); }
		}

		public ZString FilterCode
		{
			get { return FilterNameWithoutHotkeys.GetUnresolvedString(); }
		}

		#endregion

		public void SyncNameFromTemplateTree()
		{
			S9_FilterName = GetTemplateShortName().SubstringSafe(0, S9_FilterNameInfo.MaxLength);
		}

		ZString GetTemplateShortName()
		{
			ZString templateName = CopyTemplateTree.ConfigurationName;
			int lastBackSlashPos = CopyTemplateTree.ConfigurationName.LastIndexOf('\\');
			if (lastBackSlashPos >= 0)
			{
				templateName = templateName.SubstringSafe(lastBackSlashPos + 1);
			}
			return templateName;
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("a2a52870-24ec-45d8-9bd0-86fab04194bf", "Copy Template"); }
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState =>
			EnterpriseBusinessObject.AutologState.AutoLogged;

		#endregion

		#region Properties

		#region IsPublishedGlobal

		public ZBool IsPublishedGlobal
		{
			get { return S9_GC.IsEmpty || S9_GC.IsMissing || !S9_GC.IsValid; }
			set
			{
				if (value != IsPublishedGlobal)
				{
					S9_GC = value ? ZGuid.Empty : EnvProxy.Instance.CurrentCompany.PK;
					ClearFilterListIfApplicable();
					IsPublishedGlobalInfo.RefreshBinding();
				}
			}
		}

		public bool IsPublishedGlobal_ReadOnly
		{
			get { return !EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed; }
		}

		public ZPropertyInfo IsPublishedGlobalInfo
		{
			get { return GetZPropertyInfo(nameof(IsPublishedGlobal)); }
		}

		public string ModuleWithoutSuffix
		{
			get { return S9_ModuleID.Substring(0, S9_ModuleID.Length - ModuleIdSuffix.UniversalCopyTemplate.Length); }
		}

		public ModuleIdentifier GetModuleIdentifier()
		{
			return ModuleIDs.AllIncludingClientModules.FirstOrDefault(module => module.Name == ModuleWithoutSuffix);
		}

		public MultilingualString ModuleDescription
		{
			get
			{
				var moduleId = GetModuleIdentifier();
				return moduleId != null ? moduleId.Description : ResString.GetMultilingualString("e3e2ac16-d9ca-4fcc-acf5-8f2c99d9fed2", "Unknown");
			}
		}

		#endregion

		public override ZBool S9_IsPublished
		{
			get { return base.S9_IsPublished; }
			set
			{
				var oldValue = S9_IsPublished;
				base.S9_IsPublished = value;
				if (!IsCopying && oldValue != S9_IsPublished)
				{
					var tree = CopyTemplateTree;
					if (tree != null)
					{
						tree.FilterListInfo.RefreshBinding();
					}
				}
			}
		}

		public bool S9_IsPublished_ReadOnly
		{
			get { return !EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed; }
		}

		#region IsApplicable

		public bool IsApplicable
		{
			get
			{
				bool result = true;
				if (IsPublishedGlobal)
				{
					var filterList = CopyTemplateTree?.FilterList ?? ZString.Empty;
					if (!filterList.IsEmpty)
					{
						foreach (var option in filterList.ToUpper().Split(';'))
						{
							if (!EvaluateOption(option))
							{
								result = false;
								break;
							}
						}
					}
				}

				return result;
			}
		}

		bool EvaluateOption(ZString option)
		{
			var result = false;
			var filterValues = option.Split('=');
			if (filterValues.Length == 2)
			{
				var filterType = filterValues[0];
				var values = filterValues[1].Split(',');
				var hasNotEquals = filterType.EndsWith("!", StringComparison.Ordinal);
				if (hasNotEquals)
				{
					filterType = filterType.TrimEnd('!');
				}
				var recognisedFilterType = true;
				switch (filterType)
				{
					case UniversalCopyTemplateFilters.Codes.Country:
						result = MatchCountry(values);
						break;

					case UniversalCopyTemplateFilters.Codes.BrokerageCountry:
						result = MatchCustomsCountryOfJurisdiction(values);
						break;

					default:
						recognisedFilterType = false;
						break;
				}

				if (recognisedFilterType && hasNotEquals)
				{
					result = !result;
				}
			}
			return result;
		}

		bool MatchCustomsCountryOfJurisdiction(ZString[] countries)
		{
			var customsCountry = Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return countries.Any(country => customsCountry == country);
		}

		bool MatchCountry(ZString[] countries)
		{
			return countries.Any(country => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == country);
		}

		#endregion

		#region CopyTemplateTree

		public CopyTemplateTreeBizo CopyTemplateTree
		{
			get
			{
				if (copyTemplateTree == null && !S9_FilterData.IsEmpty)
				{
					using (var stream = new MemoryStream(S9_FilterData))
					{
						copyTemplateTree = new CopyTemplateTreeBizo(CargoWise.UniversalCopy.CopyTemplateTree.Deserialize(stream), this);
					}
					RegisterEditableChildObject(copyTemplateTree);
				}
				return copyTemplateTree;
			}
			set
			{
				if (copyTemplateTree != null)
				{
					UnRegisterEditableChildObject(copyTemplateTree);
				}

				copyTemplateTree = value;

				if (copyTemplateTree != null)
				{
					RegisterEditableChildObject(copyTemplateTree);
				}
			}
		}

		CopyTemplateTreeBizo copyTemplateTree;

		void ClearFilterListIfApplicable()
		{
			if (!IsPublishedGlobal)
			{
				var tree = CopyTemplateTree;
				if (tree != null && !tree.FilterList.IsEmpty)
				{
					tree.FilterList = ZString.Empty;
				}
			}
		}

		#endregion

		#region IsActive

		public ZBool IsActive
		{
			get { return CopyTemplateTree.IsActive; }
			set { CopyTemplateTree.IsActive = value; }
		}

		public ZPropertyInfo IsActiveInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsActive), _ => CopyTemplateTree.IsActiveInfo); }
		}

		#endregion

		#endregion

		#region Validation

		protected override StmModuleFilterValidation GetNewValidation()
		{
			return new UniversalCopyConfigurationValidation(this);
		}

		#endregion

		#region IUniversalCopyTemplate

		ZString IUniversalCopyTemplate.ConfigurationName => CopyTemplateTree?.ConfigurationName ?? ZString.Empty;

		ZString IUniversalCopyTemplate.ConfigurationSource => CopyTemplateTree?.ConfigurationSource ?? ZString.Empty;

		ZGuid IUniversalCopyTemplate.NominatedRecordPk => CopyTemplateTree?.NominatedRecordPk ?? ZGuid.Empty;

		#endregion
	}

	#region Validation Class

	class UniversalCopyConfigurationValidation : StmModuleFilterValidation
	{
		public UniversalCopyConfigurationValidation(UniversalCopyTemplate parent)
			: base(parent)
		{
			copyTemplate = parent;
		}

		readonly UniversalCopyTemplate copyTemplate;

		#region Properties validation

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateIsPublishedGlobal();
		}

		#region S9_IsPublished

		protected override void CheckS9_IsPublished()
		{
			base.CheckS9_IsPublished();

			if (!EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed && copyTemplate.S9_IsPublished)
			{
				copyTemplate.S9_IsPublishedInfo.AddError(Res.GetString("3411e30f-55f5-471a-90ce-bbf6e8903f10", "You don't have permission to create published Universal Copy Templates."));
			}

			if (!copyTemplate.S9_IsPublished && copyTemplate.Factory.Exists(typeof(StmUniversalCopy), new ZQuery(StmUniversalCopySchema.SUC_S9_CopyTemplate, copyTemplate.PK)))
			{
				copyTemplate.S9_IsPublishedInfo.AddError(Res.GetString("481f7d6d-7e74-45ff-a861-ace98f9f8b08", "This template is used in a Copy Schedule(s) and should be published."));
			}
		}

		#endregion

		#region IsPublishedGlobal

		public void ValidateIsPublishedGlobal()
		{
			((IValidationInternals)this).Validate(copyTemplate.IsPublishedGlobalInfo, CheckIsPublishedGlobal);
		}

		protected virtual void CheckIsPublishedGlobal()
		{
			if (copyTemplate.IsPublishedGlobal && !copyTemplate.S9_IsPublished)
			{
				copyTemplate.IsPublishedGlobalInfo.AddError(Res.GetString("18922b8a-e7ac-47d9-a733-9958624aac18", "'Published' should be selected also if you want to publish template across all companies."));
			}

			if (!EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches && copyTemplate.IsPublishedGlobal)
			{
				copyTemplate.IsPublishedGlobalInfo.AddError(Res.GetString("130edf36-6566-4a0f-ba16-d9a53887b4ef", "You don't have permission to publish Universal Copy Templates for all companies."));
			}
			var copyTemplateTree = copyTemplate.CopyTemplateTree;
			if (copyTemplateTree != null)
			{
				copyTemplateTree.Validation.ValidateFilterList();
			}
		}

		#endregion

		#region S9_FilterName

		protected override void CheckS9_FilterName()
		{
			base.CheckS9_FilterName();

			if (copyTemplate.S9_FilterNameMultilingual.IsEmpty)
			{
				copyTemplate.S9_FilterNameInfo.AddError(Res.GetString("5114ea9c-c608-4ca1-9a69-e7442acc56c5", "Please enter a copy template name."));
			}
			else
			{
				CheckUniqueName();
				CheckConflictingNames();
				CheckOnlyOneHotKey();
				CheckHotKeys();
			}
		}

		void CheckUniqueName()
		{
			var parameterCollection = new ZSqlParameterCollection
			{
				{ "@filterName", copyTemplate.S9_FilterNameMultilingual.GetUnresolvedString().Replace("&", ""), StmModuleFilterSchema.S9_FilterName }
			};

			var query = new ZDBOnlyQuery(typeof(UniversalCopyTemplate));
			query.AddFilterAndZSQLParameterCollection($"replace({StmModuleFilterSchema.Constants.S9_FilterName}, '&', '') = @filterName", parameterCollection);
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, copyTemplate.S9_ModuleID);
			query.AddToFilter(StmModuleFilterSchema.S9_GC, copyTemplate.S9_GC);
			query.AddToFilter(StmModuleFilterSchema.S9_FilterType, SQLComparisonOperator.NotEqual, StmModuleFilterTypes.Codes.FilterRule);

			if (!copyTemplate.S9_IsPublished)
			{
				query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, copyTemplate.S9_RelatedEntityID);
			}
			query.AddToFilter(StmModuleFilterSchema.PK, SQLComparisonOperator.NotEqual, copyTemplate.PK);

			if (copyTemplate.Factory.LoadTop1<UniversalCopyTemplate>(query) != null)
			{
				copyTemplate.S9_FilterNameInfo.AddError(Res.GetString("802b3457-8a0b-4ad9-bc61-901096b2123d", "A copy template with same name already exists in current scope."));
			}
		}

		void CheckConflictingNames()
		{
			if (copyTemplate.CopyTemplateTree == null)
			{
				return;
			}

			var templateWithoutHotKey = copyTemplate.CopyTemplateTree.ConfigurationName.Replace("&", string.Empty);

			foreach (var otherTemplate in copyTemplate.LoadAllTemplateForCurrentModule())
			{
				var otherTemplateWithoutHotKey = otherTemplate.CopyTemplateTree.ConfigurationName.Replace("&", string.Empty);

				if (otherTemplateWithoutHotKey.StartsWith(templateWithoutHotKey + "\\", StringComparison.InvariantCultureIgnoreCase) ||
					templateWithoutHotKey.StartsWith(otherTemplateWithoutHotKey + "\\", StringComparison.InvariantCultureIgnoreCase))
				{
					copyTemplate.S9_FilterNameInfo.AddError(Res.GetString("69bb16d4-9530-471a-b299-ccb0578ac21f",
						"The copy template name conflicts with existing copy template '{0}'. One template name should not be same as beginning part of other (character '&' is ignored).",
						otherTemplate.CopyTemplateTree.ConfigurationName));
					break;
				}
			}
		}

		void CheckOnlyOneHotKey()
		{
			if (copyTemplate.CopyTemplateTree != null)
			{
				foreach (var namePart in copyTemplate.CopyTemplateTree.ConfigurationName.ToString().Split('\\'))
				{
					if (!CheckOnlyOneHotKey(namePart))
					{
						break;
					}
				}
			}
		}

		bool CheckOnlyOneHotKey(string namePart)
		{
			if (namePart.Contains("&&") || namePart.Contains("& ") || namePart.EndsWith("&"))
			{
				copyTemplate.S9_FilterNameInfo.AddError(Res.GetString("0256b695-8f29-419c-bb0d-2b523b258677", "Please use ampersand character (&) only to select hotkeys in a copy template name."));
				return false;
			}
			if (namePart.Count(c => c == '&') > 1)
			{
				copyTemplate.S9_FilterNameInfo.AddError(Res.GetString("3a77df79-fc60-44fe-878b-8c3c496b36eb", "Select only 1 hotkey in a copy template name part (separated by backslash '\\')."));
				return false;
			}

			return true;
		}

		void CheckHotKeys()
		{
			if (copyTemplate.CopyTemplateTree != null)
			{
				string[] nameParts = copyTemplate.CopyTemplateTree.ConfigurationName.ToString().Split('\\');
				var allTemplates = copyTemplate.LoadAllTemplateForCurrentModule();
				string prefix = string.Empty;

				for (int i = 0; i < nameParts.Length; i++)
				{
					string hotKey = string.Empty;
					string currentPart = nameParts[i];
					int hotKeyIndex = currentPart.IndexOf('&');
					if (hotKeyIndex >= 0 && hotKeyIndex < (currentPart.Length - 1))
					{
						hotKey = currentPart.Substring(hotKeyIndex, 2);
					}

					if (!string.IsNullOrEmpty(hotKey))
					{
						UniversalCopyTemplate otherTemplate = copyTemplate.FindTemplateWithHotKey(prefix, hotKey, allTemplates);
						var otherTemplatesNamePart = otherTemplate != null ? otherTemplate.CopyTemplateTree.ConfigurationName.ToString().Split('\\')[i] : null;
						if (!string.IsNullOrEmpty(otherTemplatesNamePart) && !otherTemplatesNamePart.Equals(currentPart, StringComparison.InvariantCultureIgnoreCase))
						{
							if (nameParts.Length > 1)
							{
								copyTemplate.S9_FilterNameInfo.AddError(
									Res.GetString("0bad39c2-17cb-4eae-8c9a-4bbf99ab6abc", "Hotkey '{0}' from part '{1}' is already used in copy template '{2}' in part '{3}'.",
										hotKey, currentPart, otherTemplate.CopyTemplateTree.ConfigurationName, otherTemplatesNamePart));
							}
							else
							{
								copyTemplate.S9_FilterNameInfo.AddError(
									Res.GetString("8f0ca188-9314-44e5-83c0-fd070fff51c9", "Hotkey '{0}' is already used in copy template '{1}'.",
										hotKey, otherTemplate.CopyTemplateTree.ConfigurationName));
							}
						}
					}

					if (i < (nameParts.Length - 1))
					{
						prefix += currentPart + @"\";
					}
				}
			}
		}

		#endregion

		#endregion

		#region MyRegion

		protected override void CheckS9_FilterDataIsValidZBlobSize()
		{
			// Universal Copy Template can be very big, do not check
		}

		#endregion
	}

	#endregion
}
