using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARAdjustmentNoteController : InvoicingBaseController
	{
		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
		{
			return GetNewAdjustmentNoteForm(businessEntity as ARAdjustmentNote);
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARAdjustmentNote; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARAdjustmentNote); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesAdjustmentNote; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesAdjustmentNote; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ViewReceivablesTransaction; }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.ARTransaction;
			}
		}

		protected virtual GUI.AdjustmentNoteForm GetNewAdjustmentNoteForm(ARAdjustmentNote adjustmentNote)
		{
			return new GUI.AdjustmentNoteForm(adjustmentNote);
		}
	}
}
