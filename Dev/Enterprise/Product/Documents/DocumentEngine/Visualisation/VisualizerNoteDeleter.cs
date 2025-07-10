using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Visualisation
{
	class VisualizerNoteDeleter : DeleteChecker
	{
		public override void BeforeSuccessfulDelete(BusinessObject businessObject)
		{
			base.BeforeSuccessfulDelete(businessObject);

			if (MightHaveRelatedVisualizerNotes(businessObject))
			{
				foreach (var relatedVisualizerNote in FindRelatedVisualizerNotes(businessObject))
				{
					relatedVisualizerNote.Delete();
				}
			}
		}

		public override void AddFetchHint(BusinessObject businessObject)
		{
			if (MightHaveRelatedVisualizerNotes(businessObject))
			{
				businessObject.Factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentID, businessObject.PK);
				businessObject.Factory.AddFetchHint(StmDocDataOverrideSchema.DD_ParentRelatedID, businessObject.PK);
			}
		}

		VisualizerNote[] FindRelatedVisualizerNotes(BusinessObject businessObject)
		{
			var query = new ZQuery(StmDocDataOverrideSchema.DD_ParentID, businessObject.PK);
			query.AddToFilter(JoinCondition.Or, StmDocDataOverrideSchema.DD_ParentRelatedID, businessObject.PK);
			return businessObject.Factory.Load<VisualizerNote>(query);
		}

		public override DeleteDetails DeleteDetails(BusinessObject businessObject)
			=> new DeleteDetails.Allow();

		bool MightHaveRelatedVisualizerNotes(BusinessObject businessObject)
			=> businessObject.IsInDatabase && businessObject.TablePrefix != StmDocDataOverrideSchema.Constants.Prefix && !Array.Exists(BOsWithNoRelatedVisualizerNotes, x => x.Equals(businessObject.TableName));

		readonly ZString[] BOsWithNoRelatedVisualizerNotes = new ZString[]
		{
			CusCodeDataSchema.Constants.TableName,
			CusAddInfoSchema.Constants.TableName,
			CusSupportingInfoSchema.Constants.TableName,
			CusEntryNumSchema.Constants.TableName,
			CusHouseContPackInvoiceLinePivotSchema.Constants.TableName,
			CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName,
			CusContainerInvoiceLinePivotSchema.Constants.TableName,
			CusInBondCargoDescSchema.Constants.TableName,
		};
	}
}
