using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestsSubclassesOf(typeof(EMCSBranchCustomsMessageProcessor<,>))]
	abstract class MessageProcessorAbstractTest<TMessageProcessor, TEDIMessage, TDataProvider> : TestCaseWithFactory where TMessageProcessor : EMCSBranchCustomsMessageProcessor<TEDIMessage, TDataProvider> where TEDIMessage : EMCSInboundEDIMessage
	{
		protected ZString MessageFriendlyName => Processor.MessageFriendlyName;

		protected abstract TMessageProcessor Processor { get; }

		protected GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = Factory.New<GlbCompany>();
					company.GC_Code = "CGB";
					company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				}
				return company;
			}
		}
		GlbCompany company;

		protected GlbBranch Branch
		{
			get
			{
				if (branch == null)
				{
					branch = Company.Branches.AddNew();
					branch.GB_Code = "BGB";
					branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				}
				return branch;
			}
		}
		GlbBranch branch;

		protected void ProcessMessage(TEDIMessage message)
		{
			using (message.Factory.AddDisposableService())
			{
				Processor.PreProcessMessage(message);
				if (message.EM_Status != EDIMessage.Status.Error)
				{
					Processor.ProcessMessage(message);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}
		protected LoggingInformation logger;
	}
}
