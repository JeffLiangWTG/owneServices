using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[CodeProperty(AccApportionmentTemplate.Schema.A0_Description), DescriptionProperty(AccApportionmentTemplate.Schema.A0_Notes)]
	public class AccApportionmentTemplate : AutoAccApportionmentTemplate
	{
		public AccApportionmentTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region Lines

		[ChildEditable()]
		public AccApportionmentTemplateLinesCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new AccApportionmentTemplateLinesCollection(this, Factory);
					if (IsInDatabase)
					{
						fLines.Load(new ZQuery(AccApportionmentTemplateLinesSchema.Y0_A0, SQLComparisonOperator.Equal, PK));
					}
					RegisterEditableChildObject(fLines);
				}
				return fLines;
			}
		}
		AccApportionmentTemplateLinesCollection fLines;

		#endregion

		#region PercentageTotal

		[DecimalPlaces(3)]
		public ZDecimal PercentageTotal
		{
			get
			{
				ZDecimal total = 0.0M;
				foreach (AccApportionmentTemplateLines line in Lines)
				{
					total += line.Y0_Percentage;
				}
				return total;
			}
		}

		public ZPropertyInfo PercentageTotalInfo
		{
			get { return GetZPropertyInfo(nameof(PercentageTotal)); }
		}

		#endregion

		#region DefaultDescription

		public ZString DefaultDescription
		{
			get
			{
				if (Lines.Count > 0)
				{
					return Lines[Lines.Count - 1].Y0_Description;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		public bool A0_GC_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (A0_GC == ZGuid.Empty)
			{
				A0_GC = GlbCompany.CurrentCompany.PK;
			}
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
