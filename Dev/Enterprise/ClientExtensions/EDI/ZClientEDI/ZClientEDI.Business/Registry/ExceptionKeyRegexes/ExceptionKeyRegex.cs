using System;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ExceptionKeyRegex : RegistryBusinessObjectTemplate
	{
		protected abstract class Schema
		{
			public const string Regex = "Regex";
			public const string Description = "Description";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExceptionKeyRegex(fallbackLevel, factory);
		}

		public ExceptionKeyRegex(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public ExceptionKeyRegex()
			: base() { }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRegex();
		}

		#region Regex

		[CargoWise.ComponentModel.MaxLength(160)]
		public ZString Regex
		{
			get { return regex; }
			set
			{
				CheckMaximumLength(RegexInfo, value);
				SetNonPersistentPropertyValue(RegexInfo, ref regex, value);

				if (!IsValidationSuspended)
				{
					ValidateRegex();
				}
			}
		}
		ZString regex;

		public ZPropertyInfo RegexInfo
		{
			get { return GetZPropertyInfo(Schema.Regex); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public void ValidateRegex()
		{
			RegexInfo.ClearAllNotifications();

			if (Regex.IsEmpty)
			{
				RegexInfo.AddError(ErrorMustHaveRegex);
				return;
			}

			try
			{
				new Regex(Regex);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				RegexInfo.AddError(InvalidRegexPattern);
			}
		}

		public static string ErrorMustHaveRegex
		{
			get { return Res.GetString("285A39F6-929A-400C-BBFB-BAC7C76370F7", "Should have Regular Expression."); }
		}

		public static string InvalidRegexPattern
		{
			get { return Res.GetString("745BED24-18EB-4CED-B26B-4BC376D712A3", "Invalid Regular Expression."); }
		}

		#endregion

		#region Descripton

		[CargoWise.ComponentModel.MaxLength(160)]
		public ZString Description
		{
			get { return description; }
			set
			{
				CheckMaximumLength(RegexInfo, value);
				SetNonPersistentPropertyValue(DescriptionInfo, ref description, value);
			}
		}
		ZString description;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region XML Reading and Writing

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Regex, Regex);
			writer.WriteElementString(Schema.Description, Description);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			Regex = reader.ReadElementString(Schema.Regex);
			Description = reader.ReadElementString(Schema.Description);
		}

		#endregion
	}
}


