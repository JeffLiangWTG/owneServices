using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Module
{
	public partial class ForeignOperatorFilterStripControl : ZFilterStripControl
	{
		public ForeignOperatorFilterStripControl()
		{
			InitializeComponent();
		}

		public ForeignOperatorFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			AddGridColumns();
		}

		void AddGridColumns()
		{
			grid.ReOrderColumns(DefaultColumnsForGrid);
		}

		string[] defaultColumnsForGrid;

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					var columnsList = new List<string>
					{
						CusBRForeignOperator.Schema.BFR_AuthorityIdentifier,
						CusBRForeignOperator.Schema.BFR_AuthorityVersion,
						CusBRForeignOperator.Schema.BFR_CustomsStatus,
						CusBRForeignOperator.Schema.BFR_OH_Owner,
						CusBRForeignOperator.Schema.ForeignOperatorName,
						CusBRForeignOperator.Schema.ForeignOperatorCountry,
						CusBRForeignOperator.Schema.BFR_MessageStatus
					};

					defaultColumnsForGrid = columnsList.ToArray();
				}
				return defaultColumnsForGrid;
			}
		}
	}
}
