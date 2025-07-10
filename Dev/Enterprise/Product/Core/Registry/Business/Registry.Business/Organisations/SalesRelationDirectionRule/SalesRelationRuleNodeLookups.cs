using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class SalesRelationRuleNodeLookups : ZLookups
	{
		public SalesRelationRuleNodeLookups(SalesRelationRuleNode node)
			: base(node)
		{
		}

		public CodeDescriptionPairList AllActivityTypes
		{
			get
			{
				var list = new CodeDescriptionPairList();
				var affectedTypes = new HashSet<string>(SalesRelationDirectionRuleCollection.AffectedActivityTypes, StringComparer.OrdinalIgnoreCase);
				foreach (ICodeDescription type in SalesRelationTypeList.New())
				{
					if (affectedTypes.Contains(type.Code))
					{
						list.Add(type);
					}
				}
				list.SortByDescription();

				list.AddPair(SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity, SalesRelationRuleNodeAdditionalTypesList.Descriptions.AnySingleActivity);
				list.AddPair(SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities, SalesRelationRuleNodeAdditionalTypesList.Descriptions.AnyNumberOfActivities);

				return list;
			}
		}
	}
}
