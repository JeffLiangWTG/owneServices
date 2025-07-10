using System;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.OrgCodeAlgorithmConfigRegistryItemEditor, Enterprise.Registry.GUI")]
	public class OrgCodeAlgorithmRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OrgCodeAlgorithm>
	{
		readonly OrgCodeAlgorithmType algorithmType;

		public OrgCodeAlgorithmRegistryDataType(OrgCodeAlgorithmType algorithmType)
		{
			this.algorithmType = algorithmType;
		}

		public OrgCodeAlgorithmType AlgorithmType
		{
			get { return algorithmType; }
		}

		protected override OrgCodeAlgorithm DeserialiseCore(byte[] value)
		{
			OrgCodeAlgorithm result = base.DeserialiseCore(value);
			result.AlgorithmType = AlgorithmType;
			return result;
		}

		protected override void ValidateCore(IRegistryItem registryItem, OrgCodeAlgorithm proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue.AlgorithmType != AlgorithmType)
			{
				throw new RegistryValidationException(Res.GetString("70ae675e-ed46-4332-bdb3-2ee8b2a34894", "The algorithm type specified ('{0}') is incorrect. It should be '{1}'.", proposedValue.AlgorithmType, AlgorithmType));
			}
		}
	}
}
