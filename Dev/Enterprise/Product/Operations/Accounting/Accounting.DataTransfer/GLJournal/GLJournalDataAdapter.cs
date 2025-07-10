using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class GLJournalDataAdapter : BaseAccountingDataAdapter<GLJournal, Xsd.GLJournal>, IGLJournalDataAdapter
	{
		public void Initialize(GLJournalDataAdapterSettings settings)
		{
			Settings = settings;
		}

		public static string InvalidXmlFileErrorMessage
		{
			get { return Res.GetString("482fc977-30b3-408d-b597-bd6b7b3416c7", "The journal XML file you tried to import was invalid."); }
		}
		public static string MoreThanOneJournalErrorMsg
		{
			get { return Res.GetString("2a1c8945-6225-4b20-a712-10813076fb26", "There is more than one Journal transaction in the XML file."); }
		}

		public static string AutoJournalDescription
		{
			get { return Res.GetString("932eeabb-2ba5-4f51-aab6-0f6cccd231e0", "Auto Journal"); }
		}

		public static string ReverseJournalDescription
		{
			get { return Res.GetString("71ed9076-5e47-492b-a2a7-254ff5be8ddd", "Reversing Journal"); }
		}

		public static string GeneralJournalDescription
		{
			get { return Res.GetString("1eb20475-fa0a-495e-b7dc-4f5db3ddde2d", "General Journal"); }
		}

		public static string NoteJournalDescription
		{
			get { return Res.GetString("87ac1ae5-4128-4b1b-86bf-5ba6191acaf7", "Note Journal"); }
		}

		public static string FCBJournalDescription
		{
			get { return Res.GetString("62a3a9fc-9b91-4716-8041-4a11406e711b", "FOREIGN CURRENCY BALANCE ADJUSTMENT"); }
		}

		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GLJournals"; }
		}

		public override string RootElementName
		{
			get { return "GLJournal"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.SingleGLJournalSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLJournalsSchema; }
		}

		protected override GLJournal NewBusinessObject(Xsd.GLJournal value, IValueObjectImportContext context)
		{
			GLJournal result;
			var factory = Settings.factoryForNewJournal ?? context.Factory;
			if (value.GLDetail.JournalType == Xsd.GLJournalGLDetailJournalType.FCB)
			{
				result = factory.New<FCBAdjustmentJournal>();
			}
			else
			{
				result = factory.New<GLJournal>();
			}

			return result;
		}

		protected override GLJournal FindBusinessObject(Xsd.GLJournal value, IValueObjectImportContext context)
		{
			return (GLJournal)Settings.journalToPopulate ?? base.FindBusinessObject(value, context);
		}

		protected override bool AllowDifferentImportContextFactory
		{
			get
			{
				return Settings.factoryForNewJournal != null;
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(GLJournal bizObj, Xsd.GLJournal constructedValueObject, IValueObjectExportContext context)
		{
			//
			//!!! All journal fields used here should be used in GLJournalApprovalRequestDetails.AreJournalsEqual !!!
			//
			var journal = bizObj;
			var journalXML = constructedValueObject;

			var journalDetail = journalXML.GLDetail;
			if (journal.AH_ReceiptType == ReceiptTypes.ForeignCurrencyBalance)
			{
				journalDetail.JournalType = Xsd.GLJournalGLDetailJournalType.FCB;
			}
			else
			{
				switch (journal.AH_TransactionType)
				{
					case TransactionTypes.GLStandardJournal:
						journalDetail.JournalType = Xsd.GLJournalGLDetailJournalType.GJL;
						break;
					case TransactionTypes.GLReversingJournal:
						journalDetail.JournalType = Xsd.GLJournalGLDetailJournalType.RJL;
						break;
					case TransactionTypes.GLAutoJournal:
						journalDetail.JournalType = Xsd.GLJournalGLDetailJournalType.AJL;
						break;
					case TransactionTypes.GLNoteJournal:
						journalDetail.JournalType = Xsd.GLJournalGLDetailJournalType.NJL;
						break;
				}
			}

			if (Settings.useJournalNumber)
			{
				journalDetail.JournalNumber = journal.AH_TransactionNum;
			}
			journalDetail.Presentation = journal.AH_TransactionCategory;
			journalDetail.Description = journal.AH_Desc;
			journalDetail.InPeriod = journal.PostPeriod.ToString();
			if (journal.IsPostDateEnabled)
			{
				journalDetail.InPeriodDate = journal.AH_PostDate.Date;
			}
			journalDetail.OutPeriod = journal.AgePeriod.ToString();
			if (journal.IsDueDateEnabled)
			{
				journalDetail.OutPeriodDate = journal.AH_DueDate.Date;
			}
			journalDetail.Branch = journal.Branch.GB_Code;
			journalDetail.Department = journal.Department.GE_Code;

			foreach (GLJournalLine line in journal.Lines)
			{
				var lineXML = journalXML.JournalLines.AddNew();

				if (Settings.useLinePKs)
				{
					lineXML.PK = line.PK.ToString();
				}

				lineXML.Account = line.GLHeader?.AG_AccountNum ?? ZString.Empty;
				lineXML.Branch = line.Branch?.GB_Code ?? ZString.Empty;
				lineXML.Department = line.Department?.GE_Code ?? ZString.Empty;
				lineXML.Description = line.AL_Desc;
				if (line.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.LocalCurrency.Code)
				{
					lineXML.Currency = line.AL_RX_NKTransactionCurrency;
					lineXML.ExchangeRate = line.AL_ExchangeRate;
				}

				lineXML.AmountSpecified = false;
				lineXML.LocalAmount.Value = line.UnsignedLocalLineAmount;

				if (line.DebitCreditSign == DebitCreditDataEntry.DR)
				{
					lineXML.DRCR = Xsd.GLJournalJournalLineDRCR.DR;
				}
				else if (line.DebitCreditSign == DebitCreditDataEntry.CR)
				{
					lineXML.DRCR = Xsd.GLJournalJournalLineDRCR.CR;
				}

				foreach (TransactionLineSubAccount subAccount in line.SubAccounts)
				{
					var subAccountXML = Invoices.SubAccountHelper.GetSubAccountXmlFromSubAccountId(subAccount.Factory, subAccount.SubAccountTypeDisplayCode, subAccount.AL1_SubClassParentId);
					if (subAccountXML != null)
					{
						lineXML.SubAccounts.Add(subAccountXML);
					}
				}

				if (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
				{
					var lineDissectionAttributes = line.AccTransactionLineDissectionAttributes;

					foreach (AccTransactionLineDissectionAttribute lineDissectionAttribute in lineDissectionAttributes)
					{
						if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
						{
							var orgHeader = line.Factory.Load<OrgHeader>(lineDissectionAttribute.ALD_AttributeValueID);
							lineXML.ORG = orgHeader?.OH_Code ?? ZString.Empty;
						}
						else if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG)
						{
							lineXML.OCG = lineDissectionAttribute.ALD_AttributeValue;
						}
						else if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO)
						{
							lineXML.LFO = lineDissectionAttribute.ALD_AttributeValue;
						}
						else if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE)
						{
							lineXML.LFE = lineDissectionAttribute.ALD_AttributeValue;
						}
						else if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC)
						{
							lineXML.TIC = lineDissectionAttribute.ALD_AttributeValue;
						}
						else if (lineDissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR)
						{
							lineXML.SPR = lineDissectionAttribute.ALD_AttributeValue;
						}
					}
				}
				if (line.Header != null)
				{
					lineXML.Organisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(line.Header, context);
				}
			}
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(GLJournal bizObj, Xsd.GLJournal value, IValueObjectImportContext context)
		{
			//
			//!!! All journal fields used here should be used in GLJournalApprovalRequestDetails.AreJournalsEqual !!!
			//
			if (value != null)
			{
				var journal = bizObj;
				var xmlJournal = value;
				var xmlGLDetail = xmlJournal.GLDetail;

				journal.Factory.SuspendValidation();
				try
				{
					ProcessHeader(journal, xmlGLDetail, context);

					if (Settings.useLinePKs)
					{
						var xmlJournalLinePKs = new HashSet<ZGuid>(xmlJournal.JournalLines.Cast<Xsd.GLJournalJournalLine>().Select(x => GetJournalLinePK(x)).Where(x => x.IsValid));
						var linesToDelete = from GLJournalLine line in journal.Lines
											where !xmlJournalLinePKs.Contains(line.PK) && !line.HasAssignedExportBatchNumber
											select line;
						linesToDelete.ToList().ForEach(line => line.Delete());
					}

					using (journal.Lines.SuspendListChanged())
					{
						foreach (Xsd.GLJournalJournalLine xmlJournalLine in xmlJournal.JournalLines)
						{
							ProcessLine(journal, xmlJournalLine, xmlGLDetail, context);
						}
					}
				}
				finally
				{
					journal.Factory.ResumeValidation();
				}
			}
		}

		void ProcessHeader(GLJournal journal, Xsd.GLJournalGLDetail xmlGLDetail, IValueObjectImportContext context)
		{
			var interchange = (Xsd.XmlInterchange)context.Interchange;
			INotifications notify = context;

			if (!(journal is FCBAdjustmentJournal))
			{
				journal.AH_TransactionType = xmlGLDetail.JournalType.ToString();
			}
			journal.PostPeriod = ZInt.CanParse(xmlGLDetail.InPeriod) ? ZInt.Parse(xmlGLDetail.InPeriod) : ZInt.Zero;
			if (journal.IsPostDateEnabled && xmlGLDetail.InPeriodDateSpecified)
			{
				journal.AH_PostDate = xmlGLDetail.InPeriodDate;
			}

			if (xmlGLDetail.Description == "")
			{
				switch (xmlGLDetail.JournalType)
				{
					case Xsd.GLJournalGLDetailJournalType.AJL:
						xmlGLDetail.Description = AutoJournalDescription;
						break;

					case Xsd.GLJournalGLDetailJournalType.RJL:
						xmlGLDetail.Description = ReverseJournalDescription;
						break;

					case Xsd.GLJournalGLDetailJournalType.GJL:
						xmlGLDetail.Description = GeneralJournalDescription;
						break;

					case Xsd.GLJournalGLDetailJournalType.FCB:
						xmlGLDetail.Description = FCBJournalDescription;
						break;

					case Xsd.GLJournalGLDetailJournalType.NJL:
						xmlGLDetail.Description = NoteJournalDescription;
						break;
				}
			}
			if (Settings.useJournalNumber)
			{
				context.SetPropertyInfoValue(journal.AH_TransactionNumInfo, xmlGLDetail.JournalNumber, xmlGLDetail.JournalNumberSpecified);
			}
			context.SetPropertyInfoValue(journal.AH_TransactionCategoryInfo, xmlGLDetail.Presentation, xmlGLDetail.PresentationSpecified);
			context.SetPropertyInfoValue(journal.AH_DescInfo, xmlGLDetail.Description, xmlGLDetail.DescriptionSpecified);

			if (!journal.IsStdJournal)
			{
				journal.AgePeriod = ZInt.CanParse(xmlGLDetail.OutPeriod) ? ZInt.Parse(xmlGLDetail.OutPeriod) : ZInt.Zero;
				if (journal.IsDueDateEnabled && xmlGLDetail.OutPeriodDateSpecified)
				{
					journal.AH_DueDate = xmlGLDetail.OutPeriodDate;
				}
			}
		}

		void ProcessLine(GLJournal journal, Xsd.GLJournalJournalLine xmlJournalLine, Xsd.GLJournalGLDetail xmlGLDetail, IValueObjectImportContext context)
		{
			GLJournalLine journalLine = null;
			if (Settings.useLinePKs)
			{
				ZGuid linePK = GetJournalLinePK(xmlJournalLine);
				journalLine = (GLJournalLine)journal.Lines.FindByPK(linePK);
			}
			if (journalLine == null)
			{
				journalLine = journal.GLJournalLines.AddNew();
			}
			ProcessLine(journalLine, xmlJournalLine, xmlGLDetail, context);

			if (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value)
			{
				ProcessAttributes(journalLine.AccTransactionLineDissectionAttributes, xmlJournalLine);
			}
		}

		void ProcessAttributes(AccTransactionLineDissectionAttributeCollection attributes, Xsd.GLJournalJournalLine xmlJournalLine)
		{
			if (attributes.Any())
			{
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, xmlJournalLine.ORG);
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, xmlJournalLine.OCG);
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, xmlJournalLine.LFO);
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, xmlJournalLine.LFE);
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, xmlJournalLine.TIC);
				UpdateTransactionLineDissectionAttribute(attributes, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, xmlJournalLine.SPR);
			}
		}

		void UpdateTransactionLineDissectionAttribute(AccTransactionLineDissectionAttributeCollection attributes, string attr, ZString attrValue)
		{
			if (!string.IsNullOrEmpty(attrValue))
			{
				var transactionLineDissectionAttribute = attributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(a => a.ALD_Attribute == attr);
				if (transactionLineDissectionAttribute == null)
				{
					ErrorReporter.ReportOnce("DissectionAttributeMustBeCreated", string.Format(CultureInfo.InvariantCulture, "{0} dissection attribute don't be created", attr));
					return;
				}
				if (attr == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
				{
					var organization = transactionLineDissectionAttribute.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, attrValue);
					transactionLineDissectionAttribute.ALD_AttributeValueID = organization.PK;
				}
				else
				{
					transactionLineDissectionAttribute.ALD_AttributeValue = attrValue;
				}
				transactionLineDissectionAttribute.ALD_Attribute = attr;
			}
		}

		ZGuid GetJournalLinePK(Xsd.GLJournalJournalLine xmlJournalLine)
		{
			ZGuid linePK;
			if (Settings.useLinePKs && ZGuid.TryParse(xmlJournalLine.PK, out linePK))
			{
				return linePK;
			}
			else
			{
				return ZGuid.Empty;
			}
		}

		void ProcessLine(GLJournalLine journalLine, Xsd.GLJournalJournalLine xmlJournalLine, Xsd.GLJournalGLDetail xmlGLDetail, IValueObjectImportContext context)
		{
			try
			{
				var journalLineHasAssignedExportBatchNumber = journalLine.HasAssignedExportBatchNumber;

				var description = xmlJournalLine.Description != "" ? xmlJournalLine.Description : xmlGLDetail.Description;
				SetValue(() => journalLine.AL_Desc, (x) => journalLine.AL_Desc = x, description, journalLineHasAssignedExportBatchNumber);

				AccGLHeader gLHeader = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(ReadOnlyFactory, xmlJournalLine.Account, false);

				if (gLHeader != null)
				{
					SetValue(() => journalLine.AL_AG, (x) => journalLine.AL_AG = x, gLHeader.PK, journalLineHasAssignedExportBatchNumber);
				}

				GetBranch(journalLine, xmlJournalLine, xmlGLDetail, journalLineHasAssignedExportBatchNumber);
				GetDepartment(journalLine, xmlJournalLine, xmlGLDetail, journalLineHasAssignedExportBatchNumber);

				SetValue(() => journalLine.DebitCreditSign, (x) => journalLine.DebitCreditSign = x, (ZString)xmlJournalLine.DRCR.ToString(), journalLineHasAssignedExportBatchNumber);

				if (xmlJournalLine.CurrencySpecified)
				{
					var currency = xmlJournalLine.Currency.PadRight(3).Substring(0, 3).Trim();
					SetValue(() => journalLine.AL_RX_NKTransactionCurrency, (x) => journalLine.AL_RX_NKTransactionCurrency = x, currency, journalLineHasAssignedExportBatchNumber);
				}

				var shouldSetOSAmount = false;
				if (journalLine.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					var isAmountSpecified = xmlJournalLine.Amount.Value > 0;
					var isLocalAmountSpecified = xmlJournalLine.LocalAmount.Value > 0;
					if ((!isAmountSpecified || isAmountSpecified && !isLocalAmountSpecified) && xmlJournalLine.ExchangeRateSpecified)
					{
						journalLine.AL_ExchangeRate = xmlJournalLine.ExchangeRate;
						SetValue(() => journalLine.AL_ExchangeRate, (x) => journalLine.AL_ExchangeRate = x, xmlJournalLine.ExchangeRate, journalLineHasAssignedExportBatchNumber);
					}
					else if (isAmountSpecified)
					{
						ZDecimal exRate = Env.CurrentCompany.ExchangeRate.GetRate(xmlJournalLine.LocalAmount.Value, xmlJournalLine.Amount.Value);
						SetValue(() => journalLine.AL_ExchangeRate, (x) => journalLine.AL_ExchangeRate = x, exRate, journalLineHasAssignedExportBatchNumber);
					}

					if (isAmountSpecified)
					{
						shouldSetOSAmount = true;
						SetValue(() => journalLine.UnsignedOSLineAmount, (x) => journalLine.UnsignedOSLineAmount = x, xmlJournalLine.Amount.Value, journalLineHasAssignedExportBatchNumber);
					}
				}

				if (journalLine.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency && xmlJournalLine.LocalAmount.Value == 0 && xmlJournalLine.Amount.Value != 0)
				{
					var localAmount = (ZDecimal)AccountingUtils.Round(xmlJournalLine.Amount.Value, journalLine.AL_RX_NKTransactionCurrency);
					SetValue(() => journalLine.UnsignedLocalLineAmount, (x) => journalLine.UnsignedLocalLineAmount = x, localAmount, journalLineHasAssignedExportBatchNumber);
				}
				else if (xmlJournalLine.LocalAmount.Value > 0)
				{
					using (shouldSetOSAmount ? journalLine.GetOSAmountCalculationSuspender() : null)
					{
						var localAmount = (ZDecimal)AccountingUtils.Round(xmlJournalLine.LocalAmount.Value, journalLine.AL_RX_NKTransactionCurrency);
						SetValue(() => journalLine.UnsignedLocalLineAmount, (x) => journalLine.UnsignedLocalLineAmount = x, localAmount, journalLineHasAssignedExportBatchNumber);
					}
				}

				if (xmlJournalLine.Organisation.IsSpecified)
				{
					var orgPK = context.FindOrCreateTempOrganisationPK(xmlJournalLine.Organisation, null, OrganisationTypes.None);
					SetValue(() => journalLine.AL_OH, (x) => journalLine.AL_OH = x, orgPK, journalLineHasAssignedExportBatchNumber);
				}

				if (xmlJournalLine.SubAccounts.Count > 0)
				{
					Invoices.SubAccountHelper.CreateSubAccountFromXml(journalLine, xmlJournalLine.SubAccounts,
						(subAccount, value) => SetValue(() => subAccount.AL1_SubClassParentId, (x) => subAccount.AL1_SubClassParentId = x, value, journalLineHasAssignedExportBatchNumber));
				}
				else if (!xmlJournalLine.SubAccount.Code.IsEmpty)
				{
					ImportSubAccountForBackwardCompatibility(journalLine, xmlJournalLine.SubAccount.Type.Code, xmlJournalLine.SubAccount.Code, journalLineHasAssignedExportBatchNumber);
				}
			}
			catch (CantUpdateValueException)
			{
				GLJournalLine newLine = journalLine.JournalHeader.GLJournalLines.AddNew();
				ProcessLine(newLine, xmlJournalLine, xmlGLDetail, context);
			}
		}

		void SetValue<T>(Func<T> getPropertyValue, Action<T> setPropertyValue, T value, bool journalLineHasAssignedExportBatchNumber) where T : struct
		{
			if (journalLineHasAssignedExportBatchNumber)
			{
				if (!getPropertyValue().Equals(value))
				{
					throw new CantUpdateValueException();
				}
			}
			else
			{
				setPropertyValue(value);
			}
		}

		void ImportSubAccountForBackwardCompatibility(DependentTransactionLine line, ZString subAccountType, ZString subAccountCode, bool journalLineHasAssignedExportBatchNumber)
		{
			if (line.GLHeader != null)
			{
				var targetSubAccount = line.SubAccounts.Cast<TransactionLineSubAccount>().SingleOrDefault(x => x.AL1_SubClassParentTableCode == SubAccountCodeConverter.ConvertSubClassCodeToSubAccountDBParentTableCode(subAccountType));
				if (targetSubAccount != null)
				{
					var parentId = Invoices.SubAccountHelper.GetSubAccountPKFromCode(line.Factory, subAccountType, subAccountCode);
					SetValue(() => targetSubAccount.AL1_SubClassParentId, (x) => targetSubAccount.AL1_SubClassParentId = x, parentId, journalLineHasAssignedExportBatchNumber);
				}
			}
		}

		void GetBranch(GLJournalLine journalLine, Xsd.GLJournalJournalLine xmlJournalLine, Xsd.GLJournalGLDetail xmlGLDetail, bool journalLineHasAssignedExportBatchNumber)
		{
			GlbBranch branch1 = null;
			if (xmlJournalLine.Branch != "")
			{
				branch1 = BusinessObjectRetriever.GetBranchFromBranchCode(ReadOnlyFactory, xmlJournalLine.Branch);
			}

			if (branch1 == null && xmlGLDetail.Branch != "")
			{
				branch1 = BusinessObjectRetriever.GetBranchFromBranchCode(ReadOnlyFactory, xmlGLDetail.Branch);
			}

			var branch = branch1 != null ? branch1.PK : GlbBranch.CurrentBranch.PK;
			SetValue(() => journalLine.AL_GB, (x) => journalLine.AL_GB = x, branch, journalLineHasAssignedExportBatchNumber);
		}

		void GetDepartment(GLJournalLine journalLine, Xsd.GLJournalJournalLine xmlJournalLine, Xsd.GLJournalGLDetail xmlGLDetail, bool journalLineHasAssignedExportBatchNumber)
		{
			GlbDepartment department1 = null;
			if (xmlJournalLine.Department != "")
			{
				department1 = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(ReadOnlyFactory, xmlJournalLine.Department);
			}

			if (department1 == null && xmlGLDetail.Department != "")
			{
				department1 = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(ReadOnlyFactory, xmlGLDetail.Department);
			}

			var department = department1 != null ? department1.PK : GlbDepartment.CurrentDepartment.PK;
			SetValue(() => journalLine.AL_GE, (x) => journalLine.AL_GE = x, department, journalLineHasAssignedExportBatchNumber);
		}

		#endregion

		#region Implementation

		BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}
				return fReadOnlyFactory;
			}
		}
		BusinessObjectFactory fReadOnlyFactory;

		GLJournalDataAdapterSettings Settings { get; set; }

		[Serializable]
		protected class CantUpdateValueException : Exception
		{
			public CantUpdateValueException()
			{
			}

			public CantUpdateValueException(string message)
				: base(message)
			{
			}

			public CantUpdateValueException(string message, Exception inner)
				: base(message, inner)
			{
			}

#if NETFRAMEWORK
			protected CantUpdateValueException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}

		#endregion

	}
}
