using System;
using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Registry.Business
{
	public class NettingCycleListRegistryDataType :  CodeDescriptionPairListRegistryDataType
	{
		public NettingCycleListRegistryDataType()	: base(9)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "This is JAS specific")]
		protected override void ValidateCore(IRegistryItem registryItem, ReadOnlyCodeDescriptionPairList proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			StringBuilder errorMessage = new StringBuilder();
			foreach (CodeDescriptionPair pair in proposedValue)
			{
				string nettingCycle = pair.Code.Trim();
				ZDateTime dateTimeValue;
				if (!ZDateTime.TryParseExact(nettingCycle, out dateTimeValue, "dd-MMM-yy"))		// This is JAS specific
				{
					errorMessage.AppendFormat("{0}\r\n", nettingCycle);
				}
			}

			if (errorMessage.Length > 0)
			{
				errorMessage.Insert(0, "The following data is not a valid Netting Cycle Date (date format should be dd-MMM-yy, i.e. 05-JAN-05):\r\n");
				throw new RegistryValidationException(errorMessage.ToString().Trim());
			}
		}
	}
}
