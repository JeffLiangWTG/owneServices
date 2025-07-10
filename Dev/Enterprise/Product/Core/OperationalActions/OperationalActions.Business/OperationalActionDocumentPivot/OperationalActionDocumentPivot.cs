using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public class OperationalActionDocumentPivot : AutoStmMenuMenuPivot, IOperationalActionMenuEditable
	{
		public OperationalActionDocumentPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		public MenuEditingMode EditingMode { get; set; }

		#region Related Business Objects

		public OperationalAction Action
		{
			get { return Factory.Load<OperationalAction>(SF_SU_Inward); }
		}

		public DocumentCommand Document
		{
			get { return (DocumentCommand)Outward; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override StmMenuItem Inward
		{
			get { throw new InvalidOperationException("Please use the Action property instead."); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override StmMenuItem Outward
		{
			get { return Factory.Load<DocumentCommand>(SF_SU_Outward); }
		}

		#endregion

		#region Binding Properties

		[RelatedBusinessObject("Action")]
		public override ZGuid SF_SU_Inward
		{
			get { return base.SF_SU_Inward; }
			set { base.SF_SU_Inward = value; }
		}

		protected bool SF_IsSystemDefined_ReadOnly
		{
			get { return !OperationalActionMenuEditableHelper.CanEdit(this); }
		}

		#endregion

		#region Strategy Objects

		public new OperationalActionDocumentPivotValidation Validation
		{
			get { return (OperationalActionDocumentPivotValidation)base.Validation; }
		}
		protected override StmMenuMenuPivotValidation GetNewValidation()
		{
			return new OperationalActionDocumentPivotValidation(this);
		}

		public new OperationalActionDocumentPivotLookups Lookups
		{
			get { return (OperationalActionDocumentPivotLookups)base.Lookups; }
		}
		protected override StmMenuMenuPivotLookups GetNewLookups()
		{
			return new OperationalActionDocumentPivotLookups(this);
		}

		#endregion

		#region BusinessObject Overrides

		public override bool ReadOnly
		{
			get { return base.ReadOnly || OperationalActionMenuEditableHelper.ReadOnly(this); }
			set { base.ReadOnly = value; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
		}

		#endregion

		#region IOperationalActionMenuEditable Members

		ZPropertyInfo IOperationalActionMenuEditable.SystemDefinedInfo
		{
			get { return SF_IsSystemDefinedInfo; }
		}

		#endregion
	}
}
