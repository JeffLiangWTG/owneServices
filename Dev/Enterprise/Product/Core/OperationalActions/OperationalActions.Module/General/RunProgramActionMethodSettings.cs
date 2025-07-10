using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Module
{
	public class RunProgramActionMethodSettings : OperationalActionMethodSettings
	{
		#region Schema

		public abstract class Schema
		{
			public const string Path = "Path";
			public const int PathMaxLength = 260;
			public const string Arguments = "Arguments";
			public const int ArgumentsMaxLength = 256;
		}

		#endregion

		#region Path

		[MaxLength(Schema.PathMaxLength)]
		public ZString Path
		{
			get { return path; }
			set
			{
				CheckMaximumLength(PathInfo, value);
				SetNonPersistentPropertyValue(PathInfo, ref path, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidatePath();
				}
			}
		}

		ZString path;

		public ZPropertyInfo PathInfo
		{
			get { return GetZPropertyInfo(Schema.Path); }
		}

		#endregion

		#region Arguments

		[MaxLength(Schema.ArgumentsMaxLength)]
		public ZString Arguments
		{
			get { return arguments; }
			set
			{
				CheckMaximumLength(ArgumentsInfo, value);
				SetNonPersistentPropertyValue(ArgumentsInfo, ref arguments, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateArguments();
				}
			}
		}

		ZString arguments;

		public ZPropertyInfo ArgumentsInfo
		{
			get { return GetZPropertyInfo(Schema.Arguments); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public RunProgramActionMethodSettingsValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected virtual RunProgramActionMethodSettingsValidation GetNewValidation()
		{
			return new RunProgramActionMethodSettingsValidation(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement(Schema.Path);
			writer.WriteValue(Path);
			writer.WriteEndElement();

			writer.WriteStartElement(Schema.Arguments);
			writer.WriteValue(Arguments);
			writer.WriteEndElement();
		}

		protected override void ReadXml(XmlReader reader)
		{
			reader.ReadStartElement();
			Path = reader.ReadElementString(Schema.Path);
			Arguments = reader.ReadElementString(Schema.Arguments);
			reader.ReadEndElement();
		}

		#endregion
	}
}
