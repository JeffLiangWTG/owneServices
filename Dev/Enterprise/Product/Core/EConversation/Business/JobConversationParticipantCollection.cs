using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.Business
{
	public class JobConversationParticipantCollection : ActiveBusinessObjectCollection<JobConversationParticipant>
	{
		readonly string[] parentTableCodes;

		public JobConversationParticipantCollection(BusinessObjectFactory factory, JobConversation parent)
			: base(factory, parent, null, JobConversationParticipantSchema.JCP_JCC_Conversation)
		{
			parentTableCodes = Array.Empty<string>();
		}

		public JobConversationParticipantCollection(BusinessObjectFactory factory, JobConversation parent, params string[] parentTableCodes)
			: base(factory, parent, new ZQuery(JobConversationParticipantSchema.JCP_ParticipantTableCode, parentTableCodes), JobConversationParticipantSchema.JCP_JCC_Conversation)
		{
			this.parentTableCodes = parentTableCodes;
		}

		#region Adding Participants

		protected override void SetDefaultsForNewElementCore(JobConversationParticipant newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			if (parentTableCodes.Length > 0)
			{
				newElement.JCP_ParticipantTableCode = parentTableCodes.First();
			}
		}

		bool DoesntMatchAnyProvidedParentTable(string tableCode)
		{
			return !string.IsNullOrEmpty(tableCode) && parentTableCodes.Length > 0 && !parentTableCodes.Contains(tableCode);
		}

		void CheckParentMatchesCode(IConversationParticipant parent)
		{
			var parentBizo = (BusinessObject)parent;
			if (DoesntMatchAnyProvidedParentTable(parentBizo.TablePrefix))
			{
				throw new ArgumentException("Parent doesnt match the provided table code. Expected any of ['" + string.Join("', '", parentTableCodes) + "'], Got: " + parentBizo.TablePrefix);
			}
		}

		public JobConversationParticipant GetWithoutAdd(IConversationParticipant parent)
		{
			CheckParentMatchesCode(parent);

			var pk = ((BusinessObject)parent).PK;
			foreach (JobConversationParticipant participant in this)
			{
				if (participant.JCP_ParticipantID == pk)
				{
					return participant;
				}
			}

			return null;
		}

		public JobConversationParticipant GetOrAdd(IConversationParticipant parent)
		{
			return GetWithoutAdd(parent) ?? AddNewParticipant(parent);
		}

		public bool HasParticipant(IConversationParticipant parent)
		{
			return GetWithoutAdd(parent) != null;
		}

		protected override void OnAdded(JobConversationParticipant businessObject)
		{
			base.OnAdded(businessObject);

			if (DoesntMatchAnyProvidedParentTable(businessObject.JCP_ParticipantTableCode))
			{
				throw new ArgumentException("New object must use a provided parent table code");
			}
		}

		public JobConversationParticipant AddNewParticipant(IConversationParticipant participant)
		{
			var result = AddNew();

			var bizo = participant as BusinessObject;

			result.JCP_ParticipantTableCode = bizo.TablePrefix;
			result.JCP_ParticipantID = bizo.PK;

			if ((participant as ZArchitecture.Environment.IUser)?.IsSystemAccount ?? false)
			{
				result.JCP_IsSubscribed = false;
			}

			return result;
		}

		public JobConversationParticipant AddNewParticipant(string email)
		{
			var result = AddNew();

			result.JCP_ParticipantTableCode = "";
			result.JCP_ParticipantID = ZGuid.Empty;
			result.JCP_EmailAddress = email;

			return result;
		}

		#endregion
	}
}
