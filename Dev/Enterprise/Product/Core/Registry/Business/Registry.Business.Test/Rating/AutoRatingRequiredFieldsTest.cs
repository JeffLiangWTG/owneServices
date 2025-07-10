using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutoRatingRequiredFields))]
	sealed class AutoRatingRequiredFieldsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRatingHeaderTypes()
		{
			AssertEquals("RatingHeaderTypes.Quotations", "Quotations", AutoRatingRequiredFields.RatingHeaderTypes.Quotations);
			AssertEquals("RatingHeaderTypes.Costs", "Costs", AutoRatingRequiredFields.RatingHeaderTypes.Costs);
			AssertEquals("RatingHeaderTypes.CompanyTariffs", "Company Tariffs", AutoRatingRequiredFields.RatingHeaderTypes.CompanyTariffs);
			AssertEquals("RatingHeaderTypes.ClientRates", "Client Rates", AutoRatingRequiredFields.RatingHeaderTypes.ClientRates);
		}

		public void TestSetRatingHeaderType()
		{
			AssertEquals("Precondition: RatingHeaderType should be empty.", null, BizObj.RatingHeaderType);
			BizObj.SetRatingHeaderType("Test!");
			AssertEquals("RatingHeaderType", "Test!", BizObj.RatingHeaderType);
		}

		#region Implementation

		new AutoRatingRequiredFields BizObj
		{
			get { return (AutoRatingRequiredFields)base.BizObj; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BizObj.RequireServiceLevel = true;
			BizObj.RequireFrequency = false;
			BizObj.RequireTransitTime = true;
			BizObj.RequireCommodityCode = false;
			BizObj.RequireIncoterm = true;
			return BizObj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			BizObj.RequireServiceLevel = false;
			BizObj.RequireFrequency = true;
			BizObj.RequireTransitTime = false;
			BizObj.RequireCommodityCode = true;
			BizObj.RequireIncoterm = false;
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
