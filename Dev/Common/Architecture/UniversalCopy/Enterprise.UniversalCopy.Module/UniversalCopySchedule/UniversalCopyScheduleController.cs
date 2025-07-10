using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.UniversalCopy.Module
{
	public class UniversalCopyScheduleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public UniversalCopyScheduleController()
		{ }

		public UniversalCopyScheduleController(StmUniversalCopy item)
		{
			this.item = item;
		}

		public override ControllerID ID
		{
			get
			{
				return ControllerIDs.UniversalCopySchedule;
			}
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get
			{
				return typeof(StmUniversalCopy);
			}
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new UniversalCopyScheduleForm(((StmUniversalCopy)businessEntity).ScheduleTask);
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.UniversalCopySchedule;
			}
		}

		#region Security

		UniversalCopySecurity Security
		{
			get
			{
				if (security == null)
				{
					if (item == null)
					{
						throw new InvalidOperationException("Copy item must be passed to UniversalCopyScheduleController");
					}
					var moduleId = item.Template.GetModuleIdentifier();
					security = new UniversalCopySecurity(moduleId);
				}
				return security;
			}
		}
		UniversalCopySecurity security;

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			using (ForBusinessObject(bizObject))
			{
				return base.GetCheckPointForView(bizObject);
			}
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return (SecurityCheckpoint)Security.UniversalCopyCEDPrivateCheckpoint ?? Env.Security.None;
			}
		}

		public override SecurityCheckpoint GetCheckPointForNew(BusinessObject bizObject)
		{
			using (ForBusinessObject(bizObject))
			{
				return base.GetCheckPointForNew(bizObject);
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get
			{
				throw new InvalidOperationException("New is not supported");
			}
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			using (ForBusinessObject(bizObject))
			{
				return base.GetCheckPointForEdit(bizObject);
			}
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get
			{
				return (SecurityCheckpoint)Security.UniversalCopyCEDPrivateCheckpoint ?? Env.Security.None;
			}
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			using (ForBusinessObject(bizObject))
			{
				return base.GetCheckPointForDelete(bizObject);
			}
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get
			{
				return (SecurityCheckpoint)Security.UniversalCopyCEDPrivateCheckpoint ?? Env.Security.None;
			}
		}

		#endregion

		#region Implementation

		StmUniversalCopy item;

		IDisposable ForBusinessObject(BusinessObject bizo)
		{
			var newItem = bizo as StmUniversalCopy;
			if (newItem != null && newItem != item && (item == null || newItem.GetType() != item.GetType()))
			{
				var originalItam = item;
				item = newItem;
				var originalSecurity = security;
				security = null;

				return new DisposableAction(() =>
				{
					item = originalItam;
					security = originalSecurity;
				});
			}
			return null;
		}

		#endregion
	}
}
