using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Core.DialogDefault
{
	internal class DialogDefaultSaveOptions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public bool SaveNewDefaults { get; set; }

		public ZBool SaveForAllContexts { get; set; }

		#region KeepShowingDialog

		public bool KeepShowingDialog_ReadOnly { get { return !IsUserLevel && !SaveForMeAsWell; } }

		[BusinessObjectTestExclude]
		public ZBool KeepShowingDialog
		{
			get { return (!IsUserLevel && !SaveForMeAsWell) || keepShowingDialog; }
			set
			{
				SetNonPersistentPropertyValue(KeepShowingDialogInfo, ref keepShowingDialog, value);
			}
		}
		ZBool keepShowingDialog;

		public ZPropertyInfo KeepShowingDialogInfo
		{
			get { return GetZPropertyInfo(nameof(KeepShowingDialog)); }
		}

		#endregion

		#region SaveForMeAsWell

		public bool SaveForMeAsWell_ReadOnly { get { return IsUserLevel; } }

		ZBool saveForMeAsWell;
		public ZBool SaveForMeAsWell
		{
			get { return IsUserLevel || saveForMeAsWell; }
			set
			{
				SetNonPersistentPropertyValue(SaveForMeAsWellInfo, ref saveForMeAsWell, value);
			}
		}

		public ZPropertyInfo SaveForMeAsWellInfo
		{
			get { return GetZPropertyInfo(nameof(SaveForMeAsWell)); }
		}

		#endregion

		#region OverridePersonal 

		public bool AllowPersonal_ReadOnly { get { return OverridePersonal_ReadOnly; } }

		public ZBool AllowPersonal
		{
			get { return !OverridePersonal; }
			set { OverridePersonal = !value; }
		}

		public bool OverridePersonal_ReadOnly { get { return IsUserLevel; } }

		ZBool overridePersonal;
		public ZBool OverridePersonal
		{
			get { return IsUserLevel || overridePersonal; }
			set
			{
				SetNonPersistentPropertyValue(OverridePersonalInfo, ref overridePersonal, value);
			}
		}

		public ZPropertyInfo OverridePersonalInfo
		{
			get { return GetZPropertyInfo(nameof(OverridePersonal)); }
		}

		#endregion

		#region Level

		[List("LevelList")]
		[MaxLength(3)]
		public ZString Level
		{
			get { return level; }
			set
			{
				SetNonPersistentPropertyValue(LevelInfo, ref level, value);

				if (!IsValidationSuspended)
				{
					ListValidation.ErrorIfInvalidCode(LevelInfo);
				}
			}
		}
		ZString level;

		public ZPropertyInfo LevelInfo
		{
			get { return GetZPropertyInfo(nameof(Level)); }
		}

		public CodeDescriptionPairList LevelList
		{
			get { return DialogDefaultLevel.DialogDefaultLevelsForCurrentUser; }
		}

		#endregion

		#region Implementation

		public new DialogDefaultSaveOptions Clone()
		{
			return (DialogDefaultSaveOptions)base.Clone();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			return new DialogDefaultSaveOptions
			{
				SaveNewDefaults = SaveNewDefaults,
				SaveForAllContexts = SaveForAllContexts,
				Level = level,
				SaveForMeAsWell = saveForMeAsWell,
				OverridePersonal = overridePersonal,
				KeepShowingDialog = keepShowingDialog,
			};
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		public bool IsUserLevel { get { return Level == DialogDefaultLevel.Codes.User; } }
	}
}
