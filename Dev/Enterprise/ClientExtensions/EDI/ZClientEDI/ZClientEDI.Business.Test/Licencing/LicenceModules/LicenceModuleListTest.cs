using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class LicenceModuleListTest : TestCaseWithFactory
	{
		public void TestSingleton()
		{
			LicenceModuleList list1 = LicenceModuleList.Instance;
			LicenceModuleList list2 = LicenceModuleList.Instance;
			Assert("Both objects must be the same", list1.Equals(list2));
		}

		public void TestNames()
		{
			var checkPoints = (new LegacyLicence()).GetAllModuleCheckpoints();
			Assert(checkPoints.Count() == LicenceModuleList.Instance.Names.Count);
		}

		public void TestNamesIncludingChildren()
		{
			var checkPoints = (new LegacyLicence()).GetAllCheckpoints();
			Assert(checkPoints.Count == LicenceModuleList.Instance.NamesIncludingChildren.Count);
		}

		public void TestGetIndentedDescriptionFromCode()
		{
			LicenceModuleList listOfModules = LicenceModuleList.Instance;
			AssertEquals("Core is 0 indented", "Core", listOfModules.GetIndentedDescriptionFromCode(LegacyLicence.Instance.Core.Name));
			AssertEquals("Forwarder is 1 indented", "Forwarder".PadLeft("Forwarder".Length + LicenceModuleList.IndentLevel, LicenceModuleList.IndentCharacter), listOfModules.GetIndentedDescriptionFromCode(Env.Licence.Forwarder.Name));
			AssertEquals("DocManager Scanning Station is 2 indented", "DocManager ScanningStation".PadLeft("DocManager ScanningStation".Length + LicenceModuleList.IndentLevel * 2, LicenceModuleList.IndentCharacter), listOfModules.GetIndentedDescriptionFromCode(Env.Licence.DocManagerScanningStation.Name));
		}

		public void TestGetDescriptionFromCode()
		{
			LicenceModuleList modules = LicenceModuleList.Instance;

			foreach (var checkPoint in new LegacyLicence().GetAllModuleCheckpoints())
			{
				AssertEquals(checkPoint.Name, modules.Names.GetDescriptionFromCode(checkPoint.Name), modules.GetDescriptionFromCode(checkPoint.Name));
			}
		}

		public void TestInternalNames()
		{
			var checkPoints = LegacyLicence.Instance.GetAllModuleCheckpoints().ToArray();
			LicenceModuleList modules = LicenceModuleList.Instance;
			AssertEquals(checkPoints.Length, modules.Names.Count);

			for (int i = 0; i < checkPoints.Length; ++i)
			{
				var checkPoint = checkPoints[i];
				AssertEquals(checkPoint.Name, checkPoint.DisplayName, modules.Names[i].Description);
			}
		}

		public void TestGetDescription()
		{
			LegacyLicence licences = new LegacyLicence();
			LicenceModuleList modules = LicenceModuleList.Instance;
			AssertEquals(licences.Core.DisplayName, modules.GetDescription("COR"));

			AssertEquals(licences.ImportBroker.DisplayName, modules.GetDescription("IBR"));
		}
	}
}
