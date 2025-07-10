using System.Globalization;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public abstract class GbInterchangeSender : BaseInterchangeSender
	{
		public override string HumanReadableName
		{
			get
			{
				return "British Customs Declaration Sender.";
			}
		}

		protected GbInterchangeSender(ILogger serviceLogger)
		{
			this.iLogger = serviceLogger;
		}

		protected override void SendOutboundInterchanges(CancellationToken token)
		{
			SendOutboundInterchanges(this.ApplicationCode, token);
		}

		protected sealed override void PackageMessagesIntoInterchanges(NonDependentEDIMessageCollection messages)
		{
			MessageCountInCurrentRun = messages.Count;

			if (MessageCountInCurrentRun > 0)
			{
				iLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Packaging {0} messages into interchanges.", MessageCountInCurrentRun));

				InterchangeProviderBase provider = GetInterchangeProvider(messages);
				EDIInterchange[] interchanges = provider.Interchanges;
				foreach (var interchange in interchanges)
				{
					interchange.EI_Status = StatusMeaningQueued;
				}

				if (iLogger != null)
				{
					for (int i = 0; i < messages.Count; i++)
					{
						iLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "Message {0} has been packaged into interchange.", messages[i].EM_MessageNum));
					}
				}
			}
		}

		public static ZQuery BritishBranches(BusinessObjectFactory sharedFactory)
		{
			return BritishBranches(sharedFactory, EDIMessageSchema.EM_GB);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ZQuery BritishBranches(BusinessObjectFactory sharedFactory, SchemaGuidColumn foreignKeyColumn)
		{
			ZQuery validBranchesForInterchangesFilter = new ZQuery();
			GlbBranch.Loader branchLoader = new GlbBranch.Loader(sharedFactory);
			foreach (GlbBranch branch in branchLoader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.UnitedKingdom))
			{
				validBranchesForInterchangesFilter.AddToFilter(JoinCondition.Or, foreignKeyColumn, SQLComparisonOperator.Equal, branch.PK);
			}
			return validBranchesForInterchangesFilter;
		}

		protected override int NumberToBatch
		{
			get
			{
				return NumberOfMessagesPerInterchange;
			}
		}

		protected override bool IsDateFilterUTC
		{
			get { return true; }
		}

		public abstract int NumberOfMessagesPerInterchange { get; }

		public abstract InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages);

		public abstract string ApplicationCode { get; }

		protected int MessageCountInCurrentRun { get; set; }

		readonly ILogger iLogger;
	}
}
