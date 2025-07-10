using System;
using CargoWise.Integration;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EnterpriseModuleList : AutoEnterpriseModuleList
	{
		public EnterpriseModuleList()
		{
			foreach (CodeDescriptionPair pair in LicenceModuleList.Instance.Names)
			{
				AddPairIfNotExist(pair.Code, pair.Description);
			}

			SortByDescriptionThenCode();
		}

		void SortByDescriptionThenCode()
		{
			Elements.Sort(delegate(ICodeDescription x, ICodeDescription y)
			{
				int result = string.Compare(x.Description, y.Description, StringComparison.Ordinal);

				if (result == 0)
				{
					result = string.Compare(x.Code, y.Code, StringComparison.Ordinal);
				}

				return result;
			});
		}
	}
}

