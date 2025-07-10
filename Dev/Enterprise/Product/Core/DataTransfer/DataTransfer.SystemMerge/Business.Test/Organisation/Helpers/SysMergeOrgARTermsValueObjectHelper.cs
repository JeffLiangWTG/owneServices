using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.Business.Organisation.Helpers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters.Testing
{
	internal class SysMergeOrgARTermsValueObjectHelper : TestCaseWithFactory
	{
		#region Import

		public void TestImportFromValueObjectCollection()
		{
			Xsd.SysMergeOrgARTermCollection xsdARTermCollection = new Xsd.SysMergeOrgARTermCollection();
			Xsd.SysMergeOrgARTermInvoiceCycleCollection xsdARTermsCycleCollection = new Xsd.SysMergeOrgARTermInvoiceCycleCollection();

			Xsd.SysMergeOrgARTerm arTerm1 = xsdARTermCollection.AddNew();
			arTerm1.ARInvoiceClass = "ALL";
			arTerm1.ARInvoiceTerm = "COD";
			arTerm1.ARInvoiceTermDays = 10;
			arTerm1.AgreedPaymentMethod = "CCD";
			arTerm1.JobType = "SHP";
			arTerm1.Direction = "DI1";
			arTerm1.TransportMode = "TM1";

			Xsd.SysMergeOrgARTerm arTerm2 = xsdARTermCollection.AddNew();
			arTerm2.ARInvoiceClass = "DSB";
			arTerm2.ARInvoiceTerm = "MIC";
			arTerm2.ARInvoiceTermDays = 20;
			arTerm2.AgreedPaymentMethod = "TRF";
			arTerm2.JobType = "QSH";
			arTerm2.Direction = "DI2";
			arTerm2.TransportMode = "TM2";

			Xsd.SysMergeOrgARTermInvoiceCycle arTermCycle = arTerm2.ARTermsCycles.AddNew();
			arTermCycle.ToDay = 10;
			arTermCycle.PaymentDay = 12;

			OrgCompanyData companyData = Factory.New<OrgCompanyData>();
			IValueObjectImportContext context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());
			arTermHelper.ImportFromValueObjectCollection(xsdARTermCollection, companyData, context);

			ZQuery query = new ZQuery(OrgARTermsSchema.PY_OB, companyData.PK);
			OrgARTerms[] arTerms = companyData.Factory.Load<OrgARTerms>(query);

			AssertEquals("2 imported AR Terms should exist", 2, arTerms.Length);

			AssertEquals("ALL", arTerms[0].PY_InvoiceClass);
			AssertEquals("COD", arTerms[0].PY_InvoiceTerm);
			AssertEquals(10, arTerms[0].PY_InvoiceDays.ToZInt());
			AssertEquals("CCD", arTerms[0].PY_AgreedPaymentMethod);
			AssertEquals("SHP", arTerms[0].PY_JobType);
			AssertEquals("DI1", arTerms[0].PY_Direction);
			AssertEquals("TM1", arTerms[0].PY_TransportMode);

			AssertEquals("DSB", arTerms[1].PY_InvoiceClass);
			AssertEquals("MIC", arTerms[1].PY_InvoiceTerm);
			AssertEquals(20, arTerms[1].PY_InvoiceDays.ToZInt());
			AssertEquals("TRF", arTerms[1].PY_AgreedPaymentMethod);
			AssertEquals("QSH", arTerms[1].PY_JobType);
			AssertEquals("DI2", arTerms[1].PY_Direction);
			AssertEquals("TM2", arTerms[1].PY_TransportMode);

			AssertEquals(10, arTerms[1].ARTermsCycles[0].P5_ToDay.ToZInt());
			AssertEquals(12, arTerms[1].ARTermsCycles[0].P5_PaymentDay.ToZInt());
		}

		public void TestImportFromValueObjectCollectionCFX()
		{
			var xsdCFXCollection = new Xsd.AccCFXConfigurationCollection
			{
				new Xsd.AccCFXConfiguration { JobType = "ALL", ServiceDirection = "IMP", TransportMode = "SEA", CFXPercentage = 3.2m, CFXMinimum = 10m },
				new Xsd.AccCFXConfiguration { JobType = "ALL", ServiceDirection = "EXP", TransportMode = "AIR", CFXPercentage = 5.2m, CFXMinimum = 20m },
			};

			var companyData = Factory.New<OrgCompanyData>();
			var context = new ValueObjectImportContext(Factory, new DataTransfer.Xml.XsdVersion1.XmlInterchange(), new SysMergeOrganisationMatching(), new NotificationBuffer());

			arTermHelper.ImportFromValueObjectCollection(xsdCFXCollection, companyData, context);

			AssertEquals("Two records imported", 2, companyData.AccCFXConfigurations.Count);
			var cfx1 = companyData.AccCFXConfigurations[0];

			AssertEquals((ZString)"ALL", cfx1.JCF_JobType);
			AssertEquals((ZString)"IMP", cfx1.JCF_ServiceDirection);
			AssertEquals((ZString)"SEA", cfx1.JCF_TransportMode);
			AssertEquals((ZDecimal)3.2m, cfx1.JCF_CFXPercentage);
			AssertEquals((ZDecimal)10m, cfx1.JCF_CFXMinimum);

			var cfx2 = companyData.AccCFXConfigurations[1];

			AssertEquals((ZString)"ALL", cfx2.JCF_JobType);
			AssertEquals((ZString)"EXP", cfx2.JCF_ServiceDirection);
			AssertEquals((ZString)"AIR", cfx2.JCF_TransportMode);
			AssertEquals((ZDecimal)5.2m, cfx2.JCF_CFXPercentage);
			AssertEquals((ZDecimal)20m, cfx2.JCF_CFXMinimum);
		}
		#endregion

		#region Export

		public void TestExportToValueObjectCollection()
		{
			SysMergeARTermsValueObjectHelper helper = new SysMergeARTermsValueObjectHelper("Error context");
			OrgCompanyData companyData = Factory.New<OrgCompanyData>();

			OrgARTerms arTerm = Factory.New<OrgARTerms>();
			arTerm.PY_OB = companyData.PK;
			arTerm.PY_InvoiceClass = "ALL";
			arTerm.PY_InvoiceTerm = "COD";
			arTerm.PY_InvoiceDays = 10;
			arTerm.PY_AgreedPaymentMethod = "CCD";
			arTerm.PY_JobType = "SHP";
			arTerm.PY_Direction = "DI1";
			arTerm.PY_TransportMode = "TM1";

			OrgARTerms arTerm2 = Factory.New<OrgARTerms>();
			arTerm2.PY_OB = companyData.PK;
			arTerm2.PY_InvoiceClass = "DSB";
			arTerm2.PY_InvoiceTerm = "MIC";
			arTerm2.PY_InvoiceDays = 20;
			arTerm2.PY_AgreedPaymentMethod = "TRF";
			arTerm2.PY_JobType = "QSH";
			arTerm2.PY_Direction = "DI2";
			arTerm2.PY_TransportMode = "TM2";

			OrgARTermsCycle arTermCycle = Factory.New<OrgARTermsCycle>();
			arTermCycle.P5_PY = arTerm2.PK;
			arTermCycle.P5_ToDay = 10;
			arTermCycle.P5_PaymentDay = 12;

			NotificationBuffer notify = new NotificationBuffer();
			Xsd.SysMergeOrgARTermCollection arTermValueCollection = new Xsd.SysMergeOrgARTermCollection();
			helper.ExportToValueObjectCollection(companyData, arTermValueCollection);

			AssertEquals("There should be 2 ARTerms exported", 2, arTermValueCollection.Count);

			Xsd.SysMergeOrgARTerm xsdArTerm1 = arTermValueCollection[0];
			AssertEquals(arTerm.PY_InvoiceClass, xsdArTerm1.ARInvoiceClass);
			AssertEquals(arTerm.PY_InvoiceTerm, xsdArTerm1.ARInvoiceTerm);
			AssertEquals(arTerm.PY_InvoiceDays, xsdArTerm1.ARInvoiceTermDays);
			AssertEquals(arTerm.PY_AgreedPaymentMethod, xsdArTerm1.AgreedPaymentMethod);
			AssertEquals(arTerm.PY_JobType, xsdArTerm1.JobType);
			AssertEquals(arTerm.PY_Direction, xsdArTerm1.Direction);
			AssertEquals(arTerm.PY_TransportMode, xsdArTerm1.TransportMode);

			Xsd.SysMergeOrgARTerm xsdArTerm2 = arTermValueCollection[1];
			AssertEquals(arTerm2.PY_InvoiceClass, xsdArTerm2.ARInvoiceClass);
			AssertEquals(arTerm2.PY_InvoiceTerm, xsdArTerm2.ARInvoiceTerm);
			AssertEquals(arTerm2.PY_InvoiceDays, xsdArTerm2.ARInvoiceTermDays);
			AssertEquals(arTerm2.PY_AgreedPaymentMethod, xsdArTerm2.AgreedPaymentMethod);
			AssertEquals(arTerm2.PY_JobType, xsdArTerm2.JobType);
			AssertEquals(arTerm2.PY_Direction, xsdArTerm2.Direction);
			AssertEquals(arTerm2.PY_TransportMode, xsdArTerm2.TransportMode);

			AssertEquals(1, xsdArTerm2.ARTermsCycles.Count);
			AssertEquals(arTermCycle.P5_PaymentDay, xsdArTerm2.ARTermsCycles[0].PaymentDay);
			AssertEquals(arTermCycle.P5_ToDay, xsdArTerm2.ARTermsCycles[0].ToDay);
		}

		public void TestExportToValueObjectColllectionCFX()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();

			var cfx1 = companyData.AccCFXConfigurations.AddNew();

			cfx1.JCF_ServiceDirection = "EXP";
			cfx1.JCF_TransportMode = "SEA";
			cfx1.JCF_CFXPercentage = 10.5m;
			cfx1.JCF_CFXMinimum = 5.6m;
			cfx1.JCF_JobType = "BRK";

			var cfx2 = companyData.AccCFXConfigurations.AddNew();

			cfx2.JCF_ServiceDirection = "OTH";
			cfx2.JCF_TransportMode = "SEA";
			cfx2.JCF_CFXPercentage = 2.5m;
			cfx2.JCF_CFXMinimum = 2.7m;
			cfx2.JCF_JobType = "SHP";

			Factory.Save();

			var xsdCFXCollection = new Xsd.AccCFXConfigurationCollection();

			arTermHelper.ExportToValueObjectCollection(companyData, xsdCFXCollection);

			AssertEquals("Two records exported", 2, xsdCFXCollection.Count);

			var xsd1 = xsdCFXCollection[0];

			AssertEquals(cfx1.JCF_ServiceDirection, xsd1.ServiceDirection);
			AssertEquals(cfx1.JCF_TransportMode, xsd1.TransportMode);
			AssertEquals(cfx1.JCF_CFXPercentage, xsd1.CFXPercentage);
			AssertEquals(cfx1.JCF_CFXMinimum, xsd1.CFXMinimum);
			AssertEquals(cfx1.JCF_JobType, xsd1.JobType);

			var xsd2 = xsdCFXCollection[1];

			AssertEquals(cfx2.JCF_ServiceDirection, xsd2.ServiceDirection);
			AssertEquals(cfx2.JCF_TransportMode, xsd2.TransportMode);
			AssertEquals(cfx2.JCF_CFXPercentage, xsd2.CFXPercentage);
			AssertEquals(cfx2.JCF_CFXMinimum, xsd2.CFXMinimum);
			AssertEquals(cfx2.JCF_JobType, xsd2.JobType);
		}

		#endregion

		#region Implementation

		readonly SysMergeARTermsValueObjectHelper arTermHelper = new SysMergeARTermsValueObjectHelper("");

		#endregion
	}
}
