using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface IEInvoiceHelper
	{
		ZString GetReasonOfCancellation(ZGuid batchPK);
	}

	class EInvoiceHelper : IEInvoiceHelper
	{
		ZString IEInvoiceHelper.GetReasonOfCancellation(ZGuid batchPK)
		{
			var sqlSelect = $@"
SELECT Top 1 {AccTransactionHeaderReference.Schema.AH1_Reference} FROM {AccTransactionHeaderReferenceSchema.Constants.SqlSchemaName}.{AccTransactionHeaderReferenceSchema.Constants.TableName}
	JOIN  {AccEInvoicingTransactionPivotSchema.Constants.SqlSchemaName}.{AccEInvoicingTransactionPivotSchema.Constants.TableName} ON {AccTransactionHeaderReference.Schema.AH1_AH} = {AccEInvoicingTransactionPivot.Schema.AIP_ParentID}
WHERE {AccEInvoicingTransactionPivot.Schema.AIP_AIB} = @batchPK AND {AccTransactionHeaderReference.Schema.AH1_Type} = @headerReferenceType";

			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@batchPK", batchPK.ToGuid(), AccEInvoicingBatchSchema.PK),
				ZSqlParameter.New("@headerReferenceType", AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.MXR, AccTransactionHeaderReferenceSchema.AH1_Type)
			};

			var dynamicBusinessObjectCollection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			dynamicBusinessObjectCollection.Load(sqlSelect, sqlParameters);

			var reference = dynamicBusinessObjectCollection.FirstOrDefault();
			return reference != null ? (ZString)reference[AccTransactionHeaderReference.Schema.AH1_Reference] : ZString.Empty;
		}
	}
}
