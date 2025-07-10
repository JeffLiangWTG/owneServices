using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.RequireReasonForCLRRegistryItemEditor, Enterprise.Registry.GUI")]
	public class RequireReasonForCLRDataType : NonPersistentBusinessObjectRegistryDataType<RequireReasonForCLRWrapper>
	{
		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, RequireReasonForCLRWrapper proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.RequireReasonForCLR && !proposedValue.ItemCollection.Any())
			{
				throw new RegistryValidationException(ResString.GetMultilingualString("D8EDA233-C667-40C4-8F40-90FF305319D0", "Please add at least 1 item in the list when this registry is overridden to 'Yes'."));
			}
		}
	}
}
