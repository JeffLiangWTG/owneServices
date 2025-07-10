using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransValidateEnvironmentTest : TestCaseWithFactory
	{
		public void TestValidateExport()
		{
			var notify = new NotificationBuffer();
			Assert("Environment should not be valid", !CaroTransValidateEnvironment.ValidateExport(notify));
			Assert(notify.AsString.Contains("Error: CaroTrans agents not specified."));
			notify.Clear();
			MFIDataRegistry.Instance.CaroTransAgents = new Guid[] { Factory.New(typeof(OrgHeader)).PK.ToGuid() };
			Assert("Environment should not be valid", !CaroTransValidateEnvironment.ValidateExport(notify));
			Assert(notify.AsString.Contains("Error: CaroTrans track email address invalid or empty."));
			notify.Clear();
			MFIDataRegistry.Instance.CaroTransTrackEmailAddress = "carotrans@test.com";
			Assert("Environment should be valid", CaroTransValidateEnvironment.ValidateExport(notify));
			Assert(string.IsNullOrEmpty(notify.AsString));
		}
	}
}
