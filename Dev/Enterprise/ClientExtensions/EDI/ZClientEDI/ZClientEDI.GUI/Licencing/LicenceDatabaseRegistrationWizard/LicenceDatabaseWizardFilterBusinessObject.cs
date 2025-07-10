using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business;

namespace ZClientEDI.GUI.Licencing
{
	public class LicenceDatabaseWizardFilterBusinessObject : LicenceDatabaseFilterBusinessObject
	{
		public LicenceDatabaseWizardFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "LicenceDatabase";
		}
	}
}