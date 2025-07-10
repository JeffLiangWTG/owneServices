using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class ShowEditNoteActionMethod : OperationalActionMethod
	{
		public ShowEditNoteActionMethod() : base(new ZGuid("5c4b7e39-b31a-4149-83d9-f07d6c556b9b")) { }

		public override string Name
		{
			get { return Res.GetString("e598659f-7ece-4c0b-95ec-210be4dc764e", "View/Edit Note"); }
		}

		public override string Description
		{
			get { return Res.GetString("0d9a0b8d-2200-49ee-b378-32058696b001", "Opens popup windows to view or edit a note record with specified description."); }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ShowEditNoteActionMethodApplicator((ShowEditNoteActionMethodSettings)settings, factory);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new ShowEditNoteActionMethodSettings();
		}

		public override IComponent NewSettingsControl()
		{
			return new ShowEditNoteActionMethodSettingsControl();
		}

		public override SecurityCheckpoint[] GetRequiredSecurityCheckpoints()
		{
			return new[] { Env.Security.NotesEdit };
		}

		public override bool RunWithoutUI
		{
			get { return true; }
		}
	}
}
