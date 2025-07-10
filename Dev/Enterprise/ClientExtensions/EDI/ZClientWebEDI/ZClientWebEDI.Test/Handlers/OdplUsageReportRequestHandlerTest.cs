using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[TestedType(typeof(OdplUsageReportRequestHandler))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class OdplUsageReportRequestHandlerTest : DataRequestHandlerTestCase<OdplUsageReportRequestHelper>
	{
		public void TestGetBinaryData()
		{
			ZBlob generatedBinaryData = RequestHandler.GetBinaryData();
			Assert("Should not be empty", generatedBinaryData.Length > 0);
		}

		public void TestContentType()
		{
			AssertEquals(DataContentTypes.Pdf, RequestHandler.ContentType);
		}

		protected override DataRequestHandler<OdplUsageReportRequestHelper> GetNewRequestHandler()
		{
			var result = new OdplUsageReportRequestHandler();
			var queryString = new SecureQueryString { [OdplUsageReportRequestHelper.Constants.PeriodStart] = new DateTime(2010, 8, 1).ToString(ZDateTime.ISO8601ShortDateFormat), [OdplUsageReportRequestHelper.Constants.SystemCode] = BillingConstants.BillingSystem.ODM, [OdplUsageReportRequestHelper.Constants.OrganisationPk] = org.PK.ToString(), [OdplUsageReportRequestHelper.Constants.LicenceCompanyPk] = org.LicCompany.PK.ToString(), [OdplUsageReportRequestHelper.Constants.FileName] = "Dummy Report", [OdplUsageReportRequestHelper.Constants.FileType] = BillingConstants.FileExtensions.Pdf };
			result.QueryString.Add(SecureQueryString.QueryStringKey, queryString.ToString());
			return result;
		}

		EDIOrgHeader org;
		protected override void SetUp()
		{
			EServicesBillingTestHelper.CreateTable();
			Client.EDI.Billing.Business.Test.BillingTestHelper.LoadClientSpecificDocuments();
			base.SetUp();
			_ = new DummyBillingSystem();
			string code = "AAA";
			org = Factory.NewWithPrimaryKey<EDIOrgHeader>(new Guid("6d79d8d7-c1fd-45a7-9a85-a3e1a5bcc60b"));
			org.OH_Code = "AAASYD";
			org.FillWithValidTestData();
			org.CreateAndLoadLicenceForOrg();
			org.OH_FullName = code + " Company";
			org.OH_RL_NKClosestPort = "AUSYD";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = code + " Name";
			contact.OC_Email = code + "@hello.com";
			Factory.Save();
		}

		public override void RunBare()
		{
			base.RunBare();
			EServicesBillingTestHelper.DropTable();
		}

		#region DummyBillingSystem
		class DummyBillingSystem : BillingSystem
		{
			public DummyBillingSystem() : base()
			{
			}

			public override string SystemCode
			{
				get
				{
					return "DUM";
				}
			}

			protected override SystemBill CreateSystemBill()
			{
				return new SystemBill(Context.Factory);
			}

			public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
			{
				return new DummySystemRawUsage(context);
			}

			public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
			{
				return new DummyStlRawUsage(context);
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
			{
			}

			protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
			{
				throw new NotImplementedException();
			}

			public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
			{
			}
		}

		class DummySystemRawUsage : SystemRawUsage
		{
			public DummySystemRawUsage(BillingLoadRawUsageContext context) : base(context)
			{
			}

			public override ZString SystemCode
			{
				get
				{
					return "DUM";
				}
			}

			public override SummarySection[] GetRawUsageSummarySections()
			{
				return Array.Empty<SummarySection>();
			}
		}

		class DummyStlRawUsage : StlRawUsage
		{
			public DummyStlRawUsage(BillingLoadRawUsageContext context) : base(context)
			{
			}

			public override SummarySection[] GetRawUsageSummarySections()
			{
				return Array.Empty<SummarySection>();
			}
		}
		#endregion
	}
}
