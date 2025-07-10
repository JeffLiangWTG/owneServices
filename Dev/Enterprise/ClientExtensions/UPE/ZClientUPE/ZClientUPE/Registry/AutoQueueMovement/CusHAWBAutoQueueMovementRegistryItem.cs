using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	public class CusHAWBAutoQueueMovementRegistryItem : StronglyTypedRegistryItem<CusHAWBAutoQueueMovementCollection>
	{
		public CusHAWBAutoQueueMovementRegistryItem(string name, string category, string caption, string hint)
			: this(name, category, caption, hint, new CusHAWBAutoQueueMovementCollection())
		{
		}

		public CusHAWBAutoQueueMovementRegistryItem(string name, string category, string caption, string hint, CusHAWBAutoQueueMovementCollection defaultValue)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new CusHAWBAutoQueueMovementRegistryDataType(), RegistryStorageFlags.System, defaultValue))
		{
		}

		public event EventHandler ValueSet;

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			OnValueSet();
		}

		void OnValueSet()
		{
			if (ValueSet != null)
			{
				ValueSet(this, EventArgs.Empty);
			}
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.Registry.GUI.CusHAWBAutoQueueMovementRegistryItemEditor, ZClientUPE")]
	class CusHAWBAutoQueueMovementRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CusHAWBAutoQueueMovementCollection>
	{
		public CusHAWBAutoQueueMovementRegistryDataType()
		{
		}
	}
}
