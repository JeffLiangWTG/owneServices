using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlExportBizObj : AutoFeatureControlExportBizObj
	{
		public FeatureControlExportBizObj() : base(new BusinessObjectFactory()) { }

		[List(nameof(Databases))]

		[RelatedBusinessObject("Database")]
		public override ZGuid DatabasePk { get => base.DatabasePk; set => base.DatabasePk = value; }

		[MaxLength(FeatureControlRule.Schema.FCR_ParametersMaxLength)]
		public override ZString RuleContent { get => base.RuleContent; set => base.RuleContent = value; }
		public bool RuleContent_ReadOnly => true;

		public ZString RuleContentRaw { get; set; }
		public bool RuleContentRaw_ReadOnly => true;

		public LicenceDatabase Database => Factory.Load<LicenceDatabase>(DatabasePk);

		public LicenceDatabaseNonDependentCollection Databases => new LicenceDatabaseNonDependentCollection(Factory);

		public void Export()
		{
			var featureControl = FeatureControlExtensions.LoadFromDatabase(DateTime.MinValue, DatabasePk);
			if (AllowAllFeatureStages)
			{
				featureControl.AllowAllFeatureStages = true;
				featureControl.AllowAllFeatureStagesSpecified = true;
			}
			RuleContentRaw = featureControl.ToXmlString();
			RuleContent = Convert.ToBase64String(featureControl.Compress());
		}
	}
}
