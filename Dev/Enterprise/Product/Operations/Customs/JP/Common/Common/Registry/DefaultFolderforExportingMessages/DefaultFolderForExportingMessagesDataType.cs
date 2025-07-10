using System;
using System.IO;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public class DefaultFolderForExportingMessagesDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK,
			Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!proposedValue.IsNullOrEmpty() && !Directory.Exists(proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("9968C62B-354C-419D-9D62-C8316665D7FB", "Please select a valid directory path as default export folder path."));
			}
		}
	}
}
