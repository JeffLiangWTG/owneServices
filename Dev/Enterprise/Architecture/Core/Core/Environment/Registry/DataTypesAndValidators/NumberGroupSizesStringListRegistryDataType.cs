using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumberGroupSizesStringListRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string currentString, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, currentString, companyPK, branchPK, departmentPK);

			var validNumberGroupSizesRegex = new Regex(@"^((([1-9],)*0)|([1-9](,[1-9])*))$");

			if (!validNumberGroupSizesRegex.IsMatch(currentString))
			{
				throw new RegistryValidationException(Res.GetString("CF436C31-42CB-4051-B032-CA24F0D897C3", "Invalid input: Please input valid Number Group Sizes"));
			}
		}
	}
}
