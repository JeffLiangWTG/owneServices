using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Codes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	public class StmALogAdderTest : TestCaseWithFactory
	{
		public void TestParametersExtractedFromReference()
		{
			var factory = new UniversalObjectFactory(new BusinessObjectFactory() { NameForDebugging = "Universal Message Processing" });
			var forwardingShipment = factory.BOFactory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var contextManager = (IEventDataContextManager)((BusinessObject)forwardingShipment).GetUniversalDataContextManager();
			var logger = new SimpleLogger();
			var logAdder = new StmALogAdder(AutoEvents.CustomisableEvent00, new DataObjects.Universal.Event()
			{
				EventReference = $"|{Codes.ReferenceNumber}=1234|{Codes.Location}=SYD",
			}, contextManager, factory, logger);
			var logParent = (IStmALogParent)forwardingShipment;
			logAdder.AddNewLogToParent(logParent);
			var log = logParent.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00.Code).First();
			AssertEquals(2, log.Parameters.Count);
			AssertEquals("AUSYD", log.Parameters[Codes.Location]);
		}

		public void TestParametersAndReferenceMerged()
		{
			var factory = new UniversalObjectFactory(new BusinessObjectFactory() { NameForDebugging = "Universal Message Processing" });
			var forwardingShipment = factory.BOFactory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			var contextManager = (IEventDataContextManager)((BusinessObject)forwardingShipment).GetUniversalDataContextManager();
			var date = ZDateTime.Now;
			var logger = new SimpleLogger();
			var logAdder = new StmALogAdder(AutoEvents.CustomisableEvent00, new DataObjects.Universal.Event()
			{
				EventReference = $"|{Codes.ReferenceNumber}=1234|{Codes.FlightDate}={date.ToString("G", CultureInfo.InvariantCulture)}",
				EventParameters = new DataObjects.Universal.EventParameters()
				{
					Location = "SYD"
				}
			}, contextManager, factory, logger);
			var logParent = (IStmALogParent)forwardingShipment;
			logAdder.AddNewLogToParent(logParent);
			var log = logParent.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00.Code).First();
			AssertEquals(3, log.Parameters.Count);
			AssertEquals("AUSYD", log.Parameters[Codes.Location]);
			AssertEquals(date.ToISO8601ShortDateString(), log.Parameters[Codes.FlightDate]);
		}
	}
}
