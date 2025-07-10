using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	public static class ClassificationTariffUserControlTestHelper
	{
		public static ResourceStringData AssertTariffColumnInfo_GetTariffFromTrfCA(Control control, ZString gridName, ZString columnName, Type type)
		{
			var grid = (ZGrid)control.Controls.Find(gridName, true)[0];
			var tariffColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == columnName);
			Assertion.AssertType(type, tariffColumnInfo);
			return tariffColumnInfo.CaptionResourceString;
		}

		public static void AssertTariffFindBox_GetTariffFromTrfCA(Control control, ZString tariffFindBoxName, Type type)
		{
			var tariffFindBox = control.Controls.Find(tariffFindBoxName, true)[0];
			Assertion.AssertType(type, tariffFindBox);
		}

		public static void AssertTariffColumnInfo_GetTariffFromSRDb(Control control, ZString gridName, ZString columnName, ResourceStringData captionResourceString)
		{
			var grid = (ZGrid)control.Controls.Find(gridName, true)[0];
			var tariffColumnInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == columnName);
			Assertion.AssertType(typeof(Universal.GUI.TariffColumnStyleInfo), tariffColumnInfo);
			var tariffInfo = (Universal.GUI.TariffColumnStyleInfo)tariffColumnInfo;
			Assertion.AssertEquals("CaptionResourceString should be same", captionResourceString, tariffInfo.CaptionResourceString);
			Assertion.AssertEquals("TariffType should be HSN", "HSN", tariffInfo.TariffType);
			Assertion.AssertEquals("GetCountryCode should be CA", Core.Constants.CountryCodes.Canada, tariffInfo.GetCountryCode());
			Assertion.AssertEquals("GetDataGrouping should be CA", Core.Constants.CountryCodes.Canada, tariffInfo.GetDataGrouping());
			Assertion.AssertNotNull("GetEffectiveDate should not null", tariffInfo.GetEffectiveDate);
		}

		public static void AssertTariffFindBox_GetTariffFromSRDb(Control control, ZString tariffFindBoxName, ZString tariffFindBoxFromSRDbName)
		{
			var tariffFindBox = (ZCodeFindBox)control.Controls.Find(tariffFindBoxName, true)[0];
			Assertion.Assert("The TariffFindBox that get data from TrfCA should not be visible.", !tariffFindBox.Visible);
			var tariffFindFromSRDbBox = (Universal.GUI.TariffFindBox)control.Controls.Find(tariffFindBoxFromSRDbName, true)[0];
			Assertion.Assert("The TariffFindBox that get data from SRDb should be visible.", tariffFindFromSRDbBox.Visible);
			Assertion.AssertEquals("Anchor should be same", tariffFindBox.Anchor, tariffFindFromSRDbBox.Anchor);
			Assertion.AssertEquals("CaptionResourceString should be same", tariffFindBox.CaptionResourceString, tariffFindFromSRDbBox.CaptionResourceString);
			Assertion.AssertEquals("Location should be same", tariffFindBox.Location, tariffFindFromSRDbBox.Location);
			Assertion.AssertEquals("Size should be same", tariffFindBox.Size, tariffFindFromSRDbBox.Size);
			Assertion.AssertEquals("TabIndex should be same", tariffFindBox.TabIndex, tariffFindFromSRDbBox.TabIndex);
			Assertion.AssertEquals("TariffType should be HSN", "HSN", tariffFindFromSRDbBox.TariffType);
			Assertion.AssertEquals("GetCountryCode should be CA", Core.Constants.CountryCodes.Canada, tariffFindFromSRDbBox.GetCountryCode());
			Assertion.AssertEquals("GetDataGrouping should be CA", Core.Constants.CountryCodes.Canada, tariffFindFromSRDbBox.GetDataGrouping());
			Assertion.AssertNotNull("GetEffectiveDate should not null", tariffFindFromSRDbBox.GetEffectiveDate);
		}
	}
}
