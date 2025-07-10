using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	internal class SRRErrorNotificationOptionsRegistryDataType : CodePairRegistryDataType
	{
		public SRRErrorNotificationOptionsRegistryDataType()
			: base(OLookUpEditType.SRRErrorNotificationOptions, false, true)
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue == Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL)
			{
				CheckErrorNotificationStaffRolesAreSelected();
			}
			else if (proposedValue == Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP)
			{
				CheckErrorNotificationGroupsAreSelected();
			}
		}

		void CheckErrorNotificationStaffRolesAreSelected()
		{
			if (!SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.Value.OfType<CodeDescriptionBool>().Any(b => b.Bool))
			{
				throw new RegistryValidationException(Res.GetString("77EB805A-82AE-4912-AD9B-53CBFFD0AC60", "The Registry item[{0}] must be configured before enabling {1}", ((IMultilingualRegistryItem)SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles).LocationMultilingual, Enterprise.Core.Constants.ErrorNotificationOptions.Description.ROL));
			}
		}

		void CheckErrorNotificationGroupsAreSelected()
		{
			if (SystemDataRegistry.Instance.SRRErrorNotificationGroups.Value == Guid.Empty)
			{
				throw new RegistryValidationException(Res.GetString("77EB805A-82AE-4912-AD9B-53CBFFD0AC60", "The Registry item[{0}] must be configured before enabling {1}", ((IMultilingualRegistryItem)SystemDataRegistry.Instance.SRRErrorNotificationGroups).LocationMultilingual, Enterprise.Core.Constants.ErrorNotificationOptions.Description.GRP));
			}
		}
	}
}
