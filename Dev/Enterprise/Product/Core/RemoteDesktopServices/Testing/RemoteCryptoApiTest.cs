using System.Security.Cryptography;
using System.Text;
using CargoWise.Cryptoki.Common.ClientServerApi;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public class RemoteCryptoApiTest : RemoteDesktopServicesTest
	{
		public void TestSupported()
		{
			Assert(RemoteCryptoApi.IsSupported);
		}

		public void TestInstance()
		{
			byte[] text = Encoding.ASCII.GetBytes("ABC");
			byte[] expectedHash;

			using (var sha256 = SHA256.Create())
			{
				expectedHash = sha256.ComputeHash(text);
			}

			var instance = RemoteCryptoApi.Instance;
			var isRemote = RemoteCryptoApi.IsRemote;
			var expectedType = isRemote ? typeof(CryptoApiClient) : typeof(CryptoApi);

			AssertType($"isRemote={isRemote}", expectedType, instance);
			AssertArrayEqualsByElements(expectedHash, instance.Sha256(text));
		}

		public void TestRemote()
		{
			byte[] text = Encoding.ASCII.GetBytes("123");
			byte[] expectedHash;

			using (var sha256 = SHA256.Create())
			{
				expectedHash = sha256.ComputeHash(text);
			}

			AssertArrayEqualsByElements(expectedHash, new CryptoApiClient(RemoteCryptoApi.Send).Sha256(text));
		}
	}
}