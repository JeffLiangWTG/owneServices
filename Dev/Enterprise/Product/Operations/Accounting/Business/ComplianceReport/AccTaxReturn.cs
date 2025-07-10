using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	[ReadOnly(true)]
	public class AccTaxReturn : AutoAccTaxReturn, ICanApplyDataRefresh
	{
		public AccTaxReturn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATR_Version), ConcurrencyPolicy.Strict);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ATR_ReturnType = ReturnType.MTD;
			ATR_Status = Status.Saved;
			ATR_Version = 0;
		}

		[RelatedBusinessObject("ComplianceReport")]
		public override ZGuid ATR_ACR_ComplianceReport
		{
			get => base.ATR_ACR_ComplianceReport;
			set => base.ATR_ACR_ComplianceReport = value;
		}

		public virtual AccComplianceReport ComplianceReport
		{
			get { return Factory.Load<AccComplianceReport>(ATR_ACR_ComplianceReport); }
		}

		[ResourceStringData("AccTaxReturn|ATR_Status", Caption = "Status")]
		[List(("Lookups.TaxReturnStatusList"))]
		[ReadOnly(true)]
		public override ZString ATR_Status { get => base.ATR_Status; set => base.ATR_Status = value; }

		[ResourceStringData("AccTaxReturn|ATR_Comment", Caption = "Comment")]
		public override ZString ATR_Comment { get => base.ATR_Comment; set => base.ATR_Comment = value; }

		[ChildEditable(true)]
		public AccTaxReturnColumnCollection Columns
		{
			get
			{
				if (columns == null)
				{
					columns = new AccTaxReturnColumnCollection(this);
					RegisterEditableChildObject(columns);
					columns.Load();
				}
				return columns;
			}
		}
		AccTaxReturnColumnCollection columns;

		[ChildEditable(true)]
		public AccTaxReturnLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new AccTaxReturnLineCollection(this);
					RegisterEditableChildObject(lines);
					lines.Load();
				}
				return lines;
			}
		}
		AccTaxReturnLineCollection lines;

		public bool IsSaved => ATR_Status == Status.Saved;
		public bool IsGenerated => ATR_Status == Status.Generated;
		public bool IsSubmitted => ATR_Status == Status.Submitted;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public bool CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher) => false;

		public static class ReturnType
		{
			public const string MTD = "MTD";
			public const string TPAR = "TPA";
			public const string PTRS = "PTR";
			public const string LIQ = "LIQ";
		}

		public static class Status
		{
			public const string Saved = "SAV";
			public const string Generated = "GEN";
			public const string Submitted = "SUB";
		}

		protected override void OnFactorySaving()
		{
			if ((ATR_ReturnType == ReturnType.MTD || ATR_ReturnType == ReturnType.LIQ) && HasChanges)
			{
				ATR_Version = (ZInt)ATR_VersionInfo.OriginalValue + 1;
			}

			base.OnFactorySaving();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (ATR_Status.IsEmpty)
			{
				ATR_Status = Status.Saved;
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				Columns.RemoveAndDeleteAll();
				base.Delete();
			}
		}

#if DEBUG
		public void ClearReportColumns_ForTestOnly()
		{
			columns = null;
		}
#endif
	}
}
