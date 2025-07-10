using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Dash.Business
{
	[XmlSerializerAssembly("Enterprise.Dash.Business.XmlSerializers")]
	public class WorkflowConfigurationStep : RegistryBusinessObjectTemplate, ICodeDescription
	{
		ZString code;

		public WorkflowConfigurationStep()
		{
		}

		[MaxLength(3)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetNonPersistentPropertyValue(CodeInfo, ref code, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		public ICodeDescriptionPairList Codes
		{
			get
			{
				var parentCollection = (WorkflowConfigurationStepCollection)GetParentCollection(this, typeof(WorkflowConfigurationStepCollection));
				return (parentCollection == null) ? null : parentCollection.Codes;
			}
		}

		[BusinessObjectTestExclude]
		public ZString Description
		{
			get
			{
				var codes = Codes;
				return (codes == null) ? string.Empty : codes.GetDescriptionFromCode(Code);
			}
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowConfigurationStep();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			code = reader.ReadElementString(CodeInfo.Name);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCode();
		}

		public void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CodeInfo);
			ListValidation.ErrorIfInvalidCode(CodeInfo, Codes);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(CodeInfo.Name, Code);
		}

		#region ICodeDescription

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		#endregion
	}
}
