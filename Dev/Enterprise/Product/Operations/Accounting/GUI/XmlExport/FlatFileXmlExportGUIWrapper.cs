using CargoWise.EntityFramework;

namespace Enterprise.Accounting.GUI.XmlExport
{
	public abstract class FlatFileXmlExportGUIWrapper : XmlExportGUIWrapper
	{
		public FlatFileXmlExportGUIWrapper(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
