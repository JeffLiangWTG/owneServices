using System;

using Enterprise.CryptoUtilities;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class FileUpLoaderX509CertificateRegistryEditorInfo : RegistryEditorInfo
	{
		public FileUpLoaderX509CertificateRegistryEditorInfo()
		{
		}

		public FileUpLoaderX509CertificateRegistryEditorInfo(IRegistryItem privateKeyRegistryItem)
		{
			if (privateKeyRegistryItem == null)
			{
				throw new ArgumentException("PrivateKeyRegistryItem cannot be null.");
			}
			else if (!(privateKeyRegistryItem.DataType is StringRegistryDataType))
			{
				throw new ArgumentException("The DataType of PrivateKeyRegistryItem must be a StringRegistryDataType.");
			}

			IsPrivateKeyPair = true;
			PrivateKeyRegistryItem = privateKeyRegistryItem;
		}

#if DEBUG
		virtual
#endif
		public ICryptoContainer GetCertificate(byte[] certificateData)
		{
			ICryptoContainer result = null;

			if (!IsPrivateKeyPair)
			{
				result = new Certificate(certificateData);
			}
			else
			{
				string password = (string)new RegistryItemProposedValueAccessor(PrivateKeyRegistryItem, FallbackLevel).GetFallBackValue().Value;
				bool storeCertificatesUnderCurrentUser = CargoWise.Application.ObjectFactory.Get<ISystemDataRegistry>().StoreCertificatesUnderCurrentUser;
				result = new Store(certificateData, password, storeCertificatesUnderCurrentUser);
			}

			return result;
		}

		public override Type BaseDataTypeToBeEdited
		{
			get { return typeof(BinaryRegistryDataType); }
		}

		public FallbackLevel FallbackLevel;
		readonly bool IsPrivateKeyPair;
		readonly IRegistryItem PrivateKeyRegistryItem;
	}
}
