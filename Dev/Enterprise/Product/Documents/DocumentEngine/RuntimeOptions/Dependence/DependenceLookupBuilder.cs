using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class DependenceLookupBuilder : LookupBuilderBase, ICustomBuilder
	{
		const string Option = FilterBuilderPropertyCodeDescriptionList.Codes.Option;

		public DependenceLookupBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(Option);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"{lookup type} dependence lookup", Res.GetString("FilterDocumentation|1246859E-1D12-43E8-9630-457C0165D517"
				, @"{0}
With the pre-condition of this filter is a dependence lookup and is set as a dependency filter to another field. This field will load the relevant lookup list if its related field value is set to the dependence value.", @"[Code 1] [lookup type 1]
[Code 2][lookup type 2]"), supportedProperties, true);
		}

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			if (fieldTree.ChildExists(Option))
			{
				foreach (StringTreeNode optionNode in fieldTree.FindChild(Option).Children)
				{
					string code = optionNode.Value;
					string description = (optionNode.Children.Count == 1 ? optionNode.Child().Value : "");
					if (!string.IsNullOrEmpty(description) && description.EndsWith((NoResString)"lookup", StringComparison.InvariantCultureIgnoreCase))
					{
						var collectionName = Regex.Match(description, @"(.*)\s+lookup", RegexOptions.IgnoreCase).Groups[1].Value.ToLower().Trim();
						var collectionProvider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(fBusinessObjectFactory, collectionName);
						if (collectionProvider != null)
						{
							var field = newField as LookupFilterFieldBase;
							if (field != null)
							{
								var provider = field.CollectionProvider as DependenceCollectionProvider;
								if (provider != null)
								{
									var list = provider.List;
									list.Add(code, collectionProvider);
								}
							}
						}
						else
						{
							throw new TemplateDefinitionException(String.Format(@"Unknown lookup type ""{0}""", collectionName), FilterTree.FindChild("type").Child().CellReference);
						}
					}
				}
			}

			newField.ReadOnly = true;
		}

		protected override string RegularExpressionToMatchFilterType
		{
			get { return (NoResString)@"lookup$"; }
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.Equals((NoResString)"dependence lookup", StringComparison.InvariantCultureIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new LookupField(fBusinessObjectFactory);
		}
	}
}
