using System;
using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.DocumentEngine.GUI
{
	public abstract class DocumentPlugin : ZAlwaysLoadPlugIn
	{
		protected DocumentPlugin(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			var collection = hostBusinessEntity as IBusinessObjectCollection;
			if (collection != null)
			{
				if (collection.Count > 0)
				{
					inputBusinessObject = (BusinessObject)((IList)collection)[0];
				}
			}
			else
			{
				inputBusinessObject = (BusinessObject)hostBusinessEntity;
			}
		}

		protected BusinessObject InputBusinessObject
		{
			get { return inputBusinessObject; }
		}
		readonly BusinessObject inputBusinessObject;

#if DEBUG
		virtual
#endif
 protected DocumentNote Note
		{
			get
			{
				if (note == null && InputBusinessObject is IStmNoteParent)
				{
					try
					{
						note = DocumentNote.LoadNote((IStmNoteParent)InputBusinessObject);
						note.HasChangesChanged += Note_HasChangesChanged;
					}
					catch (IOException)
					{
						NotifyOperatingSystemMutexWentWrong();
					}
					catch (UnauthorizedAccessException)
					{
						NotifyOperatingSystemMutexWentWrong();
					}
				}
				return note;
			}
		}
		DocumentNote note;

		protected virtual void Note_HasChangesChanged(object sender, EventArgs e)
		{
		}

		void NotifyOperatingSystemMutexWentWrong()
		{
			Globals.Message.ShowError(Res.GetString("2fd6552f-b67e-4644-896a-13f74ccdc6a2", "An error occurred when loading the plug-in; another user may have tried to load it simultaneously. Please close the form and try again."));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (note != null)
				{
					note.HasChangesChanged -= Note_HasChangesChanged;
				}
			}

			base.Dispose(disposing);
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.AlwaysAllow; }
		}
	}
}
