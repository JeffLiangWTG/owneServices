using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ExportCustomsManifestHeaderLookups : Customs.Business.ExportCustomsManifestHeaderLookups
	{
		public ExportCustomsManifestHeaderLookups(ExportCustomsManifestHeader parent)
			: base(parent)
		{
		}

		public override RefCountryCollection CountryOfDestinations
		{
			get
			{
				if (fCountryOfDestinations == null)
				{
					fCountryOfDestinations = new RefCountryCollection(Parent.Factory);
				}
				return fCountryOfDestinations;
			}
		}
		protected RefCountryCollection fCountryOfDestinations;

		public override RefUNLOCOCollection PortOfDepartures
		{
			get
			{
				if (fPortOfDepartures == null)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "AU");
					fPortOfDepartures = new RefUNLOCOCollection(Parent.Factory, filter);
				}
				return fPortOfDepartures;
			}
		}
		protected RefUNLOCOCollection fPortOfDepartures;

		public override RefUNLOCOCollection PortOfDestinations
		{
			get
			{
				if (fPortOfDestinations == null)
				{
					fPortOfDestinations = new RefUNLOCOCollection(Parent.Factory);
				}
				return fPortOfDestinations;
			}
		}
		protected RefUNLOCOCollection fPortOfDestinations;

		public override CodeDescriptionPairList DocumentStatusList
		{
			get
			{
				if (fDocumentStatusList == null)
				{
					fDocumentStatusList = new CMR3CharDocumentStatusList();
				}
				return fDocumentStatusList;
			}
		}
		CodeDescriptionPairList fDocumentStatusList;

		public override CodeDescriptionPairList DocumentStatusConditionsList
		{
			get
			{
				if (fDocumentStatusConditionsList == null)
				{
					fDocumentStatusConditionsList = new CMR3CharDocumentStatusConditionsList();
				}
				return fDocumentStatusConditionsList;
			}
		}
		CodeDescriptionPairList fDocumentStatusConditionsList;

		public override OrgHeaderCollection OrgHeaders
		{
			get { return new CTOCollection(Parent.Factory); }
		}
	}
}
