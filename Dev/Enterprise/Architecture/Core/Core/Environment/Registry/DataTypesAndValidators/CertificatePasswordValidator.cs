using System;
using Enterprise.CryptoUtilities;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class CertificatePasswordValidator : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			byte[] privateKeyBytes = (byte[])((IRegistryItemInternals)PrivateKeyDataRegistryItem).GetProposedValue(companyPK, branchPK, departmentPK)
				?? PrivateKeyDataRegistryItem.Value;

			if (privateKeyBytes != null && privateKeyBytes.Length > 0 && proposedValue != null && proposedValue.Length > 0)
			{
				try
				{
					bool storeCertificatesUnderCurrentUser = CargoWise.Application.ObjectFactory.Get<ISystemDataRegistry>().StoreCertificatesUnderCurrentUser;
					using (var store = new Store(privateKeyBytes, proposedValue, storeCertificatesUnderCurrentUser))
					{
						var state = new CertificateState(store, EnvProxy.Instance.Time.CurrentLocalDateTime);
						if (state.Errors != null && state.Errors.Length > 0)
						{
							string errors = Res.GetString("8f17f62d-9e4b-4eaa-a78c-7305e3414900", "There is something wrong with your Key/Password pair:") + " " + string.Join(", ", state.Errors);
							throw new RegistryValidationException(errors);
						}
					}
				}
				catch (CryptoUtilitiesException ex)
				{
					throw new RegistryValidationException(Res.GetString("bb090539-340d-49b8-81a7-cd0bf302efdd", "There is something wrong with your Key/Password pair:{0}", ex.Message));
				}
			}
		}

		protected BinaryRegistryItem PrivateKeyDataRegistryItem
		{
			get { return EnvProxy.Instance.Registry.RawRegistry.AUCCompanyCertificateData; }
		}
	}
}
