using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	[CodeProperty(FeatureControlSetSchema.Constants.FCS_ProductName), DescriptionProperty(FeatureControlSetSchema.Constants.FCS_ProductName)]
	public class FeatureControlSet : AutoFeatureControlSet, IAuditParent
	{
		public FeatureControlSet(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => FCS_ProductName;

		protected override ZString HumanReadableItemCodeCore => FCS_ProductName;

		[ResourceStringData("FeatureControlSet|FCS_ProductName", Caption = "Feature Set Name")]
		public override ZString FCS_ProductName { get => base.FCS_ProductName; set => base.FCS_ProductName = value; }

		[ChildEditable]
		public FeatureSetDatabaseCollection Databases
		{
			get
			{
				if (databases == null)
				{
					var collection = new FeatureSetDatabaseCollection(this, Factory);
					collection.Load();
					databases = collection;
					RegisterEditableChildObject(databases);
				}
				return databases;
			}
		}
		FeatureSetDatabaseCollection databases;

		public FeatureControlRuleNonDependentCollection FeatureRules
		{
			get
			{
				if (rules == null)
				{
					var query = new ZQuery(FeatureControlRuleSchema.FCR_FCS_FeatureSet, PK);
					var collection = new FeatureControlRuleNonDependentCollection(Factory, query);
					collection.Load();
					rules = collection;
				}
				return rules;
			}
		}
		FeatureControlRuleNonDependentCollection rules;

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		public IEnumerable<AuditChildInfo> RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(FeatureControlRuleSchema.FCR_FCS_FeatureSet, null);
			}
		}
	}
}
