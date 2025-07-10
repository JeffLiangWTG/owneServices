using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BillingResponsibleAgreementsCalculator
	{
		public BillingResponsibleAgreementsCalculator(BusinessObjectFactory factory, ZGuid customerPk, ZString product, ZString service, ZString submodule, ZDate date)
		{
			this.factory = factory;
			this.customerPk = customerPk;
			this.product = product;
			this.service = service;
			this.subModule = submodule;
			this.date = date;
		}

		#region Fields

		readonly BusinessObjectFactory factory;
		readonly ZGuid customerPk;
		readonly ZString product;
		readonly ZString service;
		readonly ZString subModule;
		readonly ZDate date;

		#endregion

		public Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> GetForClientCompanies(ZGuid[] pivotPks)
		{
			var itemArgs = new CommissionItemArgs(customerPk, product, service, subModule, date, "", "", "");
			var itemsPart = ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs);

			var querySql = ZString.Format(@"
	SELECT CA0_PK, PivotPk, Stream
	FROM
	(
		SELECT
			CA0_PK,
			c.Value PivotPk,
			CA0_CommissionStream Stream,
			ROW_NUMBER() OVER (PARTITION BY CA0_CommissionStream, c.Value ORDER BY CASE WHEN EZN_IsAllCompanies IS NULL THEN 1 ELSE 0 END, EZN_IsAllCompanies, {1}) RANK
		FROM
			dbo.OrgCommissionAgreement
			JOIN dbo.ViewCommissionAgreementOverallItem ON CA0_PK = VCI_CA0
			CROSS JOIN @ClientCompanies c
			LEFT JOIN dbo.EdiCommissionAgreementCustomization ON CA0_PK = EZN_CA0
			LEFT JOIN dbo.EdiCommissionAgreementCompanyPivot ON EPY_EZN = EZN_PK
		WHERE (c.Value = EPY_LCC OR ISNULL(EZN_IsAllCompanies, 1) = 1) AND ({0})
	) a
	WHERE
		RANK = 1
", itemsPart.ParameterisedText.ParameterisedQueryText, itemsPart.OrderBy);

			var paramCollection = new ZSqlParameterCollection();
			paramCollection.Add(ZSqlParameter.New("@ClientCompanies", pivotPks.Where(p => !p.IsEmpty && p.IsValid).ToArray(), EdiCommissionAgreementCompanyPivotSchema.EPY_LCC, true));
			paramCollection.AddRange(itemsPart.Params);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(querySql, paramCollection);

			var agreementList = collection.Select(b => (ZGuid)b["CA0_PK"]).ToArray();

			var query = new ZDBOnlyQuery(typeof(EdiCommissionAgreement));
			query.AddToFilter(OrgCommissionAgreementSchema.PK, agreementList);

			var agreements = factory.Load<EdiCommissionAgreement>(query);
			return CombineToDictionary(collection, agreements);
		}

		public Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> GetForLicenseDatabases(ZGuid[] pivotPks)
		{
			var itemArgs = new CommissionItemArgs(customerPk, product, service, subModule, date, "", "", "");
			var itemsPart = ResponsibleOverallItemsQueryBuilder.New(ZGuid.Empty, itemArgs);

			var querySql = ZString.Format(@"
	SELECT CA0_PK, PivotPk, Stream
	FROM
	(
		SELECT
			CA0_PK,
			d.Value PivotPk,
			CA0_CommissionStream Stream,
			ROW_NUMBER() OVER (PARTITION BY CA0_CommissionStream, d.Value ORDER BY CASE WHEN EZN_IsAllDatabases IS NULL THEN 1 ELSE 0 END, EZN_IsAllDatabases, {1}) RANK
		FROM
			dbo.OrgCommissionAgreement
			JOIN dbo.ViewCommissionAgreementOverallItem ON CA0_PK = VCI_CA0
			CROSS JOIN @LicenseDatabases d
			LEFT JOIN dbo.EdiCommissionAgreementCustomization ON CA0_PK = EZN_CA0
			LEFT JOIN dbo.EdiCommissionAgreementDatabasePivot ON EZD_EZN = EZN_PK
		WHERE (d.Value = EZD_LD OR ISNULL(EZN_IsAllDatabases, 1) = 1) AND ({0})
	) a
	WHERE
		RANK = 1
", itemsPart.ParameterisedText.ParameterisedQueryText, itemsPart.OrderBy);

			var paramCollection = new ZSqlParameterCollection();
			paramCollection.Add(ZSqlParameter.New("@LicenseDatabases", pivotPks, EdiCommissionAgreementDatabasePivotSchema.EZD_LD, true));
			paramCollection.AddRange(itemsPart.Params);

			var collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(querySql, paramCollection);

			var agreementList = collection.Select(b => (ZGuid)b["CA0_PK"]).ToArray();

			var query = new ZDBOnlyQuery(typeof(EdiCommissionAgreement));
			query.AddToFilter(OrgCommissionAgreementSchema.PK, agreementList);

			var agreements = factory.Load<EdiCommissionAgreement>(query);
			return CombineToDictionary(collection, agreements);
		}

		Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement> CombineToDictionary(DynamicBusinessObjectCollection collection, EdiCommissionAgreement[] agreements)
		{
			var dictionary = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();
			foreach (DynamicBusinessObject item in collection)
			{
				var agreement = agreements.FirstOrDefault(a => a.PK == (ZGuid)item["CA0_PK"]);
				if (agreement != null)
				{
					dictionary[(item["Stream"].ToString(), (ZGuid)item["PivotPk"])] = agreement;
				}
			}
			return dictionary;
		}
	}
}

