using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public sealed class ClientInformation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T110";
		public ZString ClientCode { get; set; }
		public ZString ClientName { get; set; }
		public ZString ClientAbbreviation { get; set; }
	}

	public sealed class ClientInformationCollection : NonPersistentBusinessObjectCollection<ClientInformation>	{
		public ClientInformationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddDefaultElements();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientInformation();
		}

		void AddDefaultElements()
		{
			OrgHeaderCollection collection = new OrgHeaderCollection(Factory, filter());
			collection.Load();
			foreach (OrgHeader client in collection)
			{
				ClientInformation clientInformation = AddNew();
				clientInformation.ClientCode = client.OH_Code;
				clientInformation.ClientName = LocalCompanyName.GetLocalCompanyName(client, OrgConstants.AddressType.Receivables);
				clientInformation.ClientAbbreviation = client.OH_Code;
			}
		}

		ZQuery filter()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.OrderBy = AutoOrgHeader.Schema.OH_Code;
			return query;
		}
	}
}

