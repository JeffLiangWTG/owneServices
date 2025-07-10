using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocDisbursementJobsCloseBatchLineCollection : DocumentWrapperCollection
	{
		public DocDisbursementJobsCloseBatchLineCollection(BusinessObject relatedObject, BusinessObjectFactory factory)
			: base(factory)
		{
			LoadCollection(relatedObject);
		}

		public new DocDisbursementJobsCloseBatchLine this[int index]
		{
			get { return (DocDisbursementJobsCloseBatchLine)base[index]; }
		}

		void LoadCollection(BusinessObject relatedObject)
		{
			var sql = $@"SELECT * FROM GetDsbJobCloseBatchDetails ('{relatedObject.PK}')";

			var coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load(sql);

			foreach (DynamicBusinessObject line in coll)
			{
				Add(DocDisbursementJobsCloseBatchLine.New(new DisbursementJobsCloseBatchLine(line), Factory));
			}
		}
	}
}
