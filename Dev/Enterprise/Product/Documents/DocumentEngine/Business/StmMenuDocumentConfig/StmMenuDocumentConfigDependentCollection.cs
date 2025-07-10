using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuDocumentConfigDependentCollection : DependentBusinessObjectCollection<StmMenuDocumentConfig, StmMenuTemplatePivotBase>
	{
		public StmMenuDocumentConfigDependentCollection(StmMenuTemplatePivotBase master)
			: base(master)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		public void SortByFallback()
		{
			Sort(new StmMenuDocumentConfigComparer());
		}

		public MenuEditingMode EditingMode
		{
			get { return fEditingMode; }
			set
			{
				fEditingMode = value;
				foreach (StmMenuDocumentConfig item in this)
				{
					item.EditingMode = value;
				}
			}
		}

		MenuEditingMode fEditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		#region StmMenuDocumentConfigComparer

		class StmMenuDocumentConfigComparer : IComparer<StmMenuDocumentConfig>
		{
			public StmMenuDocumentConfigComparer()
			{
			}

			public int Compare(StmMenuDocumentConfig x, StmMenuDocumentConfig y)
			{
				int xOrder = GetFallbackLevelOrder(x);
				int yOrder = GetFallbackLevelOrder(y);
				int result = xOrder.CompareTo(yOrder);

				if (result == 0)
				{
					if (!x.S3_GC.IsEmpty)
					{
						result = x.Company.GC_Code.CompareTo(y.Company.GC_Code);
						if ((result == 0) && (!x.S3_OH.IsEmpty))
						{
							result = x.Header.OH_Code.CompareTo(y.Header.OH_Code);
						}
					}
					else if (!x.S3_OH.IsEmpty)
					{
						result = x.Header.OH_Code.CompareTo(y.Header.OH_Code);
					}
				}

				return result;
			}

			int GetFallbackLevelOrder(StmMenuDocumentConfig docConfig)
			{
				int result;
				if (docConfig.S3_IsSystem)
				{
					result = 0;
				}
				else if (docConfig.S3_GC.IsEmpty)
				{
					result = docConfig.S3_OH.IsEmpty ? 1 : 3;
				}
				else
				{
					result = docConfig.S3_OH.IsEmpty ? 2 : 4;
				}
				return result;
			}
		}

		#endregion
	}
}
