using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.FetchStrategies;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public partial class JobComInvoiceHeader : Customs.Business.BaseJobComInvoiceHeader
		, Integration.Customs.AsycudaCustoms.IJobComInvoiceHeader
		, ISupportingDocumentsProvider
	{
		public JobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region CusSupportingInfo Related

		SupportingDocumentCollection fSupportingDocuments;

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (fSupportingDocuments == null)
				{
					fSupportingDocuments = new SupportingDocumentCollection(this);
					fSupportingDocuments.Load();
					RegisterEditableChildObject(fSupportingDocuments);
				}
				return fSupportingDocuments;
			}
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ Constants.CusSupportingInfoTypes.CusSupportingDocument, typeof(SupportingDocument) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		ZGuid Integration.Customs.ICusSupportingInfoTypeSupporter.PK => PK;

		BusinessObjectFactory Integration.Customs.ICusSupportingInfoTypeSupporter.Factory => Factory;

		bool Integration.Customs.ICusSupportingInfoTypeSupporter.IsInDatabase => IsInDatabase;

		#endregion

		public override ZGuid JZ_GB
		{
			get { return base.JZ_GB; }
			set
			{
				var oldValue = JZ_GB;
				base.JZ_GB = value;
				if (oldValue != JZ_GB)
				{
					applicationBusinessProvider?.InvalidateCache();
				}
			}
		}

		public BaseApplicationBusinessProvider ApplicationBusinessProvider => (applicationBusinessProvider ?? (applicationBusinessProvider = new RecalculableCachedValue<BaseApplicationBusinessProvider>(() => JobDeclaration?.ApplicationBusinessProvider ?? BaseApplicationBusinessProvider.GetApplicationBusinessProvider(Factory, CountryCode)))).Value;
		RecalculableCachedValue<BaseApplicationBusinessProvider> applicationBusinessProvider;

		protected override ZBool IsReciprocalRatesCore => ApplicationBusinessProvider?.IsReciprocalRates(JobDeclaration) ?? base.IsReciprocalRatesCore;
	}
}
