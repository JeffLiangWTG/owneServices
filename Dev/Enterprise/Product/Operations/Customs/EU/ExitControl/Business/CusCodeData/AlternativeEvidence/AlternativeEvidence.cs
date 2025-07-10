using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class AlternativeEvidence : CusCodeData
		, Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		public AlternativeEvidence(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusExitReport ExitReport => (CusExitReport)Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusExitReport));

		public new AlternativeEvidenceLookups Lookups => (AlternativeEvidenceLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new AlternativeEvidenceLookups(this);

		public new AlternativeEvidenceValidation Validation => (AlternativeEvidenceValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new AlternativeEvidenceValidation(this);

		protected override ZString HumanReadableNameCore => Res.GetString("9AD72166-A7FF-437E-AA17-EEC532080761", "Alternative Evidence");

		[MaxLength(2)]
		[ResourceStringData("72278BCD-05C4-4053-853B-8BF0D59009A2", Caption = "Type")]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

		[ChildEditable(true)]
		public IAdditionalInfoCollection<AdditionalInfo> AdditionalInfos
		{
			get
			{
				if (additionalInfos == null)
				{
					additionalInfos = CreateNewAdditionalInfoCollection();
					additionalInfos.Load();
					RegisterEditableChildObject(additionalInfos);
				}
				return additionalInfos;
			}
		}
		IAdditionalInfoCollection<AdditionalInfo> additionalInfos;

		protected virtual IAdditionalInfoCollection<AdditionalInfo> CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection<AdditionalInfo>(this);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(AdditionalInfo));
			return result;
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies() => GetAdditionalBusinessObjectFetchStrategies();

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
	}
}
