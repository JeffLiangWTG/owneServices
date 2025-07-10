using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgAddress : OrgAddress
	{
		public EDIOrgAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgAddressValidation GetNewValidation()
		{
			return new EDIOrgAddressValidation(this);
		}

		public override ZGuid OA_OH
		{
			get { return base.OA_OH; }
			set
			{
				base.OA_OH = value;
				if (Header != null)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#region ReadOnly

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsLinkedToClientBranch;
			set => base.ReadOnly = value;
		}

		public bool IsLinkedToClientBranch
		{
			get
			{
				return Factory.GetCachedValue("EDIOrgAddress.IsLinkedToClientBranch" + PK, () =>
					{
						return Factory.ExistsInDatabase(ClientBranchSchema.Constants.TableName, new ZQuery(ClientBranchSchema.LCB_OA, PK));
					});
			}
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && !ReadOnly;
		}

		#endregion
	}
}