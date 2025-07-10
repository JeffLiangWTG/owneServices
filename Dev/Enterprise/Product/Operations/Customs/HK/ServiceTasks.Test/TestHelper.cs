using System;
using System.IO;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class TestHelper
	{
		public static string CreateDirectory()
		{
			var fullPath = Path.Combine(Env.TempPath, ZGuid.NewZGuid().ToString());
			Directory.CreateDirectory(fullPath);
			return fullPath;
		}

		public static void SetUpRegistry(Guid companyPK, string senderID, string password, string outPath)
		{
			HKDataRegistry.Instance.HKTraxonSenderID.SetValue(companyPK, Guid.Empty, Guid.Empty, senderID);
			HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetValue(companyPK, Guid.Empty, Guid.Empty, password);
			HKDataRegistry.Instance.HKTraxonOutputDirectory.SetValue(companyPK, Guid.Empty, Guid.Empty, outPath);
		}
	}
}
