using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public abstract class TagSecurityBase
	{
		#region Implementation

		protected Dictionary<ZGuid, DisplayableIdentified> GetOwnerGroups(BusinessObjectFactory factory)
		{
			var query = new ZDBOnlyQuery(typeof(GlbGroup));
			var subQuery = new ZDBOnlySubQuery(typeof(ITagMagnitude), TagMagnitudeSchema.TGM_GG_OwnerGroup);
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.OrderBy = GlbGroupSchema.GG_Code.Name;

			var ownerGroups = factory.Load<GlbGroup>(query).Select(bizo => new DisplayableIdentified(bizo.PK, bizo.GG_Code)).ToDictionary(d => d.PK);
			ownerGroups.Add(ZGuid.Empty, new DisplayableIdentified(ZGuid.Empty, Res.GetString("A367972E-C340-4FF4-928F-B9E388B506D8", "Not Specified")));

			return ownerGroups;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected IEnumerable<IGrouping<ZGuid, ITagMagnitude>> GetActiveTags(BusinessObjectFactory factory, bool isWorkQueue)
		{
			var tagsQuery = new ZDBOnlyQuery(typeof(ITagMagnitude));

			var tagDefinitionQuery = new ZDBOnlySubQuery(typeof(ITagDefinition), TagMagnitudeSchema.TGM_TGD_Tag);
			tagDefinitionQuery.AddToFilter(TagDefinitionSchema.TGD_UsageScope, SQLComparisonOperator.Equal, new string[]
			{
				TagUsageScopeList.Codes.All,
				TagUsageScopeList.Codes.User,
			});

			var codeComparisonOperator = isWorkQueue ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			tagDefinitionQuery.AddToFilter(TagDefinitionSchema.TGD_Code, codeComparisonOperator, "QUE");

			tagsQuery.AddSubQuery(tagDefinitionQuery, JoinCondition.And);

			return factory.Load<ITagMagnitude>(tagsQuery).GroupBy(g => g.TGM_GG_OwnerGroup);
		}

		protected static bool CheckSecurity(ISecurityCheckpoint parentCheckpoint, string childCode, bool showSecurityDialog = true)
		{
			if (parentCheckpoint != null)
			{
				var checkpoint = parentCheckpoint.FindChild(childCode);

				var checkpointFound = checkpoint != null;
				if (!checkpointFound)
				{
					checkpoint = parentCheckpoint;
				}

				if (!checkpoint.IsAllowed)
				{
					if (showSecurityDialog)
					{
						checkpoint.ShowError();
					}

					return false;
				}
			}

			return true;
		}

		#endregion

		#region DTOs

		protected sealed class DisplayableIdentified
		{
			internal DisplayableIdentified(ZGuid pk, string displayName)
			{
				PK = pk;
				DisplayName = displayName;
			}

			internal readonly ZGuid PK;
			internal readonly string DisplayName;
		}

		#endregion
	}
}
