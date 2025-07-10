using System;
using System.Threading;
using CargoWise.Licensing;
using CargoWise.Licensing.Registration;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProductRegistration.Client
{
	public interface IRegistrationKeyProvider
	{
		RegistrationKeyXmlPair KeyXmlPair { get; }
		void SetKey(string xml);
	}

	public class RegistrationKeyProvider : IRegistrationKeyProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public RegistrationKeyXmlPair KeyXmlPair
		{
			get
			{
				string newKeyEncrypted = RawDataRegistry.Instance.EncryptedRegistrationKey.Value;

				if (currentKeyEncypted != newKeyEncrypted)
				{
					var newPair = RegistrationKeyUtility.GetRegistrationKeyXmlPair(newKeyEncrypted);

					if (0 != Interlocked.Exchange(ref currentKeyLock, 1))
					{
						// we lost a race, don't use cache and don't wait for the other thread
						return newPair;
					}

					current = newPair;
					currentKeyEncypted = newKeyEncrypted;
					Interlocked.Exchange(ref currentKeyLock, 0);
				}

				return current;
			}
		}

		string currentKeyEncypted;
		RegistrationKeyXmlPair current;
		int currentKeyLock;

		public void SetKey(string xml)
		{
			if (string.IsNullOrEmpty(xml))
			{
				RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			}
			else
			{
				var key = RegistrationKeyUtility.DeserializeIfAuthentic(xml);
				if (key != null)
				{
					var encryptedKey = TwoWayEncoder.NewWithStandardInitialisationVector().Encrypt(xml);
					RawDataRegistry.Instance.EncryptedRegistrationKey.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, encryptedKey);
					UpdateOtherRegistriesIfNeeded(key);
				}
			}
		}

		internal static void UpdateOtherRegistriesIfNeeded(IRegistrationKey key)
		{
			var rawReg = RawDataRegistry.Instance;
			if (key.EnterpriseCode != rawReg.SystemEnterpriseCode.Value)
			{
				rawReg.SystemEnterpriseCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, key.EnterpriseCode);
			}

			if (key.ServerCode != DataRegistry.Instance.PhysicalServerID)
			{
				DataRegistry.Instance.PhysicalServerID = key.ServerCode;
			}

			EnvProxy.Instance.CurrentCompany?.UpdateLicenceKeyIdentifier();
		}
	}
}
