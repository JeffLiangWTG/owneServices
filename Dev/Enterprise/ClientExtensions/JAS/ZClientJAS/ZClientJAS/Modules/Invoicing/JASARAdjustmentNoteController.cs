using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Module;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Client.JAS.GUI;

namespace Enterprise.Client.JAS.Module
{
	public class JASARAdjustmentNoteController : ARAdjustmentNoteController
	{
		protected override AdjustmentNoteForm GetNewAdjustmentNoteForm(ARAdjustmentNote adjustmentNote)
		{
			return new JASARAdjustmentNoteForm(adjustmentNote as JASARAdjustmentNote);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JASARAdjustmentNote); }
		}
	}
}
