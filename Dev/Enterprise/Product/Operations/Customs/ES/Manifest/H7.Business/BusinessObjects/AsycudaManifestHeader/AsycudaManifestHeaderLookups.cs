using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class AsycudaManifestHeaderLookups : EU.H7.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		protected override OrganisationsFindBoxCollection PresenterListCore
		{
			get
			{
				var presenterList = new OrganisationsFindBoxCollection(Factory);
				presenterList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCarrierFilters.Country, "Property", (ZString)Core.Constants.CountryCodes.Spain));
				return presenterList;
			}
		}

		public CodeDescriptionPairList CertificateNames => CertificateHelper.CertificateNames(Factory, Parent.CustomsAgent, GetType().Name);

		public override CodeDescriptionPairList AgentTypeList => Factory.GetCachedValue("ES.H7.AsycudaManifestHeaderLookups.AgentTypeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(base.AgentTypeList);
			result.AddRange(new ESH7AgentTypes());

			return result;
		});

		public override CodeDescriptionPairList RegistrationStatusList => Factory.GetCachedValue<ESH7AISEntryStatusList>();

		public ICollection TransportDocumentTypes => Factory.GetCachedValue("ES.H7.AsycudaManifestHeaderLookups.TransportDocumentTypes", () =>
		{
			var result = new CodeDescriptionPairList();
			var collection = ZZRefCusCodeListCombined.Loader.Load(Factory, CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ESG3TransportDocumentCode, ZDateTime.Today);
			collection.ForEach(x => result.AddPairIfNotExist(x.ZZD_Code, x.ZZD_Description));
			return result;
		});

		public CodeDescriptionPairList G3MRNToRevokeList => Factory.GetCachedValue("ES.H7.AsycudaManifestHeaderLookups.G3MRNToRevokeList", () =>
		{
			var result = new CodeDescriptionPairList();

			var g3MrnCusEntryNumberQuery = new ZDBOnlyQuery(typeof(CusEntryNumber));
			g3MrnCusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryLineReference, AsycudaBill.G3DeclarationType);
			g3MrnCusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			g3MrnCusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, ZString.Empty);

			var billSubQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), CusEntryNumSchema.CE_ParentID);
			billSubQuery.AddToFilter(AsycudaBillSchema.ABL_AMA, Parent.PK);
			g3MrnCusEntryNumberQuery.AddSubQuery(billSubQuery, JoinCondition.And);

			var g3Mrns = Factory.Load<CusEntryNumber>(g3MrnCusEntryNumberQuery)
				.Select(entry => entry.CE_EntryNum)
				.Distinct();

			foreach (var mrn in g3Mrns)
			{
				result.AddPair(mrn, ZString.Empty);
			}

			return result;
		});
	}
}
