using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty(Schema.PickSequence), DescriptionProperty(Schema.Description)]
	public class PickGroup : RegistryBusinessObjectTemplate, ICodeDescription
	{
		#region Schema

		public static class Schema
		{
			public const string PickSequence = "PickSequence";
			public const string Description = "Description";
		}

		#endregion

		#region Related Entities

		#region Parent

		PickGroupCollection Parent
		{
			get { return (PickGroupCollection)GetParentCollection(this, typeof(PickGroupCollection)); }
		}

		#endregion

		#endregion

		#region Properties

		#region PickSequence

		public ZShort PickSequence
		{
			get { return pickSequence; }
			set
			{
				SetNonPersistentPropertyValue(PickSequenceInfo, ref pickSequence, value);

				if (!IsValidationSuspended)
				{
					ValidatePickSequence();
				}
			}
		}

		public ZPropertyInfo PickSequenceInfo
		{
			get { return GetZPropertyInfo(Schema.PickSequence); }
		}

		ZShort pickSequence;

		#endregion

		#region Description

		[MaxLength(MaxDescriptionLength)]
		public MultilingualString Description
		{
			get { return description ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				CheckMaximumLength(DescriptionInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value, false);
				EnglishDescriptionInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		MultilingualString description;

		internal const int MaxDescriptionLength = 256;

		[MaxLength(MaxDescriptionLength)]
		public virtual ZString EnglishDescription
		{
			get { return Description.GetUnresolvedString(); }
			set { Description = (NoResString)value; }
		}

		public virtual ZPropertyInfo EnglishDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(EnglishDescription)); }
		}

		#endregion

		#endregion

		#region Validation

		#region ValidatePickSequence

		void ValidatePickSequence()
		{
			PickSequenceInfo.ClearAllNotifications();

			MandatoryValidation.CheckNotNegative(PickSequenceInfo);
			MandatoryValidation.CheckNotZero(PickSequenceInfo);

			CheckPickSequenceIsUnique();
		}

		void CheckPickSequenceIsUnique()
		{
			if (!PickSequenceInfo.HasErrors())
			{
				var parent = Parent;
				if (parent != null)
				{
					foreach (PickGroup pickGroup in parent)
					{
						if (pickGroup.PK != PK && pickGroup.PickSequence == PickSequence)
						{
							PickSequenceInfo.AddError(Res.GetString("dd5d2cfe-e7e3-496e-a73d-094c0abf6080", "Pick Sequence must be unique."));
						}
					}
				}
			}
		}

		#endregion

		#region ValidateDescription

		void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		#endregion

		#region RunPreSaveValidation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidatePickSequence();
			ValidateDescription();
		}

		#endregion

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var pickGroup = new PickGroup();
			pickGroup.PickSequence = PickSequence;
			pickGroup.Description = Description;

			return pickGroup;
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.PickSequence, PickSequence.ToString());
			writer.WriteElementString(Schema.Description, EnglishDescription);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PickSequence = reader.ReadElementStringAsZShort(Schema.PickSequence);
			EnglishDescription = reader.ReadElementString(Schema.Description);
		}

		#endregion

		#region ICodeDescription Members

		public string Code
		{
			get { return PickSequence.ToString(); }
		}

		#endregion
	}
}
