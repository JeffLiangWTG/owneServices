using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	public class ExtractionColumnCollection : SQLColumnSpecificationCollection
	{
		public ExtractionColumnCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Collection<SQLColumnSpecification> ColumnsToAddOnBuild()
		{
			return new Collection<SQLColumnSpecification>
			{
				new SQLColumnSpecification(Factory, MENTColumns.Codes.ReleaseGroup, GlbGroupSchema.GG_Code.Name, MENTConstants.StringColumn, requireCoalesce: true),
				new SQLColumnSpecification(Factory, MENTColumns.Codes.Score, MENTAgedScoreMetricSchema.MAS_AgedScoreValue.Name, MENTConstants.DecimalColumn, requireCoalesce: false),
				new SQLColumnSpecification(Factory, MENTColumns.Codes.AttributeValue, MENTAgedScoreMetricSchema.MAS_AttributeValue.Name, MENTConstants.StringColumn, requireCoalesce: false),
				new SQLColumnSpecification(Factory, MENTColumns.Codes.Component, BMComponentSchema.FC_Name.Name, MENTConstants.StringColumn, requireCoalesce: true),
				new SQLColumnSpecification(Factory, MENTColumns.Codes.DateTime, MENTAgedScoreMetricSchema.MAS_TimeRecordedUtc.Name, MENTConstants.DateColumn, requireCoalesce: false),
				new SQLColumnSpecification(Factory, MENTColumns.Codes.Staff, GlbStaffSchema.GS_FullName.Name, MENTConstants.StringColumn, requireCoalesce: true)
			};
		}
	}
}
