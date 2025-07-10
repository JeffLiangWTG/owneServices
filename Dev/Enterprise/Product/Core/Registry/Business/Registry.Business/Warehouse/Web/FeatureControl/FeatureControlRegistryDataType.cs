using System;
using CargoWise.FeatureControl;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class FeatureControlRegistryDataType : StringRegistryDataType
	{
		public FeatureControlRegistryDataType(bool isEncrypted)
		: base(isEncrypted)
		{
		}

		public FeatureControlRegistryDataType()
		: base(string.Empty)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			var isFeatureControlContent = true;
			try
			{
				_ = FeatureControlDeserializer.GetFeatureControlCoreSafe(proposedValue, null);
			}
			catch (Exception)
			{
				isFeatureControlContent = false;
			}

			if (!isFeatureControlContent)
			{
				throw new RegistryValidationException(Res.GetString("FF3B67D9-4C54-45B7-8479-D7B5998F639F", "Invalid Feature Control String Setting"));
			}
		}
	}
}
