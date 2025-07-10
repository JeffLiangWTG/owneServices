using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.DataExport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.DataExport
{
	public class PositivePayDataExportAdapter
	{
		public PositivePayDataExportAdapter(BusinessObjectFactory factory, TransactionHeader[] transactionHeaders, AccBankAccount bankAccount)
		{
			Factory = factory;
			TransactionHeaders = transactionHeaders;
			BankAccount = bankAccount;
		}

		readonly BusinessObjectFactory Factory;
		readonly TransactionHeader[] TransactionHeaders;
		readonly AccBankAccount BankAccount;

		public IEnumerable<BusinessObject> GetBusinessObjectsForExport()
		{
			return GetBusinessObjectsForExport(TransactionHeaders);
		}

		public IEnumerable<BusinessObject> GetBusinessObjectsForExport(TransactionHeader[] transactionHeaders)
		{
			DataExportPositivePayHeader exportHeader = new DataExportPositivePayHeader(Factory, transactionHeaders, BankAccount);
			yield return exportHeader;

			int sequenceNumber = 1;
			foreach (TransactionHeader payment in transactionHeaders)
			{
				DataExportPayment exportPayment = new DataExportPaymentHeader(Factory, payment);

				TransactionMatchLink[] allMatchLinks = null;

				if (payment is Payment)
				{
					AccTransactionMatchLink paymentMatchLink = payment.Factory.LoadTop1<AccTransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, payment.PK));
					allMatchLinks = GetAllMatchlinksExcludingEXX((Payment)payment, paymentMatchLink);
					exportPayment.MatchLinks = allMatchLinks;
				}
				exportPayment.SequenceNumber = sequenceNumber;
				yield return exportPayment;

				if (payment is Payment)
				{
					List<TransactionHeader> transactionHeadersForMatchLinks = new List<TransactionHeader>();

					if (allMatchLinks != null)
					{
						foreach (var matchLink in allMatchLinks)
						{
							TransactionHeader transactionHeader = payment.Factory.Load<TransactionHeader>(matchLink.AP_AH);
							transactionHeader.SetMatchedAmount(matchLink, false);
							transactionHeadersForMatchLinks.Add(transactionHeader);
						}
					}

					transactionHeadersForMatchLinks.Sort((x, y) => { return CompareTransactionHeader(x, y); });

					foreach (TransactionHeader transactionHeader in transactionHeadersForMatchLinks)
					{
						DataExportPaidTransaction exportPaidTransaction = new DataExportPaidTransaction(Factory, transactionHeader, exportPayment);
						yield return exportPaidTransaction;
					}
				}
				exportPayment = new DataExportPaymentFooter(Factory, payment);
				exportPayment.SequenceNumber = sequenceNumber++;
				if (allMatchLinks != null)
				{
					exportPayment.MatchLinks = allMatchLinks;
				}
				yield return exportPayment;
			}
			DataExportPositivePayFooter exportFooter = new DataExportPositivePayFooter(Factory, transactionHeaders, BankAccount);
			yield return exportFooter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1135: DoNotUseCountrySpecificBusinessRule", Justification = "Testing")]
		bool IsCurrentCompanyUSAOrCanada
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.UnitedStates || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada;
			}
		}

		public IExportCollectionInfo GetMultiTypeCollectionInfo(IEnumerable<BusinessObject> businessObjects)
		{
			var info = new ExportCollectionInfoImpl(Factory, businessObjects);

			AddProperties<DataExportPositivePayHeader>(info, IsCurrentCompanyUSAOrCanada ? ResString.GetMultilingualString("9920b4ad-2488-4a2f-9979-823ab2522b3d", "Positive Pay Header") : ResString.GetMultilingualString("1e717f45-17d2-4a97-bc1b-e86d0c38e73d", "Check Payment Header"));
			AddProperties<DataExportPaymentHeader>(info, ResString.GetMultilingualString("cda7dce8-f61d-4506-ae96-cf9534cee4d6", "Payment Header"));
			AddProperties<DataExportPaidTransaction>(info, ResString.GetMultilingualString("5e442df3-d340-4971-8eed-74d0f7a4ea58", "Paid Transaction"));
			AddProperties<DataExportPaymentFooter>(info, ResString.GetMultilingualString("37db73ec-1b1c-46a8-90b8-3f6acde62fd3", "Payment Footer"));
			AddProperties<DataExportPositivePayFooter>(info, IsCurrentCompanyUSAOrCanada ? ResString.GetMultilingualString("f647f70d-eb5b-4de7-bf14-afd3640efb74", "Positive Pay Footer") : ResString.GetMultilingualString("1a356e94-e32c-4b22-9ce5-dbdf8f9b5733", "Check Payment Footer"));
			return info;
		}

		ExportWizard ExportWizard
		{
			get
			{
				if (exportWizard == null)
				{
					Sort(TransactionHeaders);
					IExportCollectionInfo collectionInfo = GetMultiTypeCollectionInfo(BusinessObjectsForExport);
					string contextPrefix = "DEW:"; // Context Prefix does not need to be localised.
					string contextKey = IsCurrentCompanyUSAOrCanada ? "PositivePay" : "CheckPayment";
					exportWizard = new ExportWizard(collectionInfo, new StmModuleFilterSettingsStorage(contextPrefix, contextKey), new FileMapper());
				}
				return exportWizard;
			}
		}

		ExportWizard exportWizard;

		IEnumerable<BusinessObject> BusinessObjectsForExport
		{
			get
			{
				if (businessObjectsForExport == null)
				{
					businessObjectsForExport = GetBusinessObjectsForExport(TransactionHeaders);
				}
				return businessObjectsForExport;
			}
		}

		IEnumerable<BusinessObject> businessObjectsForExport;

		StmData DataExportSetting
		{
			get
			{
				if (dataExportSetting == null)
				{
					ZQuery query = new ZQuery(StmDataSchema.SD_Owner, BankAccount.PK);
					query.AddToFilter(StmDataSchema.SD_Name, "PositivePayDataExportSetting");
					dataExportSetting = Factory.LoadTop1<StmData>(query);
				}
				return dataExportSetting;
			}
		}

		StmData dataExportSetting;

		public bool IsFileNameExpressionUsed
		{
			get
			{
				if (DataExportSetting == null)
				{
					return false;
				}
				ExportWizard.Setting = DataExportSetting.SD_BinaryValue.ToAscii();
				if (!string.IsNullOrEmpty(ExportWizard.FileNameExpression))
				{
					return true;
				}
				return false;
			}
		}

		public ZString LastExportedFullFileName
		{
			get
			{
				return ExportWizard.LastExportedFullFileName;
			}
		}

		public bool CreateFile(string unmappedFilePath, out string errorMessage)
		{
			bool result = false;

			if (DataExportSetting == null)
			{
				errorMessage = Res.GetString("d43bf38d-d85f-4ab7-b341-d141236ab009", "You must configure an export for this bank account.");
				return result;
			}

			ExportWizard.Setting = DataExportSetting.SD_BinaryValue.ToAscii();
			ExportWizard.FileName = unmappedFilePath;
			ExportWizard.FileNameExpressionObject = new DataExportPositivePayHeader(Factory, TransactionHeaders, BankAccount);
			result = ExportWizard.ExportCollection(BusinessObjectsForExport, out errorMessage);

			if (result)
			{
				ZInt xB_BatchNumber = ZInt.Zero;
				if (TransactionHeaders.Length > 0)
				{
					PositivePayBatchNumberAllocator allocator = new PositivePayBatchNumberAllocator();
					xB_BatchNumber = allocator.GetNewBatchNumber();

					int sequence = 1;
					BusinessObjectFactory factory = new BusinessObjectFactory();

					foreach (TransactionHeader transaction in TransactionHeaders)
					{
						GenExportBatchSequence batchSequence = factory.New<GenExportBatchSequence>();
						batchSequence.XB_BatchNumber = xB_BatchNumber;
						batchSequence.XB_Sequence = sequence++;
						batchSequence.XB_ParentID = transaction.PK;
						batchSequence.XB_ParentTableCode = transaction.TablePrefix;
						batchSequence.XB_Type = Core.Constants.DataExportBatchSubTypes.Codes.PositivePayFile;
					}
					factory.Save();
				}
			}

			return result;
		}

		public void Sort(TransactionHeader[] transactionHeaders)
		{
			Array.Sort(transactionHeaders, (x, y) => { return CompareTransactionHeader(x, y); });
		}

		int CompareTransactionHeader(TransactionHeader x, TransactionHeader y)
		{
			int value = x.AH_Ledger.CompareTo(y.AH_Ledger);

			if (value == 0)
			{
				value = x.AH_TransactionType.CompareTo(y.AH_TransactionType);
				if (value == 0)
				{
					return x.AH_TransactionNum.CompareTo(y.AH_TransactionNum);
				}
				return value;
			}
			return value;
		}

		internal static TransactionMatchLink[] GetAllMatchlinksExcludingEXX(Payment payment, AccTransactionMatchLink paymentMatchLink)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionMatchLink));
			if (paymentMatchLink != null)
			{
				ZString whereClause = AccTransactionMatchLinkSchema.PK.Name + " IN (Select " + AccTransactionMatchLinkSchema.PK.Name +
					" from " + AccTransactionMatchLinkSchema.Constants.SqlSchemaName + "." + AccTransactionMatchLinkSchema.Constants.TableName + " join " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName +
					@" on ap_ah = ah_pk join
                                    dbo.glbbranch on ah_gb = gb_pk
                                    where ap_matchgroupnum = @MatchGroupNum AND
                                    GB_GC = @CurrentCompany AND
                                    AP_PK != @PaymentMatchLinkPK AND
                                    AH_TransactionType != 'EXX')";
				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add("@MatchGroupNum", paymentMatchLink.AP_MatchGroupNum, AccTransactionMatchLinkSchema.AP_MatchGroupNum);
				@params.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, GlbBranchSchema.GB_GC);
				@params.Add("@PaymentMatchLinkPK", paymentMatchLink.PK, AccTransactionMatchLinkSchema.PK);

				query.AddFilterAndZSQLParameterCollection(whereClause, @params);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			TransactionMatchLink[] paymentMatchlinks = payment.Factory.Load<TransactionMatchLink>(query);
			return paymentMatchlinks;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Class Name does not need to be localised")]
		void AddProperties<T>(ExportCollectionInfoImpl info, MultilingualString name)
		{
			ImportPropertyInfoCollection fields = new ImportPropertyInfoCollection();
			string schemaClassName = (NoResString)"Schema";
			Type schemaType = typeof(T).BaseType.GetNestedType(schemaClassName) ?? typeof(T).BaseType.BaseType.GetNestedType(schemaClassName);

			if (schemaType != null)
			{
				foreach (FieldInfo fieldInfo in schemaType.GetFields())
				{
					if (fieldInfo.FieldType.Name == "String" && !fieldInfo.Name.EndsWith("MaxLength"))
					{
						fields.Add(new ImportPropertyInfoImpl<T>(fieldInfo.Name));
					}
				}
			}
			info.Add(name, typeof(T), fields);
		}

		//void AddBusinessObjectProperties<T>(ExportCollectionInfoImpl info, string name)
		//{
		//    ImportPropertyInfoCollection fields = new ImportPropertyInfoCollection();
		//    foreach (PropertyInfo property in typeof(T).GetProperties())
		//    {
		//        List<Type> interfaces = new List<Type>(property.PropertyType.GetInterfaces());
		//        if (interfaces.Contains(typeof(CargoWise.Types.IZType)) && !property.Name.Equals("PK"))
		//        {
		//            fields.Add(new ImportPropertyInfoImpl<T>(property.Name));
		//        }
		//    }
		//    info.Add(name, typeof(T), fields);
		//}
	}
}
