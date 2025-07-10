using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public class DurationRegistryDataType : StringRegistryDataType
	{
		public DurationRegistryDataType() : base() { }

		public DurationRegistryDataType(int minLength, int maxLength) : base(minLength, maxLength) { }

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var match = Regex.Match(proposedValue, @"^\d{2}:[0-5][0-9]$", RegexOptions.Singleline); // regex expression
			if (!match.Success)
			{
				throw new RegistryValidationException(Res.GetString("e11ac36f-1f35-488a-81e7-ea20c24f97f9", @"Incorrect duration format.
Duration should be in HH:mm format.
e.g 08:00, 37:05 and 99:59"));
			}
		}
	}
}
