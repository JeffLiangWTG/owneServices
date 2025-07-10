using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class NumericOnlyStringRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			Match match = new Regex(@"^[0-9]+$").Match(proposedValue);

			if (!match.Success)
			{
				throw new RegistryValidationException(Res.GetString("516d95cb-c14c-402c-8b59-65cdaf91e72e", "Please enter a numeric value (value that consists of numbers only)."));
			}
		}
	}
}
