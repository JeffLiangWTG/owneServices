using System.Xml;

using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class ShowEditNoteActionMethodSettings : OperationalActionMethodSettings
	{
		#region Schema

		public abstract class Schema
		{
			public const string NoteDescription = "NoteDescription";
			public const int NoteDescriptionMaxLength = 50;
		}

		#endregion

		#region NoteDescription

		[CargoWise.ComponentModel.MaxLength(Schema.NoteDescriptionMaxLength)]
		public ZString NoteDescription
		{
			get { return noteDescription; }
			set
			{
				CheckMaximumLength(NoteDescriptionInfo, value);
				SetNonPersistentPropertyValue(NoteDescriptionInfo, ref noteDescription, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNoteDescription();
				}
			}
		}

		ZString noteDescription;

		public ZPropertyInfo NoteDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.NoteDescription); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public ShowEditNoteActionMethodSettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual ShowEditNoteActionMethodSettingsValidation GetNewValidation()
		{
			return new ShowEditNoteActionMethodSettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.NoteDescription);
			writer.WriteValue(NoteDescription);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			NoteDescription = reader.ReadElementString(Schema.NoteDescription);
			reader.ReadEndElement();
		}

		#endregion
	}
}
