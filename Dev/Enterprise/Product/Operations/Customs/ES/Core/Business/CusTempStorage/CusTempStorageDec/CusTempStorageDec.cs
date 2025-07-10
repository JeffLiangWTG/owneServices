using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ES.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageJobHeader), "CusTempStorageDecs")]
	public class CusTempStorageDec : EU.Business.CusTempStorage.CusTempStorageDec,
		Integration.Customs.ES.ICusTempStorageDec
		, ICusSupportingInfoTypeSupporter
		, ICusCodeDataTypeSupporter
		, EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport
		, IAllowPermitProcessing
		, ISavingProvider<CusTempStorageDec>
	{
		public CusTempStorageDec(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static CusTempStorageDec New(CusTempStorageJobHeader parent)
		{
			var result = parent.Factory.New<CusTempStorageDec>();
			using (result.SuspendSettingHasChanges())
			{
				result.STH_SJH = parent.PK;
			}
			return result;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			STH_DeclarationType = "IST";
			STH_SystemCreateTimeUtc = ZDateTime.Now;
		}

		#region CusTempStorageLines

		public new CusTempStorageLineCollection CusTempStorageLines => (CusTempStorageLineCollection)base.CusTempStorageLines;

		protected override EU.Business.CusTempStorage.CusTempStorageLineCollection CreateNewCusTempStorageLines() => new CusTempStorageLineCollection(this);

		#endregion

		#region Properties

		[ReadOnly(true)]
		public override ZDateTime STH_SystemCreateTimeUtc { get => base.STH_SystemCreateTimeUtc; set => base.STH_SystemCreateTimeUtc = value; }

		[ReadOnly(true)]
		public override ZString STH_MessageStatus { get => base.STH_MessageStatus; set => base.STH_MessageStatus = value; }

		#region STH_Calc_CustomsDocumentReferences
		public ZString STH_Calc_CustomsDocumentReferences
		{
			get
			{
				return string.Join("; ", SupportingDocuments.Cast<SupportingDocument>().OrderBy(x => x.CSI_Code).Select(x => x.CSI_Code + "=" + x.CSI_ReferenceNumber));
			}
		}
		#endregion

		#region STH_Calc_Containers
		public ZString STH_Calc_Containers
		{
			get
			{
				return string.Join("; ", CusTempStorageContainers.Cast<CusTempStorageContainer>().OrderBy(x => x.CY_Code).Select(x => x.CY_Data));
			}
		}
		#endregion

		public override ZGuid STH_SJH
		{
			get => base.STH_SJH;
			set
			{
				var oldValue = STH_SJH;
				base.STH_SJH = value;
				if (oldValue != STH_SJH && StorageHeader != null)
				{
					SetStorageHeaderDefaultValues();
				}
			}
		}

		public ZString LocationOfGoods
		{
			get
			{
				var list = CusTempStorageLines.Cast<CusTempStorageLine>().Select(x => x.TSL_LocationOfGoods).Distinct();
				return list.Count() <= 1 ? list.FirstOrDefault().ToString() : Multiple;
			}
		}

		public ZString UnionStatus
		{
			get
			{
				var list = CusTempStorageLines.Cast<CusTempStorageLine>().Select(x => x.TSL_UnionStatus).Distinct();
				return list.Count() <= 1 ? list.FirstOrDefault().ToString() : Multiple;
			}
		}

		#endregion

		#region Implement
		#region SupportingDocuments

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments => supportingDocuments ?? (supportingDocuments = GetSupportingDocuments());
		SupportingDocumentCollection supportingDocuments;

		SupportingDocumentCollection GetSupportingDocuments()
		{
			var result = GetNewSupportingDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual SupportingDocumentCollection GetNewSupportingDocumentCollection() => new SupportingDocumentCollection(this);

		#endregion

		#region CusTempStorageContainers

		[ChildEditable(true)]
		public CusTempStorageContainerCollection CusTempStorageContainers => cusTempStorageContainers ?? (cusTempStorageContainers = GetCusTempStorageContainers());
		CusTempStorageContainerCollection cusTempStorageContainers;

		CusTempStorageContainerCollection GetCusTempStorageContainers()
		{
			var result = GetNewCusTempStorageContainerCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual CusTempStorageContainerCollection GetNewCusTempStorageContainerCollection() => new CusTempStorageContainerCollection(this);

		#endregion

		public new CusTempStorageJobHeader StorageHeader => base.StorageHeader as CusTempStorageJobHeader;

		protected virtual void SetStorageHeaderDefaultValues()
		{
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			FireOnSavingEvent();
		}
		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region ICusSupportingInfoTypeSupporter Members

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes() => GetCusSupportingInfoTypesCore();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) }
			};
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		public IEnumerable<IBusinessObjectFetchStrategy> GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		const string TempStorageContainerCode = "TSC";

		public IDictionary<ZString, Type> GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>
			{
				{ TempStorageContainerCode, typeof(CusTempStorageContainer) }
			};
			return result;
		}

		#endregion

		#region ICanBeImportOrExport
		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsImport => ZBool.False;
		ZBool EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.IsExport => ZBool.False;
		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.Level => Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header;
		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.TrueCountryCode => StorageHeader?.CountryCode ?? ZString.Empty;
		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.DataGroupingCode => StorageHeader?.CountryCode ?? ZString.Empty;
		void EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.ValidatePreviousDocuments()
		{
		}
		#endregion

		#region IAllowPermitProcessing

		ZInt IAllowPermitProcessing.PermitValueDecimalPlaceCount => LocalCurrency?.Decimals ?? 2;

		ZInt IAllowPermitProcessing.PermitQuantityDecimalPlaceCount => 5;

		ZInt IAllowPermitProcessing.PackageCount => CusTempStorageLines.Cast<CusTempStorageLine>().Sum(x => x.TSL_PackageQty);

		ZString IAllowPermitProcessing.GetPermitReference() => StorageHeader.SJH_JobReference;

		ZInt IAllowPermitProcessing.GetPermitReferenceNumberLine() => ZInt.Zero;

		IList<PermitRecord> IAllowPermitProcessing.GetPermitRecords()
		{
			var result = new List<PermitRecord>();
			var guarantee = StorageHeader.GuaranteeHeader;
			var hasGuaranteedValue = CusTempStorageLines.Cast<CusTempStorageLine>().Any(x => x.CusTempStorageLineItems.Any(y => !y.TSI_GuaranteedValue.IsEmpty));
			var localCurrency = LocalCurrency;
			if (guarantee != null && hasGuaranteedValue && localCurrency != null)
			{
				var amount = TotalTSIGuaranteedValueAmount;
				if (!amount.IsEmpty)
				{
					result.Add(new PermitRecord() { PermitHeader = guarantee, Quantity = ZDecimal.Zero, Value = amount });
				}
			}
			return result;
		}

		public ZDecimal TotalTSIGuaranteedValueAmount
		{
			get
			{
				var currencyConverter = CurrencyConverter.New(Factory);
				var sumAmount = Money.Empty;
				foreach (CusTempStorageLine line in CusTempStorageLines)
				{
					foreach (var lineItem in line.CusTempStorageLineItems)
					{
						if (!lineItem.TSI_GuaranteedValue.IsEmpty)
						{
							sumAmount = currencyConverter.Add(sumAmount, new Money(lineItem.TSI_GuaranteedValue, lineItem.Currency));
						}
					}
				}
				return currencyConverter.ConvertExact(sumAmount, LocalCurrency).Amount;
			}
		}

		ZString IAllowPermitProcessing.GetPermitComment(PermitRecord permitRecord) => StorageHeader.SJH_ReferenceNumber;

		RefCurrency LocalCurrency => RefCurrency.LoadFromCurrencyCode(Factory, StorageHeader.Branch.Company.GC_RX_NKLocalCurrency);
		#endregion

		#region ISavingProvider
		public void FireOnSavingEvent()
		{
			if (Saving != null)
			{
				Saving(this);
			}
		}

		public event SavingEventHandler<CusTempStorageDec> Saving;
		#endregion
	}
}
