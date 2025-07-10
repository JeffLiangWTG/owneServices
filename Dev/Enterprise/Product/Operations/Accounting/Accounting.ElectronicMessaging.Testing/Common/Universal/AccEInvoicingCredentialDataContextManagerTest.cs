using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	[TestedType(typeof(AccEInvoicingCredentialDataContextManager))]
	class AccEInvoicingCredentialDataContextManagerTest : DataContextManagerTestCase<AccEInvoicingCredentialDataContextManager, GlbCompanyEInvoicingCertificateCredential>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("GlbCompanyEInvoicingCredential doesn't support IJobNumber", true);
		}

		public void TestAdditionalFieldsToUpdate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Romania))
			{
				var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
				Factory.SaveForTesting();

				AssertEquals("Pre-Condition", PasswordTypesList.Codes.EIM, credential.GP_PasswordType);
				AssertEquals("Pre-Condition", PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
				AssertEquals("Pre-Condition", ZDateTime.Empty, credential.GP_IssueDate);
				AssertEquals("Pre-Condition", ZDateTime.Empty, credential.GP_ExpiryDate);

				var universalEvent = SetupUniversalEvent();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var message = GetQueuedUniversalEventMessage(universalEvent, UniversalXmlInfo.Namespace_2012_11);
				var importResults = manager.Process(message).ImportResults;

				AssertEquals(PasswordStatusList.Codes.Valid, credential.GP_PasswordStatus);
				AssertEquals(new ZDateTime(2024, 02, 13), credential.GP_IssueDate);
				AssertEquals(new ZDateTime(2024, 05, 13), credential.GP_ExpiryDate);
			}
		}

		UniversalEvent SetupUniversalEvent()
		{
			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingCredential, string.Empty);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.ContextCollection = new List<Context>
			{
				new Context() { Type = new ContextType() { Type = "CompanyCode" }, Value = GlbCompany.CurrentCompany.GC_Code },
				new Context() { Type = new ContextType() { Type = "PasswordType" }, Value = PasswordTypesList.Codes.EIM },
			}.WhereNotNull().ToList();
			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>()
			{
				new AdditionalFieldToUpdate() { Type = GlbExternalPasswordSchema.Constants.TableName + "." + GlbExternalPasswordSchema.Constants.GP_PasswordStatus, Value = PasswordStatusList.Codes.Valid },
				new AdditionalFieldToUpdate() { Type = GlbExternalPasswordSchema.Constants.TableName + "." + GlbExternalPasswordSchema.Constants.GP_IssueDate, Value = "2024-02-13T00:00:00" },
				new AdditionalFieldToUpdate() { Type = GlbExternalPasswordSchema.Constants.TableName + "." + GlbExternalPasswordSchema.Constants.GP_ExpiryDate, Value = "2024-05-13T00:00:00" }
			};

			return eventDataObject;
		}
	}
}
