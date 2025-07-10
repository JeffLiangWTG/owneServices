using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.XmlExport.Testing
{
	[TestedType(typeof(FlatFileXmlExportGUIWrapperTestClass))]
	public class FlatFileXmlExportGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FlatFileXmlExportGUIWrapperTestClass(Factory);
		}

		class FlatFileXmlExportGUIWrapperTestClass : FlatFileXmlExportGUIWrapper
		{
			public FlatFileXmlExportGUIWrapperTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}
		}
	}
}
