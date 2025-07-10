using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal abstract class LookupBuilderBase : FilterBuilder, ICustomBuilder
	{
		const string Relates = FilterBuilderPropertyCodeDescriptionList.Codes.Relates;
		public LookupBuilderBase(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(Relates);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		protected internal void SetupListAndModuleIDs(string lookupTypeName, FilterField newField, StringTreeNode fieldTree = null)
		{
			CollectionProvider collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(fBusinessObjectFactory, lookupTypeName.ToLowerInvariant().Trim());
			if (collectionProvider != null)
			{
				if (newField is LookupFilterFieldBase newFieldBase)
				{
					newFieldBase.SetCollectionProvider(collectionProvider);
				}
			}
			else
			{
				throw new TemplateDefinitionException(string.Format(CultureInfo.InvariantCulture, @"Unknown lookup type ""{0}""", lookupTypeName), (FilterTree ?? fieldTree).FindChild("type").Child().CellReference);
			}
		}

		public virtual void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			string lookupTypeName = GetLookupTypeName(fieldTree);
			SetupListAndModuleIDs(lookupTypeName, newField, fieldTree);
			if (fieldTree.ChildExists(Relates))
			{
				string relatedTo = fieldTree.FindChild(Relates).Child().Value;
				string relationType = fieldTree.FindChild(Relates).Child().Child().Value;
				((LookupFilterFieldBase)newField).AddMasterRelation(relatedTo, relationType);
			}

			ProcessFieldTreeBeforeValidation(fieldTree, newField);

			((LookupFilterFieldBase)newField).CollectionProvider.AddValidationAndDefault(newField, fValidators);
		}

		protected virtual void ProcessFieldTreeBeforeValidation(StringTreeNode fieldTree, FilterField newField)
		{
		}

		public virtual void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		protected internal string GetLookupTypeName(StringTreeNode fieldTree)
		{
			return Regex.Match(fieldTree.FindChild((NoResString)"type").Child().Value, @"(.*)\s+" + RegularExpressionToMatchFilterType, RegexOptions.IgnoreCase).Groups[1].Value;
		}

		protected abstract string RegularExpressionToMatchFilterType { get; }
	}
}
