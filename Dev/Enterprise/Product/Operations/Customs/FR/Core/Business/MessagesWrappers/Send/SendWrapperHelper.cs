using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryHeaderCharges = Enterprise.Customs.FR.Business.Declaration.CusEntryHeaderCharges;
using CusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.FR.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send
{
	public static class SendWrapperHelper
	{
		public static List<LiquidationItemWrapper> GetLiquidation(CusEntryHeader entryHeader, bool useConfirmedFees)
		{
			var liquidation = new List<LiquidationItemWrapper>();
			foreach (var entryLine in entryHeader.MergedLines.Cast<CusEntryLine>().ToList())
			{
				var fees = useConfirmedFees ? entryLine.ConfirmedFees.Cast<CusEntryLineFee>().ToArray() : entryLine.Fees.Cast<CusEntryLineFee>().ToArray();

				if (entryLine.RandomLine != null && !entryLine.RandomLine.JI_TariffBypassCode.IsEmpty)
				{
					foreach (CusEntryLineFee entryLineFee in fees.Where(x => x.CF_RateOverrideReasonCode != RateOverrideReasonList.Codes.Additional && x.CF_RateOverrideReasonCode != RateOverrideReasonList.Codes.Precalcule && !x.CF_RateOverrideReasonCode.IsEmpty))
					{
						liquidation.Add(new LiquidationItemWrapper(entryLineFee));
					}
				}
				else
				{
					foreach (CusEntryLineFee entryLineFee in fees.Where(x => x.CF_RateOverrideReasonCode != RateOverrideReasonList.Codes.Additional && x.CF_RateOverrideReasonCode != RateOverrideReasonList.Codes.Precalcule))
					{
						liquidation.Add(new LiquidationItemWrapper(entryLineFee));
					}
				}

				if (entryLine.CL_LineNumber == 1)
				{
					var charges = useConfirmedFees ? entryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>().ToArray() : entryHeader.Charges.Cast<CusEntryHeaderCharges>().ToArray();
					foreach (CusEntryHeaderCharges entryHeaderCharge in charges)
					{
						liquidation.Add(new LiquidationItemWrapper(entryHeaderCharge));
					}
				}
			}

			return liquidation;
		}

		public static IEnumerable<IArticle> GetArticles(CusEntryHeader entryHeader, ErrorCollector itemErrorCollector)
		{
			List<ArticleWrapper> articleWrapperList = new List<ArticleWrapper>();

			foreach (var entryLine in entryHeader.MergedLines.Cast<CusEntryLine>().ToList())
			{
				articleWrapperList.Add(new ArticleWrapper(entryHeader, entryLine));
			}

			return articleWrapperList;
		}

		public static ZString SendDDT(BusinessObjectFactory factory, CusTempStorageJobHeader header, ErrorCollector errorCollector)
		{
			if (AllocateDDTNumberIfNeeded(header, errorCollector))
			{
				var cusTempStorageDec = header.CusTempStorageDec;
				header.SetTempStorageEndDateUtcDefaultValue();
				AddPermitRecords(cusTempStorageDec);
				CreateTempStorageRegister(factory, header);
				UpdateOldTemporaryStorageIfApplicable(header);
				AddEventAfterSendingDDT(header);
				new AdviceOnPlacementInIstEDocsSaver(header).RenderDocumentAndSaveInEDocs();
				factory.Save();
			}

			return errorCollector.ErrorCount == 0 ? Res.GetString("82FB5CB9-CB3D-4569-B9FF-C14FF843DE5B", "Temp. Storage Register {0} was created.", header.DDTNumber + "/" + header.SJH_JobReference) : Res.GetString("C77B14E4-17E5-476E-A7D1-34F2AA6401E8", "Failed to send DDT.");
		}

		static void UpdateOldTemporaryStorageIfApplicable(CusTempStorageJobHeader header)
		{
			CusTempStorageJobHeader complementaryJobParent = header?.PreviousISTHeader;
			if (complementaryJobParent != null && header.CusTempStorageDec is ISTCusTempStorageDec tempStorageDec)
			{
				var processor = new EFTA.TemporaryStorageRegister.Business.CusTempStorageRegisterProcessor<CusTempStorageDec>(tempStorageDec, new LoggingInformation());
				try
				{
					processor.CalculateTransactionsAndLockMutexIfNeededForAddingTransaction();
					processor.AddTransactionsWhenSaving(tempStorageDec);
				}
				finally
				{
					processor.UnlockRegistersMutexes();
				}
			}
		}

		static bool AllocateDDTNumberIfNeeded(CusTempStorageJobHeader header, ErrorCollector errorCollector)
		{
			try
			{
				if (header.DDTNumber.IsEmpty)
				{
					header.AllocateDDTNumberOnSaving = true;
					header.Factory.Save();
				}
			}
			catch (ZCannotSaveException e)
			{
				errorCollector.AddError(e.Message);
			}
			finally
			{
				header.AllocateDDTNumberOnSaving = false;
			}

			return errorCollector.ErrorCount == 0;
		}

		static void AddPermitRecords(CusTempStorageDec tempStorageDec)
		{
			var permitProcessor = new FRCusPermitCusDecProcessorForTempStorage(tempStorageDec);
			try
			{
				permitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				permitProcessor.AddPermitTransactions(tempStorageDec, (msg) => GetPermitAppIdForMessage(tempStorageDec), Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
			finally
			{
				permitProcessor.UnlockPermitMutexes();
			}
		}

		static void CreateTempStorageRegister(BusinessObjectFactory factory, CusTempStorageJobHeader header)
		{
			var regHeader = factory.New<CusTempStorageRegHeader>();
			regHeader.SRH_InternalReference = header.SJH_JobReference;
			regHeader.SRH_Reference = header.DDTNumber;
			regHeader.SRH_ArrivalDate = header.SJH_ArrivalDate;
			regHeader.SRH_PresentationDate = header.SJH_PresentationDate;
			regHeader.SRH_PreviousReferenceType = header.SJH_PreviousReferenceType;
			regHeader.SRH_PreviousReference = header.SJH_PreviousReferenceNumber;

			foreach (CusTempStorageLine line in header.CusTempStorageDec.CusTempStorageLines)
			{
				var regLine = regHeader.CusTempStorageRegLines.AddNew();
				regLine.SRL_LineNumber = line.TSL_LineNo;
				regLine.SRL_OwnerReferenceType = line.TSL_OwnerReferenceType;
				regLine.SRL_OwnerReference = line.TSL_OwnerReferenceNumber;
				regLine.SRL_UnionStatus = line.TSL_UnionStatus;
				regLine.SRL_LocationOfGoods = line.TSL_LocationOfGoods.Left(CusTempStorageRegLine.Schema.SRL_LocationOfGoodsMaxLength);
				regLine.SRL_PackageType = line.TSL_PackageType;
				regLine.SRL_LimitDate = header.SJH_TempStorageEndDateUtc.Date;
				regLine.SRL_GrossWeightUQ = Core.Constants.Weight.Kilograms;

				var packageQty = line.TSL_PackageQty;
				if (packageQty > 0)
				{
					ZString grossWeightUQ = Core.Constants.Weight.ContainsCode(line.TSL_GrossWeightUQ) ? line.TSL_GrossWeightUQ : (ZString)Core.Constants.Weight.Kilograms;
					var transaction = regLine.CusTempStorageRegLineTransactions.AddNew();
					transaction.SRT_GrossWeight = new ZWeight(line.TSL_GrossWeight, grossWeightUQ).InKilograms;
					transaction.SRT_PackageQty = packageQty;
					transaction.SRT_ReferenceType = CusTempStorageRegLineTransactionReferenceTypeList.Codes.IST;
					transaction.SRT_Reference = header.DDTNumber;
					transaction.SRT_InternalReferenceNumber = header.SJH_JobReference;
				}
			}
		}

		static void AddEventAfterSendingDDT(CusTempStorageJobHeader header)
		{
			header.Logs.AddNew(Events.InStore);
		}

		public static ZString GetPermitAppIdForMessage(CusTempStorageDec tempStorageDec) => tempStorageDec.StorageHeader.SJH_JobReference;
	}
}
