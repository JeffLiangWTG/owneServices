using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7FilterBusinessObject : FilterStripBusinessObject
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string JobNumber = "Job #";
			public const string MRN = "MRN";
			public const string HasOutboundMessage = "Has Outbound Messages?";
			public const string HasInboundMessage = "Has Inbound Messages?";

			#endregion
		}

		public static class HasMessageFilterItems
		{
			public const string Yes = "Y";
			public const string No = "N";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var jobNumberFilter = result.AddTextFilter(Descriptions.JobNumber, AsycudaManifestHeaderSchema.AMA_JobReference);
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DC315616-C9D7-4D44-9725-8593865D2DC7", Descriptions.JobNumber);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var mRNFilter = result.AddTextFilter(Descriptions.MRN, CusEntryNumSchema.CE_EntryNum)
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			mRNFilter.Category = FilterCategories.NumbersAndReferences;
			mRNFilter.SubGroup = new CusEntryNumSubGroup();
			mRNFilter.MultilingualDescription = ResString.GetMultilingualString("1ec68bf8-a9c2-40a2-bd00-c0614fab0667", Descriptions.MRN);

			var hasOutboundMessageFilter =
				result.AddTextFilter(Descriptions.HasOutboundMessage, GetAsycudaManifestHeadersByOutboundMessageQuery, HasOutboundMessageList);
			hasOutboundMessageFilter.MultilingualDescription = ResString.GetMultilingualString("683a1ecd-183d-49dd-908a-6b976020dda7", Descriptions.HasOutboundMessage);
			hasOutboundMessageFilter.Category = FilterCategories.StatusAndFlags;

			var hasInboundMessageFilter =
				result.AddTextFilter(Descriptions.HasInboundMessage, GetAsycudaManifestHeadersByInboundMessageQuery, HasInboundMessageList);
			hasInboundMessageFilter.MultilingualDescription = ResString.GetMultilingualString("36ab3850-faeb-4fc5-b733-b51ecbe869f0", Descriptions.HasInboundMessage);
			hasInboundMessageFilter.Category = FilterCategories.StatusAndFlags;

			return result;
		}

		class CusEntryNumSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
				var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
				var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
				cusEntryNumQuery.AddToFilter(filter);
				billQuery.AddSubQuery(AsycudaBillSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);
				headerQuery.AddSubQuery(AsycudaManifestHeaderSchema.PK, AsycudaBillSchema.ABL_AMA, billQuery, JoinCondition.And);

				return headerQuery;
			}
		}

		static ZQuery GetAsycudaManifestHeadersByOutboundMessageQuery(ZString value)
		{
			return GetAsycudaManifestHeadersByMessageDirectionQuery(EDIInterchange.Direction.Transmit, value);
		}

		static ZQuery GetAsycudaManifestHeadersByInboundMessageQuery(ZString value)
		{
			return GetAsycudaManifestHeadersByMessageDirectionQuery(EDIInterchange.Direction.Receive, value);
		}

		static ZDBOnlyQuery GetAsycudaManifestHeadersByMessageDirectionQuery(string direction, string hasMessage)
		{
			var query = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));

			if (hasMessage is HasMessageFilterItems.Yes or HasMessageFilterItems.No)
			{
				var subqueryBill = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA, hasMessage == HasMessageFilterItems.No);
				var subqueryMessage = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID);
				subqueryMessage.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, direction);
				subqueryBill.AddSubQuery(subqueryMessage, JoinCondition.And);
				query.AddSubQuery(subqueryBill, JoinCondition.And);
			}
			else if (!string.IsNullOrEmpty(hasMessage))
			{
				query.AddToFilter(AsycudaManifestHeaderSchema.PK, Guid.Empty);
			}

			return query;
		}

		CodeDescriptionPairList HasOutboundMessageList
		{
			get
			{
				if (fHasOutboundMessageList == null)
				{
					fHasOutboundMessageList = new CodeDescriptionPairList();
					fHasOutboundMessageList.AddPair(HasMessageFilterItems.Yes, Res.GetString("8bd40d24-548e-4f45-9d86-54949a086325", "Has Outbound Message"));
					fHasOutboundMessageList.AddPair(HasMessageFilterItems.No, Res.GetString("7f13ae97-d154-4fe2-b8ec-3e1f9edb35bf", "Has No Outbound Message"));
				}

				return fHasOutboundMessageList;
			}
		}

		CodeDescriptionPairList HasInboundMessageList
		{
			get
			{
				if (fHasInboundMessageList == null)
				{
					fHasInboundMessageList = new CodeDescriptionPairList();
					fHasInboundMessageList.AddPair(HasMessageFilterItems.Yes, Res.GetString("d069cf17-e633-49aa-9c2d-c307edfa413a", "Has Inbound Message"));
					fHasInboundMessageList.AddPair(HasMessageFilterItems.No, Res.GetString("23710cbd-9550-4c23-ba10-8040a44da2b7", "Has No Inbound Message"));
				}

				return fHasInboundMessageList;
			}
		}

		CodeDescriptionPairList fHasOutboundMessageList;
		CodeDescriptionPairList fHasInboundMessageList;
	}
}
