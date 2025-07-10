using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(VesselProvider))]
	sealed class VesselProviderTest : TestCaseWithFactory
	{
		public void TestCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			declaration.JE_RadioCallSign = "T4";
			var provider = new VesselProvider(declaration, messageSendingObject);
			AssertEquals("T4", provider.Code);
		}

		public void TestName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSendingObject(cusEntryHeader);
			var provider = new VesselProvider(declaration, messageSendingObject);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			messageSendingObject.ProcedureCode = JPProcedureCodeList.Codes.IDA;

			declaration.JE_VesselName = "C1";

			declaration.JE_VoyageFlightNo = "AB0001";
			AssertEquals("AB0001", provider.Name);

			declaration.JE_ExportDate = new ZDateTime(2025, 3, 4);
			AssertEquals("AB0001/04MAR", provider.Name);

			declaration.JE_ExportDate = new ZDateTime(2025, 3, 14);
			AssertEquals("AB0001/14MAR", provider.Name);

			declaration.JE_VoyageFlightNo = "";
			declaration.JE_ExportDate = new ZDateTime(2025, 3, 14);
			AssertEquals("14MAR", provider.Name);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_RadioCallSign = "9999";
			declaration.JE_VesselName = "C1";
			AssertEquals("C1", provider.Name);

			declaration.JE_RadioCallSign = "T4";
			AssertEquals(ZString.Empty, provider.Name);
		}
	}
}
