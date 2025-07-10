using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class ExceptionKeyStacktraceDepth : RegistryBusinessObjectTemplate
	{
		public ExceptionKeyStacktraceDepth()
			: base()
		{
		}

		public ExceptionKeyStacktraceDepth(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ExceptionKeyStacktraceDepth(fallbackLevel, factory) { ExceptionType = ExceptionType, StackDepth = StackDepth };
		}

		#region Properties

		#region ExceptionType

		[ResourceStringData("ExceptionKeyStacktraceDepth.ExceptionType", Caption = "Exception Type", FullDescription = "The type of exception to limit stack trace depth. This should be the same as it appears next to Exception Type on Issue Manager occurrences.")]
		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString ExceptionType
		{
			get { return exceptionType; }
			set
			{
				CheckMaximumLength(ExceptionTypeInfo, value);
				SetNonPersistentPropertyValue(ExceptionTypeInfo, ref exceptionType, value);
				if (!IsValidationSuspended)
				{
					ValidateExceptionType();
				}
			}
		}
		ZString exceptionType;

		public ZPropertyInfo ExceptionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ExceptionType)); }
		}

		#endregion

		#region StackDepth

		[ResourceStringData("ExceptionKeyStacktraceDepth.StackDepth", Caption = "Stack Depth", FullDescription = "The number of stack frames to use when building keys for this exception type.")]
		public ZInt StackDepth
		{
			get { return stackDepth; }
			set
			{
				SetNonPersistentPropertyValue(StackDepthInfo, ref stackDepth, value);
				if (!IsValidationSuspended)
				{
					ValidateStackDepth();
				}
			}
		}
		ZInt stackDepth;

		public ZPropertyInfo StackDepthInfo
		{
			get { return GetZPropertyInfo(nameof(StackDepth)); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateStackDepth();
		}

		public void ValidateExceptionType()
		{
			ExceptionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ExceptionTypeInfo);
		}

		public void ValidateStackDepth()
		{
			StackDepthInfo.ClearAllNotifications();
			CompareValidation.CheckGreaterThanOrEqualTo(StackDepthInfo, 1m);
		}

		#endregion

		#region Xml

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ExceptionType = reader.ReadElementString(Schema.ExceptionType);
			StackDepth = reader.ReadElementStringAsZInt(Schema.StackDepth);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ExceptionType, ExceptionType);
			writer.WriteElementString(Schema.StackDepth, StackDepth.ToString());
		}

		static class Schema
		{
			internal const string ExceptionType = "ExceptionType";
			internal const string StackDepth = "StackDepth";
		}

		#endregion
	}
}

