using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class TransactionTypePrefix : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Ledger = "Ledger";
			public const string TransactionType = "TransactionType";
			public const string Prefix = "Prefix";
			public const string IsAutoAdded = "IsAutoAdded";
		}

		#endregion

		TransactionTypePrefixCollection ParentCollection
		{
			get { return (TransactionTypePrefixCollection)GetParentCollection(this, typeof(TransactionTypePrefixCollection)); }
		}

		#region Overrides

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var transactionTypePrefix = new TransactionTypePrefix();
			transactionTypePrefix.IsAutoAdded = IsAutoAdded;

			return transactionTypePrefix;
		}

		#endregion

		#region WriteElements

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Ledger, Ledger);
			writer.WriteElementString(Schema.TransactionType, TransactionType);
			writer.WriteElementString(Schema.Prefix, Prefix);
			writer.WriteElementString(Schema.IsAutoAdded, IsAutoAdded.ToString());
		}

		#endregion

		#region ReadElements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Ledger = reader.ReadElementString(Schema.Ledger);
			TransactionType = reader.ReadElementString(Schema.TransactionType);
			Prefix = (NoResString)reader.ReadElementString(Schema.Prefix);
			IsAutoAdded = reader.ReadElementStringAsZBool(Schema.IsAutoAdded);
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateLedger();
			ValidateTransactionType();
			ValidatePrefix();
			ValidateSamePrefixForCTR();
		}

		public override void Delete()
		{
			if (!IsAutoAdded && IsARAPContraPrefix)
			{
				AutoDeletePairPrefix();
			}
			base.Delete();
		}

		public override bool ReadOnly
		{
			get => base.ReadOnly || IsAutoAdded;
		}

		#endregion

		#region Properties

		#region Ledger

		[MaxLength(2)]
		[List("LedgerList")]
		public ZString Ledger
		{
			get { return ledger; }
			set
			{
				CheckMaximumLength(LedgerInfo, value);

				if (IsARAPContraPrefix && value != LedgerTypes.AccountsPayable && value != LedgerTypes.AccountsReceivable)
				{
					AutoDeletePairPrefix();
				}

				SetNonPersistentPropertyValue(LedgerInfo, ref ledger, value);

				if (!IsValidationSuspended)
				{
					ValidateLedger();
				}

				AutoAddPairPrefix();
			}
		}

		ZString ledger;

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(Schema.Ledger); }
		}

		#endregion

		#region TransactionType

		[MaxLength(3)]
		[List("TransactionTypeList")]
		public ZString TransactionType
		{
			get { return transactionType; }
			set
			{
				CheckMaximumLength(TransactionTypeInfo, value);

				if (IsARAPContraPrefix && value != TransactionTypes.Contra)
				{
					AutoDeletePairPrefix();
				}

				SetNonPersistentPropertyValue(TransactionTypeInfo, ref transactionType, value, false);

				if (!IsValidationSuspended)
				{
					ValidateTransactionType();
				}

				AutoAddPairPrefix();
			}
		}

		ZString transactionType;

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionType); }
		}

		#endregion

		#region Prefix

		[MaxLength(3)]
		public ZString Prefix
		{
			get { return fPrefix; }
			set
			{
				CheckMaximumLength(PrefixInfo, value);
				SetNonPersistentPropertyValue(PrefixInfo, ref fPrefix, value, false);

				if (!IsValidationSuspended)
				{
					ValidatePrefix();
				}

				if (IsARAPContraPrefix && !IsAutoAdded)
				{
					if (AutoAddedPrefix != null)
					{
						AutoAddedPrefix.Prefix = Prefix;
					}
				}
			}
		}

		ZString fPrefix;

		public ZPropertyInfo PrefixInfo
		{
			get { return GetZPropertyInfo(Schema.Prefix); }
		}

		#endregion

		#region IsAutoAdded

		bool IsAutoAdded { get; set; }

		#endregion

		#region List

		public CodeDescriptionPairList LedgerList
		{
			get
			{
				return fLedgerList ?? (fLedgerList = GetLedgerList());
			}
		}

		CodeDescriptionPairList fLedgerList;

		protected CodeDescriptionPairList GetLedgerList()
		{
			var resultPairList = new LedgerTypesList();
			resultPairList.RemoveCode(LedgerTypesList.Codes.UnapprovedPayableTransactions);
			resultPairList.RemoveCode(LedgerTypesList.Codes.TransactionsPendingAllocation);
			resultPairList.RemoveCode(LedgerTypesList.Codes.IncompleteTransactions);

			return resultPairList;
		}

		public CodeDescriptionPairList TransactionTypeList => GetTransactionTypeList(Ledger);

		protected CodeDescriptionPairList GetTransactionTypeList(string ledger)
		{
			var resultPairList = new CodeDescriptionPairList();
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
				case LedgerTypes.AccountsReceivable:
					resultPairList = new CodeDescriptionPairList(OLookUpEditType.ARAPTransactionTypes);
					resultPairList.RemoveCode("ALL");
					break;
				case LedgerTypes.CashBook:
					resultPairList = new CodeDescriptionPairList(OLookUpEditType.CashBookTransactionTypes);
					resultPairList.RemoveCode("ALL");
					resultPairList.RemoveCode(TransactionTypes.Payment);
					resultPairList.RemoveCode(TransactionTypes.Receipt);
					break;
				case LedgerTypes.General:
					resultPairList = new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes);
					break;
				case LedgerTypes.JobCosting:
					resultPairList = new CodeDescriptionPairList(OLookUpEditType.JobCostingTransactionTypes);
					break;
				default:
					break;
			}

			return resultPairList;
		}

		#endregion

		void AutoAddPairPrefix()
		{
			if (ParentCollection != null && !IsAutoAdded && !ParentCollection.Cast<TransactionTypePrefix>().Any(x => x.IsAutoAdded) && IsARAPContraPrefix)
			{
				var newPrefix = ParentCollection.AddNew();
				newPrefix.IsAutoAdded = true;
				newPrefix.Ledger = Ledger == LedgerTypes.AccountsPayable ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
				newPrefix.TransactionType = TransactionTypes.Contra;
				newPrefix.Prefix = Prefix;
			}
		}

		void AutoDeletePairPrefix()
		{
			if (ParentCollection != null && !TransactionTypeInfo.HasErrors() && !LedgerInfo.HasErrors())
			{
				if (AutoAddedPrefix != null)
				{
					ParentCollection.RemoveAndDelete(AutoAddedPrefix);
				}
			}
		}

		TransactionTypePrefix AutoAddedPrefix => ParentCollection?.Cast<TransactionTypePrefix>().FirstOrDefault(x => x.IsAutoAdded && x.ledger != Ledger);

		bool IsARAPContraPrefix => TransactionType == TransactionTypes.Contra && (Ledger == LedgerTypes.AccountsPayable || Ledger == LedgerTypes.AccountsReceivable);

		#endregion

		#region Validation

		void ValidateLedger()
		{
			LedgerInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LedgerInfo);
			ListValidation.ErrorIfInvalidCode(LedgerInfo);

			if (!LedgerInfo.HasErrors())
			{
				var errorMessage = CheckUnique();
				if (!errorMessage.IsEmpty)
				{
					LedgerInfo.AddError(errorMessage);
				}
			}
		}

		void ValidateTransactionType()
		{
			TransactionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TransactionTypeInfo);
			ListValidation.ErrorIfInvalidCode(TransactionTypeInfo);

			if (!TransactionTypeInfo.HasErrors())
			{
				var errorMessage = CheckUnique();
				if (!errorMessage.IsEmpty)
				{
					TransactionTypeInfo.AddError(errorMessage);
				}
			}

			if (!TransactionTypeInfo.HasErrors() && ParentCollection != null)
			{
				if (Ledger == LedgerTypes.General && GeneralTransactionTypeGroup.Contains(TransactionType))
				{
					if (GeneralTransactionTypeGroup.Any(x => !ParentCollection.Cast<TransactionTypePrefix>().Where(y => y.Ledger == Ledger).Select(z => z.TransactionType).Contains(x)))
					{
						TransactionTypeInfo.AddError(Res.GetString("3dfd9c1d-ee3e-4a21-8526-7aac94501917", "For GJL, RJL, AJL and NJL, you must specify a prefix for these transaction types when a prefix is specified for one of them."));
					}
				}

				if ((Ledger == LedgerTypes.AccountsPayable || Ledger == LedgerTypes.AccountsReceivable) &&
					ARAPShareNumberTransactionTypeGroup.Contains(TransactionType))
				{
					if (ARAPShareNumberTransactionTypeGroup.Any(x => !ParentCollection.Cast<TransactionTypePrefix>().Where(y => y.Ledger == Ledger).Select(z => z.TransactionType).Contains(x)))
					{
						TransactionTypeInfo.AddError(Res.GetString("e81ac6ea-daf0-49c7-8c8d-1aacca7b882f", "For INV, CRD and ADJ, you must specify a prefix for these transaction types when a prefix is specified for one of them. The system will enforce this rule for AR and AP ledger separately."));
					}
				}
			}
		}

		IEnumerable<ZString> GeneralTransactionTypeGroup => new List<ZString> { TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLNoteJournal, TransactionTypes.GLStandardJournal };

		IEnumerable<ZString> ARAPShareNumberTransactionTypeGroup => new List<ZString> { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote };

		void ValidatePrefix()
		{
			PrefixInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PrefixInfo);
		}

		ZString CheckUnique()
		{
			ZString result = ZString.Empty;
			if (!Ledger.IsEmpty && !TransactionType.IsEmpty && ParentCollection != null)
			{
				if (ParentCollection.Cast<TransactionTypePrefix>().Any(x => x.PK != PK && x.Ledger == Ledger && x.TransactionType == TransactionType))
				{
					result = Res.GetString("23259d84-5adf-48b3-bb78-fa5c8a3cedd0", "Ledger and Transaction Type must be unique.");
				}
			}
			return result;
		}

		void ValidateSamePrefixForCTR()
		{
			ClearRowNotifications();
			if (IsARAPContraPrefix && !IsAutoAdded && ParentCollection != null)
			{
				if (AutoAddedPrefix == null || AutoAddedPrefix.Prefix != Prefix)
				{
					AddRowError(Res.GetString("9e0e3696-fc6f-4a40-b77e-fb7ea7373b96", "Prefix of AR/AP Contra must be same."));
				}
			}
		}

		#endregion
	}
}
