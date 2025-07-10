using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class NewConfiguration : NonPersistentBusinessObject
	{
		public NewConfiguration(ColumnConfigurationsManager manager)
		{
			this.Manager = manager;
			linkedField = manager.LinkedLookupField;
		}

		internal readonly ColumnConfigurationsManager Manager;
		public LookupField LinkedField
		{
			get { return linkedField; }
		}
		readonly LookupField linkedField;

		public NewConfigurationNameValidation Validation
		{
			get { return new NewConfigurationNameValidation(this); }
		}

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString NewName
		{
			get { return newName; }
			set
			{
				string newValue = value.Trim();
				CheckMaximumLength(NewNameInfo, newValue);
				SetNonPersistentPropertyValue<ZString>(NewNameInfo, ref newName, newValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNewName();
				}
			}
		}
		ZString newName;

		public ZPropertyInfo NewNameInfo
		{
			get { return GetZPropertyInfo(nameof(NewName), "New Configuration Name"); }
		}

		public CombinedConfigurationManager NewManager
		{
			get
			{
				this.ClearRowNotifications();
				if (LinkPK.IsValid)
				{
					ICodeDescription pKCodeDescription = LinkedField.GetCodeDescriptionForGUID(LinkPK);
					fNewManager = new CombinedConfigurationManager(Manager, LinkPK, pKCodeDescription.Code, pKCodeDescription.Description, NewName);
				}
				else
				{
					fNewManager = new CombinedConfigurationManager(Manager, NewName);
				}
				return fNewManager;
			}
		}
		CombinedConfigurationManager fNewManager;

		internal void ResetNewManager()
		{
			fNewManager = null;
		}

		ZGuid LinkPK
		{
			get { return LinkedField == null ? ZGuid.Empty : new ZGuid(LinkedField.Value); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}
	}

	public class NewConfigurationNameValidation : ZValidation
	{
		public NewConfigurationNameValidation(NewConfiguration parent)
			: base(parent)
		{
			this.parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(NewConfiguration); }
		}

		public override void ValidateAll()
		{
			ValidateNewName();
			validateNewManager();
		}

		public void ValidateNewName()
		{
			ValidateCalculatedProperty(parent.NewNameInfo);
		}

		public void validateNewManager()
		{
			if (this.parent.Manager.ConfigurationExists(this.parent.NewManager))
			{
				this.parent.ResetNewManager();
				this.parent.AddRowError(Res.GetString("ffac7864-1e9f-4988-bb56-062304124926", "A configuration exists for the chosen Description and or Link.  Please choose another link or description"));
			}
		}

		protected void CheckNewName()
		{
			MandatoryValidation.CheckEntered(parent.NewNameInfo, Res.GetString("7f93f31f-f202-4e7b-92ad-62eb0190b2d5", "New Configuration Name"));
		}
		readonly NewConfiguration parent;
	}
}
