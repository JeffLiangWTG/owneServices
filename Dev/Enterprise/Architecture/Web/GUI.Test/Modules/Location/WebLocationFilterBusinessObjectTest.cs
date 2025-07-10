using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	[TestedType(typeof(WebLocationFilterBusinessObject))]
	sealed class WebLocationFilterBusinessObjectTest : FilterBusinessObjectTestCase
	{
		#region Filter

		public void TestFilter()
		{
			string expectedFilterFormat = "{0} like 'A%' and {1} like '%Desc%'";
			string expectedFilter = "";
			FilterObject.Code = "A";
			FilterObject.Description = "Desc";

			FilterObject.IsPort = ZBool.True;
			expectedFilter = String.Format(expectedFilterFormat, RefUNLOCOSchema.RL_Code.Name, RefUNLOCOSchema.RL_PortName.Name);
			AssertEquals("Filter for Location", expectedFilter, FilterObject.Filter.LiteralTextADO);

			FilterObject.IsRegion = ZBool.True;
			expectedFilter = String.Format(expectedFilterFormat, RefZoneHeaderSchema.FZ_Code.Name, RefZoneHeaderSchema.FZ_Description.Name);
			AssertEquals("Filter for Region", expectedFilter, FilterObject.Filter.LiteralTextADO);

			FilterObject.IsCountry = ZBool.True;
			expectedFilter = String.Format(expectedFilterFormat, RefCountrySchema.RN_Code.Name, RefCountrySchema.RN_Desc.Name);
			AssertEquals("Filter for Country", expectedFilter, FilterObject.Filter.LiteralTextADO);
		}

		#endregion Filter 

		#region LocationType

		public void TestLocationType()
		{
			AssertEquals("IsPort should be true by default", ZBool.True, FilterObject.IsPort);
			AssertEquals("IsCountry should be false when IsPort is true", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsRegion should be false when IsPort is true", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsCountry = ZBool.True;

			AssertEquals("IsCountry should now be true", ZBool.True, FilterObject.IsCountry);
			AssertEquals("IsPort should be set to false now that IsCountry is true", ZBool.False, FilterObject.IsPort);
			AssertEquals("IsRegion should also be false after IsCountry is set to true", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsRegion = ZBool.True;

			AssertEquals("IsRegion should now be true", ZBool.True, FilterObject.IsRegion);
			AssertEquals("IsCountry should be set to false now that IsRegion is true", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsPort should be also be false after IsRegion is set to true ", ZBool.False, FilterObject.IsPort);
		}

		public void TestLocationTypeSetToFalse()
		{
			AssertEquals("IsPort should be true by default", ZBool.True, FilterObject.IsPort);
			AssertEquals("IsCountry should be false when IsPort is true", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsRegion should be false when IsPort is true", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsPort = ZBool.False;

			AssertEquals("IsPort should now be false", ZBool.False, FilterObject.IsPort);
			AssertEquals("IsCountry should be set to true when IsPort is set to false", ZBool.True, FilterObject.IsCountry);
			AssertEquals("IsRegion should remain false when IsPort is set to false", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsCountry = ZBool.True;

			AssertEquals("IsCountry should now be true", ZBool.True, FilterObject.IsCountry);
			AssertEquals("IsPort should be set to false now that IsCountry is true", ZBool.False, FilterObject.IsPort);
			AssertEquals("IsRegion should also be false after IsCountry is set to true", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsCountry = ZBool.False;

			AssertEquals("IsPort should be set to true when IsCountry is set to false", ZBool.True, FilterObject.IsPort);
			AssertEquals("IsCountry should now be false", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsRegion should remain false when IsCountry is set to false", ZBool.False, FilterObject.IsRegion);

			FilterObject.IsRegion = ZBool.True;

			AssertEquals("IsRegion should now be true", ZBool.True, FilterObject.IsRegion);
			AssertEquals("IsCountry should be set to false now that IsRegion is true", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsPort should be also be false after IsRegion is set to true ", ZBool.False, FilterObject.IsPort);

			FilterObject.IsRegion = ZBool.False;

			AssertEquals("IsPort should be set to true when IsRegion is set to false", ZBool.True, FilterObject.IsPort);
			AssertEquals("IsCountry should remain false when IsRegino is set to false", ZBool.False, FilterObject.IsCountry);
			AssertEquals("IsRegion should now be false", ZBool.False, FilterObject.IsRegion);
		}

		public void TestCodeAndDescriptionTooLong()
		{
			var expectedLocationCode = new string('A', RefUNLOCOSchema.RL_Code.MaxLength);
			var expectedLocationDescription = new string('B', RefUNLOCOSchema.RL_PortName.MaxLength);
			FilterObject.Code = expectedLocationCode + "A";
			FilterObject.Description = expectedLocationDescription + "B";
			FilterObject.IsPort = ZBool.True;
			var expectedLocationFilter = $"{RefUNLOCOSchema.RL_Code.Name} = '{expectedLocationCode}' and {RefUNLOCOSchema.RL_PortName.Name} = '{expectedLocationDescription}'";

			AssertEquals("Filter for Location", expectedLocationFilter, FilterObject.Filter.LiteralTextADO);

			var expectedRegionCode = new string('A', RefZoneHeaderSchema.FZ_Code.MaxLength);
			var expectedRegionDescription = new string('B', RefZoneHeaderSchema.FZ_Description.MaxLength);
			FilterObject.Code = expectedRegionCode + "A";
			FilterObject.Description = expectedRegionDescription + "B";
			FilterObject.IsRegion = ZBool.True;
			var expectedRegionFilter = $"{RefZoneHeaderSchema.FZ_Code.Name} = '{expectedRegionCode}' and {RefZoneHeaderSchema.FZ_Description.Name} = '{expectedRegionDescription}'";

			AssertEquals("Filter for Region", expectedRegionFilter, FilterObject.Filter.LiteralTextADO);

			var expectedCountryCode = new string('A', RefCountrySchema.RN_Code.MaxLength);
			var expectedCountryDescription = new string('B', RefCountrySchema.RN_Desc.MaxLength);
			FilterObject.Code = expectedRegionCode + "A";
			FilterObject.Description = expectedCountryDescription + "B";
			FilterObject.IsCountry = ZBool.True;
			var expectedCountryFilter = $"{RefCountrySchema.RN_Code.Name} = '{expectedCountryCode}' and {RefCountrySchema.RN_Desc.Name} = '{expectedCountryDescription}'";

			AssertEquals("Filter for Country", expectedCountryFilter, FilterObject.Filter.LiteralTextADO);
		}

		#endregion LocationType

		#region FilterObject

		WebLocationFilterBusinessObject FilterObject
		{
			get
			{
				if (fFilterObject == null)
				{
					fFilterObject = (WebLocationFilterBusinessObject)GetNewBusinessObject();
				}
				return fFilterObject;
			}
		}
		WebLocationFilterBusinessObject fFilterObject;

		#endregion FilterObject
	}
}
