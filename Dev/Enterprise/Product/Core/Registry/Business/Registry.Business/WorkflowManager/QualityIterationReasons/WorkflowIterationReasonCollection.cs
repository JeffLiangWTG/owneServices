using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WorkflowIterationReasonCollection : RegistryBusinessObjectCollection, ICodeDescriptionPairList
	{
		public WorkflowIterationReasonCollection()
		{
		}

		public WorkflowIterationReasonCollection(ReadOnlyCodeDescriptionPairList list)
			: base(list)
		{
		}

		public new WorkflowIterationReason this[int i]
		{
			get { return (WorkflowIterationReason)base[i]; }
		}

		public new WorkflowIterationReason AddNew()
		{
			return (WorkflowIterationReason)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowIterationReasonCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowIterationReason();
		}

		protected override bool IgnoreCaseInCodes => true;

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return ContainsCode(code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			var element = FindByCode(code);
			return (element != null) ? element.Description : ZString.Empty;
		}

		#endregion
	}
}
