using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.DataTransfer.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.WCB.GUI.Testing
{
	[TestedType(typeof(WCBDataImporterForm))]
	public class WCBDataImporterFormTest : DataImporterFormTest
	{
		protected override DataImporterForm NewDataImporterForm()
		{
			return new WCBDataImporterForm(new DataImporterBusinessObject(Factory), null);
		}

		protected override Form GetFormToBashCore()
		{
			DataImporterBusinessObject dataImporter = new DataImporterBusinessObject(new BusinessObjectFactory());
			return new WCBDataImporterForm(dataImporter, "Data Importer");
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			if (control.Name == "DaimlerFormatRadioButton" || control.Name == "FreightlinerFormatRadioButton")
			{
				return true;
			}
			else
			{
				return base.ShouldIgnoreMissingBindingMember(control);
			}
		}
	}
}
