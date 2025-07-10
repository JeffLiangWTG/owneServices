using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class FindTriageFilterHelper : AutoFindTriageFilterHelper
	{
		public FindTriageFilterHelper(SupportIncident incident) : base(incident.Factory)
		{
			Incident = incident;
		}

		#region Properties

		[List("Lookups.Types")]
		[ResourceStringData("FindTriageFilterHelper|NodeType", Caption = "Node Type")]
		public override ZString NodeType { get => base.NodeType; set => base.NodeType = value; }

		[List("Lookups.ProductList")]
		[ResourceStringData("FindTriageFilterHelper|Product", Caption = "Product")]
		public override ZString Product { get => base.Product; set => base.Product = value; }

		[List("Lookups.ProductAreaList")]
		[ResourceStringData("FindTriageFilterHelper|ProductArea", Caption = "Product Area")]
		public override ZString ProductArea { get => base.ProductArea; set => base.ProductArea = value; }

		[ResourceStringData("FindTriageFilterHelper|ClientMessageBuilder", Caption = "Client Message Builder")]
		public override ZString ClientMessageBuilder { get => base.ClientMessageBuilder; set => base.ClientMessageBuilder = value; }

		public SupportIncident Incident { get; }

		#endregion

		#region Collection and Filter

		[ResourceStringData("FindTriageFilterHelper|TriageCollection", Caption = "Triage Results")]
		public IncidentTriageCollection TriageCollection
		{
			get
			{
				if (triageCollection == null)
				{
					triageCollection = new IncidentTriageCollection(Factory);
				}

				return triageCollection;
			}
		}

		IncidentTriageCollection triageCollection;

		public void RefreshTriageCollection()
		{
			var triageQuery = new ZDBOnlyQuery(typeof(IncidentTriage));

			if (ShowInternalOnly)
			{
				triageQuery.AddToFilter(IncidentTriageSchema.IMT_IsInternal, true);
			}

			if (!string.IsNullOrWhiteSpace(NodeType))
			{
				triageQuery.AddToFilter(IncidentTriageSchema.IMT_Type, NodeType);
			}

			if (!string.IsNullOrWhiteSpace(Product))
			{
				triageQuery.AddToFilter(IncidentTriageSchema.IMT_Product, Product);
			}

			if (!string.IsNullOrWhiteSpace(ProductArea))
			{
				triageQuery.AddToFilter(IncidentTriageSchema.IMT_ProductArea, ProductArea);
			}

			if (!string.IsNullOrWhiteSpace(DescriptionKeyword1) || !string.IsNullOrWhiteSpace(DescriptionKeyword2) || !string.IsNullOrWhiteSpace(DescriptionKeyword3))
			{
				var descriptionSubQuery = new ZQuery();
				var orderBy = new ZStringBuilder("1");

				if (!string.IsNullOrWhiteSpace(DescriptionKeyword1))
				{
					descriptionSubQuery.AddToFilter(JoinCondition.Or, IncidentTriageSchema.IMT_SupportDescription, SQLComparisonOperator.Contains, DescriptionKeyword1);
					orderBy.Append(FormattableString.Invariant($" + CASE WHEN ({IncidentTriageSchema.Constants.IMT_SupportDescription} LIKE '%{DescriptionKeyword1}%') THEN 1 ELSE 0 END"));
				}

				if (!string.IsNullOrWhiteSpace(DescriptionKeyword2))
				{
					descriptionSubQuery.AddToFilter(JoinCondition.Or, IncidentTriageSchema.IMT_SupportDescription, SQLComparisonOperator.Contains, DescriptionKeyword2);
					orderBy.Append(FormattableString.Invariant($" + CASE WHEN ({IncidentTriageSchema.Constants.IMT_SupportDescription} LIKE '%{DescriptionKeyword2}%') THEN 1 ELSE 0 END"));
				}

				if (!string.IsNullOrWhiteSpace(DescriptionKeyword3))
				{
					descriptionSubQuery.AddToFilter(JoinCondition.Or, IncidentTriageSchema.IMT_SupportDescription, SQLComparisonOperator.Contains, DescriptionKeyword3);
					orderBy.Append(FormattableString.Invariant($" + CASE WHEN ({IncidentTriageSchema.Constants.IMT_SupportDescription} LIKE '%{DescriptionKeyword3}%') THEN 1 ELSE 0 END"));
				}

				triageQuery.AddToFilter(descriptionSubQuery, JoinCondition.And);
				triageQuery.OrderBy = orderBy.ToString() + OrderByClause.Descending;
			}

			TriageCollection.LoadWithMoreFiltering(triageQuery);
		}

		#endregion

		#region Lookups

		public FindTriageFilterHelperLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = GetNewLookups();
				}

				return lookups;
			}
		}

		protected virtual FindTriageFilterHelperLookups GetNewLookups()
		{
			return new FindTriageFilterHelperLookups(this);
		}

		FindTriageFilterHelperLookups lookups;

		#endregion
	}
}
