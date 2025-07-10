//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJobConversationParticipantLookups
//
//    This class should be used for overriding collections in AutoJobConversationParticipantLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.EConversation.Business
{
	public class JobConversationParticipantLookups : AutoJobConversationParticipantLookups
	{
		public JobConversationParticipantLookups(AutoJobConversationParticipant parent) : base(parent)
		{
		}

		protected new JobConversationParticipant Parent => (JobConversationParticipant)base.Parent;

		public ICollection AvailableParentsList
		{
			get
			{
				if (!Equals(parentsList?.Item1, Parent.JCP_ParticipantTableCode))
				{
					parentsList = Tuple.Create(Parent.JCP_ParticipantTableCode, GetParentsForPrefix(Parent.Factory, Parent.JCP_ParticipantTableCode));
				}

				return parentsList.Item2;
			}
		}
		Tuple<ZString, ICollection> parentsList;

		public const string EmailConstant = "EML";

		public CodeDescriptionPairList RelatedPartyTypesList
				=> new CodeDescriptionPairList
					{
						new CodeDescriptionPair(OrgHeaderSchema.Constants.Prefix, ResString.GetMultilingualString("ef4b4ea5-07ff-4a7b-a59c-6ee888028e43", "Organization")),
						new CodeDescriptionPair(OrgContactSchema.Constants.Prefix, ResString.GetMultilingualString("97f17029-9f1f-449f-9173-4f247cffc02c", "Contact")),
						new CodeDescriptionPair(EmailConstant, ResString.GetMultilingualString("cd89412b-f77e-4d9c-a25a-9e416f604def", "Email")),
					};

		static ICollection GetParentsForPrefix(BusinessObjectFactory factory, string prefix)
		{
			switch (prefix)
			{
				case GlbStaffSchema.Constants.Prefix:
					return (ICollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbStaffCollection>(), factory);

				case GlbGroupSchema.Constants.Prefix:
					return (ICollection)Activator.CreateInstance(ObjectFactory.GetType<IGlbGroupCollection>(), factory);

				case OrgHeaderSchema.Constants.Prefix:
					return (ICollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgHeaderCollection>(), factory);

				case OrgContactSchema.Constants.Prefix:
					var contactCollection = (IOrgContactCollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgContactCollection>(), factory);
					contactCollection.UseOrgCodeFilter = true;
					return contactCollection;

				default:
					return new List<IBusiness>();
			}
		}
	}
}