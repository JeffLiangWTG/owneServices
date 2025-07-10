using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	[DebuggerDisplay("{CE_RN_NKCountryCode} {CE_EntryType} {CE_EntryNum}")]
	[UniversalCopyWithExtendedEntities]
	public class CusEntryNumber : AutoCusEntryNum, Integration.Customs.ICusEntryNumber, ISearcheableCusEntryNumber
	{
		public new class Schema : AutoCusEntryNum.Schema
		{
			public const string AdditionalReferenceNumberTypeDescription = "AdditionalReferenceNumberTypeDescription";
		}

		protected override CusEntryNumValidation GetNewValidation()
		{
			if (Parent != null)
			{
				var parentAsTypeHelper = Parent as ICusEntryNumberValidationDeciderOfType;
				if (parentAsTypeHelper != null)
				{
					var t = parentAsTypeHelper.GetCusEntryNumberValidationType();
					if (t != null && t.IsSubclassOf(typeof(CusEntryNumValidation)) && !t.IsAbstract)
					{
						return (CusEntryNumValidation)Activator.CreateInstance(t, new[] { this });
					}
				}
			}
			return new CusEntryNumValidation(this);
		}

		#region Entry Types

		public static class EntryType
		{
			public const string ImportManifestStatus = "IMS";
			public const string ImpendingArrivalStatus = "IAS";
			public const string ActualArrivalStatus = "AAS";
			public const string CargoListStatus = "CLS";
			public const string CargoReportStatus = "CRS";
			public const string ImpendingArrivalResponseStatus = "IAR";
			public const string ActualArrivalResponseStatus = "AAR";
			public const string UnderbondStatus = "UBM";
			public const string OutturnStatus = "OUT";
			public const string DepartureReportStatus = "DEP";
			public const string MainManifestStatus = "MMS";
			public const string CTORECStatus = "REC";
			public const string CTOREMStatus = "REM";
			public const string RequestForPermitStatus = "RFS";
			public const string CertificateRequestID = "CRI";
			public const string ExdocPermitNumber = "EPN";
			public const string CATransactionNumber = "REL";
			public const string InBond = "INB";
			public const string InspectionStatus = "INS";
			public const string AdditionalInspectionStatus = "AIN";
			public const string CAURNNumber = "URN";
			public const string CustomsNumber = "CEN";
			public const string CustomsReleaseNumber = "CRN";
			public const string PortAuthorityNumber = "PAN";
			public const string ExportReceiveConsignmentNumber = "ERC";
		}

		#endregion

		#region Categories

		public static class Categories
		{
			public const string CustomsPermitClearanceNumber = "CUS";
			public const string PortAuthorityReferenceNumber = "PRT";
			public const string AdditionalReferenceNumber = "OTH";
			public const string PortReferenceNumber = "PRT";
			public const string InspectionStatus = "INS";
			public const string DocumentsRelated = "DRE";
			public const string DispatchInstructionDocument = "DID";
		}

		#endregion

		#region Construction / Loading

		public CusEntryNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CusEntryNumber New(BusinessObject businessObject, ZString entryType, ZString countryCode)
		{
			return New<CusEntryNumber>(businessObject, entryType, countryCode);
		}

		public static T New<T>(BusinessObject businessObject, ZString entryType, ZString countryCode, ZString entryLineReference)
			where T : CusEntryNumber
		{
			var result = New<T>(businessObject, entryType, countryCode);
			result.CE_EntryLineReference = entryLineReference;
			return result;
		}

		public static T New<T>(BusinessObject businessObject, ZString entryType, ZString countryCode, ZString entryNumber, bool isSystemGenerated)
			where T : CusEntryNumber
		{
			var result = New<T>(businessObject, entryType, countryCode);
			result.CE_EntryIsSystemGenerated = isSystemGenerated;
			result.CE_EntryNum = entryNumber;
			return result;
		}

		public static T New<T>(BusinessObject businessObject, ZString entryType, ZString countryCode)
			where T : CusEntryNumber
		{
			var result = (T)businessObject.Factory.New(typeof(T));
			using (result.SuspendSettingHasChanges())
			{
				result.CE_EntryType = entryType;
				result.Parent = businessObject;
				result.CE_RN_NKCountryCode = countryCode;
			}
			return result;
		}

		public static CusEntryNumber[] Load(BusinessObject businessObject)
		{
			return Load<CusEntryNumber>(businessObject);
		}

		public static T[] Load<T>(BusinessObject businessObject)
			where T : CusEntryNumber
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, businessObject.PK);
			filter.FetchOnlyFromLocalCache = !businessObject.IsInDatabase;

			return businessObject.Factory.Load<T>(filter);
		}

		public static CusEntryNumber Load(BusinessObject parent, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
		{
			return Load<CusEntryNumber>(parent, entryType, countryCode, reLoadExistingRows);
		}

		public static T Load<T>(BusinessObject parent, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, parent.PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			filter.ReLoadExistingRows = reLoadExistingRows;
			filter.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			var result = parent.Factory.LoadTop1<T>(filter);
			if (result != null)
			{
				result.Parent = parent;
			}

			return result;
		}

		public static T Load<T>(BusinessObject parent, ZString entryType, ZString countryCode, ZString entryLineReference, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, parent.PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryLineReference, SQLComparisonOperator.Equal, entryLineReference);
			filter.ReLoadExistingRows = reLoadExistingRows;

			return parent.Factory.LoadTop1<T>(filter);
		}

		public static CusEntryNumber[] Load(BusinessObjectFactory factory, ZString entryType, ZString entryNumber, ZString countryCode, bool reLoadExistingRows = false)
		{
			return Load<CusEntryNumber>(factory, entryType, entryNumber, countryCode, reLoadExistingRows);
		}

		public static CusEntryNumber LoadMostRecentByCreateTime(BusinessObjectFactory factory, ZString entryType, ZString entryNumber, ZString countryCode, bool reLoadExistingRows = false)
		{
			return LoadMostRecentByCreateTime<CusEntryNumber>(factory, entryType, entryNumber, countryCode, reLoadExistingRows);
		}

		public static T[] Load<T>(BusinessObjectFactory factory, ZString entryType, ZString entryNumber, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, entryNumber);
			filter.ReLoadExistingRows = reLoadExistingRows;

			return factory.Load<T>(filter);
		}

		public static T[] Load<T>(BusinessObjectFactory factory, ZString entryType, ZString[] entryNumbers, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_EntryNum, entryNumbers);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.ReLoadExistingRows = reLoadExistingRows;

			return factory.Load<T>(filter);
		}

		public static T LoadMostRecentByCreateTime<T>(BusinessObjectFactory factory, ZString entryType, ZString entryNumber, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			ZQuery filter = new ZQuery(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, entryType);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.Equal, entryNumber);
			filter.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name + OrderByClause.Descending;
			filter.ReLoadExistingRows = reLoadExistingRows;

			return factory.LoadTop1<T>(filter);
		}

		public static CusEntryNumber LoadOrCreate(BusinessObject businessObject, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
		{
			return LoadOrCreate<CusEntryNumber>(businessObject, entryType, countryCode, reLoadExistingRows);
		}

		public static T LoadOrCreate<T>(BusinessObject businessObject, ZString entryType, ZString countryCode, ZString entryLineReference, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			return Load<T>(businessObject, entryType, countryCode, entryLineReference, reLoadExistingRows) ?? New<T>(businessObject, entryType, countryCode, entryLineReference);
		}

		public static T LoadOrCreate<T>(BusinessObject businessObject, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			return Load<T>(businessObject, entryType, countryCode, reLoadExistingRows) ?? New<T>(businessObject, entryType, countryCode);
		}

		public static CusEntryNumber LoadOrCreate(BusinessObject businessObject, ZString countryCode, bool reLoadExistingRows = false)
		{
			return LoadOrCreate<CusEntryNumber>(businessObject, countryCode, reLoadExistingRows);
		}

		public static T LoadOrCreate<T>(BusinessObject businessObject, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			return Load<T>(businessObject, countryCode, reLoadExistingRows) ?? New<T>(businessObject, "", countryCode);
		}

		public static CusEntryNumber Load(BusinessObject parent, ZString countryCode, bool reLoadExistingRows = false)
		{
			return Load<CusEntryNumber>(parent, countryCode, reLoadExistingRows);
		}

		public static T Load<T>(BusinessObject parent, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, parent.PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, countryCode);
			filter.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			filter.ReLoadExistingRows = reLoadExistingRows;
			filter.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;
			var result = parent.Factory.LoadTop1<T>(filter);
			if (result != null)
			{
				result.Parent = parent;
			}
			return result;
		}

		public BusinessObject Parent
		{
			get
			{
				if (parent == null && !CE_ParentTable.IsEmpty && CE_ParentID.IsValid)
				{
					ZString tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CE_ParentTable);
					if (!tablePrefix.IsEmpty)
					{
						parent = Factory.Load(tablePrefix, CE_ParentID);
					}
				}
				return parent;
			}
			set
			{
				if (parent != value)
				{
					isSettingParent = true;
					try
					{
						parent = value;

						if (parent != null)
						{
							if (CE_ParentID != parent.PK)
							{
								CE_ParentID = parent.PK;
							}

							if (CE_ParentTable != parent.TableName)
							{
								CE_ParentTable = parent.TableName;
							}
						}
						else
						{
							CE_ParentID = ZGuid.Empty;
							CE_ParentTable = ZString.Empty;
						}
					}

					finally
					{
						isSettingParent = false;
					}

					if (parent is IPackLineSynchroniseProvider provider)
					{
						PackLineSynchroniser = provider.PackLineSynchronise;
					}
				}
			}
		}
		BusinessObject parent;

		bool isSettingParent;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		#endregion

		#region Saving

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void OnSaving()
		{
			base.OnSaving();

			ReportIfError();

			if (EntryNumberShouldBeDeletedIfEmpty && CE_EntryNum.IsEmpty)
			{
				Delete();
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				if (!IsInDatabase && CE_EntryIsSystemGenerated && ShouldDeleteIfFailedToCommitToDB)
				{
					Delete();
				}
			}
		}

		/// <summary>
		/// For these entry types, entry numbers are generated from number fountain.
		/// If save fails, number fountain rolls back. To synchronise with the action, this number should not be saved to db in the next attempt of saving.
		/// </summary>
		bool ShouldDeleteIfFailedToCommitToDB
		{
			get
			{
				bool result = false;

				if (CE_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates)
				{
					result = CE_EntryType == CusEntryNumberTypes.UnitedStates.EntrySummary ||
						CE_EntryType == CusEntryNumberTypes.UnitedStates.InBond ||
						CE_EntryType == CusEntryNumberTypes.UnitedStates.FTZ;
				}

				return result;
			}
		}

		bool EntryNumberShouldBeDeletedIfEmpty
		{
			get
			{
				foreach (string numType in EntryNumberTypesThatShouldBeDeletedIfEmpty)
				{
					if (CE_EntryType == numType)
					{
						return true;
					}
				}
				return false;
			}
		}

		string[] EntryNumberTypesThatShouldBeDeletedIfEmpty
		{
			get
			{
				return new string[]
				{
					CusEntryNumberTypes.Australia.CAN,
					CusEntryNumberTypes.Australia.CRN,
					CusEntryNumberTypes.Australia.ECN,
					CusEntryNumberTypes.Australia.MMN,
					CusEntryNumberTypes.Standard.MovementReferenceNumber,
					CusEntryNumberTypes.EU.MasterUCR,
					CusEntryNumberTypes.EU.PRE,
					CusEntryNumberTypes.EU.LocalReferenceNumber,
					CusEntryNumberTypes.UnitedStates.FTZ,
					CusEntryNumberTypes.China.BillOfLading,
					CusEntryNumberTypes.China.PreEntryNumber,
					CusEntryNumberTypes.China.DeclarationUnifiedNumber,
					CusEntryNumberTypes.China.CIQNumber,
					CusEntryNumberTypes.Brazil.EAK,
				};
			}
		}

		#endregion

		#region Properties

		public override ZString CE_ParentTable
		{
			get { return base.CE_ParentTable; }
			set
			{
				var originalValue = base.CE_ParentTable;
				base.CE_ParentTable = value;
				if (originalValue != base.CE_ParentTable)
				{
					LogStackTraceForShipmentIfNeed(AutoCusEntryNum.Schema.CE_ParentTable);
					if (!IsCopying && !isSettingParent)
					{
						parent = null;
						if (Parent is IPackLineSynchroniseProvider provider)
						{
							PackLineSynchroniser = provider.PackLineSynchronise;
						}
					}
				}
			}
		}

		[UniversalCopyAlwaysCopyProperty(UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning)]
		public override ZBool CE_EntryIsSystemGenerated
		{
			get { return base.CE_EntryIsSystemGenerated; }
			set
			{
				if (base.CE_EntryIsSystemGenerated != value)
				{
					base.CE_EntryIsSystemGenerated = value;
					LogStackTraceForShipmentIfNeed(AutoCusEntryNum.Schema.CE_EntryIsSystemGenerated);
				}
			}
		}

		public override ZString CE_Category
		{
			get { return base.CE_Category; }
			set
			{
				if (base.CE_Category != value)
				{
					base.CE_Category = value;
					LogStackTraceForShipmentIfNeed(AutoCusEntryNum.Schema.CE_Category);
				}
			}
		}

		public override bool ReadOnly
		{
			get
			{
				var dataImportSupport = parent as ISupportDataImporting;
				var customsReferenceNumberType = Lookups.AdditionalReferenceNumberTypes?.OfType<ICustomsNumberTypeCodeDescription>().FirstOrDefault(codeDescription => codeDescription.Code.Equals(CE_EntryType, StringComparison.OrdinalIgnoreCase));
				return base.ReadOnly || ((customsReferenceNumberType?.IsAutomation ?? false) && IsInDatabase && !(dataImportSupport?.IsImportingData ?? false));
			}
		}

		public override ZString CE_RN_NKCountryCode
		{
			get => base.CE_RN_NKCountryCode;
			set
			{
				base.CE_RN_NKCountryCode = value;

				if (!IsValidationSuspended && value == Constants.CountryCodes.Egypt)
				{
					Validation.ValidateCE_EntryNum();
				}
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("21674C2D-3A50-4AD5-8A50-D975DF0BD360", "Number {0}", CE_EntryType);

		public ZString AdditionalReferenceNumberTypeDescription
		{
			get { return Lookups.AdditionalReferenceNumberTypes.GetDescriptionFromCode(CE_EntryType); }
		}

		public ZPropertyInfo AdditionalReferenceNumberTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.AdditionalReferenceNumberTypeDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryNumLookups.AdditionalReferenceNumberTypes))]
		public override ZString CE_EntryType
		{
			get { return base.CE_EntryType; }
			set
			{
				base.CE_EntryType = value;
				if (PackLineSynchroniser != null)
				{
					PackLineSynchroniser.MarkSyncDirty();
				}

				if (MarkParentAsNeedingValidation)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime CE_IssueDate
		{
			get => base.CE_IssueDate;
			set
			{
				base.CE_IssueDate = value;
				if (MarkParentAsNeedingValidation)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZDateTime CE_ExpiryDate
		{
			get => base.CE_ExpiryDate;
			set
			{
				base.CE_ExpiryDate = value;
				if (MarkParentAsNeedingValidation)
				{
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public bool CE_EntryType_ReadOnly { get; set; }

		public bool CE_EntryNum_ReadOnly { get; set; }

		public override ZString CE_EntryNum
		{
			get { return base.CE_EntryNum; }
			set
			{
				var oldValue = CE_EntryNum;
				base.CE_EntryNum = value.Left(base.CE_EntryNumInfo.MaxLength);
				if (PackLineSynchroniser != null)
				{
					PackLineSynchroniser.MarkSyncDirty();
				}

				var entryNumberParent = Parent as ICusEntryNumberParent;
				var binding = Parent as IAdditionalReferenceNumberSupporter;
				if (binding != null)
				{ binding.OnEntryNumChanged(this); }
				if (oldValue != CE_EntryNum && FilledPlaceHolderMessages.Any(x => !x.IsDeleted)
					&& (entryNumberParent == null || !entryNumberParent.CanBeChangedOrDeleted(this, out string errMsg)))
				{
					ErrorReporter.ReportOnce(string.Format("{0} was used to fill placeholder in a message and now is changed to {1}", oldValue, CE_EntryNum));
				}

				entryNumberParent?.EntryNumberChanged(oldValue, CE_EntryNum);
			}
		}

		public override ZString CE_EntryStatus
		{
			get { return base.CE_EntryStatus; }
			set
			{
				var oldValue = CE_EntryStatus;
				base.CE_EntryStatus = value;
				if (oldValue != CE_EntryStatus && !IsCopying && !IsRefreshingByDataRefreshBus)
				{
					OnStatusChanged(oldValue, CE_EntryStatus);
					var parentAsStatusListProvider = Parent as ICusEntryNumEntryStatusListProvider;
					if (parentAsStatusListProvider != null)
					{
						parentAsStatusListProvider.OnEntryStatusSet(CE_EntryType, CE_EntryStatus);
					}
					if (MarkParentAsNeedingValidation)
					{
						Parent?.MarkAsNeedingValidation();
					}
				}
			}
		}

		public IPackLineSynchronise PackLineSynchroniser;

		public bool MarkParentAsNeedingValidation;
		public ZString EntryStatusDescription
		{
			get
			{
				var result = ZString.Empty;

				var parentAsStatusListProvider = Parent as ICusEntryNumEntryStatusListProvider;
				if (parentAsStatusListProvider != null)
				{
					result = parentAsStatusListProvider.EntryStatusList.GetDescriptionFromCode(CE_EntryStatus);
				}
				return result;
			}
		}

		public override ZGuid CE_ParentID
		{
			get => base.CE_ParentID;
			set
			{
				var originalValue = base.CE_ParentID;
				base.CE_ParentID = value;
				if (originalValue != base.CE_ParentID && !IsCopying && !isSettingParent)
				{
					parent = null;
				}
			}
		}

		#endregion

		#region Creating System Generated CUS Number For Shipment

		bool IsShipmentAttachedSystemGeneratedCUSNumber
		{
			get
			{
				return CE_ParentTable == JobShipmentSchema.Constants.TableName
				  && CE_EntryIsSystemGenerated
				  && CE_Category == Categories.CustomsPermitClearanceNumber;
			}
		}

		void LogStackTraceForShipmentIfNeed(string key)
		{
			if (IsShipmentAttachedSystemGeneratedCUSNumber && ShouldLogStackTraceForCertainCountries())
			{
				dicForShipment = dicForShipment ?? new Dictionary<string, string>();

				if (dicForShipment.ContainsKey(key))
				{
					dicForShipment[key] = System.Environment.StackTrace;
				}
				else
				{
					dicForShipment.Add(key, System.Environment.StackTrace);
				}
			}
		}

		Dictionary<string, string> dicForShipment;

		static bool ShouldLogStackTraceForCertainCountries()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.Sweden
				|| GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.VietNam;
		}

		#endregion

		#region Entry Number Type Display

		public static ZString GetEntryNumberTypeForDisplay(ZString entryNumberType, ZString countryCode, ZBool isImport)
		{
			return countryCode == Constants.CountryCodes.Australia && !isImport
				? AU.CMR.CMRExportExemptionCodes.Get4CharCode(entryNumberType)
				: entryNumberType;
		}

		public static ZString GetEntryNumberTypeFromDisplay(ZString displayEntryNumberType, ZString countryCode, ZBool isImport)
		{
			var result = countryCode == Constants.CountryCodes.Australia && !isImport
				? AU.CMR.CMRExportExemptionCodes.Get3CharCode(displayEntryNumberType)
				: displayEntryNumberType;

			return result.SubstringSafe(0, CusEntryNumSchema.CE_EntryType.MaxLength);
		}

		#endregion

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			ReportIfError();
		}

		void ReportIfError()
		{
			if (IsDeleting || CE_EntryNumInfo.HasChanges || CE_EntryTypeInfo.HasChanges)
			{
				var errMsg = string.Empty;
				var entryNumberParent = Parent as ICusEntryNumberParent;
				if (entryNumberParent != null && !entryNumberParent.CanBeChangedOrDeleted(this, out errMsg))
				{
					if (string.IsNullOrEmpty(entryNumberParent.EntryNumberChangedCallStack))
					{
						throw new InvalidOperationException(errMsg);
					}
					var innerException = new InvalidOperationException("Call stack of entry number:" + entryNumberParent.EntryNumberChangedCallStack);
					throw new InvalidOperationException(errMsg, innerException);
				}
			}
		}

		string deletedStackTrace;

		public override void Delete()
		{
			deletedStackTrace = System.Environment.StackTrace;
			base.Delete();
		}

		protected override StringBuilder BuildRowDeletedReport(string columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, string message)
		{
			return base.BuildRowDeletedReport(columnName, ex, versionToUse, version, message)
				.AppendLine((NoResString)"Deletion StackTrace: ")
				.AppendLine(deletedStackTrace ?? (NoResString)"Deletion stack trace never collected.");
		}

		#region Logs

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString result = base.CustomLogReferenceSuffix;

				if (!IsInDatabase)
				{
					result += CE_EntryType + (string.IsNullOrEmpty(CE_EntryNum) ? "" : ": <" + CE_EntryNum + ">");
				}

				if (CE_EntryTypeInfo.HasChanges)
				{
					result += (NoResString)"Type: From <" + (ZString)CE_EntryTypeInfo.OriginalValue + (NoResString)"> to <" + CE_EntryType + (NoResString)">";
				}

				if (CE_EntryNumInfo.HasChanges)
				{
					if (!result.IsEmpty)
					{
						result += ". ";
					}

					result += CE_EntryType + (NoResString)": From <" + (ZString)CE_EntryNumInfo.OriginalValue + (NoResString)"> to <" + CE_EntryNum + (NoResString)">";
				}

				if (!result.IsEmpty && !AdditionalCustomLogReferenceSuffix.IsEmpty)
				{
					result += " " + AdditionalCustomLogReferenceSuffix;
				}

				AdditionalCustomLogReferenceSuffix = ZString.Empty;
				return result;
			}
		}

		public ZString AdditionalCustomLogReferenceSuffix { get; set; }

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				bool result = true;
				if (CanDeleteHandler != null)
				{
					CanDeleteCusEntryNumberEventArgs canDeleteEventArgs = new CanDeleteCusEntryNumberEventArgs();
					CanDeleteHandler(this, canDeleteEventArgs);
					result = canDeleteEventArgs.CanDelete;
					savedReasonForNotAbleToDelete = canDeleteEventArgs.ReasonForNotAbleToDelete;
				}

				return result;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return savedReasonForNotAbleToDelete; }
		}
		MultilingualString savedReasonForNotAbleToDelete;

		public event EventHandler<CanDeleteCusEntryNumberEventArgs> CanDeleteHandler;

		#endregion

		#region Status Changed

		void OnStatusChanged(ZString oldStatus, ZString newStatus)
		{
			if (StatusChanged != null)
			{
				StatusChanged(this, new StatusChangedEventArgs(oldStatus, newStatus));
			}
		}

		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return !CE_EntryIsSystemGenerated || CE_Category == Categories.AdditionalReferenceNumber;
		}

		#endregion

		#region ICusEntryNumber Members

		Integration.Customs.ICusEntryNumLookups Integration.Customs.ICusEntryNumber.Lookups
		{
			get { return Lookups; }
		}

		#endregion

		#region DataRefresh

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			CE_EntryNumInfo.RefreshBinding();
		}

		#endregion

		public void FillMessagePlaceHolder(EDIMessage message, ZString oldValue, ZString newValue)
		{
			var newText = message.EM_MessageText.Replace(oldValue, newValue);
			if (newText != message.EM_MessageText && !FilledPlaceHolderMessages.Contains(message))
			{
				FilledPlaceHolderMessages.Add(message);
			}
			message.EM_MessageText = newText;
		}

		List<EDIMessage> FilledPlaceHolderMessages
		{
			get { return fFilledPlaceHolderMessages ?? (fFilledPlaceHolderMessages = new List<EDIMessage>()); }
		}
		List<EDIMessage> fFilledPlaceHolderMessages;
	}

	#region Delete Event Args

	public class CanDeleteCusEntryNumberEventArgs : EventArgs
	{
		public bool CanDelete { get; set; }
		public MultilingualString ReasonForNotAbleToDelete { get; set; }
	}

	#endregion

	#region Status Changed Event Args

	public class StatusChangedEventArgs : EventArgs
	{
		public StatusChangedEventArgs(ZString oldStatus, ZString newStatus)
			: base()
		{
			this.OldStatus = oldStatus;
			this.NewStatus = newStatus;
		}

		public ZString OldStatus { get; private set; }
		public ZString NewStatus { get; private set; }
	}

	#endregion
}
