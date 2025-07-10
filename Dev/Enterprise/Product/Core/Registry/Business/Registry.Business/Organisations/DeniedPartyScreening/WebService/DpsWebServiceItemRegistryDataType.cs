using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.DpsWebServiceItemRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DpsWebServiceItemRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DpsWebServiceItemCollection>
	{
		public DpsWebServiceItemRegistryDataType()
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, DpsWebServiceItemCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.Count == 0)
			{
				throw new RegistryValidationException(ResString.GetMultilingualString("CE00DAB7-2423-4374-B4CD-360880FA541F", "Please add at least one Web Service URL when this registry is overridden."));
			}
		}
	}
}
