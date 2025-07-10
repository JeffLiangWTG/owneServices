using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class PRAMessagingRegistry
	{
		public PRAMessagingRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		#region Common

		T GetRegistryItemValueForBranch<T>(IRegistryItem registryItem, Guid branchPK)
		{
			return (T)registryItem.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, branchPK, EnvProxy.Instance.CurrentDepartment.PK);
		}

#if DEBUG

		void SetRegistryItemValueForBranch(IRegistryItem registryItem, Guid branchPK, object value)
		{
			registryItem.SetValue(Guid.Empty, branchPK, Guid.Empty, value);
		}

#endif

		#endregion

		#region AcknowledgementEmailGroup

		public Guid AcknowledgementEmailGroup
		{
			get { return (Guid)RawRegistry.AcknowledgementEmailGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public Guid GetAcknowledgementEmailGroupForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<Guid>(RawRegistry.AcknowledgementEmailGroup, branchPK);
		}

#if DEBUG

		public void SetAcknowledgementEmailGroupForBranch(Guid branchPK, object value)
		{
			SetRegistryItemValueForBranch(RawRegistry.AcknowledgementEmailGroup, branchPK, value);
		}

#endif

		#endregion

		#region AcknowledgementEmailMode

		public string AcknowledgementEmailMode
		{
			get { return (string)RawRegistry.AcknowledgementEmailMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string GetAcknowledgementEmailModeForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<string>(RawRegistry.AcknowledgementEmailMode, branchPK);
		}

#if DEBUG

		public void SetAcknowledgementEmailModeForBranch(Guid branchPK, object value)
		{
			SetRegistryItemValueForBranch(RawRegistry.AcknowledgementEmailMode, branchPK, value);
		}

#endif

		#endregion

		#region ImpedimentEmailGroup

		public Guid ImpedimentEmailGroup
		{
			get { return (Guid)RawRegistry.ImpedimentEmailGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public Guid GetImpedimentEmailGroupForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<Guid>(RawRegistry.ImpedimentEmailGroup, branchPK);
		}

		#endregion

		#region ImpedimentEmailMode

		public string ImpedimentEmailMode
		{
			get { return (string)RawRegistry.ImpedimentEmailMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string GetImpedimentEmailModeForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<string>(RawRegistry.ImpedimentEmailMode, branchPK);
		}

		#endregion

		#region ErrorEmailGroup

		public Guid ErrorEmailGroup
		{
			get { return (Guid)RawRegistry.ErrorEmailGroup.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public Guid GetErrorEmailGroupForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<Guid>(RawRegistry.ErrorEmailGroup, branchPK);
		}

		#endregion

		#region ErrorEmailMode

		public string ErrorEmailMode
		{
			get { return (string)RawRegistry.ErrorEmailMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public string GetErrorEmailModeForBranch(Guid branchPK)
		{
			return GetRegistryItemValueForBranch<string>(RawRegistry.ErrorEmailMode, branchPK);
		}

		#endregion

		public bool PRATestMode
		{
			get { return (bool)RawRegistry.PRATestMode.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public Guid SeaFreightDangerousGoodsContact
		{
			get { return (Guid)RawRegistry.PRAMessagingSeaFreightDangerousGoodsContact.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		readonly RawDataRegistry RawRegistry;
	}
}
