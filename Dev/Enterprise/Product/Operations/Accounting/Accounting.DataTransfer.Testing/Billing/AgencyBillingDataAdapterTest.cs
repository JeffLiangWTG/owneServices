using System;
using System.IO;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	sealed class AgencyBillingDataAdapterTest : BillingDataAdapterTest
	{
		public void TestImport_Create()
		{
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "CLIENT";

			Factory.Save();

			AgencyShipment shipment = Factory.New<AgencyShipment>();

			Xsd.Billing billing = new Xsd.BillingWithExchangeRates()
			{
				LocalClient = new Xsd.Organisation() { EDICode = "CLIENT" },
				ChargeLines =
				{
					new Xsd.ChargeLine()
					{
						ChargeCode = "FRT",
						Collect = true,
						OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 500m },
					},
				},
			};

			ValueObjectImportContext context = new ValueObjectImportContext(new BusinessObjectFactory(), new NotificationBuffer());
			IValueObjectDataAdapter adapter = new AgencyBillingDataAdapter();
			try
			{
				adapter.ImportFromValueObject(shipment, billing, context);

				AssertContainsExactElementsInAnyOrder("should have imported the charges correctly",
					new string[]
					{
						"Charge Code: FRT\r\n" +
						"Invoice Type: ",
					},
					ChargesAsStringArray((Job)shipment.Job));
			}
			finally
			{
				(adapter as BillingDataAdapter).LastHeaderCreated?.Dispose();
			}
		}

		public void TestImport_Update()
		{
			AgencyShipment shipment;
			{
				BusinessObjectFactory createFactory = new BusinessObjectFactory();
				OrgHeader client = createFactory.NewWithValidTestData<OrgHeader>();
				client.OH_Code = "CLIENT";

				AgencyShipment createdShipment = createFactory.New<AgencyShipment>();

				Job job = new Job.Loader(createdShipment).TryLoadOrCreateWithoutMutexForTestOnly();
				job.JH_GC = GlbCompany.CurrentCompany.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;

				ZQuery chargeCodeFilter = new ZQuery();
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
				chargeCodeFilter.AddToFilter(AccChargeCodeSchema.AC_GC, job.JH_GC);

				Charge charge = job.Charges.AddNew();
				charge.JR_AC = Factory.LoadTop1<AccChargeCode>(chargeCodeFilter).PK;
				charge.JR_RX_NKCostCurrency = "AUD";
				charge.JR_OSSellAmt = 500;

				createFactory.Save();

				shipment = Factory.Load<AgencyShipment>(createdShipment.PK);
			}

			Xsd.Billing billing = new Xsd.BillingWithExchangeRates()
			{
				LocalClient = new Xsd.Organisation() { EDICode = "CLIENT" },
				ChargeLines =
				{
					new Xsd.ChargeLine()
					{
						ChargeCode = "FRT",
						Collect = true,
						OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 500m },
					},
				},
			};

			ValueObjectImportContext context = new ValueObjectImportContext(new BusinessObjectFactory(), new NotificationBuffer());
			IValueObjectDataAdapter adapter = new AgencyBillingDataAdapter();
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertContainsExactElementsInAnyOrder("should have imported the charges correctly",
				new string[]
				{
					"Charge Code: FRT\r\n" +
					"Invoice Type: ",
				},
				ChargesAsStringArray((Job)shipment.Job));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportForeignPrepaidAndCollect_Import()
		{
			TestImportPrepaidAndCollect_Import(true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportLocalPrepaidAndCollect_Import()
		{
			TestImportPrepaidAndCollect_Import(false);
		}

		void TestImportPrepaidAndCollect_Import(bool isForeignTest)
		{
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "DEBT1";
			debtor.OH_FullName = "debtor1";
			debtor.OH_IsDebtor = true;

			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "CRED1";
			creditor1.OH_FullName = "creditor1";
			creditor1.OH_IsCreditor = true;

			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_Code = "CRED2";
			creditor2.OH_FullName = "creditor2";
			creditor2.OH_IsCreditor = true;

			var group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle =
				isForeignTest ? InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal : InvoicePostingOptionsList.Codes.DisbursementInvoiceOnly;

			ChargeCodeCC1.AC_Code = "CC1";
			ChargeCodeCC1.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			ChargeCodeCC3.AC_Code = "CC3";
			ChargeCodeCC3.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			ChargeCodeCC4.AC_Code = "CC4";
			ChargeCodeCC4.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_OH_DeliveryAgent = creditor2.PK;
			shipment.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			shipment.JS_RL_NKOrigin = "NLAMS";
			shipment.JS_RL_NKDestination = "AUBNE";

			var adapter = (IValueObjectDataAdapter)new AgencyBillingDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			try
			{
				adapter.ImportFromValueObject(shipment, GetBillingFromFile(BillingWithPrepaidAndCollectChargesFileName), context);

				if (isForeignTest)
				{
					AssertContainsExactElementsInAnyOrder("should have imported the charges correctly",
						new string[]
					{
					"Charge Code: CC1\r\n" +
					"Invoice Type: FCO",

					"Charge Code: CC3\r\n" +
					"Invoice Type: FPP",

					"Charge Code: CC4\r\n" +
					"Invoice Type: FCO",
					},
						ChargesAsStringArray((Job)shipment.Job));
				}
				else
				{
					AssertContainsExactElementsInAnyOrder("should have imported the charges correctly",
						new string[]
					{
					"Charge Code: CC1\r\n" +
					"Invoice Type: LCO",

					"Charge Code: CC3\r\n" +
					"Invoice Type: LPP",

					"Charge Code: CC4\r\n" +
					"Invoice Type: LCO",
					},
						ChargesAsStringArray((Job)shipment.Job));
				}

				AssertEquals("Creditor from Defaulting", creditor2, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC1));
				AssertEquals("Creditor from Import File", creditor1, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC3));
				AssertEquals("Creditor from Defaulting", creditor2, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC4));
			}
			finally
			{
				(adapter as BillingDataAdapter).LastHeaderCreated.Dispose();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportPrepaidAndCollect_Export()
		{
			AgencyRegistry.Instance.DefaultCreditorFromPrincipal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			debtor.OH_Code = "DEBT1";
			debtor.OH_FullName = "debtor1";
			debtor.OH_IsDebtor = true;

			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "CRED1";
			creditor1.OH_FullName = "creditor1";
			creditor1.OH_IsCreditor = true;

			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_Code = "CRED2";
			creditor2.OH_FullName = "creditor2";
			creditor2.OH_IsCreditor = true;

			var group = debtor.CompanyData.InvoiceRollupOrGroups.AddNew();
			group.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			group.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			group.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			group.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.ForeignCurrencyInvoiceAndFinal;

			ChargeCodeCC1.AC_Code = "CC1";
			ChargeCodeCC1.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			ChargeCodeCC3.AC_Code = "CC3";
			ChargeCodeCC3.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			ChargeCodeCC4.AC_Code = "CC4";
			ChargeCodeCC4.AC_ChargeType = Constants.ChargeType.Margin;
			ChargeCodeCC4.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			ChargeCodeCC4.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "NLAMS";
			shipment.JS_OH_DeliveryAgent = creditor2.PK;

			var adapter = (IValueObjectDataAdapter)new AgencyBillingDataAdapter();
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			try
			{
				adapter.ImportFromValueObject(shipment, GetBillingFromFile(BillingWithPrepaidAndCollectChargesFileName), context);

				AssertContainsExactElementsInAnyOrder("should have imported the charges correctly",
					new string[]
					{
					"Charge Code: CC1\r\n" +
					"Invoice Type: FCO",

					"Charge Code: CC3\r\n" +
					"Invoice Type: FPP",

					"Charge Code: CC4\r\n" +
					"Invoice Type: FCO",
					},
					ChargesAsStringArray((Job)shipment.Job));

				AssertEquals("Creditor should be Empty", null, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC1));
				AssertEquals("Creditor from Import File", creditor1, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC3));
				AssertEquals("Creditor should be Empty", null, GetCreditorByChargeCode((Job)shipment.Job, ChargeCodeCC4));
			}
			finally
			{
				(adapter as BillingDataAdapter).LastHeaderCreated.Dispose();
			}
		}

		public void TestExport()
		{
			Job job = CreateFullyPopulatedJob();

			ChargeCodeCC3.AC_Code = "CC_P";
			ChargeCodeCC3.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal;

			ChargeCodeCC4.AC_Code = "CC_A";
			ChargeCodeCC4.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Agent;

			string expected_CC1 = "<ChargeCode>CC1</ChargeCode>";
			string expected_CC_P = "<ChargeCode>CC_P</ChargeCode>";
			string expected_CC_A = "<ChargeCode>CC_A</ChargeCode>";

			string xmlString = ExportJob(job);
			AssertContains("Should have CC1", expected_CC1, xmlString);
			AssertContains("Should have CC_P", expected_CC_P, xmlString);
			AssertContains("Should have CC_A", expected_CC_A, xmlString);

			SystemDataRegistry.Instance.IncludeBillingInfoInAgencyXMLFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.IncludeBillingInfoInXMLMethod.Codes.NotInclude);
			xmlString = ExportJob(job);
			AssertNotContains("Should NOT have CC1", expected_CC1, xmlString);
			AssertNotContains("Should NOT have CC_P", expected_CC_P, xmlString);
			AssertNotContains("Should NOT have CC_A", expected_CC_A, xmlString);

			SystemDataRegistry.Instance.IncludeBillingInfoInAgencyXMLFile.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.IncludeBillingInfoInXMLMethod.Codes.PrincipalOnly);
			xmlString = ExportJob(job);
			AssertContains("Should have CC1", expected_CC1, xmlString);
			AssertContains("Should have CC_P", expected_CC_P, xmlString);
			AssertNotContains("Should NOT have CC_A", expected_CC_A, xmlString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithExchangeRates()
		{
			Job job = CreateFullyPopulatedJob();
			JobVoyage voyage = ((AgencyShipment)job.Parent).Sailing.Voyage;

			SetExchangeRate(voyage, "USD", 0.94m);
			SetExchangeRate(voyage, "NZD", 0.74m);

			job.ExchangeRates.RemoveAndDeleteAll();
			SetExchangeRate(job, "USD", 0.95m);
			SetExchangeRate(job, "SGD", 0.85m);

			this.AssertXMLEqualsByDiff("", File.ReadAllText(Path.Combine(BaseBillingTestPath, "PopulatedAgencyBillingWithExchangeRates.xml")), ExportJob(job));
		}

		#region Implementation

		OrgHeader GetCreditorByChargeCode(Job job, AccChargeCode chargeCode)
		{
			foreach (Charge charge in job.Charges)
			{
				if (charge.ChargeCode != null && charge.ChargeCode.PK == chargeCode.PK)
				{
					return charge.CostAccount;
				}
			}

			return null;
		}

		protected override string ExportJob(Job job)
		{
			Xsd.Billing billing = NewBilling();

			AgencyBillingDataAdapter adapter = new AgencyBillingDataAdapter();
			adapter.Export(job, billing, new ValueObjectExportContext(new NotificationBuffer()));

			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.Formatting = Formatting.Indented;

				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(billing.GetType());
				serialiser.Serialize(writer, billing);

				writer.Flush();

				return stream.ToString();
			}
		}

		protected override Job ImportJob(string xml)
		{
			Xsd.Billing billing = NewBilling();

			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(billing.GetType());
				billing = (Xsd.Billing)serialiser.Deserialize(reader);
			}

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			AgencyBillingDataAdapter adapter = new AgencyBillingDataAdapter();
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			}
			finally
			{
				adapter.LastHeaderCreated.Dispose();
			}
			return (Job)shipment.Job;
		}

		protected override Xsd.Billing NewBilling()
		{
			return new Xsd.BillingWithExchangeRates();
		}

		Xsd.Billing GetBillingFromFile(string filename)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.Billing));

			using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
			{
				return (Xsd.Billing)serializer.Deserialize(stream);
			}
		}

		protected override string PopulatedBillingFileName
		{
			get { return BaseBillingTestPath + "PopulatedAgencyBilling.xml"; }
		}

		string BillingWithPrepaidAndCollectChargesFileName
		{
			get { return BaseBillingTestPath + "BillingWithPrepaidAndCollectCharges.xml"; }
		}

		#endregion

	}
}
