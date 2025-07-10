using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebDummyModule))]
	public class DummyWebModuleTest : ZFilterGridModuleTest
	{
		#region Overrides

		protected override bool AllowActiveStatusFilterTest()
		{
			return false;
		}

		protected override bool GetShoudTestLoadDBHitsWithDBOnlyQuery(IBusinessObjectCollection collection)
		{
			return false;
		}

		public override void TestTranslatability()
		{
			Assert(true);
		}

		#endregion

		#region Setup

		public override FilterBusinessObjectDefault[] GetArrExpectedFilterBusinessObjectDefault()
		{
			FilterBusinessObjectDefault @default = new FilterBusinessObjectDefault(DummyBizoSchema.Constants.Z0_Description, "Property", (ZString)"WebDummy");
			return new FilterBusinessObjectDefault[] { @default };
		}

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.Dummy; }
		}

		protected override DataGridColumn[] ExpectedDefaultGridColumns
		{
			get
			{
				List<DataGridColumn> result = new List<DataGridColumn>();
				DataGridColumn[] defaults = base.ExpectedDefaultGridColumns;
				result.Add(defaults[0]);
				result.Add(defaults[4]);
				return result.ToArray();
			}
		}

		#endregion
	}
}
