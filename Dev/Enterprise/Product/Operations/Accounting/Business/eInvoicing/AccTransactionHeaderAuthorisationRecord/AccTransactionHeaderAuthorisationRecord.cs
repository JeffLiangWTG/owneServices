using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using AuthRecordConstants = Enterprise.Accounting.Integration.DataTransferConstants.AccTransactionHeaderAuthorisationRecord;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class AccTransactionHeaderAuthorisationRecord : AutoAccTransactionHeaderAuthorisationRecord
	{
		public AccTransactionHeaderAuthorisationRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected T LoadParent<T>()
			where T : EnterpriseBusinessObject
		{
			var parent = Factory.Load<T>(AHF_ParentTableCode, AHF_ParentId);
			return parent;
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AHF_RecordType = "INI";
			AHF_IDType = "GVT";
			AHF_Counter = "ZXY987";
			AHF_Number = "ABC123";
			AHF_ParentTableCode = "AH";
		}

		public IUniqueIndexFailureHandler UniqueIndexFailureHandler_ForTestOnly => UniqueIndexFailureHandlers.Single();
#endif

		public bool TrySetNumberOnce(ZString value)
		{
			if (AHF_Number.IsEmpty || AHF_Number == AuthRecordConstants.NullPlaceholderForNVarchar || AHF_Number == value)
			{
				AHF_Number = value;
				return true;
			}
			return false;
		}

		public bool TrySetCounterOnce(ZString value)
		{
			if (AHF_Counter.IsEmpty || AHF_Counter == AuthRecordConstants.NullPlaceholderForNVarchar || AHF_Counter == value)
			{
				AHF_Counter = value;
				return true;
			}
			return false;
		}

		public bool TrySetIDTypeOnce(ZString value)
		{
			if (AHF_IDType.IsEmpty || AHF_IDType == AuthRecordConstants.NullPlaceholderForChar3 || AHF_IDType == value)
			{
				AHF_IDType = value;
				return true;
			}
			return false;
		}

		public bool TrySetIDNumberOnce(ZString value)
		{
			if (AHF_IDNumber.IsEmpty || AHF_IDNumber == value)
			{
				AHF_IDNumber = value;
				return true;
			}
			return false;
		}

		public bool TrySetDateTimeOnce(ZDateTimeOffset value)
		{
			if (AHF_DateTime.IsEmpty || AHF_DateTime == AuthRecordConstants.NullPlaceholderForDateTime || AHF_DateTime == value)
			{
				AHF_DateTime = value;
				return true;
			}
			return false;
		}

		public bool TrySetVerificationUrlOnce(ZString value)
		{
			if (AHF_VerificationUrl.IsEmpty || AHF_VerificationUrl == value)
			{
				AHF_VerificationUrl = value;
				return true;
			}
			return false;
		}

		public bool TrySetPublicKeyOnce(ZBlob value)
		{
			if (AHF_PublicKey.IsEmpty || AHF_PublicKey == value)
			{
				AHF_PublicKey = value;
				return true;
			}
			return false;
		}

		public bool TrySetAuthorisationDataOnce(ZBlob value)
		{
			if (AHF_AuthorisationData.IsEmpty || AHF_AuthorisationData == value)
			{
				AHF_AuthorisationData = value;
				return true;
			}
			return false;
		}

		public bool TrySetITransactionHashOnce(ZBlob value)
		{
			if (AHF_ITransactionHash.IsEmpty || AHF_ITransactionHash == value)
			{
				AHF_ITransactionHash = value;
				return true;
			}
			return false;
		}

		public bool TrySetIssuerCertificateIdentifierOnce(ZString value)
		{
			if (AHF_IssuerCertificateIdentifier.IsEmpty || AHF_IssuerCertificateIdentifier == value)
			{
				AHF_IssuerCertificateIdentifier = value;
				return true;
			}
			return false;
		}

		public bool TrySetIssuerAuthorizationDataOnce(ZBlob value)
		{
			if (AHF_IssuerAuthorizationData.IsEmpty || AHF_IssuerAuthorizationData == value)
			{
				AHF_IssuerAuthorizationData = value;
				return true;
			}
			return false;
		}

		public bool TrySetDebtorNumberOnce(ZString value)
		{
			if (AHF_DebtorNumber.IsEmpty || AHF_DebtorNumber == value)
			{
				AHF_DebtorNumber = value;
				return true;
			}
			return false;
		}

		public bool TrySetPlaceOfIssueOnce(ZString value)
		{
			if (AHF_PlaceOfIssue.IsEmpty || AHF_PlaceOfIssue == value)
			{
				AHF_PlaceOfIssue = value;
				return true;
			}
			return false;
		}

		public bool IsAHF_NumberNullOrEmpty()
		{
			return AHF_Number.IsEmpty || ZString.Equals(AHF_Number, AuthRecordConstants.NullPlaceholderForNVarchar);
		}

		public bool IsAHF_DateTimeNullOrEmpty() => AHF_DateTime.IsEmpty || ZDateTimeOffset.Equals(AHF_DateTime, AuthRecordConstants.NullPlaceholderForDateTime);

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return new AccTransactionHeaderAuthorisationRecordUniqueIndexFailureHandler(this); }
		}

		public class AccTransactionHeaderAuthorisationRecordUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public AccTransactionHeaderAuthorisationRecordUniqueIndexFailureHandler(AccTransactionHeaderAuthorisationRecord authorisationRecord)
			{
				this.authorisationRecord = authorisationRecord;
			}

			readonly AccTransactionHeaderAuthorisationRecord authorisationRecord;

			#region IUniqueIndexFailureHandler Members

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return AccTransactionHeaderAuthorisationRecordSchema.Constants.Indexes.NR_UC__AHF_ParentId_AHF_ParentTableCode_AHF_RecordType; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				var authorisationRecordInDatabase = LoadExistingAuthorizationRecord();
				if (authorisationRecordInDatabase != null)
				{
					notifier?.ReportError(GetNotificationMessage(authorisationRecordInDatabase), Res.GetString("e14f5ad5-f3dc-4803-8833-cc17991636c6", "Authorization record already exists"));
				}
			}

			string GetNotificationMessage(AccTransactionHeaderAuthorisationRecord authorisationRecordInDatabase)
			{
				var messageBuilder = new ZStringBuilder(Res.GetString("33a52039-2b54-4de2-81f2-20c320d50b42", @"An authorization record for this transaction already exists in database.
Property values of the existing record:"));

				AddPropertyValuesToMessage(authorisationRecordInDatabase);

				messageBuilder.AppendLine();
				messageBuilder.Append(Res.GetString("d86e42a2-9b23-4bb9-9a02-71e8e437376f", "Property values of the record to be saved:"));

				AddPropertyValuesToMessage(authorisationRecord);

				return messageBuilder.ToStringWithNewLineBetweenAppends();

				void AddPropertyValuesToMessage(AccTransactionHeaderAuthorisationRecord record)
				{
					foreach (ZPropertyInfo propertyInfo in record.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.All))
					{
						messageBuilder.Append(FormattableString.Invariant($"{propertyInfo.HumanReadableName} : {propertyInfo.Value}"));
					}
				}
			}

			AccTransactionHeaderAuthorisationRecord LoadExistingAuthorizationRecord()
			{
				var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, authorisationRecord.AHF_ParentId);
				query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, authorisationRecord.AHF_ParentTableCode);
				query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, authorisationRecord.AHF_RecordType);
				query.FetchOnlyFromLocalCache = false;
				query.ReLoadExistingRows = true;
				var authorisationRecordInDatabase = authorisationRecord.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(query);
				return authorisationRecordInDatabase;
			}

			#endregion
		}
	}
}
