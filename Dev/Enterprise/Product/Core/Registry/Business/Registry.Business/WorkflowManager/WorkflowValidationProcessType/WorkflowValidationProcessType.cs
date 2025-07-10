using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WorkflowValidationProcessType : AutoAutoWorkflowValidationProcessType
	{
		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowValidationProcessType();
		}

		#endregion

		#region Properties

		[List(nameof(Lookups) + "." + nameof(WorkflowValidationProcessTypeLookups.ProcessTypeList))]
		public override ZString ProcessType
		{
			get { return base.ProcessType; }
			set { base.ProcessType = value; }
		}

		public ZString ProcessTypeDescription => Lookups.ProcessTypeList.GetDescriptionFromCode(ProcessType);

		public WorkflowValidationProcessTypeLookups Lookups => fLookups ?? (fLookups = new WorkflowValidationProcessTypeLookups(this));
		WorkflowValidationProcessTypeLookups fLookups;

		public override void ValidateProcessType()
		{
			base.ValidateProcessType();
			ListValidation.ErrorIfInvalidCode(ProcessTypeInfo);
			if (!ProcessTypeInfo.HasErrors())
			{
				var parentCollection = GetParentCollection(this, typeof(WorkflowValidationProcessTypeCollection));
				if (parentCollection != null && parentCollection.Cast<WorkflowValidationProcessType>().Any((WorkflowValidationProcessType x) => x.ProcessType == ProcessType && x != this))
				{
					ProcessTypeInfo.AddError(DuplicatedCodesError);
				}
			}
		}

		internal static string DuplicatedCodesError => ResString.GetMultilingualString("D6419015-0A99-47AB-9C49-F3ED3512FA22", "The codes cannot be duplicated.");

		#endregion
	}
}
