using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineIntegration.RollUpSort;
using Enterprise.DocumentWrappers.HelperClasses.DocRollUpSort;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.DocRollUpSort;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Quotation.RollUpSort
{
	sealed class RatingChargeDocRollUpper : BaseDocRollUpper<ZString, MultilingualString, DocLineList>
	{
		public RatingChargeDocRollUpper(IDocHeader header, BusinessObjectFactory factory, DocLineList lines, ZString rollUpStyleId, ZString rollUpGroupId, params ZString[] chargeGroups)
			: base(header, factory, lines)
		{
			this.RollUpStyleId = rollUpStyleId;
			ChargeGroupToGroupIdMap = new Dictionary<ZString, ZString>();
			foreach (var chargeGroup in chargeGroups)
			{
				ChargeGroupToGroupIdMap.Add(chargeGroup, rollUpGroupId);
			}
		}

		public RatingChargeDocRollUpper(IDocHeader header, BusinessObjectFactory factory, DocLineList lines, ZString rollUpStyleId, Dictionary<ZString, ZString> chargeGroupToGroupIdMap)
			: base(header, factory, lines)
		{
			this.RollUpStyleId = rollUpStyleId;
			ChargeGroupToGroupIdMap = chargeGroupToGroupIdMap;
		}

		protected override void OnBeforeRollUp(BaseDocRollUpGroupList<ZString, DocLineList> allGroups)
		{
			var originalOrder = allGroups.ToArray();

			allGroups.Sort((DocRollUpGroup<ZString, DocLineList> docRollUpGroup1, DocRollUpGroup<ZString, DocLineList> docRollUpGroup2) =>
			{
				var result = 0;

				if (docRollUpGroup1.NeedsRollUp != docRollUpGroup2.NeedsRollUp)
				{
					result = docRollUpGroup1.NeedsRollUp ? 1 : -1;
				}
				else if (docRollUpGroup1.NeedsRollUp && docRollUpGroup2.NeedsRollUp && docRollUpGroup1.ID != docRollUpGroup2.ID)
				{
					foreach (var groupId in ChargeGroupToGroupIdMap.Values)
					{
						if (groupId == docRollUpGroup1.ID)
						{
							result = -1;
							break;
						}
						if (groupId == docRollUpGroup2.ID)
						{
							result = 1;
							break;
						}
					}
				}

				if (result == 0 && docRollUpGroup1 != docRollUpGroup2)
				{
					foreach (var group in originalOrder)
					{
						if (group == docRollUpGroup1)
						{
							result = -1;
							break;
						}
						if (group == docRollUpGroup2)
						{
							result = 1;
							break;
						}
					}
				}

				return result;
			});
		}

		ZString RollUpStyleId { get; }

		Dictionary<ZString, ZString> ChargeGroupToGroupIdMap { get; }

		protected override IRolledUpDocLine RollUpGroup(DocLineList group, ZString groupId)
		{
			var result = default(IRolledUpDocLine);

			if (group.Count > 0)
			{
				var descriptionForRolledUpLine = GetDescriptionForRolledUpLine(group, groupId);

				result = new RolledUpDocRateLine(group, descriptionForRolledUpLine);
			}

			return result;
		}

		protected override ZString GetGroupId(IDocLine line)
		{
			var result = ZString.Empty;

			var lineChargeCode = line != null
				? GetChargeCode(line)
				: null;

			if (lineChargeCode != null)
			{
				if (!ChargeGroupToGroupIdMap.TryGetValue(lineChargeCode.AC_ChargeGroup, out result))
				{
					result = ZString.Empty;
				}
			}

			return result;
		}

		protected override MultilingualString GetDescriptionForRolledUpLine(DocLineList group, ZString groupId)
		{
			var chargeGroupCode = groupId;
			return GetDescriptionByStyleAndGroup(RollUpStyleId, chargeGroupCode);
		}

		internal static MultilingualString GetDescriptionByStyleAndGroup(ZString styleId, ZString groupId)
			=> AccountingConfigurationRegistry.Instance.InvoiceRollupAndGroupDescriptionRegistryItem.GetDescription(styleId, groupId);

		AccChargeCode GetChargeCode(IDocLine docLine)
			=> ((DocRateLine)docLine).RateLine.ChargeCode;

		public override DocLineList GetNewLines(BusinessObjectFactory factory)
			=> new DocLineList();

		protected override BaseDocRollUpGroupList<ZString, DocLineList> GetDocRollUpGroupList()
			=> new RatingDocRollUpGroupList();
	}
}
