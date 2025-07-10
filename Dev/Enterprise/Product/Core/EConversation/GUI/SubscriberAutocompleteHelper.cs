using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.GUI
{
	public class SubscriberAutocompleteHelper : IAutoCompleteField
	{
		readonly BusinessObjectFactory factory;
		readonly IConversationProvider provider;

		public char MagicChar => '@';

		public SubscriberAutocompleteHelper(BusinessObjectFactory factory, IConversationProvider provider)
		{
			this.factory = factory;
			this.provider = provider;
		}

		[SuppressWeaklyTypedCollectionMessage]
		public IList GetList(string partialResult)
		{
			return LoadResults<GlbStaff>(GlbStaffSchema.GS_FullName, GlbStaffSchema.GS_IsActive, partialResult)
				.Concat(LoadResults<GlbGroup>(GlbGroupSchema.GG_Desc, GlbGroupSchema.GG_IsActive, partialResult))
				.Concat(FilterAdditionalParticipants(partialResult))
				.ToList();
		}

		IEnumerable<SubscriberWrapper> FilterAdditionalParticipants(string partialResult)
		{
			var existingParties = provider.eConversation.RelatedParties
				.Where(p => p.Parent != null)
				.Select(p => new SubscriberWrapper(p.Parent));

			return provider.AdditionalParticipants
				.Where(party => party.Participant != null)
				.Select(bizo => new SubscriberWrapper(bizo.Participant, bizo.Relation?.ToString() ?? string.Empty))
				.Concat(existingParties)
				.Distinct()
				.Where(NameOrRelationContains(partialResult))
				.Where(bizo => bizo.Parent.IsActive)
				.Take(10);
		}

		Func<SubscriberWrapper, bool> NameOrRelationContains(string partialResult)
		{
			partialResult = partialResult.ToUpperInvariant();
			return participant =>
			{
				var participantName = participant.Parent.Name.ToUpperInvariant();
				if (participantName.Contains(partialResult, StringComparison.CurrentCultureIgnoreCase))
				{
					return true;
				}

				var relation = participant.Relation?.ToUpperInvariant();
				return relation != null && relation.Contains(partialResult);
			};
		}

		IEnumerable<SubscriberWrapper> LoadResults<T>(SchemaStringColumn column, SchemaBoolColumn isActiveColumn, string partialResult) where T : BusinessObject, IConversationParticipant
		{
			var query = new ZQuery(column, SQLComparisonOperator.StartsWith, partialResult)
				.AddToFilter(isActiveColumn, true);

			query.MaximumRows = 10;

			return factory.Load<T>(query).Select(bizo => new SubscriberWrapper(bizo));
		}
	}

	public class SubscriberWrapper : ICodeDescription
	{
		public IConversationParticipant Parent { get; }
		public string Relation { get; }

		public SubscriberWrapper(IConversationParticipant parent, string relation = null)
		{
			Parent = parent;
			Relation = relation;
		}

		public string Code => Parent.DisplayText;
		public string Description => Parent.Name;
		public object PK => Parent.Code;

		public override bool Equals(object obj)
		{
			SubscriberWrapper wrapper;
			return ReferenceEquals(this, obj) || (
				(wrapper = obj as SubscriberWrapper) != null &&
				wrapper.Parent == Parent);
		}

		public override int GetHashCode()
		{
			return PK.GetHashCode();
		}
	}
}
