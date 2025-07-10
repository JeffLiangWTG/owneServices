using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class OpportunityValueAnalysisDefault : AutoOpportunityValueAnalysisDefault
	{
		public OpportunityValueAnalysisDefault() { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OpportunityValueAnalysisDefault();
		}

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
			}
		}

		public override void ValidateValueInUSD()
		{
			base.ValidateValueInUSD();
			MandatoryValidation.CheckNotNegative(ValueInUSDInfo);
		}

		#endregion
	}
}

