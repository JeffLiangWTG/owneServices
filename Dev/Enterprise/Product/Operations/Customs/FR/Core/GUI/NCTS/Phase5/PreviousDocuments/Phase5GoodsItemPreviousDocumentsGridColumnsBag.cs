using System;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public class Phase5GoodsItemPreviousDocumentsGridColumnsBag
	{
		public Phase5GoodsItemPreviousDocumentsGridColumnsBag()
		{
			ReferenceNumberMultiControlColumn = new GridColumnReference<ZMultiControlColumnStyleInfo>(NctsPreviousDocument.Schema.CSI_ReferenceNumber, 200,
			c =>
			{
				c.FieldTypeColumnName = "ReferenceNumberFieldType";
			});
		}

		public static Phase5GoodsItemPreviousDocumentsGridColumnsBag Instance => instance ??= new Phase5GoodsItemPreviousDocumentsGridColumnsBag();

		public IGridColumnReference ReferenceNumberMultiControlColumn { get; }

		[ThreadStatic]
		static Phase5GoodsItemPreviousDocumentsGridColumnsBag instance;
	}
}
