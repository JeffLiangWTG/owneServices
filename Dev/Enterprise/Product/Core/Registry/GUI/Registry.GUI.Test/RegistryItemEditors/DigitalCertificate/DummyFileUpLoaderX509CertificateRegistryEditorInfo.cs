using System;
using Enterprise.CryptoUtilities;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI.Testing
{
	sealed class DummyFileUpLoaderX509CertificateRegistryEditorInfo : FileUpLoaderX509CertificateRegistryEditorInfo
		{
			public DummyFileUpLoaderX509CertificateRegistryEditorInfo()
			{
			}

			public DummyFileUpLoaderX509CertificateRegistryEditorInfo(IRegistryItem privateKeyRegistryItem) : base(privateKeyRegistryItem)
			{
			}

			public override ICryptoContainer GetCertificate(byte[] certificateData)
			{
				throw new Exception("GetCertificate() threw an exception!");
			}
		}
}
