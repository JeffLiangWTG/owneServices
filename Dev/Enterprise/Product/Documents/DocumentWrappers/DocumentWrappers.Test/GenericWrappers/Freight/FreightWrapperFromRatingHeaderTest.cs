using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromRatingHeader))]
	sealed class FreightWrapperFromRatingHeaderTest : FreightWrapperTest
	{
		public void TestRating()
		{
			Quote quote = Factory.New<Quote>();
			ClientRate clientRate = Factory.New<ClientRate>();
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			Costing costing = Factory.New<Costing>();

			AssertEquals(quote, new FreightWrapperFromRatingHeader(quote, Factory).Rating.WrappedObject);
			AssertEquals(clientRate, new FreightWrapperFromRatingHeader(clientRate, Factory).Rating.WrappedObject);
			AssertEquals(tariff, new FreightWrapperFromRatingHeader(tariff, Factory).Rating.WrappedObject);
			AssertEquals(costing, new FreightWrapperFromRatingHeader(costing, Factory).Rating.WrappedObject);
		}

		public override void TestJobHeaderLocalClient()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();

			var mainAddress = client.MainAddress;
			mainAddress.CompanyName = "MAIN ADDRESS";

			var arAddress = client.Addresses.AddNew(OrgAddressType.Receivables, true);
			arAddress.CompanyName = "AR ADDRESS";

			var salesAddress = client.Addresses.AddNew(OrgAddressType.Sales, true);
			salesAddress.CompanyName = "SALES ADDRESS";

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = client.PK;

			var wrapper = new FreightWrapperFromRatingHeader(rate, Factory);
			AssertEquals(client, wrapper.JobHeaderLocalClient.WrappedObject);
			AssertEquals("MAIN ADDRESS", wrapper.JobHeaderLocalClient.CompanyName);

			var quotation = Factory.New<Quote>();
			quotation.QuotationClientAddress.OrganisationPK = client.PK;

			wrapper = new FreightWrapperFromRatingHeader(quotation, Factory);

			AssertNotNull("Pre-condition", quotation.QuotationClientAddress);
			AssertNotNull("Pre-condition", wrapper.JobHeaderLocalClient);
			AssertEquals("Should prefer the sales address by default", "SALES ADDRESS", wrapper.JobHeaderLocalClient.CompanyName);

			salesAddress.Delete();
			wrapper = new FreightWrapperFromRatingHeader(quotation, Factory);

			AssertEquals("Should fall back to main if there is no sales address", "MAIN ADDRESS", wrapper.JobHeaderLocalClient.CompanyName);
		}

		public void TestSalesRepForQuote()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "SC1";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "SC2";

			OrgHeader client = Factory.New<OrgHeader>();

			OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;

			Quote header = Factory.New<Quote>();
			header.TH_OH = client.PK;
			header.TH_GS_NKFirstSignatory = staff2.GS_Code;

			FreightWrapperFromRatingHeader wrapper = new FreightWrapperFromRatingHeader(header, Factory);

			AssertEquals(staff2, wrapper.SalesRep.WrappedObject);
		}

		public void TestSalesRepForNonQuote()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "SC1";

			GlbStaff staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "SC2";

			OrgHeader client = Factory.New<OrgHeader>();

			OrgStaffAssignments assignment = client.StaffAssignments.AddNew();
			assignment.O8_GS_NKPersonResponsible = staff1.GS_Code;
			assignment.O8_Role = StaffAssignmentRoles.Codes.SalesRep;
			assignment.O8_Department = OrgStaffAssignmentsLookups.AllServices;

			ClientRate header = Factory.New<ClientRate>();
			header.TH_OH = client.PK;
			header.TH_GS_NKFirstSignatory = staff2.GS_Code;

			FreightWrapperFromRatingHeader wrapper = new FreightWrapperFromRatingHeader(header, Factory);

			AssertEquals(staff1, wrapper.SalesRep.WrappedObject);
		}

		public override void TestTrackingBusinessObjectPK()
		{
			Quote header = Factory.New<Quote>();
			var wrapper = new FreightWrapperFromRatingHeader(header, Factory);
			AssertEquals("TrackingBusinessObjectPK", header.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "JobNumber", "100" },
					{ "JobNumberBarcodeText", "^QUO=100;;|" },
					{ "JobNumberBarcodeTextForFont", "È^QUO=100;;|PÊ" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "È100ÂÊ" },
					{ "JobNumberHeading", "Quote No" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Rating : (No Default Field Value Available on Rating Information)
SalesRep : CargoWise Support";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Quote header = Factory.New<Quote>();
			header.TH_QuoteNumber = "00000100";
			header.TH_GS_NKFirstSignatory = GlbStaff.CurrentUser.GS_Code;

			return new FreightWrapperFromRatingHeader(header, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<CompanyTariff>();
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
