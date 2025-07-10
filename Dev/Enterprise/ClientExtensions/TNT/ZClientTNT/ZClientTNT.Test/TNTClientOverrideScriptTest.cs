using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.TNT;
using Enterprise.ClientSharedComponents.Registry;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Testing
{
	public class TNTClientOverrideScriptTest : TestCaseWithFactory
	{
		public void TestLogSubscribers()
		{
			ClientOverride clientOverride = ClientOverride.Instance;
			AssertEquals("registry for the events is not set", 0, TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.Value.Count);
			int noOfSubScriber = (new List<ILogSubscriber>(clientOverride.LogSubscribers)).Count;
			AssertEquals("no of logsubscriber", 0, noOfSubScriber);
			EventRegistryBusinessObjectCollection collection = new EventRegistryBusinessObjectCollection();
			EventRegistryBusinessObject bizObj = collection.AddNew();
			bizObj.Code = Events.CustomsEntryStatus.Code;
			bizObj.Reference = "ABC";
			bool result = false;
			TNTDataRegistry.Instance.DeclarationEventsForCustomsResponseItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			noOfSubScriber = (new List<ILogSubscriber>(clientOverride.LogSubscribers)).Count;
			AssertEquals("no of logsubscriber", 1, noOfSubScriber);
			foreach (LogSubscriber subscriber in clientOverride.LogSubscribers)
			{
				if (subscriber.GetType() == typeof(DeclarationCustomResponseSenderSubscriber))
				{
					result = true;
					break;
				}
			}

			AssertEquals(true, result);
		}

		public void TestDeletingUsedDeclarationsBasedJobNumber()
		{
			SetupDeclarations();
			RunClientTNT_DeleteUnusedDeclarations(Declaration1.JE_DeclarationReference, null, null, DateTime.MinValue, DateTime.MinValue);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			RunClientTNT_DeleteUnusedDeclarations(Declaration3.JE_DeclarationReference, null, null, DateTime.MinValue, DateTime.MinValue);
			newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
		}

		public void TestDeletingUsedDeclarationsBasedConsignorCode()
		{
			SetupDeclarations();
			RunClientTNT_DeleteUnusedDeclarations(null, Supplier1.OH_Code, null, DateTime.MinValue, DateTime.MinValue);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			RunClientTNT_DeleteUnusedDeclarations(null, Supplier2.OH_Code, null, DateTime.MinValue, DateTime.MinValue);
			newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
		}

		public void TestDeletingUsedDeclarationsBasedConsignorName()
		{
			SetupDeclarations();
			RunClientTNT_DeleteUnusedDeclarations(null, null, Supplier1.OH_FullName, DateTime.MinValue, DateTime.MinValue);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			RunClientTNT_DeleteUnusedDeclarations(null, null, Supplier2.OH_FullName, DateTime.MinValue, DateTime.MinValue);
			newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
		}

		public void TestDeletingUsedDeclarationsBasedOnDateRange()
		{
			SetupDeclarations();
			RunClientTNT_DeleteUnusedDeclarations(null, null, null, ZDateTime.Now.AddDays(-20).ToDateTime(), ZDateTime.Now.ToDateTime());
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			RunClientTNT_DeleteUnusedDeclarations(null, null, null, ZDateTime.Now.AddDays(-50).ToDateTime(), ZDateTime.Now.ToDateTime());
			newFactory = new BusinessObjectFactory();
			AssertDeclarationHasBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclarationHasBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
		}

		public void TestDeletingUsedDeclarations_NotDeleteAnyIfInvalidParameter()
		{
			SetupDeclarations();
			// No Parameter
			RunClientTNT_DeleteUnusedDeclarations(null, null, null, DateTime.MinValue, DateTime.MinValue);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			// Only DateFrom parameter specified
			RunClientTNT_DeleteUnusedDeclarations(null, null, null, ZDateTime.Now.AddDays(-20).ToDateTime(), DateTime.MinValue);
			newFactory = new BusinessObjectFactory();
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
			// Only DateTo parameter specified
			RunClientTNT_DeleteUnusedDeclarations(null, null, null, DateTime.MinValue, ZDateTime.Now.ToDateTime());
			newFactory = new BusinessObjectFactory();
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration1.PK, Declaration1Invoice.PK, Declaration1Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration2.PK, Declaration2Invoice.PK, Declaration2Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration3.PK, Declaration3Invoice.PK, Declaration3Cushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, Declaration4.PK, Declaration4Invoice.PK, Declaration4Cushawb.PK);
			AssertCertainDataShouldNeverBeDeleted(newFactory);
		}

		#region Implementation
		void AssertCertainDataShouldNeverBeDeleted(BusinessObjectFactory newFactory)
		{
			AssertDeclartionHasNotBeenDeleted(newFactory, DeclarationWithMoreThan1Event.PK, DeclarationWithMoreThan1EventInvoice.PK, DeclarationWithMoreThan1EventCushawb.PK);
			AssertDeclartionHasNotBeenDeleted(newFactory, DeclarationWithJobHeader.PK, DeclarationWithJobHeaderInvoice.PK, DeclarationWithJobHeaderCushawb.PK);
			AssertOrganisationHasNotBeenDeleted(newFactory, Supplier1.PK);
			AssertOrganisationHasNotBeenDeleted(newFactory, Supplier2.PK);
		}

		void AssertOrganisationHasNotBeenDeleted(BusinessObjectFactory newFactory, ZGuid orgPK)
		{
			AssertNotNull("Organisation should not be deleted", newFactory.Load<OrgHeader>(orgPK));
		}

		void AssertCushawbHasNotBeenDeleted(BusinessObjectFactory newFactory, ZGuid declarationPK, ZGuid cushawbPK)
		{
			CusHAWB cushawb = newFactory.Load<CusHAWB>(cushawbPK);
			AssertNotNull("Cushawb linked to Declaration should not be deleted", cushawb);
			AssertEquals("Cushawb linked to Declaration", declarationPK, cushawb.CS_JE_CustomsFormalEntry);
		}

		void AssertDeclartionHasNotBeenDeleted(BusinessObjectFactory newFactory, ZGuid declarationPK, ZGuid declarationInvoicePK, ZGuid cushawbPK)
		{
			AssertNotNull("Declaration should not be deleted", newFactory.Load<Customs.Business.BaseJobDeclaration>(declarationPK));
			AssertNotNull("Declaration's Invoice should not be deleted", newFactory.Load<Customs.Business.BaseJobComInvoiceHeader>(declarationInvoicePK));
			AssertCushawbHasNotBeenDeleted(newFactory, declarationPK, cushawbPK);
		}

		void AssertDeclarationHasBeenDeleted(BusinessObjectFactory newFactory, ZGuid declarationPK, ZGuid declarationInvoicePK, ZGuid cushawbPK)
		{
			AssertNull("Declaration should have been deleted", newFactory.Load<Customs.Business.BaseJobDeclaration>(declarationPK));
			AssertNull("Declaration's Invoice should have been deleted", newFactory.Load<Customs.Business.BaseJobComInvoiceHeader>(declarationInvoicePK));
			AssertCushawbHasNotBeenDeleted(newFactory, ZGuid.Empty, cushawbPK);
		}

		public void SetupDeclarations()
		{
			AssertNotNull("PreCondition: Declaration1", Declaration1);
			AssertNotNull("PreCondition: Declaration1Invoice", Declaration1Invoice);
			AssertNotNull("PreCondition: Declaration1Cushawb", Declaration1Cushawb);
			AssertNotNull("PreCondition: Declaration2", Declaration2);
			AssertNotNull("PreCondition: Declaration2Invoice", Declaration2Invoice);
			AssertNotNull("PreCondition: Declaration2Cushawb", Declaration2Cushawb);
			AssertNotNull("PreCondition: Declaration3", Declaration3);
			AssertNotNull("PreCondition: Declaration3Invoice", Declaration3Invoice);
			AssertNotNull("PreCondition: Declaration3Cushawb", Declaration3Cushawb);
			AssertNotNull("PreCondition: Declaration4", Declaration4);
			AssertNotNull("PreCondition: Declaration4Invoice", Declaration4Invoice);
			AssertNotNull("PreCondition: Declaration4Cushawb", Declaration4Cushawb);
			AssertNotNull("PreCondition: DeclarationWithMoreThan1Event", DeclarationWithMoreThan1Event);
			AssertNotNull("PreCondition: DeclarationWithMoreThan1EventInvoice", DeclarationWithMoreThan1EventInvoice);
			AssertNotNull("PreCondition: DeclarationWithMoreThan1EventCushawb", DeclarationWithMoreThan1EventCushawb);
			AssertNotNull("PreCondition: DeclarationWithJobHeader", DeclarationWithJobHeader);
			AssertNotNull("PreCondition: DeclarationWithJobHeaderInvoice", DeclarationWithJobHeaderInvoice);
			AssertNotNull("PreCondition: DeclarationWithJobHeaderCushawb", DeclarationWithJobHeaderCushawb);
			AssertEquals("PreCondition: DeclarationWithJobHeaderJobHeader should be linked to DeclarationWithJobHeader", DeclarationWithJobHeader.PK, DeclarationWithJobHeaderJobHeader.JH_ParentID);
			AssertNotNull("PreCondition: Supplier1", Supplier1);
			AssertNotNull("PreCondition: Supplier2", Supplier2);
			Factory.Save();
			AssertEquals("PreCondition: DeclarationWithMoreThan1Event should have 3 Logs", 3, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, DeclarationWithMoreThan1Event.PK)).Length);
		}

		#region Declaration1
		Customs.Business.BaseJobDeclaration Declaration1
		{
			get
			{
				if (fDeclaration1 == null)
				{
					fDeclaration1 = CreateDeclaration("DECLARATION1", Supplier1.PK, -10);
					AddLinkedData(fDeclaration1, ref Declaration1Invoice, ref Declaration1Cushawb);
				}

				return fDeclaration1;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclaration1;
		Customs.Business.BaseJobComInvoiceHeader Declaration1Invoice;
		CusHAWB Declaration1Cushawb;
		#endregion
		#region Declaration2
		Customs.Business.BaseJobDeclaration Declaration2
		{
			get
			{
				if (fDeclaration2 == null)
				{
					fDeclaration2 = CreateDeclaration("DECLARATION2", Supplier2.PK, -40);
					AddLinkedData(fDeclaration2, ref Declaration2Invoice, ref Declaration2Cushawb);
				}

				return fDeclaration2;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclaration2;
		Customs.Business.BaseJobComInvoiceHeader Declaration2Invoice;
		CusHAWB Declaration2Cushawb;
		#endregion
		#region Declaration3
		Customs.Business.BaseJobDeclaration Declaration3
		{
			get
			{
				if (fDeclaration3 == null)
				{
					fDeclaration3 = CreateDeclaration("DECLARATION3", Supplier2.PK, -11);
					AddLinkedData(fDeclaration3, ref Declaration3Invoice, ref Declaration3Cushawb);
				}

				return fDeclaration3;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclaration3;
		Customs.Business.BaseJobComInvoiceHeader Declaration3Invoice;
		CusHAWB Declaration3Cushawb;
		#endregion
		#region Declaration4
		Customs.Business.BaseJobDeclaration Declaration4
		{
			get
			{
				if (fDeclaration4 == null)
				{
					fDeclaration4 = CreateDeclaration("DECLARATION4", Supplier1.PK, -41);
					AddLinkedData(fDeclaration4, ref Declaration4Invoice, ref Declaration4Cushawb);
				}

				return fDeclaration4;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclaration4;
		Customs.Business.BaseJobComInvoiceHeader Declaration4Invoice;
		CusHAWB Declaration4Cushawb;
		#endregion
		#region DeclarationWithMoreThan1Event
		Customs.Business.BaseJobDeclaration DeclarationWithMoreThan1Event
		{
			get
			{
				if (fDeclarationWithMoreThan1Event == null)
				{
					fDeclarationWithMoreThan1Event = CreateDeclaration("MORETHAN1EVENT", Supplier1.PK, -10);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					fDeclarationWithMoreThan1Event.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					AddLinkedData(fDeclarationWithMoreThan1Event, ref DeclarationWithMoreThan1EventInvoice, ref DeclarationWithMoreThan1EventCushawb);
				}

				return fDeclarationWithMoreThan1Event;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclarationWithMoreThan1Event;
		Customs.Business.BaseJobComInvoiceHeader DeclarationWithMoreThan1EventInvoice;
		CusHAWB DeclarationWithMoreThan1EventCushawb;
		#endregion
		#region DeclarationWithJobHeader
		Customs.Business.BaseJobDeclaration DeclarationWithJobHeader
		{
			get
			{
				if (fDeclarationWithJobHeader == null)
				{
					fDeclarationWithJobHeader = CreateDeclaration("WITHJOBHEADER", Supplier2.PK, -40);
					DeclarationWithJobHeaderJobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					DeclarationWithJobHeaderJobHeader.JH_ParentID = fDeclarationWithJobHeader.PK;
					AddLinkedData(fDeclarationWithJobHeader, ref DeclarationWithJobHeaderInvoice, ref DeclarationWithJobHeaderCushawb);
				}

				return fDeclarationWithJobHeader;
			}
		}

		Customs.Business.BaseJobDeclaration fDeclarationWithJobHeader;
		Customs.Business.BaseJobComInvoiceHeader DeclarationWithJobHeaderInvoice;
		CusHAWB DeclarationWithJobHeaderCushawb;
		JobHeader DeclarationWithJobHeaderJobHeader;
		#endregion
		#region Supplier1
		OrgHeader Supplier1
		{
			get
			{
				if (fSupplier1 == null)
				{
					fSupplier1 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					fSupplier1.OH_FullName = "SUPPLIER1 NAME";
					fSupplier1.OH_IsConsignor = true;
				}

				return fSupplier1;
			}
		}

		OrgHeader fSupplier1;
		#endregion
		#region Supplier2
		OrgHeader Supplier2
		{
			get
			{
				if (fSupplier2 == null)
				{
					fSupplier2 = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
					fSupplier2.OH_FullName = "SUPPLIER2 NAME";
					fSupplier2.OH_IsConsignor = true;
				}

				return fSupplier2;
			}
		}

		OrgHeader fSupplier2;
		#endregion
		Customs.Business.BaseJobDeclaration CreateDeclaration(ZString declarationReference, ZGuid supplierPK, ZInt noOfDays)
		{
			Customs.Business.BaseJobDeclaration result = Factory.NewWithValidTestData<Customs.Business.BaseJobDeclaration>(TestBusinessObjectKind.MinimumRequiredToSave);
			result.JE_DeclarationReference = declarationReference;
			result.JE_OH_Supplier = supplierPK;
			result.JE_SystemCreateTimeUtc = ZDateTime.Now.AddDays(noOfDays);
			Factory.Save();
			return result;
		}

		void AddLinkedData(Customs.Business.BaseJobDeclaration declaration, ref Customs.Business.BaseJobComInvoiceHeader invoice, ref CusHAWB cushawb)
		{
			invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			invoice.JZ_JE = declaration.PK;
			cushawb = Factory.NewWithValidTestData<CusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			cushawb.CS_JE_CustomsFormalEntry = declaration.PK;
		}

		void RunClientTNT_DeleteUnusedDeclarations(string jobNumber, string consignorCode, string consignorName, DateTime dateFrom, DateTime dateTo)
		{
			string query = string.Format("exec ClientTNT_DeleteUnusedDeclarations {0},{1},{2},{3},{4}", string.IsNullOrEmpty(jobNumber) ? "null" : "@JobNumber", string.IsNullOrEmpty(consignorCode) ? "null" : "@ConsignorCode", string.IsNullOrEmpty(consignorName) ? "null" : "@ConsignorName", dateFrom == DateTime.MinValue ? "null" : "@DateFrom", dateTo == DateTime.MinValue ? "null" : "@DateTo");
			using (CargoWise.Data.DbCommand command = CargoWise.Data.Db.Connection.Command(query))
			{
				if (!string.IsNullOrEmpty(jobNumber))
				{
					command.AddParameter("@JobNumber", System.Data.SqlDbType.VarChar, jobNumber);
				}

				if (!string.IsNullOrEmpty(consignorCode))
				{
					command.AddParameter("@ConsignorCode", System.Data.SqlDbType.NVarChar, consignorCode);
				}

				if (!string.IsNullOrEmpty(consignorName))
				{
					command.AddParameter("@ConsignorName", System.Data.SqlDbType.NVarChar, consignorName);
				}

				if (dateFrom != DateTime.MinValue)
				{
					command.AddParameter("@DateFrom", System.Data.SqlDbType.DateTime, dateFrom);
				}

				if (dateTo != DateTime.MinValue)
				{
					command.AddParameter("@DateTo", System.Data.SqlDbType.DateTime, dateTo);
				}

				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}
		#endregion
	}
}
