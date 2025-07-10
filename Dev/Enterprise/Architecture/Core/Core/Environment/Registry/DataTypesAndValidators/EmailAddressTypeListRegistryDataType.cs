using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailAddressTypeListRegistryDataType : CodeDescriptionPairListRegistryDataType
	{
		public EmailAddressTypeListRegistryDataType(int codeMaxLength) : this(codeMaxLength, true)
		{
		}

		public EmailAddressTypeListRegistryDataType(int codeMaxLength, bool isEmptyListAllowed) : base(codeMaxLength, isEmptyListAllowed)
		{
			AllowDuplicateCodes = false;
			AllowDuplicateDescriptions = false;
			AllowEmptyCodes = false;
			AllowEmptyDescriptions = false;
		}

		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue != null)
			{
				for (var i = 0; i < proposedValue.Count; i++)
				{
					if (string.Equals(proposedValue[i].Description, Constants.EmailFromAddressTypes.Descriptions.Main.GetUnresolvedString(), StringComparison.OrdinalIgnoreCase))
					{
						throw new RegistryValidationException(Res.GetString("68D37C4A-3BA5-4CBF-9D5C-A219FBFB48D9", "The description '{0}' cannot be used as it is reserved for the default staff email address.", proposedValue[i].Description));
					}

					if (string.Equals(proposedValue[i].Code, Constants.EmailFromAddressTypes.Codes.Main, StringComparison.OrdinalIgnoreCase))
					{
						throw new RegistryValidationException(Res.GetString("12F46563-F77F-4BBE-9FB8-A659680DBCF9", "The code '{0}' cannot be used as it is reserved for the default staff email address.", proposedValue[i].Code));
					}
				}
			}
		}
	}
}
