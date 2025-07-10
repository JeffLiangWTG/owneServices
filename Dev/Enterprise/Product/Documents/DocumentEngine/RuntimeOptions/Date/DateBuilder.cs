using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class DateBuilder : BaseDateBuilder<ZDateTime>
	{
		public DateBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DateFormat);
			ExpectedProperties.Add(DefaultValue);
		}

		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Date", Res.GetString("FilterDocumentation|4ACE9AE1-ECD1-4C44-BF9E-6823A143657F", "Generates a date filter. Filters data matching the date."), supportedProperties);
		}

		public override bool CanBuild(string filterType) =>
			filterType.Equals((NoResString)"date", StringComparison.OrdinalIgnoreCase);

		protected override FilterField GetFilterField()
		{
			return new DateField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			if (fieldTree.ChildExists(DefaultValue))
			{
				var defaultValueNode = fieldTree.FindChild(DefaultValue);
				var defaultString = defaultValueNode.Child().Value;

				try
				{
					((DateField)(newField)).Value = GetMacroDateReplacement(defaultString);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException("Unknown date format in default value", defaultValueNode.Child().CellReference);
				}
			}
		}

		#endregion
	}
}
