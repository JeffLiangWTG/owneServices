using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	abstract class MaxMinDateFieldBuilder : FilterBuilder, ICustomBuilder
	{
		public MaxMinDateFieldBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
		}

		protected abstract string GetPattern();
		protected abstract void SetDefaultValue(FilterField newField, ZDateTime defaultValue);

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, GetPattern(), RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
		}

		#region ICustomBuilder Members

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			ZDateTime value = ZDateTime.Empty;
			string defaultString = "";
			if (fieldTree.ChildExists(DefaultValue))
			{
				StringTreeNode defaultValueNode = fieldTree.FindChild(DefaultValue);
				defaultString = defaultValueNode.Child().Value;
				if (defaultString.Equals((NoResString)"<now>", StringComparison.OrdinalIgnoreCase))
				{
					value = ZDateTime.Now;
				}
				else
				{
					try
					{
						defaultString = defaultString.Replace("<", "").Replace(">", "");
						DateTime defaultTime = DateTime.Parse(defaultString);
						value = new ZDateTime(defaultTime.Year, defaultTime.Month, defaultTime.Day);
					}
					catch
					{
						throw new TemplateDefinitionException("Unknown date format in default value", defaultValueNode.Child().CellReference);
					}
				}

				SetDefaultValue(newField, value);
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion
	}
}
