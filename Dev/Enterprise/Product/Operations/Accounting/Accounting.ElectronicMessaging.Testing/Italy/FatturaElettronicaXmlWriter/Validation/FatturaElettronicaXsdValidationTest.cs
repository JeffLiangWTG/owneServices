using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.Italy;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	sealed class FatturaElettronicaXsdValidationTest : TestCaseWithFactory
	{
		public void TestValidation_XmlHasNoErrors()
		{
			using (var fileStream = ItalyEInvoiceTestHelper.GetEmbeddedResourceAsStream("FatturaElettronicaWithoutErrors.xml"))
			{
				var testLogger = new NotificationBuffer();
				Assert(!testLogger.HasErrors);
				Assert(!testLogger.HasWarnings);
				new FatturaElettronicaXsdValidation(testLogger).ValidateXml(fileStream);
				Assert(!testLogger.HasErrors);
				Assert(!testLogger.HasWarnings);
			}
		}

		public void TestValidation_XmlHasErrors()
		{
			using (var fileStream = ItalyEInvoiceTestHelper.GetEmbeddedResourceAsStream("FatturaElettronicaWithErrors.xml"))
			{
				var testLogger = new NotificationBuffer();
				Assert(!testLogger.HasErrors);
				Assert(!testLogger.HasWarnings);
				new FatturaElettronicaXsdValidation(testLogger).ValidateXml(fileStream);
				var errors = testLogger.Events.Where(x => x.Type == NotificationType.Error);
				AssertEquals(2, errors.Count());
				Assert(errors.Contains("The 'CodiceDestinatario' element is invalid - The value '123' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:CodiceDestinatarioType' - The Pattern constraint failed."));
				Assert(errors.Contains("The 'Denominazione' element is invalid - The value 'ESSE DI TRUCKS SRL TEST ESSE DI TRUCKS SRL TEST ESSE DI TRUCKS SRL TEST ESSE DI TRUCKS SRL TEST' is invalid according to its datatype 'http://ivaservizi.agenziaentrate.gov.it/docs/xsd/fatture/v1.2:String80LatinType' - The Pattern constraint failed."));
				Assert(!testLogger.HasWarnings);
			}
		}

		[TestDate(2020, 10, 14)]
		public void TestXsdSchemaResource()
		{
			using (var fileStream = ItalyEInvoiceTestHelper.GetEmbeddedResourceAsStream("FatturaElettronicaWithoutErrors.xml"))
			{
				var testLogger = new NotificationBuffer();
				var validation = new FatturaElettronicaXsdValidation(testLogger);

				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 15)))
				{
					validation.ValidateXml(fileStream);
					AssertEquals("XSD Schema is the old one (1.2)", "Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.FatturaPA_versione_1.2.xsd", validation.XsdSchemaResource);
				}

				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 14)))
				{
					validation.ValidateXml(fileStream);
					AssertEquals("XSD Schema is the new one (1.2.1)", "Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.FatturaPA_versione_1.2.1.xsd", validation.XsdSchemaResource);
				}

				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDateNewSchema.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new DateTime(2020, 10, 13)))
				{
					validation.ValidateXml(fileStream);
					AssertEquals("XSD Schema is the new one (1.2.1)", "Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.FatturaPA_versione_1.2.1.xsd", validation.XsdSchemaResource);
				}
			}
		}
	}
}
