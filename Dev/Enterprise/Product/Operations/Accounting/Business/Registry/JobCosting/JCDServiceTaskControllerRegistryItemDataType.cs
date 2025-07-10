using System;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JCDServiceTaskControllerRegistryItemDataType : CodePairRegistryDataType
	{
		public JCDServiceTaskControllerRegistryItemDataType()
			: base(new CodeDescriptionPairListProvider(() => new JCDActionList()), false, true)
		{
		}

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var currentValue = (string)registryItem.Value;

			if (proposedValue != currentValue)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

				var msg = string.Empty;
				if (!JCDStateTransitionValidation.IsValidTransition(proposedValue, ref msg))
				{
					throw new RegistryValidationException(msg);
				}
			}
		}

		protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore
		{
			get { return true; }
		}
	}
}
