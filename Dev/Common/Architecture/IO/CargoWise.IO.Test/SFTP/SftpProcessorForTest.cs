using System;

namespace CargoWise.IO.Testing;

public class SftpProcessorForTest : SftpProcessor
{
	public SftpProcessorForTest(string remoteDirectory)
	{
		SftpClient = new SftpClientMock(remoteDirectory);
		SftpClient.Connect();
	}

	// Todo: Consideration about using the true sftp server to doing the unit test if sftp server deployed.
	// This is my local sftp server connection info.
	public SftpProcessorForTest()
		: base("172.23.34.61", "sftp", "19972279999cK.", null, TimeSpan.Zero, TimeSpan.Zero)
	{
	}
}


