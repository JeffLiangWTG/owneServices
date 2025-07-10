using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CodeDescriptionWithEnabledAndDefault : RegistryBusinessObject, IObsoleteValidation
	{
		#region Constructors

		public CodeDescriptionWithEnabledAndDefault()
		{
		}

		#endregion

		#region Properties

		public ZBool IsSystemDefined { get; set; }

		#region Code

		protected bool Code_ReadOnly
		{
			get { return IsSystemDefined; }
		}

		#endregion

		#region IsDefault

		public ZBool IsDefault
		{
			get { return isDefault; }
			set
			{
				SetNonPersistentPropertyValue(IsDefaultInfo, ref isDefault, value);

				if (!IsValidationSuspended)
				{
					ValidateIsDefault();
				}
			}
		}
		ZBool isDefault = false;

		public ZPropertyInfo IsDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.IsDefault); }
		}

		public virtual ZBool IsDefaultForBinding
		{
			get { return IsDefault; }
			set
			{
				try
				{
					using (GetValidationSuspender())
					{
						IsDefault = value;
						if (IsDefault)
						{
							foreach (var parentCollection in ParentCollections.OfType<CodeDescriptionWithEnabledAndDefaultCollection>())
							{
								foreach (CodeDescriptionWithEnabledAndDefault item in parentCollection)
								{
									if (item != this && item.IsDefault)
									{
										item.IsDefault = false;
									}
								}
							}
						}
					}
				}
				finally
				{
					ValidateIsDefault();
				}
			}
		}

		public ZWrappedPropertyInfo IsDefaultForBindingInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(IsDefaultForBinding), x => IsDefaultInfo); }
		}

		#endregion

		#region IsEnabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
				if (!IsValidationSuspended)
				{
					ValidateIsEnabled();
					ValidateIsDefault();
				}
			}
		}
		ZBool isEnabled = true;

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		#endregion

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !IsSystemDefined; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("67639e0f-1ab6-4517-b360-f2c87bcdf294", "This is system defined and cannot be deleted."); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateIsDefault();
			ValidateIsEnabled();
		}

		protected override void ValidateDescriptionCore()
		{
			base.ValidateDescriptionCore();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		protected virtual void ValidateIsDefault()
		{
			IsDefaultInfo.ClearAllNotifications();

			if (IsDefault)
			{
				if (!IsEnabled)
				{
					IsDefaultInfo.AddError(ResString.GetMultilingualString("43e9c3da-6fda-499d-8d17-471b5d8fa581", "Only enabled items can be the default."));
				}
				else
				{
					foreach (var parentCollection in ParentCollections.OfType<CodeDescriptionWithEnabledAndDefaultCollection>())
					{
						foreach (CodeDescriptionWithEnabledAndDefault item in parentCollection)
						{
							if (item != this && item.IsDefault)
							{
								IsDefaultInfo.AddError(ResString.GetMultilingualString("9402e81e-cf28-4f4e-a7bf-f0f4d1ac7edf", "There can only be one default."));
							}
						}
					}
				}
			}
		}

		void ValidateIsEnabled()
		{
			IsEnabledInfo.ClearAllNotifications();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(Schema.IsDefault, IsDefault.ToString());
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			IsDefault = new ZBool(reader.ReadElementString(Schema.IsDefault));
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
		}

		protected new class Schema : RegistryBusinessObject.Schema
		{
			public const string Name = "Name";
			public const string IsDefault = "IsDefault";
			public const string IsEnabled = "IsEnabled";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CodeDescriptionWithEnabledAndDefault();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var codeDescriptionClone = (CodeDescriptionWithEnabledAndDefault)clone;
			codeDescriptionClone.Code = Code;
			codeDescriptionClone.Description = Description;
			codeDescriptionClone.isDefault = isDefault;
			codeDescriptionClone.isEnabled = isEnabled;
			codeDescriptionClone.IsSystemDefined = IsSystemDefined;
		}

		#endregion
	}
}
