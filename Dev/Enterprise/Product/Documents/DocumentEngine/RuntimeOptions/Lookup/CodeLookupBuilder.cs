using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class CodeLookupBuilder : LookupBuilderBase
	{
		public CodeLookupBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"{lookup type} Lookup Code", Res.GetString("FilterDocumentation|61001615-306F-4E02-880B-C38F13BE3C97", "Generates a lookup filter whose value will be a code, with available selections from the lookup list. Filters data matching that code."), supportedProperties, true);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.ToLower().EndsWith((NoResString)"lookup code");
		}

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);
			var lookupFilterField = newField as LookupFilterFieldBase;
			var collectionProviderWithCodeSupport = lookupFilterField.CollectionProvider as CollectionProviderWithCodeSupport;

			if (collectionProviderWithCodeSupport == null)
			{
				var fieldTreeChild = fieldTree.FindChild(Type).Child();
				throw new TemplateDefinitionException(Res.GetString("BEB53E90-F8FB-4483-A55B-E2D14D816BCA", "The filter type '{0}' is not configured for code lookups. Please use a lookup without the 'code' option.", fieldTreeChild.Value), fieldTreeChild.CellReference);
			}
		}

		protected override string RegularExpressionToMatchFilterType
		{
			get { return (NoResString)@"lookup code$"; }
		}

		protected override FilterField GetFilterField()
		{
			return new CodeLookupField(fBusinessObjectFactory);
		}
	}
}
