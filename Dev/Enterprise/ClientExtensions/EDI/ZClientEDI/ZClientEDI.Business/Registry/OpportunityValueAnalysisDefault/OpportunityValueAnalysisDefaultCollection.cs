using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class OpportunityValueAnalysisDefaultCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new OpportunityValueAnalysisDefault this[int index]
		{
			get { return (OpportunityValueAnalysisDefault)Elements[index]; }
		}

		public new OpportunityValueAnalysisDefault AddNew()
		{
			return (OpportunityValueAnalysisDefault)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OpportunityValueAnalysisDefaultCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OpportunityValueAnalysisDefault();
		}

		protected override bool AllowSort
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}

