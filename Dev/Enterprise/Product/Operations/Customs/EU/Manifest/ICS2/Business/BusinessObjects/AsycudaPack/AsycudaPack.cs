using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ICusSupportingInfoTypeSupporter, ICusReferenceTypeSupporter, IAsycudaTransportMeansProvider
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		public override ZString APA_GoodsDescription
		{
			get => base.APA_GoodsDescription;
			set
			{
				base.APA_GoodsDescription = value;

				if (PackedItems.Count == 1)
				{
					PackedItem.API_GoodsDescription = Regex.Replace(value, @"[\r\n]", "");
				}
			}
		}

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		public new AsycudaPackLookups Lookups => (AsycudaPackLookups)base.Lookups;

		protected override ManifestBase.AsycudaPackLookups GetNewLookups() => new AsycudaPackLookups(this);

		protected override ManifestBase.AsycudaPackValidation GetNewValidation() => new AsycudaPackValidation(this);

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		protected override ASYCUDA.Business.IAsycudaPackedItemCollection<ASYCUDA.Business.AsycudaPackedItem, ASYCUDA.Business.AsycudaPack> CreateNewAsycudaPackedItemCollection()
		{
			return new ASYCUDA.Business.AsycudaPackedItemCollection<ASYCUDA.Business.AsycudaPackedItem, AsycudaPack>(this);
		}

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;

		#endregion

		#region Supporting Documents

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}

		SupportingDocumentCollection supportingDocuments;

		#endregion

		#region Additional Infos

		[ChildEditable(true)]
		public AdditionalInfoCollection AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = new AdditionalInfoCollection(this);
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}

				return additionalInfos;
			}
		}

		AdditionalInfoCollection additionalInfos;

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

		#region ICusSupportingInfoTypeSupporter

		public IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
				{ CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region Cus Supply Chain Actor Reference

		[ChildEditable(true)]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences
		{
			get
			{
				if (cusSupplyChainActorReferences == null)
				{
					cusSupplyChainActorReferences = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
					cusSupplyChainActorReferences.Load();
					RegisterEditableChildObject(cusSupplyChainActorReferences);
				}
				return cusSupplyChainActorReferences;
			}
		}

		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> cusSupplyChainActorReferences;

		#endregion

		#region TranssportMeans

		[ChildEditable(true)]
		public AsycudaTransportMeansCollection AsycudaTransportMeans
		{
			get
			{
				if (asycudaTransportMeans == null)
				{
					asycudaTransportMeans = new AsycudaTransportMeansCollection(this);
					asycudaTransportMeans.Load();
					RegisterEditableChildObject(asycudaTransportMeans);
				}
				return asycudaTransportMeans;
			}
		}
		AsycudaTransportMeansCollection asycudaTransportMeans;

		#endregion

		#region ICusReferenceTypeSupporter Members

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, typeof(CusSupplyChainActorReference) },
		};

		#endregion

		public ZDecimal GrossWeightInKG => new ZWeight(APA_Weight, APA_WeightUQ).InKilogramsSafe;

		protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property) => base.GetShouldPropertiesBeReadOnly(property) || (Bill is AsycudaBill { Header: { } header } && !header.CanBeAmended(property.Name));
	}
}
