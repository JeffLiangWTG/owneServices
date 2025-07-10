using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.GUI
{
	public class UpdateJobOverseasAgentActionMethodApplicator : UpdateJobActionMethodApplicatorBase
	{
		public UpdateJobOverseasAgentActionMethodApplicator(BusinessObjectFactory factory) : base(factory, "UpdateJobOverseasAgentActionMethodApplicator")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string OverseasAgent = "OverseasAgent";
		}

		#endregion

		protected override string[] UpdateJobProperty(JobHeader job)
		{
			var errors = Array.Empty<string>();

			job.JH_OA_AgentCollectAddr = OverseasAgent;
			if (job.JH_OA_AgentCollectAddrInfo.HasErrors())
			{
				errors = job.JH_OA_AgentCollectAddrInfo.GetErrors().Select(x => x.Message).ToArray();
			}

			return errors;
		}

		protected override bool GetPropertyReadonly(JobHeader job)
		{
			return job.JH_OA_AgentCollectAddrInfo.ReadOnly;
		}

		protected override bool GetValueSameWithPrevious(JobHeader job)
		{
			return job.JH_OA_AgentCollectAddr == OverseasAgent;
		}

		[List("OverseasAgentList")]
		public ZGuid OverseasAgent
		{
			get
			{
				return fOverseasAgent;
			}
			set
			{
				SetNonPersistentPropertyValue(OverseasAgentInfo, ref fOverseasAgent, value);
				if (!IsValidationSuspended)
				{
					ValidateOverseasAgent();
				}
			}
		}
		ZGuid fOverseasAgent;

		public ZPropertyInfo OverseasAgentInfo => GetZPropertyInfo(Schema.OverseasAgent);

		public OrganisationsFindBoxCollection OverseasAgentList
		{
			get
			{
				if (fLocalClientList == null)
				{
					fLocalClientList = Factory.GetCachedValue("UpdateJobOverseasAgent_" + FindboxLookupCollections.CachingKey, () =>
					{
						var collection = new OrganisationsFindBoxCollection(Factory);
						collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property0", ZBool.True)); // It is not clear whether or how it should be replaced by a Resource String
						return collection;
					});

					fLocalClientList.OrganisationType = OrganisationTypes.Debtor;
				}
				return fLocalClientList;
			}
		}
		OrganisationsFindBoxCollection fLocalClientList;

		#region ZAddress

		public ZAddress AgentCollectAddr_ZAddress
		{
			get
			{
				if (fAgentCollectAddr_ZAddress == null)
				{
					fAgentCollectAddr_ZAddress = GetNewAgentCollectAddr_ZAddress();
				}
				return fAgentCollectAddr_ZAddress;
			}
		}
		ZAddress fAgentCollectAddr_ZAddress;

		ZAddress GetNewAgentCollectAddr_ZAddress()
		{
			return new ZAddress(OverseasAgentInfo);
		}

		#endregion

		#region Validation

		protected override void ValidateAll()
		{
			ValidateOverseasAgent();
		}

		void ValidateOverseasAgent()
		{
			OverseasAgentInfo.ClearAllNotifications();
			TypeValidation.CheckValidGuid(OverseasAgentInfo);
			OverseasAgentInfo.RunAdditionalValidation();
		}

		#endregion
	}
}
