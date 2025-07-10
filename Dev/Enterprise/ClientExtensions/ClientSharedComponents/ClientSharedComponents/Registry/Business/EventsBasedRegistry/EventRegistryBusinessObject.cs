using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class EventRegistryBusinessObject : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
			public const string Reference = "Reference";
		}

		#endregion

		public EventRegistryBusinessObject()
		{
		}

		#region Code

		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				code = value;

				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
				CodeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}
		ZString code;

		#endregion

		#region Description

		[MaxLength(250)]
		public ZString Description
		{
			get { return EventsCodeDescriptionPairList.GetDescriptionFromCode(Code); }
		}
		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region Reference

		[MaxLength(StmALog.Schema.SL_ReferenceMaxLength)]
		public ZString Reference
		{
			get { return reference; }
			set
			{
				CheckMaximumLength(ReferenceInfo, value);
				reference = value;
				ReferenceInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.Reference); }
		}
		ZString reference;

		#endregion

		#region LookUps

		public CodeDescriptionPairList EventsCodeDescriptionPairList
		{
			get
			{
				if (eventsCodeDescriptionPairList == null)
				{
					eventsCodeDescriptionPairList = new CodeDescriptionPairList();
					foreach (Event eventLog in Events.All)
					{
						eventsCodeDescriptionPairList.AddPair(eventLog.Code, eventLog.Description);
					}
					eventsCodeDescriptionPairList.Sort();
				}
				return eventsCodeDescriptionPairList;
			}
		}
		CodeDescriptionPairList eventsCodeDescriptionPairList;

		#endregion

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			base.RunPreSaveValidationCore();
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			if (Code.Trim().IsEmpty)
			{
				MandatoryValidation.CheckEntered(CodeInfo);
			}
			if (!CodeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CodeInfo, EventsCodeDescriptionPairList);
			}
		}

		#endregion

		#region Write/Read XML

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Code, Code);
			writer.WriteElementString(Schema.Reference, Reference);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Code = reader.ReadElementString(Schema.Code);
			Reference = reader.ReadElementString(Schema.Reference);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EventRegistryBusinessObject();
		}

		#endregion
	}
}
