using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataTransfer;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(FindSimilarManufacturerForm))]
	public class FindSimilarManufacturerFormTest : ZFormBasherTest
	{
		public void TestCaptions()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			using (var form = new FindSimilarManufacturerForm(finder))
			{
				AssertEquals("Caption should be", "Select Manufacturer", form.FormHeading);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var factory = new BusinessObjectFactory();
			var finder = new OrganisationFinder(new UnknownOrganisationCodeEventArgs(), factory);
			return new FindSimilarManufacturerForm(finder);
		}

		#endregion
	}
}
