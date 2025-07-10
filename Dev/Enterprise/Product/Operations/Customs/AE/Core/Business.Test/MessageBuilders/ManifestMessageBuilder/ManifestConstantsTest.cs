using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

public class ManifestConstantsTest : TestCase
{
	public void TestGetManifestCode()
	{
		AssertEquals("F", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.FCL));
		AssertEquals("L", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.LCL));
		AssertEquals("M", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.Empty));
		AssertEquals("B", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.Bulk));
		AssertEquals("Q", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.Liquid));
		AssertEquals("R", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.RollOnRollOff));
		AssertEquals("G", ManifestConstants.CargoCode.GetManifestCode(Core.Constants.ContainerModes.BreakBulk));
		AssertEquals("G", ManifestConstants.CargoCode.GetManifestCode("CRAP"));
	}

	public void TestConstants()
	{
		AssertEquals("MessageType", "MFI", ManifestConstants.MessageType.ToUpper());
		AssertEquals("ManifestFileExtention", "MFT", ManifestConstants.ManifestFileExtention.ToUpper());
	}
}
