using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public class SecurityFilterContainer : NonPersistentBusinessObject, IObsoleteValidation
	{
		CheckpointLookupKey lookupKey;

		public SecurityFilterContainer()
		{
			new SecurityVector().Initialise(Env.Security); // Make all LookupKeys loaded include all reports.
		}

		public SecurityFilterContainer(string customizedHumanReadableName)
			: this ()
		{
			CustomizedHumanReadableName = customizedHumanReadableName;
		}

		readonly string CustomizedHumanReadableName;

		protected override ZString HumanReadableNameCore => CustomizedHumanReadableName ?? base.HumanReadableNameCore;

		public CheckpointLookupKey LookupKey
		{
			get { return lookupKey; }
			set
			{
				if (lookupKey != value)
				{
					lookupKey = value;
					if (!IsValidationSuspended)
					{
						ValidateLookupKeyValidationProxy();
					}
					OnElementChanged();
				}
			}
		}

		// This is a dummy property used only to provide a PropertyInfo for notifications.
		public ZByte LookupKeyValidationProxy
		{
			get { return ZByte.Zero; }
		}

		public ZPropertyInfo LookupKeyValidationProxyInfo
		{
			get { return GetZPropertyInfo(Schema.LookupKeyValidationProxy); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateLookupKeyValidationProxy();
		}

		public void ValidateLookupKeyValidationProxy()
		{
			ValidateLookupKeyValidationProxyCore();
		}

		protected virtual void ValidateLookupKeyValidationProxyCore()
		{
			if (IsValidationSuspended)
			{
				ResumeValidation();
			}
			else
			{
				LookupKeyValidationProxyInfo.ClearAllNotifications();

				if (!LookupKey.IsEmpty && Env.Security.FindCheckPoint(LookupKey) == null)
				{
					LookupKeyValidationProxyInfo.AddError(Res.GetString("23decd7b-4e6b-4ff0-a57b-27bf506cac33", "Please select a valid Security Right."));
				}
			}
		}

		#region class Schema

		public static class Schema
		{
			public const string LookupKey = "LookupKey";
			public const string LookupKeyValidationProxy = "LookupKeyValidationProxy";
		}

		#endregion
	}
}
