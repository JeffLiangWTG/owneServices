using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Name")]
	public class DGContactWrapper : GenericWrapper
	{
		#region Constructors

		public DGContactWrapper(OrgMiscServ orgMiscServ, BusinessObjectFactory factory) : base(null, factory)
		{
			if (orgMiscServ != null)
			{
				OrgContact contact = factory.Load<OrgContact>(orgMiscServ.OM_OC_EXDefaultDGContact);
				if (contact != null)
				{
					fName = contact.OC_ContactName;
				}
				fPhone = orgMiscServ.DGPhoneNumber;
				fPhoneFormatted = orgMiscServ.DGPhoneNumber_Formatted;
				fPhoneType = orgMiscServ.OM_EXDefaultDGContactPhoneUsed;
			}
		}

		#endregion

		#region Properties

		public ZString Name => fName;

		public ZString Phone => fPhone;

		public ZString PhoneFormatted => fPhoneFormatted;

		public ZString PhoneType => fPhoneType;

		#endregion

		#region Implementation

		readonly ZString fName;
		readonly ZString fPhone;
		readonly ZString fPhoneFormatted;
		readonly ZString fPhoneType;

		#endregion
	}
}
