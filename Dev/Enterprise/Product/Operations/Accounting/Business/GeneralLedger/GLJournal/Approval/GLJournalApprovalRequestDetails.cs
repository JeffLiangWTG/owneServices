using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GLJournalApprovalRequestDetails : ApprovalRequestDetails
	{
		#region Schema

		public new abstract class Schema : ApprovalRequestDetails.Schema
		{
			public const string Journal = "Journal";
		}

		#endregion

		[Obsolete("For serializer only")]
		protected GLJournalApprovalRequestDetails()
			: base(new BusinessObjectFactory())
		{
		}

		public GLJournalApprovalRequestDetails(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Journal

		[ResourceStringData("GLJournalApprovalRequestDetails|Journal", Caption = "Journal")]
		public GLJournal Journal
		{
			get { return journal; }
			private set
			{
				if (journal == null && value != null)
				{
					journal = value;
				}
				else if (journal != null)
				{
					ErrorReporter.ReportOnce("Accounting.GLJournalApprovalRequestDetails.Journal", "A try to override live Journal on GLJournalApprovalRequestDetails. This is not allowed as Journal can be bound to a GUI control.");
				}
			}
		}
		GLJournal journal;

		internal GLJournal JournalReplacementForSerialization { get; set; }
		internal GLJournal JournalReplacementForDeserialization { get; set; }
		internal BusinessObjectFactory FactoryReplacementForDeserialization { get; set; }

#if DEBUG
		public void SetJournal_ForTestOnly(GLJournal value)
		{
			Journal = value;
		}
#endif

		#endregion

		#region Overrides 

		protected override void CopyFromCore(ApprovalRequestDetails approvalDetailsToCopy)
		{
			base.CopyFromCore(approvalDetailsToCopy);

			var approvalDetailsToCopyCasted = approvalDetailsToCopy as GLJournalApprovalRequestDetails;
			if (approvalDetailsToCopyCasted != null)
			{
				Journal = approvalDetailsToCopyCasted.Journal;
			}
		}

		protected override bool IsEqual(ApprovalRequestDetails b)
		{
			var b_Casted = b as GLJournalApprovalRequestDetails;
			if (b_Casted != null)
			{
				return AreJournalsEqual(Journal, b_Casted.Journal);
			}

			return false;
		}

		static bool AreJournalsEqual(GLJournal journal1, GLJournal journal2)
		{
			//
			//!!! All journal fields here should be used in GLJournalDataAdapter to do journal serialization or in this class serialization !!!
			//
			if (journal1 == null && journal2 == null)
			{
				return true;
			}
			if (journal1 == null || journal2 == null)
			{
				return false;
			}

			var result =
				journal1.AH_TransactionNum == journal2.AH_TransactionNum &&
				journal1.AH_TransactionCategory == journal2.AH_TransactionCategory &&
				journal1.AH_TransactionType == journal2.AH_TransactionType &&
				journal1.AH_ReceiptType == journal2.AH_ReceiptType &&
				journal1.AH_Desc == journal2.AH_Desc &&
				journal1.AH_PostDate == journal2.AH_PostDate &&
				journal1.PostPeriod == journal2.PostPeriod &&
				journal1.AH_DueDate == journal2.AH_DueDate &&
				journal1.AgePeriod == journal2.AgePeriod &&
				journal1.AH_GB == journal2.AH_GB &&
				journal1.AH_GE == journal2.AH_GE &&
				journal1.Lines.Count == journal2.Lines.Count;

			if (result)
			{
				var lines1 = journal1.Lines.Cast<GLJournalLine>().ToList();
				var lines2 = journal2.Lines.Cast<GLJournalLine>().ToList();

				for (int index1 = 0; index1 < lines1.Count; index1++)
				{
					var line1 = lines1[index1];
					int index2;
					for (index2 = 0; index2 < lines2.Count; index2++)
					{
						var line2 = lines2[index2];
						if (AreJournalLinesEqual(line1, line2))
						{
							lines1.RemoveAt(index1--);
							lines2.RemoveAt(index2--);
							break;
						}
					}
					if (index2 == lines2.Count)
					{
						break;
					}
				}

				result = lines1.Count == 0 && lines2.Count == 0;
			}

			return result;
		}

		static bool AreJournalLinesEqual(GLJournalLine line1, GLJournalLine line2)
		{
			if (line1 == null && line2 == null)
			{
				return true;
			}
			if (line1 == null || line2 == null)
			{
				return false;
			}

			return
				line1.AL_AG == line2.AL_AG &&
				line1.AL_GB == line2.AL_GB &&
				line1.AL_GE == line2.AL_GE &&
				line1.AL_Desc == line2.AL_Desc &&
				line1.AL_RX_NKTransactionCurrency == line2.AL_RX_NKTransactionCurrency &&
				line1.AL_ExchangeRate == line2.AL_ExchangeRate &&
				line1.UnsignedLocalLineAmount == line2.UnsignedLocalLineAmount &&
				line1.DebitCreditSign == line2.DebitCreditSign &&
				line1.AL_OH == line2.AL_OH &&
				AreJournalLineSubAccountsEqual(line1?.SubAccounts, line2?.SubAccounts) &&
				(!AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.Value ||
					AreJournalLineAttributesEqual(line1.AccTransactionLineDissectionAttributes, line2.AccTransactionLineDissectionAttributes));
		}

		static bool AreJournalLineAttributesEqual(AccTransactionLineDissectionAttributeCollection accTransactionLineDissectionAttributes1, AccTransactionLineDissectionAttributeCollection accTransactionLineDissectionAttributes2)
		{
			if ((accTransactionLineDissectionAttributes1 == null && accTransactionLineDissectionAttributes2 == null))
			{
				return true;
			}
			if (accTransactionLineDissectionAttributes1 == null || accTransactionLineDissectionAttributes2 == null || accTransactionLineDissectionAttributes1.Count != accTransactionLineDissectionAttributes2.Count)
			{
				return false;
			}

			accTransactionLineDissectionAttributes1.Sort(new Comparison<AccTransactionLineDissectionAttribute>((x, y) => x.ALD_Attribute < y.ALD_Attribute ? 1 : -1));
			accTransactionLineDissectionAttributes2.Sort(new Comparison<AccTransactionLineDissectionAttribute>((x, y) => x.ALD_Attribute < y.ALD_Attribute ? 1 : -1));
			for (var i = 0; i < accTransactionLineDissectionAttributes1.Count; i++)
			{
				var lineAttribute1 = accTransactionLineDissectionAttributes1[i];
				var lineAttribute2 = accTransactionLineDissectionAttributes2[i];
				if (lineAttribute1.ALD_Attribute != lineAttribute2.ALD_Attribute || lineAttribute1.ALD_AttributeValue != lineAttribute2.ALD_AttributeValue || lineAttribute1.ALD_AttributeValueID != lineAttribute2.ALD_AttributeValueID)
				{
					return false;
				}
			}

			return true;
		}

		static bool AreJournalLineSubAccountsEqual(TransactionLineSubAccountCollection lineSubAccounts1, TransactionLineSubAccountCollection lineSubAccounts2)
		{
			if (lineSubAccounts1 == null && lineSubAccounts2 == null)
			{
				return true;
			}

			if (lineSubAccounts1 == null || lineSubAccounts2 == null || lineSubAccounts1.Count != lineSubAccounts2.Count)
			{
				return false;
			}

			if (lineSubAccounts1.Count == 0 && lineSubAccounts2.Count == 0)
			{
				return true;
			}

			var subAccounts1 = new System.Collections.Generic.List<TransactionLineSubAccount>();
			var subAccounts2 = new System.Collections.Generic.List<TransactionLineSubAccount>();
			lineSubAccounts1.CopyToList(subAccounts1);
			lineSubAccounts2.CopyToList(subAccounts2);
			int index2;

			for (int index1 = 0; index1 < subAccounts1.Count; index1++)
			{
				var line1 = subAccounts1[index1];
				for (index2 = 0; index2 < subAccounts2.Count; index2++)
				{
					var line2 = subAccounts2[index2];
					if (AreSubAccountsEqual(line1, line2))
					{
						subAccounts1.RemoveAt(index1--);
						subAccounts2.RemoveAt(index2--);
						break;
					}
				}
			}

			return subAccounts1.Count == 0 && subAccounts2.Count == 0;

			bool AreSubAccountsEqual(TransactionLineSubAccount subAccount1, TransactionLineSubAccount subAccount2)
			{
				if (subAccount1 == null && subAccount2 == null)
				{
					return true;
				}
				if (subAccount1 == null || subAccount2 == null)
				{
					return false;
				}

				return
					subAccount1.AL1_SubClassParentId == subAccount2.AL1_SubClassParentId &&
					subAccount1.AL1_SubClassParentTableCode == subAccount2.AL1_SubClassParentTableCode;
			}
		}

		protected override bool IsPostingActionTheSameCore(ApprovalRequestDetails approvalDetails) => approvalDetails is GLJournalApprovalRequestDetails; //only one posting action is allowed for this type

		#region Serialization

		protected sealed override void ReadXmlCore(XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
			}
			else
			{
				reader.ReadStartElement(Schema.Journal);

				var adapterSettings = new GLJournalDataAdapterSettings { journalToPopulate = JournalReplacementForDeserialization, factoryForNewJournal = FactoryReplacementForDeserialization, useJournalNumber = true, useLinePKs = true };

				JournalReplacementForDeserialization = null;
				FactoryReplacementForDeserialization = null;

				var dataAdapter = GetDataAdapter(adapterSettings);
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				var notifications = new NotificationBuffer();
				var collection = new GLJournalCollection(Factory);
				try
				{
					serializer.ReadInterchangeOrCollectionFromXml(reader, dataAdapter, collection, null, notifications);
					if (adapterSettings.journalToPopulate == null && collection.Count == 1)
					{
						Journal = collection[0];
						var isJournalCreatedForPostingDetailsAndSoJustAsXMLWrapper = adapterSettings.factoryForNewJournal == null;
						if (isJournalCreatedForPostingDetailsAndSoJustAsXMLWrapper)
						{
							Journal.SetReadOnlyIncludingChildren(true);
							Journal.Factory.SuspendValidation();
						}
					}
				}
				finally
				{
					collection.RemoveAll();
				}

				reader.ReadEndElement();
			}
		}

		protected sealed override void WriteXmlCore(XmlWriter writer)
		{
			var journalToSerialize = JournalReplacementForSerialization ?? Journal;
			JournalReplacementForSerialization = null;

			writer.WriteStartElement(Schema.Journal);
			if (journalToSerialize != null)
			{
				var adapterSettings = new GLJournalDataAdapterSettings { useJournalNumber = true, useLinePKs = true };
				var dataAdapter = GetDataAdapter(adapterSettings);
				var serializer = new XmlValueObjectSerializer(dataAdapter.ValueObjectType);
				serializer.WriteToXml(writer, dataAdapter, journalToSerialize, new ValueObjectExportContext(new NotificationBuffer()));
			}
			writer.WriteEndElement();
		}

		static IValueObjectDataAdapter GetDataAdapter(GLJournalDataAdapterSettings? adapterSettings = null)
		{
			var dataAdapter = (IGLJournalDataAdapter)Activator.CreateInstance(ObjectFactory.GetType<IGLJournalDataAdapter>());
			if (adapterSettings.HasValue)
			{
				dataAdapter.Initialize(adapterSettings.Value);
			}

			return (IValueObjectDataAdapter)dataAdapter;
		}

		#endregion

		#endregion
	}
}
