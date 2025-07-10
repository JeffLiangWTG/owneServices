using System;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.AccountBalanceUpdate
{
	public class BalanceValueObjectDataAdapter : ValueObjectDataAdapter<Journal, Xsd.Balance>
	{
		#region Data Adapter Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not for front end")]
		public override string RootCollectionElementName
		{
			get { return "Balances"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not for front end")]
		public override string RootElementName
		{
			get { return "Balance"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.SingleBalanceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.BalancesSchema; }
		}

		protected override Journal NewBusinessObject(Xsd.Balance journal, IValueObjectImportContext context)
		{
			Journal bizObj = null;

			Type newBizObjType = journal.Ledger == Xsd.TxnLedgerType.AR ? typeof(ARJournal) : typeof(APJournal);
			if (newBizObjType != null)
			{
				GlbBranch branch = context.Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, journal.BranchCode));

				if (branch != null)
				{
					if (factories == null)
					{
						factories = new Dictionary<ZGuid, BusinessObjectFactory>();
					}

					if (!factories.ContainsKey(branch.GB_GC))
					{
						BusinessObjectFactory newFactory = new BusinessObjectFactory();
						factories.Add(branch.GB_GC, newFactory);
						context.Factory.ChildFactories.Add(newFactory);
					}
					bizObj = factories[branch.GB_GC].New(newBizObjType) as Journal;
				}

				if (bizObj == null)
				{
					bizObj = context.Factory.New(newBizObjType) as Journal;
				}
			}

			return bizObj;
		}

		Dictionary<ZGuid, BusinessObjectFactory> factories;

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		public override Journal CreateOrUpdateFromValueObject(Xsd.Balance value, IValueObjectImportContext context)
		{
			Journal journal = null;

			var branch = context.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, value.BranchCode);

			using (branch != null ? (new TemporaryUserContext() { BranchPK = branch.PK.ToGuid() }).Set() : null)
			{
				try
				{
					journal = base.CreateOrUpdateFromValueObject(value, context);
				}
				catch (ArgumentNullException)
				{
					NotificationManager notificationManager = new NotificationManager(context);
					notificationManager.AddErrorToNotifications(Res.GetString("f8e7abd0-5d08-480e-bfda-c67cfa760ed8", "This transaction cannot be imported. Transaction type is not compatible."));
				}
			}
			return journal;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(Journal bizObj, Xsd.Balance constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting journal is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(Journal bizObj, Xsd.Balance value, IValueObjectImportContext context)
		{
			NotificationManager notificationManager = new NotificationManager(context);
			string errorContext = value.Ledger.ToString() + " " + Res.GetString("beeddb0a-0d53-4ab4-a0c1-a3e3c9b8f836", "Journal for organization {0} in branch {1}:", value.OrganisationCode, value.BranchCode);

			if (bizObj != null)
			{
				notificationManager.AddInfoNotification(Res.GetString("e85c103d-79d9-4b09-8fe4-3afdb762131c", "Processing : {0}", errorContext));
				ProcessTransaction(bizObj, value, context, errorContext, notificationManager);
			}
			else if (value.GetType() == typeof(Xsd.Balance))
			{
				string errorMessage = Res.GetString("e7559b07-ade6-4f22-b6a5-de4d3dc29e09", "This Journal cannot be imported.") + " ";
				errorMessage += Res.GetString("0e5a96d2-630e-484b-b2f3-7ba138d81d4f", "Ledger:") + " " + value.Ledger.ToString() + ", ";
				errorMessage += Res.GetString("14db76e7-0142-4a84-bd7d-70d67f4f32a0", "Transaction Type: {0},", value.OrganisationCode) + " ";
				errorMessage += Res.GetString("47002ea2-b876-4071-83b1-66b1b5568221", "Branch:") + " " + value.BranchCode.ToString() + ".";
				notificationManager.AddErrorToNotifications(errorMessage);
			}
			else
			{
				notificationManager.AddErrorToNotifications(Res.GetString("d3192c25-7b46-4b80-b4a4-978476d280a5", "This transaction cannot be imported."));
			}
		}

		void ProcessTransaction(Journal journal, Xsd.Balance balance, IValueObjectImportContext importContext, string errorContext, NotificationManager notificationManager)
		{
			OrgHeader matchedOrganisation = GetMatchedOrganisation(balance, importContext);
			GlbBranch branch = importContext.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, balance.BranchCode);
			GlbDepartment department = importContext.Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, balance.DepartmentCode);

			if (matchedOrganisation == null)
			{
				notificationManager.AddErrorToNotifications(Res.GetString("548a4362-8438-4961-8516-69578a6e9576", "{0} Could not match organization {1}. Account Balance will not be updated.", errorContext, balance.OrganisationCode));
				journal.DeleteFromDB();
			}
			else if (branch == null)
			{
				notificationManager.AddErrorToNotifications(Res.GetString("628b78c0-ab12-4f59-977f-491654faf96b", "{0} Could not match branch {1}. Account Balance will not be updated.", errorContext, balance.BranchCode));
				journal.DeleteFromDB();
			}
			else
			{
				ZDecimal amountToUpdate = GetAmount(balance, matchedOrganisation, branch, importContext);
				if (amountToUpdate == ZDecimal.Zero)
				{
					notificationManager.AddInfoNotification(Res.GetString("78d7a264-1e58-4a08-9bc2-fb53c2e1dd11", "{0} balance not updated as it is already correct.", errorContext));
					journal.DeleteFromDB();
				}
				else
				{
					journal.AH_OH = matchedOrganisation.PK;
					journal.AH_Ledger = balance.Ledger.ToString();
					journal.AH_GB = branch.PK;
					journal.AH_GE = department != null ? department.PK : GlbDepartment.CurrentDepartment.PK;
					journal.AH_RX_NKTransactionCurrency = branch.Company.GC_RX_NKLocalCurrency;

					importContext.SetPropertyInfoValue(journal.AH_OutstandingAmountInfo, amountToUpdate, AccTransactionHeaderSchema.AH_OutstandingAmount);
					importContext.SetPropertyInfoValue(journal.AH_InvoiceAmountInfo, amountToUpdate, AccTransactionHeaderSchema.AH_InvoiceAmount);
					importContext.SetPropertyInfoValue(journal.AH_OSTotalInfo, amountToUpdate, AccTransactionHeaderSchema.AH_OSTotal);

					journal.OnLoaded();

					Validate(journal, importContext);
				}
			}
		}

		void Validate(Journal journal, IValueObjectImportContext importContext)
		{
			journal.RunPreSaveValidation();

			if (journal.HasErrors)
			{
				foreach (var notification in journal.Notifications)
				{
					importContext.Add(notification);
				}
				journal.DeleteFromDB();
			}
		}

		ZDecimal GetAmount(Xsd.Balance value, OrgHeader matchedOrganisation, GlbBranch branch, IValueObjectImportContext context)
		{
			ZDecimal outstandingAmount = ZDecimal.Zero;
			string sqlText = @"SELECT SUM(AH_OutstandingAmount) as OutstandingAmount 
                                                              FROM dbo.AccTransactionHeader 
                                                              WHERE AH_IsCancelled <> 1
                                                              AND AH_OH = @OrgHeader
                                                              AND AH_Ledger = @Ledger
                                                              AND AH_GC = @Company";

			DynamicBusinessObjectCollection query = new DynamicBusinessObjectCollection(context.Factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();

			parameters.Add("@OrgHeader", matchedOrganisation.PK.ToGuid(), AccTransactionHeaderSchema.AH_OH);
			parameters.Add("@Ledger", value.Ledger.ToString(), AccTransactionHeaderSchema.AH_Ledger);
			parameters.Add("@Company", branch.Company.PK.ToGuid(), GlbBranchSchema.GB_GC);

			query.Load(sqlText, parameters);
			outstandingAmount = (ZDecimal)query[0]["OutstandingAmount"];

			return value.BalanceValue - outstandingAmount;
		}

		OrgHeader GetMatchedOrganisation(Xsd.Balance value, IValueObjectImportContext context)
		{
			if (SystemDataRegistry.Instance.OrganisationMatchnigTypeItem.Value == "LEG")
			{
				return FindOrganisationByLegacyCode(value, context);
			}
			else
			{
				return FindOrganisationByEnterpriseCode(value, context);
			}
		}

		OrgHeader FindOrganisationByLegacyCode(Xsd.Balance value, IValueObjectImportContext context)
		{
			OrgHeader result = null;

			ZString legacySystemCode = value.OrganisationCode;
			OrgCusCode orgCusCode = (new OrgCusCode.Loader(context.Factory)).LoadFromLegacyCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, legacySystemCode);
			if (orgCusCode != null)
			{
				result = context.Factory.Load<OrgHeader>(orgCusCode.OK_OH);
			}

			return result;
		}

		OrgHeader FindOrganisationByEnterpriseCode(Xsd.Balance value, IValueObjectImportContext context)
		{
			return context.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, value.OrganisationCode);
		}

		#endregion
	}
}
