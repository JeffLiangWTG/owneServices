using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.UniversalCopy;

namespace Enterprise.UniversalCopy.Business
{
	[System.Diagnostics.DebuggerDisplay("{GetType().FullName} Description = {Description}")]
	public abstract class CopyTemplateNodeBizo : NonPersistentBusinessObject
	{
		protected CopyTemplateNodeBizo(CopyTemplateNode copyTemplateNode)
		{
			CopyTemplateNode = copyTemplateNode;
		}

		public CopyTemplateNode CopyTemplateNode { get; private set; }

		#region Properties

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore => GetFriendlyName(CopyTemplateNode.Name);

		#endregion

		#region Name

		public ZString Name
		{
			get { return CopyTemplateNode.Name; }
		}

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		#endregion

		#region Description

		[BusinessObjectTestExclude]
		public ZString Description
		{
			get
			{
				var description = CopyTemplateNode.Description;
				if (string.IsNullOrEmpty(description))
				{
					description = GetFriendlyName(CopyTemplateNode.Name);
				}
				return description;
			}
			set
			{
				if (CopyTemplateNode.Description != value)
				{
					HasChanges = true;
					CopyTemplateNode.Description = value;
				}
				DescriptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		protected virtual string GetFriendlyName(string name)
		{
			return string.IsNullOrEmpty(name) ? name : ZPropertyInfo.GetFriendlyColumnNameShared(name);
		}

		#endregion

		#endregion

		#region Defaulting

		public void DefaultToDoNotCopy()
		{
			DefaultToDoNotCopyCore();
		}
		protected abstract void DefaultToDoNotCopyCore();

		public void DefaultToCopy(int level = 0)
		{
			DefaultToCopyCore(level);
		}
		protected abstract void DefaultToCopyCore(int level);

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		public CopyTemplateNodeBizoValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected abstract CopyTemplateNodeBizoValidation GetNewValidation();

		#endregion
	}

	#region Validation Class

	public abstract class CopyTemplateNodeBizoValidation : ZValidation
	{
		protected CopyTemplateNodeBizoValidation(CopyTemplateNodeBizo parent)
			: base(parent)
		{
			Parent = parent;
			ZValidationInternals = this;
		}

		protected CopyTemplateNodeBizo Parent { get; private set; }
		protected readonly IValidationInternals ZValidationInternals;

		public override void ValidateAll()
		{
			ValidateName();
		}

		public override Type AutoValidationType
		{
			get { return typeof(CopyTemplateNodeBizo); }
		}

		#region Validate Properties

		public void ValidateName() { }

		#endregion
	}

	#endregion
}
