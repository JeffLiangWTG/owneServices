using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader
		, Integration.Customs.ASYCUDA.EUManifest.IAsycudaManifestHeader
		, IEuOfficeCodeProvider
		, ICusCodeDataTypeSupporter
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region EUCustomsOffices

		[ChildEditable(true)]
		public IcsOfficeCodeCollection EUCustomsOffices => euCustomsOffices ?? (euCustomsOffices = GetCustomsOffices());

		IcsOfficeCodeCollection euCustomsOffices;

		protected virtual IcsOfficeCodeCollection GetCustomsOffices()
		{
			var result = new IcsOfficeCodeCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		public CusCodeDataValidation GetIcsOfficeCodeValidation(IcsOfficeCode code) => GetIcsOfficeCodeValidationCore(code);
		protected virtual CusCodeDataValidation GetIcsOfficeCodeValidationCore(IcsOfficeCode code) => new IcsOfficeCodeValidation(code);
		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchStrategy.FetchForDelete();
				EUCustomsOffices.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public override ZString AMA_CustomsOffice
		{
			get
			{
				return base.AMA_CustomsOffice;
			}
			set
			{
				base.AMA_CustomsOffice = value;
				SetDefaultCustomsOffice(value);
			}
		}

		protected virtual void SetDefaultCustomsOffice(ZString office)
		{
			if (EUCustomsOffices == null || EUCustomsOffices.Cast<IcsOfficeCode>().All(x => x.CY_Code != OfficeCodes_ICS.Codes.OfficeOfFirstEntry))
			{
				EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfFirstEntry, office);
			}
		}

		public bool IsICSManifest => AMA_ManifestType == EUManifestTypes.Codes.ICS;

		protected override IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;

		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation()
		{
			AsycudaManifestHeaderValidation result = null;
			if (IsICSManifest)
			{
				result = new ICSAsycudaManifestHeaderValidation(this);
			}
			else
			{
				result = new AsycudaManifestHeaderValidation(this);
			}
			return result;
		}

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Latvia;

		public new IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (IAsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		public bool IsInlandTransport => AMA_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport;

		public ZString CountryCode => Branch.Company.GC_RN_NKCountryCode;

		public override ZBool IsImport => false;

		public override ZBool IsExport => false;

		public bool IsNCTS => false;

		public bool IsEMCS => false;

		IEnumerable<EuOfficeCode> IEuOfficeCodeProvider.CustomsOffices => EUCustomsOffices.Cast<IcsOfficeCode>();

		public CustomsOfficeRequirementHelper CustomsOfficeRequirementHelper => customsOfficeRequirementHelper ?? (customsOfficeRequirementHelper = GetCustomsOfficeRequirementHelper());
		CustomsOfficeRequirementHelper customsOfficeRequirementHelper;

		protected virtual CustomsOfficeRequirementHelper GetCustomsOfficeRequirementHelper() => new IcsCustomsOfficeRequirementHelper(this);

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);

		#region ICusCodeDataTypeSupporter

		protected override IDictionary<ZString, Type> SupportedCusCodeDataTypes
		{
			get
			{
				var result = new Dictionary<ZString, Type>();
				result.Add(CusCodeDataTypeList.Codes.OfficeCode, typeof(IcsOfficeCode));
				return result;
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			AMA_ManifestType = EUManifestTypes.Codes.ICS;
		}
#endif

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusCodeDataTypeSupporterFetchStrategy(this);
		}

		class AsycudaManifestHeaderFetchStrategy : ASYCUDA.Business.AsycudaManifestHeaderFetchStrategy
		{
			public AsycudaManifestHeaderFetchStrategy(AsycudaManifestHeader parent)
				: base(parent)
			{
			}

			protected override void AddFetchHintsForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			}

			protected new AsycudaManifestHeader BusinessObject => (AsycudaManifestHeader)base.BusinessObject;
		}
	}
}
