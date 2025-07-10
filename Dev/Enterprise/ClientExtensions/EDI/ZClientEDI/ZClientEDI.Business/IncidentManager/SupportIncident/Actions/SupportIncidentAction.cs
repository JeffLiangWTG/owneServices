using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public abstract class SupportIncidentAction : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected SupportIncidentAction(SupportIncident incident)
			: base(incident.Factory)
		{
			this.Incident = incident;
		}

		protected SupportIncidentAction()
		{
		}

		public SupportIncident Incident { get; protected set; }

		public bool NeedNotifyInternal { get; set; }

		#region Incident Synchronisation

		public bool SynchroniseToIncident()
		{
			RunPreSaveValidation();
			if (HasErrors)
			{
				return false;
			}
			else
			{
				PerformAction();
				return true;
			}
		}

		protected virtual void PerformAction()
		{
		}

		#endregion

		#region Comment

		[MaxLength(32000)]
		public virtual ZString Comment
		{
			get { return comment; }
			set
			{
				if (comment != value)
				{
					SetNonPersistentPropertyValue(CommentInfo, ref comment, value);
					if (!IsValidationSuspended)
					{
						ValidateComment();
					}
				}
			}
		}
		ZString comment;

		public ZPropertyInfo CommentInfo
		{
			get { return GetZPropertyInfo(nameof(Comment)); }
		}

		#endregion

		#region Set eRequest Status Only

		public ZBool SetERequestStatusOnly
		{
			get { return setERequestStatusOnly; }
			set
			{
				setERequestStatusOnly = value;
				Incident.SetERequestStatusOnly = value;
				SetERequestStatusOnlyInfo.RefreshBinding();
			}
		}
		ZBool setERequestStatusOnly;

		public ZPropertyInfo SetERequestStatusOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(SetERequestStatusOnly)); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateComment();
		}

		protected virtual void ValidateComment()
		{
		}

		#endregion
	}
}

