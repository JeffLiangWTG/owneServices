using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class OpenURLActionMethodSettings : OperationalActionMethodSettings
	{
		#region Schema

		public abstract class Schema
		{
			public const string URL = "URL";
			public const int URLMaxLength = 2000;
		}

		#endregion

		#region URL

		[MaxLength(Schema.URLMaxLength)]
		public ZString URL
		{
			get { return url; }
			set
			{
				CheckMaximumLength(URLInfo, value);
				SetNonPersistentPropertyValue(URLInfo, ref url, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateURL();
				}
			}
		}

		ZString url;

		public ZPropertyInfo URLInfo
		{
			get { return GetZPropertyInfo(Schema.URL); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public OpenURLActionMethodSettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual OpenURLActionMethodSettingsValidation GetNewValidation()
		{
			return new OpenURLActionMethodSettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.URL);
			writer.WriteValue(URL);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			URL = reader.ReadElementString(Schema.URL);
			reader.ReadEndElement();
		}

		#endregion
	}
}
