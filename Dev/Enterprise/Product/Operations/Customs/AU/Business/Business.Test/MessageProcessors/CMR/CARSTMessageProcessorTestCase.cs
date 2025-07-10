using System;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CARSTMessageProcessorTestCase : TestCaseWithFactory
	{
		public void TestEmailGroups()
		{
			Guid cargoStatusAcknowledgementEmailGroup = Guid.NewGuid();
			Guid cargoStatusImpedimentEmailGroup = Guid.NewGuid();
			Guid cargoStatusErrorEmailGroup = Guid.NewGuid();
			string cargoStatusSendAcknowledgementsMode = Enterprise.Core.Constants.EmailTo.NoEmails;
			string cargoStatusSendImpedimentsMode = Enterprise.Core.Constants.EmailTo.NominatedGroup;
			string cargoStatusSendErrorMode = Enterprise.Core.Constants.EmailTo.StaffMember;
			CARSTMessageProcessor processor = new CARSTMessageProcessor(new LoggingInformation());
			Env.Registry.AUCustoms.CargoStatusSendAcknowledgements = cargoStatusSendAcknowledgementsMode;
			Env.Registry.AUCustoms.CargoStatusSendAcknowledgementsToGroup = cargoStatusAcknowledgementEmailGroup;
			Env.Registry.AUCustoms.CargoStatusSendErrors = cargoStatusSendErrorMode;
			Env.Registry.AUCustoms.CargoStatusSendErrorsToGroup = cargoStatusErrorEmailGroup;
			Env.Registry.AUCustoms.CargoStatusSendImpediments = cargoStatusSendImpedimentsMode;
			Env.Registry.AUCustoms.CargoStatusSendImpedimentsToGroup = cargoStatusImpedimentEmailGroup;
			AssertEquals("Acknowledgement Mode", cargoStatusSendAcknowledgementsMode, GetProtectedPropertyValue<ZString>(processor, "AcknowledgementEmailMode"));
			AssertEquals("Acknowledgement Email Group", cargoStatusAcknowledgementEmailGroup, GetProtectedPropertyValue<ZGuid>(processor, "AcknowledgementEmailGroup"));
			AssertEquals("Impediments Mode", cargoStatusSendImpedimentsMode, GetProtectedPropertyValue<ZString>(processor, "ImpedimentEmailMode"));
			AssertEquals("Impediments Email Group", cargoStatusImpedimentEmailGroup, GetProtectedPropertyValue<ZGuid>(processor, "ImpedimentEmailGroup"));
			AssertEquals("Error Mode", cargoStatusSendErrorMode, GetProtectedPropertyValue<ZString>(processor, "ErrorEmailMode"));
			AssertEquals("Error Email Group", cargoStatusErrorEmailGroup, GetProtectedPropertyValue<ZGuid>(processor, "ErrorEmailGroup"));
		}

		T GetProtectedPropertyValue<T>(CARSTMessageProcessor processor, string propertyName)
			where T : IZType
		{
			return (T)processor.GetType()
				.InvokeMember(propertyName,
				BindingFlags.GetProperty |
				BindingFlags.NonPublic |
				BindingFlags.Instance,
				null,
				processor,
				null);
		}
	}
}
