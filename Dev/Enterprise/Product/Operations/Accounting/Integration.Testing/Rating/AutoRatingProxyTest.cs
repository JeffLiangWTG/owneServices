using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Integration.Testing
{
	internal sealed class AutoRatingProxyTest : TestCaseWithFactory
	{
		public void TestNonExpander()
		{
			var autoRating = new Mock<IAutoRating>().Object;

			var proxy = new AutoRatingProxy(autoRating);
			AssertEquals("CanExpandMacros", false, proxy.CanExpandMacros);
			AssertEquals("ExpandMacros", null, proxy.ExpandMacro("BOB"));
		}

		public void TestExpander()
		{
			var mockExpander = new Mock<IAutoRating>().As<IAutoRatingDescriptionMacroExpander>();

			mockExpander.Setup(x => x.CanExpandMacros).Returns(false);
			mockExpander.Setup(x => x.ExpandMacro("BOB")).Returns((string)null);

			var proxy = new AutoRatingProxy((IAutoRating)mockExpander.Object);
			AssertEquals("CanExpandMacros", false, mockExpander.Object.CanExpandMacros);
			AssertEquals("ExpandMacros", null, mockExpander.Object.ExpandMacro("BOB"));

			mockExpander.Setup(x => x.CanExpandMacros).Returns(true);
			mockExpander.Setup(x => x.ExpandMacro("BOB")).Returns("blat");

			proxy = new AutoRatingProxy((IAutoRating)mockExpander.Object);
			AssertEquals("CanExpandMacros", true, mockExpander.Object.CanExpandMacros);
			AssertEquals("ExpandMacros", "blat", mockExpander.Object.ExpandMacro("BOB"));
		}

		public void TestUpdateContractNumbers()
		{
			var mockAutoRating = new Mock<IAutoRating>();
			var mockJobDataUpdater = mockAutoRating.As<IJobDataUpdater>();

			var proxy = new AutoRatingProxy(mockAutoRating.Object);
			var updateToken = new UpdateCarrierContractNumberToken(new[] { "Magic1" });
			proxy.UpdateCarrierContractNumber(updateToken);
			proxy.UpdateClientContractNumber(new[] { "Magic2" });

			mockJobDataUpdater.Verify(x => x.UpdateCarrierContractNumber(updateToken), Times.Once);
			mockJobDataUpdater.Verify(x => x.UpdateClientContractNumber(new[] { "Magic2" }), Times.Once);

			Assert("Moq verified but DAT thinks this is an empty test.", true);
		}

		public void TestJobNumber()
		{
			var autoRating = new Mock<IAutoRating>();
			var jobInvoicingSupporter = new Mock<IJobInvoicingSupporter>();
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();

			autoRating.Setup(x => x.InvoicingSupporter).Returns((IJobInvoicingSupporter)null);
			var proxy = new AutoRatingProxy(null);
			AssertEquals("No AutoRating", "", proxy.JobNumber);

			proxy = new AutoRatingProxy(autoRating.Object);
			AssertEquals("AutoRating does not have a Job Invoicing Supporter", "", proxy.JobNumber);

			autoRating.Setup(x => x.InvoicingSupporter).Returns(jobInvoicingSupporter.Object);
			jobInvoicingSupporter.Setup(x => x.Job).Returns((JobHeader)null);
			proxy = new AutoRatingProxy(autoRating.Object);
			AssertEquals("jobInvoicingSupporter does not have a Job", "", proxy.JobNumber);

			autoRating.Setup(x => x.InvoicingSupporter).Returns(jobInvoicingSupporter.Object);
			jobInvoicingSupporter.Setup(x => x.Job).Returns(jobHeader);
			proxy = new AutoRatingProxy(autoRating.Object);
			AssertEquals("jobHeader does not have a Parent", "", proxy.JobNumber);

			jobHeader.Parent = jobHeaderParent;
			proxy = new AutoRatingProxy(autoRating.Object);
			AssertEquals("parent does not have an ID", "JobNumber", proxy.JobNumber);
		}

		[TestDate(2023, 4, 4)]
		public void TestUpdateAutoratingDate_UsingIJobDataUpdater()
		{
			var mockAutoRating = new Mock<IAutoRating>();
			var mockJobDataUpdater = mockAutoRating.As<IJobDataUpdater>();

			var proxy = new AutoRatingProxy(mockAutoRating.Object);
			proxy.UpdateAutoratingDate(ZDate.Today, isCosting: true);
			mockJobDataUpdater.Verify(x => x.UpdateAutoratingDate(It.IsAny<ZDate>(), true), Times.Once);

			proxy.UpdateAutoratingDate(ZDate.Today, isCosting: false);
			mockJobDataUpdater.Verify(x => x.UpdateAutoratingDate(It.IsAny<ZDate>(), false), Times.Once);

			Assert("This test uses Moq to verify the calls for IJobDataUpdater.", condition: true);
		}

		#region WarehouseFallbackConsignorForFilterOnly

		public void TestWarehouseFallbackConsignorForFilterOnly_Getter()
		{
			var mockAutoRating = new Mock<IAutoRating>();
			var mockAutoRatingWarehouseInfo = mockAutoRating.As<IAutoRatingWarehouseInfo>();

			var someClient = Factory.New<OrgHeader>();

			mockAutoRatingWarehouseInfo.Setup(m => m.WarehouseFallbackConsignorForFilterOnly).Returns(someClient);

			var proxy = new AutoRatingProxy(mockAutoRating.Object);

			AssertEquals(someClient, proxy.WarehouseFallbackConsignorForFilterOnly);
			mockAutoRatingWarehouseInfo.Verify(m => m.WarehouseFallbackConsignorForFilterOnly, Times.Once);

			AssertEquals("Result should be cached.", someClient, proxy.WarehouseFallbackConsignorForFilterOnly);
			mockAutoRatingWarehouseInfo.Verify(m => m.WarehouseFallbackConsignorForFilterOnly, Times.Once);
		}

		public void TestWarehouseFallbackConsignorForFilterOnly_Setter()
		{
			var mockAutoRating = new Mock<IAutoRating>();

			var someClient = Factory.New<OrgHeader>();

			var proxy = new AutoRatingProxy(mockAutoRating.Object);

			proxy.ValuesCanBeSet = true;
			AssertNoExceptionThrown(() => proxy.WarehouseFallbackConsignorForFilterOnly = someClient);
			AssertEquals(someClient, proxy.WarehouseFallbackConsignorForFilterOnly);

			proxy.ValuesCanBeSet = false;

			AssertExceptionThrown<NotSupportedException>(
				"Correct exception should be thrown.",
				"You cannot set values on this object unless ValuesCanBeSet is true.",
				() => proxy.WarehouseFallbackConsignorForFilterOnly = null);
			AssertEquals(someClient, proxy.WarehouseFallbackConsignorForFilterOnly);
		}

		#endregion
	}
}
