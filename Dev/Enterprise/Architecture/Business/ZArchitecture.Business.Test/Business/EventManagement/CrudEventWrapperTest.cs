using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CrudEventWrapperTest : TestCaseWithFactory
	{
		public void TestShouldCreateAddNoteEventWhenNoteAdded()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment))) as IStmALogParent;
			var stmALogProvider = new Mock<IStmALogProvider>();
			stmALogProvider.Setup(provider => provider.Logs).Returns(new Logs(shipment));
			var wrapper = new CrudEventWrapper<StmNote>(new NoteForTest(Factory.New<StmNote>(), false), stmALogProvider.Object);
			var log = wrapper.CreateLog(stmEvent => "some reference");
			AssertEquals(AutoEvents.NoteAddedCode, log.SL_SE_NKEvent);
		}

		public void TestShouldCreateDeleteNoteEventWhenNoteDeleted()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment))) as IStmALogParent;
			var stmALogProvider = new Mock<IStmALogProvider>();
			stmALogProvider.Setup(provider => provider.Logs).Returns(new Logs(shipment));
			var wrapper = new CrudEventWrapper<StmNote>(new NoteForTest(Factory.New<StmNote>(), true, true), stmALogProvider.Object);
			var log = wrapper.CreateLog(stmEvent => "some reference");
			AssertEquals(AutoEvents.NoteDeletedCode, log.SL_SE_NKEvent);
		}

		public void TestShouldCreateModifiedNoteEventWhenNoteUpdated()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment))) as IStmALogParent;
			var stmALogProvider = new Mock<IStmALogProvider>();
			stmALogProvider.Setup(provider => provider.Logs).Returns(new Logs(shipment));
			var wrapper = new CrudEventWrapper<StmNote>(new NoteForTest(Factory.New<StmNote>(), true, hasChanges: true), stmALogProvider.Object);
			var log = wrapper.CreateLog(stmEvent => "some reference");
			AssertEquals(AutoEvents.NoteModifiedCode, log.SL_SE_NKEvent);
		}

		public void TestShouldCreateAddedNoteEventWhenNoteUpdatedButNotYetSaved()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment))) as IStmALogParent;
			var stmALogProvider = new Mock<IStmALogProvider>();
			stmALogProvider.Setup(provider => provider.Logs).Returns(new Logs(shipment));
			var wrapper = new CrudEventWrapper<StmNote>(new NoteForTest(Factory.New<StmNote>(), false, hasChanges: true), stmALogProvider.Object);
			var log = wrapper.CreateLog(stmEvent => "some reference");
			AssertEquals(AutoEvents.NoteAddedCode, log.SL_SE_NKEvent);
		}

		public void TestShouldCreateNoEventWhenStmNoteDeletedAndNotYetSaved()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment))) as IStmALogParent;
			var stmALogProvider = new Mock<IStmALogProvider>();
			stmALogProvider.Setup(provider => provider.Logs).Returns(new Logs(shipment));
			var wrapper = new CrudEventWrapper<StmNote>(new NoteForTest(Factory.New<StmNote>(), false, isDeleted: true), stmALogProvider.Object);
			var log = wrapper.CreateLog(stmEvent => "some reference");
			AssertNull(nameof(log), log);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestShouldThrowExceptionIfItemToWrapIsNull()
		{
			new CrudEventWrapper<StmNote>(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestShouldThrowExceptionIfItemToWrapIsNullAndProvidedLogProvider()
		{
			new CrudEventWrapper<StmNote>(null, new Mock<IStmALogProvider>().Object);
		}
	}
}
