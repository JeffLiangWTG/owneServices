using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JournalEntriesClassificationGroup : RegistryBusinessObjectTemplate
	{
		public JournalEntriesClassificationGroup()
		{
		}

		public JournalEntriesClassificationGroup(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public static class Schema
		{
			public const string Ledger = "Ledger";
			public const string TransactionType = "TransactionType";
			public const string GroupCode = "GroupCode";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateGroupCode();
		}

		#region Properties

		#region Ledger

		ZString fLedger;

		[MaxLength(2)]
		public ZString Ledger
		{
			get { return fLedger; }
			set
			{
				SetNonPersistentPropertyValue(LedgerInfo, ref fLedger, value);
			}
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(Schema.Ledger); }
		}

		protected bool Ledger_ReadOnly => true;

		#endregion

		#region TransactionType

		ZString fTransactionType;

		[MaxLength(3)]
		public ZString TransactionType
		{
			get { return fTransactionType; }
			set
			{
				SetNonPersistentPropertyValue(TransactionTypeInfo, ref fTransactionType, value);
			}
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionType); }
		}

		protected bool TransactionType_ReadOnly => true;

		#endregion

		#region GroupCode

		ZString fGroupCode;

		[List("GroupCodeList")]
		public ZString GroupCode
		{
			get
			{
				return fGroupCode;
			}
			set
			{
				SetNonPersistentPropertyValue(GroupCodeInfo, ref fGroupCode, value);

				if (!IsValidationSuspended)
				{
					ValidateGroupCode();
				}

				if (Ledger == LedgerTypeCodes.AccountsReceivable && TransactionType == TransactionTypes.Contra)
				{
					var aPCTR = ParentCollection?.Cast<JournalEntriesClassificationGroup>().FirstOrDefault(x => x.Ledger == LedgerTypeCodes.AccountsPayable && x.TransactionType == TransactionTypes.Contra);
					if (aPCTR != null)
					{
						SetNonPersistentPropertyValue(aPCTR.GroupCodeInfo, ref fGroupCode, value);

						aPCTR.GroupCode = value;
					}
				}
			}
		}

		public ZPropertyInfo GroupCodeInfo
		{
			get { return GetZPropertyInfo(Schema.GroupCode); }
		}

		protected bool GroupCode_ReadOnly => this.Ledger == LedgerTypeCodes.AccountsPayable && this.TransactionType == TransactionTypes.Contra ;

		void ValidateGroupCode()
		{
			GroupCodeInfo.ClearAllNotifications();
			if (!string.IsNullOrEmpty(GroupCode) && !GroupCodeList.Cast<JournalEntriesClassificationGroupCode>().Any(x => x.Code == GroupCode))
			{
				GroupCodeInfo.AddError(ResString.GetMultilingualString("31B2057D-EC6A-4AFE-96BF-1A3260E78922", "Please enter a valid Group Code."));
			}

			if (!ParentCollection.Cast<JournalEntriesClassificationGroup>().AllSame(x => string.IsNullOrEmpty(x.GroupCode)))
			{
				GroupCodeInfo.AddError(ResString.GetMultilingualString("C13356CE-A173-4B5C-AE68-FF5B5D6B42AA", "Group Code must be all filled or all not filled."));
			}
		}

		#endregion

		#region GroupCodeDescription

		public ZString GroupCodeDescription
		{
			get
			{
				return GroupCodeList.Cast<JournalEntriesClassificationGroupCode>().FirstOrDefault(x => x.Code == GroupCode)?.Description ?? string.Empty;
			}
		}

		#endregion

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(JournalEntriesClassificationGroupCollection)) ?? new JournalEntriesClassificationGroupCollection(CurrentFallbackLevel); }
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JournalEntriesClassificationGroup(fallbackLevel);
		}

		#region List

		public JournalEntriesClassificationGroupCodeCollection GroupCodeList
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.JournalEntriesClassificationGroupCode.GetFallBackValueAtAllLevels(CurrentFallbackLevel?.CompanyPK(true) ?? Guid.Empty, Guid.Empty, Guid.Empty);
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Ledger, Ledger);
			writer.WriteElementString(Schema.TransactionType, TransactionType);
			writer.WriteElementString(Schema.GroupCode, GroupCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Ledger = reader.ReadElementString(Schema.Ledger);
			TransactionType = reader.ReadElementString(Schema.TransactionType);
			GroupCode = reader.ReadElementString(Schema.GroupCode);
		}

		#endregion
	}
}
