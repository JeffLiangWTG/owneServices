using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestsSubclassesOf(typeof(MessageProcessor<,>))]
	public abstract class MessageProcessorAbstractTest<TMessageProcessor, TEDIMessage, TDataProvider> : TestCaseWithFactory
		where TMessageProcessor : MessageProcessor<TEDIMessage, TDataProvider>
		where TEDIMessage : InboundEDIMessage
	{
		public void TestMessageFriendlyName()
		{
			AssertEquals("Message Processor should have a Correct FriendlyName.", MessageFriendlyName, Processor.MessageFriendlyName);
		}

		protected abstract ZString MessageFriendlyName { get; }

		protected abstract TMessageProcessor Processor { get; }

		protected GlbCompany Company
		{
			get
			{
				if (company == null)
				{
					company = Factory.New<GlbCompany>();
					company.GC_Code = "CIE";
					company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
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
					branch.GB_Code = "BIE";
					branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;
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
