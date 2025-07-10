using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyBizOWithAutoLogs : DummyEnterpriseBusinessObject
	{
		public DummyBizOWithAutoLogs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new void OnSaveRollback()
		{
			base.OnSaveRollback();
		}

		protected override EnterpriseBusinessObject.AutologState AutoLoggingState => EnterpriseBusinessObject.AutologState.AutoLogged;

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();
				result.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
				result.Add(PredefinedNoteTypes.Instance.OutturnNotes);
				result.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
				result.Add(PredefinedNoteTypes.Instance.PaymentHandlingInstructions);
				result.Add(PredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes);
				return result;
			}
		}

		#region CustomLogReferenceSuffix

		protected override ZString CustomLogReferenceSuffix
		{
			get { return fLogReferenceSuffix; }
		}
		ZString fLogReferenceSuffix;

		public void SetLogReferenceSuffix(ZString logReferenceSuffix)
		{
			fLogReferenceSuffix = logReferenceSuffix;
		}
		#endregion
	}
}
