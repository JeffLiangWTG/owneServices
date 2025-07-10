using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ZEmptyFormForBasherTest))]
	sealed class CountryListControlTest : Enterprise.ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		[RequiresSTA]
		public void TestReadOnly()
		{
			using (ZForm form = new ZForm())
			{
				CountryListControl control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("CountryListGrid.ReadOnly", false, control.CountryListGrid.ReadOnly);

				control.ReadOnly = true;
				AssertEquals("ReadOnly", true, control.ReadOnly);
				AssertEquals("CountryListGrid.ReadOnly", true, control.CountryListGrid.ReadOnly);

				control.ReadOnly = false;
				AssertEquals("ReadOnly", false, control.ReadOnly);
				AssertEquals("CountryListGrid.ReadOnly", false, control.CountryListGrid.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestCountryPKs()
		{
			using (ZForm form = new ZForm())
			{
				CountryListControl control = GetNewControl();
				form.Controls.Add(control);
				form.Show();

				AssertNull("Precondition: collection should be null.", control.collection);

				Guid guid1 = Guid.NewGuid();
				Guid guid2 = Guid.NewGuid();
				Guid guid3 = Guid.NewGuid();
				Guid[] guids = new Guid[] { guid1, guid2, guid3 };
				control.CountryPKs = guids;

				CountryListCollection gridCollection = control.collection;

				AssertEquals("gridCollection.Count", 3, gridCollection.Count);
				AssertEquals("gridCollection[0].CountryPKs", guid1, gridCollection[0].CountryPK);
				AssertEquals("gridCollection[1].CountryPKs", guid2, gridCollection[1].CountryPK);
				AssertEquals("gridCollection[2].CountryPKs", guid3, gridCollection[2].CountryPK);

				Guid[] countryPKs = control.CountryPKs;
				AssertEquals("CountryPKs.Length", 3, countryPKs.Length);
				AssertEquals("CountryPKs[0]", guid1, countryPKs[0]);
				AssertEquals("CountryPKs[1]", guid2, countryPKs[1]);
				AssertEquals("CountryPKs[2]", guid3, countryPKs[2]);
			}
		}

		public void TestFactory()
		{
			using (CountryListControl control = GetNewControl())
			{
				AssertEquals("gridCollection.Factory", Factory, control.factory);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			ZEmptyFormForBasherTest result = new ZEmptyFormForBasherTest();
			result.MinimumSize = new Size(1024, 600);
			result.Size = new Size(1024, 600);
			result.CaptionRenderingEnabled = true;

			CountryListControl control = GetNewControl();
			result.Controls.Add(control);

			return result;
		}

		CountryListControl GetNewControl()
		{
			return new CountryListControl(Factory);
		}

		#endregion
	}
}
