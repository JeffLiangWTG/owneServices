using System.Collections;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatHeaderLookups : AutoCusIntrastatHeaderLookups
	{
		public CusIntrastatHeaderLookups(AutoCusIntrastatHeader parent) : base(parent)
		{
		}

		public override OrgHeaderCollection Suppliers => new ConsignorCollection(Factory);

		public override OrgHeaderCollection Consignees => new ConsigneeCollection(Factory);

		public ICollection Countries => GetCountryCollection();

		protected virtual ICollection GetCountryCollection() => new RefCountryCollection(Factory);

		public ICodeDescriptionPairList NatureOfTransactionList => GetNatureOfTransactionList();

		protected virtual ICodeDescriptionPairList GetNatureOfTransactionList() => new NatureOfTransactionList();

		public ICodeDescriptionPairList ModeOfTransportList => GetModeOfTransportList();

		protected virtual ICodeDescriptionPairList GetModeOfTransportList() => new TransportTypeList();

		public ICodeDescriptionPairList IncoTermList => GetIncoTermList();

		protected virtual ICodeDescriptionPairList GetIncoTermList() => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
	}
}
