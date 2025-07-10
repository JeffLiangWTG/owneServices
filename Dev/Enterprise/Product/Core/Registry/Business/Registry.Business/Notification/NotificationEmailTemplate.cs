using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class NotificationEmailTemplate : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string RawEmailBody = "RawEmailBody";
			public const string RawEmailSubject = "RawEmailSubject";
			public const string EmailBody = "EmailBody";
			public const string EmailSubject = "EmailSubject";
			public const string ShouldHideEmailBody = "ShouldHideEmailBody";
			public const string EnglishEmailBody = "EnglishEmailBody";
			public const string EnglishEmailSubject = "EnglishEmailSubject";
		}

		#endregion

		public NotificationEmailTemplate()
		{
		}

		public NotificationEmailTemplate(Type docSourceType)
			: this(docSourceType, "")
		{
		}

		public NotificationEmailTemplate(Type docSourceType, ZString defaultEmailBody)
			: this(docSourceType, "", defaultEmailBody)
		{
		}

		public NotificationEmailTemplate(Type docSourceType, ZString defaultEmailSubject, ZString defaultEmailBody, bool shouldHideEmailBody = false)
		{
			DocSourceType = docSourceType;
			EmailSubject = defaultEmailSubject;
			EmailBody = defaultEmailBody;
			ShouldHideEmailBody = shouldHideEmailBody;
		}

		public ZBool ShouldHideEmailBody { get; private set; }

		#region EmailSubject

		internal MultilingualString RawEmailSubject
		{
			get { return rawEmailSubject ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				rawEmailSubject = value;
				RawEmailSubjectInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RawEmailSubjectInfo
		{
			get { return GetZPropertyInfo(Schema.RawEmailSubject); }
		}

		MultilingualString rawEmailSubject;

		[MaxLength(256)]
		public ZString EmailSubject
		{
			get { return RawEmailSubject.ToString(); }
			set
			{
				CheckMaximumLength(EmailSubjectInfo, value);
				RawEmailSubject = (NoResString)value;
				EmailSubjectInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EmailSubjectInfo
		{
			get { return GetZPropertyInfo(Schema.EmailSubject); }
		}

		[MaxLength(256)]
		public ZString EnglishEmailSubject => RawEmailSubject.GetUnresolvedString();

		public ZPropertyInfo EnglishEmailSubjectInfo => GetZPropertyInfo(Schema.EnglishEmailSubject);

		#endregion

		#region EmailBody

		internal MultilingualString RawEmailBody
		{
			get { return rawEmailBody ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				rawEmailBody = value;
				RawEmailBodyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RawEmailBodyInfo
		{
			get { return GetZPropertyInfo(Schema.RawEmailBody); }
		}

		MultilingualString rawEmailBody;

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString EmailBody
		{
			get { return RawEmailBody.ToString(); }
			set
			{
				RawEmailBody = (NoResString)value;
				EmailBodyInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EmailBodyInfo
		{
			get { return GetZPropertyInfo(Schema.EmailBody); }
		}

		[MaxLength(StmNote.Schema.ST_NoteTextMaxLength)]
		public ZString EnglishEmailBody => RawEmailBody.GetUnresolvedString();

		public ZPropertyInfo EnglishEmailBodyInfo => GetZPropertyInfo(Schema.EnglishEmailBody);

		#endregion

		public IDocumentFieldDefinitionCollection DocumentFields
		{
			get { return documentFields ?? (documentFields = ObjectFactory.Get<IDocumentFieldAttributeFinder>().FindProperties(DocSourceType)); }
		}

		public Type DocSourceType { get; set; }
		IDocumentFieldDefinitionCollection documentFields;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NotificationEmailTemplate();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			NotificationEmailTemplate cloneObject = (NotificationEmailTemplate)clone;
			cloneObject.DocSourceType = DocSourceType;
			cloneObject.EmailSubject = RawEmailSubject;
			cloneObject.EmailBody = RawEmailBody;
			cloneObject.ShouldHideEmailBody = ShouldHideEmailBody;
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.EmailSubject, EmailSubject);
			writer.WriteElementString(Schema.EmailBody, EmailBody);
			writer.WriteElementString(Schema.ShouldHideEmailBody, ShouldHideEmailBody.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EmailSubject = new ZString(reader.ReadElementString(Schema.EmailSubject));
			EmailBody = new ZString(reader.ReadElementString(Schema.EmailBody));
			ShouldHideEmailBody = reader.ReadElementStringAsZBool(Schema.ShouldHideEmailBody);
		}

		#endregion
	}
}
