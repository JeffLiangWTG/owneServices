using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class LocalDirectoryRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.StartsWith(@"\\"))
			{
				throw new RegistryValidationException(Res.GetString("50aef922-8171-47b3-9500-d8b95532f739", @"The path must refer to a local directory and cannot start with '\\'."));
			}

			Regex r = new Regex(@"^([A-Z]:\\)(.*[^:\/*<>?|""])?$");
			if (!r.IsMatch(proposedValue) || proposedValue.IsNullOrEmpty())
			{
				throw new RegistryValidationException(Res.GetString("d57a512a-26af-41ed-bdc3-21aadae15545", "The entered path {0} is not a valid directory", proposedValue));
			}
		}
	}
}
