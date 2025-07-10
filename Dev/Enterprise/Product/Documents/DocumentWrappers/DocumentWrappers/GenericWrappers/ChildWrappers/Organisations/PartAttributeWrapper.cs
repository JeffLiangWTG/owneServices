using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Name")]
	public class PartAttributeWrapper : GenericWrapper
	{
		public PartAttributeWrapper(OrgMiscServ orgMiscServ, int partAttributeIndex, BusinessObjectFactory factory)
			: base(orgMiscServ, factory)
		{
			if (partAttributeIndex < 0 || partAttributeIndex > PartAttributeManager.MaxAttributes)
			{
				partAttributeIndex = 1;
			}
			this.partAttributeIndex = partAttributeIndex;
		}

		public ZString Name
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null ? PartAttributeManager.PartAttributeName(partAttributeIndex) : ZString.Empty; }
		}

		public ZString Description
		{
			get
			{
				if (OrgMiscServBO != null && PartAttributeManager != null && description.IsEmpty)
				{
					switch (Type)
					{
						case PartAttributeTypeList.Codes.BatchNumber:
							description = PartAttributeTypeList.Descriptions.BatchNumber;
							break;

						case PartAttributeTypeList.Codes.Mandatory:
							description = PartAttributeTypeList.Descriptions.Mandatory;
							break;

						case PartAttributeTypeList.Codes.NonMandatory:
							description = PartAttributeTypeList.Descriptions.NonMandatory;
							break;

						case PartAttributeTypeList.Codes.VIN:
							description = PartAttributeTypeList.Descriptions.VIN;
							break;
					}
				}
				return description;
			}
		}
		ZString description;

		public ZString Type
		{
			get
			{
				if (OrgMiscServBO != null && PartAttributeManager != null)
				{
					return !type.IsEmpty ? type : (type = PartAttributeManager.PartAttributeType(partAttributeIndex));
				}
				else
				{
					return ZString.Empty;
				}
			}
		}
		ZString type;

		public ZBool IsMandatory
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null && PartAttributeManager.IsPartAttributeMandatory(partAttributeIndex); }
		}

		public ZBool IsExpiryDateUsedByOrganisation
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null && PartAttributeManager.IsExpiryDateUsedByOrganisation; }
		}

		public ZBool IsPackingDateUsedByOrganisation
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null && PartAttributeManager.IsPackingDateUsedByOrganisation; }
		}

		public ZBool IsPartAttributeUsedByOrganisation
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null && PartAttributeManager.IsPartAttributeUsedByOrganisation(partAttributeIndex); }
		}

		public ZBool IsSerialNumberUsedByOrganisation
		{
			get { return OrgMiscServBO != null && PartAttributeManager != null && PartAttributeManager.IsSerialNumberUsedByOrganisation; }
		}

		#region Implementation

		OrgHeader OrgHeaderBO
		{
			get { return orgHeaderBO ?? (orgHeaderBO = Factory.Load<OrgHeader>(OrgMiscServBO.OM_OH)); }
		}
		OrgHeader orgHeaderBO;

		OrgMiscServ OrgMiscServBO
		{
			get { return orgMiscServBO ?? (orgMiscServBO = (OrgMiscServ)WrappedBO); }
		}
		OrgMiscServ orgMiscServBO;

		PartAttributeManager PartAttributeManager
		{
			get
			{
				if (OrgHeaderBO != null && partAttributeManager == null)
				{
					partAttributeManager = new PartAttributeManager(OrgHeaderBO);
				}
				return partAttributeManager;
			}
		}
		PartAttributeManager partAttributeManager;

		readonly int partAttributeIndex;

		#endregion
	}
}
