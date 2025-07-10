using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RestrictedTaskTypes : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string WorkflowType = "WorkflowType";
			public const string TaskTypeList = "TaskTypeList";
		}

		#endregion

		public RestrictedTaskTypes()
		{
		}

		public RestrictedTaskTypes(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Lookup

		public CodeDescriptionPairList TaskTypeList
		{
			get { return WorkflowDataRegistryHelper.GetTaskTypeList(WorkflowType); }
		}

		#endregion

		#region RegistryBusinessObject Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RestrictedTaskTypes();
		}

		[List(Schema.TaskTypeList)]
		[ResourceStringData("654A5236-867D-4BCF-9B70-8B7ED94826EE", Caption = "Task Type")]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("C866600E-3A56-4C60-ADF4-24BD97579AF4", Caption = "Type Description")]
		public override ZString EnglishDescription
		{
			get { return base.EnglishDescription; }
			set { base.EnglishDescription = value; }
		}

		protected override string CodeDisplayName
		{
			get { return (NoResString)"Task Type"; }
		}

		public override MultilingualString Description
		{
			get { return (NoResString)TaskTypeList.GetDescriptionFromCode(Code); }
		}

		protected override int MaxDescriptionLength => 256;

		#endregion

		#region WorkflowType

		[ResourceStringData("391399A9-84B2-4955-83BC-4E7413D52EDB", Caption = "Workflow Type")]
		public ZString WorkflowType
		{
			get { return workflowType; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(WorkflowTypeInfo, ref workflowType, value);
			}
		}

		public ZPropertyInfo WorkflowTypeInfo
		{
			get { return GetZPropertyInfo(nameof(WorkflowType)); }
		}

		ZString workflowType;

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			if (reader.IsStartElement(Schema.WorkflowType))
			{
				WorkflowType = reader.ReadElementString(Schema.WorkflowType);
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.WorkflowType, WorkflowType);
		}

		#endregion

		#region Validation

		protected override void ValidateCodeCore()
		{
			CodeInfo.HumanReadableName = Res.GetString("741672bd-89e2-41d7-bad1-4d6ccf575d76", "Task Type");

			base.ValidateCodeCore();

			if (!TaskTypeList.ContainsCode(Code))
			{
				CodeInfo.AddError(EnterValidSelectionErrorMessage);
			}
		}

		#endregion
	}
}
